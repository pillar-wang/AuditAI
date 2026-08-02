# C1SplitContainer 多面板(>=3)布局问题扫描报告（修订版）

扫描日期: 2026-07-26
扫描目录: e:\lq\AuditAI

## 判定规则

### 触发条件
- C1SplitContainer **面板数量>=3**
- 且同时存在 **Dock面板**（Left/Right/Top/Bottom）和 **非Dock面板**

### 面板分类规则
- **Dock面板**: Dock = Left/Right/Top/Bottom（固定在外围）
- **固定尺寸非Dock面板**: KeepRelativeSize=false 且设置了 Height/Width，或 SizeRatio<1% 且 KRS=false（这类是标题栏/工具栏等固定小面板，不需要SizeRatio=100）
- **主填充面板**: 其余非Dock面板（占据剩余空间的核心面板）

### 错误判定
- **规则A (AutoSizeElement)**: 若 = None 或 未设置 → 错误，应为 Both
- **规则B (主填充面板)**: 对主填充面板，若缺 Location/Size/SizeRatio（或 SizeRatio≠100.0±0.01）→ 错误

## 扫描结果汇总

- 满足条件的 SplitContainer 总数: **16**
- 存在问题的 SplitContainer: **14**
- 无问题的 SplitContainer: 2

- **有问题的文件总数: 13**

- 规则A问题数 (AutoSizeElement): 2
- 规则B问题数 (主面板缺属性): 17

### 有问题的文件列表

1. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\ChatForm.cs` [1个SC, B×2]
2. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\dlgProjectEditor.cs` [1个SC, B×2]
3. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\dlgTemplateEditor.cs` [1个SC, B×2]
4. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmAccessManage.cs` [1个SC, B×2]
5. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmAuxEdit.cs` [1个SC, B×1]
6. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmNodeSelector.cs` [1个SC, B×1]
7. `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmNodeSelectorWithTicketRecord.cs` [1个SC, B×1]
8. `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls.CellCollect\frmCellCollect.cs` [1个SC, B×1]
9. `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\LinkForm.cs` [1个SC, A×1]
10. `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\UpdateForm.cs` [1个SC, A×1, B×1]
11. `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\SubsidiaryEditor.cs` [2个SC, B×2]
12. `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\VoucherMarkedEditor.cs` [1个SC, B×1]
13. `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\frmVoucherEditor.cs` [1个SC, B×1]

## 风险等级分类

### 🔴 严重 - AutoSizeElement 错误 (共 2 个SC)

- `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\LinkForm.cs` → `ctnAll`: AutoSizeElement=AutoSizeElement.None
- `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\UpdateForm.cs` → `ctnAll`: AutoSizeElement=C1.Framework.AutoSizeElement.None

### 🟠 高风险 - 主面板缺 Location/Size (共 2 个SC)

- `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\UpdateForm.cs` → `ctnAll`
  - B: 主面板 pnlContent (Dock=None) 缺: Location, Size, SizeRatio
- `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\SubsidiaryEditor.cs` → `View`
  - B: 主面板 pnlSubsidiaryGrid (Dock=None) 缺: Location, Size, SizeRatio≠100 (当前=59.74)

### 🟡 中等风险 - 仅 SizeRatio 不对 (共 11 个SC)

- `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\ChatForm.cs` → `ctnAll`
- `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\dlgProjectEditor.cs` → `ctnMain`
- `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\dlgTemplateEditor.cs` → `ctnMain`
- `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmAccessManage.cs` → `ctnAll`
- `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmAuxEdit.cs` → `ctnDropInput`
- `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmNodeSelector.cs` → `ctnAll`
- `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmNodeSelectorWithTicketRecord.cs` → `ctnAll`
- `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls.CellCollect\frmCellCollect.cs` → `ctnAll`
- `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\frmVoucherEditor.cs` → `ctnVoucher`
- `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\SubsidiaryEditor.cs` → `ctnVoucher`
- `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\VoucherMarkedEditor.cs` → `ctnDetails`

## 详细问题报告（含修复建议）

### 1. 🟡 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\ChatForm.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **3**
  - Dock面板: 1
  - 非Dock(主填充): 2 → `['pnlChat', 'pnlSend']`
- AutoSizeElement: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlGroup` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ✅ | ✅ | 27.437 | false | W=296 | Dock |
| 2 | `pnlChat` | None | ✅ | ✅ | 67.181 ❌ | - | H=450 | ⭐主面板 |
| 3 | `pnlSend` | None | ✅ | ✅ | (未设置) ❌ | - | H=220 | ⭐主面板 |

**问题清单:**

- ❌ B: 主面板 pnlChat (Dock=None) 缺: SizeRatio≠100 (当前=67.181)
- ❌ B: 主面板 pnlSend (Dock=None) 缺: SizeRatio

**修复建议代码:**

```csharp
// ===== ctnAll 修复建议 =====
// ✅ AutoSizeElement = C1.Framework.AutoSizeElement.Both 已正确

// 【B类】主填充面板修复
// 面板 pnlChat 缺少: SizeRatio
this.pnlChat.SizeRatio = 100.0;
// 面板 pnlSend 缺少: SizeRatio
this.pnlSend.SizeRatio = 100.0;
```

---

### 2. 🟡 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\dlgProjectEditor.cs`

#### SplitContainer: `ctnMain`

- 面板总数: **6**
  - Dock面板: 4
  - 非Dock(主填充): 2 → `['pnlUserHeader', 'pnlUserSelector']`
- AutoSizeElement: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlButtons` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 5.946 | false | H=78, W=1100 | Dock |
| 2 | `pnlProjectInfo` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ✅ | ✅ | 57.988 | - | H=547, W=637 | Dock |
| 3 | `c1SplitterPanel1` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ✅ | ✅ | 2.286 | - | W=10 | Dock |
| 4 | `pnlUserHeader` | None | ✅ | ✅ | 9.501 ❌ | - | H=52, W=450 | ⭐主面板 |
| 5 | `pnlEmpty` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 4.211 | - | H=21 | Dock |
| 6 | `pnlUserSelector` | None | ✅ | ✅ | 97.872 ❌ | - | H=473, W=450 | ⭐主面板 |

**问题清单:**

- ❌ B: 主面板 pnlUserHeader (Dock=None) 缺: SizeRatio≠100 (当前=9.501)
- ❌ B: 主面板 pnlUserSelector (Dock=None) 缺: SizeRatio≠100 (当前=97.872)

**修复建议代码:**

```csharp
// ===== ctnMain 修复建议 =====
// ✅ AutoSizeElement = C1.Framework.AutoSizeElement.Both 已正确

// 【B类】主填充面板修复
// 面板 pnlUserHeader 缺少: SizeRatio
this.pnlUserHeader.SizeRatio = 100.0;
// 面板 pnlUserSelector 缺少: SizeRatio
this.pnlUserSelector.SizeRatio = 100.0;
```

---

### 3. 🟡 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\dlgTemplateEditor.cs`

#### SplitContainer: `ctnMain`

- 面板总数: **6**
  - Dock面板: 4
  - 非Dock(主填充): 2 → `['pnlUserHead', 'pnlUserSelect']`
- AutoSizeElement: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlButtons` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | (未设置) | false | H=82 | Dock |
| 2 | `pnlInfoInput` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ✅ | ✅ | 59.683 | false | H=541, W=637 | Dock |
| 3 | `c1SplitterPanel1` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ✅ | ✅ | 1.818 | - | W=8 | Dock |
| 4 | `pnlUserHead` | None | ✅ | ✅ | 9.662 ❌ | - | H=52 | ⭐主面板 |
| 5 | `pnlEmpty` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 4.301 | - | H=21 | Dock |
| 6 | `pnlUserSelect` | None | ✅ | ✅ | 94.334 ❌ | - | H=463 | ⭐主面板 |

**问题清单:**

- ❌ B: 主面板 pnlUserHead (Dock=None) 缺: SizeRatio≠100 (当前=9.662)
- ❌ B: 主面板 pnlUserSelect (Dock=None) 缺: SizeRatio≠100 (当前=94.334)

**修复建议代码:**

```csharp
// ===== ctnMain 修复建议 =====
// ✅ AutoSizeElement = C1.Framework.AutoSizeElement.Both 已正确

// 【B类】主填充面板修复
// 面板 pnlUserHead 缺少: SizeRatio
this.pnlUserHead.SizeRatio = 100.0;
// 面板 pnlUserSelect 缺少: SizeRatio
this.pnlUserSelect.SizeRatio = 100.0;
```

---

### 4. 🟡 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmAccessManage.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **3**
  - Dock面板: 1
  - 非Dock(主填充): 2 → `['pnlSearch', 'pnlEditor']`
- AutoSizeElement: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlSearch` | None | ✅ | ✅ | 3.318 ❌ | false | - | ⭐主面板 |
| 2 | `pnlButtons` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 5.548 | false | H=52, W=1095 | Dock |
| 3 | `pnlEditor` | None | ✅ | ✅ | (未设置) ❌ | - | H=744, W=1095 | ⭐主面板 |

**问题清单:**

- ❌ B: 主面板 pnlSearch (Dock=None) 缺: SizeRatio≠100 (当前=3.318)
- ❌ B: 主面板 pnlEditor (Dock=None) 缺: SizeRatio

**修复建议代码:**

```csharp
// ===== ctnAll 修复建议 =====
// ✅ AutoSizeElement = C1.Framework.AutoSizeElement.Both 已正确

// 【B类】主填充面板修复
// 面板 pnlSearch 缺少: SizeRatio
this.pnlSearch.SizeRatio = 100.0;
// 面板 pnlEditor 缺少: SizeRatio
this.pnlEditor.SizeRatio = 100.0;
```

---

### 5. 🟡 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmAuxEdit.cs`

#### SplitContainer: `ctnDropInput`

- 面板总数: **4**
  - Dock面板: 2
  - 非Dock(主填充): 1 → `['pnlCombo']`
  - 非Dock(固定尺寸): 1 → `['pnlFunctions']`（已豁免SizeRatio检查）
- AutoSizeElement: `AutoSizeElement.Both`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlInput` | PanelDockStyle.Top | ✅ | ✅ | 11.396 | false | H=52 | Dock |
| 2 | `pnlFunctions` | None | ✅ | ✅ | 7.767 | false | H=31 | 🔧固定 |
| 3 | `pnlFunctionHint` | PanelDockStyle.Top | ✅ | ✅ | 16.194 | false | H=104 | Dock |
| 4 | `pnlCombo` | None | ✅ | ✅ | (未设置) ❌ | - | H=460 | ⭐主面板 |

**问题清单:**

- ❌ B: 主面板 pnlCombo (Dock=None) 缺: SizeRatio

**修复建议代码:**

```csharp
// ===== ctnDropInput 修复建议 =====
// ✅ AutoSizeElement = AutoSizeElement.Both 已正确

// 【B类】主填充面板修复
// 面板 pnlCombo 缺少: SizeRatio
this.pnlCombo.SizeRatio = 100.0;
```

---

### 6. 🟡 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmNodeSelector.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **3**
  - Dock面板: 1
  - 非Dock(主填充): 1 → `['pnlEditor']`
  - 非Dock(固定尺寸): 1 → `['pnlSearch']`（已豁免SizeRatio检查）
- AutoSizeElement: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlButton` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 10.0 | false | H=52 | Dock |
| 2 | `pnlSearch` | None | ✅ | ✅ | 4.598 | false | H=31 | 🔧固定 |
| 3 | `pnlEditor` | None | ✅ | ✅ | (未设置) ❌ | - | H=647, W=532 | ⭐主面板 |

**问题清单:**

- ❌ B: 主面板 pnlEditor (Dock=None) 缺: SizeRatio

**修复建议代码:**

```csharp
// ===== ctnAll 修复建议 =====
// ✅ AutoSizeElement = C1.Framework.AutoSizeElement.Both 已正确

// 【B类】主填充面板修复
// 面板 pnlEditor 缺少: SizeRatio
this.pnlEditor.SizeRatio = 100.0;
```

---

### 7. 🟡 文件: `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\frmNodeSelectorWithTicketRecord.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **3**
  - Dock面板: 1
  - 非Dock(主填充): 1 → `['pnlEditor']`
  - 非Dock(固定尺寸): 1 → `['pnlSearch']`（已豁免SizeRatio检查）
- AutoSizeElement: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlButton` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 10.0 | false | H=52 | Dock |
| 2 | `pnlSearch` | None | ✅ | ✅ | 4.598 | false | H=31 | 🔧固定 |
| 3 | `pnlEditor` | None | ✅ | ✅ | (未设置) ❌ | - | H=647, W=532 | ⭐主面板 |

**问题清单:**

- ❌ B: 主面板 pnlEditor (Dock=None) 缺: SizeRatio

**修复建议代码:**

```csharp
// ===== ctnAll 修复建议 =====
// ✅ AutoSizeElement = C1.Framework.AutoSizeElement.Both 已正确

// 【B类】主填充面板修复
// 面板 pnlEditor 缺少: SizeRatio
this.pnlEditor.SizeRatio = 100.0;
```

---

### 8. 🟡 文件: `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls.CellCollect\frmCellCollect.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **3**
  - Dock面板: 1
  - 非Dock(主填充): 1 → `['pnlDockingTab']`
  - 非Dock(固定尺寸): 1 → `['pnlHeader']`（已豁免SizeRatio检查）
- AutoSizeElement: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlHeader` | None | ✅ | ✅ | 6.0 | false | H=68, W=1014 | 🔧固定 |
| 2 | `pnlBottomBtn` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | (未设置) | false | H=78, W=1014 | Dock |
| 3 | `pnlDockingTab` | None | ✅ | ✅ | 92.0 ❌ | - | H=387, W=1014 | ⭐主面板 |

**问题清单:**

- ❌ B: 主面板 pnlDockingTab (Dock=None) 缺: SizeRatio≠100 (当前=92.0)

**修复建议代码:**

```csharp
// ===== ctnAll 修复建议 =====
// ✅ AutoSizeElement = C1.Framework.AutoSizeElement.Both 已正确

// 【B类】主填充面板修复
// 面板 pnlDockingTab 缺少: SizeRatio
this.pnlDockingTab.SizeRatio = 100.0;
```

---

### 9. 🔴 文件: `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\LinkForm.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **4**
  - Dock面板: 3
  - 非Dock(主填充): 1 → `['pnlMessage']`
- AutoSizeElement: `AutoSizeElement.None`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlButton` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 25.806 | false | W=666 | Dock |
| 2 | `pnlImage` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ✅ | ✅ | 23.438 | false | H=181, W=156 | Dock |
| 3 | `pnlLink` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | (未设置) | - | W=510 | Dock |
| 4 | `pnlMessage` | None | ✅ | ✅ | 100.0 ✅ | - | - | ⭐主面板 |

**问题清单:**

- ❌ A: AutoSizeElement = AutoSizeElement.None（错误，应为 Both）

**修复建议代码:**

```csharp
// ===== ctnAll 修复建议 =====
// 【A类】AutoSizeElement 当前: AutoSizeElement.None
this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
```

---

### 10. 🔴 文件: `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\UpdateForm.cs`

#### SplitContainer: `ctnAll`

- 面板总数: **3**
  - Dock面板: 2
  - 非Dock(主填充): 1 → `['pnlContent']`
- AutoSizeElement: `C1.Framework.AutoSizeElement.None`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlButtons` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ❌ | ❌ | (未设置) | false | - | Dock |
| 2 | `pnlImage` | C1.Win.C1SplitContainer.PanelDockStyle.Left | ❌ | ❌ | (未设置) | false | W=120 | Dock |
| 3 | `pnlContent` | None | ❌ | ❌ | (未设置) ❌ | - | - | ⭐主面板 |

**问题清单:**

- ❌ A: AutoSizeElement = C1.Framework.AutoSizeElement.None（错误，应为 Both）
- ❌ B: 主面板 pnlContent (Dock=None) 缺: Location, Size, SizeRatio

**修复建议代码:**

```csharp
// ===== ctnAll 修复建议 =====
// 【A类】AutoSizeElement 当前: C1.Framework.AutoSizeElement.None
this.ctnAll.AutoSizeElement = C1.Framework.AutoSizeElement.Both;

// 【B类】主填充面板修复
// 面板 pnlContent 缺少: Location, Size, SizeRatio
this.pnlContent.Location = new System.Drawing.Point(120, 0);
this.pnlContent.Size = new System.Drawing.Size(100, 100);
this.pnlContent.SizeRatio = 100.0;
```

---

### 11. 🟠 文件: `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\SubsidiaryEditor.cs`

#### SplitContainer: `View`

- 面板总数: **5**
  - Dock面板: 1
  - 非Dock(主填充): 2 → `['pnlSubsidiaryGrid', 'pnlSubsidiaryVoucher']`
  - 非Dock(固定尺寸): 2 → `['pnlSubsidiaryTitle', 'pnlSubsidiaryHead']`（已豁免SizeRatio检查）
- AutoSizeElement: `AutoSizeElement.Both`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlSubsidiayFoot` | PanelDockStyle.Bottom | ✅ | ✅ | 3.657 | - | H=28 | Dock |
| 2 | `pnlSubsidiaryTitle` | None | ✅ | ✅ | 5.025 | false | H=39 | 🔧固定 |
| 3 | `pnlSubsidiaryHead` | None | ✅ | ✅ | 2.0 | false | H=40 | 🔧固定 |
| 4 | `pnlSubsidiaryGrid` | None | ❌ | ❌ | 59.74 ❌ | true | H=327 | ⭐主面板 |
| 5 | `pnlSubsidiaryVoucher` | None | ✅ | ✅ | 100.0 ✅ | - | H=220 | ⭐主面板 |

**问题清单:**

- ❌ B: 主面板 pnlSubsidiaryGrid (Dock=None) 缺: Location, Size, SizeRatio≠100 (当前=59.74)

**修复建议代码:**

```csharp
// ===== View 修复建议 =====
// ✅ AutoSizeElement = AutoSizeElement.Both 已正确

// 【B类】主填充面板修复
// 面板 pnlSubsidiaryGrid 缺少: Location, Size, SizeRatio
this.pnlSubsidiaryGrid.Location = new System.Drawing.Point(0, 0);
this.pnlSubsidiaryGrid.Size = new System.Drawing.Size(100, 100);
this.pnlSubsidiaryGrid.SizeRatio = 100.0;
```

---

#### SplitContainer: `ctnVoucher`

- 面板总数: **4**
  - Dock面板: 1
  - 非Dock(主填充): 1 → `['pnlVoucherGrid']`
  - 非Dock(固定尺寸): 2 → `['pnlVoucherTitle', 'pnlVoucherHead']`（已豁免SizeRatio检查）
- AutoSizeElement: `AutoSizeElement.Both`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlVoucherFoot` | PanelDockStyle.Bottom | ✅ | ✅ | 100.0 | false | H=30 | Dock |
| 2 | `pnlVoucherTitle` | None | ✅ | ✅ | 10.204 | false | H=30 | 🔧固定 |
| 3 | `pnlVoucherHead` | None | ✅ | ✅ | 2.0 | false | H=25, W=927 | 🔧固定 |
| 4 | `pnlVoucherGrid` | None | ✅ | ✅ | 95.0 ❌ | - | H=126 | ⭐主面板 |

**问题清单:**

- ❌ B: 主面板 pnlVoucherGrid (Dock=None) 缺: SizeRatio≠100 (当前=95.0)

**修复建议代码:**

```csharp
// ===== ctnVoucher 修复建议 =====
// ✅ AutoSizeElement = AutoSizeElement.Both 已正确

// 【B类】主填充面板修复
// 面板 pnlVoucherGrid 缺少: SizeRatio
this.pnlVoucherGrid.SizeRatio = 100.0;
```

---

### 12. 🟡 文件: `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\VoucherMarkedEditor.cs`

#### SplitContainer: `ctnDetails`

- 面板总数: **3**
  - Dock面板: 1
  - 非Dock(主填充): 1 → `['pnlDetailGrid']`
  - 非Dock(固定尺寸): 1 → `['pnlDetailHead']`（已豁免SizeRatio检查）
- AutoSizeElement: `AutoSizeElement.Both`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlDetailFoot` | PanelDockStyle.Bottom | ✅ | ✅ | 10.239 | false | H=30 | Dock |
| 2 | `pnlDetailHead` | None | ✅ | ✅ | 19.011 | false | H=50 | 🔧固定 |
| 3 | `pnlDetailGrid` | None | ✅ | ✅ | (未设置) ❌ | - | H=212 | ⭐主面板 |

**问题清单:**

- ❌ B: 主面板 pnlDetailGrid (Dock=None) 缺: SizeRatio

**修复建议代码:**

```csharp
// ===== ctnDetails 修复建议 =====
// ✅ AutoSizeElement = AutoSizeElement.Both 已正确

// 【B类】主填充面板修复
// 面板 pnlDetailGrid 缺少: SizeRatio
this.pnlDetailGrid.SizeRatio = 100.0;
```

---

### 13. 🟡 文件: `e:\lq\AuditAI\LedgerViewer2\Auditai.UI.LedgerView\frmVoucherEditor.cs`

#### SplitContainer: `ctnVoucher`

- 面板总数: **5**
  - Dock面板: 1
  - 非Dock(主填充): 1 → `['pnlVoucherGrid']`
  - 非Dock(固定尺寸): 3 → `['pnlToolbar', 'pnlVoucherTitle', 'pnlVoucherHead']`（已豁免SizeRatio检查）
- AutoSizeElement: `C1.Framework.AutoSizeElement.Both`

**各面板详情:**

| # | 面板名 | Dock | Loc | Size | SizeRatio | KRS | H/W | 分类 |
|---|--------|------|-----|------|-----------|-----|-----|------|
| 1 | `pnlVoucherFoot` | C1.Win.C1SplitContainer.PanelDockStyle.Bottom | ✅ | ✅ | 12.745 | false | H=51, W=1030 | Dock |
| 2 | `pnlToolbar` | None | ✅ | ✅ | 22.105 | false | H=82 | 🔧固定 |
| 3 | `pnlVoucherTitle` | None | ✅ | ✅ | 9.774 | false | H=42, W=1030 | 🔧固定 |
| 4 | `pnlVoucherHead` | None | ✅ | ✅ | 12.146 | false | H=39, W=1030 | 🔧固定 |
| 5 | `pnlVoucherGrid` | None | ✅ | ✅ | 95.0 ❌ | - | H=444, W=1030 | ⭐主面板 |

**问题清单:**

- ❌ B: 主面板 pnlVoucherGrid (Dock=None) 缺: SizeRatio≠100 (当前=95.0)

**修复建议代码:**

```csharp
// ===== ctnVoucher 修复建议 =====
// ✅ AutoSizeElement = C1.Framework.AutoSizeElement.Both 已正确

// 【B类】主填充面板修复
// 面板 pnlVoucherGrid 缺少: SizeRatio
this.pnlVoucherGrid.SizeRatio = 100.0;
```

---

## ✅ 无问题的 SplitContainer（仅供参考）

- `e:\lq\AuditAI\AuditAI\Auditai.UI.Platform\ChatForm.cs` → `ctnSend` [面板数=3, Dock=1, 主=1]
- `e:\lq\AuditAI\CommonControls\Auditai.UI.Controls\MessageShowBox.cs` → `ctnAll` [面板数=3, Dock=2, 主=1]
