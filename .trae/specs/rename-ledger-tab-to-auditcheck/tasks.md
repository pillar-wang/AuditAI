# Tasks

- [x] Task 1: 主窗口标签更名与点击行为调整（`AppTabLedger.cs`）
  - SubTask 1.1: `Text` 由"账务数据"改为"审计检查"
  - SubTask 1.2: 移除 `Selected()` 覆写（点击标签仅选中，不再调用 ShowLedgerWindow）
  - SubTask 1.3: `OnAppStateChanged` 移除依赖 `LedgerWindowActive` 的常显分支，可见性逻辑与其他普通标签（如 AppTabFile）一致，保留模块授权判断
  - SubTask 1.4: 更新文件中"标签随页面迁入账务窗口"等过时注释

- [x] Task 2: 新增"账务数据"命令与按钮组
  - SubTask 2.1: 新建 `AppCommandLedgerWindow.cs`（Text="账务数据"，LargeIcon 使用现有账务相关图标或按瓦片风格新增，Clicked → `Program.MainForm.ShowLedgerWindow()`）
  - SubTask 2.2: 在 `AppCommands.cs` 静态属性中注册 `AppCommands.LedgerWindow`
  - SubTask 2.3: 新建 `AppGroupLedgerWindow` 组（Text="账务数据"），作为 `AppTabLedger.Groups` 的首个元素，保证按钮位于"审计检查"标签下第一组（紧邻标签页头）

- [x] Task 3: 移除账务数据窗口顶部 Ribbon 与迁移机制（`LedgerWindow.cs`、`MainForm.cs`）
  - SubTask 3.1: `LedgerWindow.cs` 删除 `LedgerRibbon` 属性、初始化代码及 `AttachViewer` 中的 `Controls.Add(LedgerRibbon)`，viewer 以 Dock.Fill 占满窗口
  - SubTask 3.2: `MainForm.cs` 删除 `MoveLedgerTabToLedgerWindow` / `MoveLedgerTabToMainForm` 方法及 `_ledgerTabMainIndex` 字段
  - SubTask 3.3: `ShowLedgerWindow` 删除 `MoveLedgerTabToLedgerWindow()` 调用；`WindowHidden` 委托仅保留 `HideRelatedLedgerTip()`；`ToggleLedgerWindow` 隐藏分支删除 `MoveLedgerTabToMainForm()` 调用
  - SubTask 3.4: 确认 `LedgerWindowActive` 无其他引用后删除该属性及对应注释（含 `AppTabLedger` 中引用，已在 Task 1 清理）

- [x] Task 4: 更新既有入口调用
  - SubTask 4.1: `MainForm.cs` 的 `ShowOpenLedgerTip` / `ShowLedgerNotFoundTip` 链接事件中 `AppCommandTabs.Ledger.Select()` 改为 `Program.MainForm.ShowLedgerWindow()`
  - SubTask 4.2: `SendFileMessage.cs` 接收账套文件成功后 `AppCommandTabs.Ledger.Select()` 改为 `Program.MainForm.ShowLedgerWindow()`

- [x] Task 5: 编译与行为验证
  - SubTask 5.1: MSBuild 编译 AuditAI 主项目（`AuditAI\AuditAI.csproj`），达到 0 error（`dotnet build` 结果：已成功生成，0 个错误）
  - SubTask 5.2: 按 checklist.md 逐项核对（UI 行为项以代码审查 + 人工运行确认）

# Task Dependencies
- Task 1 先行（标签更名与可见性调整，Task 2/3/4 的前提）
- Task 2 与 Task 3 在 Task 1 完成后可并行执行（分别改动 AppCommands/AppCommandGroups/新文件 与 LedgerWindow/MainForm）
- Task 4 依赖 Task 1（4.1 改动 MainForm，与 3.2/3.3 同文件，须等 Task 3 完成后再合并改动避免冲突）
- Task 5 依赖 Task 1-4 全部完成