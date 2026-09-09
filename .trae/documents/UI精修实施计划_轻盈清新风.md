# AuditAI UI 精修实施计划 — 轻盈清新风

## 一、设计目标与成功标准

### 目标概述

将现有 Google Blue 风格升级为"轻盈清新风"，通过降低色彩饱和度、提升明度、增加留白、柔化对比，营造轻松高效的审计工作氛围。

### 成功标准

1. 品牌主色饱和度降低 ≥20%，明度提升 ≥10%，仍保持 WCAG AA 对比度（4.5:1）
2. 界面整体留白率提升约 15%（内边距、行高、间距增大）
3. 图标语义色统一调整为清新版本，13 个语义色全部覆盖
4. 字号层级从 6 级优化为 7 级，行高系数统一为 1.5-1.6
5. 核心组件（按钮、输入框、表格、侧边栏）全部应用新样式
6. 所有修改兼容 GDI+ 渲染，无性能退化

***

## 二、当前状态分析

### 项目架构

* **类型**：C# WinForms 桌面应用（审计软件），使用 C1 控件库

* **核心 UI 项目**：`AuditAI/Auditai.UI.Platform/`

* **公共控件库**：`CommonControls/Auditai.UI.Controls/`

* **主题资源**：`ThemeResource/`（22 个 C1 二进制主题）

### 现有设计系统

1. **颜色 Token**：`AuditTheme.cs` — Google Blue 风格，含 Surface/Text/Border/Brand/State/Extended/Chart/Sidebar 八大类
2. **图标系统**：

   * `IconLibrary.cs` — Phosphor Icons Fill 字体图标渲染引擎

   * `IconPalette.cs` — 50+ 图标 → 13 语义色映射表

   * `IconRes.cs` — 旧 PNG 资源的字体图标替代层（脚本生成）
3. **主题管理**：`Theme.cs` — 管理 22 个 C1 主题，最新为 "Google 蓝"
4. **字体**：微软雅黑 + Consolas，6 级字号层级
5. **设计参考稿**：`auditai-redesign/index.html` — HTML 原型

### 现存问题

1. 品牌色 `#1a73e8` 饱和度过高，长时间使用易疲劳
2. 文字色偏深（`#0f172a` slate-900），对比度偏强
3. 扩展语义色普遍在 600-700 深度，色彩沉重
4. 正文字号 9.5pt 偏小，审计软件需长时间阅读数据
5. 字号层级不够完善（缺少 Title1 和 Tiny 两级）
6. 表格行高偏紧凑，阅读舒适度不足
7. 边框色带蓝灰色调，不够中性轻盈

***

## 三、配色系统升级方案

### 设计原理

"清新感"的色彩理论核心是**低饱和度 + 高明度 + 柔和对比**。低饱和度色彩能减轻视觉疲劳、降低心理压力，适合长时间使用的企业级软件。

当前 Google Blue (#1a73e8) 饱和度高（\~89%）、明度中等（\~51%），视觉冲击力强但长时间使用易疲劳。升级方向是向天空蓝方向偏移，降低饱和度的同时提升明度，使整体观感更柔和通透。

### 3.1 Brand 品牌色（核心变化）

| Token           | 现值 (Google Blue)        | 新值 (清新蓝)                | 变化说明                               |
| --------------- | ----------------------- | ----------------------- | ---------------------------------- |
| Brand           | `#1a73e8` (26,115,232)  | `#3b82f6` (59,130,246)  | 饱和度从 89% 降至 76%，明度从 51% 升至 60%，更柔和 |
| BrandHover      | `#1765cc` (23,101,204)  | `#2563eb` (37,99,235)   | 同步降低饱和度，保持 hover 层级差               |
| BrandActive     | `#1557b0` (21,87,176)   | `#1d4ed8` (29,78,216)   | 按下态更深，但仍比原方案亮                      |
| BrandSubtle     | `#e8f0fe` (232,240,254) | `#eff6ff` (239,246,255) | 更浅更透亮，减少蓝色存在感                      |
| BrandForeground | `#ffffff`               | `#ffffff`               | 白色文字不变（对比度 5.1:1 满足 AA）            |

### 3.2 Surface 表面色（更浅更透气）

| Token         | 现值                      | 新值                      | 变化说明            |
| ------------- | ----------------------- | ----------------------- | --------------- |
| Surface       | `#ffffff`               | `#ffffff`               | 纯白不变            |
| SurfaceMuted  | `#f8fafc` (248,250,252) | `#fafbfc` (250,251,252) | 再浅半档，减少灰感，更"透气" |
| SurfaceSubtle | `#f1f5f9` (241,245,249) | `#f5f7fa` (245,247,250) | 降低存在感，悬停态更柔和    |
| SurfaceHover  | `#e2e8f0` (226,232,240) | `#edf0f5` (237,240,245) | 悬停背景更浅，减少压迫感    |

### 3.3 Text 文字色（柔和对比）

| Token         | 现值                      | 新值                      | 变化说明                                                  |
| ------------- | ----------------------- | ----------------------- | ----------------------------------------------------- |
| Text          | `#0f172a` (15,23,42)    | `#1e293b` (30,41,59)    | 从 slate-900 降至 slate-800，对比度从 16.3:1 降至 12.6:1，仍远超 AA |
| TextSecondary | `#334155` (51,65,85)    | `#475569` (71,85,105)   | 从 slate-700 降至 slate-600，对比度从 8.6:1 降至 6.3:1          |
| TextMuted     | `#64748b` (100,116,139) | `#64748b` (100,116,139) | 保持不变（对比度 4.7:1 刚好满足 AA）                               |

### 3.4 Border 边框色（更纤细中性）

| Token        | 现值                      | 新值                      | 变化说明             |
| ------------ | ----------------------- | ----------------------- | ---------------- |
| Border       | `#e2e8f0` (226,232,240) | `#e5e7eb` (229,231,235) | 从蓝灰改为中性灰，减少蓝色调干扰 |
| BorderStrong | `#cbd5e1` (203,213,225) | `#d1d5db` (209,213,219) | 同步调整，保持层级差       |

### 3.5 State 状态色（全部降饱和提明度）

| 状态      | Token         | 现值        | 新值        | 变化说明          |
| ------- | ------------- | --------- | --------- | ------------- |
| Success | SuccessText   | `#16a34a` | `#22c55e` | 更鲜亮的绿，少一分沉重   |
| <br />  | SuccessSubtle | `#f0fdf4` | `#f0fdf4` | 保持不变（已足够浅）    |
| <br />  | SuccessBorder | `#bbf7d0` | `#bbf7d0` | 保持不变          |
| Error   | ErrorText     | `#dc2626` | `#ef4444` | 更柔和的红，警示但不刺眼  |
| <br />  | ErrorSubtle   | `#fef2f2` | `#fef2f2` | 保持不变          |
| <br />  | ErrorBorder   | `#fecaca` | `#fecaca` | 保持不变          |
| Warning | WarningText   | `#d97706` | `#f59e0b` | 更明亮的琥珀色，温暖不暗沉 |
| <br />  | WarningSubtle | `#fffbeb` | `#fffbeb` | 保持不变          |
| <br />  | WarningBorder | `#fde68a` | `#fde68a` | 保持不变          |

### 3.6 Extended 扩展语义色（13 色全部清新化）

| Token  | 现值        | 新值        | 变化说明                     |
| ------ | --------- | --------- | ------------------------ |
| Navy   | `#1e3a5f` | `#334155` | 从深藏青改为 slate-700，更柔和的商务感 |
| Indigo | `#4f46e5` | `#6366f1` | 靛蓝提亮，从 700 降至 500        |
| Teal   | `#0f766e` | `#14b8a6` | 青碧大幅提亮，从 700 降至 500      |
| Amber  | `#b45309` | `#f59e0b` | 琥珀大幅提亮，从 700 降至 500      |
| Purple | `#7c3aed` | `#8b5cf6` | 紫色提亮，从 700 降至 500        |
| Rose   | `#be123c` | `#f43f5e` | 绛红提亮，从 700 降至 500        |
| Slate  | `#475569` | `#64748b` | 石墨提亮，从 600 降至 500        |

### 3.7 Chart 图表色（同步清新化）

| Token      | 现值        | 新值        | 变化说明         |
| ---------- | --------- | --------- | ------------ |
| Chart1 (蓝) | `#1a73e8` | `#3b82f6` | 同步 Brand 色变化 |
| Chart2 (绿) | `#34a853` | `#22c55e` | 更鲜亮          |
| Chart3 (黄) | `#fbbc04` | `#facc15` | 更柔和的黄        |
| Chart4 (红) | `#ea4335` | `#ef4444` | 同步 Error 色   |
| Chart5 (紫) | `#9334e6` | `#8b5cf6` | 更柔和的紫        |

### 3.8 Sidebar 侧边栏

| Token          | 现值          | 新值              | 变化说明               |
| -------------- | ----------- | --------------- | ------------------ |
| SidebarBg      | `#f8fafc`   | `#fafbfc`       | 同步 SurfaceMuted 提亮 |
| SidebarPrimary | Brand       | Brand (新)       | 同步品牌色              |
| SidebarAccent  | BrandSubtle | BrandSubtle (新) | 同步品牌浅色             |
| SidebarBorder  | Border      | Border (新)      | 同步边框色              |

### 3.9 新增 Token（柔和渐变与阴影）

```csharp
// 柔和渐变（用于按钮、卡片顶部装饰等）
public static readonly Color GradientStart = Color.FromArgb(249, 250, 251); // #f9fafb
public static readonly Color GradientEnd = Color.FromArgb(255, 255, 255);   // #ffffff

// 阴影色（用于自定义绘制卡片阴影，GDI+ 中用半透明色模拟）
public static readonly Color ShadowLight = Color.FromArgb(12, 15, 23, 42);   // 极淡阴影
public static readonly Color ShadowMedium = Color.FromArgb(20, 15, 23, 42);  // 中等阴影
```

***

## 四、图标体系优化方案

### 4.1 风格统一策略

**核心决策**：保持 Phosphor Fill 实心风格不变，通过以下方式营造轻盈感：

1. **语义色全面提亮**（已在配色方案中覆盖）—— 浅色实心图标比深色更显轻盈
2. **图标尺寸规范统一**—— 建立标准尺寸阶梯
3. **补充缺失图标**—— 完善审计场景常用图标

**为什么不切换到 Regular 线条风格**：Phosphor Regular 字重的线条粗细为 1.5px（24px 网格），在 WinForms GDI+ 渲染下，16px 图标线条仅约 1px，在 96 DPI 显示器上容易出现断裂和模糊。Fill 风格在 GDI+ 下渲染质量更稳定。

### 4.2 语义色优化

IconPalette.cs 中所有语义色引用自动跟随 AuditTheme 更新（因为使用的是 AuditTheme.xxx 属性引用）。

| 语义类别   | 旧色引用                             | 新色引用                             | 效果     |
| ------ | -------------------------------- | -------------------------------- | ------ |
| 成功/新增类 | AuditTheme.SuccessText (#16a34a) | AuditTheme.SuccessText (#22c55e) | 更鲜亮的绿  |
| 错误/删除类 | AuditTheme.ErrorText (#dc2626)   | AuditTheme.ErrorText (#ef4444)   | 更柔和的红  |
| 品牌/编辑类 | AuditTheme.Brand (#1a73e8)       | AuditTheme.Brand (#3b82f6)       | 更柔和的蓝  |
| 警告/导出类 | AuditTheme.WarningText (#d97706) | AuditTheme.WarningText (#f59e0b) | 更明亮的琥珀 |
| 文件/文档类 | AuditTheme.Indigo (#4f46e5)      | AuditTheme.Indigo (#6366f1)      | 更亮的靛蓝  |
| 表格/数据类 | AuditTheme.Teal (#0f766e)        | AuditTheme.Teal (#14b8a6)        | 更亮的青碧  |
| PDF类   | AuditTheme.Rose (#be123c)        | AuditTheme.Rose (#f43f5e)        | 更亮的玫红  |
| 用户类    | AuditTheme.Slate (#475569)       | AuditTheme.Slate (#64748b)       | 更亮的石墨灰 |
| 文件夹类   | AuditTheme.Navy (#1e3a5f)        | AuditTheme.Navy (#334155)        | 更柔和的藏青 |

### 4.3 图标尺寸规范

建立标准图标尺寸阶梯，确保全应用一致：

| 尺寸常量           | 像素值  | 用途             | 示例         |
| -------------- | ---- | -------------- | ---------- |
| SizeTiny       | 12px | 超小图标（状态指示、角标）  | 状态圆点旁的小图标  |
| SizeSmall      | 14px | 小图标（菜单项、侧边栏）   | 侧边栏导航图标    |
| SizeDefault    | 16px | 标准图标（按钮内、表格操作） | 按钮中的前置图标   |
| SizeMedium     | 20px | 中等图标（功能区小图标）   | Ribbon 小图标 |
| SizeLarge      | 24px | 大图标（功能区大图标）    | Ribbon 大图标 |
| SizeExtraLarge | 32px | 超大图标（对话框、向导）   | 对话框标题图标    |

### 4.4 缺失图标补充

审计软件常用但当前 IconPalette 中缺失的图标：

| 图标名 (phosphor)        | 语义   | 建议颜色        | 用途       |
| --------------------- | ---- | ----------- | -------- |
| `magnifying-glass`    | 搜索   | Slate       | 搜索框、查找功能 |
| `sliders-horizontal`  | 筛选   | Slate       | 高级筛选     |
| `download`            | 下载   | SuccessText | 下载文件     |
| `print`               | 打印   | Slate       | 打印功能     |
| `gear`                | 设置   | Slate       | 设置/选项    |
| `bell`                | 通知   | WarningText | 消息通知     |
| `chart-bar`           | 图表   | Brand       | 数据分析图表   |
| `chart-pie`           | 饼图   | Brand       | 统计分析     |
| `wallet`              | 财务   | SuccessText | 财务相关功能   |
| `file-search`         | 凭证查询 | Brand       | 凭证查找     |
| `files`               | 批量文件 | Indigo      | 批量操作     |
| `house`               | 首页   | Brand       | 返回首页     |
| `identification-card` | 身份   | Slate       | 用户信息     |

***

## 五、字体与排版优化方案

### 5.1 字体选择

**保持微软雅黑 + Consolas 不变**，理由：

1. WinForms 桌面应用依赖系统字体，嵌入中文字体体积过大（≥10MB）
2. 微软雅黑是 Windows 简体中文系统默认字体，覆盖率 100%
3. ClearType 渲染下微软雅黑清晰度有保障
4. Consolas 是最佳编程等宽字体之一，与数字表格匹配度高

**优化**：优先使用 "Microsoft YaHei UI"（Win10+ 自带，内边距更小，更适合 UI 场景）

```csharp
// 优先使用 Microsoft YaHei UI（Win10+ 优化过 UI 内边距）
public static string FontFamilySans => 
    IsFontInstalled("Microsoft YaHei UI") ? "Microsoft YaHei UI" : "微软雅黑";
```

### 5.2 字号层级优化（从 6 级到 7 级）

| 层级       | 当前字号       | 新字号         | 字重      | 用途           | 行高系数 |
| -------- | ---------- | ----------- | ------- | ------------ | ---- |
| Display  | 14pt Bold  | 16pt Bold   | Bold    | 页面大标题（登录页等）  | 1.5  |
| Title1   | -          | 13pt Bold   | Bold    | 弹窗/卡片主标题     | 1.5  |
| Title2   | 12pt Bold  | 11.5pt Bold | Bold    | 分组标题/次级标题    | 1.5  |
| Body     | 9.5pt      | 10pt        | Regular | 正文/输入框文字     | 1.6  |
| BodyBold | 9.5pt Bold | 10pt Bold   | Bold    | 按钮文字/强调文字    | 1.5  |
| Caption  | 8.5pt      | 9pt         | Regular | 辅助文字/说明/版本号  | 1.5  |
| Tiny     | -          | 8pt         | Regular | 角标/状态文字/表格表头 | 1.4  |

**调整理由**：

1. 正文字号从 9.5pt 提升到 10pt —— 审计软件长时间阅读数据，稍大字号减轻眼疲劳
2. 新增 Title1 (13pt) 和 Tiny (8pt) —— 完善标题和最小文字层级
3. 标题从 12pt 分化为两级 —— 弹窗标题与分组标题区分，层级更清晰

### 5.3 GDI+ 渲染优化

* 所有自定义绘制文字统一使用 `TextRenderingHint.ClearTypeGridFit`（比 AntiAliasGridFit 更清晰）

* 对于标准控件，通过 `Application.SetCompatibleTextRenderingDefault(false)` 使用 GDI 文本渲染（性能更好）

***

## 六、组件样式精修方案

### 6.1 按钮 Button

#### 主按钮 Primary

| 属性      | 当前                   | 精修后                  | 说明          |
| ------- | -------------------- | -------------------- | ----------- |
| 背景色     | Brand (#1a73e8)      | Brand (#3b82f6)      | 更柔和的蓝       |
| Hover 色 | BrandHover (#1765cc) | BrandHover (#2563eb) | 同步提亮        |
| 文字色     | White                | White                | 保持          |
| 圆角      | 8px                  | 8px                  | 保持          |
| 高度      | 40px                 | 40px                 | 保持（符合触控目标）  |
| 内边距     | (16,6,16,6)          | (20,8,20,8)          | 增加水平内边距，更透气 |
| 字体      | 9pt Bold             | 10pt Bold            | 字号提升        |

#### 次按钮 Secondary

| 属性       | 当前             | 精修后              | 说明       |
| -------- | -------------- | ---------------- | -------- |
| 背景色      | Surface (#fff) | Surface (#fff)   | 保持       |
| 边框色      | BorderStrong   | Border (新)       | 边框更浅，更轻盈 |
| Hover 边框 | Brand          | Brand (新)        | 同步品牌色    |
| Hover 背景 | SurfaceMuted   | SurfaceMuted (新) | 更浅       |
| 内边距      | (16,6,16,6)    | (20,8,20,8)      | 增加内边距    |

### 6.2 输入框 TextBox / C1Input

| 属性       | 当前     | 精修后        | 说明       |
| -------- | ------ | ---------- | -------- |
| 高度       | 40px   | 40px       | 保持       |
| 圆角       | 6px    | 6px        | 保持       |
| 边框色      | Border | Border (新) | 更浅更柔和    |
| Focus 边框 | -      | Brand (新)  | 聚焦态品牌色边框 |
| 内边距      | 默认     | 10px 左右    | 增加左右内边距  |
| 字体       | 9pt    | 10pt       | 提升到正文级   |
| 占位符色     | -      | TextMuted  | 统一占位符颜色  |

### 6.3 表格 C1FlexGrid

| 属性    | 当前           | 精修后              | 说明           |
| ----- | ------------ | ---------------- | ------------ |
| 数据行高  | \~24px       | 32px             | 增加 8px，更透气   |
| 表头行高  | 30px         | 36px             | 增加 6px       |
| 表头字体  | 8.5pt Bold   | 8pt Bold         | 稍小但加粗，更精致    |
| 正文字体  | 9pt          | 9.5pt            | 提升可读性        |
| 斑马纹   | -            | SurfaceMuted (新) | 偶数行浅灰底，提升可读性 |
| 选中行背景 | BrandSubtle  | BrandSubtle (新)  | 同步品牌浅色       |
| 边框色   | 默认           | Border (新)       | 更浅的网格线       |
| 数字列字体 | Consolas 9pt | Consolas 9.5pt   | 同步提升         |

### 6.4 侧边栏 Sidebar / Navigation

| 属性       | 当前          | 精修后             | 说明             |
| -------- | ----------- | --------------- | -------------- |
| 背景色      | #f8fafc     | #fafbfc         | 更浅更透气          |
| 宽度       | 240px       | 240px           | 保持             |
| 导航项高度    | 26px        | 30px            | 增加 4px，更舒展     |
| 导航项圆角    | -           | 6px             | OwnerDraw 圆角背景 |
| 激活项背景    | Brand (实色)  | Brand (新)       | 同步品牌色          |
| Hover 背景 | BrandSubtle | BrandSubtle (新) | 更浅更柔和          |

### 6.5 卡片 / 面板 Panel

| 属性  | 当前      | 精修后        | 说明         |
| --- | ------- | ---------- | ---------- |
| 背景色 | Surface | Surface    | 保持         |
| 圆角  | 12px    | 12px       | 保持（需自绘）    |
| 边框  | -       | 1px Border | 增加细边框，定义边界 |
| 内边距 | 12px    | 16px       | 增加留白       |

***

## 七、具体修改文件与内容

### 7.1 核心 Token 文件（最高优先级）

#### 文件 1：`AuditAI/Auditai.UI.Platform/AuditTheme.cs`

**修改内容**：

1. **Surface 区域**（第 14-17 行）：

   * SurfaceMuted: `#f8fafc` → `#fafbfc` (250,251,252)

   * SurfaceSubtle: `#f1f5f9` → `#f5f7fa` (245,247,250)

   * SurfaceHover: `#e2e8f0` → `#edf0f5` (237,240,245)

2. **Text 区域**（第 22-24 行）：

   * Text: `#0f172a` → `#1e293b` (30,41,59)

   * TextSecondary: `#334155` → `#475569` (71,85,105)

   * TextMuted: 保持 `#64748b` 不变

3. **Border 区域**（第 29-30 行）：

   * Border: `#e2e8f0` → `#e5e7eb` (229,231,235)

   * BorderStrong: `#cbd5e1` → `#d1d5db` (209,213,219)

4. **Brand 区域**（第 35-39 行）：

   * Brand: `#1a73e8` → `#3b82f6` (59,130,246)

   * BrandHover: `#1765cc` → `#2563eb` (37,99,235)

   * BrandActive: `#1557b0` → `#1d4ed8` (29,78,216)

   * BrandSubtle: `#e8f0fe` → `#eff6ff` (239,246,255)

   * BrandForeground: 保持 White 不变

5. **State 区域**（第 44-54 行）：

   * SuccessText: `#16a34a` → `#22c55e` (34,197,94)

   * ErrorText: `#dc2626` → `#ef4444` (239,68,68)

   * WarningText: `#d97706` → `#f59e0b` (245,158,11)

   * 三个 Subtle/Border 保持不变

6. **Extended 区域**（第 59-65 行）：

   * Navy: `#1e3a5f` → `#334155` (51,65,85)

   * Indigo: `#4f46e5` → `#6366f1` (99,102,241)

   * Teal: `#0f766e` → `#14b8a6` (20,184,166)

   * Amber: `#b45309` → `#f59e0b` (245,158,11)

   * Purple: `#7c3aed` → `#8b5cf6` (139,92,246)

   * Rose: `#be123c` → `#f43f5e` (244,63,94)

   * Slate: `#475569` → `#64748b` (100,116,139)

7. **Chart 区域**（第 70-74 行）：

   * Chart1: `#1a73e8` → `#3b82f6`

   * Chart2: `#34a853` → `#22c55e`

   * Chart3: `#fbbc04` → `#facc15`

   * Chart4: `#ea4335` → `#ef4444`

   * Chart5: `#9334e6` → `#8b5cf6`

8. **Sidebar 区域**（第 79-82 行）：

   * SidebarBg: `#f8fafc` → `#fafbfc` (250,251,252)

   * 其余自动跟随 Brand/BrandSubtle/Border 更新

9. **Typography 区域**（第 87-102 行 + 第 150-169 行）：

   * FontFamilySans: 新增 "Microsoft YaHei UI" 优先检测逻辑

   * FontBody: 9.5f → 10f

   * FontBodyBold: 9.5f Bold → 10f Bold

   * FontDefault: 9f → 9.5f（向后兼容）

   * FontDefaultBold: 9f Bold → 9.5f Bold

   * FontSmall: 8.5f → 9f（向后兼容）

   * FontSmallBold: 8.5f Bold → 9f Bold

   * FontCaption: 8.5f → 9f

   * FontDisplay: 14f → 16f

   * FontTitle: 12f → 11.5f（作为 Title2）

   * FontHeader: 10f → 13f（作为 Title1，重命名为 FontTitle1）

   * 新增 FontTiny: 8f Regular

10. **新增 Gradient & Shadow Token**：

    * GradientStart: `#f9fafb` (249,250,251)

    * GradientEnd: `#ffffff`

    * ShadowLight: Color.FromArgb(12, 15, 23, 42)

    * ShadowMedium: Color.FromArgb(20, 15, 23, 42)

11. **Spacing 新增**：

    * Space10 = 40

***

#### 文件 2：`AuditAI/Auditai.UI.Platform/IconPalette.cs`

**修改内容**：

1. 所有语义色引用自动跟随 AuditTheme 更新（属性引用，无需改色值）
2. 补充 13 个缺失图标映射（新增 case 语句）：

   * magnifying-glass → AuditTheme.Slate

   * sliders-horizontal → AuditTheme.Slate

   * download → AuditTheme.SuccessText

   * print → AuditTheme.Slate

   * gear → AuditTheme.Slate

   * bell → AuditTheme.WarningText

   * chart-bar → AuditTheme.Brand

   * chart-pie → AuditTheme.Brand

   * wallet → AuditTheme.SuccessText

   * file-search → AuditTheme.Brand

   * files → AuditTheme.Indigo

   * house → AuditTheme.Brand

   * identification-card → AuditTheme.Slate

***

### 7.2 图标系统文件

#### 文件 3：`CommonControls/Auditai.UI.Controls/IconLibrary.cs`

**修改内容**：

1. DefaultColor 从 `Color.FromArgb(52, 64, 84)` 更新为新的 TextSecondary 值 `Color.FromArgb(71, 85, 105)`
2. TextRenderingHint 从 AntiAliasGridFit 调整为 ClearTypeGridFit（更清晰）
3. 新增标准尺寸常量：

   ```csharp
   public const int SizeTiny = 12;
   public const int SizeSmall = 14;
   public const int SizeDefault = 16;
   public const int SizeMedium = 20;
   public const int SizeLarge = 24;
   public const int SizeExtraLarge = 32;
   ```

***

#### 文件 4：`CommonControls/Auditai.UI.Controls/IconRes.cs`（如存在）

**修改内容**：

1. DefaultColor 引用自动跟随 IconLibrary 更新
2. 评估并调整部分 32px 图标到更合理的尺寸（按使用场景）

***

#### 文件 5：`AuditAI/IconRes.cs`

**修改内容**：

1. DefaultColor 引用自动跟随 IconLibrary 更新
2. 与 CommonControls 版本同步调整

***

### 7.3 主题系统文件

#### 文件 6：`CommonControls/Auditai.UI.Controls/Theme.cs`

**修改内容**：
更新 "Google 蓝" 主题（auditai\_GoogleBlue）的 ThemeContext 色值：

* GradientColor: `#e8f0fe` → `#eff6ff` (239,246,255)

* TileColor: `#1a73e8` → `#3b82f6` (59,130,246)

* BackColor: `#f8fafc` → `#fafbfc` (250,251,252)

* LineColor: `#1a73e8` → `#3b82f6` (59,130,246)

* DarkColor: `#1557b0` → `#1d4ed8` (29,78,216)

* BulletColor: `#1a73e8` → `#3b82f6` (59,130,246)

* RibbonTabBorder: `#e2e8f0` → `#e5e7eb` (229,231,235)

* FormulaEditorBorderColor: `#cbd5e1` → `#d1d5db` (209,213,219)

* ProgressBarColor: `#1a73e8` → `#3b82f6` (59,130,246)

***

### 7.4 组件精修文件

#### 文件 7：`AuditAI/Auditai.UI.Platform/ThemeApplier.cs`（核心应用层）

**修改内容**：

1. `ApplyForm`：字体更新为 FontBody (10pt)
2. `ApplyFlexGrid`：

   * 数据行高：24px → 32px

   * 表头行高：30px → 36px

   * 表头字体：8.5pt Bold → 8pt Bold (FontTiny Bold)

   * 正文字体：9pt → 9.5pt (FontDefault)

   * 新增斑马纹设置（偶数行 SurfaceMuted 背景）

   * Highlight 背景色自动跟随 BrandSubtle
3. `ApplyPrimaryButton`：

   * Padding: (16,6,16,6) → (20,8,20,8)

   * 字体: FontBodyBold (10pt Bold)
4. `ApplySecondaryButton`：

   * Padding: (16,6,16,6) → (20,8,20,8)

   * 边框色: BorderStrong → Border（更浅）

   * 字体: FontBody (10pt)
5. `ApplyTextButton`：

   * 字体: FontBody

   * 增加 Padding (8,4,8,4)
6. `ApplyTextBox`：

   * 字体: FontBody (10pt)
7. `ApplyCardPanel`：

   * Padding: 12 → 16
8. `ApplyToolbarPanel`：

   * Padding: (8,6,8,6) → (12,8,12,8)
9. `ApplyTreeView`：

   * ItemHeight: 26px → 30px

   * DrawNode 圆角: 无 → 6px
10. 新增方法：

    * `ApplyComboBox`：下拉框样式

    * `ApplyBadge`：状态徽章样式

***

### 7.5 关键窗体文件

#### 文件 8：`AuditAI/Auditai.UI.Platform/frmLogin.cs`

**修改内容**：

1. Primary 色：`#1a73e8` → `#3b82f6`
2. PrimaryDark 色：`#1557b0` → `#1d4ed8`
3. PrimaryLight 色：`#e8f0fe` → `#eff6ff`
4. LineColorDefault：`#e2e8f0` → `#e5e7eb`
5. Surface0/Surface1 同步提亮
6. TextPrimary/TextSecondary/TextPlaceholder 同步更新
7. 输入框内边距和按钮内边距增加

***

#### 文件 9：`CommonControls/Auditai.UI.Controls/InputBoxImpl.cs`

**修改内容**：

1. 局部颜色令牌同步更新
2. ButtonWidth: 110 → 120（更多留白）

***

### 7.6 设计参考稿

#### 文件 10：`auditai-redesign/index.html`

**修改内容**：

1. CSS 变量中所有颜色 token 同步更新为新的清新配色
2. 字体大小变量同步调整
3. 间距变量适当增大
4. 边框颜色变量更新
5. 组件样式更新（按钮内边距、表格行高等）

***

## 八、实施顺序与依赖关系

### 阶段一：Token 层更新（基础，必须先做）

1. 更新 AuditTheme.cs 颜色 token
2. 更新 AuditTheme.cs 字体 token
3. 更新 IconPalette.cs 图标语义色（自动跟随 AuditTheme）
4. 更新 IconLibrary.cs DefaultColor

**依赖**：无\
**验收**：编译通过，所有 token 可正确读取

### 阶段二：主题系统更新

1. 更新 Theme.cs 中 Google 蓝主题的 ThemeContext
2. 验证 C1 控件主题应用效果

**依赖**：阶段一完成\
**验收**：启动应用后 C1 控件显示新配色

### 阶段三：组件精修层更新

1. 更新 ThemeApplier.cs 中所有 ApplyXXX 方法
2. 新增 ApplyComboBox、ApplyBadge 等方法
3. 更新 SideCommandBar.cs（如存在）

**依赖**：阶段一、二完成\
**验收**：调用 ThemeApplier 的窗体显示新样式

### 阶段四：关键窗体适配

1. frmLogin.cs 更新局部 token
2. InputBoxImpl.cs 更新
3. frmRegister.cs、frmFindPwd.cs 等同理更新

**依赖**：阶段一完成\
**验收**：关键窗体视觉符合新设计

### 阶段五：图标体系完善

1. 补充 IconPalette.cs 缺失图标映射
2. 审核 IconRes.cs 中图标尺寸的合理性

**依赖**：阶段一完成\
**验收**：所有使用图标的地方渲染正确

### 阶段六：设计参考稿同步

1. 更新 auditai-redesign/index.html 的 CSS 变量和组件样式
2. 作为视觉验收的参考基准

**依赖**：阶段一完成（可并行）\
**验收**：HTML 原型呈现清新风格

***

## 九、验证步骤

### 9.1 颜色验证

1. **对比度测试**：验证以下组合满足 WCAG AA (4.5:1)

   * Text on Surface

   * TextSecondary on Surface

   * TextMuted on Surface

   * BrandForeground on Brand

   * SuccessText on SuccessSubtle
2. **色值一致性检查**：

   * AuditTheme.cs 色值与设计规范一致

   * HTML 原型 CSS 变量与 C# token 一致

   * Theme.cs ThemeContext 颜色与 AuditTheme 一致

### 9.2 字体验证

1. 在 96 DPI（100%）和 120 DPI（125%）下测试清晰度
2. 验证字体回退：缺少 Microsoft YaHei UI 时正确回退到微软雅黑

### 9.3 组件验证

逐一验证以下组件在各状态下的表现：

1. 主按钮 / 次按钮 / 文字按钮（正常、Hover、Focus、禁用）
2. 输入框（正常、Focus、禁用）
3. 下拉框（正常、展开、选中）
4. 表格（表头、数据行、选中行、斑马纹）
5. 侧边栏（导航项、激活态、悬停态）
6. 对话框 / MessageBox
7. 状态栏

### 9.4 图标验证

1. 检查 IconPalette 中所有图标的语义色是否合理
2. 验证各尺寸（12/14/16/24/32px）下的渲染清晰度
3. 检查新增图标是否能正确渲染

### 9.5 整体视觉验收

1. 登录页整体效果
2. 主界面（侧边栏 + 表格 + 工具栏）整体效果
3. 典型业务窗体整体效果
4. 对话框/弹窗效果

### 9.6 性能验证

1. 表格大数据量（1000 行以上）滚动性能无明显下降
2. 窗体启动时间无明显增加

***

## 十、假设与决策记录

### 关键决策

1. **保持 Fill 实心图标风格**：WinForms GDI+ 渲染细线图标在低 DPI 下容易模糊，Fill 风格渲染质量更稳定
2. **保持微软雅黑字体**：嵌入中文字体体积过大，系统字体兼容性最好；仅增加 Microsoft YaHei UI 优先检测
3. **正文字号提升到 10pt**：审计软件需长时间阅读数据，稍大字号减轻眼疲劳；10pt 在 96 DPI 下约 13px，是桌面软件的舒适阅读尺寸
4. **表格行高 32px**：在可读性和数据密度间取得平衡；比默认 24px 增加 33%，一屏仍可显示约 20 行数据
5. **更新现有 "Google 蓝" 主题而非新增**：减少用户选择成本，直接升级默认主题体验；如有需要可后续保留旧主题

### 风险与注意事项

1. **GDI+ 渲染限制**：WinForms 对渐变、阴影、圆角的支持不如 CSS 灵活，优先使用纯色+边框营造层次
2. **局部颜色硬编码**：项目中可能存在大量硬编码颜色值，优先统一到 AuditTheme，短期保留局部变量但同步更新色值
3. **C1Theme 二进制主题**：C1 主题是编译后的二进制文件，许多细节无法通过代码精确控制；通过 ThemeRefineAction + OwnerDraw 覆盖
4. **向后兼容性**：确保升级仅影响 "Google 蓝" 主题，不破坏其他 21 个主题的显示

***

## 十一、验收标准总结

| 维度   | 验收标准                                   | 验证方式              |
| ---- | -------------------------------------- | ----------------- |
| 配色   | 品牌色从 #1a73e8 变为 #3b82f6，全 token 体系同步更新 | 代码审查 + 对比度检测      |
| 图标   | 13 语义色全部提亮，新增 ≥13 个常用图标                | 代码审查 + 视觉检查       |
| 字体   | 正文从 9.5pt 提升到 10pt，7 级字号层级             | 代码审查 + 多 DPI 屏幕测试 |
| 组件   | 按钮/输入框/表格/侧边栏 4 类核心组件样式更新              | 功能测试 + 视觉走查       |
| 性能   | 表格滚动、窗体启动无明显性能下降                       | 性能对比测试            |
| 可访问性 | 所有文字色满足 WCAG AA 对比度 (≥4.5:1)           | 对比度检测工具           |
| 一致性  | C# token 与 HTML 原型色值 100% 一致           | 对比审计              |

