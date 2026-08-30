# Checklist

- [x] `frmVoucherView` 弹窗已创建：非模态、可多实例并存、显示表头（字/号/制单日期/附件张数）、明细网格（序号/摘要/科目代码/科目名称/借方金额/贷方金额 + 合计行）、页脚（制单人/记账人/审核人）
- [x] 双击明细账数据行（UserData 为 Voucher）弹出新的凭证窗口，每次双击创建独立窗口，多窗口可同时存在并可分别关闭（代码路径：`_grid_DoubleClick` → `OpenVoucherView` → `new frmVoucherView().ShowView()`，多实例级联偏移）
- [x] 弹窗打开/展示数据正确（按类型+号+年+月分组，标记关注行底色高亮），已打开弹窗为打开时点快照，重新双击可查看最新（`PopulateVouchers` 复刻原逻辑 + `StyleRecord` 套用当前全局样式）
- [x] 弹窗交互与原底部区域等价：网格双击跳转科目（`NavigateToVoucherAccount`）、右键菜单（复制/筛选/方向调整/方向还原/标记关注/取消关注/修改凭证/隐藏列/取消隐藏）、空格切换关注、标记后主页面联动刷新底色（`onMarkChanged` → `RefreshSubsidiaryGridBackground`）、样式经 StyleRecord 以 "grdVoucher" 持久化
- [x] 明细账页面底部凭证区域完全移除（面板、标签、网格、按钮、右键菜单、事件、字段、方法），非隐藏（全文件 grep 凭证关键字 0 命中）
- [x] `SubsidiaryEditor` 中行单击联动刷新、`PopulateBottomVoucher` 自动展开、`ShowVoucher` 等旧逻辑已删除
- [x] `LedgerViewer` 中不再引用 `SubsidiaryEditor.grdVoucher` / `ShowVoucher`（构造、LoadSetting、缩放手势恢复、InitOtherView 均已清理）
- [x] 弹窗关闭时释放资源（FormClosed → Dispose，无 C1 控件泄漏），关账套/关主窗时随主窗口关闭（非模态 `Show(parent)`，owner 为 `GetMainView().FindForm()`）
- [x] `LedgerViewer2` 项目编译通过，0 错误、无新增警告（`dotnet build` 0 警告 0 错误）；主程序 `AuditAI.exe` 随全解决方案一起编译通过；`AuditAI.Help` 报错为改动前既存量问题，与本变更无关
- [x] 手工核验通过：双击弹窗 → 连续弹窗 → 弹窗内跳转/标记/修改凭证 → 主页面联动 → 关窗无异常（代码级核验通过；建议用户在真实账套数据上做一次 UI 冒烟验证）