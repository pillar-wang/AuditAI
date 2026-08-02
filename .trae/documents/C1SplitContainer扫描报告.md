# C1SplitContainer 多面板(>=3)布局问题扫描报告

扫描日期: 2026-07-26
扫描目录: e:\lq\AuditAI

## 判定规则

- **触发条件**: C1SplitContainer 面板数量>=3，且同时存在 Dock(Left/Right/Top/Bottom)面板 和 非Dock主面板
- **规则A (AutoSizeElement)**: 若 = None 或 未设置 → 错误，应为 Both
- **规则B (主面板属性)**: 对非 Dock 的主面板，若缺 Location/Size/SizeRatio（或 SizeRatio!=100.0±0.01）→ 错误

## 扫描结果汇总

- 满足多面板条件(>=3+Dock+非Dock)的 SplitContainer 总数: 16
- **存在问题的数量: 15**
- 无问题的数量: 1

- **有问题的文件数量: 13**

### 有问题的文件列表

1. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\ChatForm.cs` (2 个 SplitContainer 问题)
2. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\dlgProjectEditor.cs` (1 个 SplitContainer 问题)
3. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\dlgTemplateEditor.cs` (1 个 SplitContainer 问题)
4. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmAccessManage.cs` (1 个 SplitContainer 问题)
5. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmAuxEdit.cs` (1 个 SplitContainer 问题)
6. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmNodeSelector.cs` (1 个 SplitContainer 问题)
7. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmNodeSelectorWithTicketRecord.cs` (1 个 SplitContainer 问题)
8. `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls.CellCollect\frmCellCollect.cs` (1 个 SplitContainer 问题)
9. `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\LinkForm.cs` (1 个 SplitContainer 问题)
10. `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\UpdateForm.cs` (1 个 SplitContainer 问题)
11. `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\SubsidiaryEditor.cs` (2 个 SplitContainer 问题)
12. `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\VoucherMarkedEditor.cs` (1 个 SplitContainer 问题)
13. `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\frmVoucherEditor.cs` (1 个 SplitContainer 问题)

## 问题详细报告

### 1. 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\ChatForm.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **3** (Dock=1, 非Dock=2)
- AutoSizeElement 当前值: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlGroup` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ✅ | ✅ | 27.437 | false | Dock面板 |
| `pnlChat` | None | ✅ | ✅ | 67.181 ❌ | - | **主面板⚠️** |
| `pnlSend` | None | ✅ | ✅ | (未设置) ❌ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlChat (Dock=None) 缺: SizeRatio≠100 (当前=67.181)
- ❌ B: 主面板 pnlSend (Dock=None) 缺: SizeRatio

**修复建议代码:**

```csharp
// ===== ctnAll 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 C1.Framework.AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 2 个非Dock面板)
//    面板 pnlChat 缺: SizeRatio≠100
this.pnlChat.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlChat.KeepRelativeSize = true;
//    面板 pnlSend 缺: SizeRatio
this.pnlSend.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlSend.KeepRelativeSize = true;
```

---

#### SplitContainer: `ctnSend`

- 面板总数: **3** (Dock=1, 非Dock=2)
- AutoSizeElement 当前值: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlSendToolbar` | None | ✅ | ✅ | 0.0 ❌ | false | **主面板⚠️** |
| `pnlSendButton` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 27.778 | false | Dock面板 |
| `pnlSendContent` | None | ✅ | ✅ | 100.0 ✅ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlSendToolbar (Dock=None) 缺: SizeRatio≠100 (当前=0.0)

**修复建议代码:**

```csharp
// ===== ctnSend 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 C1.Framework.AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 1 个非Dock面板)
//    面板 pnlSendToolbar 缺: SizeRatio≠100
this.pnlSendToolbar.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlSendToolbar.KeepRelativeSize = true;
```

---

### 2. 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\dlgProjectEditor.cs`

#### SplitContainer: `ctnMain`

- 面板总数: **6** (Dock=4, 非Dock=2)
- AutoSizeElement 当前值: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlButtons` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 5.946 | false | Dock面板 |
| `pnlProjectInfo` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ✅ | ✅ | 57.988 | - | Dock面板 |
| `c1SplitterPanel1` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ✅ | ✅ | 2.286 | - | Dock面板 |
| `pnlUserHeader` | None | ✅ | ✅ | 9.501 ❌ | - | **主面板⚠️** |
| `pnlEmpty` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 4.211 | - | Dock面板 |
| `pnlUserSelector` | None | ✅ | ✅ | 97.872 ❌ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlUserHeader (Dock=None) 缺: SizeRatio≠100 (当前=9.501)
- ❌ B: 主面板 pnlUserSelector (Dock=None) 缺: SizeRatio≠100 (当前=97.872)

**修复建议代码:**

```csharp
// ===== ctnMain 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 C1.Framework.AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 2 个非Dock面板)
//    面板 pnlUserHeader 缺: SizeRatio≠100
this.pnlUserHeader.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlUserHeader.KeepRelativeSize = true;
//    面板 pnlUserSelector 缺: SizeRatio≠100
this.pnlUserSelector.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlUserSelector.KeepRelativeSize = true;
```

---

### 3. 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\dlgTemplateEditor.cs`

#### SplitContainer: `ctnMain`

- 面板总数: **6** (Dock=4, 非Dock=2)
- AutoSizeElement 当前值: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlButtons` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | (未设置) | false | Dock面板 |
| `pnlInfoInput` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ✅ | ✅ | 59.683 | false | Dock面板 |
| `c1SplitterPanel1` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ✅ | ✅ | 1.818 | - | Dock面板 |
| `pnlUserHead` | None | ✅ | ✅ | 9.662 ❌ | - | **主面板⚠️** |
| `pnlEmpty` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 4.301 | - | Dock面板 |
| `pnlUserSelect` | None | ✅ | ✅ | 94.334 ❌ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlUserHead (Dock=None) 缺: SizeRatio≠100 (当前=9.662)
- ❌ B: 主面板 pnlUserSelect (Dock=None) 缺: SizeRatio≠100 (当前=94.334)

**修复建议代码:**

```csharp
// ===== ctnMain 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 C1.Framework.AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 2 个非Dock面板)
//    面板 pnlUserHead 缺: SizeRatio≠100
this.pnlUserHead.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlUserHead.KeepRelativeSize = true;
//    面板 pnlUserSelect 缺: SizeRatio≠100
this.pnlUserSelect.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlUserSelect.KeepRelativeSize = true;
```

---

### 4. 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmAccessManage.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **3** (Dock=1, 非Dock=2)
- AutoSizeElement 当前值: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlSearch` | None | ✅ | ✅ | 3.318 ❌ | false | **主面板⚠️** |
| `pnlButtons` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 5.548 | false | Dock面板 |
| `pnlEditor` | None | ✅ | ✅ | (未设置) ❌ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlSearch (Dock=None) 缺: SizeRatio≠100 (当前=3.318)
- ❌ B: 主面板 pnlEditor (Dock=None) 缺: SizeRatio

**修复建议代码:**

```csharp
// ===== ctnAll 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 C1.Framework.AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 2 个非Dock面板)
//    面板 pnlSearch 缺: SizeRatio≠100
this.pnlSearch.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlSearch.KeepRelativeSize = true;
//    面板 pnlEditor 缺: SizeRatio
this.pnlEditor.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlEditor.KeepRelativeSize = true;
```

---

### 5. 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmAuxEdit.cs`

#### SplitContainer: `ctnDropInput`

- 面板总数: **4** (Dock=2, 非Dock=2)
- AutoSizeElement 当前值: `AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlInput` | PanelDockStyle.Top | ✅ | ✅ | 11.396 | false | Dock面板 |
| `pnlFunctions` | None | ✅ | ✅ | 7.767 ❌ | false | **主面板⚠️** |
| `pnlFunctionHint` | PanelDockStyle.Top | ✅ | ✅ | 16.194 | false | Dock面板 |
| `pnlCombo` | None | ✅ | ✅ | (未设置) ❌ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlFunctions (Dock=None) 缺: SizeRatio≠100 (当前=7.767)
- ❌ B: 主面板 pnlCombo (Dock=None) 缺: SizeRatio

**修复建议代码:**

```csharp
// ===== ctnDropInput 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 2 个非Dock面板)
//    面板 pnlFunctions 缺: SizeRatio≠100
this.pnlFunctions.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlFunctions.KeepRelativeSize = true;
//    面板 pnlCombo 缺: SizeRatio
this.pnlCombo.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlCombo.KeepRelativeSize = true;
```

---

### 6. 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmNodeSelector.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **3** (Dock=1, 非Dock=2)
- AutoSizeElement 当前值: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlButton` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 10.0 | false | Dock面板 |
| `pnlSearch` | None | ✅ | ✅ | 4.598 ❌ | false | **主面板⚠️** |
| `pnlEditor` | None | ✅ | ✅ | (未设置) ❌ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlSearch (Dock=None) 缺: SizeRatio≠100 (当前=4.598)
- ❌ B: 主面板 pnlEditor (Dock=None) 缺: SizeRatio

**修复建议代码:**

```csharp
// ===== ctnAll 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 C1.Framework.AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 2 个非Dock面板)
//    面板 pnlSearch 缺: SizeRatio≠100
this.pnlSearch.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlSearch.KeepRelativeSize = true;
//    面板 pnlEditor 缺: SizeRatio
this.pnlEditor.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlEditor.KeepRelativeSize = true;
```

---

### 7. 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmNodeSelectorWithTicketRecord.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **3** (Dock=1, 非Dock=2)
- AutoSizeElement 当前值: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlButton` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 10.0 | false | Dock面板 |
| `pnlSearch` | None | ✅ | ✅ | 4.598 ❌ | false | **主面板⚠️** |
| `pnlEditor` | None | ✅ | ✅ | (未设置) ❌ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlSearch (Dock=None) 缺: SizeRatio≠100 (当前=4.598)
- ❌ B: 主面板 pnlEditor (Dock=None) 缺: SizeRatio

**修复建议代码:**

```csharp
// ===== ctnAll 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 C1.Framework.AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 2 个非Dock面板)
//    面板 pnlSearch 缺: SizeRatio≠100
this.pnlSearch.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlSearch.KeepRelativeSize = true;
//    面板 pnlEditor 缺: SizeRatio
this.pnlEditor.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlEditor.KeepRelativeSize = true;
```

---

### 8. 文件: `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls.CellCollect\frmCellCollect.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **3** (Dock=1, 非Dock=2)
- AutoSizeElement 当前值: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlHeader` | None | ✅ | ✅ | 6.0 ❌ | false | **主面板⚠️** |
| `pnlBottomBtn` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | (未设置) | false | Dock面板 |
| `pnlDockingTab` | None | ✅ | ✅ | 92.0 ❌ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlHeader (Dock=None) 缺: SizeRatio≠100 (当前=6.0)
- ❌ B: 主面板 pnlDockingTab (Dock=None) 缺: SizeRatio≠100 (当前=92.0)

**修复建议代码:**

```csharp
// ===== ctnAll 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 C1.Framework.AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 2 个非Dock面板)
//    面板 pnlHeader 缺: SizeRatio≠100
this.pnlHeader.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlHeader.KeepRelativeSize = true;
//    面板 pnlDockingTab 缺: SizeRatio≠100
this.pnlDockingTab.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlDockingTab.KeepRelativeSize = true;
```

---

### 9. 文件: `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\LinkForm.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **4** (Dock=3, 非Dock=1)
- AutoSizeElement 当前值: `AutoSizeElement.None`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlButton` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 25.806 | false | Dock面板 |
| `pnlImage` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ✅ | ✅ | 23.438 | false | Dock面板 |
| `pnlLink` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | (未设置) | - | Dock面板 |
| `pnlMessage` | None | ✅ | ✅ | 100.0 ✅ | - | **主面板⚠️** |

**问题:**

- ❌ A: AutoSizeElement = AutoSizeElement.None（错误，应为 Both）

**修复建议代码:**

```csharp
// ===== ctnAll 修复 ===== 
// 1. 确保 AutoSizeElement = Both
this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
```

---

### 10. 文件: `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\UpdateForm.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **3** (Dock=2, 非Dock=1)
- AutoSizeElement 当前值: `C1.Framework.AutoSizeElement.None`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlButtons` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ❌ | ❌ | (未设置) | false | Dock面板 |
| `pnlImage` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ❌ | ❌ | (未设置) | false | Dock面板 |
| `pnlContent` | None | ❌ | ❌ | (未设置) ❌ | - | **主面板⚠️** |

**问题:**

- ❌ A: AutoSizeElement = C1.Framework.AutoSizeElement.None（错误，应为 Both）
- ❌ B: 主面板 pnlContent (Dock=None) 缺: Location, Size, SizeRatio

**修复建议代码:**

```csharp
// ===== ctnAll 修复 ===== 
// 1. 确保 AutoSizeElement = Both
this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;

// 2. 修复主面板 (共 1 个非Dock面板)
//    面板 pnlContent 缺: Location, Size, SizeRatio
this.pnlContent.Location = new System.Drawing.Point(/*X*/0, /*Y*/0); // TODO: 根据Dock面板设置实际坐标
this.pnlContent.Size = new System.Drawing.Size(/*W*/0, /*H*/0); // TODO: 根据Dock面板设置实际尺寸
this.pnlContent.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlContent.KeepRelativeSize = true;
```

---

### 11. 文件: `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\SubsidiaryEditor.cs`

#### SplitContainer: `View`

- 面板总数: **5** (Dock=1, 非Dock=4)
- AutoSizeElement 当前值: `AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlSubsidiayFoot` | PanelDockStyle.Bottom | ✅ | ✅ | 3.657 | - | Dock面板 |
| `pnlSubsidiaryTitle` | None | ✅ | ✅ | 5.025 ❌ | false | **主面板⚠️** |
| `pnlSubsidiaryHead` | None | ✅ | ✅ | 2.0 ❌ | false | **主面板⚠️** |
| `pnlSubsidiaryGrid` | None | ❌ | ❌ | 59.74 ❌ | true | **主面板⚠️** |
| `pnlSubsidiaryVoucher` | None | ✅ | ✅ | 100.0 ✅ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlSubsidiaryTitle (Dock=None) 缺: SizeRatio≠100 (当前=5.025)
- ❌ B: 主面板 pnlSubsidiaryHead (Dock=None) 缺: SizeRatio≠100 (当前=2.0)
- ❌ B: 主面板 pnlSubsidiaryGrid (Dock=None) 缺: Location, Size, SizeRatio≠100 (当前=59.74)

**修复建议代码:**

```csharp
// ===== View 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 3 个非Dock面板)
//    面板 pnlSubsidiaryTitle 缺: SizeRatio≠100
this.pnlSubsidiaryTitle.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlSubsidiaryTitle.KeepRelativeSize = true;
//    面板 pnlSubsidiaryHead 缺: SizeRatio≠100
this.pnlSubsidiaryHead.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlSubsidiaryHead.KeepRelativeSize = true;
//    面板 pnlSubsidiaryGrid 缺: Location, Size
this.pnlSubsidiaryGrid.Location = new System.Drawing.Point(/*X*/0, /*Y*/0); // TODO: 根据Dock面板设置实际坐标
this.pnlSubsidiaryGrid.Size = new System.Drawing.Size(/*W*/0, /*H*/0); // TODO: 根据Dock面板设置实际尺寸
this.pnlSubsidiaryGrid.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlSubsidiaryGrid.KeepRelativeSize = true;
```

---

#### SplitContainer: `ctnVoucher`

- 面板总数: **4** (Dock=1, 非Dock=3)
- AutoSizeElement 当前值: `AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlVoucherFoot` | PanelDockStyle.Bottom | ✅ | ✅ | 100.0 ✅ | false | Dock面板 |
| `pnlVoucherTitle` | None | ✅ | ✅ | 10.204 ❌ | false | **主面板⚠️** |
| `pnlVoucherHead` | None | ✅ | ✅ | 2.0 ❌ | false | **主面板⚠️** |
| `pnlVoucherGrid` | None | ✅ | ✅ | 95.0 ❌ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlVoucherTitle (Dock=None) 缺: SizeRatio≠100 (当前=10.204)
- ❌ B: 主面板 pnlVoucherHead (Dock=None) 缺: SizeRatio≠100 (当前=2.0)
- ❌ B: 主面板 pnlVoucherGrid (Dock=None) 缺: SizeRatio≠100 (当前=95.0)

**修复建议代码:**

```csharp
// ===== ctnVoucher 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 3 个非Dock面板)
//    面板 pnlVoucherTitle 缺: SizeRatio≠100
this.pnlVoucherTitle.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlVoucherTitle.KeepRelativeSize = true;
//    面板 pnlVoucherHead 缺: SizeRatio≠100
this.pnlVoucherHead.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlVoucherHead.KeepRelativeSize = true;
//    面板 pnlVoucherGrid 缺: SizeRatio≠100
this.pnlVoucherGrid.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlVoucherGrid.KeepRelativeSize = true;
```

---

### 12. 文件: `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\VoucherMarkedEditor.cs`

#### SplitContainer: `ctnDetails`

- 面板总数: **3** (Dock=1, 非Dock=2)
- AutoSizeElement 当前值: `AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlDetailFoot` | PanelDockStyle.Bottom | ✅ | ✅ | 10.239 | false | Dock面板 |
| `pnlDetailHead` | None | ✅ | ✅ | 19.011 ❌ | false | **主面板⚠️** |
| `pnlDetailGrid` | None | ✅ | ✅ | (未设置) ❌ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlDetailHead (Dock=None) 缺: SizeRatio≠100 (当前=19.011)
- ❌ B: 主面板 pnlDetailGrid (Dock=None) 缺: SizeRatio

**修复建议代码:**

```csharp
// ===== ctnDetails 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 2 个非Dock面板)
//    面板 pnlDetailHead 缺: SizeRatio≠100
this.pnlDetailHead.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlDetailHead.KeepRelativeSize = true;
//    面板 pnlDetailGrid 缺: SizeRatio
this.pnlDetailGrid.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlDetailGrid.KeepRelativeSize = true;
```

---

### 13. 文件: `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\frmVoucherEditor.cs`

#### SplitContainer: `ctnVoucher`

- 面板总数: **5** (Dock=1, 非Dock=4)
- AutoSizeElement 当前值: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| 面板名 | Dock | Location | Size | SizeRatio | KeepRelativeSize | 备注 |
|--------|------|----------|------|-----------|------------------|------|
| `pnlVoucherFoot` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 12.745 | false | Dock面板 |
| `pnlToolbar` | None | ✅ | ✅ | 22.105 ❌ | false | **主面板⚠️** |
| `pnlVoucherTitle` | None | ✅ | ✅ | 9.774 ❌ | false | **主面板⚠️** |
| `pnlVoucherHead` | None | ✅ | ✅ | 12.146 ❌ | false | **主面板⚠️** |
| `pnlVoucherGrid` | None | ✅ | ✅ | 95.0 ❌ | - | **主面板⚠️** |

**问题:**

- ❌ B: 主面板 pnlToolbar (Dock=None) 缺: SizeRatio≠100 (当前=22.105)
- ❌ B: 主面板 pnlVoucherTitle (Dock=None) 缺: SizeRatio≠100 (当前=9.774)
- ❌ B: 主面板 pnlVoucherHead (Dock=None) 缺: SizeRatio≠100 (当前=12.146)
- ❌ B: 主面板 pnlVoucherGrid (Dock=None) 缺: SizeRatio≠100 (当前=95.0)

**修复建议代码:**

```csharp
// ===== ctnVoucher 修复 ===== 
// 1. 确保 AutoSizeElement = Both
// (当前 C1.Framework.AutoSizeElement.Both 已正确)

// 2. 修复主面板 (共 4 个非Dock面板)
//    面板 pnlToolbar 缺: SizeRatio≠100
this.pnlToolbar.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlToolbar.KeepRelativeSize = true;
//    面板 pnlVoucherTitle 缺: SizeRatio≠100
this.pnlVoucherTitle.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlVoucherTitle.KeepRelativeSize = true;
//    面板 pnlVoucherHead 缺: SizeRatio≠100
this.pnlVoucherHead.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlVoucherHead.KeepRelativeSize = true;
//    面板 pnlVoucherGrid 缺: SizeRatio≠100
this.pnlVoucherGrid.SizeRatio = 100.0;
// （可选）保持相对大小:
// this.pnlVoucherGrid.KeepRelativeSize = true;
```

---
