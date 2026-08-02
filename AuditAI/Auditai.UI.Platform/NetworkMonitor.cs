using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Auditai.UI.Platform
{
    /// <summary>
    /// 网络状态监视器（P2 协同增强 Task 11）。
    /// 通过 NetworkChange.NetworkAddressChanged + 定时 Ping 服务器健康端点
    /// 判定当前是否在线，并在状态切换时触发 NetworkStatusChanged 事件。
    /// 供 OfflinePushQueue、MainForm 标题栏等模块订阅。
    /// </summary>
    public class NetworkMonitor
    {
        private static readonly Lazy<NetworkMonitor> _instance =
            new Lazy<NetworkMonitor>(() => new NetworkMonitor());

        public static NetworkMonitor Instance => _instance.Value;

        private volatile bool _isOnline = true;
        private Timer _pingTimer;
        private readonly object _statusLock = new object();

        /// <summary>当前是否在线（最近一次健康检查成功）。</summary>
        public bool IsOnline => _isOnline;

        /// <summary>网络状态发生变化时触发（true=恢复在线，false=掉线）。</summary>
        public event Action<bool> NetworkStatusChanged;

        private NetworkMonitor()
        {
            try
            {
                NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
            }
            catch
            {
                // 某些环境（如沙箱）可能不支持 NetworkChange，忽略
            }
            // 启动后 5 秒首次检查，之后每 15 秒 ping 一次服务器（原 30 秒，缩短以更快感知服务器状态变化）
            _pingTimer = new Timer(PingServer, null,
                TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15));
        }

        private void OnNetworkAddressChanged(object sender, EventArgs e)
        {
            // 网络地址变化后立即检查服务器可达性
            CheckServerReachability();
        }

        private void PingServer(object state)
        {
            CheckServerReachability();
        }

        /// <summary>
        /// 立即触发一次服务器可达性检查，不等定时器周期。
        /// 供 MainForm 启动时调用，尽快检测服务器状态。
        /// </summary>
        public void CheckNow() => CheckServerReachability();

        /// <summary>
        /// 通过 HTTP GET 健康检查端点判定服务器是否可达。
        /// 200/400/401 均视为"服务器可达"（返回 4xx 说明 HTTP 链路正常，只是参数问题）。
        /// </summary>
        private void CheckServerReachability()
        {
            Task.Run(async () =>
            {
                bool reachable = false;
                try
                {
                    string baseUrl = ConfigurationManager.AppSettings["AppServer"];
                    if (string.IsNullOrWhiteSpace(baseUrl))
                    {
                        baseUrl = "http://82.156.108.218:8957/api/";
                    }
                    // 健康检查端点：复用现有的 UserNameExists 接口（不依赖鉴权）
                    string healthUrl = baseUrl + "User/UserNameExists?userName=healthcheck";
                    using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) })
                    {
                        var resp = await client.GetAsync(healthUrl).ConfigureAwait(false);
                        // 2xx / 4xx 都说明服务器可达；仅 5xx 或异常视为不可达
                        reachable = (int)resp.StatusCode < 500;
                    }
                }
                catch
                {
                    reachable = false;
                }
                UpdateStatus(reachable);
            });
        }

        private void UpdateStatus(bool online)
        {
            bool changed = false;
            lock (_statusLock)
            {
                if (_isOnline != online)
                {
                    _isOnline = online;
                    changed = true;
                }
            }
            if (changed)
            {
                try
                {
                    NetworkStatusChanged?.Invoke(online);
                }
                catch
                {
                    // 订阅方异常不应影响监视器本身
                }
            }
        }
    }
}
