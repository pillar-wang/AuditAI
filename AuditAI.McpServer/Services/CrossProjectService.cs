﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Auditai.DTO;
using Auditai.LocalDataStore;
using Auditai.Model;
using AuditAI.McpServer.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Table = Auditai.Model.Table;
using Row = Auditai.Model.Row;
using Column = Auditai.Model.Column;
using Cell = Auditai.Model.Cell;
using Project = Auditai.Model.Project;

namespace AuditAI.McpServer.Services
{
    /// <summary>
    /// 跨项目服务
    /// 封装项目合并、跨项目数据引用管理及跨项目公式求值等操作
    /// </summary>
    public static class CrossProjectService
    {
        // =============================================
        // 项目合并
        // =============================================

        /// <summary>
        /// 合并多个项目，生成合并工作底稿
        /// </summary>
        /// <param name="projectIds">要合并的项目 ID 列表（Guid 字符串）</param>
        /// <param name="mergeMode">合并模式："group_summary"（分组汇总）或 "row_append"（行追加）</param>
        /// <returns>JSON 格式的合并结果</returns>
        public static string ConsolidateProjects(JArray projectIds, string mergeMode)
        {
            try
            {
                if (projectIds == null || projectIds.Count == 0)
                {
                    return ErrorJson("项目 ID 列表不能为空");
                }

                // 解析合并模式
                MergeMode mode;
                string modeDesc;
                if (string.Equals(mergeMode, "group_summary", StringComparison.OrdinalIgnoreCase))
                {
                    mode = MergeMode.Aggregate;
                    modeDesc = "分组汇总";
                }
                else if (string.Equals(mergeMode, "row_append", StringComparison.OrdinalIgnoreCase))
                {
                    mode = MergeMode.Append;
                    modeDesc = "行追加";
                }
                else
                {
                    return ErrorJson($"不支持的合并模式: {mergeMode}，支持: group_summary（分组汇总）、row_append（行追加）");
                }

                // 收集各项目数据
                var projectSummaries = new JArray();
                var aggregatedTables = new Dictionary<string, AggregateTableInfo>(StringComparer.OrdinalIgnoreCase);
                var appendedRows = new JArray();
                int successCount = 0;
                int failCount = 0;
                int totalTableCount = 0;
                int totalRowCount = 0;

                foreach (var pidToken in projectIds)
                {
                    string pidStr = pidToken?.ToString();
                    if (string.IsNullOrWhiteSpace(pidStr))
                    {
                        failCount++;
                        continue;
                    }

                    if (!Guid.TryParse(pidStr, out Guid projectId))
                    {
                        projectSummaries.Add(new JObject
                        {
                            ["project_id"] = pidStr,
                            ["status"] = "failed",
                            ["error"] = "项目 ID 格式无效（需要 Guid 字符串）"
                        });
                        failCount++;
                        continue;
                    }

                    // 打开外部项目
                    Project externalProject = OpenExternalProject(projectId);
                    if (externalProject == null)
                    {
                        projectSummaries.Add(new JObject
                        {
                            ["project_id"] = projectId.ToString(),
                            ["status"] = "failed",
                            ["error"] = "项目数据库不存在或无法打开"
                        });
                        failCount++;
                        continue;
                    }

                    // 收集项目表格数据
                    var tableNodes = externalProject.GetAllTableNodes().ToList();
                    int projectRowCount = 0;
                    var tablesArr = new JArray();

                    foreach (var tableNode in tableNodes)
                    {
                        Table table = null;
                        try
                        {
                            table = tableNode.Table;
                            if (table == null) continue;
                            table.LoadAndReturn(true);
                        }
                        catch
                        {
                            continue;
                        }

                        string tableName = tableNode.Name ?? table.Title?.TitleCell?.Value?.ToString() ?? $"表格_{table.Id.Value}";
                        int rowCount = table.Rows?.Count ?? 0;
                        int colCount = table.Columns?.Count ?? 0;
                        projectRowCount += rowCount;
                        totalRowCount += rowCount;
                        totalTableCount++;

                        // 收集数值列汇总数据（用于 group_summary 模式）
                        var numericSummary = CollectNumericSummary(table);

                        var tableInfo = new JObject
                        {
                            ["table_node_id"] = tableNode.Id.Value,
                            ["table_id"] = table.Id.Value.ToString(),
                            ["name"] = tableName,
                            ["row_count"] = rowCount,
                            ["col_count"] = colCount,
                            ["numeric_columns"] = numericSummary
                        };
                        tablesArr.Add(tableInfo);

                        // 分组汇总模式：按表名聚合数值
                        if (mode == MergeMode.Aggregate)
                        {
                            AggregateTableInfo aggInfo;
                            if (!aggregatedTables.TryGetValue(tableName, out aggInfo))
                            {
                                aggInfo = new AggregateTableInfo { Name = tableName, SourceProjects = new List<string>() };
                                aggregatedTables[tableName] = aggInfo;
                            }
                            aggInfo.SourceProjects.Add(externalProject.Name ?? projectId.ToString());
                            aggInfo.RowCount += rowCount;
                            MergeNumericSummary(aggInfo.NumericColumns, numericSummary);
                        }
                        else
                        {
                            // 行追加模式：收集每行数据
                            AppendRows(appendedRows, externalProject.Name, tableName, table);
                        }
                    }

                    projectSummaries.Add(new JObject
                    {
                        ["project_id"] = projectId.ToString(),
                        ["project_name"] = externalProject.Name,
                        ["status"] = "success",
                        ["table_count"] = tableNodes.Count,
                        ["row_count"] = projectRowCount,
                        ["tables"] = tablesArr
                    });
                    successCount++;
                }

                // 构建合并结果
                var result = new JObject
                {
                    ["success"] = true,
                    ["merge_mode"] = mergeMode,
                    ["merge_mode_desc"] = modeDesc,
                    ["total_projects"] = projectIds.Count,
                    ["success_count"] = successCount,
                    ["failed_count"] = failCount,
                    ["total_tables"] = totalTableCount,
                    ["total_rows"] = totalRowCount,
                    ["projects"] = projectSummaries
                };

                if (mode == MergeMode.Aggregate)
                {
                    // 分组汇总：输出按表名聚合的结果
                    var consolidatedArr = new JArray();
                    foreach (var kv in aggregatedTables)
                    {
                        var agg = kv.Value;
                        var numericCols = new JObject();
                        foreach (var nk in agg.NumericColumns)
                        {
                            numericCols[nk.Key] = nk.Value;
                        }
                        consolidatedArr.Add(new JObject
                        {
                            ["table_name"] = agg.Name,
                            ["source_project_count"] = agg.SourceProjects.Count,
                            ["source_projects"] = new JArray(agg.SourceProjects.Distinct()),
                            ["total_row_count"] = agg.RowCount,
                            ["aggregated_values"] = numericCols
                        });
                    }
                    result["consolidated_tables"] = consolidatedArr;
                    result["message"] = $"合并完成（分组汇总模式）：共合并 {successCount} 个项目，{aggregatedTables.Count} 个去重表格";
                }
                else
                {
                    // 行追加：输出所有行
                    result["appended_rows"] = appendedRows;
                    result["appended_row_count"] = appendedRows.Count;
                    result["message"] = $"合并完成（行追加模式）：共合并 {successCount} 个项目，追加 {appendedRows.Count} 行数据";
                }

                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("合并项目失败: " + ex.Message);
            }
        }

        // =============================================
        // 跨项目引用管理
        // =============================================

        /// <summary>
        /// 设置跨项目数据引用关系
        /// 将引用关系持久化到当前项目的 CrossProjectDataRef 表
        /// </summary>
        /// <param name="sourceProjectId">来源项目 ID（Guid 字符串）</param>
        /// <param name="targetProjectId">目标项目 ID（Guid 字符串，应为当前打开的项目）</param>
        /// <param name="formula">引用公式表达式</param>
        /// <returns>JSON 格式的设置结果</returns>
        public static string SetCrossProjectReference(string sourceProjectId, string targetProjectId, string formula)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sourceProjectId))
                {
                    return ErrorJson("来源项目 ID 不能为空");
                }
                if (string.IsNullOrWhiteSpace(targetProjectId))
                {
                    return ErrorJson("目标项目 ID 不能为空");
                }
                if (!Guid.TryParse(sourceProjectId, out Guid sourceId))
                {
                    return ErrorJson("来源项目 ID 格式无效（需要 Guid 字符串）");
                }
                if (!Guid.TryParse(targetProjectId, out Guid targetId))
                {
                    return ErrorJson("目标项目 ID 格式无效（需要 Guid 字符串）");
                }

                // 确保当前项目已打开
                SessionState.Current.EnsureProject();
                var currentProject = SessionState.Current.CurrentProject;

                // 校验目标项目是否为当前项目
                if (currentProject.Id != targetId)
                {
                    return ErrorJson($"目标项目 {targetId} 不是当前打开的项目 {currentProject.Id}，请先打开目标项目");
                }

                // 验证来源项目是否存在
                string sourceDbPath = GetExternalDbPath(sourceId);
                if (!File.Exists(sourceDbPath))
                {
                    return ErrorJson($"来源项目数据库不存在: {sourceDbPath}");
                }

                // 获取来源项目名称
                string sourceProjectName = TryGetProjectName(sourceId) ?? sourceId.ToString();

                // 创建跨项目数据引用配置
                var dataRef = new CrossProjectDataRef
                {
                    Id = currentProject.GetNextId(),
                    Name = $"跨项目引用_{sourceProjectName}_{DateTime.Now:yyyyMMddHHmmss}",
                    SourceProjectId = sourceId,
                    SourceProjectName = sourceProjectName,
                    SourceTableId = Id64.Zero,
                    TargetTableId = Id64.Zero,
                    TargetTableName = currentProject.Name,
                    RefMode = RefMode.FormulaCompute,
                    FormulaExpression = formula ?? string.Empty,
                    AutoRefresh = true,
                    Enabled = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CacheDurationSeconds = 60,
                    VersionStrategy = VersionStrategy.Latest
                };

                // 保存到当前项目的 CrossProjectDataRef 表
                var store = new CrossProjectDataRefStore(currentProject);
                store.Save(dataRef).GetAwaiter().GetResult();

                var result = new JObject
                {
                    ["success"] = true,
                    ["ref_id"] = dataRef.Id.Value.ToString(),
                    ["name"] = dataRef.Name,
                    ["source_project_id"] = sourceId.ToString(),
                    ["source_project_name"] = sourceProjectName,
                    ["target_project_id"] = targetId.ToString(),
                    ["target_project_name"] = currentProject.Name,
                    ["formula"] = dataRef.FormulaExpression,
                    ["ref_mode"] = dataRef.RefMode.ToString(),
                    ["auto_refresh"] = dataRef.AutoRefresh,
                    ["enabled"] = dataRef.Enabled,
                    ["message"] = "跨项目引用关系已设置并保存"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("设置跨项目引用失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取项目的所有跨项目引用关系
        /// </summary>
        /// <param name="projectId">项目 ID（Guid 字符串，应为当前打开的项目）</param>
        /// <returns>JSON 格式的引用列表</returns>
        public static string GetCrossProjectReference(string projectId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return ErrorJson("项目 ID 不能为空");
                }
                if (!Guid.TryParse(projectId, out Guid pid))
                {
                    return ErrorJson("项目 ID 格式无效（需要 Guid 字符串）");
                }

                // 确保当前项目已打开
                SessionState.Current.EnsureProject();
                var currentProject = SessionState.Current.CurrentProject;

                // 校验项目是否为当前项目
                if (currentProject.Id != pid)
                {
                    return ErrorJson($"项目 {pid} 不是当前打开的项目 {currentProject.Id}，请先打开该项目");
                }

                // 从 CrossProjectDataRef 表加载所有引用
                var store = new CrossProjectDataRefStore(currentProject);
                var refs = store.LoadAll().GetAwaiter().GetResult();

                // 同时加载 CrossProjectFormula 表中的公式引用
                var formulaStore = currentProject.FormulaStore;
                List<CrossProjectFormula> formulas = new List<CrossProjectFormula>();
                try
                {
                    formulas = formulaStore.Load().GetAwaiter().GetResult();
                }
                catch
                {
                    // 公式引用表可能不存在，忽略
                }

                var refsArr = new JArray();
                foreach (var r in refs)
                {
                    refsArr.Add(new JObject
                    {
                        ["ref_id"] = r.Id.Value.ToString(),
                        ["name"] = r.Name,
                        ["source_project_id"] = r.SourceProjectId.ToString(),
                        ["source_project_name"] = r.SourceProjectName,
                        ["source_table_id"] = r.SourceTableId.Value.ToString(),
                        ["target_table_id"] = r.TargetTableId.Value.ToString(),
                        ["ref_mode"] = r.RefMode.ToString(),
                        ["formula"] = r.FormulaExpression ?? "",
                        ["auto_refresh"] = r.AutoRefresh,
                        ["enabled"] = r.Enabled,
                        ["created_at"] = r.CreatedAt.ToString("o"),
                        ["updated_at"] = r.UpdatedAt.ToString("o")
                    });
                }

                var formulasArr = new JArray();
                foreach (var f in formulas)
                {
                    formulasArr.Add(new JObject
                    {
                        ["formula_id"] = f.Id.Value.ToString(),
                        ["source_project_id"] = f.SourceProjectId.ToString(),
                        ["source_table_id"] = f.SourceTableId.Value.ToString(),
                        ["target_table_id"] = f.TargetTableId.Value.ToString(),
                        ["formula_type"] = f.FormulaType.ToString(),
                        ["formula_expression"] = f.FormulaExpression ?? "",
                        ["enabled"] = f.Enabled,
                        ["created_at"] = f.CreatedAt.ToString("o")
                    });
                }

                var result = new JObject
                {
                    ["success"] = true,
                    ["project_id"] = pid.ToString(),
                    ["project_name"] = currentProject.Name,
                    ["data_ref_count"] = refs.Count,
                    ["data_refs"] = refsArr,
                    ["formula_ref_count"] = formulas.Count,
                    ["formula_refs"] = formulasArr,
                    ["message"] = $"共 {refs.Count} 条数据引用，{formulas.Count} 条公式引用"
                };
                return JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("获取跨项目引用失败: " + ex.Message);
            }
        }

        // =============================================
        // 跨项目公式求值
        // =============================================

        /// <summary>
        /// 在跨项目上下文中求值公式
        /// </summary>
        /// <param name="projectId">项目 ID（Guid 字符串，应为当前打开的项目）</param>
        /// <param name="formula">要求值的公式表达式</param>
        /// <returns>JSON 格式的求值结果</returns>
        public static string EvaluateCrossProjectFormula(string projectId, string formula)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(projectId))
                {
                    return ErrorJson("项目 ID 不能为空");
                }
                if (string.IsNullOrWhiteSpace(formula))
                {
                    return ErrorJson("公式不能为空");
                }
                if (!Guid.TryParse(projectId, out Guid pid))
                {
                    return ErrorJson("项目 ID 格式无效（需要 Guid 字符串）");
                }

                // 确保当前项目已打开
                SessionState.Current.EnsureProject();
                var currentProject = SessionState.Current.CurrentProject;

                // 校验项目是否为当前项目
                if (currentProject.Id != pid)
                {
                    return ErrorJson($"项目 {pid} 不是当前打开的项目 {currentProject.Id}，请先打开该项目");
                }

                // 尝试通过 FormulaStore 求值已注册的跨项目公式
                var formulaStore = currentProject.FormulaStore;
                List<CrossProjectFormula> formulas;
                try
                {
                    formulas = formulaStore.Load().GetAwaiter().GetResult();
                }
                catch
                {
                    formulas = new List<CrossProjectFormula>();
                }

                // 查找匹配的已注册公式（按表达式匹配）
                CrossProjectFormula matched = formulas.FirstOrDefault(
                    f => f.Enabled && !string.IsNullOrEmpty(f.FormulaExpression)
                         && formula.IndexOf(f.FormulaExpression, StringComparison.OrdinalIgnoreCase) >= 0);

                if (matched != null)
                {
                    // 使用 FormulaStore 求值已注册公式
                    var results = formulaStore.Evaluate().GetAwaiter().GetResult();
                    var matchedResult = results.FirstOrDefault(r => r.FormulaId == matched.Id);

                    if (matchedResult != null)
                    {
                        var dataObj = new JObject();
                        if (matchedResult.Data != null)
                        {
                            foreach (var kv in matchedResult.Data)
                            {
                                dataObj[kv.Key] = kv.Value == null ? null : JToken.FromObject(kv.Value);
                            }
                        }

                        var result = new JObject
                        {
                            ["success"] = matchedResult.Success,
                            ["project_id"] = pid.ToString(),
                            ["formula"] = formula,
                            ["matched_formula_id"] = matched.Id.Value.ToString(),
                            ["formula_type"] = matched.FormulaType.ToString(),
                            ["source_project_id"] = matched.SourceProjectId.ToString(),
                            ["result_data"] = dataObj,
                            ["error"] = matchedResult.Error ?? ""
                        };
                        return JsonConvert.SerializeObject(result, Formatting.Indented);
                    }
                }

                // 未匹配到已注册公式，执行基本求值
                // 尝试解析公式中的跨项目引用 [ProjectGuid.TableId.ColId] 并汇总
                var evalResult = EvaluateBasicCrossProjectFormula(currentProject, formula);

                var basicResult = new JObject
                {
                    ["success"] = evalResult.Success,
                    ["project_id"] = pid.ToString(),
                    ["formula"] = formula,
                    ["result"] = evalResult.Result,
                    ["result_type"] = evalResult.ResultType,
                    ["evaluated_refs"] = evalResult.EvaluatedRefs,
                    ["message"] = evalResult.Success
                        ? "跨项目公式求值完成（基本求值模式）"
                        : "跨项目公式求值失败: " + evalResult.Error,
                    ["error"] = evalResult.Error ?? ""
                };
                return JsonConvert.SerializeObject(basicResult, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return ErrorJson("求值跨项目公式失败: " + ex.Message);
            }
        }

        // =============================================
        // 辅助方法
        // =============================================

        /// <summary>
        /// 打开外部项目
        /// </summary>
        private static Project OpenExternalProject(Guid projectId)
        {
            try
            {
                string dbPath = GetExternalDbPath(projectId);
                if (!File.Exists(dbPath))
                {
                    return null;
                }

                var project = new Project { Id = projectId };
                project.Dal = new ProjectDAL(dbPath);
                project.Load();

                var dto = project.Dal.GetProject();
                if (dto != null)
                {
                    project.PopulateFieldsFromDto(dto);
                }
                return project;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取外部项目数据库路径
        /// </summary>
        private static string GetExternalDbPath(Guid projectId)
        {
            long userId = Auditai.Model.User.Current?.Id ?? 1;
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "data", userId.ToString(), $"{projectId}.db");
        }

        /// <summary>
        /// 尝试获取项目名称
        /// </summary>
        private static string TryGetProjectName(Guid projectId)
        {
            try
            {
                string dbPath = GetExternalDbPath(projectId);
                if (!File.Exists(dbPath)) return null;

                var dal = new ProjectDAL(dbPath);
                var dto = dal.GetProject();
                return dto?.Name;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 收集表格中数值列的汇总信息（求和）
        /// </summary>
        private static JObject CollectNumericSummary(Table table)
        {
            var summary = new JObject();
            try
            {
                var rows = table.Rows?.Where(r => r != null && r.Role == RowRole.Normal).ToList() ?? new List<Row>();
                var columns = table.Columns?.Where(c => c != null).ToList() ?? new List<Column>();

                foreach (var col in columns)
                {
                    double colSum = 0;
                    bool hasNumeric = false;
                    foreach (var row in rows)
                    {
                        var cell = table[row.Index, col.Index];
                        if (cell?.Value == null) continue;
                        var val = cell.Value;
                        if (val is double d)
                        {
                            colSum += d;
                            hasNumeric = true;
                        }
                        else if (val is int i)
                        {
                            colSum += i;
                            hasNumeric = true;
                        }
                        else if (val is decimal dec)
                        {
                            colSum += (double)dec;
                            hasNumeric = true;
                        }
                        else if (val is string s && double.TryParse(s, out var parsed))
                        {
                            colSum += parsed;
                            hasNumeric = true;
                        }
                    }
                    if (hasNumeric)
                    {
                        summary[col.Caption ?? $"列{col.Index}"] = Math.Round(colSum, 2);
                    }
                }
            }
            catch
            {
                // 忽略汇总异常
            }
            return summary;
        }

        /// <summary>
        /// 合并数值汇总到聚合信息
        /// </summary>
        private static void MergeNumericSummary(Dictionary<string, double> target, JObject source)
        {
            if (source == null) return;
            foreach (var prop in source.Properties())
            {
                double val = prop.Value?.Value<double>() ?? 0;
                if (target.ContainsKey(prop.Name))
                {
                    target[prop.Name] += val;
                }
                else
                {
                    target[prop.Name] = val;
                }
            }
        }

        /// <summary>
        /// 追加行数据到结果数组
        /// </summary>
        private static void AppendRows(JArray target, string projectName, string tableName, Table table)
        {
            try
            {
                var rows = table.Rows?.Where(r => r != null && r.Role == RowRole.Normal).ToList() ?? new List<Row>();
                var columns = table.Columns?.Where(c => c != null).ToList() ?? new List<Column>();

                foreach (var row in rows)
                {
                    var rowObj = new JObject
                    {
                        ["source_project"] = projectName,
                        ["source_table"] = tableName,
                        ["row_index"] = row.Index
                    };
                    var rowData = new JObject();
                    foreach (var col in columns)
                    {
                        var cell = table[row.Index, col.Index];
                        string colName = col.Caption ?? $"列{col.Index}";
                        rowData[colName] = cell?.Value == null ? "" : cell.Value.ToString();
                    }
                    rowObj["data"] = rowData;
                    target.Add(rowObj);
                }
            }
            catch
            {
                // 忽略行追加异常
            }
        }

        /// <summary>
        /// 基本跨项目公式求值
        /// 解析公式中 [ProjectGuid] 形式的引用，打开对应项目并尝试求值
        /// </summary>
        private static BasicEvalResult EvaluateBasicCrossProjectFormula(Project currentProject, string formula)
        {
            var result = new BasicEvalResult();
            var evaluatedRefs = new JArray();

            try
            {
                // 检查是否为本地模式
                if (!StorageRouter.IsLocalMode)
                {
                    result.Error = "非本地模式暂不支持跨项目公式求值";
                    return result;
                }

                // 解析公式中的项目 Guid 引用
                var projectIds = new HashSet<Guid>();
                int guidStart = formula.IndexOf('[');
                while (guidStart >= 0)
                {
                    int guidEnd = formula.IndexOf(']', guidStart);
                    if (guidEnd < 0) break;
                    string refContent = formula.Substring(guidStart + 1, guidEnd - guidStart - 1);
                    // 尝试解析为 Guid
                    var parts = refContent.Split(new[] { '.', '|', ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0 && Guid.TryParse(parts[0], out Guid pid))
                    {
                        projectIds.Add(pid);
                    }
                    guidStart = formula.IndexOf('[', guidEnd);
                }

                // 打开引用的项目并收集数据
                foreach (var pid in projectIds)
                {
                    var extProject = OpenExternalProject(pid);
                    evaluatedRefs.Add(new JObject
                    {
                        ["project_id"] = pid.ToString(),
                        ["project_name"] = extProject?.Name ?? "未知",
                        ["exists"] = extProject != null,
                        ["table_count"] = extProject?.GetAllTableNodes().Count() ?? 0
                    });
                }

                // 尝试使用 DataTable.Compute 进行基本运算
                // 将公式中的引用替换为数值后计算
                string expr = formula;
                foreach (var pid in projectIds)
                {
                    var extProject = OpenExternalProject(pid);
                    if (extProject == null) continue;

                    // 汇总该项目所有表格的数值列总和
                    double projectTotal = 0;
                    foreach (var tableNode in extProject.GetAllTableNodes())
                    {
                        try
                        {
                            var table = tableNode.Table;
                            if (table == null) continue;
                            table.LoadAndReturn(true);
                            var summary = CollectNumericSummary(table);
                            foreach (var prop in summary.Properties())
                            {
                                projectTotal += prop.Value?.Value<double>() ?? 0;
                            }
                        }
                        catch { }
                    }
                    // 替换公式中的项目引用为汇总值
                    expr = expr.Replace($"[{pid}]", projectTotal.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }

                // 尝试计算剩余表达式
                if (expr.Contains("["))
                {
                    // 仍有未解析的引用，移除无法解析的部分
                    result.Result = "公式包含未解析的引用，已收集引用项目信息";
                    result.ResultType = "Partial";
                    result.Success = true;
                }
                else
                {
                    try
                    {
                        var computed = new System.Data.DataTable().Compute(expr, null);
                        result.Result = computed?.ToString() ?? "";
                        result.ResultType = computed?.GetType()?.Name ?? "Null";
                        result.Success = true;
                    }
                    catch
                    {
                        result.Result = expr;
                        result.ResultType = "Expression";
                        result.Success = true;
                    }
                }

                result.EvaluatedRefs = evaluatedRefs;
                return result;
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                result.EvaluatedRefs = evaluatedRefs;
                return result;
            }
        }

        /// <summary>
        /// 生成错误 JSON 响应
        /// </summary>
        private static string ErrorJson(string message)
        {
            var result = new JObject
            {
                ["success"] = false,
                ["error"] = message
            };
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        // =============================================
        // 内部辅助类型
        // =============================================

        private class AggregateTableInfo
        {
            public string Name { get; set; }
            public List<string> SourceProjects { get; set; }
            public int RowCount { get; set; }
            public Dictionary<string, double> NumericColumns { get; set; } = new Dictionary<string, double>();
        }

        private class BasicEvalResult
        {
            public bool Success { get; set; }
            public string Result { get; set; } = "";
            public string ResultType { get; set; } = "";
            public string Error { get; set; }
            public JArray EvaluatedRefs { get; set; } = new JArray();
        }
    }
}
