# 将 Protobuf 二次解析修复（与 .tmp-server 已修版本一致）应用到规范服务器源码
$ErrorActionPreference = 'Stop'

$tsPath = 'E:\lq\Server\AuditApiServer\Services\TableSyncService.cs'
$prgPath = 'E:\lq\Server\AuditApiServer\Program.cs'

# ---------- 1) TableSyncService.PushTableQuickAsync: byte[] -> PushTable ----------
$ts = [IO.File]::ReadAllText($tsPath)

$oldQuick = @'
    public async Task<object> PushTableQuickAsync(byte[] protobufBytes, long userId)
    {
        PushTable pushTable;
        try
        {
            pushTable = PushTable.Parser.ParseFrom(protobufBytes);
        }
        catch (InvalidProtocolBufferException ex)
        {
            _logger.LogWarning(ex, "PushTable Protobuf 解析失败: UserId={UserId}", userId);
            throw new ArgumentException("无效的 Protobuf 数据: " + ex.Message, ex);
        }

        ValidatePushTableActions(pushTable);

        return await SavePushTableAsync(pushTable, userId);
    }
'@
$newQuick = @'
    public async Task<object> PushTableQuickAsync(PushTable pushTable, long userId)
    {
        ValidatePushTableActions(pushTable);
        return await SavePushTableAsync(pushTable, userId);
    }
'@
if (-not $ts.Contains($oldQuick)) { throw 'TableSyncService: PushTableQuickAsync 原文未匹配' }
$ts = $ts.Replace($oldQuick, $newQuick)

# ---------- 2) PushTableAsync 解析失败补 HeadHex 诊断日志 ----------
$oldCatch = @'
        catch (InvalidProtocolBufferException ex)
        {
            _logger.LogWarning(ex, "PushTableAsync Protobuf 解析失败: TaskId={TaskId}", taskId);
            throw new ArgumentException("无效的 Protobuf 数据: " + ex.Message, ex);
        }
'@
$newCatch = @'
        catch (InvalidProtocolBufferException ex)
        {
            // 诊断日志：记录前 32 字节 hex 便于排查上传文件是否损坏
            byte[] head = bytes.Length >= 32 ? bytes.AsSpan(0, 32).ToArray() : bytes;
            _logger.LogWarning(ex,
                "PushTableAsync Protobuf 解析失败: TaskId={TaskId}, BytesLength={Len}, HeadHex={Head}",
                taskId, bytes.Length, Convert.ToHexString(head));
            throw new ArgumentException("无效的 Protobuf 数据: " + ex.Message, ex);
        }
'@
if (-not $ts.Contains($oldCatch)) { throw 'TableSyncService: PushTableAsync catch 原文未匹配' }
$ts = $ts.Replace($oldCatch, $newCatch)

[IO.File]::WriteAllText($tsPath, $ts, (New-Object System.Text.UTF8Encoding($false)))
Write-Host 'TableSyncService.cs 已更新'

# ---------- 3) Program.cs PushTableQuick 端点: 解析一次 + HeadHex + 传已解析对象 ----------
$prg = [IO.File]::ReadAllText($prgPath)

$oldParse = @'
    // 解析 Protobuf 提取 projectId 进行跨团队访问校验
    PushTable pushTable;
    try { pushTable = PushTable.Parser.ParseFrom(bytes); }
    catch { return ApiResponseHelper.Error("无效的 Protobuf 数据"); }
'@
$newParse = @'
    // 解析 Protobuf（仅一次）：同时用于提取 projectId 访问校验 + 业务处理
    PushTable pushTable;
    try { pushTable = PushTable.Parser.ParseFrom(bytes); }
    catch (InvalidProtocolBufferException ex)
    {
        // 诊断日志：记录前 32 字节 hex 便于排查客户端是否发错了格式
        byte[] head = bytes.Length >= 32 ? bytes.AsSpan(0, 32).ToArray() : bytes;
        app.Logger.LogWarning(ex,
            "PushTableQuick Protobuf 解析失败: BytesLength={Len}, HeadHex={Head}, UserId={UserId}",
            bytes.Length, Convert.ToHexString(head), userId);
        return ApiResponseHelper.Error("无效的 Protobuf 数据: " + ex.Message);
    }
'@
if (-not $prg.Contains($oldParse)) { throw 'Program.cs: PushTableQuick 解析块原文未匹配' }
$prg = $prg.Replace($oldParse, $newParse)

$oldCall = '    var result = await svc.PushTableQuickAsync(bytes, userId);'
$newCall = '    // 直接传入已解析的 PushTable 对象，避免 TableSyncService 里重复 ParseFrom 同一份 bytes
    var result = await svc.PushTableQuickAsync(pushTable, userId);'
if (-not $prg.Contains($oldCall)) { throw 'Program.cs: PushTableQuickAsync 调用原文未匹配' }
$prg = $prg.Replace($oldCall, $newCall)

[IO.File]::WriteAllText($prgPath, $prg, (New-Object System.Text.UTF8Encoding($false)))
Write-Host 'Program.cs 已更新'
Write-Host 'ALL DONE'
