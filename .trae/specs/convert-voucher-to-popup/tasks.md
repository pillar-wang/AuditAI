# Tasks

## Task 1: 新增 frmVoucherView 凭证查看弹窗
在 `LedgerViewer2/Auditai.UI.LedgerView/frmVoucherView.cs` 新建只读凭证查看弹窗（非模态、可多窗口并存、可多实例）。

- [x] 1.1 定义表单：`frmVoucherView : Form`，构造函数 `frmVoucherView(LedgerViewer owner, IEnumerable<Voucher> vouchers, Action<Voucher> onVoucherDoubleClick = null, Action onMarkChanged = null)`，`StartPosition = CenterScreen`，初始大小约 940×260（紧凑小巧，标题栏带内边距），主题图标 `Theme.SelectedAuditaiTheme.GetThemedIcon(...)`，`FormClosed` 时 `Dispose()` 释放 C1 控件
- [x] 1.2 构建界面：表头行（字/号/制单日期/附件张数，样式参照原 `pnlVoucherHead`）、`C1FlexGridEx grdVoucher`（`Name="grdVoucher"`，列定义复刻原 `InitializeVoucherCaption`：序号/摘要/科目代码/科目名称/借方金额/贷方金额）、页脚行（制单人/记账人/审核人，参照原 `pnlVoucherFoot`）
- [x] 1.3 数据填充：`PopulateVouchers(IEnumerable<Voucher>)`（复刻原逻辑：逐行填充 Index/Digest/Code/Name/Debit/Credit、标记关注底色、合计行、`SetVoucherHeader` 填充表头页脚）、填充后执行列自适应（复刻原 `AutoSizeVoucherColumns`）
- [x] 1.4 交互能力：
  - 网格双击：`UserData is Voucher` 时触发导航回调（`onVoucherDoubleClick` 交由 `SubsidiaryEditor` 处理跳转科目）
  - 右键菜单：复刻原 `BindVoucherContexMenu`（复制/筛选/方向调整/方向还原/标记关注/取消关注/修改凭证/隐藏列/取消隐藏），含 `ctxVouCell/ctxVouFixed/ctxVouEmpty`
  - 标记/取消关注与空格键：调用 `owner.MakeMark_Click/MarkCancel_Click` 后刷新本弹窗网格底色，并触发 `onMarkChanged` 通知主页面刷新
- [x] 1.5 样式持久化：`AfterResizeRow/AfterResizeColumn/AfterDragColumn` 通过 `owner.StyleRecord` 以 `"grdVoucher"` 名义记录行高/列宽/列序；`PopulateVouchers` 末尾 `owner.StyleRecord.ResumeStyle(grdVoucher)` 套用全局字体/行高
- [x] 1.6 编译通过：`LedgerViewer2` 项目无错误（0 警告 0 错误）

## Task 2: 改造 SubsidiaryEditor——双击弹窗 + 移除底部凭证区域
修改 `LedgerViewer2/Auditai.UI.LedgerView/SubsidiaryEditor.cs`。

- [x] 2.1 双击行为改造：`_grid_DoubleClick` 明细分支替换为：按「类型+号+年+月」收集凭证组 → `new frmVoucherView(_owner, vouchers, NavigateToVoucherAccount, RefreshSubsidiaryGridBackground)` → `ShowView()`；新增 `OpenVoucherView`/`NavigateToVoucherAccount`（含辅助核算跳转）；总账分支保持展开明细的原逻辑
- [x] 2.2 删除 `_grid_Click` 凭证分支及事件订阅（`grdSubsidiary.Click/RowColChange`）
- [x] 2.3 删除底部凭证区域控件与初始化：`pnlSubsidiaryVoucher`/`ctnVoucher`/`pnlVoucherTitle/Head/Grid/Foot`/`lblVoucher*`/`grdVoucher`/`lblChecker/Maker/Booker`/`lblNumAttachments`/`btnCloseVoucher`/`initializedVoucherCaption`/`_voucherVisible` 字段与创建、UI 构建整段、`View.Panels.Add(pnlSubsidiaryVoucher)` 已移除（其余 4 个 panel 保留）
- [x] 2.4 删除凭证相关方法：`ShowVoucher`、`PopulateVouchers`、`InitializeVoucherCaption`、`SetVoucherHeader`、`AutoSizeVoucherColumns`、`PopulateBottomVoucher`、`RefreshVouchersGridBackground`、`_grdVoucher_AfterResizeRow/Column`、`_grdVoucher_AfterDragColumn`、`GrdVoucher_KeyDown/Resize`、`BindVoucherContexMenu`、`GrdVoucher_MouseClick`、`_grdVoucher_DoubleClick` 及相应事件绑定（含 Paint/DrawFormBorder）
- [x] 2.5 清理残留引用：`PopulateSubsidiarySheet` 中 `PopulateBottomVoucher()`/`ShowVoucher(false)`/`ResumeStyle(grdVoucher)`/`AutoSizeVoucherColumns()` 调用、`MakeMarkImpl/CancelMarkImpl` 中 grdVoucher 分支与 `RefreshVouchersGridBackground()` 调用、`GrdSubsidiary_BeforeMouseDown` 末尾调用、`SetTheme` 中 `btnCloseVoucher/grdVoucher` 相关与 tooltip，以及 `cmdCopy2…lnkCancelHide2`/`ctxVou*` 字段全部移除；两处保留性替换（`grdVoucher.Styles.Add("center")`→`grdSubsidiary.Styles.Add(...)`、`RefreshSubsidiaryGridBackground` 内 `grdVoucher.ForeColor`→`grdSubsidiary.ForeColor`）
- [x] 2.6 编译通过：`LedgerViewer2` 项目无错误、无残留未使用告警（全文件 grep 凭证关键字 0 命中）

## Task 3: 清理 LedgerViewer 中对凭证区域控件的引用
修改 `LedgerViewer2/Auditai.UI.LedgerView/LedgerViewer.cs`。

- [x] 3.1 构造函数：删除 `subsidiaryEditor.ShowVoucher(visible: false);` 与 `AttachGenerateEvent1(subsidiaryEditor.grdVoucher);`
- [x] 3.2 `LoadSetting` 字体行高套用：删除 `SetGridStyle(SubsidiaryEditor.grdVoucher)` 与 `SetGridHeight(SubsidiaryEditor.grdVoucher)`
- [x] 3.3 缩放手势样式恢复：删除 `StyleRecord.ResumeFont/ResumeHeight(SubsidiaryEditor.grdVoucher)`；另清理 `InitOtherView()` 中的 `ShowVoucher(false)` 残留
- [x] 3.4 编译通过：`LedgerViewer2` 项目无错误（全文件 grep 仅剩 `voucherMarkedEditor.grdVouchers` 与 `ShowVoucherList()` 无关引用）

## Task 4: 构建与功能验证
- [x] 4.1 构建 `LedgerViewer2.csproj`（net48 / x86），0 错误 0 警告；全解决方案构建中 `LedgerViewer2` 与主程序 `AuditAI.exe` 均通过（仅存量项目 `AuditAI.Help` 存在与本次改动无关的既有框架引用错误）
- [x] 4.2 代码级核验通过（运行时行为需用户实测）：
  - 双击数据行 → `OpenVoucherView` 弹出 `frmVoucherView`（表头/网格/合计/页脚由 `PopulateVouchers` 填充）✓ 代码路径确认
  - 连续双击 → 每次 `new frmVoucherView` + `ShowView()` 多实例并存、级联偏移 ✓ 代码路径确认
  - 弹窗网格双击 → `NavigateToVoucherAccount` 跳转科目；右键菜单/标记/空格 → 弹窗与主页面联动刷新 ✓ 代码路径确认
  - 非模态 `Show(parent)` 随主窗关闭；`FormClosed → Dispose` 释放资源 ✓ 代码路径确认
- [x] 4.3 更新 `.trae/specs/convert-voucher-to-popup/checklist.md` 勾选结论

# Task Dependencies
- [Task 1] 独立，先行（弹窗不依赖移除逻辑）
- [Task 2] 依赖 [Task 1]（引用 `frmVoucherView`）
- [Task 3] 依赖 [Task 2]（引用 `SubsidiaryEditor.grdVoucher/ShowVoucher`；与 Task 2 不同文件，可在 Task 2 完成后并行收尾）
- [Task 4] 依赖 [Task 1][Task 2][Task 3] 全部完成