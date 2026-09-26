# AuditAI 项目长期备忘

## 源码与构建

- **规范服务端源码 = `E:\lq\Server\AuditApiServer`**（独立 git，分支 `feature/ai-assistant`）。仓内 `.tmp-server/` 只是镜像快照，改动打在那里上不了生产。
- 全解决方案验证：`cd E:\lq\AuditAI && dotnet build AuditAI.sln -c Debug`（22 个项目）。客户端 `AuditAI/AuditAI.csproj` 是 SDK 风格 net48，**不需要 VS/C1 控件安装**即可 `dotnet build`（C1 程序集从仓内 `Libs/` 引用）。
- 本机 Git Bash 的 PATH 异常，命令前需 `export PATH="/usr/bin:/bin:/c/Windows/System32:$PATH"`；PowerShell 工具输出常为空，优先用 Bash。
- `git status` 里大量 "LF will be replaced by CRLF" 是 autocrlf 正常噪音。工作区长期有大量未提交改动，**动手前先 `git status` 区分归属**。

## 账套（Ledger SQLite）最小夹具构造法

`LedgerDAL` 构造函数会跑 `UpdateSchema()`，空库会在 `Update_Voucher_Number` 的 `source.First(...)` 抛「序列不包含任何匹配元素」。做数据层单测时只需：

```sql
CREATE TABLE `Voucher`(`id` INTEGER PRIMARY KEY, `number` TEXT,
  `DirectionToggled` INTEGER NOT NULL DEFAULT 0, `VoucherMark` INTEGER NOT NULL DEFAULT 0);
CREATE TABLE `Ledger`(`startDate` DATE, `EndDate` DATE);
PRAGMA user_version=4;   -- 跳过 0_1~3 历史迁移，且 Update_AccountBalanceValue 只在 num==3 时跑
```

即可让 `new LedgerDAL(path)` 通过，无需真实 Account/Item 数据。回归工程在 `.workbuddy/tmp/rc-verify/`（net48 控制台 + ProjectReference 真实 `LedgerModel.csproj` + `System.Data.SQLite.Core 1.0.119`）。

## 风险检查（RiskCheck）数据约定

- 表：`RiskCheckScheme` / `RiskCheckRule`（`id` 均为 `INTEGER PRIMARY KEY`）。方案保存是**规则全删全插**（`SaveRiskCheckScheme`）。
- **rule.Id 分配**：起点必须取「全表 `MAX(id)`」与「本方案保留 Id 的最大值」的**较大者**再递增。只取全表 MAX 会在单方案场景下与保留规则撞主键（已修，见 2026-09-25 日志）。
- 规则字段与 DB 列一一对应，无漏存漏读；`RiskCheckResult` 不落库（`Voucher` 引用不序列化）。
- 对称：组合期初 `ItemComboBalance` / `ItemComboBalanceRel` 也是保存时全删全插重建，且内存态 `ledger.ComboOpeningBalances` 不随「编辑期初余额」更新——改动这块要同时考虑脏标记。

## 表格模型的行主序不变式（改这块前必读）

- `Table.this[r,c]` → `CellCollection.Get` → `_list[r * Columns.Count + c]`。**`Cells._list` 必须严格行主序**，且 `Rows.Count × Columns.Count == Cells.Count`。
- 维护不变式的正确姿势：插入/删除行列（`RowCollection.cs:97`、`ColumnCollection.cs:181`、`Insert/DeleteRows|ColumnsCommand`）都是**按下标 splice**；`Syncer.Merge` 重建时用 `orderby Row.Index, Column.Index`。
- ⚠️ **`Table.EnsureAllCellsExist()` 是 `Cells._list.Add(...)` 追加到末尾**，空洞不在末尾时会让其后所有格位置索引整体错位（静默读到别的格）。**可达性未实测**（需"中间缺格"状态，最可能是 Merge 的"本地 New ∩ 服务器 New 未完整建格"）。5 个调用点全是"计数不一致就补"（Table.cs:678/943/975/2147、Syncer.cs:2221），而 Table.cs:658-663 注释明确把该计数不一致认定为**正常编辑中间态**，故这些补全路径会被日常使用常态进入。详见 2026-09-26 日志与第二轮报告。
- 反证（别往错方向追）：按 `CanLoad`/`LoadRowOwnerLoadView` 过滤掉中间行**不触发**补全（整行的 Cols 个格会一起被孤儿清理，计数同步减少）。
- 同一 (Row,Column) 位置**不允许重复 Cell**；`Table.TryRepairCellCountBeforeSave` 的取舍策略是"`Dirty.AnySet() || Status==New` 优先，其次有内容/公式优先"，改任何补全逻辑都必须并入该策略，否则会丢用户编辑内容。

## AuditAI.McpServer（云端验证/运维 MCP 服务）

- 传输**仅 stdio**，无网络监听。378 个工具注册名全唯一（`ToolRegistry` 字典不会静默吞工具）。
- `Services/ServerOpsService.cs` 经 SSH 操作生产机：`GetServerLogs`/`QueryServerDb` 的参数只做 `Replace("\"","\\\"")` 后拼 shell（`BuildSshArgs` 用双引号包裹）→ **远端 `$(...)`/反引号仍展开，可 root 执行任意命令**（`App.config` `SSHUser=root`）。改这里必须走 stdin 传参或做远端 shell 转义。
- `App.config` 含生产 `SSHHost`/`ServerDbPath`/`ServerDeployPath` + 开发机路径 `ServerProjectPath`，且 `AuditAISetup/AuditAI.iss:49` 把整个 McpServer 目录打进安装包（会随交付分发）；`TestUserName/TestPassword/SSHKeyPath` 为空。
- 输出路径校验有现成实现 `ExportService.ValidateOutputPath`（4 处在用）；`ProjectExtensionService.backup_project`、`DocumentService.ExportDocument`、`TableService.ExportTable` 未复用。
- **注意"同一逻辑多副本未同步修复"**：如许可证窗口匹配，主程序已改为"ComponentOne **且** 评估/试用"，本副本仍是"**或**"。改主程序时需全仓排查副本。


## 同步链路

- 客户端同步核心 `ProjectModel/Auditai.Model/Syncer.cs`；编排 `AuditAI/Auditai.UI.Platform/MainForm.cs`（`SyncProjectsCore`/`SyncProjectCore`，经 `_syncGate` 串行化）；HTTP 层 `WebApiLib/Auditai.Util/WebApiClient.cs`。
- 大表推送走任务流：客户端 `UploadTaskInputFile` 必须写**整表 Protobuf**（`ServerTaskInputFileStreamWriter.WriteRaw`），服务端 GET 端点直接 `PushTable.Parser.ParseFrom(整个文件)`；旧的分「逐集合增量」写入格式会让服务端解析出 invalid wire type。
