using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class StringOperand : ValueOperand
{
	internal static readonly string[] _splitter = new string[1] { "|" };

	public static StringOperand Empty { get; } = new StringOperand("");


	public string Value { get; }

	public override OperandType OperandType => OperandType.StringOperand;

	public StringOperand(string value)
		: base(value)
	{
		Value = value;
	}

	public override object Evaluate()
	{
		return Value;
	}

	public override string ToString()
	{
		return Value;
	}

	public override NumberOperand ToNumber()
	{
		double result;
		return double.TryParse(Value, out result) ? result : 0.0;
	}

	public override DateOperand ToDate()
	{
		if (string.IsNullOrWhiteSpace(Value))
		{
			return DateOperand.Zero;
		}
		if (!DateTime.TryParse(Value, out var result))
		{
			throw new FormulaTypeMismatchException();
		}
		return result;
	}

	public override BoolOperand ToBool()
	{
		if (Value == "√")
		{
			return true;
		}
		return false;
	}

	public override TimeOperand ToTime()
	{
		if (TimeSpan.TryParse(Value, out var result))
		{
			return result;
		}
		throw new FormulaBadValueException("文本无法转换为时间型");
	}

	public override DateYearMonthOperand ToDateYearMonth()
	{
		if (string.IsNullOrWhiteSpace(Value))
		{
			return DateYearMonthOperand.Zero;
		}
		if (!DateTime.TryParse(Value, out var result))
		{
			throw new FormulaTypeMismatchException();
		}
		return new DateYearMonth(result);
	}

	public override ValueOperand Add(Operand other)
	{
		return ToNumber().Add(other);
	}

	public override ValueOperand Subtract(Operand other)
	{
		return ToNumber().Subtract(other);
	}

	public override ValueOperand Multiply(Operand other)
	{
		return ToNumber().Multiply(other);
	}

	public override ValueOperand Divide(Operand other)
	{
		return ToNumber().Divide(other);
	}

	public override Operand Concatenate(Operand other)
	{
		return ToString() + other.ToString();
	}

	public override ValueOperand Negate()
	{
		return ToNumber().Negate();
	}

	public override Operand Equal(Operand other)
	{
		if (!(other is StringOperand stringOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (!(other is NumberOperand numberOperand))
					{
						if (other is BoolOperand boolOperand)
						{
							return boolOperand.Equal(this);
						}
						return false;
					}
					return Value == string.Empty && numberOperand.Value == 0.0;
				}
				return Equal(rangeOperand.SingleValue);
			}
			return Equal(cellOperand.Value);
		}
		return IsMatch(Value, stringOperand.Value);
	}

	public override Operand NotEqual(Operand other)
	{
		if (!(other is StringOperand stringOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (!(other is NumberOperand numberOperand))
					{
						if (other is BoolOperand boolOperand)
						{
							return boolOperand.NotEqual(this);
						}
						return true;
					}
					return Value != string.Empty || numberOperand.Value != 0.0;
				}
				return NotEqual(rangeOperand.SingleValue);
			}
			return NotEqual(cellOperand.Value);
		}
		return !IsMatch(Value, stringOperand.Value);
	}

	public override Operand GreaterThan(Operand other)
	{
		if (!(other is StringOperand stringOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (other is NumberOperand numberOperand)
					{
						if (string.IsNullOrEmpty(Value))
						{
							return 0.0 > numberOperand.Value;
						}
						double result;
						return double.TryParse(Value, out result) && result > numberOperand.Value;
					}
					return false;
				}
				return GreaterThan(rangeOperand.SingleValue);
			}
			return GreaterThan(cellOperand.Value);
		}
		List<string> set1 = Value.Split(_splitter, StringSplitOptions.RemoveEmptyEntries).ToList();
		string[] source = stringOperand.Value.Split(_splitter, StringSplitOptions.RemoveEmptyEntries);
		return source.Any() && source.All((string s2) => set1.Any((string s1) => IsMatch(s1, s2)));
	}

	public override Operand GreaterThanOrEqual(Operand other)
	{
		if (!(other is StringOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (other is NumberOperand numberOperand)
					{
						if (string.IsNullOrEmpty(Value))
						{
							return 0.0 >= numberOperand.Value;
						}
						double result;
						return double.TryParse(Value, out result) && result >= numberOperand.Value;
					}
					return false;
				}
				return GreaterThanOrEqual(rangeOperand.SingleValue);
			}
			return GreaterThanOrEqual(cellOperand.Value);
		}
		return Equal(other).Or(GreaterThan(other));
	}

	public override Operand LessThan(Operand other)
	{
		if (!(other is StringOperand stringOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (other is NumberOperand numberOperand)
					{
						if (string.IsNullOrEmpty(Value))
						{
							return 0.0 < numberOperand.Value;
						}
						double result;
						return double.TryParse(Value, out result) && result < numberOperand.Value;
					}
					return false;
				}
				return LessThan(rangeOperand.SingleValue);
			}
			return LessThan(cellOperand.Value);
		}
		return stringOperand.GreaterThan(this);
	}

	public override Operand LessThanOrEqual(Operand other)
	{
		if (!(other is StringOperand))
		{
			if (!(other is CellOperand cellOperand))
			{
				if (!(other is RangeOperand rangeOperand))
				{
					if (other is NumberOperand numberOperand)
					{
						if (string.IsNullOrEmpty(Value))
						{
							return 0.0 <= numberOperand.Value;
						}
						double result;
						return double.TryParse(Value, out result) && result <= numberOperand.Value;
					}
					return false;
				}
				return LessThanOrEqual(rangeOperand.SingleValue);
			}
			return LessThanOrEqual(cellOperand.Value);
		}
		return Equal(other).Or(LessThan(other));
	}

	public override Operand And(Operand other)
	{
		return ToBool().And(other);
	}

	public override Operand Or(Operand other)
	{
		return ToBool().Or(other);
	}

	public override int GetHashCode()
	{
		return Value.Trim().GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is StringOperand stringOperand))
		{
			return false;
		}
		return stringOperand.Value == Value;
	}

	public static string GetRegxExpression(string str)
	{
		if (str.IndexOf("**") != -1)
		{
			string text = string.Empty;
			string text2 = string.Empty;
			string empty = string.Empty;
			string text3 = str;
			if (text3.StartsWith("*"))
			{
				text = ".*";
				text3 = text3.Substring(1, text3.Length - 1);
			}
			if (text3.EndsWith("*"))
			{
				text2 = ".*";
				text3 = text3.Substring(0, text3.Length - 1);
			}
			char c = '\0';
			int num = -1;
			int num2 = -1;
			for (int i = 0; i < text3.Length; i++)
			{
				if (text3[i] != '*')
				{
					num = i;
					break;
				}
			}
			if (num >= 0)
			{
				for (int num3 = text3.Length - 1; num3 >= num; num3--)
				{
					if (text3[num3] != '*')
					{
						num2 = num3;
						break;
					}
				}
			}
			if (num != -1)
			{
				string str2 = text3.Substring(0, num);
				str2 = Regex.Escape(str2);
				string str3 = text3.Substring(num2 + 1, text3.Length - 1 - num2);
				str3 = Regex.Escape(str3);
				string text4 = text3.Substring(num, num2 - num + 1);
				StringBuilder stringBuilder = new StringBuilder(text4.Length);
				for (int j = 0; j < text4.Length; j++)
				{
					char c2 = text4[j];
					if (j == 0)
					{
						c = c2;
						stringBuilder.Append(c2);
					}
					else if (c2 != '*')
					{
						c = c2;
						stringBuilder.Append(c2);
					}
					else if (c != '*')
					{
						c = c2;
						stringBuilder.Append(c2);
					}
				}
				text4 = Regex.Escape(stringBuilder.ToString()).Replace("\\*", ".*").Replace("\\?", ".");
				empty = str2 + text4 + str3;
			}
			else
			{
				empty = Regex.Escape(str).Replace("\\?", ".");
			}
			return "^" + text + empty + text2 + "$";
		}
		return "^" + Regex.Escape(str).Replace("\\*", ".*").Replace("\\?", ".") + "$";
	}

	private static bool IsMatch(string s1, string s2)
	{
		return Regex.IsMatch(s1.Trim(), GetRegxExpression(s2.Trim()));
	}

	public override ValueSetOperand ToValueSet()
	{
		return new ValueSetOperand(from s in Value.Split(_splitter, StringSplitOptions.RemoveEmptyEntries)
			select Tuple.Create<Row, ValueOperand>(null, ValueOperand.FromObject(s)));
	}

	public override StringOperand ToStringOp()
	{
		return this;
	}
}
