# 账务数据标签改造为"审计检查"标签 + 窗口按钮入口 Spec

## Why
账务数据页面拆分为独立窗口（LedgerWindow）后，页面顶部仍保留"账务数据"标签，既白占一片顶部区域，又把窗口入口绑定在标签点击上。需要把打开项目后主窗口顶部工具栏的"账务数据"标签更名为"审计检查"，在该标签下新增"账务数据"按钮作为打开账务数据窗口的入口，并移除账务数据窗口顶部工具栏上的"账务数据"标签。

## What Changes
- 主窗口 Ribbon 标签："账务数据"标签更名为"审计检查"；标签下的命令组（数据采集/账套管理/最近账套/账套查询/账套分析/采账填充/账套打印）继续保留在主窗口该标签下
- "审计检查"标签命令区首组新增"账务数据"按钮（新命令 `AppCommandLedgerWindow` + 新组 `AppGroupLedgerWindow`），点击调用 `MainForm.ShowLedgerWindow()` 打开/激活账务数据窗口
- 账务数据窗口（LedgerWindow）顶部 Ribbon 整体移除，账套查看器占满整个窗口
- 删除标签往返迁移机制（`MoveLedgerTabToLedgerWindow` / `MoveLedgerTabToMainForm` / `_ledgerTabMainIndex`），以及 `AppTabLedger.OnAppStateChanged` 中依赖 `LedgerWindowActive` 的常显分支
- 点击"审计检查"标签不再直接打开账务数据窗口（`Selected()` 覆写移除，恢复基类行为：仅选中标签）
- 既有入口（"打开账套"提示链接、收到账套文件后）由 `AppCommandTabs.Ledger.Select()` 改为直接调用 `MainForm.ShowLedgerWindow()`
- **BREAKING**: 账务数据窗口不再携带自身顶部工具栏；账务相关命令仅从主窗口"审计检查"标签使用；主窗口在部分视图模式下隐藏该标签时，账务命令对应隐藏

## Impact
- 受影响能力：Ribbon 标签体系、账务数据独立窗口管理、账套命令组可见性
- 受影响代码：
  - `AuditAI\Auditai.UI.Platform\AppTabLedger.cs`（更名、去掉 Selected/常显分支、挂新组）
  - `AuditAI\Auditai.UI.Platform\MainForm.cs`（删除迁移机制与字段、更新提示链接入口）
  - `AuditAI\Auditai.UI.Platform\LedgerWindow.cs`（移除 LedgerRibbon）
  - `AuditAI\Auditai.UI.Platform\AppCommands.cs` / `AppCommandGroups.cs`（注册新命令/新组）
  - `AuditAI\Auditai.UI.Platform\AppCommandLedgerWindow.cs`（新文件）
  - `AuditAI\Auditai.UI.Platform\SendFileMessage.cs`（入口调用改为 ShowLedgerWindow）
  - `AuditAI\IconRes.cs`（可选：新按钮图标）

## ADDED Requirements
### Requirement: "账务数据"打开按钮
系统 SHALL 在"审计检查"标签的命令区首组提供"账务数据"按钮，点击后打开（或激活已打开的）账务数据独立窗口。

#### Scenario: 成功打开
- **WHEN** 打开项目后点击"审计检查"标签下的"账务数据"按钮
- **THEN** 账务数据窗口显示（已打开时前置激活）；账务模块未授权时该按钮随标签一并隐藏

### Requirement: 账务数据窗口顶部标签去除
系统 SHALL 不再在账务数据窗口顶部显示"账务数据"标签及其载体 Ribbon，账套查看器占满窗口。

#### Scenario: 窗口内容占满
- **WHEN** 打开账务数据窗口
- **THEN** 窗口顶部无"账务数据"标签/Ribbon，账套查看器填充整个窗口

## MODIFIED Requirements
### Requirement: 主窗口顶部"审计检查"标签
原"账务数据"标签更名为"审计检查"，携带的命令组不变；点击标签仅选中并显示命令组，不再打开账务数据窗口；可见性与授权逻辑保持不变（账务模块未授权时整标签隐藏）。

### Requirement: 账务窗口入口调用更新
"账务数据"按钮、"打开账套"提示链接、收到账套文件后的入口均直接调用 `MainForm.ShowLedgerWindow()`；Ctrl+Q 切换账务窗口显隐的行为不变。

## REMOVED Requirements
### Requirement: 标签随窗口往返迁移
**Reason**: 账务数据窗口不再展示顶部标签，打开入口改由主窗口"审计检查"标签下的按钮承担，标签随窗口迁移已无意义
**Migration**: 删除 `MoveLedgerTabToLedgerWindow` / `MoveLedgerTabToMainForm` / `_ledgerTabMainIndex`、`LedgerWindow.LedgerRibbon` 及其初始化，清理 `AppTabLedger.OnAppStateChanged` 中依赖 `LedgerWindowActive` 的分支，删除已无引用的 `MainForm.LedgerWindowActive` 属性