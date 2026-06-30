# AuditAI MCP Server 使用说明

## 概述

AuditAI MCP Server 将审计系统的核心功能暴露为 MCP (Model Context Protocol) 工具，允许 AI 客户端（如 Claude Desktop、Cursor、VS Code Copilot）直接调用审计功能，实现审计全流程自动化。

## 支持的 MCP 客户端

- Claude Desktop
- Cursor
- VS Code (with GitHub Copilot)
- 任何支持 MCP 协议的 AI 客户端

## 配置方法

### Claude Desktop

1. 打开 Claude Desktop 配置文件位置：
   - Windows: `%APPDATA%\Claude\claude_desktop_config.json`
   - macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`

2. 将 `Docs/claude_desktop_config.json` 的内容复制到配置文件中

3. 重启 Claude Desktop

4. 在对话中即可使用审计工具（如"列出所有审计项目"）

### VS Code / Cursor

1. 在项目根目录创建 `.vscode/mcp.json` 文件

2. 将 `Docs/mcp.json` 的内容复制到文件中

3. 重启 VS Code / Cursor

## 可用工具列表

### 项目管理（6个工具）
- `list_projects` — 列举所有本地审计项目
- `create_project` — 创建新审计项目
- `open_project` — 打开指定项目
- `save_project` — 保存当前项目
- `close_project` — 关闭当前项目
- `get_project_info` — 获取项目信息

### 导航树（8个工具）
- `get_project_tree` — 获取项目导航树
- `create_directory_node` — 创建目录节点
- `create_document_node` — 创建文档节点
- `create_table_node` — 创建表格节点
- `delete_node` — 删除节点
- `move_node` — 移动节点
- `copy_node` — 复制节点
- `rename_node` — 重命名节点

### 文档操作（4个工具）
- `get_document_content` — 获取文档内容
- `set_document_content` — 设置文档内容
- `add_paragraph` — 添加段落
- `export_document` — 导出文档（Word/PDF）

### 表格操作（10个工具）
- `get_table_data` — 获取表格数据
- `get_cell_value` — 获取单元格值
- `set_cell_value` — 设置单元格值
- `add_table_row` — 添加行
- `add_table_column` — 添加列
- `delete_table_row` — 删除行
- `delete_table_column` — 删除列
- `merge_cells` — 合并单元格
- `set_cell_style` — 设置单元格样式
- `export_table` — 导出表格为 Excel

### 公式计算（6个工具）
- `set_cell_formula` — 设置单元格公式
- `get_cell_formula` — 获取单元格公式
- `evaluate_formula` — 求值公式
- `calculate_table` — 计算整个表格
- `calculate_all_tables` — 计算所有表格
- `get_formula_dependencies` — 获取公式依赖

### 账簿查询（7个工具）
- `get_ledger_accounts` — 获取科目列表
- `get_account_balance` — 获取科目余额
- `get_trial_balance` — 获取试算平衡表
- `get_vouchers` — 获取凭证列表
- `get_subsidiary_ledger` — 获取明细账
- `get_general_ledger` — 获取总账
- `import_ledger` — 导入账簿

### 数据采集（3个工具）
- `list_supported_databases` — 列举支持的数据库类型
- `collect_data` — 采集财务数据
- `get_collection_status` — 查询采集进度

### 数据校验（4个工具）
- `validate_document` — 校验文档
- `validate_table` — 校验表格
- `validate_all_tables` — 校验所有表格
- `get_validation_results` — 获取校验结果

### 导出（5个工具）
- `export_to_excel` — 导出为 Excel
- `export_to_word` — 导出为 Word
- `export_to_pdf` — 导出为 PDF
- `export_to_image` — 导出为图片
- `batch_export` — 批量导出

### 跨项目操作（4个工具）
- `consolidate_projects` — 合并项目
- `set_cross_project_reference` — 设置跨项目引用
- `get_cross_project_reference` — 获取跨项目引用
- `evaluate_cross_project_formula` — 求值跨项目公式

### 审计工作流（3个工具）
- `create_audit_project` — 创建完整审计项目（一键创建）
- `generate_audit_report` — 生成审计报告
- `run_full_audit` — 端到端全自动审计

## 使用示例

### 示例1：查看现有项目
对 AI 说："列出所有审计项目"

### 示例2：创建新审计项目
对 AI 说："创建一个名为 ABC有限公司2025年度审计 的新项目"

### 示例3：查看账簿数据
对 AI 说："打开 ABC有限公司 项目，显示试算平衡表"

### 示例4：全自动审计
对 AI 说："对 ABC有限公司 进行2025年度审计，财务数据库是 SQL Server，连接字符串是 ..."

## 故障排除

### MCP Server 无法启动
1. 检查可执行文件路径是否正确
2. 确保工作目录（cwd）设置正确
3. 查看 Claude Desktop 的日志（Help > Toggle Developer Tools）

### 工具调用失败
1. 确保已先调用 `open_project` 打开项目
2. 检查项目文件路径是否正确
3. 查看 MCP Server 的 stderr 日志输出

## 技术细节

- 协议：MCP (Model Context Protocol) 2025-11-25
- 传输：JSON-RPC 2.0 over stdio
- 框架：.NET Framework 4.6.2
- 依赖：C1 Studio Enterprise 4.x、TX TextControl、System.Data.SQLite、Microsoft.AspNet.SignalR.Client、Google.Protobuf

## 云端全自动验证平台（97 个工具）

为支持云端服务端（`AuditApiServer`，106 个 HTTP 端点 + 1 个 SignalR Hub）的全自动验证与修复闭环，MCP Server 新增 7 个工具集共 97 个工具。所有工具独立注册，不影响现有 60+ 业务工具。

### 配置要求

在 `App.config` 中配置以下 appSettings：

```xml
<add key="ServerBaseUrl" value="http://82.156.108.218:8957" />
<add key="ProductionBaseUrl" value="https://api.auditai.top" />
<add key="TestUserName" value="admin" />
<add key="TestPassword" value="admin" />
<add key="SSHHost" value="82.156.108.218" />
<add key="SSHUser" value="root" />
<add key="SSHKeyPath" value="" /> <!-- 填写 SSH 私钥路径以启用运维工具 -->
<add key="SSHPort" value="22" />
<add key="ServerDbPath" value="/opt/auditapi/Data/auditai_server.db" />
<add key="ServerProjectPath" value="e:\lq\Server\AuditApiServer" />
<add key="ServerDeployPath" value="/opt/auditapi/" />
```

测试夹具位于 `TestFixtures/` 目录，包含：
- `seed_users.json` — 3 个测试用户凭证（admin/testuser1/testuser2）
- `seed_teams.json` — 2 个测试团队
- `seed_projects.json` — 2 个测试项目
- `sample_push_table.bin` — Protobuf 格式的 3 列 5 行样本表格
- `sample_push_document.bin` — Protobuf 格式的 3 段落样本文档

### CloudApiTools — 云端 API 调用工具集（58 个）

直接调用服务端 HTTP API，覆盖 8 大模块：

| 模块 | 工具数 | 示例工具 |
|------|--------|---------|
| 认证 | 10 | `cloud_login`、`cloud_sms_relogin`、`cloud_update_token`、`cloud_client_quit` |
| 用户/团队 | 10 | `cloud_create_team`、`cloud_add_user_to_team`、`cloud_invite_user` |
| 项目 | 11 | `cloud_get_projects`、`cloud_create_project`、`cloud_delete_project` |
| 表格/文档同步 | 9 | `cloud_push_table_quick`、`cloud_pull_table`、`cloud_revert_table` |
| 文件存储 | 6 | `cloud_upload_file`、`cloud_download_file`、`cloud_push_image` |
| 异步任务 | 4 | `cloud_generate_task_id`、`cloud_upload_task_input_file` |
| 数据字典 | 3 | `cloud_get_table_collect_dic`、`cloud_get_ledger_validate_dic` |
| 许可/配额 | 5 | `cloud_get_license`、`cloud_create_license`、`cloud_renew_license` |

每个工具接收参数、发起 HTTP 请求、捕获响应（状态码/头/体/耗时），存入 `SessionState.LastResponse` 供后续断言。登录类工具成功后自动保存 Token 到会话。

### AssertionTools — 断言工具集（8 个）

对上一步 CloudApi 调用的响应做结构化校验：

- `assert_status` — 断言 HTTP 状态码
- `assert_json_path` — 用 JPath 提取并断言（支持 eq/ne/contains/not_empty/exists）
- `assert_error_code` — 断言 error 字段
- `assert_header` — 断言响应头
- `assert_response_time` — 断言耗时
- `assert_body_contains` — 断言响应体包含子串
- `assert_body_empty` / `assert_body_not_empty`

所有断言结果自动记录到 `SessionState.AssertionResults`，供报告生成。

### SignalRTestTools — 实时协作测试工具集（5 个）

- `connect_hub` — 连接 ChatHub，QueryString 传 userId + token
- `hub_login` — 调用 Hub Login 方法加入 Group
- `hub_send_peer_event` — 调用 UpLoadTableCellId 等方法触发 Peer 回调
- `wait_peer_callback` — 等待指定回调名（如 PeerTableCellChange），带超时
- `disconnect_hub` — 关闭连接

支持多会话隔离，可模拟双用户协作场景。

### ScenarioTools — 场景编排工具集（11 个）

封装典型业务流程为单次调用，内部串接 CloudApi + 断言，返回结构化测试报告：

| 工具 | 描述 |
|------|------|
| `run_login_flow` | 登录全流程（login → update_token → client_quit） |
| `run_user_team_flow` | 团队用户管理（create_team → add_user → set_permissions → dismiss_team） |
| `run_project_crud_flow` | 项目 CRUD（create → update → get → delete → restore） |
| `run_project_sync_flow` | 表格同步（push_table_quick → pull_table → revert_table） |
| `run_document_sync_flow` | 文档同步（push_document → pull_document） |
| `run_collaboration_flow` | 双会话 SignalR 协作 |
| `run_license_enforcement_flow` | 许可证强制执行（402/429 验证） |
| `run_quota_enforcement_flow` | 配额强制 |
| `run_concurrent_write_flow` | 并发写入冲突 |
| `run_file_upload_download_flow` | 文件上传下载 |
| `run_unauthorized_access_flow` | 越权访问（401/403 验证） |

返回格式：`{scenarioName, passed, durationMs, steps: [...], summary}`

### ServerOpsTools — 服务端运维工具集（6 个）

- `build_server` — 本地构建服务端（`dotnet build`）
- `publish_server` — 发布服务端（`dotnet publish -c Release -r linux-x64`）
- `deploy_to_production` — SCP 上传 + SSH 重启服务 + 健康检查
- `get_server_logs` — SSH 获取 systemd 日志（`journalctl -u auditapi`）
- `query_server_db` — SSH 执行 SQLite 查询
- `get_server_health` — HTTP 健康检查

支持 OpenSSH 和 PuTTY（pscp/plink）双兼容。需配置 `SSHKeyPath` 启用 SSH 相关工具。

### AutoFixTools — 自动修复闭环工具集（5 个）

- `read_finding` — 读取指定 ID 的安全问题详情（从 `cloud-features-audit-review/findings/summary.md`）
- `list_findings` — 列出所有问题清单（支持 severity/status 过滤）
- `apply_fix` — 应用修复闭环：记录 fixes.log → build_server → run_regression
- `run_regression` — 执行回归测试套件（critical=4 场景 / all=11 场景）
- `mark_finding_fixed` — 标记问题为已修复

通过反射调用 ScenarioTools 私有 impl 方法执行回归测试。

### TestReportTools — 测试报告工具集（4 个）

- `generate_test_report` — 生成 Markdown 测试报告，写入 `reports/` 目录
- `compare_with_baseline` — 与基线报告对比，识别新增/已修复/未变化失败
- `save_baseline` — 保存当前结果为新基线
- `list_known_failures` — 列出当前会话中所有失败的断言

报告路径：`e:\lq\.trae\specs\cloud-e2e-automation\reports\report-<timestamp>.md`

## 使用示例

### 示例 1：验证登录全流程

对 AI 说："调用 run_login_flow 验证登录"

或通过 JSON-RPC 直接调用：

```json
{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"run_login_flow","arguments":{}}}
```

### 示例 2：执行 critical 回归套件

对 AI 说："执行 critical 回归测试套件并生成报告"

工具调用顺序：
1. `run_regression`（suite: "critical"）
2. `generate_test_report`
3. `compare_with_baseline`（与上次基线对比）

### 示例 3：自动修复闭环

对 AI 说："读取问题 C-01 并应用修复"

工具调用顺序：
1. `read_finding`（findingId: "C-01"）
2. 修改服务端代码（通过 Edit 工具）
3. `apply_fix`（findingId: "C-01", fixDescription: "..."）
4. `mark_finding_fixed`（findingId: "C-01"）

### 示例 4：服务端运维

对 AI 说："构建服务端并部署到生产"

工具调用顺序：
1. `build_server`（configuration: "Release"）
2. `publish_server`
3. `deploy_to_production`
4. `get_server_health`（验证部署成功）
