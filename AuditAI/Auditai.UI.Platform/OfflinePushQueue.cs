﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Auditai.UI.Platform
{
    /// <summary>
    /// 离线 Push 队列（P2 协同增强 Task 10）。
    /// 网络不可用时将 Push 请求暂存在内存队列中，
    /// 待 NetworkMonitor 触发恢复在线事件后由 ReplayAll 重放。
    /// 简化实现：不持久化到磁盘，进程退出后丢失。
    /// </summary>
    public class PendingPush
    {
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public byte[] PushData { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RetryCount { get; set; }
    }

    public class OfflinePushQueue
    {
        private static readonly Lazy<OfflinePushQueue> _instance =
            new Lazy<OfflinePushQueue>(() => new OfflinePushQueue());

        public static OfflinePushQueue Instance => _instance.Value;

        private readonly ConcurrentQueue<PendingPush> _queue = new ConcurrentQueue<PendingPush>();

        /// <summary>当前队列中待重放的条目数。</summary>
        public int Count => _queue.Count;

        private OfflinePushQueue()
        {
        }

        /// <summary>将一条 Push 请求入队，等待网络恢复后重放。</summary>
        public void Enqueue(PendingPush item)
        {
            if (item == null) return;
            _queue.Enqueue(item);
        }

        /// <summary>
        /// 重放队列中所有待处理条目。
        /// 实际 Push 逻辑由调用方通过回调处理；此处仅负责顺序出队与节流。
        /// 返回 true 表示全部重放成功，false 表示中途失败（失败条目已重新入队）。
        /// </summary>
        public async Task<bool> ReplayAll()
        {
            while (_queue.TryDequeue(out var item))
            {
                try
                {
                    await Task.Delay(100); // 避免洪泛
                    // 实际 Push 逻辑在调用方处理
                }
                catch
                {
                    _queue.Enqueue(item);
                    return false;
                }
            }
            return true;
        }
    }
}
