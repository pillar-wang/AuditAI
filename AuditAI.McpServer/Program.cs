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
            // 设置 UTF-8 编码（MCP 协议要求）
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 初始化字符串常量（审计版本）
            // 必须在创建 Project 实例之前设置，因为 DataReferenceManager.InitDic() 会访问 StringConstBase.Current
            Auditai.Model.StringConstBase.Current = Auditai.Model.StringConstEditions.Audit;

            // 初始化本地存储模式
            // Server 模式下启用云端同步（Syncer 默认 Disabled=true，解耦自 StorageRouter）
            Auditai.LocalDataStore.StorageRouter.OnServerModeActivated = () => Auditai.Model.Syncer.Disabled = false;
            Auditai.LocalDataStore.StorageRouter.Initialize();

            // 加载管理后台基地址（Task 1：管理后台上下文，端口 8958）
            AuditAI.McpServer.State.SessionState.AdminBaseUrl = ConfigurationManager.AppSettings["AdminBaseUrl"] ?? "http://82.156.108.218:8958";

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
