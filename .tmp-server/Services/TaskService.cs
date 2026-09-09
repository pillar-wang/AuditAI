﻿using System.Collections.Concurrent;
using System.IO.Compression;
using AuditApiServer.Data;
using AuditApiServer.Models;

namespace AuditApiServer.Services;

/// <summary>
/// 异步任务引擎服务。阶段 5 Task 5.2 + 阶段 6 修正：
/// ConcurrentDictionary 内存任务状态管理（key 为 long TaskId，对应 SQLite rowid）、
/// Task.Run 后台执行、分片上传文件合并（按 offset 随机写入 {ContentRoot}/TaskCache/{taskId}.tmp）、
/// 30 秒超时检测、任务状态响应格式严格匹配文档：
/// {progressValue, isTaskEnd, isTaskSuccess, isTimeOut, taskDesc, taskResult}
/// </summary>
public class TaskService
{
    private readonly TaskRepository _taskRepo;
    private readonly ILogger<TaskService> _logger;
    private readonly IWebHostEnvironment _env;

    // 内存任务状态：服务重启后丢失，回退到数据库查询
    private static readonly ConcurrentDictionary<long, TaskRunState> _runningTasks = new();

    public TaskService(TaskRepository taskRepo, ILogger<TaskService> logger, IWebHostEnvironment env)
    {
        _taskRepo = taskRepo;
        _logger = logger;
        _env = env;
    }

    /// <summary>
    /// 生成任务 ID 并记录到数据库 + 内存状态。
    /// 返回 long 类型 TaskId（客户端通过 (long)jObject["taskId"] 读取）。
    /// </summary>
    public async Task<long> GenerateTaskIdAsync(long userId, string taskType)
    {
        var task = await _taskRepo.CreateAsync(userId, taskType);
        _runningTasks[task.TaskId] = new TaskRunState
        {
            TaskId = task.TaskId,
            Status = 0,
            Progress = 0,
            StartTime = DateTime.UtcNow,
            IsTaskEnd = false,
            IsTaskSuccess = false,
            IsTimeOut = false,
            TaskDesc = "",
            TaskResult = ""
        };
        return task.TaskId;
    }

    /// <summary>
    /// 启动后台任务。30 秒超时（与客户端轮询策略一致：30 秒超时，最多 6 次重试）。
    /// </summary>
    public void StartBackgroundTask(long taskId, Func<IProgressReporter, CancellationToken, Task> work)
    {
        StartBackgroundTaskCore(taskId, async (progress, ct) =>
        {
            await work(progress, ct);
            return null;
        });
    }

    /// <summary>
    /// 启动后台任务（带返回值重载）。30 秒超时。
    /// work 委托返回的字符串会写入 TaskRunState.TaskResult + DB Result 列，
    /// 客户端轮询 GetTaskRunningStatus 时通过 taskResult 字段返回。
    /// 用于 PullProjectDirect 等需要返回 JSON 结果（如 { Url, Length }）的场景。
    /// </summary>
    public void StartBackgroundTask(long taskId, Func<IProgressReporter, CancellationToken, Task<string?>> work)
    {
        StartBackgroundTaskCore(taskId, work);
    }

    private void StartBackgroundTaskCore(long taskId, Func<IProgressReporter, CancellationToken, Task<string?>> work)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var progress = new ProgressReporter(taskId);

        _ = Task.Run(async () =>
        {
            try
            {
                await _taskRepo.UpdateStatusAsync(taskId, 1, 0); // Running
                if (_runningTasks.TryGetValue(taskId, out var st))
                {
                    st.Status = 1;
                }

                var resultStr = await work(progress, cts.Token);

                if (_runningTasks.TryGetValue(taskId, out var state))
                {
                    state.IsTaskEnd = true;
                    state.IsTaskSuccess = true;
                    state.Progress = 1.0;
                    if (resultStr != null) state.TaskResult = resultStr;
                }
                await _taskRepo.UpdateStatusAsync(taskId, 2, 1.0, resultStr); // Completed
            }
            catch (OperationCanceledException)
            {
                if (_runningTasks.TryGetValue(taskId, out var state))
                {
                    state.IsTaskEnd = true;
                    state.IsTimeOut = true;
                    state.TaskDesc = "BadRequestMessage:任务超时";
                }
                await _taskRepo.UpdateStatusAsync(taskId, 3, 0, "Timeout");
                _logger.LogWarning("任务 {TaskId} 执行超时", taskId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "任务 {TaskId} 执行失败", taskId);
                if (_runningTasks.TryGetValue(taskId, out var state))
                {
                    state.IsTaskEnd = true;
                    state.IsTaskSuccess = false;
                    state.TaskDesc = $"BadRequestMessage:{ex.Message}";
                }
                await _taskRepo.UpdateStatusAsync(taskId, 3, 0, ex.Message);
            }
            finally
            {
                // 5 分钟后从内存清理（保留一段时间供客户端轮询查询最终状态）
                _ = Task.Delay(TimeSpan.FromMinutes(5)).ContinueWith(t =>
                {
                    _runningTasks.TryRemove(taskId, out _);
                });
            }
        }, cts.Token);
    }

    /// <summary>
    /// 获取任务状态（客户端轮询用）。优先从内存读取，内存中无（服务重启后）从数据库读取。
    /// 安全审计修复（High）：校验 userId 是任务创建者，否则返回"任务不存在"（不泄露任务存在性）。
    /// </summary>
    public async Task<TaskStatusDto> GetTaskStatusAsync(long taskId, long userId)
    {
        if (userId == 0) throw new UnauthorizedAccessException("未登录");

        // 校验任务归属（同时获取任务数据，避免重复查询）
        var task = await _taskRepo.GetByIdAsync(taskId);
        if (task == null || task.UserId != userId)
        {
            return new TaskStatusDto
            {
                IsTaskEnd = true,
                IsTaskSuccess = false,
                TaskDesc = "BadRequestMessage:任务不存在"
            };
        }

        // 优先从内存读取
        if (_runningTasks.TryGetValue(taskId, out var state))
        {
            return new TaskStatusDto
            {
                ProgressValue = state.Progress,
                IsTaskEnd = state.IsTaskEnd,
                IsTaskSuccess = state.IsTaskSuccess,
                IsTimeOut = state.IsTimeOut,
                TaskDesc = state.TaskDesc,
                TaskResult = state.TaskResult
            };
        }

        // 内存中无（服务重启后），从已查询的 task 构造
        return new TaskStatusDto
        {
            ProgressValue = task.Progress,
            IsTaskEnd = task.Status == 2 || task.Status == 3,
            IsTaskSuccess = task.Status == 2,
            IsTimeOut = task.Result == "Timeout",
            TaskDesc = task.Status == 3 ? $"BadRequestMessage:{task.Result}" : "",
            TaskResult = task.Result ?? ""
        };
    }

    /// <summary>
    /// 分片上传到临时文件。按 offset 随机写入，支持并行分片。
    /// 安全审计修复（High）：校验 userId 是任务创建者，否则抛 UnauthorizedAccessException。
    /// </summary>
    public async Task UploadChunkAsync(long taskId, long offset, Stream chunkStream, long userId)
    {
        if (userId == 0) throw new UnauthorizedAccessException("未登录");

        var task = await _taskRepo.GetByIdAsync(taskId);
        if (task == null || task.UserId != userId)
            throw new UnauthorizedAccessException("无权访问该任务");

        var cachePath = GetTaskCachePath(taskId);
        Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);

        using var fs = new FileStream(cachePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        fs.Seek(offset, SeekOrigin.Begin);
        await chunkStream.CopyToAsync(fs);
    }

    /// <summary>
    /// 读取任务输入文件（业务接口处理时调用）。
    /// 客户端 UploadTaskInputFile 上传前用 CompressToGZipStream 对文件内容做了 GZip 压缩，
    /// 因此磁盘上的 {taskId}.tmp 是 GZip 流。此处返回 GZip 解压流，调用方直接读即可拿到
    /// 原始 Protobuf / JSON 字节，无需在 PushProject/PushTable/PushDocument 三处分别加解压层。
    /// </summary>
    public Stream OpenTaskInput(long taskId)
    {
        var cachePath = GetTaskCachePath(taskId);
        if (!File.Exists(cachePath)) throw new FileNotFoundException($"任务输入文件不存在: {taskId}");
        var fs = File.OpenRead(cachePath);
        return new GZipStream(fs, CompressionMode.Decompress, leaveOpen: false);
    }

    /// <summary>
    /// 清理任务缓存（删除临时文件 + DB 记录 + 内存状态）。
    /// 安全审计修复（High）：校验 userId 是任务创建者，否则抛 UnauthorizedAccessException。
    /// </summary>
    public async Task ClearTaskCacheAsync(long taskId, long userId)
    {
        if (userId == 0) throw new UnauthorizedAccessException("未登录");

        var task = await _taskRepo.GetByIdAsync(taskId);
        if (task == null || task.UserId != userId)
            throw new UnauthorizedAccessException("无权清理该任务缓存");

        var cachePath = GetTaskCachePath(taskId);
        try
        {
            if (File.Exists(cachePath)) File.Delete(cachePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "清理任务缓存文件失败: {TaskId}", taskId);
        }
        await _taskRepo.DeleteAsync(taskId);
        _runningTasks.TryRemove(taskId, out _);
    }

    /// <summary>
    /// 获取任务缓存文件路径：{ContentRoot}/TaskCache/{taskId}.tmp
    /// 服务器部署后会是 /opt/auditapi/TaskCache/{taskId}.tmp（由 systemd WorkingDirectory 决定绝对路径）。
    /// </summary>
    public string GetTaskCachePath(long taskId)
    {
        var cacheDir = Path.Combine(_env.ContentRootPath, "TaskCache");
        return Path.Combine(cacheDir, $"{taskId}.tmp");
    }

    /// <summary>
    /// 内存任务运行状态。
    /// </summary>
    private class TaskRunState
    {
        public long TaskId { get; set; }
        public int Status { get; set; }
        public double Progress { get; set; }
        public DateTime StartTime { get; set; }
        public bool IsTaskEnd { get; set; }
        public bool IsTaskSuccess { get; set; }
        public bool IsTimeOut { get; set; }
        public string TaskDesc { get; set; } = "";
        public string TaskResult { get; set; } = "";
    }

    /// <summary>
    /// 进度报告器：后台任务通过此接口更新内存中的进度。
    /// </summary>
    private class ProgressReporter : IProgressReporter
    {
        private readonly long _taskId;
        public ProgressReporter(long taskId) { _taskId = taskId; }
        public void Report(double progress, string? desc = null)
        {
            if (_runningTasks.TryGetValue(_taskId, out var state))
            {
                state.Progress = progress;
                if (desc != null) state.TaskDesc = desc;
            }
        }
    }
}

/// <summary>
/// 进度报告接口：后台任务通过此接口向 TaskService 报告执行进度。
/// </summary>
public interface IProgressReporter
{
    void Report(double progress, string? desc = null);
}
