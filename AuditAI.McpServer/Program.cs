﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Configuration;

namespace AuditAI.McpServer
{
    /// <summary>
    /// AuditAI MCP Server 入口
    /// 通过 MCP 协议（JSON-RPC 2.0 over stdio）向 AI 客户端暴露审计功能
    /// </summary>
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // 设置 UTF-8 编码（MCP 协议要求）。
            // 修复：Console.InputEncoding 的 setter 在 .NET Framework 下走 SetConsoleCP，
            // 当客户端以"无控制台 + 重定向管道"方式启动时可能抛 IOException，
            // 原实现会让服务在注册任何工具之前直接退出。改为尽力设置、失败不致命。
            try
            {
                Console.InputEncoding = System.Text.Encoding.UTF8;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[Program] Console.InputEncoding 设置失败（无控制台句柄？已忽略）: " + ex.Message);
            }
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[Program] Console.OutputEncoding 设置失败（已忽略）: " + ex.Message);
            }

            // 退出兜底：客户端直接断开连接时，打开中的项目库可能残留 -wal。
            // 与 Auditai.UI.Platform/Program.cs 的兜底一致，在此注册进程退出/未处理异常钩子，
            // 确保 Dal.Dispose（内部 wal_checkpoint(TRUNCATE)）至少被执行一次。
            AppDomain.CurrentDomain.ProcessExit += (s, e) => CloseCurrentProjectSafely("ProcessExit");
            AppDomain.CurrentDomain.UnhandledException += (s, e) => CloseCurrentProjectSafely("UnhandledException");

            // 初始化字符串常量（审计版本）
            // 必须在创建 Project 实例之前设置，因为 DataReferenceManager.InitDic() 会访问 StringConstBase.Current
            Auditai.Model.StringConstBase.Current = Auditai.Model.StringConstEditions.Audit;

            // 初始化本地存储模式
            // Server 模式下启用云端同步（Syncer 默认 Disabled=true，解耦自 StorageRouter）
            Auditai.LocalDataStore.StorageRouter.OnServerModeActivated = () => Auditai.Model.Syncer.Disabled = false;
            Auditai.LocalDataStore.StorageRouter.Initialize();

            // 加载管理后台基地址（Task 1：管理后台上下文，端口 8958）
            // 修复：原默认值是生产机 IP 的明文 HTTP；改为本机回环默认值，
            // 连生产请在 App.config 显式配置（建议 HTTPS）。
            AuditAI.McpServer.State.SessionState.AdminBaseUrl = ConfigurationManager.AppSettings["AdminBaseUrl"] ?? "http://127.0.0.1:8958";

            // 初始化本地用户和团队（复用 Program.cs 中的逻辑）
            InitializeLocalUser();

            // 初始化 C1/TX TextControl 许可（无头模式）
            AuditAI.McpServer.Licensing.LicenseInitializer.Initialize();

            // 注册 MCP 工具
            AuditAI.McpServer.Tools.ProjectTools.Register();
            AuditAI.McpServer.Tools.TreeTools.Register();
            AuditAI.McpServer.Tools.TableTools.Register();
            AuditAI.McpServer.Tools.LedgerTools.Register();
            AuditAI.McpServer.Tools.DocumentTools.Register();
            AuditAI.McpServer.Tools.CollectionTools.Register();
            AuditAI.McpServer.Tools.FormulaTools.Register();
            AuditAI.McpServer.Tools.ExportTools.Register();
            AuditAI.McpServer.Tools.CrossProjectTools.Register();
            AuditAI.McpServer.Tools.ValidationTools.Register();
            AuditAI.McpServer.Tools.WorkflowTools.Register();
            AuditAI.McpServer.Tools.DocumentBookmarkTools.Register();
            AuditAI.McpServer.Tools.ProjectVariableTools.Register();
            AuditAI.McpServer.Tools.TableStructureTools.Register();
            AuditAI.McpServer.Tools.VoucherDetailTools.Register();
            AuditAI.McpServer.Tools.FormulaInspectionTools.Register();
            AuditAI.McpServer.Tools.NodeSearchTools.Register();
            AuditAI.McpServer.Tools.ImportTools.Register();

            // 细粒度扩展工具集（覆盖 AppCommands 中原本缺失的 UI 操作）
            AuditAI.McpServer.Tools.AdvancedTableTools.Register();
            AuditAI.McpServer.Tools.NodeManagementTools.Register();
            AuditAI.McpServer.Tools.CellFormatTools.Register();
            AuditAI.McpServer.Tools.DocumentExtensionTools.Register();
            AuditAI.McpServer.Tools.ProjectExtensionTools.Register();
            Console.Error.WriteLine("[Program] Fine-grained extension toolsets registered (AdvancedTable/NodeManagement/CellFormat/DocumentExtension/ProjectExtension).");

            // 第三批扩展工具集（覆盖快照/验证点/页面设置/文档格式/批量操作/票据/采集汇总等 UI 操作）
            AuditAI.McpServer.Tools.SnapshotTools.Register();
            AuditAI.McpServer.Tools.ValidationPointTools.Register();
            AuditAI.McpServer.Tools.PageSetupTools.Register();
            AuditAI.McpServer.Tools.DocumentFormatTools.Register();
            AuditAI.McpServer.Tools.BatchOperationTools.Register();
            AuditAI.McpServer.Tools.TicketTools.Register();
            AuditAI.McpServer.Tools.CollectConsolidateTools.Register();
            AuditAI.McpServer.Tools.CellBorderTools.Register();
            AuditAI.McpServer.Tools.DocumentCharFormatTools.Register();
            AuditAI.McpServer.Tools.TableStylePresetTools.Register();
            AuditAI.McpServer.Tools.DocumentInsertTools.Register();
            Console.Error.WriteLine("[Program] Third-batch extension toolsets registered (Snapshot/ValidationPoint/PageSetup/DocumentFormat/BatchOperation/Ticket/CollectConsolidate/CellBorder/DocumentCharFormat/TableStylePreset/DocumentInsert).");

            // 加载云端验证测试夹具（Task 3）
            try
            {
                AuditAI.McpServer.Services.TestFixtures.Load();
                Console.Error.WriteLine("[Program] TestFixtures loaded.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[Program] TestFixtures.Load failed: " + ex.Message);
            }

            // 云端全自动验证平台工具集（Task 4-10）
            AuditAI.McpServer.Tools.CloudApiTools.Register();
            AuditAI.McpServer.Tools.AssertionTools.Register();
            AuditAI.McpServer.Tools.SignalRTestTools.Register();
            AuditAI.McpServer.Tools.ScenarioTools.Register();
            AuditAI.McpServer.Tools.ServerOpsTools.Register();
            AuditAI.McpServer.Tools.AutoFixTools.Register();
            AuditAI.McpServer.Tools.TestReportTools.Register();
            AuditAI.McpServer.Tools.CollaborationReadinessTools.Register();
            AuditAI.McpServer.Tools.AcceptanceReportTools.Register();
            // 云端功能全面自动化测试覆盖（cloud-comprehensive-automation-coverage spec）
            AuditAI.McpServer.Tools.AdminApiTools.Register();
            AuditAI.McpServer.Tools.ComprehensiveReportTools.Register();
            Console.Error.WriteLine("[Program] Cloud verification toolsets registered (incl. Admin + Comprehensive).");

            // 启动 MCP 协议主循环
            AuditAI.McpServer.Protocol.McpServer.Run();
        }

        /// <summary>
        /// 初始化本地用户和团队
        /// 复用 AuditAI Program.cs 中的本地模式用户初始化逻辑
        /// </summary>
        /// <summary>
        /// 退出前的兜底：关闭当前项目，让 ProjectDAL.Dispose 执行 wal_checkpoint(TRUNCATE)，
        /// 避免客户端直接断连时部署目录/项目目录残留 -wal。
        /// 任何异常都不得影响退出流程，故全程吞掉并只写 stderr。
        /// </summary>
        private static void CloseCurrentProjectSafely(string trigger)
        {
            try
            {
                var state = AuditAI.McpServer.State.SessionState.Current;
                if (state != null && state.HasProject)
                {
                    state.CloseProject();
                    Console.Error.WriteLine($"[Program] {trigger}: 已关闭当前项目并 checkpoint");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[Program] {trigger}: 关闭项目失败（忽略）: {ex.Message}");
            }
        }

        private static void InitializeLocalUser()
        {
            // 设置本地团队
            Guid teamId = Guid.NewGuid();
            Auditai.Model.UserTeam.Current = new Auditai.Model.UserTeam
            {
                Id = teamId,
                Name = "本地团队",
                Type = 2, // Audit 版本
                LicenseDate = DateTime.Now.AddYears(10)
            };
            Auditai.Model.UserTeam.Teams = new System.Collections.Generic.List<Auditai.Model.UserTeam>();
            Auditai.Model.UserTeam.Teams.Add(Auditai.Model.UserTeam.Current);
            Auditai.Model.UserTeam.CurrentTeamIsPayByProject = true;

            // 设置本地管理员用户
            Auditai.Model.User.Current = new Auditai.Model.User
            {
                Id = 1,
                Name = "管理员",
                TelPhone = "13800138000",
                TeamId = teamId,
                IsTeamAdmin = true,
                IsSystemSupporter = true
            };
        }
    }
}
