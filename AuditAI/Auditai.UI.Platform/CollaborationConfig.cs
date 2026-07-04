using System.Configuration;

namespace Auditai.UI.Platform
{
    /// <summary>
    /// P2 协同增强配置中心。
    /// 统一管理 app.config 中以 Collaboration* / Auto* / OfflineQueue* 等键的读取，
    /// 替换阶段二代码中散落的 ConfigurationManager.AppSettings["..."] != "false" 模式。
    /// 静态属性在首次访问时一次性读取，运行时修改 app.config 不会自动生效（与原行为一致）。
    /// </summary>
    public static class CollaborationConfig
    {
        /// <summary>协同模式：Auto（默认）/ Manual / Off。当前仅作为标记位，由其他模块读取。</summary>
        public static string CollaborationMode { get; } = GetAppSetting("CollaborationMode", "Auto");

        /// <summary>保存时自动 Push 到服务器（默认启用）。</summary>
        public static bool AutoPushOnSave { get; } = GetAppSetting("AutoPushOnSave", "true") != "false";

        /// <summary>对端单元格/段落变更事件触发时自动 Pull（默认启用）。</summary>
        public static bool AutoPullOnPeerEvent { get; } = GetAppSetting("AutoPullOnPeerEvent", "true") != "false";

        /// <summary>ProjectSynced 事件触发时自动 Pull 项目结构（默认启用）。</summary>
        public static bool AutoPullOnProjectSynced { get; } = GetAppSetting("AutoPullOnProjectSynced", "true") != "false";

        /// <summary>Push 冲突（OutOfDate）时自动 Pull 后重试的最大次数。</summary>
        public static int ConflictRetryCount { get; } = int.Parse(GetAppSetting("ConflictRetryCount", "3"));

        /// <summary>是否启用离线 Push 队列：网络不可用时将 Push 请求暂存，恢复后重放。</summary>
        public static bool OfflineQueueEnabled { get; } = GetAppSetting("OfflineQueueEnabled", "true") != "false";

        private static string GetAppSetting(string key, string defaultValue)
        {
            var val = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(val) ? defaultValue : val;
        }
    }
}
