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
- 依赖：C1 Studio Enterprise 4.x、TX TextControl、System.Data.SQLite
