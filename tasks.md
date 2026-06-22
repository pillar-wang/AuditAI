# Task 10: 跨项目引用数据可视化标记

## 已完成

### 子任务 10.1: 在 CrossProjectDataRefManager 中添加状态标记
- `e:\lq\LeqiAudit-Decompiled\ProjectModel\Leqisoft.Model\CrossProjectDataRefManager.cs`
  - `DataRefResult.RefStatus` 属性已存在（默认0=Normal）
  - 在 `ExecuteRef` 成功写入数据后显式设置 `RefStatus = 0`（Normal）
  - 缓存降级时设置 `RefStatus = 1`（CacheFallback）
  - 默认值降级时设置 `RefStatus = 2`（DefaultValue）
  - 异常捕获中设置 `RefStatus = 3`（Error）
  - 在版本追踪后添加引用标记记录逻辑，写入 `CrossProjectRefCellStyle` 静态字典

### 子任务 10.2: 创建 CrossProjectRefCellStyle 帮助类
- 新建 `e:\lq\LeqiAudit-Decompiled\ProjectModel\Leqisoft.Model\CrossProjectRefCellStyle.cs`
  - `RefStatus` 枚举：Normal(0), CacheFallback(1), DefaultValue(2), Error(3), Refreshing(4)
  - `RefCellMark` 类：记录 RefId、状态、行列范围、来源信息、ToolTip 生成
  - 静态字典 `_marks` 存储所有引用的标记信息
  - 静态方法：SetMark、GetMark、GetAllMarks、ClearMark、ClearAll

### 子任务 10.3: 在 TableEditor 中扩展单元格样式
- `e:\lq\LeqiAudit-Decompiled\LeqiAudit\Leqisoft.UI.Platform\TableEditor.cs`
  - `PopulateTable()` 末尾添加 `ApplyCrossProjectRefCellStyles()` 调用
  - 新增 `ApplyCrossProjectRefCellStyles()` 方法：遍历所有引用标记，根据状态设置单元格背景色
    - CacheFallback → 浅黄色 (255,255,200) + 斜体
    - DefaultValue/Error → 浅红色 (255,220,220)
    - Refreshing → 浅蓝色 (200,220,255)
  - 新增 `ResolveRefGridRange()` 方法：根据 RefConfig 解析目标行列范围
  - 新增 `AttachCrossProjectRefToolTip()` 方法：在 MouseMove 中显示引用信息的 ToolTip
  - 构造函数中调用 `AttachCrossProjectRefToolTip()`

### 子任务 10.4: 在右键菜单增加引用操作
- `e:\lq\LeqiAudit-Decompiled\LeqiAudit\Leqisoft.UI.Platform\TableEditor.cs`
  - 新增 `cmdJumpToCrossProjectRef` / `lnkJumpToCrossProjectRef`："跳转到跨项目引用配置"
  - 新增 `cmdRefreshSingleRef` / `lnkRefreshSingleRef`："刷新此引用"
  - 两个菜单项添加到 `ctxCell` 右键菜单中
  - 实现了 `CmdJumpToCrossProjectRef_Click` / `CmdJumpToCrossProjectRef_CommandStateQuery`
  - 实现了 `CmdRefreshSingleRef_Click`（按当前单元格定位到具体引用并刷新）/ `CmdRefreshSingleRef_CommandStateQuery`

## 涉及的文件
- `e:\lq\LeqiAudit-Decompiled\ProjectModel\Leqisoft.Model\CrossProjectDataRefManager.cs`（修改）
- `e:\lq\LeqiAudit-Decompiled\ProjectModel\Leqisoft.Model\CrossProjectRefCellStyle.cs`（新建）
- `e:\lq\LeqiAudit-Decompiled\LeqiAudit\Leqisoft.UI.Platform\TableEditor.cs`（修改）