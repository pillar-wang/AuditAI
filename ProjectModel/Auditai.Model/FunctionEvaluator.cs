﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Auditai.DTO;

namespace Auditai.Model;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class FunctionEvaluator
{
	private const string CATEGORY_DATE = "日期";

	private const string CATEGORY_MATH = "数学";

	private const string CATEGORY_LOGICAL = "逻辑";

	private const string CATEGORY_TEXT = "文本";

	private const string CATEGORY_STAT = "统计";

	private const string CATEGORY_REFERENCE = "引用";

	private const string CATEGORY_CONTROL = "控制";

	private const double RoundEqualCompareEpsilon = 1E-09;

	private FormulaEvaluationEnvironment _env;

	private FormulaEvaluationVisitor _visitor;

	internal static Dictionary<List<ColumnOperand>, List<List<Cell>>> _distinctCache = new Dictionary<List<ColumnOperand>, List<List<Cell>>>(SequenceEqualsComparer<List<ColumnOperand>, ColumnOperand>.Instance);

	internal static CellListRowsCountDictionary _cellListRowsCountCache = new CellListRowsCountDictionary();

	public FunctionEvaluator(FormulaEvaluationEnvironment env, FormulaEvaluationVisitor visitor)
	{
		_env = env;
		_visitor = visitor;
	}

	[Category("逻辑")]
	[DisplayName("返回逻辑值真")]
	[Description("返回逻辑值真")]
	[Order(44)]
	public bool True()
	{
		return true;
	}

	[Category("逻辑")]
	[DisplayName("返回逻辑值假")]
	[Description("返回逻辑值假")]
	[Order(53)]
	public bool False()
	{
		return false;
	}

	[Category("日期")]
	[DisplayName("返回当前日期")]
	[Description("返回当前日期")]
	[Order(62)]
	public DateTime Today()
	{
		return DateTime.Today;
	}

	[Category("日期")]
	[DisplayName("年月日组合日期")]
	[Description("根据年、月、日计算日期")]
	[Order(71)]
	public DateTime Date([ParameterName("数值参数1")][Description("年份数值")] Operand year, [ParameterName("数值参数2")][Description("月份数值")] Operand month, [ParameterName("数值参数3")][Description("日期数值")] Operand day)
	{
		int num = (int)year.ToNumber();
		int num2 = (int)month.ToNumber();
		int num3 = (int)day.ToNumber();
		try
		{
			return new DateTime(num, num2, num3);
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new FormulaBadValueException($"给出的{num}年、{num2}月、{num3}日无法组成有效的日期");
		}
	}

	[Category("日期")]
	[DisplayName("年月组合日期")]
	[Description("根据年、月计算日期")]
	[Order(93)]
	public Operand YearMonth([ParameterName("数值参数1")][Description("年份数值")] Operand year, [ParameterName("数值参数2")][Description("月份数值")] Operand month)
	{
		int num = (int)year.ToNumber();
		int num2 = (int)month.ToNumber();
		try
		{
			if (num < 1 || num > 9999 || num2 < 1 || num2 > 12)
			{
				return StringOperand.Empty;
			}
			if (num != 0 && num2 != 0)
			{
				return new DateYearMonthOperand(new DateYearMonth(new DateTime(num, num2, 1)));
			}
			if (num == 0 || num2 == 0)
			{
				return StringOperand.Empty;
			}
			return new DateYearMonthOperand(new DateYearMonth(new DateTime(num, num2, 1)));
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new FormulaBadValueException($"给出的{num}年、{num2}月无法组成有效的日期");
		}
	}

	[Category("日期")]
	[DisplayName("从日期中提取年份")]
	[Description("从日期中提取年份")]
	[Order(128)]
	public Operand Year([ParameterName("日期参数")][Description("从中提取年份的日期值")] Operand date)
	{
		DateOperand dateOperand = date.ToDate();
		if (dateOperand.Value == DateOperand.Zero.Value)
		{
			return StringOperand.Empty;
		}
		return dateOperand.Value.Year;
	}

	[Category("日期")]
	[DisplayName("两日期间隔年数")]
	[Description("计算两日期间隔的年份数")]
	[Order(143)]
	public Operand Years([ParameterName("日期参数1")][Description("起始日期")] Operand startDate, [ParameterName("日期参数2")][Description("结束日期")] Operand endDate)
	{
		DateOperand dateOperand = endDate.ToDate();
		DateOperand dateOperand2 = startDate.ToDate();
		if (dateOperand.Value == DateOperand.Zero.Value || dateOperand2.Value == DateOperand.Zero.Value)
		{
			return StringOperand.Empty;
		}
		return dateOperand.Value.Year - dateOperand2.Value.Year;
	}

	[Category("日期")]
	[DisplayName("从日期中提取月份")]
	[Description("从日期中提取月份")]
	[Order(160)]
	public Operand Month([ParameterName("日期参数")][Description("从中提取月份的日期值")] Operand date)
	{
		DateOperand dateOperand = date.ToDate();
		if (dateOperand.Value == DateOperand.Zero.Value)
		{
			return StringOperand.Empty;
		}
		return dateOperand.Value.Month;
	}

	[Category("日期")]
	[DisplayName("两日期间隔月数")]
	[Description("计算两日期间隔的月份数")]
	[Order(175)]
	public Operand Months([ParameterName("日期参数1")][Description("起始日期")] Operand startDate, [ParameterName("日期参数2")][Description("结束日期")] Operand endDate)
	{
		DateOperand dateOperand = endDate.ToDate();
		DateOperand dateOperand2 = startDate.ToDate();
		if (dateOperand.Value == DateOperand.Zero.Value || dateOperand2.Value == DateOperand.Zero.Value)
		{
			return StringOperand.Empty;
		}
		return (dateOperand.Value.Year - dateOperand2.Value.Year) * 12 + (dateOperand.Value.Month - dateOperand2.Value.Month);
	}

	[Category("日期")]
	[DisplayName("从日期中提取日")]
	[Description("从日期中提取出日")]
	[Order(192)]
	public Operand Day([ParameterName("日期参数")][Description("从中提取日的日期值")] Operand date)
	{
		DateOperand dateOperand = date.ToDate();
		if (dateOperand.Value == DateOperand.Zero.Value)
		{
			return StringOperand.Empty;
		}
		return dateOperand.Value.Day;
	}

	[Category("日期")]
	[DisplayName("两日期间隔天数")]
	[Description("计算两日期间隔的天数")]
	[Order(207)]
	public Operand Days([ParameterName("日期参数1")][Description("起始日期")] Operand startDate, [ParameterName("日期参数2")][Description("结束日期")] Operand endDate)
	{
		DateOperand dateOperand = endDate.ToDate();
		DateOperand dateOperand2 = startDate.ToDate();
		if (dateOperand.Value == DateOperand.Zero.Value || dateOperand2.Value == DateOperand.Zero.Value)
		{
			return StringOperand.Empty;
		}
		return (dateOperand.Value.Date - dateOperand2.Value.Date).Days;
	}

	[Category("日期")]
	[DisplayName("当前时间")]
	[Description("返回当前时间")]
	[Order(224)]
	public Operand Now()
	{
		return DateTime.Now.TimeOfDay;
	}

	[Category("日期")]
	[DisplayName("时分秒组合时间")]
	[Description("根据时、分、秒计算时间")]
	[Order(233)]
	public Operand Time([ParameterName("数值参数1")][Description("时")] Operand hour, [ParameterName("数值参数2")][Description("分")] Operand minute, [ParameterName("数值参数3")][Description("秒")] Operand second)
	{
		int num = (int)hour.ToNumber();
		int num2 = (int)minute.ToNumber();
		int num3 = (int)second.ToNumber();
		try
		{
			return new TimeSpan(num, num2, num3);
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new FormulaBadValueException($"给出的{num}时、{num2}分、{num3}秒无法组成有效的时间");
		}
	}

	[Category("日期")]
	[DisplayName("从时间中提取时")]
	[Description("从时间中提取时")]
	[Order(255)]
	public Operand Hour([ParameterName("时间参数")][Description("时间")] Operand time)
	{
		return time.ToTime().Value.Hours;
	}

	[Category("日期")]
	[DisplayName("从时间中提取分")]
	[Description("从时间中提取分")]
	[Order(265)]
	public Operand Minute([ParameterName("时间参数")][Description("时间")] Operand time)
	{
		return time.ToTime().Value.Minutes;
	}

	[Category("日期")]
	[DisplayName("从时间中提取秒")]
	[Description("从时间中提取秒")]
	[Order(275)]
	public Operand Second([ParameterName("时间参数")][Description("时间")] Operand time)
	{
		return time.ToTime().Value.Seconds;
	}

	[Category("数学")]
	[DisplayName("求和")]
	[Description("求和")]
	[Order(285)]
	public Operand Sum([ParameterName("一个或多个数值参数")][Description("要求和的单个或多个值")] params Operand[] args)
	{
		Operand sum = 0;
		foreach (Operand operand in args)
		{
			if (!(operand is CellOperand cellOperand))
			{
				if (!(operand is CellsOperand cellsOperand))
				{
					if (!(operand is StringOperand stringOperand))
					{
						if (operand is NumberOperand numberOperand)
						{
							sum = sum.Add(new NumberOperand(numberOperand.Value));
						}
						else
						{
							sum = sum.Add(new NumberOperand(0.0));
						}
					}
					else
					{
						sum = sum.Add(ParseDoubleString(stringOperand.Value));
					}
					continue;
				}
				foreach (Cell cell in cellsOperand.Cells)
				{
					SumOrSubtract(cell);
				}
			}
			else
			{
				SumOrSubtract(cellOperand.Cell);
			}
		}
		return sum;
		static Operand CellSum(Cell c)
		{
			if (c.Value is double value)
			{
				return new NumberOperand(value);
			}
			if (c.Value is string s)
			{
				return ParseDoubleString(s);
			}
			return new NumberOperand(0.0);
		}
		void SumOrSubtract(Cell c)
		{
			if (c.Row.Role == RowRole.Normal)
			{
				sum = sum.Add(CellSum(c));
			}
			else if (c.Row.Role == RowRole.Minus)
			{
				sum = sum.Subtract(CellSum(c));
			}
		}
	}

	[Category("数学")]
	[DisplayName("数组求和")]
	[Description("对数组中的数字进行求和")]
	[Order(348)]
	public Operand ArraySum([ParameterName("多个数组型数字参数")][Description("要求和的数组型数字，数组型数字是指以“|”分隔的多个数字，如\"1|2|3\"")] params Operand[] args)
	{
		Operand sum = 0;
		foreach (Operand operand in args)
		{
			if (!(operand is CellOperand cellOperand))
			{
				if (!(operand is CellsOperand cellsOperand))
				{
					if (!(operand is StringOperand stringOperand))
					{
						if (!(operand is NumberOperand numberOperand))
						{
							if (operand is ValueSetOperand valueSetOperand)
							{
								sum = sum.Add(SumStringValue(valueSetOperand.ToString()));
							}
							else
							{
								sum = sum.Add(new NumberOperand(0.0));
							}
						}
						else
						{
							sum = sum.Add(new NumberOperand(numberOperand.Value));
						}
					}
					else
					{
						sum = sum.Add(SumStringValue(stringOperand.Value));
					}
					continue;
				}
				foreach (Cell cell in cellsOperand.Cells)
				{
					SumOrSubtract(cell);
				}
			}
			else
			{
				SumOrSubtract(cellOperand.Cell);
			}
		}
		return sum;
		static Operand CellSum(Cell c)
		{
			if (c.Value is double value)
			{
				return new NumberOperand(value);
			}
			if (c.Value is string s2)
			{
				return SumStringValue(s2);
			}
			return new NumberOperand(0.0);
		}
		void SumOrSubtract(Cell c)
		{
			if (c.Row.Role == RowRole.Normal)
			{
				sum = sum.Add(CellSum(c));
			}
			else if (c.Row.Role == RowRole.Minus)
			{
				sum = sum.Subtract(CellSum(c));
			}
		}
		static double SumStringValue(string s)
		{
			if (s.Length == 0)
			{
				return 0.0;
			}
			double num = 0.0;
			string[] array = s.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			for (int j = 0; j < array.Length; j++)
			{
				string s3 = array[j].Trim();
				if (double.TryParse(s3, out var result))
				{
					num += result;
				}
			}
			return num;
		}
	}

	[AcceptExpr]
	[Category("逻辑")]
	[DisplayName("条件判断求值")]
	[Description("根据条件满足与否返回不同的值")]
	[Order(436)]
	public Operand If([ParameterName("条件参数")][Description("条件表达式")] FormulaParser.ExprContext logical_test, [ParameterName("返回值1")][Description("条件满足时的返回值")] FormulaParser.ExprContext value_if_true, [ParameterName("返回值2")][Description("条件不满足时的返回值")] FormulaParser.ExprContext value_if_false)
	{
		Operand operand = _visitor.Visit(logical_test);
		if (!operand.ToBool().Value)
		{
			return _visitor.Visit(value_if_false);
		}
		return _visitor.Visit(value_if_true);
	}

	[AcceptExpr]
	[Category("逻辑")]
	[DisplayName("条件分支求值")]
	[Description("返回首个满足条件的参数对应的返回值")]
	[Order(450)]
	public Operand Select([ParameterName("条件和返回值参数")][Description("第1、3、5...个参数是条件；相对应的返回值为第2、4、6...个参数；任一条件满足时，就会返回对应的返回值")] params FormulaParser.ExprContext[] operands)
	{
		if (operands.Length % 2 != 0)
		{
			throw new FormulaParameterCountException();
		}
		int num = operands.Length / 2;
		for (int i = 0; i < num; i++)
		{
			Operand operand = _visitor.Visit(operands[i * 2]);
			if (!(operand is ErrorOperand) && operand.ToBool().Value)
			{
				Operand operand2 = _visitor.Visit(operands[i * 2 + 1]);
				if (!(operand2 is ErrorOperand))
				{
					return operand2;
				}
			}
		}
		return StringOperand.Empty;
	}

	[Category("数学")]
	[DisplayName("计算乘积")]
	[Description("将多个数值相乘")]
	[Order(478)]
	public Operand Product([ParameterName("多个数值参数")][Description("求乘积的数值参数")] params Operand[] args)
	{
		return args.Aggregate((Operand op1, Operand op2) => op1.Multiply(op2));
	}

	[Category("数学")]
	[DisplayName("四舍五入")]
	[Description("根据指定的小数位数四舍五入")]
	[Order(488)]
	public Operand Round([ParameterName("数值参数1")][Description("要四舍五入的数值")] Operand number, [ParameterName("数值参数2")][Description("要保留的小数位数")] Operand num_digits)
	{
		NumberOperand numberOperand = number.ToNumber();
		NumberOperand numberOperand2 = num_digits.ToNumber();
		double value = numberOperand.Value;
		if (double.IsNaN(value) || double.IsInfinity(value))
		{
			return 0;
		}
		decimal num = (decimal)value;
		int num2 = (int)Math.Truncate(numberOperand2.Value);
		if (num2 < 0)
		{
			int num3 = -num2;
			long num4 = 1L;
			for (int i = 1; i <= num3; i++)
			{
				num4 *= 10;
			}
			long num5 = (long)num / num4;
			long num6 = (long)num % num4;
			if (num6 < 5)
			{
				return num5 * num4;
			}
			return num5 * num4 + num4;
		}
		return (double)Math.Round(num, num2, MidpointRounding.AwayFromZero);
	}

	[Category("数学")]
	[DisplayName("进位取整")]
	[Description("指定小数位后有尾数时，一律进位")]
	[Order(531)]
	public Operand RoundUp([ParameterName("数值参数1")][Description("要进位取整的数值")] Operand number, [ParameterName("数值参数2")][Description("要保留的小数位数")] Operand num_digits)
	{
		NumberOperand numberOperand = number.ToNumber();
		NumberOperand numberOperand2 = num_digits.ToNumber();
		double value = numberOperand.Value;
		if (double.IsNaN(value) || double.IsInfinity(value))
		{
			return 0;
		}
		decimal num = (decimal)value;
		int num2 = (int)Math.Truncate(numberOperand2.Value);
		if (num2 == 0)
		{
			double num3 = Math.Truncate(numberOperand.Value);
			if (IsDoubleEqualInRoundFunction(numberOperand.Value, num3))
			{
				return num3;
			}
			double num4 = ((numberOperand.Value < 0.0) ? (-1) : ((numberOperand.Value != 0.0) ? 1 : 0));
			return num3 + num4 * 1.0;
		}
		if (num2 < 0)
		{
			int num5 = -num2;
			long num6 = 1L;
			for (int i = 1; i <= num5; i++)
			{
				num6 *= 10;
			}
			double num7 = numberOperand.Value / (double)num6;
			double num8 = Math.Truncate(num7);
			double num9 = ((numberOperand.Value < 0.0) ? (-1) : ((numberOperand.Value != 0.0) ? 1 : 0));
			if (IsDoubleEqualInRoundFunction(num7, num8))
			{
				return num8 * (double)num6;
			}
			return (num8 + num9 * 1.0) * (double)num6;
		}
		int num10 = num2;
		long num11 = 1L;
		for (int j = 1; j <= num10; j++)
		{
			num11 *= 10;
		}
		double num12 = numberOperand.Value * (double)num11;
		double num13 = Math.Truncate(num12);
		double num14 = ((numberOperand.Value < 0.0) ? (-1) : ((numberOperand.Value != 0.0) ? 1 : 0));
		if (IsDoubleEqualInRoundFunction(num12, num13))
		{
			return num13 / (double)num11;
		}
		return (num13 + num14 * 1.0) / (double)num11;
	}

	[Category("数学")]
	[DisplayName("舍位取整")]
	[Description("指定小数位后有尾数时，一律舍去")]
	[Order(602)]
	public Operand RoundDown([ParameterName("数值参数1")][Description("要舍位取整的数值")] Operand number, [ParameterName("数值参数2")][Description("要保留的小数位数")] Operand num_digits)
	{
		NumberOperand numberOperand = number.ToNumber();
		NumberOperand numberOperand2 = num_digits.ToNumber();
		double value = numberOperand.Value;
		if (double.IsNaN(value) || double.IsInfinity(value))
		{
			return 0;
		}
		decimal num = (decimal)value;
		int num2 = (int)Math.Truncate(numberOperand2.Value);
		if (num2 == 0)
		{
			return Math.Truncate(numberOperand.Value);
		}
		if (num2 < 0)
		{
			int num3 = -num2;
			long num4 = 1L;
			for (int i = 1; i <= num3; i++)
			{
				num4 *= 10;
			}
			double d = (double)(num / (decimal)num4);
			double num5 = Math.Truncate(d);
			return num5 * (double)num4;
		}
		int num6 = num2;
		long num7 = 1L;
		for (int j = 1; j <= num6; j++)
		{
			num7 *= 10;
		}
		double d2 = (double)(num * (decimal)num7);
		double num8 = Math.Truncate(d2);
		return num8 / (double)num7;
	}

	[Category("数学")]
	[DisplayName("计算余数")]
	[Description("计算余数")]
	[Order(650)]
	public Operand Mod([ParameterName("数值参数1")][Description("被除数")] Operand Number, [ParameterName("数值参数2")][Description("除数")] Operand Divisor)
	{
		NumberOperand numberOperand = Number.ToNumber();
		NumberOperand numberOperand2 = Divisor.ToNumber();
		double value = numberOperand.Value;
		double value2 = numberOperand2.Value;
		if (value2 == 0.0)
		{
			throw new FormulaBadValueException("除数不能为0");
		}
		Math.DivRem((int)value, (int)value2, out var result);
		return result;
	}

	[Category("数学")]
	[DisplayName("计算商整数")]
	[Description("计算出商的整数部分")]
	[Order(670)]
	public Operand Quotient([ParameterName("数值参数1")][Description("被除数")] Operand numerator, [ParameterName("数值参数2")][Description("除数")] Operand denominator)
	{
		NumberOperand numberOperand = numerator.ToNumber();
		NumberOperand numberOperand2 = denominator.ToNumber();
		double value = numberOperand.Value;
		double value2 = numberOperand2.Value;
		if (value2 == 0.0)
		{
			throw new FormulaBadValueException("除数不能为0");
		}
		int result;
		int num = Math.DivRem((int)value, (int)value2, out result);
		return num;
	}

	[Category("数学")]
	[DisplayName("计算平方根")]
	[Description("计算平方根")]
	[Order(691)]
	public Operand Sqrt([ParameterName("数值参数")][Description("要计算平方根的数值")] Operand number)
	{
		NumberOperand numberOperand = number.ToNumber();
		double value = numberOperand.Value;
		if (value < 0.0)
		{
			throw new FormulaBadValueException("不能计算负数的平方根");
		}
		return Math.Sqrt(value);
	}

	[Category("数学")]
	[DisplayName("计算绝对值")]
	[Description("计算绝对值")]
	[Order(710)]
	public Operand Abs([ParameterName("数值参数")][Description("要计算绝对值的数值")] Operand number)
	{
		NumberOperand numberOperand = number.ToNumber();
		if (numberOperand != null)
		{
			NumberOperand numberOperand2 = numberOperand;
			return Math.Abs((double)numberOperand2);
		}
		return numberOperand;
	}

	[Category("文本")]
	[DisplayName("全角转半角")]
	[Description("将全角文本转换成半角文本")]
	[Order(723)]
	public Operand Asc([ParameterName("文本参数")][Description("要转换的文本")] Operand text)
	{
		string text2 = text.ToString();
		char[] array = text2.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == '\u3000')
			{
				array[i] = ' ';
			}
			else if (array[i] > '\uff00' && array[i] < '｟')
			{
				array[i] = (char)(array[i] - 65248);
			}
		}
		return new string(array);
	}

	[Category("文本")]
	[DisplayName("半角转全角")]
	[Description("将半角文本转换成全角文本")]
	[Order(747)]
	public Operand WideChar([ParameterName("文本参数")][Description("要转换的文本")] Operand text)
	{
		string text2 = text.ToString();
		char[] array = text2.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == ' ')
			{
				array[i] = '\u3000';
			}
			else if (array[i] < '\u007f')
			{
				array[i] = (char)(array[i] + 65248);
			}
		}
		return new string(array);
	}

	[Category("文本")]
	[DisplayName("拼接文本")]
	[Description("将多个文本拼接为一个文本")]
	[Order(771)]
	public Operand Concat([ParameterName("多个文本参数")][Description("多个要拼接的文本")] params Operand[] args)
	{
		return args.Aggregate((Operand op1, Operand op2) => op1.Concatenate(op2));
	}

	[Category("文本")]
	[DisplayName("由左开始截取文本")]
	[Description("从一个文本的第一个字符开始，截取指定数目的文本，也支持对数组型文本中的每一个元素进行截取")]
	[Order(781)]
	public Operand Left([ParameterName("文本参数")][Description("要截取的文本或数组型文本")] Operand text, [ParameterName("数值参数")][Description("指定要从左所提取的字符数，必须大于或等于0")] Operand num_chars)
	{
		string text2 = text.ToString();
		NumberOperand numberOperand = num_chars.ToNumber();
		int num = (int)numberOperand.Value;
		if (num < 0)
		{
			throw new FormulaBadValueException("要提取的字符数不能为负数");
		}
		return string.Join("|", from s in text2.Split('|')
			select (num <= s.Length) ? s.Substring(0, num) : s);
	}

	[Category("文本")]
	[DisplayName("由右开始截取文本")]
	[Description("从一个文本的最后一个字符开始，截取指定数目的文本，也支持对数组型文本中的每一个元素进行截取")]
	[Order(810)]
	public Operand Right([ParameterName("文本参数")][Description("要截取的文本或数组型文本")] Operand text, [ParameterName("数值参数")][Description("指定要从右所提取的字符数，必须大于或等于0")] Operand num_chars)
	{
		string text2 = text.ToString();
		NumberOperand numberOperand = num_chars.ToNumber();
		int num = (int)numberOperand.Value;
		if (num < 0)
		{
			throw new FormulaBadValueException("要提取的字符数不能为负数");
		}
		return string.Join("|", from s in text2.Split('|')
			select (num <= s.Length) ? s.Substring(s.Length - num, num) : s);
	}

	[Category("文本")]
	[DisplayName("指定位置截取文本")]
	[Description("从一个文本的指定位置开始，截取指定数目的文本，也支持对数组型文本中的每一个元素进行截取")]
	[Order(839)]
	public Operand Mid([ParameterName("文本参数")][Description("要截取的文本或数组型文本")] Operand text, [ParameterName("数值参数1")][Description("文本中要提取的第一个字符的位置")] Operand start_num, [ParameterName("数值参数2")][Description("要截取的字符个数")] Operand num_chars)
	{
		string text2 = text.ToString();
		int start = (int)start_num.ToNumber();
		int num = (int)num_chars.ToNumber();
		if (num < 0)
		{
			throw new FormulaBadValueException("要提取的字符数不能为负数");
		}
		return string.Join("|", from s in text2.Split('|').Select(delegate(string s)
			{
				if (start == 0 || start > s.Length)
				{
					return "";
				}
				if (start > 0)
				{
					if (start - 1 + num <= s.Length)
					{
						return s.Substring(start - 1, num);
					}
					return s.Substring(start - 1);
				}
				if (num + start + s.Length < 0)
				{
					return "";
				}
				if (-start > s.Length)
				{
					if (start + num <= 0)
					{
						return s.Substring(0, num + start + s.Length);
					}
					return s;
				}
				return (start + num <= 0) ? s.Substring(start + s.Length, num) : s.Substring(start + s.Length);
			})
			where s != ""
			select s);
	}

	[Category("文本")]
	[DisplayName("小写转大写")]
	[Description("将所有英文字母转换成大写字母")]
	[Order(906)]
	public Operand Upper([ParameterName("文本参数")][Description("要转换成大写字母的文本")] Operand text)
	{
		string text2 = text.ToString();
		return text2.ToUpper();
	}

	[Category("文本")]
	[DisplayName("大写转小写")]
	[Description("将所有英文字母转换成小写字母")]
	[Order(917)]
	public Operand Lower([ParameterName("文本参数")][Description("要转换为小写字母的文本")] Operand text)
	{
		string text2 = text.ToString();
		return text2.ToLower();
	}

	[Category("文本")]
	[DisplayName("首字转大写")]
	[Description("将英文单词的开头字母转换成大写字母")]
	[Order(928)]
	public Operand Proper([ParameterName("文本参数")][Description("要转换的文本")] Operand text)
	{
		string str = text.ToString();
		return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str);
	}

	[Category("文本")]
	[DisplayName("计算文本字数")]
	[Description("计算文本字数")]
	[Order(939)]
	public Operand Len([ParameterName("文本参数")][Description("要计算的文本")] Operand text)
	{
		string text2 = text.ToString();
		return text2.Length;
	}

	[Category("文本")]
	[DisplayName("计算子文本个数")]
	[Description("计算文本中指定字符出现的次数")]
	[Order(950)]
	public Operand CountSub([ParameterName("文本参数1")][Description("要计算的文本")] Operand text, [ParameterName("文本参数2")][Description("要查找的文本")] Operand substring)
	{
		string text2 = text.ToString();
		string text3 = substring.ToString();
		return text2.Split(new string[1] { text3 }, StringSplitOptions.None).Length - 1;
	}

	[Category("文本")]
	[DisplayName("文本转数值")]
	[Description("将表示数值的文本转换成数值")]
	[Order(963)]
	public Operand Value([ParameterName("文本参数")][Description("要转换的文本")] Operand text)
	{
		return text.ToNumber();
	}

	[Category("文本")]
	[DisplayName("删除多余空格和分隔符")]
	[Description("删除文本首尾的空格和数组分隔符“|”，并将文本中间连续的空格字符及数组分隔符“|”删减为一个；")]
	[Order(973)]
	public Operand Trim([ParameterName("文本参数")][Description("需要清除多余空格的文本")] Operand text)
	{
		string text2 = text.ToString().Trim();
		text2 = string.Join("|", text2.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries));
		return Regex.Replace(text2, "\\b\\s+\\b", " ");
	}

	[Category("文本")]
	[DisplayName("判断两文本是否相同")]
	[Description("检查两文本是否完全相同")]
	[Order(985)]
	public Operand Exact([ParameterName("文本参数1")][Description("待比较的第一个文本")] Operand text1, [ParameterName("文本参数2")][Description("待比较的第二个文本")] Operand text2)
	{
		string a = text1.ToString();
		string b = text2.ToString();
		return string.Equals(a, b);
	}

	[Category("文本")]
	[DisplayName("格式化文本")]
	[Description("把数值、日期和时间转化为格式化文本")]
	[Order(998)]
	public Operand Format([ParameterName("任意参数")][Description("要格式化的数据")] Operand data, [ParameterName("文本参数")][Description("格式描述")] Operand format)
	{
		string text = format.ToString();
		try
		{
			if (!(data is NumberOperand { Value: var value }))
			{
				if (!(data is DateOperand { Value: var value2 }))
				{
					if (!(data is TimeOperand { Value: var value3 }))
					{
						if (!(data is DateYearMonthOperand { Value: var value4 }))
						{
							if (data is CellOperand cellOperand)
							{
								return string.Format("{0:" + text + "}", cellOperand.Cell.Value);
							}
							return data.ToString();
						}
						return value4.Date.ToString(text);
					}
					return value3.ToString(text);
				}
				return value2.ToString(text);
			}
			return value.ToString(text);
		}
		catch (FormatException)
		{
			return data.ToString();
		}
	}

	[Category("文本")]
	[DisplayName("人民币大写")]
	[Description("把数值转化为人民币大写形式")]
	[Order(1031)]
	public Operand Rmb([ParameterName("数值参数")][Description("要转化为人民币大写的数值")] Operand data)
	{
		NumberOperand numberOperand = data.ToNumber();
		return NumberingHelper.NumToRmbBig(numberOperand.Value);
	}

	[Category("数学")]
	[DisplayName("计算平均值")]
	[Description("计算平均值")]
	[Order(1042)]
	public Operand Average([ParameterName("多个数值参数")][Description("要计算平均值的多个数值")] params Operand[] args)
	{
		Operand operand = 0;
		int num = 0;
		foreach (Operand operand2 in args)
		{
			if (!(operand2 is CellOperand cellOperand))
			{
				if (operand2 is CellsOperand cellsOperand)
				{
					foreach (Cell cell in cellsOperand.Cells)
					{
						ValueOperand valueOperand = ValueOperand.FromObject(cell.Value);
						if (NeedSum(valueOperand))
						{
							operand = operand.Add(valueOperand);
							num++;
						}
					}
				}
				else
				{
					operand = operand.Add(operand2);
					num++;
				}
			}
			else
			{
				num++;
				if (NeedSum(cellOperand.Value))
				{
					operand = operand.Add(cellOperand.Value);
				}
			}
		}
		return (double)operand.ToNumber() / (double)num;
		static bool NeedSum(ValueOperand v)
		{
			return v is NumberOperand;
		}
	}

	[Category("数学")]
	[DisplayName("找出最大值")]
	[Description("找出最大值")]
	[Order(1081)]
	public Operand Max([ParameterName("多个数值参数")][Description("要从中找出最大值的多个数值")] params Operand[] args)
	{
		if (args.Length == 0)
		{
			return 0.0;
		}
		int valueTypeFlag = 0;
		double num = args.Max((Operand a) => GetValue(a));
		if (valueTypeFlag == 8)
		{
			if (double.IsNegativeInfinity(num))
			{
				return DateYearMonthOperand.Zero;
			}
			return ValueOperand.FromObject(num).ToDateYearMonth();
		}
		if (valueTypeFlag == 4)
		{
			if (double.IsNegativeInfinity(num))
			{
				return DateOperand.Zero;
			}
			return ValueOperand.FromObject(num).ToDate();
		}
		if (valueTypeFlag == 16)
		{
			if (double.IsNegativeInfinity(num))
			{
				return new TimeOperand(TimeSpan.Zero);
			}
			return ValueOperand.FromObject(num).ToTime();
		}
		return double.IsNegativeInfinity(num) ? 0.0 : num;
		static double GetStringValue(string s)
		{
			return s.Split('|').Max(delegate(string s0)
			{
				Match match = Regex.Match(s0, "\\d+");
				return match.Success ? double.Parse(match.Value) : double.NegativeInfinity;
			});
		}
		double GetValue(Operand v)
		{
			if (v is CellOperand cellOperand)
			{
				if (cellOperand.Cell.Row.Role != 0 && cellOperand.Cell.Row.Role != RowRole.Among && cellOperand.Cell.Row.Role != RowRole.Minus)
				{
					return double.NegativeInfinity;
				}
				return GetValue(cellOperand.Value);
			}
			if (v is CellsOperand cellsOperand)
			{
				if (cellsOperand.Cells.Count != 0)
				{
					return cellsOperand.Cells.Max((Cell c) => (c.Row.Role != 0 && c.Row.Role != RowRole.Among && c.Row.Role != RowRole.Minus) ? double.NegativeInfinity : GetValue(ValueOperand.FromObject(c.Value)));
				}
				return double.NegativeInfinity;
			}
			if (v is NumberOperand numberOperand)
			{
				valueTypeFlag |= 1;
				return numberOperand.Value;
			}
			if (v is StringOperand stringOperand)
			{
				if (!string.IsNullOrWhiteSpace(stringOperand.Value))
				{
					valueTypeFlag |= 2;
				}
				return GetStringValue(stringOperand.Value);
			}
			if (v is ValueSetOperand valueSetOperand)
			{
				if (valueSetOperand.Set.Count != 0)
				{
					return valueSetOperand.Set.Max((Tuple<Row, ValueOperand> tup) => GetValue(tup.Item2));
				}
				return double.NegativeInfinity;
			}
			if (v is DateOperand dateOperand)
			{
				valueTypeFlag |= 4;
				return dateOperand.Value.Subtract(DateTime.MinValue).Days;
			}
			if (v is DateYearMonthOperand dateYearMonthOperand)
			{
				valueTypeFlag |= 8;
				return (dateYearMonthOperand.Value.Date.Year - DateTime.MinValue.Year) * 12 + (dateYearMonthOperand.Value.Date.Month - DateTime.MinValue.Month);
			}
			if (v is TimeOperand timeOperand)
			{
				valueTypeFlag |= 16;
				return timeOperand.Value.Ticks;
			}
			return double.NegativeInfinity;
		}
	}

	[Category("数学")]
	[DisplayName("找出最小值")]
	[Description("找出最小值")]
	[Order(1179)]
	public Operand Min([ParameterName("多个数值参数")][Description("要从中找出最小值的多个数值")] params Operand[] args)
	{
		if (args.Length == 0)
		{
			return 0.0;
		}
		int valueTypeFlag = 0;
		double num = args.Min((Operand a) => GetValue(a));
		if (valueTypeFlag == 8)
		{
			if (double.IsNegativeInfinity(num))
			{
				return DateYearMonthOperand.Zero;
			}
			return ValueOperand.FromObject(num).ToDateYearMonth();
		}
		if (valueTypeFlag == 4)
		{
			if (double.IsNegativeInfinity(num))
			{
				return DateOperand.Zero;
			}
			return ValueOperand.FromObject(num).ToDate();
		}
		if (valueTypeFlag == 16)
		{
			if (double.IsNegativeInfinity(num))
			{
				return new TimeOperand(TimeSpan.Zero);
			}
			return ValueOperand.FromObject(num).ToTime();
		}
		return double.IsPositiveInfinity(num) ? 0.0 : num;
		static double GetStringValue(string s)
		{
			return s.Split('|').Min(delegate(string s0)
			{
				Match match = Regex.Match(s0, "\\d+");
				return match.Success ? double.Parse(match.Value) : double.PositiveInfinity;
			});
		}
		double GetValue(Operand v)
		{
			if (v is CellOperand cellOperand)
			{
				if (cellOperand.Cell.Row.Role != 0 && cellOperand.Cell.Row.Role != RowRole.Among && cellOperand.Cell.Row.Role != RowRole.Minus)
				{
					return double.PositiveInfinity;
				}
				return GetValue(cellOperand.Value);
			}
			if (v is CellsOperand cellsOperand)
			{
				if (cellsOperand.Cells.Count != 0)
				{
					return cellsOperand.Cells.Min((Cell c) => (c.Row.Role != 0 && c.Row.Role != RowRole.Among && c.Row.Role != RowRole.Minus) ? double.PositiveInfinity : GetValue(ValueOperand.FromObject(c.Value)));
				}
				return double.PositiveInfinity;
			}
			if (v is NumberOperand numberOperand)
			{
				valueTypeFlag |= 1;
				return numberOperand.Value;
			}
			if (v is StringOperand stringOperand)
			{
				if (!string.IsNullOrWhiteSpace(stringOperand.Value))
				{
					valueTypeFlag |= 2;
				}
				return GetStringValue(stringOperand.Value);
			}
			if (v is ValueSetOperand valueSetOperand)
			{
				if (valueSetOperand.Set.Count != 0)
				{
					return valueSetOperand.Set.Min((Tuple<Row, ValueOperand> tup) => GetValue(tup.Item2));
				}
				return double.PositiveInfinity;
			}
			if (v is DateOperand dateOperand)
			{
				valueTypeFlag |= 4;
				return dateOperand.Value.Subtract(DateTime.MinValue).Days;
			}
			if (v is DateYearMonthOperand dateYearMonthOperand)
			{
				valueTypeFlag |= 8;
				return (dateYearMonthOperand.Value.Date.Year - DateTime.MinValue.Year) * 12 + (dateYearMonthOperand.Value.Date.Month - DateTime.MinValue.Month);
			}
			if (v is TimeOperand timeOperand)
			{
				valueTypeFlag |= 16;
				return timeOperand.Value.Ticks;
			}
			return double.PositiveInfinity;
		}
	}

	[Category("日期")]
	[DisplayName("求某月天数")]
	[Description("根据一个日期，得到该日期所在月份的天数")]
	[Order(1277)]
	public Operand DaysInMonth([ParameterName("日期参数")][Description("要求月份天数的日期值")] Operand o)
	{
		DateOperand dateOperand = o.ToDate();
		if (dateOperand == DateOperand.Zero)
		{
			return StringOperand.Empty;
		}
		try
		{
			return ValueOperand.FromObject(DateTime.DaysInMonth(dateOperand.Value.Year, dateOperand.Value.Month));
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new FormulaBadValueException("DaysInMonth函数的参数无效");
		}
	}

	[Category("统计")]
	[DisplayName("去重填充")]
	[Description("把一列或多列的数据去掉重复值，并填充到目标列，且目标列行次自动生成。该函数主要用于跨表去重筛选，主要适用于列公式，列公式应用举例如：LqDistinct({被统表1}[项目],{被统表2}[项目])")]
	[Order(1299)]
	public Operand LqDistinct([ParameterName("任意个列参数")][Description("一列或多列的来源列")] params Operand[] operands)
	{
		return ValueSetOperand.UnionAll(operands.Select((Operand o) => o.ToValueSetOrderByRowIndex()));
	}

	[Category("统计")]
	[DisplayName("去重填充")]
	[Description("将某列的值去掉重复项后填充到目标列，公式举例如：Distinct({销售明细表}[产品名称])")]
	[Order(1309)]
	public Operand Distinct([ParameterName("列参数")][Description("一列或多列的来源列")] params Operand[] operands)
	{
		return LqDistinct(operands);
	}

	[Category("统计")]
	[DisplayName("筛选去重填充")]
	[Description("筛选满足条件的行，然后将某列的值去掉重复项后填充到目标列，公式举例如：DistinctF({销售明细表}[产品大类]=\"家电\",{销售明细表}[产品名称])")]
	[Order(1319)]
	public Operand DistinctF([ParameterName("条件列参数，查找列参数")][Description("条件列参数指某列的条件表达式；查找列参数指要查找并返回的某列。")] params Operand[] operands)
	{
		return LqFilter(operands);
	}

	[Category("统计")]
	[DisplayName("筛选填充")]
	[Description("筛选条件列中满足条件的行，并将这些行对应的查找列的内容填充到目标列，且目标列行次自动生成。该函数主要用于跨表条件筛选，主要适用于列公式，列公式应用举例如：LqFilter({被筛选表}[金额]>=1000000,{被筛选表}[项目])")]
	[Order(1329)]
	public Operand LqFilter([ParameterName("条件列参数，查找列参数")][Description("条件列参数指某列的条件表达式；查找列参数指要查找并返回的某列。")] params Operand[] operands)
	{
		if (operands.Length % 2 != 0)
		{
			throw new FormulaParameterCountException();
		}
		int num = operands.Length / 2;
		List<ValueSetOperand> list = new List<ValueSetOperand>();
		for (int i = 0; i < num; i++)
		{
			if (operands[i * 2] is ErrorOperand || operands[i * 2 + 1] is ErrorOperand)
			{
				continue;
			}
			CellsOperand cellsOperand = operands[i * 2] as CellsOperand;
			CellsOperand column = operands[i * 2 + 1] as CellsOperand;
			if (cellsOperand == null || column == null)
			{
				continue;
			}
			try
			{
				List<Cell> list2 = cellsOperand.Cells.Select((Cell c) => column.GetCellByRowIndex(c.Row.Index)).ToList();
				list2.Sort((Cell left, Cell right) => left.Row.Index.CompareTo(right.Row.Index));
				list.Add(new CellsOperand(list2, cellsOperand.Table).ToValueSet());
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new FormulaNotApplicableException("条件列和对应的查找列必须属于同一表格。");
			}
		}
		return ValueSetOperand.UnionAll(list);
	}

	[Category("统计")]
	[DisplayName("条件找值")]
	[Description("筛选满足条件的行，返回这些行中某列的第一个值。公式举例如：VLookUp({销售明细表}[产品名称]=[产品名称,*],{销售明细表}[计量单位])")]
	[Order(1365)]
	public Operand VLookUp([ParameterName("条件列参数，查找列参数")][Description("条件列参数指某列的条件表达式；查找列参数指要查找并返回的某列。")] params Operand[] operands)
	{
		return LqVLookUp(operands);
	}

	[Category("统计")]
	[DisplayName("条件找值")]
	[Description("筛选条件列中满足条件的行，并返回该行的查找列的内容。该函数主要用于跨表查找匹配项，且更适用于列公式，列公式应用举例如：LqVLookUp({被查找表}[项目]=[项目,*],{被查找表}[查找并返回的列])")]
	[Order(1375)]
	public Operand LqVLookUp([ParameterName("条件列参数，查找列参数")][Description("条件列参数指某列的条件表达式；查找列参数指要查找并返回的某列。")] params Operand[] operands)
	{
		if (operands.Length % 2 != 0)
		{
			throw new FormulaParameterCountException();
		}
		int num = operands.Length / 2;
		for (int i = 0; i < num; i++)
		{
			if (operands[i * 2] is ErrorOperand || operands[i * 2 + 1] is ErrorOperand)
			{
				continue;
			}
			CellsOperand cellsOperand = operands[i * 2] as CellsOperand;
			CellsOperand cellsOperand2 = operands[i * 2 + 1] as CellsOperand;
			if (cellsOperand == null || cellsOperand2 == null)
			{
				continue;
			}
			if (cellsOperand.Cells.Count <= 0)
			{
				continue;
			}
			try
			{
				int index = cellsOperand.Cells[0].Row.Index;
				Cell cellByRowIndex = cellsOperand2.GetCellByRowIndex(index);
				if (cellByRowIndex != null)
				{
					return ValueOperand.FromObject(cellByRowIndex.Value);
				}
				return ValueOperand.FromObject(string.Empty);
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new FormulaNotApplicableException("VLookUp函数运算错误，返回列中没有对应单元格。");
			}
		}
		return ValueOperand.FromObject(string.Empty);
	}

	[Category("统计")]
	[DisplayName("条件汇总")]
	[Description("筛选满足条件的行，返回这些行中某列值的总和。公式举例如：SumIf({销售明细表}[产品名称]=[产品名称,*],{销售明细表}[销售数量])")]
	[Order(1420)]
	public Operand SumIf([ParameterName("条件列参数，汇总列参数")][Description("条件列参数指某列的条件表达式；汇总列参数要汇总的某数据列")] params Operand[] operands)
	{
		return LqSumIf(operands);
	}

	[Category("统计")]
			[DisplayName("条件汇总")]
			[Description("筛选条件列中满足条件的行，并对这些行的汇总列加总。该函数主要用于跨表条件汇总，且更适用于列公式，列公式应用举例如：LqSumif({被统表}[项目]=[项目,*],{被统表}[金额])")]
			[Order(1430)]
			public Operand LqSumIf([ParameterName("条件列参数，汇总列参数")][Description("条件列参数指某列的条件表达式；汇总列参数要汇总的某数据列")] params Operand[] operands)
			{
				if (operands.Length == 0 || operands.Length % 2 != 0)
				{
					throw new FormulaParameterCountException();
				}
				int num = operands.Length / 2;
				ValueOperand valueOperand = ValueOperand.FromObject(0);
				for (int i = 0; i < num; i++)
				{
					// 尝试将非 CellsOperand 类型转换为 CellsOperand
					if (operands[i * 2] is ValueSetOperand)
					{
						continue;
					}
					else if (operands[i * 2] is ErrorOperand)
					{
						continue;
					}
					if (operands[i * 2 + 1] is ErrorOperand)
					{
						continue;
					}
					CellsOperand cellsOperand = operands[i * 2] as CellsOperand;
					CellsOperand cellsOperand2 = operands[i * 2 + 1] as CellsOperand;
					if (cellsOperand == null || cellsOperand2 == null)
					{
						// 如果条件参数无法转换为 CellsOperand，跳过该组
						if (cellsOperand == null)
						{
							continue;
						}
						// 如果汇总列参数无法转换为 CellsOperand，创建一个空结果
						continue;
					}
			if (cellsOperand2 is ColumnOperand columnOperand)
			{
				if (cellsOperand.Cells == null)
				{
					continue;
				}
				foreach (Cell cell2 in cellsOperand.Cells)
				{
					Cell cell = columnOperand.Table[cell2.Row.Index, columnOperand.Column.Index];
					if (cell == null)
					{
						continue;
					}
					if (cell2.Row.Role == RowRole.Normal)
					{
						valueOperand = valueOperand.Add(ValueOperand.FromObject(cell.Value));
					}
					else if (cell2.Row.Role == RowRole.Minus)
					{
						valueOperand = valueOperand.Subtract(ValueOperand.FromObject(cell.Value));
					}
				}
				continue;
			}
			HashSet<int> rows = cellsOperand.Rows;
			foreach (Cell cell3 in cellsOperand2.Cells)
			{
				if (rows.Contains(cell3.Row.Index))
				{
					if (cell3.Row.Role == RowRole.Normal)
					{
						valueOperand = valueOperand.Add(ValueOperand.FromObject(cell3.Value));
					}
					else if (cell3.Row.Role == RowRole.Minus)
					{
						valueOperand = valueOperand.Subtract(ValueOperand.FromObject(cell3.Value));
					}
				}
			}
		}
		return valueOperand;
	}

	[Category("统计")]
	[DisplayName("条件计数")]
	[Description("筛选满足条件的行，返回这些行的行数。公式举例如：CountIf({销售明细表}[产品名称]=[产品名称,*])")]
	[Order(1500)]
	public Operand CountIf([ParameterName("条件列参数")][Description("某列的条件表达式")] CellsOperand cells)
	{
		return LqCountIf(cells);
	}

	[Category("统计")]
	[DisplayName("不去重填充")]
	[Description("将某列的值不去重复项填充到目标列，公式举例如：Collect({销售明细表}[产品名称])")]
	[Order(1510)]
	public Operand Collect([ParameterName("列参数")][Description("列参数指要查找并返回的某列。")] params Operand[] operands)
	{
		if (operands.Length == 0)
		{
			throw new FormulaParameterCountException();
		}
		CellsOperand cellsOperand = null;
		for (int i = 0; i < operands.Length; i++)
		{
			if (operands[i] is ErrorOperand)
			{
				continue;
			}
			if (!(operands[i] is CellsOperand cellsOperand2))
			{
				continue;
			}
			try
			{
				CellsOperand cellsOperand3 = new CellsOperand(cellsOperand2.Cells.Where((Cell c) => c.Row.Role == RowRole.Normal || c.Row.Role == RowRole.Among || c.Row.Role == RowRole.Minus).ToList(), cellsOperand2.Table);
				if (cellsOperand3.Cells != null)
				{
					cellsOperand3.Cells.Sort(delegate(Cell left, Cell right)
					{
						int index = left.Row.Index;
						int index2 = right.Row.Index;
						if (index < index2)
						{
							return -1;
						}
						return (index > index2) ? 1 : 0;
					});
				}
				cellsOperand = ((cellsOperand != null) ? ((CellsOperand)cellsOperand.Concatenate(cellsOperand3)) : cellsOperand3);
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new FormulaColumnWildcardNoRowException();
			}
		}
		if (cellsOperand == null)
		{
			cellsOperand = new CellsOperand(new List<Cell>(), _env.HostTable);
		}
		cellsOperand.IsCollectFill = true;
		return cellsOperand;
	}

	[Category("统计")]
	[DisplayName("筛选不去重填充")]
	[Description("筛选满足条件的行，然后将某列的值不去掉重复项填充到目标列，公式举例如：CollectF({销售明细表}[产品大类]=\"家电\",{销售明细表}[产品名称])")]
	[Order(1574)]
	public Operand CollectF([ParameterName("条件列参数，查找列参数")][Description("条件列参数指某列的条件表达式；查找列参数指要查找并返回的某列。")] params Operand[] operands)
	{
		return LqCollect(operands);
	}

	[Category("统计")]
	[DisplayName("合并填充")]
	[Description("筛选条件列中满足条件的行，并将这些行对应的查找列的内容填充到目标列，且目标列行次自动生成。")]
	[Order(1584)]
	public Operand LqCollect([ParameterName("条件列参数，查找列参数")][Description("条件列参数指某列的条件表达式；查找列参数指要查找并返回的某列。")] params Operand[] operands)
	{
		if (operands.Length == 0 || operands.Length % 2 != 0)
		{
			throw new FormulaParameterCountException();
		}
		int num = operands.Length / 2;
		CellsOperand cellsOperand = null;
		for (int i = 0; i < num; i++)
		{
			if (operands[i * 2] is ErrorOperand || operands[i * 2 + 1] is ErrorOperand)
			{
				continue;
			}
			CellsOperand cellsOperand2 = operands[i * 2] as CellsOperand;
			CellsOperand column = operands[i * 2 + 1] as CellsOperand;
			if (cellsOperand2 == null || column == null)
			{
				continue;
			}
			try
			{
				CellsOperand cellsOperand3 = new CellsOperand((from c in cellsOperand2.Cells
					where c.Row.Role == RowRole.Normal || c.Row.Role == RowRole.Among || c.Row.Role == RowRole.Minus
					select column.GetCellByRowIndex(c.Row.Index)).ToList(), cellsOperand2.Table);
				if (cellsOperand3.Cells != null)
				{
					cellsOperand3.Cells.Sort(delegate(Cell left, Cell right)
					{
						int index = left.Row.Index;
						int index2 = right.Row.Index;
						if (index < index2)
						{
							return -1;
						}
						return (index > index2) ? 1 : 0;
					});
				}
				cellsOperand = ((cellsOperand != null) ? ((CellsOperand)cellsOperand.Concatenate(cellsOperand3)) : cellsOperand3);
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new FormulaNotApplicableException("条件列和对应的查找列必须属于同一表格。");
			}
		}
		cellsOperand.IsCollectFill = true;
		return cellsOperand;
	}

	[Category("统计")]
	[DisplayName("升序去重填充")]
	[Description("将某列的值去掉重复项后，升序排序填充到目标列，公式举例如：DistinctUP({销售明细表}[产品代码])")]
	[Order(1649)]
	public Operand DistinctUp([ParameterName("多个参数")][Description("要升序排序的多个参数")] params Operand[] operands)
	{
		return LqAsc(operands);
	}

	[Category("统计")]
	[DisplayName("升序排序填充")]
	[Description("将所有参数按照升序排序，并填充到目标列")]
	[Order(1659)]
	public Operand LqAsc([ParameterName("多个参数")][Description("要升序排序的多个参数")] params Operand[] operands)
	{
		return LqDistinct(operands);
	}

	[Category("统计")]
	[DisplayName("降序去重填充")]
	[Description("将某列的值去掉重复项后，降序排序填充到目标列，公式举例如：DistinctDown({销售明细表}[产品代码])")]
	[Order(1669)]
	public Operand DistinctDown([ParameterName("多个参数")][Description("要降序排序的多个参数")] params Operand[] operands)
	{
		return LqDesc(operands);
	}

	[Category("统计")]
	[DisplayName("降序排序填充")]
	[Description("将所有参数按照降序排序，并填充到目标列")]
	[Order(1679)]
	public Operand LqDesc([ParameterName("多个参数")][Description("要降序排序的多个参数")] params Operand[] operands)
	{
		return LqDistinct(operands);
	}

	[Category("统计")]
	[DisplayName("筛选最大值填充")]
	[Description("筛选数值最大的n行，然后将某列的值填充到目标列，公式举例如：MaxF({客户欠款明细表}[欠款余额],5,{客户欠款明细表}[客户名称])")]
	[Order(1689)]
	public Operand MaxF([ParameterName("列参数1")][Description("要从哪列筛选最大值")] CellsOperand condition, [ParameterName("数值参数")][Description("要筛选的最大值个数")] Operand count, [ParameterName("列参数2")][Description("要填充的列")] CellsOperand source)
	{
		return LqMax(condition, count, source);
	}

	[Category("统计")]
	[DisplayName("筛选最大值填充")]
	[Description("筛选出某列1中数值最大的n行，并将这些行对应的某列2的内容填充到目标列，且目标列行次自动生成。该函数主要适用于列公式，列公式应用举例如：LqMax({被筛选表}[金额],5,{被筛选表}[项目])")]
	[Order(1701)]
	public Operand LqMax([ParameterName("列参数1")][Description("要从哪列筛选最大值")] CellsOperand condition, [ParameterName("数值参数")][Description("要筛选的最大值个数")] Operand count, [ParameterName("列参数2")][Description("要填充的列")] CellsOperand source)
	{
		try
		{
			IEnumerable<Tuple<Row, ValueOperand>> values = from c in condition.Cells.Where((Cell c) => c.Row.Role == RowRole.Normal || c.Row.Role == RowRole.Among || c.Row.Role == RowRole.Minus).OrderByDescending((Cell c) => c.Value, CellValueSortComparer.Instance).Take((int)count.ToNumber().Value)
				select Tuple.Create(c.Row, ValueOperand.FromObject(source.GetCellByRowIndex(c.Row.Index).Value));
			return new ValueSetOperand(values);
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new FormulaColumnWildcardNoRowException();
		}
	}

	[Category("统计")]
	[DisplayName("筛选最小值填充")]
	[Description("筛选数值最小的n行，然后将某列的值填充到目标列，公式举例如：MinF({客户欠款明细表}[欠款余额],5,{客户欠款明细表}[客户名称])")]
	[Order(1725)]
	public Operand MinF([ParameterName("列参数1")][Description("要从哪列筛选最小值")] CellsOperand condition, [ParameterName("数值参数")][Description("要筛选的最小值个数")] Operand count, [ParameterName("列参数2")][Description("要填充的列")] CellsOperand source)
	{
		return LqMin(condition, count, source);
	}

	[Category("统计")]
	[DisplayName("筛选最小值填充")]
	[Description("筛选出某列1中数值最小的n行，并将这些行对应的某列2的内容填充到目标列，且目标列行次自动生成。该函数主要适用于列公式，列公式应用举例如：LqMin({被筛选表}[金额],5,{被筛选表}[项目])")]
	[Order(1737)]
	public Operand LqMin([ParameterName("列参数1")][Description("要从哪列筛选最小值")] CellsOperand condition, [ParameterName("数值参数")][Description("要筛选的最小值个数")] Operand count, [ParameterName("列参数2")][Description("要填充的列")] CellsOperand source)
	{
		try
		{
			IEnumerable<Tuple<Row, ValueOperand>> values = from c in condition.Cells.Where((Cell c) => c.Row.Role == RowRole.Normal || c.Row.Role == RowRole.Among || c.Row.Role == RowRole.Minus).OrderBy((Cell c) => c.Value, CellValueSortComparer.Instance).Take((int)count.ToNumber().Value)
				select Tuple.Create(c.Row, ValueOperand.FromObject(source.GetCellByRowIndex(c.Row.Index).Value));
			return new ValueSetOperand(values);
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new FormulaColumnWildcardNoRowException();
		}
	}

	[Category("统计")]
	[DisplayName("条件提取汇总")]
	[Description("筛选条件列中满足条件的行，按给定的模糊匹配条件对汇总列（文本型列）中的数字进行提取并加总，该函数主要用于跨表模糊汇总，公式应用举例如：SumFind({被统表}[项目]=[项目,*],\"明细: *\",{被统表}[明细及金额])")]
	[Order(1761)]
	public Operand SumFind([ParameterName("条件列参数")][Description("列的条件表达式")] Operand cells, [ParameterName("文本参数")][Description("模糊匹配的文本条件，通常含有通配符“*”，“*”代表任意字符串")] StringOperand pattern, [ParameterName("汇总列参数")][Description("要汇总的某文本型列")] CellsOperand column)
	{
		return LqSumFind(cells, pattern, column);
	}

	[Category("统计")]
	[DisplayName("条件提取汇总")]
	[Description("筛选条件列中满足条件的行，按给定的模糊匹配条件对汇总列（文本型列）中的数字进行提取并加总，该函数主要用于跨表模糊汇总，公式应用举例如：LqSumFind({被统表}[项目]=[项目,*],\"明细: *\",{被统表}[明细及金额])")]
	[Order(1773)]
	public Operand LqSumFind([ParameterName("条件列参数")][Description("列的条件表达式")] Operand cells, [ParameterName("文本参数")][Description("模糊匹配的文本条件，通常含有通配符“*”，“*”代表任意字符串")] StringOperand pattern, [ParameterName("汇总列参数")][Description("要汇总的某文本型列")] CellsOperand column)
	{
		if (cells is CellsOperand cellsOperand)
		{
			string pat = pattern.ToString();
			pat = Regex.Replace(pat, "(?<=.*\\*.*)\\*", "");
			if (!pat.Contains("*"))
			{
				return 0;
			}
			pat = Regex.Escape(pat);
			if (pat.StartsWith("\\*") || pat.EndsWith("\\*"))
			{
				pat = pat.Replace("\\*", ".*");
			}
			else
			{
				pat = pat.Replace("\\*", ".*?");
			}
			HashSet<int> rows = cellsOperand.Rows;
			return ((column is ColumnOperand columnOperand) ? FormulaEvaluator.GetCells(columnOperand.Column) : column.Cells).Where((Cell c) => rows.Contains(c.Row.Index)).Sum(delegate(Cell c)
			{
				double num = 0.0;
				MatchCollection matchCollection = Regex.Matches(c.GetDisplayValue(), pat);
				foreach (Match item in matchCollection)
				{
					num += StringSum(item.Value);
				}
				if (c.Row.Role == RowRole.Normal)
				{
					return num;
				}
				return (c.Row.Role == RowRole.Minus) ? (0.0 - num) : 0.0;
			});
		}
		throw new FormulaTypeMismatchException();
	}

	[Category("统计")]
	[DisplayName("条件计数")]
	[Description("筛选条件列中满足条件的行，并返回这些行的行数。该函数主要用于跨表计数，且更适用于列公式，列公式应用举例如：LqCountif({被统表}[项目]=[项目,*])")]
	[Order(1828)]
	public Operand LqCountIf([ParameterName("条件列参数")][Description("某列的条件表达式")] CellsOperand cells)
	{
		if (_cellListRowsCountCache.Get(cells.Cells, out var result))
		{
			return result;
		}
		int num = 0;
		HashSet<Row> hashSet = new HashSet<Row>();
		foreach (Cell cell in cells.Cells)
		{
			if (!hashSet.Contains(cell.Row))
			{
				hashSet.Add(cell.Row);
				if (cell.Row.Role == RowRole.Normal || cell.Row.Role == RowRole.Among || cell.Row.Role == RowRole.Minus)
				{
					num++;
				}
			}
		}
		_cellListRowsCountCache.Add(cells.Cells, num);
		return num;
	}

	[Category("统计")]
	[DisplayName("生成交叉汇总表")]
	[Description("按筛选条件，以列参数1为纵向分组列，列参数2为横向分组列，列参数3为一个或多个数据统计列，生成交叉分组汇总表。")]
	[Order(1860)]
	public Operand CrossTable([ParameterName("条件列参数，列参数1，列参数2，列参数3")][Description("条件列参数指要筛选满足条件的行；列参数1指纵向分组列，列参数2指横向分组列，列参数3指要统计的一个或多个数据列。")] params Operand[] operands)
	{
		return LqCrossTable(operands);
	}

	[Category("统计")]
	[DisplayName("生成交叉汇总表")]
	[Description("按筛选条件，以列参数1为纵向分组列，列参数2为横向分组列，列参数3为一个或多个数据统计列，生成交叉分组汇总表。")]
	[Order(1871)]
	public Operand LqCrossTable([ParameterName("条件列参数，列参数1，列参数2，列参数3")][Description("条件列参数指要筛选满足条件的行；列参数1指纵向分组列，列参数2指横向分组列，列参数3指要统计的一个或多个数据列。")] params Operand[] operands)
	{
		if (operands.Length < 4)
		{
			throw new FormulaParameterCountException();
		}
		Operand operand = operands[0];
		CellsOperand cond = operand as CellsOperand;
		if (cond == null)
		{
			throw new FormulaTypeMismatchException();
		}
		if (!(operands[1] is ColumnOperand columnOperand))
		{
			throw new FormulaTypeMismatchException();
		}
		if (!(operands[2] is ColumnOperand columnOperand2))
		{
			throw new FormulaTypeMismatchException();
		}
		List<ColumnOperand> list = operands.Skip(3).Select(delegate(Operand op)
		{
			if (!(op is ColumnOperand result))
			{
				throw new FormulaTypeMismatchException();
			}
			return result;
		}).ToList();
		CrossTableOperand crossTableOperand = new CrossTableOperand();
		List<int> rows = cond.Rows.Where(delegate(int i)
		{
			RowRole role = cond.Table.Rows[i].Role;
			return role == RowRole.Normal || role == RowRole.Among || role == RowRole.Minus;
		}).ToList();
		List<Cell> list2 = columnOperand.Cells.Where((Cell c) => rows.Contains(c.Row.Index)).ToList();
		List<Cell> list3 = columnOperand2.Cells.Where((Cell c) => rows.Contains(c.Row.Index)).ToList();
		for (int l = 0; l < list.Count; l++)
		{
			DataTable dataTable = new DataTable();
			Column column = list[l].Column;
			crossTableOperand.DataTables.Add(Tuple.Create(column, dataTable));
			Dictionary<Tuple<string, string>, double> dictionary = new Dictionary<Tuple<string, string>, double>();
			List<Cell> list4 = list[l].Cells.Where((Cell c) => rows.Contains(c.Row.Index)).ToList();
			for (int m = 0; m < list2.Count; m++)
			{
				Tuple<string, string> key = Tuple.Create(list2[m].GetDisplayValue().Trim(), list3[m].GetDisplayValue().Trim());
				if (!dictionary.TryGetValue(key, out var value))
				{
					dictionary.Add(key, list4[m].ValueToDoubleOr0());
				}
				else
				{
					dictionary[key] = value + list4[m].ValueToDoubleOr0();
				}
			}
			List<string> dim1Labels = dictionary.Keys.Select((Tuple<string, string> tup) => tup.Item1).Distinct().ToList();
			DataColumn dataColumn = dataTable.Columns.Add();
			dataColumn.DataType = typeof(string);
			dataColumn.Caption = columnOperand.Column.Caption;
			List<string> dim2Labels = dictionary.Keys.Select((Tuple<string, string> tup) => tup.Item2).Distinct().ToList();
			for (int n = 0; n < dim2Labels.Count; n++)
			{
				dataColumn = dataTable.Columns.Add();
				dataColumn.DataType = typeof(double);
				dataColumn.Caption = dim2Labels[n];
			}
			dataColumn = dataTable.Columns.Add();
			dataColumn.DataType = typeof(double);
			dataColumn.Caption = "合计";
			DataRow dataRow;
			for (int num = 0; num < dim1Labels.Count; num++)
			{
				dataRow = dataTable.Rows.Add();
				dataRow[0] = dim1Labels[num];
			}
			for (int num2 = 0; num2 < dim1Labels.Count; num2++)
			{
				for (int num3 = 0; num3 < dim2Labels.Count; num3++)
				{
					if (dictionary.TryGetValue(Tuple.Create(dim1Labels[num2], dim2Labels[num3]), out var value2))
					{
						dataTable.Rows[num2][num3 + 1] = value2;
					}
					else
					{
						dataTable.Rows[num2][num3 + 1] = DBNull.Value;
					}
				}
			}
			dataRow = dataTable.Rows.Add();
			dataRow[0] = "合计";
			int k;
			for (k = 0; k < dim1Labels.Count; k++)
			{
				dataTable.Rows[k][dataTable.Columns.Count - 1] = dictionary.Where((KeyValuePair<Tuple<string, string>, double> kv) => kv.Key.Item1 == dim1Labels[k]).Sum((KeyValuePair<Tuple<string, string>, double> kv) => kv.Value);
			}
			int j;
			for (j = 0; j < dim2Labels.Count; j++)
			{
				dataTable.Rows[dataTable.Rows.Count - 1][j + 1] = dictionary.Where((KeyValuePair<Tuple<string, string>, double> kv) => kv.Key.Item2 == dim2Labels[j]).Sum((KeyValuePair<Tuple<string, string>, double> kv) => kv.Value);
			}
			dataTable.Rows[dataTable.Rows.Count - 1][dataTable.Columns.Count - 1] = dictionary.Sum((KeyValuePair<Tuple<string, string>, double> kv) => kv.Value);
		}
		return crossTableOperand;
	}

	[Category("引用")]
	[DisplayName("行号")]
	[Description("返回单元格所在行号")]
	[Order(1961)]
	public Operand Row()
	{
		if (_env.IsInTicketEnvironment && _env.IsFormulaComeFromTable)
		{
			if (_env.TicketCellRefTableRow != null)
			{
				return _env.TicketCellRefTableRow.Index + 1;
			}
			return string.Empty;
		}
		if (_env.TicketDataRowIndex >= 0)
		{
			return _env.TicketDataRowIndex + 1;
		}
		return _env.RowIndex + 1;
	}

	[Category("引用")]
	[DisplayName("索引号")]
	[Description("返回一个表格的索引号")]
	[Order(1990)]
	public Operand Index([ParameterName("表名参数")][Description("表名")] Operand table)
	{
		if (table is TreeNodeOperand treeNodeOperand)
		{
			return treeNodeOperand.TreeNode.Number ?? string.Empty;
		}
		throw new FormulaTypeMismatchException();
	}

	[Category("文本")]
	[DisplayName("数组求并集")]
	[Description("求多个数组型文本的并集")]
	[Order(2007)]
	public Operand Union([ParameterName("多个数组型文本参数")][Description("要求并集的数组型文本，数组型文本是指以“|”分隔的多个文本值，如\"A|B|C\"")] params Operand[] operands)
	{
		return ValueSetOperand.UnionAll(operands.Select((Operand o) => o.ToValueSet()));
	}

	[Category("文本")]
	[DisplayName("数组求交集")]
	[Description("求多个数组型文本的交集")]
	[Order(2017)]
	public Operand Intersect([ParameterName("多个数组型文本参数")][Description("要求交集的数组型文本，数组型文本是指以“|”分隔的多个文本值，如\"A|B|C\"")] params Operand[] operands)
	{
		return ValueSetOperand.IntersectAll(operands.Select((Operand o) => o.ToValueSet()));
	}

	[Category("文本")]
	[DisplayName("数组求差集")]
	[Description("求多个数组型文本的差集，即第一个数组的元素去掉所有在第二个、第三个、...数组中出现的元素")]
	[Order(2027)]
	public Operand Except([ParameterName("多个数组型文本参数")][Description("要求差集的数组型文本，数组型文本是指以“|”分隔的多个文本值，如\"A|B|C\"")] params Operand[] operands)
	{
		return ValueSetOperand.ExceptAll(operands.Select((Operand o) => o.ToValueSet()));
	}

	[Category("文本")]
	[DisplayName("数组取元素")]
	[Description("获取数组型文本中指定位置的元素")]
	[Order(2037)]
	public Operand Split([ParameterName("文本参数1")][Description("数组型文本")] Operand text, [ParameterName("数值参数")][Description("元素所在的位置，负数表示从后往前的位置")] Operand index, [ParameterName("文本参数2")][Description("数组型文本的分隔符")] Operand delimiter)
	{
		string text2 = text.ToString();
		int num = (int)index.ToNumber();
		if (num == int.MinValue || num == int.MaxValue)
		{
			return StringOperand.Empty;
		}
		string text3 = delimiter.ToString();
		string[] array = text2.Split(new string[1] { text3 }, StringSplitOptions.None);
		if (num < 0 && -num <= array.Length)
		{
			return ValueOperand.FromObject(array[num + array.Length]);
		}
		if (num > 0 && num <= array.Length)
		{
			return ValueOperand.FromObject(array[num - 1]);
		}
		return StringOperand.Empty;
	}

	[Category("文本")]
	[DisplayName("取数组最后一个元素")]
	[Description("获取数组型文本中最后一个元素")]
	[Order(2071)]
	public Operand SplitEnd([ParameterName("文本参数1")][Description("数组型文本")] Operand text, [ParameterName("文本参数2")][Description("数组型文本的分隔符")] Operand delimiter)
	{
		string text2 = text.ToString();
		string text3 = delimiter.ToString();
		string[] array = text2.Split(new string[1] { text3 }, StringSplitOptions.None);
		if (array.Length == 0)
		{
			return StringOperand.Empty;
		}
		return ValueOperand.FromObject(array[array.Length - 1]);
	}

	[Category("文本")]
	[DisplayName("数组元素个数")]
	[Description("计算数组型文本的元素个数")]
	[Order(2090)]
	public Operand SizeOf([ParameterName("文本参数1")][Description("数组型文本")] Operand text, [ParameterName("文本参数2")][Description("数组型文本的分隔符")] Operand delimiter)
	{
		string text2 = text.ToString();
		string text3 = delimiter.ToString();
		return text2.Split(new string[1] { text3 }, StringSplitOptions.None).Length;
	}

	[Category("文本")]
	[DisplayName("数组升序")]
	[Description("数组型文本的元素按照升序顺序重新排列")]
	[Order(2103)]
	public Operand SortUp([ParameterName("数组型文本参数")][Description("要排序的数组型文本，数组型文本是指以“|”分隔的多个文本值，如\"B|C|A\"")] Operand op)
	{
		return Sort(op);
	}

	[Category("文本")]
	[DisplayName("数组降序")]
	[Description("数组型文本的元素按照降序顺序重新排列")]
	[Order(2113)]
	public Operand SortDown([ParameterName("数组型文本参数")][Description("要排序的数组型文本，数组型文本是指以“|”分隔的多个文本值，如\"B|C|A\"")] Operand op)
	{
		ValueSetOperand valueSetOperand = op.ToValueSet();
		IEnumerable<Tuple<Row, ValueOperand>> values = valueSetOperand.Set.OrderByCellValue((Tuple<Row, ValueOperand> tup) => tup.Item2.Object).Reverse();
		return new ValueSetOperand(values);
	}

	[Category("文本")]
	[DisplayName("数组排序")]
	[Description("数组型文本的元素按照顺序重新排列")]
	[Order(2125)]
	public Operand Sort([ParameterName("数组型文本参数")][Description("要排序的数组型文本，数组型文本是指以“|”分隔的多个文本值，如\"B|C|A\"")] Operand op)
	{
		ValueSetOperand valueSetOperand = op.ToValueSet();
		IEnumerable<Tuple<Row, ValueOperand>> values = valueSetOperand.Set.OrderByCellValue((Tuple<Row, ValueOperand> tup) => tup.Item2.Object);
		return new ValueSetOperand(values);
	}

	[Category("文本")]
	[DisplayName("数组组合")]
	[Description("将数组型文本交叉组合，生成新的数组型文本")]
	[Order(2137)]
	public Operand Combination([ParameterName("多个数组型文本")][Description("要组合的多个数组型文本")] params Operand[] arys)
	{
		if (arys.Length < 1)
		{
			throw new FormulaParameterCountException();
		}
		IEnumerable<IEnumerable<string>> source = from ary in arys
			select ary.ToString().Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable() into ary
			where ary.Any()
			select ary;
		if (source.Any())
		{
			return string.Join("|", source.Aggregate((IEnumerable<string> a1, IEnumerable<string> a2) => a1.SelectMany((string prev) => a2.Select((string next) => prev + "-" + next))));
		}
		return "";
	}

	[Category("文本")]
	[DisplayName("数组组合")]
	[Description("将数组型文本交叉组合，生成新的数组型文本")]
	[Order(2157)]
	public Operand Comb([ParameterName("多个数组型文本")][Description("要组合的多个数组型文本")] params Operand[] arys)
	{
		if (arys.Length < 1)
		{
			throw new FormulaParameterCountException();
		}
		IEnumerable<IEnumerable<string>> source = from ary in arys
			select ary.ToString().Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable() into ary
			where ary.Any()
			select ary;
		if (source.Any())
		{
			return string.Join("|", source.Aggregate((IEnumerable<string> a1, IEnumerable<string> a2) => a1.SelectMany((string prev) => a2.Select((string next) => prev + "¦" + next))));
		}
		return "";
	}

	[Category("文本")]
	[DisplayName("拼接数组型文本")]
	[Description("将文本拼接成以“|”分隔的数组型文本，公式举例如: Join（\"A\",\"B\",\"C\")=\"A|B|C\"")]
	[Order(2177)]
	public Operand Join([ParameterName("多个文本参数")][Description("要拼接的多个文本")] params Operand[] arys)
	{
		if (arys.Length < 1)
		{
			throw new FormulaParameterCountException();
		}
		IEnumerable<string> values = arys.Select((Operand u) => u.ToString());
		return string.Join("|", values);
	}

	[Category("文本")]
	[DisplayName("重复字符")]
	[Description("生成多个重复字符，公式举例如: Repeat（\"A\",5)=\"AAAAA\"，Repeat（\"AB\",3)=\"ABABAB\"")]
	[Order(2189)]
	public Operand Repeat([ParameterName("文本参数")][Description("要重复的文本内容")] Operand textOp, [ParameterName("数值参数")][Description("要重复的次数")] Operand count)
	{
		if (textOp == null)
		{
			return "";
		}
		string text = textOp.ToString();
		if (text == null)
		{
			return "";
		}
		int num = (int)count.ToNumber();
		if (num < 0)
		{
			throw new FormulaBadValueException();
		}
		return string.Concat(Enumerable.Repeat(text, num));
	}

	[Category("逻辑")]
	[DisplayName("是否文本型")]
	[Description("判断参数值是否为文本型")]
	[Order(2217)]
	public Operand IsText([ParameterName("参数")][Description("要判断的参数")] Operand op)
	{
		return op is StringOperand || (op is CellOperand cellOperand && cellOperand.Value is StringOperand);
	}

	[Category("逻辑")]
	[DisplayName("是否数值型")]
	[Description("判断参数值是否为数值型")]
	[Order(2227)]
	public Operand IsNumber([ParameterName("参数")][Description("要判断的参数")] Operand op)
	{
		return op is NumberOperand || (op is CellOperand cellOperand && cellOperand.Value is NumberOperand);
	}

	[Category("逻辑")]
	[DisplayName("是否日期型")]
	[Description("判断参数值是否为日期型")]
	[Order(2237)]
	public Operand IsDate([ParameterName("参数")][Description("要判断的参数")] Operand op)
	{
		return op is DateOperand || (op is CellOperand cellOperand && cellOperand.Value is DateOperand) || (op is CellOperand cellOperand2 && cellOperand2.Value is DateYearMonthOperand);
	}

	[Category("逻辑")]
	[DisplayName("是否判断型")]
	[Description("判断参数值是否为判断型")]
	[Order(2247)]
	public Operand IsBool([ParameterName("参数")][Description("要判断的参数")] Operand op)
	{
		return op is BoolOperand || (op is CellOperand cellOperand && cellOperand.Value is BoolOperand);
	}

	[Category("逻辑")]
	[DisplayName("返回数据类型")]
	[Description("获取参数的数据类型")]
	[Order(2257)]
	public Operand Type([ParameterName("参数")][Description("获取该参数的数据类型，返回值是一个数值，1、2、3、4分别表示参数为文本型、数值型、日期型、判断型")] Operand op)
	{
		if (!(op is StringOperand))
		{
			if (!(op is NumberOperand))
			{
				if (!(op is DateOperand) && !(op is DateYearMonthOperand))
				{
					if (!(op is BoolOperand))
					{
						if (op is CellOperand cellOperand)
						{
							return Type(cellOperand.Value);
						}
						return 0;
					}
					return 4;
				}
				return 3;
			}
			return 2;
		}
		return 1;
	}

	[Category("逻辑")]
	[DisplayName("停止执行")]
	[Description("取消采集公式的执行")]
	[Order(2282)]
	public Operand Cancel()
	{
		if (_env.IsAllowExecuteCancelFunction)
		{
			throw new FormulaBreakExecuteException();
		}
		return string.Empty;
	}

	[Category("文本")]
	[DisplayName("模糊提取")]
	[Description("从文本中模糊提取出匹配字符串，举例如：Extract(\"[客户]A公司[部门]销售一部\",\"[*]\")，可得到：\"[客户]|[部门]\"")]
	[Order(2296)]
	public Operand Extract([ParameterName("文本参数1")][Description("要查找的文本")] Operand text, [ParameterName("文本参数2")][Description("模糊匹配的文本条件，通常含有通配符“*”，“*”代表任意字符串")] Operand pattern)
	{
		string text2 = text.ToString();
		string input = pattern.ToString();
		input = Regex.Replace(input, "(?<=.*\\*.*)\\*", "");
		if (!input.Contains("*"))
		{
			return ValueSetOperand.Empty;
		}
		List<string> list = new List<string>();
		if (input == "*")
		{
			list.Add(text2);
		}
		else
		{
			input = Regex.Escape(input);
			input = ((!input.StartsWith("\\*") && !input.EndsWith("\\*")) ? input.Replace("\\*", ".*?") : input.Replace("\\*", ".*"));
			MatchCollection matchCollection = Regex.Matches(text2, input);
			for (int i = 0; i < matchCollection.Count; i++)
			{
				list.Add(matchCollection[i].Value);
			}
		}
		return string.Join("|", list);
	}

	[Category("文本")]
	[DisplayName("文字提取")]
	[Description("从文本中提取出文字字符串，举例如：ExtractText(\"1001银行存款\")，可得到：\"银行存款\"")]
	[Order(2338)]
	public Operand ExtractText([ParameterName("文本参数")][Description("要进行提取处理的字符串，如果字符串中存在多个文本段，提取出来的多个文本之间将会以字符|进行分割，ExtractText(\"银行存款1001工商银行\")，可得到\"银行存款|工商银行\"")] Operand op)
	{
		string text = op.ToString();
		if (text.Length == 0)
		{
			return StringOperand.Empty;
		}
		return new StringOperand(text.ExtractText());
	}

	[Category("文本")]
	[DisplayName("数字提取")]
	[Description("从文本中提取出数字字符串，举例如：ExtractNumber(\"1001银行存款\")，可得到：\"1001\"")]
	[Order(2355)]
	public Operand ExtractNumber([ParameterName("文本参数")][Description("要进行提取处理的字符串，如果字符串中存在多个数字，提取出来的多个数字之间将会以字符|进行分割，例如ExtractNumber(\"1001银行存款300\")，可得到\"1001|300\"")] Operand op)
	{
		string text = op.ToString();
		if (text.Length == 0)
		{
			return StringOperand.Empty;
		}
		return new StringOperand(text.ExtractNumber());
	}

	[Category("文本")]
	[DisplayName("文本替换")]
	[Description("将文本中的指定文本全部替换为另一个文本")]
	[Order(2372)]
	public Operand Replace([ParameterName("多个文本参数")][Description("第1个参数是原始文本，第2、4、6...个参数是要查找的文本，第3、5、7...个参数是相应的要替换的文本")] params Operand[] ops)
	{
		if (ops.Length < 3 || ops.Length % 2 == 0)
		{
			throw new FormulaParameterCountException();
		}
		string text = ops[0].ToString();
		for (int i = 1; i < ops.Length; i += 2)
		{
			string text2 = ops[i].ToString();
			if (text2 != "")
			{
				string text3 = ops[i + 1].ToString();
				string text4 = Regex.Replace(text2, "(?<=.*\\*.*)\\*", "");
				if (text4.Contains("*"))
				{
					text4 = Regex.Escape(text4);
					text4 = (text4.StartsWith("\\*") ? Regex.Replace(text4, "^\\\\\\*(.*)", "^.*$1") : ((!text4.EndsWith("\\*")) ? Regex.Replace(text4, "(.*)\\\\\\*(.*)", "$1.*?$2") : Regex.Replace(text4, "(.*)\\\\\\*$", "$1.*$")));
					text = Regex.Replace(text, text4, text3);
				}
				else
				{
					text = text.Replace(text2, text3);
				}
			}
		}
		return text;
	}

	[Category("文本")]
	[DisplayName("文本删除")]
	[Description("将文本中的指定文本删除")]
	[Order(2419)]
	public Operand Remove([ParameterName("多个文本参数")][Description("第1个参数是原始文本，第2个及以后的参数是要删除的文本")] params Operand[] ops)
	{
		Operand[] array = new Operand[(ops.Length - 1) * 2 + 1];
		array[0] = ops[0];
		for (int i = 1; i < ops.Length; i++)
		{
			array[i * 2 - 1] = ops[i];
			array[i * 2] = new StringOperand("");
		}
		return Replace(array);
	}

	[Category("文本")]
	[DisplayName("序号前缀")]
	[Description("数组型文本增加指定格式的自动序号前缀")]
	[Order(2436)]
	public Operand Prefix([ParameterName("数组型文本参数")][Description("数组型文本")] Operand ary, [ParameterName("文本参数")][Description("指定格式的序号前缀，例如“1、”，“(1)”，“（一）”")] Operand prefix)
	{
		HashSet<Tuple<Row, ValueOperand>> set = ary.ToValueSet().Set;
		List<Tuple<Row, ValueOperand>> list = set.ToList();
		List<string> list2 = new List<string>();
		string text = prefix.ToString();
		if (Regex.IsMatch(text, "(0*)1"))
		{
			int length = Regex.Match(text, "(0*)1").Groups[1].Value.Length;
			for (int i = 0; i < list.Count; i++)
			{
				string text2 = list[i].Item2.ToString();
				list2.Add(Regex.Replace(text, "(0*)1", (i + 1).ToString(new string('0', length + 1))) + text2);
			}
		}
		else if (Regex.IsMatch(text, "一"))
		{
			for (int j = 0; j < list.Count; j++)
			{
				string text3 = list[j].Item2.ToString();
				list2.Add(Regex.Replace(text, "一", NumberingHelper.NumToChinese(j + 1)) + text3);
			}
		}
		else if (Regex.IsMatch(text, "A"))
		{
			for (int k = 0; k < list.Count; k++)
			{
				string text4 = list[k].Item2.ToString();
				list2.Add(Regex.Replace(text, "A", Column.GetExcelColumnName(k)) + text4);
			}
		}
		else if (Regex.IsMatch(text, "a"))
		{
			for (int l = 0; l < list.Count; l++)
			{
				string text5 = list[l].Item2.ToString();
				list2.Add(Regex.Replace(text, "a", Column.GetExcelColumnName(l).ToLower()) + text5);
			}
		}
		else
		{
			for (int m = 0; m < list.Count; m++)
			{
				string text6 = list[m].Item2.ToString();
				list2.Add(text + text6);
			}
		}
		return new ValueSetOperand(set.Select((Tuple<Row, ValueOperand> tup) => tup.Item1).Zip(list2, (Row row, string s) => Tuple.Create(row, ValueOperand.FromObject(s))));
	}

	[Category("引用")]
	[DisplayName("变量")]
	[Description("返回某变量的值")]
	[Order(2493)]
	public Operand Var([ParameterName("文本参数")][Description("变量名")] Operand name)
	{
		DataReference dataReference = _env.RefManager.Get(name.ToString());
		if (dataReference == null)
		{
			return StringOperand.Empty;
		}
		return dataReference.GetValue(_env.RefEvalContext);
	}

	[Category("引用")]
	[DisplayName("取表标题")]
	[Description("返回表格标题区某个单元格的值")]
	[Order(2511)]
	public Operand Title([ParameterName("表格参数")][Description("获取哪个表格的标题，若是当前表，可以省略此参数")] Operand table, [ParameterName("数值参数1")][Description("标题区行号，主标题为1，副标题依次为2，3，4...")] Operand row, [ParameterName("数值参数2")][Description("标题区列号，必须为1，2或3")] Operand col)
	{
		if (table is TreeNodeOperand treeNodeOperand)
		{
			if (treeNodeOperand.TreeNode is TreeTableNode treeTableNode)
			{
				if (_env.TableTitleCellResolver != null)
				{
					try
					{
						int num = (int)row.ToNumber().Value;
						int num2 = (int)col.ToNumber().Value;
						TableTitleCell tableTitleCell = _env.TableTitleCellResolver.GetTableTitleCell(treeTableNode.Table, num - 1, num2 - 1);
						if (tableTitleCell == null)
						{
							return string.Empty;
						}
						return ValueOperand.FromObject(tableTitleCell.Value);
					}
					catch (ArgumentOutOfRangeException)
					{
						return string.Empty;
					}
				}
				TableTitle title = treeTableNode.Table.LoadAndReturn().Title;
				int num3 = (int)row.ToNumber().Value;
				int num4 = (int)col.ToNumber().Value;
				try
				{
					return ValueOperand.FromObject(title.GetCell(num3 - 1, num4 - 1).Value);
				}
				catch (ArgumentOutOfRangeException)
				{
					return string.Empty;
				}
			}
			throw new FormulaBadValueException("第一个参数必须是表格");
		}
		throw new FormulaTypeMismatchException();
	}

	[Category("引用")]
	[DisplayName("取表底签名")]
	[Description("返回表格表底签名区某个单元格的值")]
	[Order(2570)]
	public Operand Foot([ParameterName("表格参数")][Description("表格")] Operand table, [ParameterName("数值参数1")][Description("表底签名区行号，依次为1，2，3...")] Operand row, [ParameterName("数值参数2")][Description("表底签名区列号，必须为1，2或3")] Operand col)
	{
		if (table is TreeNodeOperand treeNodeOperand)
		{
			if (treeNodeOperand.TreeNode is TreeTableNode treeTableNode)
			{
				TableFoot foot = treeTableNode.Table.LoadAndReturn().Foot;
				int num = (int)row.ToNumber().Value;
				int num2 = (int)col.ToNumber().Value;
				try
				{
					return ValueOperand.FromObject(foot.GetCell(num - 1, num2 - 1).Value);
				}
				catch (ArgumentOutOfRangeException)
				{
					return string.Empty;
				}
			}
			throw new FormulaBadValueException("第一个参数必须是表格");
		}
		throw new FormulaTypeMismatchException();
	}

	[Category("引用")]
	[DisplayName("取表单标题")]
	[Description("返回表单标题区某个单元格的值")]
	[Order(2606)]
	public Operand TicketTitle([ParameterName("数值参数1")][Description("标题区行号，必须为1，2，3，4...")] Operand row, [ParameterName("数值参数2")][Description("标题区列号，必须为1，2，3，4...")] Operand col)
	{
		if (_env.TicketInputDataResolver == null)
		{
			return string.Empty;
		}
		int num = (int)row.ToNumber().Value;
		int num2 = (int)col.ToNumber().Value;
		try
		{
			Cell ticketTitleCell = _env.TicketInputDataResolver.GetTicketTitleCell(num - 1, num2 - 1);
			if (ticketTitleCell == null)
			{
				return string.Empty;
			}
			return ValueOperand.FromObject(ticketTitleCell.Value);
		}
		catch (ArgumentOutOfRangeException)
		{
			return string.Empty;
		}
	}

	[Category("引用")]
	[DisplayName("取表单表底签名")]
	[Description("返回表单表底签名区某个单元格的值")]
	[Order(2636)]
	public Operand TicketFoot([ParameterName("数值参数1")][Description("表底签名区行号，必须为1，2，3，4...")] Operand row, [ParameterName("数值参数2")][Description("表底签名区列号，必须为1，2，3，4...")] Operand col)
	{
		if (_env.TicketInputDataResolver == null)
		{
			return string.Empty;
		}
		int num = (int)row.ToNumber().Value;
		int num2 = (int)col.ToNumber().Value;
		try
		{
			Cell ticketFooterCell = _env.TicketInputDataResolver.GetTicketFooterCell(num - 1, num2 - 1);
			if (ticketFooterCell == null)
			{
				return string.Empty;
			}
			return ValueOperand.FromObject(ticketFooterCell.Value);
		}
		catch (ArgumentOutOfRangeException)
		{
			return string.Empty;
		}
	}

	[Category("引用")]
	[DisplayName("返回列")]
	[Description("根据列名返回列对象")]
	[Order(2666)]
	public Operand Col([ParameterName("表格参数")][Description("所在表格")] Operand sheet, [ParameterName("文本参数")][Description("列名")] Operand colName)
	{
		if (sheet is TreeNodeOperand { TreeNode: TreeTableNode treeNode })
		{
			Column byCaption = treeNode.Table.Columns.GetByCaption(colName.ToString());
			if (byCaption != null)
			{
				return new ColumnOperand(byCaption);
			}
			Cell byCaption2 = treeNode.Table.Cells.GetByCaption(colName.ToString());
			if (byCaption2 != null)
			{
				return new HeaderCellOperand(byCaption2);
			}
			if (_env.IsIgnoreColSheetFunBadRefrence)
			{
				return new CellsOperand(new List<Cell>(), treeNode.Table);
			}
			throw new FormulaIgnorableException();
		}
		if (sheet is TreeNodeOperand { TreeNode: TreeNodeNull })
		{
			return new CellsOperand(new List<Cell>(), _env.HostTable);
		}
		throw new FormulaTypeMismatchException();
	}

	[Category("引用")]
	[DisplayName("返回表格")]
	[Description("根据表名返回表对象")]
	[Order(2708)]
	public Operand Sheet([ParameterName("文本参数")][Description("表名")] Operand sheetName)
	{
		try
		{
			TreeNodeBase treeNodeBase = _env.Resolver.ResolveTreeNodeString(sheetName.ToString());
			if (treeNodeBase == null)
			{
				return new TreeNodeOperand(new TreeNodeNull());
			}
			return new TreeNodeOperand(treeNodeBase);
		}
		catch (FormulaBadReferenceException)
		{
			if (_env.IsIgnoreColSheetFunBadRefrence)
			{
				return new TreeNodeOperand(new TreeNodeNull());
			}
			throw new FormulaIgnorableException();
		}
	}

	[Category("引用")]
	[DisplayName("返回行创建者姓名")]
	[Description("返回公式所在行的创建者的姓名")]
	[Order(2736)]
	public Operand RowCreatorName()
	{
		return RowCreator();
	}

	[Category("引用")]
	[DisplayName("返回行创建者姓名")]
	[Description("返回公式所在行的创建者的姓名")]
	[Order(2745)]
	public Operand RowCreator()
	{
		long createUserId = 0L;
		try
		{
			if (_env.IsInTicketEnvironment)
			{
				if (_env.IsFormulaComeFromTable)
				{
					Row ticketCellRefTableRow = _env.TicketCellRefTableRow;
					if (ticketCellRefTableRow == null)
					{
						return "";
					}
					createUserId = ticketCellRefTableRow.Creator;
				}
				else
				{
					createUserId = _env.CurrentUserId;
				}
			}
			else
			{
				Row row = _env.HostTable.Rows[_env.RowIndex];
				if (row != null)
				{
					createUserId = row.Creator;
				}
			}
		}
		catch (ArgumentOutOfRangeException)
		{
			return "";
		}
		KeyValuePair<Auditai.DTO.User, UserRole> keyValuePair = _env.RefEvalContext.Project.Users.FirstOrDefault((KeyValuePair<Auditai.DTO.User, UserRole> u) => u.Key.Id == createUserId);
		if (keyValuePair.Equals(default(KeyValuePair<Auditai.DTO.User, UserRole>)))
		{
			return StringOperand.Empty;
		}
		return keyValuePair.Key.Name;
	}

	[Category("引用")]
	[DisplayName("返回行创建者账号")]
	[Description("返回公式所在行的创建者的账号")]
	[Order(2797)]
	public Operand RowCreatorAccount()
	{
		long createUserId = 0L;
		try
		{
			if (_env.IsInTicketEnvironment)
			{
				if (_env.IsFormulaComeFromTable)
				{
					Row ticketCellRefTableRow = _env.TicketCellRefTableRow;
					if (ticketCellRefTableRow == null)
					{
						return "";
					}
					createUserId = ticketCellRefTableRow.Creator;
				}
				else
				{
					createUserId = _env.CurrentUserId;
				}
			}
			else
			{
				Row row = _env.HostTable.Rows[_env.RowIndex];
				if (row != null)
				{
					createUserId = row.Creator;
				}
			}
		}
		catch (ArgumentOutOfRangeException)
		{
			return "";
		}
		KeyValuePair<Auditai.DTO.User, UserRole> keyValuePair = _env.RefEvalContext.Project.Users.FirstOrDefault((KeyValuePair<Auditai.DTO.User, UserRole> u) => u.Key.Id == createUserId);
		if (keyValuePair.Equals(default(KeyValuePair<Auditai.DTO.User, UserRole>)))
		{
			return StringOperand.Empty;
		}
		return keyValuePair.Key.UserName;
	}

	[Category("引用")]
	[DisplayName("返回用户名称")]
	[Description("返回用户账号对应的用户名称")]
	[Order(2849)]
	public Operand GetName([ParameterName("文本参数")][Description("用户账号，数组型文本")] Operand userAccount)
	{
		string text = userAccount.ToString();
		if (string.IsNullOrWhiteSpace(text))
		{
			return string.Empty;
		}
		string[] array = text.Split(new char[1] { '|' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0)
		{
			return StringOperand.Empty;
		}
		if (array.Length == 1)
		{
			string account2 = array[0];
			KeyValuePair<Auditai.DTO.User, UserRole> keyValuePair = _env.RefEvalContext.Project.Users.FirstOrDefault((KeyValuePair<Auditai.DTO.User, UserRole> u) => u.Key.UserName == account2);
			if (keyValuePair.Equals(default(KeyValuePair<Auditai.DTO.User, UserRole>)))
			{
				return StringOperand.Empty;
			}
			return keyValuePair.Key.Name;
		}
		string[] array2 = new string[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			string account = array[i];
			KeyValuePair<Auditai.DTO.User, UserRole> keyValuePair2 = _env.RefEvalContext.Project.Users.FirstOrDefault((KeyValuePair<Auditai.DTO.User, UserRole> u) => u.Key.UserName == account);
			if (keyValuePair2.Equals(default(KeyValuePair<Auditai.DTO.User, UserRole>)))
			{
				array2[i] = string.Empty;
			}
			else
			{
				array2[i] = keyValuePair2.Key.Name;
			}
		}
		return string.Join("|", array2);
	}

	[Category("日期")]
	[DisplayName("文本转日期")]
	[Description("文本转换为日期")]
	[Order(2900)]
	public Operand DateValue([ParameterName("文本参数")][Description("要转换为日期的文本")] Operand text)
	{
		string text2 = text.ToString();
		if (string.IsNullOrWhiteSpace(text2))
		{
			return string.Empty;
		}
		if (DateTime.TryParse(text2, out var result))
		{
			return result;
		}
		return string.Empty;
	}

	[Category("日期")]
	[DisplayName("文本转年月")]
	[Description("文本转换为年月")]
	[Order(2922)]
	public Operand YearMonthValue([ParameterName("文本参数")][Description("要转换为年月的文本")] Operand text)
	{
		string text2 = text.ToString();
		if (string.IsNullOrWhiteSpace(text2))
		{
			return string.Empty;
		}
		if (DateTime.TryParse(text2, out var result))
		{
			return new DateYearMonthOperand(new DateYearMonth(result));
		}
		return string.Empty;
	}

	[Category("文本")]
	[DisplayName("转文本")]
	[Description("任意类型的数据转换为文本")]
	[Order(2944)]
	public Operand Text([ParameterName("任意参数")][Description("要转换为文本的数据")] Operand data)
	{
		return data.ToStringOp();
	}

	[Category("引用")]
	[DisplayName("返回行类型")]
	[Description("获取单元格所在的行的类型")]
	[Order(2954)]
	public Operand RowType([ParameterName("单元格参数")][Description("要获取所在行类型的单元格，返回值是一个数值，1、2、3、4、5、6分别代表常规、合计、其中、减项、列头、固定类行")] Operand cell)
	{
		if (cell is CellOperand cellOperand)
		{
			switch (cellOperand.Cell.Row.Role)
			{
			case RowRole.Normal:
				return 1;
			case RowRole.Subtotal:
			case RowRole.Total:
				return 2;
			case RowRole.Among:
				return 3;
			case RowRole.Minus:
				return 4;
			case RowRole.Header:
				return 5;
			case RowRole.Fixed:
				return 6;
			}
		}
		throw new FormulaTypeMismatchException();
	}

	[Category("引用")]
	[DisplayName("取列名")]
	[Description("返回列的名称")]
	[Order(2983)]
	public Operand ColName([ParameterName("列参数")][Description("要获取其名称的列")] Operand col)
	{
		if (col is ColumnOperand columnOperand)
		{
			return columnOperand.Column.GetUniqueFormulaName();
		}
		if (col is HeaderCellOperand headerCellOperand)
		{
			return headerCellOperand.HeaderCell.GetUniqueFormulaName();
		}
		throw new FormulaTypeMismatchException();
	}

	[Category("引用")]
	[DisplayName("生成树形列表")]
	[Description("筛选条件列中满足条件的行，并返回树形列表，该列表的各级次数据依次来源于某表格的指定列。例如：TreeList([地区]=\"华北\",[地区],[省份],[城市])")]
	[Order(3004)]
	private Operand TreeList([ParameterName("条件列参数，来源列参数1，来源列参数2，...")][Description("条件列参数指某列的条件表达式；来源列参数指组成树形列表的各个级次的列。")] params Operand[] ops)
	{
		if (ops.Length < 2)
		{
			throw new FormulaParameterCountException();
		}
		Operand operand = ops[0];
		CellsOperand cond = operand as CellsOperand;
		if (cond != null)
		{
			if (!ops.Skip(1).All((Operand c) => c is CellsOperand))
			{
				throw new FormulaTypeMismatchException();
			}
			List<CellsOperand> list = (from op in ops.Skip(1)
				select op as CellsOperand).ToList();
			TreeListOperand treeListOperand = new TreeListOperand();
			List<int> list2 = cond.Rows.Where((int r) => cond.Table.Rows[r].Role != RowRole.Total).ToList();
			list2.Sort((int left, int right) => left.CompareTo(right));
			for (int i = 0; i < list2.Count; i++)
			{
				int rowIndex = list2[i];
				TreeListNode treeListNode = null;
				for (int j = 0; j < list.Count; j++)
				{
					CellsOperand cellsOperand = list[j];
					string displayValue = cellsOperand.GetCellByRowIndex(rowIndex).GetDisplayValue();
					if (string.IsNullOrEmpty(displayValue))
					{
						break;
					}
					if (j == 0)
					{
						treeListNode = treeListOperand.AddOrGet(displayValue);
					}
					else if (j == list.Count - 1)
					{
						string[] array = displayValue.Split(new string[1] { "|" }, StringSplitOptions.RemoveEmptyEntries);
						foreach (string s in array)
						{
							treeListNode.AddOrGet(s);
						}
					}
					else
					{
						treeListNode = treeListNode.AddOrGet(displayValue);
					}
				}
			}
			return treeListOperand;
		}
		throw new FormulaTypeMismatchException();
	}

	[Category("引用")]
	[DisplayName("表式列表")]
	[Description("筛选条件列中满足条件的行，并返回表式列表，该列表的各列数据依次来源于某表格的指定列。例如：TableList({合同明细表}[地区]=\"北京\",{合同明细表}[客户名称],{合同明细表}[合同名称],{合同明细表}[合同金额])")]
	[Order(3065)]
	private Operand TableList([ParameterName("条件列参数，返回值的列参数，仅显示的列参数1，仅显示的列参数2，...")][Description("条件列参数指某列的条件表达式；返回值的列参数指实际要取值的列；仅显示的列参数指仅辅助显示的列")] params Operand[] operands)
	{
		if (operands.Length < 2)
		{
			throw new FormulaParameterCountException();
		}
		CellsOperand cond = operands[0] as CellsOperand;
		if (cond == null)
		{
			throw new FormulaTypeMismatchException();
		}
		List<int> list = cond.Rows.Where((int r) => cond.Table.Rows[r].Role != RowRole.Total).ToList();
		list.Sort((int left, int right) => left.CompareTo(right));
		TableListOperand tableListOperand = new TableListOperand();
		DataTable dataTable = tableListOperand.DataTable;
		for (int i = 0; i < list.Count; i++)
		{
			dataTable.Rows.Add(dataTable.NewRow());
		}
		for (int j = 1; j < operands.Length; j++)
		{
			if (!(operands[j] is CellsOperand cellsOperand))
			{
				throw new FormulaTypeMismatchException();
			}
			DataColumn dataColumn = dataTable.Columns.Add();
			if (cellsOperand is ColumnOperand columnOperand)
			{
				tableListOperand.Aligns.Add((columnOperand.Column.Style?.Align).GetValueOrDefault(CellTextAlign.MiddleLeft));
				dataColumn.Caption = columnOperand.Column.CaptionDisplay;
			}
			else if (cellsOperand is HeaderCellOperand headerCellOperand)
			{
				tableListOperand.Aligns.Add((headerCellOperand.HeaderCell.Column.Style?.Align).GetValueOrDefault(CellTextAlign.MiddleLeft));
				dataColumn.Caption = headerCellOperand.HeaderCell.GetDisplayValue();
			}
			for (int k = 0; k < list.Count; k++)
			{
				try
				{
					dataTable.Rows[k][dataColumn] = cellsOperand.GetCellByRowIndex(list[k]).GetDisplayValue();
				}
				catch (ArgumentOutOfRangeException)
				{
					throw new FormulaNotApplicableException("TableList函数运算错误，返回列中没有对应单元格。");
				}
			}
		}
		return tableListOperand;
	}

	[Category("引用")]
	[DisplayName("返回上一行单元格的值")]
	[Description("获取单元格所在列的上一行单元格的值")]
	[Order(3119)]
	public Operand PreCell([ParameterName("单元格参数")][Description("要获取哪个单元格上一行的值")] Operand cell)
	{
		if (cell is CellOperand cellOperand)
		{
			if (cellOperand.Cell.Row.Index == 0)
			{
				return string.Empty;
			}
			Cell prevCell = cellOperand.Cell._Table[cellOperand.Cell.Row.Index - 1, cellOperand.Cell.Column.Index];
			if (prevCell == null)
			{
				return string.Empty;
			}
			return ValueOperand.FromObject(prevCell.Value);
		}
		throw new FormulaTypeMismatchException();
	}

	[Category("引用")]
	[DisplayName("返回下一行单元格的值")]
	[Description("获取单元格所在列的下一行单元格的值")]
	[Order(3140)]
	public Operand NextCell([ParameterName("单元格参数")][Description("要获取哪个单元格下一行的值")] Operand cell)
	{
		if (cell is CellOperand cellOperand)
		{
			if (cellOperand.Cell.Row.Index >= cellOperand.Cell._Table.Rows.Count - 1)
			{
				return string.Empty;
			}
			Cell nextCell = cellOperand.Cell._Table[cellOperand.Cell.Row.Index + 1, cellOperand.Cell.Column.Index];
			if (nextCell == null)
			{
				return string.Empty;
			}
			return ValueOperand.FromObject(nextCell.Value);
		}
		throw new FormulaTypeMismatchException();
	}

	[Category("引用")]
	[DisplayName("输入列表")]
	[Description("筛选条件列中满足条件的行，返回一个可输入值的项目列表，该列表的项目来源于某表格的指定列。例如：InputList({合同明细表}[合同类型]=\"销售合同\",[收款方式])")]
	[Order(3161)]
	private Operand InputList([ParameterName("条件列参数")][Description("列的条件表达式")] Operand cond, [ParameterName("列参数")][Description("生成项目列表的某表格指定列")] Operand column)
	{
		if (!(cond is CellsOperand))
		{
			throw new FormulaTypeMismatchException();
		}
		if (!(column is ColumnOperand columnOperand))
		{
			throw new FormulaTypeMismatchException();
		}
		ValueSetOperand vso = LqFilter(cond, column) as ValueSetOperand;
		InputListOperand inputListOperand = new InputListOperand(vso);
		inputListOperand.KeyName = columnOperand.Column.Caption;
		return inputListOperand;
	}

	[Category("引用")]
	[DisplayName("多维列表")]
	[Description("返回多维列表，每维列表可以是简单列表、树形列表或表式列表。例如MultiList(\"产品\",{产品明细表}[产品名称],\"客户\",TableList({客户明细表}[地区]=\"北京\",{客户明细表}[客户名称],{客户明细表}[客户地址]))")]
	[Order(3177)]
	private Operand MultiList([ParameterName("文本参数1，列表参数1，文本参数2，列表参数2，...")][Description("文本参数作为多维列表每一维的名称；列表参数可以是简单列表、TreeList函数返回的树形列表或TableList函数返回的表式列表。")] params Operand[] operands)
	{
		if (operands.Length < 2 || operands.Length % 2 != 0)
		{
			throw new FormulaParameterCountException();
		}
		MultiListOperand multiListOperand = new MultiListOperand();
		for (int i = 0; i < operands.Length; i += 2)
		{
			string value = operands[i].ToStringOp().Value;
			Operand operand = operands[i + 1];
			if (!(operand is TreeListOperand) && !(operand is TableListOperand))
			{
				operand = operand.ToValueSetOrderByRowIndex();
			}
			multiListOperand.MultiList.Add(Tuple.Create(value, operand));
		}
		return multiListOperand;
	}

	[Category("控制")]
	[DisplayName("锁定")]
	[Description("筛选条件列中满足条件的行，并将这些行对应的查找列的单元格锁定")]
	[Order(3202)]
	private void Lock([ParameterName("条件列参数，列参数1，列参数2，...")][Description("条件列参数指某列的条件表达式；其余列参数指要锁定对应行的单元格的列。")] params Operand[] ops)
	{
		if (ops.Length < 2)
		{
			throw new FormulaParameterCountException();
		}
		CellsOperand cond = ops[0] as CellsOperand;
		if (cond == null)
		{
			throw new FormulaTypeMismatchException();
		}
		List<int> list = cond.Rows.Where((int r) => cond.Table.Rows[r].Role != RowRole.Total).ToList();
		foreach (Operand item in ops.Skip(1))
		{
			ColumnOperand colOp = item as ColumnOperand;
			if (colOp != null)
			{
				CellsOperand o = new CellsOperand(cond.Cells.Select((Cell c) => colOp.GetCellByRowIndex(c.Row.Index)).ToList(), cond.Table);
				if (_env.ControlFormulaContext != null)
				{
					_env.ControlFormulaContext.DoLock(o);
				}
				continue;
			}
			throw new FormulaTypeMismatchException();
		}
	}

	[Category("控制")]
	[DisplayName("允许行编辑")]
	[Description("筛选条件列中满足条件的行，允许对这些行进行编辑，其它行则不允许进行编辑")]
	[Order(3233)]
	private void AllowRowEdit([ParameterName("条件列参数")][Description("条件列参数指某列的条件表达式")] params Operand[] ops)
	{
		if (ops.Length < 1)
		{
			throw new FormulaParameterCountException();
		}
		CellsOperand cond = ops[0] as CellsOperand;
		if (cond == null)
		{
			throw new FormulaTypeMismatchException();
		}
		if (_env.ControlFormulaContext == null)
		{
			return;
		}
		_env.ControlFormulaContext.DoAllowEditRow(null);
		List<int> list = cond.Rows.Where((int r) => cond.Table.Rows[r].Role != RowRole.Total).ToList();
		foreach (int row2 in cond.Rows)
		{
			Row row = cond.Table.Rows[row2];
			if (row.Role != RowRole.Total)
			{
				_env.ControlFormulaContext.DoAllowEditRow(row);
			}
		}
	}

	[Category("控制")]
	[DisplayName("告警")]
	[Description("筛选条件列中满足条件的行，并将这些行对应的查找列的单元格显示为告警状态")]
	[Order(3265)]
	private void Warning([ParameterName("条件列参数，列参数1，列参数2，...")][Description("条件列参数指某列的条件表达式；其余列参数指对应行上需要显示为告警状态的单元格所在的列。")] params Operand[] ops)
	{
		if (ops.Length < 2)
		{
			throw new FormulaParameterCountException();
		}
		if (!(ops[0] is CellsOperand cellsOperand))
		{
			throw new FormulaTypeMismatchException();
		}
		List<int> list = cellsOperand.Rows.ToList();
		foreach (Operand item in ops.Skip(1))
		{
			ColumnOperand colOp = item as ColumnOperand;
			if (colOp != null)
			{
				CellsOperand o = new CellsOperand(cellsOperand.Cells.Select((Cell c) => colOp.GetCellByRowIndex(c.Row.Index)).ToList(), cellsOperand.Table);
				if (_env.ControlFormulaContext != null)
				{
					_env.ControlFormulaContext.DoWarning(o);
				}
				continue;
			}
			throw new FormulaTypeMismatchException();
		}
	}

	[Category("控制")]
	[DisplayName("提醒")]
	[Description("筛选条件列中满足条件的行，并将这些行对应的查找列的单元格显示为提醒状态")]
	[Order(3296)]
	private void Remind([ParameterName("条件列参数，列参数1，列参数2，...")][Description("条件列参数指某列的条件表达式；其余列参数指对应行上需要显示为提醒状态的单元格所在的列。")] params Operand[] ops)
	{
		if (ops.Length < 2)
		{
			throw new FormulaParameterCountException();
		}
		if (!(ops[0] is CellsOperand cellsOperand))
		{
			throw new FormulaTypeMismatchException();
		}
		List<int> list = cellsOperand.Rows.ToList();
		foreach (Operand item in ops.Skip(1))
		{
			ColumnOperand colOp = item as ColumnOperand;
			if (colOp != null)
			{
				CellsOperand o = new CellsOperand(cellsOperand.Cells.Select((Cell c) => colOp.GetCellByRowIndex(c.Row.Index)).ToList(), cellsOperand.Table);
				if (_env.ControlFormulaContext != null)
				{
					_env.ControlFormulaContext.DoRemind(o);
				}
				continue;
			}
			throw new FormulaTypeMismatchException();
		}
	}

	[Category("控制")]
	[DisplayName("设置文本的颜色")]
	[Description("筛选条件列中满足条件的行，并将这些行对应的查找列的单元格文本颜色设置为指定的颜色")]
	[Order(3327)]
	private void ForeColor([ParameterName("条件列参数，文本的颜色, 列参数1，列参数2，...")][Description("条件列参数指某列的条件表达式；其余列参数指对应行上需要设置文本颜色的单元格所在的列，例如：ForeColor([列1]=某值,rgb(255,0,0),[列2],[列3],…) 或者 ForeColor([列1]=某值,red(),[列2],[列3],…)")] params Operand[] ops)
	{
		if (ops.Length < 3)
		{
			throw new FormulaParameterCountException();
		}
		if (!(ops[0] is CellsOperand cellsOperand))
		{
			throw new FormulaTypeMismatchException();
		}
		double value = ops[1].ToNumber().Value;
		Color color = Color.FromArgb((int)value);
		List<int> list = cellsOperand.Rows.ToList();
		foreach (Operand item in ops.Skip(2))
		{
			ColumnOperand colOp = item as ColumnOperand;
			if (colOp != null)
			{
				CellsOperand o = new CellsOperand(cellsOperand.Cells.Select((Cell c) => colOp.GetCellByRowIndex(c.Row.Index)).ToList(), cellsOperand.Table);
				if (_env.ControlFormulaContext != null)
				{
					_env.ControlFormulaContext.DoForeColor(o, color);
				}
				continue;
			}
			throw new FormulaTypeMismatchException();
		}
	}

	[Category("控制")]
	[DisplayName("设置单元格的背景色")]
	[Description("筛选条件列中满足条件的行，并将这些行对应的查找列的单元格背景色设置为指定的颜色")]
	[Order(3360)]
	private void BackColor([ParameterName("条件列参数，文本的颜色, 列参数1，列参数2，...")][Description("条件列参数指某列的条件表达式；其余列参数指对应行上需要设置背景色的单元格所在的列，例如：BackColor([列1]=某值,rgb(255,0,0),[列2],[列3],…) 或者 BackColor([列1]=某值,red(),[列2],[列3],…)")] params Operand[] ops)
	{
		if (ops.Length < 3)
		{
			throw new FormulaParameterCountException();
		}
		if (!(ops[0] is CellsOperand cellsOperand))
		{
			throw new FormulaTypeMismatchException();
		}
		double value = ops[1].ToNumber().Value;
		Color color = Color.FromArgb((int)value);
		List<int> list = cellsOperand.Rows.ToList();
		foreach (Operand item in ops.Skip(2))
		{
			ColumnOperand colOp = item as ColumnOperand;
			if (colOp != null)
			{
				CellsOperand o = new CellsOperand(cellsOperand.Cells.Select((Cell c) => colOp.GetCellByRowIndex(c.Row.Index)).ToList(), cellsOperand.Table);
				if (_env.ControlFormulaContext != null)
				{
					_env.ControlFormulaContext.DoBackColor(o, color);
				}
				continue;
			}
			throw new FormulaTypeMismatchException();
		}
	}

	[Category("控制")]
	[DisplayName("颜色值")]
	[Description("返回指定的颜色值")]
	[Order(3393)]
	private Operand Rgb([ParameterName("颜色的R(红色)分量、G(绿色)分量、B(蓝色)分量")][Description("颜色分量的取值范围为0 - 255，例如：rgb(0, 255, 0) 代表绿色")] params Operand[] ops)
	{
		if (ops.Length < 3)
		{
			throw new FormulaParameterCountException();
		}
		int val = (int)ops[0].ToNumber().Value;
		int val2 = (int)ops[1].ToNumber().Value;
		int val3 = (int)ops[2].ToNumber().Value;
		val = Math.Min(Math.Max(0, val), 255);
		val2 = Math.Min(Math.Max(0, val2), 255);
		val3 = Math.Min(Math.Max(0, val3), 255);
		return new NumberOperand(Color.FromArgb(val, val2, val3).ToArgb());
	}

	[Category("控制")]
	[DisplayName("红色")]
	[Description("返回颜色值: 红色")]
	[Order(3414)]
	private Operand Red()
	{
		return new NumberOperand(Color.Red.ToArgb());
	}

	[Category("控制")]
	[DisplayName("橙色")]
	[Description("返回颜色值: 橙色")]
	[Order(3423)]
	private Operand Orange()
	{
		return new NumberOperand(Color.Orange.ToArgb());
	}

	[Category("控制")]
	[DisplayName("黄色")]
	[Description("返回颜色值: 黄色")]
	[Order(3432)]
	private Operand Yellow()
	{
		return new NumberOperand(Color.Yellow.ToArgb());
	}

	[Category("控制")]
	[DisplayName("绿色")]
	[Description("返回颜色值: 绿色")]
	[Order(3441)]
	private Operand Green()
	{
		return new NumberOperand(Color.Green.ToArgb());
	}

	[Category("控制")]
	[DisplayName("青色")]
	[Description("返回颜色值: 青色")]
	[Order(3450)]
	private Operand Cyan()
	{
		return new NumberOperand(Color.Cyan.ToArgb());
	}

	[Category("控制")]
	[DisplayName("蓝色")]
	[Description("返回颜色值: 蓝色")]
	[Order(3459)]
	private Operand Blue()
	{
		return new NumberOperand(Color.Blue.ToArgb());
	}

	[Category("控制")]
	[DisplayName("紫色")]
	[Description("返回颜色值: 紫色")]
	[Order(3468)]
	private Operand Purple()
	{
		return new NumberOperand(Color.Purple.ToArgb());
	}

	[Category("控制")]
	[DisplayName("灰色")]
	[Description("返回颜色值: 灰色")]
	[Order(3477)]
	private Operand Gray()
	{
		return new NumberOperand(Color.Gray.ToArgb());
	}

	[Category("控制")]
	[DisplayName("粉色")]
	[Description("返回颜色值: 粉色")]
	[Order(3486)]
	private Operand Pink()
	{
		return new NumberOperand(Color.Pink.ToArgb());
	}

	[Category("控制")]
	[DisplayName("黑色")]
	[Description("返回颜色值: 黑色")]
	[Order(3495)]
	private Operand Black()
	{
		return new NumberOperand(Color.Black.ToArgb());
	}

	[Category("控制")]
	[DisplayName("白色")]
	[Description("返回颜色值: 白色")]
	[Order(3504)]
	private Operand White()
	{
		return new NumberOperand(Color.White.ToArgb());
	}

	[Category("控制")]
	[DisplayName("棕色")]
	[Description("返回颜色值: 棕色")]
	[Order(3513)]
	private Operand Brown()
	{
		return new NumberOperand(Color.Brown.ToArgb());
	}

	public static List<string> GetFunctionOnlyShowInControlFormulaEditWindow()
	{
		return new List<string>
		{
			"ForeColor", "BackColor", "Rgb", "Red", "Orange", "Yellow", "Green", "Cyan", "Blue", "Purple",
			"Gray", "Pink", "Black", "White", "Brown"
		};
	}

	public static List<string> GetObsoletedFunction()
	{
		return new List<string>
		{
			"LqDistinct", "LqFilter", "LqCollect", "LqVLookUp", "LqSumIf", "LqCountIf", "LqAsc", "LqDesc", "LqMax", "LqMin",
			"LqSumFind", "LqCrossTable", "Sort", "Concat", "Product", "Exact", "Combination", "RowCreator"
		};
	}

	private static double StringSum(string s)
	{
		double num = 0.0;
		foreach (Match item in Regex.Matches(s, "-?\\d+(,\\d{3})*(\\.\\d+)?"))
		{
			num += double.Parse(item.Value);
		}
		return num;
	}

	private static double ParseDoubleString(string s)
	{
		if (s.Length == 0)
		{
			return 0.0;
		}
		if (double.TryParse(s, out var result))
		{
			return result;
		}
		return 0.0;
	}

	private static bool IsDoubleEqualInRoundFunction(double left, double right)
	{
		if (Math.Abs(left - right) < 1E-09)
		{
			return true;
		}
		return false;
	}
}
