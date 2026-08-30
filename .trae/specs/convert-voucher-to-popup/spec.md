# 明细账查看凭证改为弹窗 Spec

## Why
明细账/总账页面底部固定占用了约 220px 高的凭证展示区域（`pnlSubsidiaryVoucher` 及其子面板 `ctnVoucher`），压缩了主表格的可视空间。用户希望把「查看凭证」改为弹窗：双击明细账某行弹出凭证窗口，继续点击其他行再弹出新的窗口。

## What Changes
- 新增只读凭证查看弹窗 `frmVoucherView`（非模态，可多窗口并存），内容与原底部区域等价：
  - 表头：字、号、制单日期、附件张数
  - 明细网格：序号/摘要/科目代码/科目名称/借方金额/贷方金额 + 合计行，标记关注行底色高亮
  - 页脚：制单人、记账人、审核人
- 明细账双击行为改造：明细模式下双击带 `Voucher` 的数据行 → 弹出新的 `frmVoucherView`；每次双击创建独立窗口，互不影响。
- 弹窗网格保留原底部区域的全部能力：双击凭证行跳转主页面到该凭证所属科目、右键菜单（复制/筛选/方向调整/方向还原/标记关注/取消关注/修改凭证/隐藏列/取消隐藏）、标记后主页面联动刷新底色、列宽/行高/列序继续通过 `StyleRecord` 以 `"grdVoucher"` 名义持久化、空格键切换关注标记。
- 完全移除 `SubsidiaryEditor` 中的凭证底部区域：`pnlSubsidiaryVoucher`、`ctnVoucher` 及 `pnlVoucherTitle/Head/Grid/Foot`、`lblVoucher*`、`grdVoucher`、`lblMaker/Booker/Checker`、`btnCloseVoucher` 等控件及相关方法、字段、右键菜单、事件（完整移除而非隐藏）。
- 移除 `LedgerViewer` 中对 `SubsidiaryEditor.grdVoucher` / `ShowVoucher` 的全部引用（构造初始化、样式套用、缩放手势样式恢复）。
- **BREAKING**：明细账行单击不再联动刷新凭证（原底部区域的实时跟随行为随区域移除）；已打开的弹窗展示打开时点的数据快照，关闭后重新双击即可查看最新数据。

## Impact
- Affected specs: 明细账/总账视图、凭证查看、凭证标记关注（关注蓝）、凭证右键菜单、账套字体缩放与列样式持久化
- Affected code:
  - `LedgerViewer2/Auditai.UI.LedgerView/frmVoucherView.cs`（新增弹窗表单）
  - `LedgerViewer2/Auditai.UI.LedgerView/SubsidiaryEditor.cs`（双击触发弹窗 + 移除底部凭证区域及逻辑）
  - `LedgerViewer2/Auditai.UI.LedgerView/LedgerViewer.cs`（清理 grdVoucher/ShowVoucher 引用）

## ADDED Requirements
### Requirement: 双击明细账行弹出凭证弹窗
系统 SHALL 在明细（Subsidiary）模式下，双击 `grdSubsidiary` 中 `UserData` 为 `Voucher` 的数据行时，打开一个新的非模态 `frmVoucherView` 弹窗，展示该行凭证按「类型 + 号 + 年 + 月」分组后的全部分录（表头、网格、合计行、页脚），且每次双击都创建新的独立弹窗。

#### Scenario: 双击打开弹窗
- **WHEN** 用户打开账套 → 明细账，双击任意数据行
- **THEN** 弹出凭证查看窗口，正确显示该凭证的表头、明细分录与合计、制单人/记账人/审核人；主页面底部不再出现凭证区域

#### Scenario: 连续双击多行弹出多个窗口
- **WHEN** 用户继续双击其他数据行
- **THEN** 每行各自弹出新的凭证窗口，多个弹窗同时存在、可分别移动与关闭

#### Scenario: 总账模式双击
- **WHEN** 总账模式下双击数据行
- **THEN** 保持原行为：先展开为明细账，再按明细模式处理弹窗（不倒退）

### Requirement: 弹窗查看能力与原底部区域等价
弹窗 SHALL 提供与原底部区域等价的能力：网格双击跳转科目、右键菜单齐全、标记关注后主页面网格联动刷新底色、网格样式通过 `StyleRecord` 以 `"grdVoucher"` 名义读写（列宽/行高/列序）、主题与字体缩放风格一致、窗体关闭时释放资源。

#### Scenario: 弹窗内双击凭证分录跳转科目
- **WHEN** 用户在弹窗网格中双击某条凭证分录
- **THEN** 主页面明细账（含辅助核算时）跳转到该分录所属科目并刷新，弹窗保持打开

#### Scenario: 弹窗内标记/取消关注
- **WHEN** 用户在弹窗网格上执行「标记关注」/「取消关注」或按空格键切换
- **THEN** 弹窗网格与主明细账网格中对应行的底色同时刷新；修改凭证（右键菜单）可正常打开编辑器

#### Scenario: 弹窗样式持久化
- **WHEN** 用户在弹窗中调整列宽/行高/列序后重新打开凭证
- **THEN** 调整结果经 `StyleRecord` 持久化并在新弹窗中生效

## MODIFIED Requirements
### Requirement: 明细账页内凭证区域（删除）
原 `pnlSubsidiaryVoucher` 区域及其显示/隐藏切换（`ShowVoucher`）、行单击联动刷新（`_grid_Click` 凭证分支）、`PopulateBottomVoucher` 自动展开、`RefreshVouchersGridBackground` 等全部行为移除，统一由「双击 → 弹窗」交互取代。

## REMOVED Requirements
### Requirement: 明细账页内凭证区域
**Reason**: 用户要求凭证查看改为弹窗，不再占用页面区域；按项目规范控件应完整移除而非隐藏。
**Migration**: 双击数据行以弹窗形式查看凭证；原凭证网格右键菜单（修改凭证/标记关注/方向调整/隐藏列等）迁移至弹窗网格右键菜单。