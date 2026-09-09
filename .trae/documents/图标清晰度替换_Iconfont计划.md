# AuditAI 账务 UI · 图标清晰度替换实施计划

## 1. Summary

在保留 5 个页面结构与内联 `theme-vars` token 的前提下，把 `pages/` 下 92 处 `<i data-lucide="...">` 与 2 个编辑器页面 148 处内联 SVG 统一替换/归一为**从 iconfont.cn 挑选**的 24×24、2px stroke、`stroke="currentColor"`、`fill="none"` 的内联 SVG；移除 `lucide.min.js` CDN 依赖与运行时替换脚本；建立一套统一 `.icon` CSS 类以保证像素吸附与 `non-scaling-stroke` 清晰度；in-place 修改 5 个 HTML，随后更新 `runtime-orchestration-summary.json` / `runtime-dispatch-manifest.json`，最后跑 `validate-design-workspace.mjs` + `validate-finish-readiness.mjs --check=all` 通过。不生成 5 张对比页（Git 历史承担回溯）。

## 2. 模糊根因

Lucide 走 CDN + `lucide.createIcons()` 运行时替换，尺寸被 Tailwind 类驱动落到三组非整数格：

| 页面 | 实际像素 | 网格换算 |
|---|---|---|
| voucher-journal | 14 / 16 / 18px（`w-3.5` / `w-4` / `w-4.5`） | 24px 网格缩到 14px ≈ 0.583px/格，落半像素 |
| trial-balance | 18px（`w-[18px]`）、16px、14px | 18px 缩放 0.75，同样落半像素 |
| subsidiary-ledger | 16px / 14px（内联 style） | 14px 下 2px stroke 占位近 14%，抗锯齿后糊 |

叠加 CSS `svg.lucide { stroke-width:1.75 }` 把原生 2px 改成非整数 1.75，进一步破坏网格对齐。`geometricPrecision` + `non-scaling-stroke` 已启用但救不回"尺寸本身非整数落格"。

**必须同时做到**：
- 尺寸只吸附到 14 / 16 / 20 / 24；
- stroke-width 归一到 2；
- 内联 SVG 直接写死整数 `width` / `height`，不再走 `w-3.5`、`w-[18px]`。

## 3. 图标语义映射

### 3.1 侧栏导航（3 个账务页面共用）

| 现值 | iconfont.cn 搜索词 | 备注 |
|---|---|---|
| `scale`（品牌徽标 / 科目余额表） | `天平` | iconfont.cn 上 442 个候选，选 stroke-based 版本 |
| `receipt-text`（凭证序时簿） | `凭证` / `发票` | |
| `book-open-text`（明细账） | `账簿` / `打开的书` | |
| `book-marked`（总账） | `书签` / `总账` | |
| `layers`（辅助核算） | `层级` | |
| `timer`（账龄分析） | `沙漏` / `计时` | |
| `clipboard-check`（凭证抽查） | `审核` / `核对清单` | |
| `chart-column`（报表分析） | `柱状图` | |
| `settings`（设置） | `设置` / `齿轮` | |
| `shield-check`（顶栏信任标） | `盾牌 认证` | |

### 3.2 顶栏 / 通用操作 / 状态

| 现值 | iconfont.cn 搜索词 |
|---|---|
| `chevron-down` / `chevron-left` / `chevron-right` | `向下箭头` / `向左箭头` / `向右箭头` |
| `calendar` | `日历` |
| `search` | `搜索` / `放大镜` |
| `bell` | `铃铛` |
| `filter` | `筛选` / `漏斗` |
| `download` | `下载` |
| `printer` | `打印` |
| `file-spreadsheet` | `Excel` / `表格` |
| `check-square` / `check-circle` | `勾选` / `对勾` |
| `eye` | `眼睛` |
| `trash-2` | `删除` / `垃圾桶` |
| `plus` | `加号` |
| `clock` | `时钟` |
| `trending-up` | `趋势` / `上升` |

### 3.3 编辑器专用（spreadsheet-editor / document-editor，148 处内联 SVG）

**原则：不重画，只归一化**（stroke-width、class、尺寸）。仅语义不佳或带硬编码颜色的才替换。

| 语义 | 处理 |
|---|---|
| 撤销/重做、加粗、斜体、下划线、对齐、列表、缩进、链接、图片、表格、引用块、代码、清除格式、分隔线、保存、历史、全屏、折叠、关闭、目录、状态标记、节点图标 | 保留原路径，只加 `class="icon"` + 统一 stroke-width=2 + 尺寸归一 |
| 状态色硬编码 `stroke="#10b981"` / `#f59e0b` / `#6b7280` | 迁到 CSS var：`--audit-success` / `--audit-warning` / `--audit-text-tertiary` |
| 非 24 网格微图（`width="12" height="10"` 三角形等） | 替换为 iconfont.cn 的"箭头"/"矩形"图标，回到 24 网格 |

## 4. 执行步骤

### 步骤 A：抓取 SVG 源码（从 iconfont.cn）

**工具选择**：使用 solo-design 附带的浏览器 MCP（`trae-remote-official:browser`）打开 iconfont.cn 搜索页并逐个复制 SVG 源码。

**具体流程**：
1. 打开 `https://www.iconfont.cn/search/index?q=<关键词>&searchType=icon`（关键词来自第 3 节映射表）。
2. 搜索结果里**优先挑官方标签 + 线性 + 单一颜色**的图标（面性/彩色过滤掉）。
3. 点入图标详情页 → 左侧选"代码引入" → 选"HTML/SVG" 或 "SVG 代码" 标签 → 复制右侧 `<svg>…</svg>` 源码。
4. **手工规范化**（每个图标复制后立刻处理）：
   - 保留 `xmlns="http://www.w3.org/2000/svg"`；
   - 删除 `width` / `height`（改由 CSS `.icon` 类控制）；
   - 删除所有 `stroke="#xxxxxx"` / `fill="#xxxxxx"` 硬编码颜色，改为 `stroke="currentColor"` / `fill="none"`；
   - `stroke-width` 归一到 `2`；
   - 补 `stroke-linecap="round" stroke-linejoin="round"`；
   - 若 viewBox 非 `0 0 24 24`，允许保留原 viewBox 但外层必须给整数像素 `width/height`。
5. **效率优化**：一次浏览器会话里连续搜索、复制、粘贴，减少上下文切换。

**Fallback 策略（当 iconfont.cn 找不到合适语义时，按顺序）**：
1. Lucide 官方 `https://lucide.dev/icons/<name>` 的 Copy SVG（视觉与项目现有一致，最稳妥）；
2. Feather Icons `https://feathericons.com`（2 个编辑器页本身就是 Feather 风格）；
3. Font Awesome Free SVG（仅 Solid/Light 版本）。

**Fallback 触发条件**：iconfont.cn 上该语义的候选图标**全部**为面型、彩色、或笔画粗细明显 ≠2px 时，直接切 fallback。

**不做**：不使用 iconfont.cn 的字体/symbol 方案（`at.alicdn.com/t/...js`）——那会引入运行时依赖。

### 步骤 B：建立统一 `.icon` CSS 类

在 5 个 HTML 的现有 `<style>` 里**替换**（不是新增）针对 `svg.lucide` 和 `[data-icon]` 的规则，改为：

```css
/* Sharp icon rendering — 24x24 grid, integer pixel sizes */
.icon {
  display: inline-block;
  vertical-align: middle;
  width: 16px;
  height: 16px;
  flex-shrink: 0;
  color: currentColor;
}
svg.icon {
  shape-rendering: geometricPrecision;
  vector-effect: non-scaling-stroke;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
  fill: none;
}
.icon--sm { width: 14px; height: 14px; }
.icon--md { width: 20px; height: 20px; }
.icon--lg { width: 24px; height: 24px; }
.icon--inline { width: 1em; height: 1em; vertical-align: -0.125em; }
```

**关键决策**：
- 默认 16px（原 17/18px 是模糊元凶）；
- stroke-width 归一到 2（删除 1.75）；
- 保留 `geometricPrecision` + `non-scaling-stroke`；
- 删除 `[data-icon]` 的 mask 规则（原为图标字体预留）；
- 不再新增 `w-3.5` / `w-4.5` / `w-[18px]` 之类非整数类。

### 步骤 C：3 个账务页面（trial-balance / voucher-journal / subsidiary-ledger）

对每个页面：

1. 删除 `<head>` 中 `<script src="https://unpkg.com/lucide@1.8.0/dist/umd/lucide.min.js"></script>`。
2. 删除底部 `<script>lucide.createIcons();</script>`。
3. 用正则 `<i\s+data-lucide="([^"]+)"([^>]*)></i>` 找出所有 92 处，替换为：
   ```html
   <svg class="icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="..."/></svg>
   ```
4. 尺寸映射：
   - 原 `w-4 h-4` / `w-[18px] h-[18px]` → `class="icon"`（16px）；
   - 原 `w-3.5 h-3.5` → `class="icon icon--sm"`（14px，仅用于紧凑状态 badge）；
   - 原 `w-4.5 h-4.5` → `class="icon icon--md"`（20px）；
   - 原 `w-6 h-6` 或更大 → `class="icon icon--lg"`（24px）。
5. 原 `<i>` 上挂的颜色 `style`（如 `color:var(--audit-text-tertiary)`）迁到 `<svg>` 上；位置类 style（`position:absolute` 等）迁到父容器。
6. `shield-check`（品牌徽标 `text-white`）→ `<svg class="icon text-white" ...>`，颜色仍走 token。

### 步骤 D：2 个编辑器页面（spreadsheet-editor / document-editor）

148 处内联 SVG，**不重画**：

1. 全局替换 `stroke-width="2.5"` → `stroke-width="2"`；`stroke-width="3"` → `stroke-width="2"`（极少数 12px 微图保留原值，见下方例外）。
2. 给所有 `<svg viewBox="0 0 24 24" ...>` 加 `class="icon"`；已有自定义 class 的合并为 `class="icon node-arrow"` 之类。
3. 尺寸归一：
   - `width="12" height="12"` / `10` / `8` → 统一走 `class="icon icon--sm"`，去掉内联 `width`/`height`；
   - `width="14"` → 走 `class="icon"`；
   - `width="16"` / `20` / `24` → 保持并加对应 `--md`/`--lg` class。
4. 硬编码颜色归一：`stroke="#10b981"` → `stroke="var(--audit-success)"` 或迁到 `class="text-[var(--audit-success)]"`；同理 `#f59e0b` → `--audit-warning`，`#6b7280` → `--audit-text-tertiary`。
5. **例外保留**：`stroke-width="2.5"` / `"3"` 用在 12px 极小符号（如编辑器 12px 对勾/警告三角）可保留，但必须补 `class="icon icon--sm"` + `vector-effect="non-scaling-stroke"`。
6. 语义不佳的非 24 网格微图（如 `width="12" height="10"` 三角形）→ 用步骤 A 抓的 iconfont 图标替换。

### 步骤 E：更新 dispatch / summary

- `runtime-orchestration-summary.json` 的 `expectedDispatches[]` 追加 5 条记录（3 账务 + 2 编辑器），`changedFiles` 只列对应 HTML；`mainAgentPostDispatchMutations` 追加 `replaced-lucide-with-iconfont-inline-svg` 记录。
- `runtime-dispatch-manifest.json` 的 `deterministicCommandsByPage` 补齐 5 页（原只有 3 页）。
- **不动**：`.design` 文件的 5 个 page 节点、`colors_and_type.css`、`<style id="theme-vars">` 内联 token。

### 步骤 F：验证

```powershell
node c:\Users\PillarW\.trae-cn\builtin\design\default\skills\solo-design\shared-runtime\deterministic-tooling\validate-design-workspace.mjs e:\lq\AuditAI\auditai-ledger-ui --report-json=e:\lq\AuditAI\auditai-ledger-ui\validation-report.json
node c:\Users\PillarW\.trae-cn\builtin\design\default\skills\solo-design\shared-runtime\deterministic-tooling\validate-finish-readiness.mjs e:\lq\AuditAI\auditai-ledger-ui --check=all
```

## 5. 图标质量门禁

每张 SVG 必须同时满足：

1. `viewBox="0 0 24 24"`（若非 24 网格必须在复制时换算）；
2. `stroke="currentColor"`，无硬编码 `stroke="#xxxxxx"`；
3. `stroke-width="2"`，无 `1.75` / `2.5` / `3`（12px 微图例外见步骤 D.5）；
4. `stroke-linecap="round"` + `stroke-linejoin="round"`；
5. `fill="none"`（纯 fill 图标显式 `fill="currentColor"`）；
6. 尺寸只允许 14 / 16 / 20 / 24；
7. 无 `<script>`、`<foreignObject>`、`onload` 等事件属性；
8. 无 `xlink:href` 到外部 URL；
9. 无 `<image>`、`data:image/` 位图；
10. 单个 `<svg>` 字符数 ≤ 1500（防误拷整个 sprite）；
11. 静态源码里完整存在，**不依赖运行时脚本渲染**。

**执行 agent 自检**：每页完成后 Grep 验证：
- `stroke-width="(1\.5|1\.75|2\.5|3)"` 命中数（编辑器例外项之外应为 0）；
- `stroke="#[0-9a-fA-F]{6}"` 硬编码颜色命中数（除状态色白名单外应为 0）；
- `width="\d+"` 非整数命中数（应为 0）。

## 6. 风险与回退

| 风险 | 概率 | 回退 |
|---|---|---|
| iconfont.cn 找不到合适账务语义图标 | 中 | 切 Lucide 官方 SVG fallback（视觉一致） |
| 图标风格不统一 | 高 | 挑 stroke-only、笔画均匀、无装饰的那一个；不行切 Lucide |
| 浏览器 MCP 抓取受限于登录/CAPTCHA | 中 | 用 `browser_waiting_for_user_interaction` 让手动介入一次；或走 Lucide 官方 fallback |
| 移除 CDN 后离线预览失败 | 低 | 图标改为纯内联，不依赖网络 |
| DOM 体积增大 | 低 | 20 图标 ≈ 4KB，可忽略；不复用 `<defs>` 避免 ID 冲突 |
| iconfont 授权问题 | 低 | iconfont.cn 免费图标基本可商用；有 CC BY-NC 标记的换 Lucide |
| `.design` 校验失败 | 低 | `.design` 不校验页面内 SVG；失败先读 `validation-report.json` |
| 用户想看前后对比 | 中 | 不生成 5 张对比页；Git 追溯即可 |
| 编辑器 SVG 有自定义 class | 中 | 合并 class，不破坏原 CSS |

## 7. 验证清单

**HTML 层 Grep（除白名单外应为 0）**：
- `data-lucide=` → 0 处
- `lucide\.createIcons|lucide@|unpkg.com/lucide` → 0 处
- `svg\.lucide\s*\{` → 0 处
- `stroke-width="(1\.75|1\.5)"` → 0 处
- `w-3\.5|w-4\.5|w-\[18px\]` → 0 处
- `stroke="#[0-9a-fA-F]{6}"` 硬编码颜色 → 0 处（编辑器状态色白名单除外）

**视觉层（浏览器打开 5 个页面）**：
- 100% / 125% / 150% 三档缩放下图标边缘锐利；
- 侧栏 10 个导航图标风格一致（同线性、同粗、同尺寸）；
- 表头图标与文字基线对齐；
- 状态 badge 内图标 14px 下依然可辨。

**结构层**：
- `.design` 5 个 page 节点完整（trial-balance / voucher-journal / subsidiary-ledger / spreadsheet-editor / document-editor）；
- `colors_and_type.css` `git diff` 为空；
- 每个 HTML 的 `<style id="theme-vars">` 内容未变；
- `runtime-orchestration-summary.json.pages[]` 5 项。

**脚本层**：
- `validate-design-workspace.mjs` exit code 0，`validation-report.json.errors` 为空；
- `validate-finish-readiness.mjs --check=all` 通过。

## 8. 明确不做

- 不写时间线 / 时长估算；
- 不重排页面结构 / 导航 / 表格布局；
- 不改 `colors_and_type.css`、不改 `<style id="theme-vars">`、不新增 CSS 变量；
- 不在计划里写具体 SVG 路径数据（执行阶段从 iconfont.cn 抓取）；
- 不生成 5 张对比页；
- 不引入 iconfont.cn 字体/symbol 方案；
- 不动 `.design` 文件页面节点、interaction、domId；
- 不改 sidebar width / topbar height 等几何约束。
