﻿using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.LedgerView;

// Row/Table/Cell 在 Auditai.DTO 与 Auditai.Model 中重名，此处固定为模型层类型
using Row = Auditai.Model.Row;
using Table = Auditai.Model.Table;
using Cell = Auditai.Model.Cell;

// 列枚举定义在两个 Builder 类内部，用别名拉平便于书写
using BalanceVirtualTableColumnIndex = Auditai.UI.Platform.BalanceVirtualTableBuilder.BalanceVirtualTableColumnIndex;
using VoucherVirtualTableColumnIndex = Auditai.UI.Platform.VoucherVirtualTableBuilder.VoucherVirtualTableColumnIndex;

namespace Auditai.UI.Platform;

// 风险检查"公式检查项"求值编排层（UI.Platform）。
//
// 【公式文本中引用账套虚拟表列的语法】
// 公式存储文本中引用虚拟表列的语法为 [2:{表Id}:{列Id}]（FormulaParser 的 RefColumn 规则，见
// ToFormulaRewriter.ExitTableColumn 生成该文本）。表Id 取 BalanceVirtualTableBuilder.BalanceVirtualTableId.Value
// （科目余额表，27 列）或 VoucherVirtualTableBuilder.VoucherVirtualTableId.Value（会计凭证表，11 列）；
// 列Id 为各 Builder 列枚举（BalanceVirtualTableColumnIndex / VoucherVirtualTableColumnIndex）的整数值，
// 同时也是虚拟表的列下标。编辑器显示态形如 "{科目余额表}[期末借方净额]"，由
// LedgerCollectFormulaDisplayRewriter（显示）与 ToFormulaRewriter（回写）双向转换。
// 求值时 FormulaEvaluationVisitorLedgerVirtualTable.VisitRefColumn 把 [2:余额表Id:列Id] 经
// LedgerVirtualTableEvalContext 委托解析为 LedgerVirtualTableColumnOperand（整列单元格集合）。
//
// 【求值模式判定】
// 左右表达式各求值一次：
//   1) 结果为 CellsOperand（含其子类 LedgerVirtualTableColumnOperand/ColumnOperand，即列值集合）→ 行级模式；
//   2) 两侧均非 CellsOperand（标量，如 Sum([...]) 聚合结果、常量运算）→ 聚合模式，整体比较一次。
//
// 【行级配对实现说明（与"构造单行虚拟表逐行求值"等价的简化实现）】
// 虚拟表列引用与行上下文无关（VisitRefColumn 不依赖 RowIndex，整列返回），因此对整表求值一次得到
// 左右两侧的 Cell 列表后按 Row 逐行配对比较，与"逐行构造单行虚拟表再求值"结果完全一致。
// 注意：不能使用 ValueSetOperand.Set 做行配对——其 HashSet 按"值"去重（Tuple2Item2Comparer），
// 同值行会被合并丢失 Row；故行级配对使用 CellsOperand.Cells（List<Cell>，与虚拟表行一一对应）。
// 若表达式求值为纯 ValueSetOperand（来自集合类函数而非列引用），无法按行对齐，输出 IsError 结果。
//
// 【比较符语义】对齐 ValidationManager.GetPassed / ValidationOperator.Code：
// 0 "="、1 ">"、2 ">="、3 "<"、4 "<="、5 "<>"；数值比较带 0.0001 容差（NumberOperand.EQUALITY_EPSILON）。
//
// 【注册约定】本类通过静态委托与 LedgerModel 的 RiskCheckEngine 协作：
//   RiskCheckEngine.FormulaRuleExecutor = RiskCheckFormulaEvaluator.EvaluateRuleForLedger;
// RiskCheckEngine.FormulaRuleExecutor 为 Func<Ledger, RiskCheckRule, List<RiskCheckResult>>
// （LedgerModel 程序集不引用 LedgerViewer2，故委托以 Ledger 传参）；本类适配方法按当前打开的
// 账套视图（Program.MainForm.CurrentLedgerViewer）解析出 LedgerViewer 后走完整求值流程。
// 注册点在 Program.Main（StartAuditaiPlatform 之前），确保任何风险检查执行前完成注册。
public static class RiskCheckFormulaEvaluator
{
	// 数值比较容差，对齐 NumberOperand.EQUALITY_EPSILON / ValidationManager 的 4 位舍入口径
	private const double Epsilon = 0.0001;

	private static readonly string[] OperatorTexts = { "=", ">", ">=", "<", "<=", "<>" };

	// RiskCheckEngine.FormulaRuleExecutor（Func<Ledger, ...>）的注册入口：
	// 把 Ledger 解析为已打开的 LedgerViewer 后走完整求值流程（优先当前视图，多账套时按账套反查）
	public static List<RiskCheckResult> EvaluateRuleForLedger(Ledger ledger, RiskCheckRule rule)
	{
		List<RiskCheckResult> results = new List<RiskCheckResult>();
		if (rule == null)
		{
			return results;
		}
		LedgerViewer viewer = null;
		MainForm mainForm = Program.MainForm;
		if (mainForm != null)
		{
			if (mainForm.CurrentLedgerViewer?.Ledger == ledger)
			{
				viewer = mainForm.CurrentLedgerViewer;
			}
			else
			{
				// 多账套场景：按目标账套反查对应视图，避免误用其它账套的虚拟表
				foreach (LedgerViewer opened in mainForm.OpenedLedgerViewerDic.Values)
				{
					if (opened?.Ledger == ledger)
					{
						viewer = opened;
						break;
					}
				}
			}
		}
		if (viewer == null)
		{
			results.Add(CreateErrorResult(rule, "未找到该账套已打开的视图（LedgerViewer），无法执行公式检查"));
			return results;
		}
		return EvaluateRule(viewer, rule);
	}

	// 求值单条公式检查规则（UI 侧直接持有 LedgerViewer 时使用）
	public static List<RiskCheckResult> EvaluateRule(LedgerViewer viewer, RiskCheckRule rule)
	{
		List<RiskCheckResult> results = new List<RiskCheckResult>();
		if (rule == null)
		{
			return results;
		}
		try
		{
			if (viewer == null || viewer.Ledger == null)
			{
				results.Add(CreateErrorResult(rule, "账套未打开，无法执行公式检查"));
				return results;
			}
			if (string.IsNullOrWhiteSpace(rule.LeftExpr) || string.IsNullOrWhiteSpace(rule.RightExpr))
			{
				results.Add(CreateErrorResult(rule, "公式检查规则缺少左/右表达式"));
				return results;
			}
			LedgerVirtualTable balanceTable = LedgerVirtualTableUtils.GetBalanceVirtualTable(viewer);
			LedgerVirtualTable voucherTable = LedgerVirtualTableUtils.GetVoucherVirtualTable(viewer.Ledger);
			if (balanceTable == null || voucherTable == null)
			{
				results.Add(CreateErrorResult(rule, "账套虚拟表（科目余额表/会计凭证表）构建失败，无法执行公式检查"));
				return results;
			}
			LedgerVirtualTableEvalContext tec = CreateEvalContext(balanceTable, voucherTable);
			Operand leftOperand = CreateEvaluator(rule.LeftExpr).EvaluateOnLedgerVirtualTable(tec, BalanceVirtualTableBuilder.BalanceVirtualTableId, VoucherVirtualTableBuilder.VoucherVirtualTableId);
			Operand rightOperand = CreateEvaluator(rule.RightExpr).EvaluateOnLedgerVirtualTable(tec, BalanceVirtualTableBuilder.BalanceVirtualTableId, VoucherVirtualTableBuilder.VoucherVirtualTableId);
			if (leftOperand is CellsOperand || rightOperand is CellsOperand)
			{
				EvaluateRowLevel(rule, viewer.Ledger, balanceTable, voucherTable, leftOperand, rightOperand, results);
			}
			else
			{
				EvaluateAggregate(rule, leftOperand, rightOperand, results);
			}
		}
		catch (Exception ex)
		{
			// 单条规则异常不中断整体检查流程
			results.Add(CreateErrorResult(rule, "公式检查执行异常：" + ex.Message));
		}
		return results;
	}

	// 聚合模式：左右均为标量，整体比较一次
	private static void EvaluateAggregate(RiskCheckRule rule, Operand leftOperand, Operand rightOperand, List<RiskCheckResult> results)
	{
		if (!TryGetNumber(leftOperand, out var leftValue))
		{
			results.Add(CreateErrorResult(rule, "左" + DescribeNotNumber(leftOperand)));
			return;
		}
		if (!TryGetNumber(rightOperand, out var rightValue))
		{
			results.Add(CreateErrorResult(rule, "右" + DescribeNotNumber(rightOperand)));
			return;
		}
		if (!Compare(leftValue, rightValue, rule.OperatorCode))
		{
			RiskCheckResult riskCheckResult = NewRuleResult(rule);
			riskCheckResult.HitDescription = $"左值 {F(leftValue)} {GetOperatorText(rule.OperatorCode)} 右值 {F(rightValue)} 不成立（左:{rule.LeftExpr}；右:{rule.RightExpr}）";
			riskCheckResult.Amount = ToAmount(leftValue);
			results.Add(riskCheckResult);
		}
	}

	// 行级模式：至少一侧为列值集合，按 Row 配对逐行比较
	// ledger 用于把凭证虚拟表行映射回 Ledger.Vouchers（BuildImpl 按下标 1:1 构建行），带出命中凭证引用
	private static void EvaluateRowLevel(RiskCheckRule rule, Ledger ledger, LedgerVirtualTable balanceTable, LedgerVirtualTable voucherTable, Operand leftOperand, Operand rightOperand, List<RiskCheckResult> results)
	{
		if (!TryResolveSide(leftOperand, out var leftMap, out var leftScalar, out var leftError))
		{
			results.Add(CreateErrorResult(rule, "左" + leftError));
			return;
		}
		if (!TryResolveSide(rightOperand, out var rightMap, out var rightScalar, out var rightError))
		{
			results.Add(CreateErrorResult(rule, "右" + rightError));
			return;
		}
		// 行遍历域：两侧集合行的并集，按行号排序；同一虚拟表的两列按 Row 对象对齐
		HashSet<Row> rowDomain = new HashSet<Row>();
		if (leftMap != null)
		{
			rowDomain.UnionWith(leftMap.Keys);
		}
		if (rightMap != null)
		{
			rowDomain.UnionWith(rightMap.Keys);
		}
		int comparedCount = 0;
		foreach (Row row in rowDomain.OrderBy((Row r) => r.Index))
		{
			// 一侧为标量时直接用标量；集合侧该行缺值则跳过（例如 SumIf 过滤后的子集行）
			object leftObj = null;
			object rightObj = null;
			if (leftMap != null && !leftMap.TryGetValue(row, out leftObj))
			{
				continue;
			}
			if (rightMap != null && !rightMap.TryGetValue(row, out rightObj))
			{
				continue;
			}
			double leftValue = leftScalar;
			bool leftValid = (leftMap == null) || TryConvertDouble(leftObj, out leftValue);
			double rightValue = rightScalar;
			bool rightValid = (rightMap == null) || TryConvertDouble(rightObj, out rightValue);
			if (!leftValid || !rightValid)
			{
				RiskCheckResult riskCheckResult = NewRuleResult(rule);
				DescribeRow(riskCheckResult, row, ledger, balanceTable, voucherTable);
				riskCheckResult.IsError = true;
				riskCheckResult.ErrorMessage = $"行数据不是数值，无法比较（左:{FormatCellValue(leftObj)}，右:{FormatCellValue(rightObj)}）";
				results.Add(riskCheckResult);
				continue;
			}
			comparedCount++;
			if (Compare(leftValue, rightValue, rule.OperatorCode))
			{
				continue;
			}
			RiskCheckResult riskCheckResult2 = NewRuleResult(rule);
			string text = DescribeRow(riskCheckResult2, row, ledger, balanceTable, voucherTable);
			riskCheckResult2.HitDescription = $"{text}：左值 {F(leftValue)} {GetOperatorText(rule.OperatorCode)} 右值 {F(rightValue)} 不成立（左:{rule.LeftExpr}；右:{rule.RightExpr}）";
			riskCheckResult2.Amount = ToAmount(leftValue);
			results.Add(riskCheckResult2);
		}
		if (comparedCount == 0 && leftMap != null && rightMap != null)
		{
			results.Add(CreateErrorResult(rule, "左右表达式引用了不同虚拟表的行，无法逐行配对比较；请对两侧分别使用 Sum 等聚合函数后再比较"));
		}
	}

	// 构建虚拟表求值上下文（照 TicketInputEditor2.GetEvalContext 模式；虚拟表已预构建，直接按列 Id 解析）
	private static LedgerVirtualTableEvalContext CreateEvalContext(LedgerVirtualTable balanceTable, LedgerVirtualTable voucherTable)
	{
		return new LedgerVirtualTableEvalContext
		{
			BalanceTable_ResolveColumn = delegate (Id64 colId)
			{
				int num = (int)colId.Value;
				return (num >= 0 && num < balanceTable.Columns.Count) ? new LedgerVirtualTableColumnOperand(balanceTable.Columns[num]) : null;
			},
			VoucherTable_ResolveColumn = delegate (Id64 colId)
			{
				int num2 = (int)colId.Value;
				return (num2 >= 0 && num2 < voucherTable.Columns.Count) ? new LedgerVirtualTableColumnOperand(voucherTable.Columns[num2]) : null;
			}
		};
	}

	private static FormulaEvaluator CreateEvaluator(string expr)
	{
		// 风险检查公式只允许引用账套虚拟表列、常量与公式函数，不引用项目内表格，
		// 因此 Env 无需 Resolver；若公式引用了项目表格，求值将抛错并由规则级 try-catch 转为 IsError 结果。
		return new FormulaEvaluator(expr)
		{
			Env = new FormulaEvaluationEnvironment
			{
				RowIndex = 0
			}
		};
	}

	// 把列值集合按行展开：Row -> 单元格原始值
	private static Dictionary<Row, object> ToRowValueMap(CellsOperand cellsOperand)
	{
		Dictionary<Row, object> dictionary = new Dictionary<Row, object>();
		foreach (Cell cell in cellsOperand.Cells)
		{
			dictionary[cell.Row] = cell.Value;
		}
		return dictionary;
	}

	// 解析单侧操作数：列集合 -> 行映射；标量 -> 数值
	private static bool TryResolveSide(Operand operand, out Dictionary<Row, object> rowMap, out double scalar, out string error)
	{
		rowMap = null;
		scalar = 0.0;
		error = null;
		if (operand is CellsOperand cellsOperand)
		{
			rowMap = ToRowValueMap(cellsOperand);
			return true;
		}
		if (TryGetNumber(operand, out scalar))
		{
			return true;
		}
		error = DescribeNotNumber(operand);
		return false;
	}

	private static string DescribeNotNumber(Operand operand)
	{
		if (operand is ValueSetOperand)
		{
			return "表达式结果为数组值集合，无法作为数值比较；请使用 Sum/Max 等聚合函数或直接引用虚拟表列（" + DescribeOperand(operand) + "）";
		}
		return "表达式结果不是数值（" + DescribeOperand(operand) + "）";
	}

	private static string DescribeOperand(Operand operand)
	{
		if (operand == null)
		{
			return "空";
		}
		if (operand is ValueOperand valueOperand)
		{
			return valueOperand.Object?.ToString() ?? "空";
		}
		return operand.ToString();
	}

	private static bool TryGetNumber(Operand operand, out double value)
	{
		value = 0.0;
		if (operand == null)
		{
			return false;
		}
		if (operand is NumberOperand numberOperand)
		{
			value = numberOperand.Value;
			return true;
		}
		if (operand is CellOperand cellOperand)
		{
			return TryGetNumber(cellOperand.Value, out value);
		}
		if (operand is ValueOperand valueOperand)
		{
			return TryConvertDouble(valueOperand.Object, out value);
		}
		return false;
	}

	private static bool TryConvertDouble(object value, out double result)
	{
		result = 0.0;
		if (value == null)
		{
			return false;
		}
		if (value is double num)
		{
			result = num;
			return true;
		}
		if (value is int num2)
		{
			result = num2;
			return true;
		}
		if (value is long num3)
		{
			result = num3;
			return true;
		}
		if (value is decimal num4)
		{
			result = (double)num4;
			return true;
		}
		if (value is float num5)
		{
			result = num5;
			return true;
		}
		if (value is bool flag)
		{
			result = (flag ? 1.0 : 0.0);
			return true;
		}
		if (value is string text && double.TryParse(text.Trim(), out result))
		{
			return true;
		}
		return false;
	}

	// 比较符语义对齐 ValidationManager.GetPassed：0 "="、1 ">"、2 ">="、3 "<"、4 "<="、5 "<>"
	private static bool Compare(double left, double right, int opCode)
	{
		bool flag = Math.Abs(left - right) < Epsilon;
		switch (opCode)
		{
		case 0:
			return flag;
		case 1:
			if (!flag)
			{
				return left > right;
			}
			return false;
		case 2:
			if (!flag)
			{
				return left > right;
			}
			return true;
		case 3:
			if (!flag)
			{
				return left < right;
			}
			return false;
		case 4:
			if (!flag)
			{
				return left < right;
			}
			return true;
		case 5:
			return !flag;
		default:
			throw new ArgumentOutOfRangeException("opCode", opCode, "未知的比较符编码");
		}
	}

	private static string GetOperatorText(int opCode)
	{
		if (opCode >= 0 && opCode < OperatorTexts.Length)
		{
			return OperatorTexts[opCode];
		}
		return opCode.ToString();
	}

	// 填充结果行的账套定位信息（余额表行填科目代码/名称/辅助核算；凭证表行填凭证信息并映射回 Ledger.Vouchers 带出凭证引用），并返回简短行标签
	private static string DescribeRow(RiskCheckResult result, Row row, Ledger ledger, LedgerVirtualTable balanceTable, LedgerVirtualTable voucherTable)
	{
		if (row == null)
		{
			return "未知行";
		}
		if (row.Table == balanceTable)
		{
			string yearMonth = FormatYearMonth(GetCellValue(balanceTable, row.Index, (int)BalanceVirtualTableColumnIndex.年月));
			result.AccountCode = GetCellText(balanceTable, row.Index, (int)BalanceVirtualTableColumnIndex.科目代码);
			result.AccountName = GetCellText(balanceTable, row.Index, (int)BalanceVirtualTableColumnIndex.科目名称);
			string auxClassName = GetCellText(balanceTable, row.Index, (int)BalanceVirtualTableColumnIndex.辅助核算类别);
			if (!string.IsNullOrEmpty(auxClassName))
			{
				result.AuxDetail = $"{auxClassName}:{GetCellText(balanceTable, row.Index, (int)BalanceVirtualTableColumnIndex.辅助核算名称)}({GetCellText(balanceTable, row.Index, (int)BalanceVirtualTableColumnIndex.辅助核算代码)})";
			}
			return $"{yearMonth} {result.AccountCode} {result.AccountName}".Trim();
		}
		if (row.Table == voucherTable)
		{
			// VoucherVirtualTableBuilder.BuildImpl 按 Ledger.Vouchers 下标 1:1 构建虚拟表行，row.Index 即凭证下标
			if (ledger != null && row.Index >= 0 && row.Index < ledger.Vouchers.Count)
			{
				result.Voucher = ledger.Vouchers[row.Index];
			}
			string voucherNo = GetCellText(voucherTable, row.Index, (int)VoucherVirtualTableColumnIndex.凭证字号);
			string voucherDate = FormatYearMonth(GetCellValue(voucherTable, row.Index, (int)VoucherVirtualTableColumnIndex.凭证日期));
			string digest = GetCellText(voucherTable, row.Index, (int)VoucherVirtualTableColumnIndex.摘要);
			result.VoucherInfo = $"{voucherNo} {voucherDate} {digest}".Trim();
			result.AccountCode = GetCellText(voucherTable, row.Index, (int)VoucherVirtualTableColumnIndex.科目代码);
			result.AccountName = GetCellText(voucherTable, row.Index, (int)VoucherVirtualTableColumnIndex.科目名称);
			return result.VoucherInfo;
		}
		return $"第{row.Index + 1}行";
	}

	private static object GetCellValue(Table table, int rowIndex, int columnIndex)
	{
		return table[rowIndex, columnIndex].Value;
	}

	private static string GetCellText(Table table, int rowIndex, int columnIndex)
	{
		return FormatCellValue(GetCellValue(table, rowIndex, columnIndex));
	}

	private static string FormatCellValue(object value)
	{
		if (value == null)
		{
			return "空";
		}
		if (value is string text)
		{
			return text.Trim();
		}
		return value.ToString();
	}

	private static string FormatYearMonth(object value)
	{
		if (value is DateYearMonth dateYearMonth)
		{
			return dateYearMonth.Date.ToString("yyyy-MM");
		}
		if (value is DateTime dateTime)
		{
			return dateTime.ToString("yyyy-MM-dd");
		}
		return value?.ToString() ?? string.Empty;
	}

	private static RiskCheckResult NewRuleResult(RiskCheckRule rule)
	{
		return new RiskCheckResult
		{
			Rule = rule,
			RuleNote = rule.Note,
			RuleType = rule.RuleType
		};
	}

	private static RiskCheckResult CreateErrorResult(RiskCheckRule rule, string message)
	{
		RiskCheckResult riskCheckResult = NewRuleResult(rule);
		riskCheckResult.IsError = true;
		riskCheckResult.ErrorMessage = message;
		return riskCheckResult;
	}

	private static decimal ToAmount(double value)
	{
		try
		{
			return Math.Round((decimal)value, 2, MidpointRounding.AwayFromZero);
		}
		catch (OverflowException)
		{
			// 超出 decimal 范围的极端值不影响命中描述，金额按 0 呈现
			return 0m;
		}
	}

	private static string F(double value)
	{
		return value.ToString("F2");
	}
}
