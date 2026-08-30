using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 服务端运维服务（Task 8）
    /// 提供本地构建、发布、SCP 部署、SSH 日志/DB 查询、HTTP 健康检查能力。
    /// 所有方法同步执行并返回 ServerOpsResult，不抛异常。
    /// </summary>
    public static class ServerOpsService
    {
        /// <summary>外部进程默认超时（毫秒）：构建/发布需要较长时间</summary>
        private const int BuildTimeoutMs = 300000;   // 5 分钟
        private const int DeployTimeoutMs = 600000;  // 10 分钟
        private const int SshTimeoutMs = 60000;      // 1 分钟
        private const int HealthTimeoutMs = 10000;   // 10 秒

        /// <summary>默认发布输出目录</summary>
        private static readonly string DefaultPublishPath =
            Path.Combine(Path.GetTempPath(), "auditapi-publish");

        // =====================================================================
        // SubTask 8.2: BuildServer
        // =====================================================================

        /// <summary>
        /// 在本地构建服务端项目（dotnet build）。
        /// 项目路径取自 App.config 的 ServerProjectPath。
        /// </summary>
        /// <param name="configuration">构建配置：Debug / Release（默认 Debug）</param>
        /// <returns>构建结果</returns>
        public static ServerOpsResult BuildServer(string configuration = "Debug")
        {
            Console.Error.WriteLine($"[ServerOps] BuildServer start: configuration={configuration}");
            try
            {
                string projectPath = ConfigurationManager.AppSettings["ServerProjectPath"];
                if (string.IsNullOrWhiteSpace(projectPath))
                    return Fail("ServerProjectPath not configured in App.config");
                if (!Directory.Exists(projectPath))
                    return Fail($"Server project directory not found: {projectPath}");

                if (string.IsNullOrWhiteSpace(configuration))
                    configuration = "Debug";

                string args = $"build \"{projectPath}\" -c {configuration} --nologo";
                var result = RunProcess("dotnet", args, projectPath, BuildTimeoutMs);

                Console.Error.WriteLine($"[ServerOps] BuildServer done: exitCode={result.ExitCode}, durationMs={result.DurationMs}");
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ServerOps] BuildServer exception: {ex.Message}");
                return Fail("BuildServer 异常: " + ex.Message);
            }
        }

        // =====================================================================
        // SubTask 8.3: PublishServer
        // =====================================================================

        /// <summary>
        /// 发布服务端到本地目录（dotnet publish -c Release -r linux-x64 --self-contained false）。
        /// 发布产物默认输出到 %TEMP%/auditapi-publish，供后续 DeployToServer 上传。
        /// </summary>
        /// <param name="configuration">发布配置（默认 Release）</param>
        /// <param name="runtime">目标 RID（默认 linux-x64）</param>
        /// <returns>发布结果</returns>
        public static ServerOpsResult PublishServer(string configuration = "Release", string runtime = "linux-x64")
        {
            Console.Error.WriteLine($"[ServerOps] PublishServer start: configuration={configuration}, runtime={runtime}");
            try
            {
                string projectPath = ConfigurationManager.AppSettings["ServerProjectPath"];
                if (string.IsNullOrWhiteSpace(projectPath))
                    return Fail("ServerProjectPath not configured in App.config");
                if (!Directory.Exists(projectPath))
                    return Fail($"Server project directory not found: {projectPath}");

                if (string.IsNullOrWhiteSpace(configuration))
                    configuration = "Release";
                if (string.IsNullOrWhiteSpace(runtime))
                    runtime = "linux-x64";

                string publishPath = DefaultPublishPath;

                string args =
                    $"publish \"{projectPath}\"" +
                    $" -c {configuration}" +
                    $" -r {runtime}" +
                    " --self-contained false" +
                    $" -o \"{publishPath}\"" +
                    " --nologo";

                var result = RunProcess("dotnet", args, projectPath, BuildTimeoutMs);
                // 附带发布路径，便于后续 DeployToServer 直接使用
                result.Output = (result.Output ?? "") + $"\n[PublishPath] {publishPath}";

                Console.Error.WriteLine($"[ServerOps] PublishServer done: exitCode={result.ExitCode}, durationMs={result.DurationMs}, publishPath={publishPath}");
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ServerOps] PublishServer exception: {ex.Message}");
                return Fail("PublishServer 异常: " + ex.Message);
            }
        }

        // =====================================================================
        // SubTask 8.4: DeployToServer
        // =====================================================================

        /// <summary>
        /// 通过 SCP 上传已发布的服务端到生产服务器，重启 auditapi 服务，并执行健康检查。
        /// SCP/SSH 客户端优先级：scp/ssh（OpenSSH）→ pscp/plink（PuTTY）。
        /// 若 Windows 上两者均不可用则返回失败。
        /// </summary>
        /// <param name="localPublishPath">本地发布产物目录（为 null 时使用 %TEMP%/auditapi-publish）</param>
        /// <returns>部署汇总结果</returns>
        public static ServerOpsResult DeployToServer(string localPublishPath = null)
        {
            Console.Error.WriteLine($"[ServerOps] DeployToServer start");
            try
            {
                string sshHost = ConfigurationManager.AppSettings["SSHHost"];
                string sshUser = ConfigurationManager.AppSettings["SSHUser"];
                string sshKey = ConfigurationManager.AppSettings["SSHKeyPath"];
                string sshPort = ConfigurationManager.AppSettings["SSHPort"];
                string deployPath = ConfigurationManager.AppSettings["ServerDeployPath"];

                if (string.IsNullOrWhiteSpace(sshHost))
                    return Fail("SSHHost not configured in App.config");
                if (string.IsNullOrWhiteSpace(sshUser))
                    return Fail("SSHUser not configured in App.config");
                if (string.IsNullOrWhiteSpace(sshPort))
                    sshPort = "22";
                if (string.IsNullOrWhiteSpace(deployPath))
                    return Fail("ServerDeployPath not configured in App.config");

                if (string.IsNullOrWhiteSpace(localPublishPath))
                    localPublishPath = DefaultPublishPath;
                if (!Directory.Exists(localPublishPath))
                    return Fail($"Local publish path not found: {localPublishPath}. Run publish_server first.");

                // 探测可用的 SCP/SSH 客户端
                string scpBin = FindExecutable(new[] { "scp", "pscp" });
                string sshBin = FindExecutable(new[] { "ssh", "plink" });
                if (scpBin == null)
                    return Fail("scp/pscp not found on Windows. Please install OpenSSH or PuTTY.");
                if (sshBin == null)
                    return Fail("ssh/plink not found on Windows. Please install OpenSSH or PuTTY.");

                bool isPutty = scpBin.Equals("pscp", StringComparison.OrdinalIgnoreCase)
                               || sshBin.Equals("plink", StringComparison.OrdinalIgnoreCase);

                string keyArg = BuildKeyArg(sshKey, isPutty);

                // 1. SCP 上传
                Console.Error.WriteLine($"[ServerOps] SCP upload: {localPublishPath} -> {sshUser}@{sshHost}:{deployPath}");
                string scpArgs;
                if (isPutty)
                {
                    // pscp: -P port -i key -r source user@host:path
                    scpArgs = $"-P {sshPort} {keyArg} -r \"{localPublishPath}\" {sshUser}@{sshHost}:{deployPath}";
                }
                else
                {
                    scpArgs = $"-P {sshPort} {keyArg} -r \"{localPublishPath}\" {sshUser}@{sshHost}:{deployPath}";
                }
                var scpResult = RunProcess(scpBin, scpArgs, null, DeployTimeoutMs);
                if (!scpResult.Success)
                {
                    return new ServerOpsResult
                    {
                        Success = false,
                        Output = scpResult.Output,
                        Error = "SCP upload failed: " + (scpResult.Error ?? ""),
                        ExitCode = scpResult.ExitCode,
                        DurationMs = scpResult.DurationMs
                    };
                }

                // 2. SSH 重启服务
                string restartCmd = "systemctl restart auditapi && sleep 2 && systemctl is-active auditapi";
                string sshArgs = BuildSshArgs(sshBin, sshPort, sshKey, sshUser, sshHost, restartCmd, isPutty);
                Console.Error.WriteLine($"[ServerOps] SSH restart auditapi");
                var sshResult = RunProcess(sshBin, sshArgs, null, SshTimeoutMs);

                // 3. 健康检查
                var healthResult = GetServerHealth();

                bool overallOk = scpResult.Success
                                 && sshResult.Success
                                 && (sshResult.Output ?? "").IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0
                                 && healthResult.Success;

                string summary =
                    $"=== Deploy Summary ===\n" +
                    $"SCP upload: success={scpResult.Success}, durationMs={scpResult.DurationMs}\n" +
                    $"Service restart: success={sshResult.Success}, exitCode={sshResult.ExitCode}, output={sshResult.Output}\n" +
                    $"Health check: success={healthResult.Success}, output={healthResult.Output}\n" +
                    $"Overall: {(overallOk ? "SUCCESS" : "FAILED")}";

                Console.Error.WriteLine($"[ServerOps] DeployToServer done: overall={overallOk}");
                return new ServerOpsResult
                {
                    Success = overallOk,
                    Output = summary,
                    Error = overallOk ? "" : (sshResult.Error ?? healthResult.Error ?? "deploy failed"),
                    ExitCode = overallOk ? 0 : 1,
                    DurationMs = scpResult.DurationMs + sshResult.DurationMs + healthResult.DurationMs
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ServerOps] DeployToServer exception: {ex.Message}");
                return Fail("DeployToServer 异常: " + ex.Message);
            }
        }

        // =====================================================================
        // SubTask 8.5: GetServerLogs
        // =====================================================================

        /// <summary>
        /// 通过 SSH 获取服务端 systemd 日志（journalctl -u auditapi）。
        /// </summary>
        /// <param name="lines">获取日志行数（默认 100）</param>
        /// <param name="filter">过滤关键词（可选，使用 grep -i）</param>
        /// <returns>SSH 命令执行结果</returns>
        public static ServerOpsResult GetServerLogs(int lines = 100, string filter = null)
        {
            Console.Error.WriteLine($"[ServerOps] GetServerLogs start: lines={lines}, filter={filter}");
            try
            {
                var cfg = ReadSshConfig();
                if (cfg.Error != null) return Fail(cfg.Error);

                if (lines <= 0) lines = 100;
                if (lines > 10000) lines = 10000;

                // journalctl 命令，filter 通过 grep -i 过滤
                string cmd = $"journalctl -u auditapi -n {lines} --no-pager";
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    string safeFilter = filter.Replace("\"", "\\\"");
                    cmd += $" | grep -i \"{safeFilter}\"";
                }

                string sshArgs = BuildSshArgs(cfg.SshBin, cfg.Port, cfg.KeyPath, cfg.User, cfg.Host, cmd, cfg.IsPutty);
                Console.Error.WriteLine($"[ServerOps] SSH get logs");
                var result = RunProcess(cfg.SshBin, sshArgs, null, SshTimeoutMs);
                Console.Error.WriteLine($"[ServerOps] GetServerLogs done: exitCode={result.ExitCode}, durationMs={result.DurationMs}");
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ServerOps] GetServerLogs exception: {ex.Message}");
                return Fail("GetServerLogs 异常: " + ex.Message);
            }
        }

        // =====================================================================
        // SubTask 8.6: QueryServerDb
        // =====================================================================

        /// <summary>
        /// 通过 SSH 在服务端执行 SQLite 查询（sqlite3）。
        /// 仅建议用于只读 SELECT。
        /// </summary>
        /// <param name="sql">SQL 语句</param>
        /// <returns>SSH 命令执行结果</returns>
        public static ServerOpsResult QueryServerDb(string sql)
        {
            Console.Error.WriteLine($"[ServerOps] QueryServerDb start");
            try
            {
                if (string.IsNullOrWhiteSpace(sql))
                    return Fail("sql is required");

                var cfg = ReadSshConfig();
                if (cfg.Error != null) return Fail(cfg.Error);

                string dbPath = ConfigurationManager.AppSettings["ServerDbPath"];
                if (string.IsNullOrWhiteSpace(dbPath))
                    return Fail("ServerDbPath not configured in App.config");

                // 转义 SQL 中的双引号
                string safeSql = sql.Replace("\"", "\\\"");
                string cmd = $"sqlite3 \"{dbPath}\" \"{safeSql}\"";

                string sshArgs = BuildSshArgs(cfg.SshBin, cfg.Port, cfg.KeyPath, cfg.User, cfg.Host, cmd, cfg.IsPutty);
                Console.Error.WriteLine($"[ServerOps] SSH query db");
                var result = RunProcess(cfg.SshBin, sshArgs, null, SshTimeoutMs);
                Console.Error.WriteLine($"[ServerOps] QueryServerDb done: exitCode={result.ExitCode}, durationMs={result.DurationMs}");
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ServerOps] QueryServerDb exception: {ex.Message}");
                return Fail("QueryServerDb 异常: " + ex.Message);
            }
        }

        // =====================================================================
        // SubTask 8.7: GetServerHealth
        // =====================================================================

        /// <summary>
        /// 执行服务端健康检查。
        /// 通过 HTTP 调用 /api/User/UserNameExists?userName=healthcheck（白名单端点，不需要鉴权），
        /// 只要返回任意 HTTP 状态码（包括 4xx）即认为服务在线；网络异常则视为离线。
        /// </summary>
        /// <returns>健康检查结果</returns>
        public static ServerOpsResult GetServerHealth()
        {
            Console.Error.WriteLine($"[ServerOps] GetServerHealth start");
            try
            {
                string baseUrl = CloudApiClient.ServerBaseUrl;
                if (string.IsNullOrWhiteSpace(baseUrl))
                    return Fail("ServerBaseUrl not configured in App.config");

                // UserNameExists 在 TokenAuthMiddleware 白名单内，无需 Token。
                // 任意 HTTP 响应（包括 404/400/500）都视为服务在线。
                string url = baseUrl + "/api/User/UserNameExists?userName=healthcheck";
                var sw = Stopwatch.StartNew();
                try
                {
                    using (var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(HealthTimeoutMs) })
                    using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                    {
                        var resp = client.SendAsync(req).Result;
                        sw.Stop();
                        bool online = (int)resp.StatusCode > 0;
                        string body = "";
                        try { body = resp.Content.ReadAsStringAsync().Result; } catch { /* 忽略读取异常 */ }

                        Console.Error.WriteLine($"[ServerOps] GetServerHealth done: status={(int)resp.StatusCode}, online={online}");
                        return new ServerOpsResult
                        {
                            Success = online,
                            Output = $"{{\"online\":{online.ToString().ToLowerInvariant()},\"statusCode\":{(int)resp.StatusCode},\"url\":\"{EscapeJson(url)}\",\"body\":\"{EscapeJson(body)}\"}}",
                            Error = "",
                            ExitCode = 0,
                            DurationMs = sw.ElapsedMilliseconds
                        };
                    }
                }
                catch (AggregateException agEx)
                {
                    sw.Stop();
                    string msg = agEx.InnerException != null ? agEx.InnerException.Message : agEx.Message;
                    Console.Error.WriteLine($"[ServerOps] GetServerHealth offline: {msg}");
                    return new ServerOpsResult
                    {
                        Success = false,
                        Output = "{\"online\":false}",
                        Error = "Service offline: " + msg,
                        ExitCode = -1,
                        DurationMs = sw.ElapsedMilliseconds
                    };
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    Console.Error.WriteLine($"[ServerOps] GetServerHealth offline: {ex.Message}");
                    return new ServerOpsResult
                    {
                        Success = false,
                        Output = "{\"online\":false}",
                        Error = "Service offline: " + ex.Message,
                        ExitCode = -1,
                        DurationMs = sw.ElapsedMilliseconds
                    };
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ServerOps] GetServerHealth exception: {ex.Message}");
                return Fail("GetServerHealth 异常: " + ex.Message);
            }
        }

        // =====================================================================
        // 辅助方法
        // =====================================================================

        /// <summary>
        /// 同步执行外部进程，捕获 stdout/stderr，支持超时。
        /// </summary>
        private static ServerOpsResult RunProcess(string fileName, string arguments, string workingDir, int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            if (!string.IsNullOrEmpty(workingDir))
                psi.WorkingDirectory = workingDir;

            Process proc = null;
            try
            {
                proc = Process.Start(psi);
                if (proc == null)
                    return new ServerOpsResult
                    {
                        Success = false,
                        Error = $"Failed to start process: {fileName}",
                        ExitCode = -1,
                        DurationMs = sw.ElapsedMilliseconds
                    };

                // 修复：原实现先同步 proc.StandardOutput.ReadToEnd() 再读 stderr。
                // 若子进程向 stderr 大量输出（如 dotnet build 大量警告）且 stdout 未关闭，
                // stderr 管道缓冲填满后子进程阻塞写，父进程阻塞读 stdout → 双方死锁。
                // 改用异步事件读取，避免管道缓冲耗尽。
                var outputSb = new System.Text.StringBuilder();
                var errorSb = new System.Text.StringBuilder();
                using (var outputWait = new System.Threading.ManualResetEvent(false))
                using (var errorWait = new System.Threading.ManualResetEvent(false))
                {
                    proc.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data == null) outputWait.Set();
                        else outputSb.AppendLine(e.Data);
                    };
                    proc.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data == null) errorWait.Set();
                        else errorSb.AppendLine(e.Data);
                    };
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();

                    bool exited = proc.WaitForExit(timeoutMs);
                    if (!exited)
                    {
                        try { proc.Kill(); } catch { /* 忽略 */ }
                        sw.Stop();
                        return new ServerOpsResult
                        {
                            Success = false,
                            Output = outputSb.ToString(),
                            Error = "Process timed out after " + timeoutMs + "ms. " + errorSb.ToString(),
                            ExitCode = -1,
                            DurationMs = sw.ElapsedMilliseconds
                        };
                    }

                    // 等待异步读取完成（有内部超时保护，避免极端情况下卡死）
                    outputWait.WaitOne(2000);
                    errorWait.WaitOne(2000);
                    string output = outputSb.ToString();
                    string error = errorSb.ToString();

                    sw.Stop();
                    return new ServerOpsResult
                    {
                        Success = proc.ExitCode == 0,
                        Output = output,
                        Error = error,
                        ExitCode = proc.ExitCode,
                        DurationMs = sw.ElapsedMilliseconds
                    };
                }
            }
            finally
            {
                if (proc != null)
                {
                    try { proc.Dispose(); } catch { /* 忽略 */ }
                }
            }
        }

        /// <summary>
        /// 在 PATH 中查找第一个可用的可执行文件。
        /// </summary>
        private static string FindExecutable(string[] names)
        {
            foreach (var name in names)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "where",
                        Arguments = name,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using (var p = Process.Start(psi))
                    {
                        if (p == null) continue;
                        string output = p.StandardOutput.ReadToEnd();
                        p.WaitForExit(3000);
                        if (p.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            return name; // 返回名称，由 Process.Start 解析 PATH
                        }
                    }
                }
                catch
                {
                    // 忽略，尝试下一个
                }
            }
            return null;
        }

        /// <summary>
        /// 构建 SSH 密钥参数（OpenSSH 用 -i key，PuTTY 用 -i key）。
        /// 若 key 为空则返回空串（依赖系统默认密钥）。
        /// </summary>
        private static string BuildKeyArg(string sshKey, bool isPutty)
        {
            if (string.IsNullOrWhiteSpace(sshKey)) return "";
            // 两种工具都用 -i 参数
            return $"-i \"{sshKey}\"";
        }

        /// <summary>
        /// 构建 SSH 远程命令的完整参数。
        /// </summary>
        private static string BuildSshArgs(string sshBin, string port, string key, string user, string host, string remoteCmd, bool isPutty)
        {
            // 转义远程命令中的双引号（Windows 侧）
            string escapedCmd = remoteCmd.Replace("\"", "\\\"");

            if (isPutty)
            {
                // plink: -P port -i key user@host "command"
                string keyArg = string.IsNullOrWhiteSpace(key) ? "" : $"-i \"{key}\"";
                return $"-P {port} {keyArg} {user}@{host} \"{escapedCmd}\"";
            }
            else
            {
                // ssh: -p port -i key user@host "command"
                string keyArg = string.IsNullOrWhiteSpace(key) ? "" : $"-i \"{key}\"";
                return $"-p {port} {keyArg} {user}@{host} \"{escapedCmd}\"";
            }
        }

        /// <summary>
        /// 读取 SSH 配置并探测可用的 SSH 客户端。
        /// </summary>
        private static SshConfig ReadSshConfig()
        {
            var cfg = new SshConfig
            {
                Host = ConfigurationManager.AppSettings["SSHHost"],
                User = ConfigurationManager.AppSettings["SSHUser"],
                KeyPath = ConfigurationManager.AppSettings["SSHKeyPath"],
                Port = ConfigurationManager.AppSettings["SSHPort"]
            };

            if (string.IsNullOrWhiteSpace(cfg.Host))
            {
                cfg.Error = "SSHHost not configured in App.config";
                return cfg;
            }
            if (string.IsNullOrWhiteSpace(cfg.User))
            {
                cfg.Error = "SSHUser not configured in App.config";
                return cfg;
            }
            if (string.IsNullOrWhiteSpace(cfg.Port))
                cfg.Port = "22";

            cfg.SshBin = FindExecutable(new[] { "ssh", "plink" });
            if (cfg.SshBin == null)
            {
                cfg.Error = "ssh/plink not found on Windows. Please install OpenSSH or PuTTY.";
                return cfg;
            }
            cfg.IsPutty = cfg.SshBin.Equals("plink", StringComparison.OrdinalIgnoreCase);
            return cfg;
        }

        /// <summary>
        /// 简单 JSON 字符串转义（避免引入 Newtonsoft 依赖到内联 JSON）。
        /// </summary>
        private static string EscapeJson(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        }

        /// <summary>
        /// 构造失败结果。
        /// </summary>
        private static ServerOpsResult Fail(string message)
        {
            return new ServerOpsResult
            {
                Success = false,
                Output = "",
                Error = message,
                ExitCode = -1,
                DurationMs = 0
            };
        }

        /// <summary>SSH 配置内部载体</summary>
        private class SshConfig
        {
            public string Host;
            public string User;
            public string KeyPath;
            public string Port;
            public string SshBin;
            public bool IsPutty;
            public string Error;
        }
    }

    /// <summary>
    /// 服务端运维操作结果。
    /// </summary>
    public class ServerOpsResult
    {
        /// <summary>操作是否成功</summary>
        public bool Success { get; set; }

        /// <summary>标准输出</summary>
        public string Output { get; set; }

        /// <summary>标准错误</summary>
        public string Error { get; set; }

        /// <summary>进程退出码</summary>
        public int ExitCode { get; set; }

        /// <summary>耗时（毫秒）</summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// 序列化为 JSON 字符串（snake_case 字段名，便于 MCP 客户端解析）。
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(new
            {
                success = Success,
                output = Output,
                error = Error,
                exitCode = ExitCode,
                durationMs = DurationMs
            }, Formatting.Indented);
        }
    }
}
