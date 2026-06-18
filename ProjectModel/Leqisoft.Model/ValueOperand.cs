﻿﻿﻿﻿﻿using System;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public abstract class ValueOperand : Operand
{
	public object Object { get; private set; }

	protected ValueOperand(object value)
	{
		Object = value;
	}

	public static implicit operator ValueOperand(int value)
	{
		return new NumberOperand(value);
	}

	public static implicit operator ValueOperand(double value)
	{
		return new NumberOperand(value);
	}

	public static implicit operator ValueOperand(DateTime value)
	{
		return new DateOperand(value);
	}

	public static implicit operator ValueOperand(string value)
	{
		return new StringOperand(value);
	}

	public static implicit operator ValueOperand(bool value)
	{
		return new BoolOperand(value);
	}

	public static implicit operator ValueOperand(TimeSpan value)
	{
		return new TimeOperand(value);
	}

	public static implicit operator ValueOperand(DateYearMonth value)
	{
		return new DateYearMonthOperand(value);
	}

	public static ValueOperand FromObject(object value)
	{
		if (!(value is int num))
		{
			if (!(value is double num2))
			{
				if (!(value is DateTime dateTime))
				{
					if (!(value is string text))
					{
						if (!(value is bool flag))
						{
							if (!(value is TimeSpan timeSpan))
							{
								if (value is DateYearMonth dateYearMonth)
								{
									return dateYearMonth;
								}
								return string.Empty;
							}
							return timeSpan;
						}
						return flag;
					}
					return text;
				}
				return dateTime;
			}
			return num2;
		}
		return num;
	}

	public static ValueOperand FromCellValue(Cell cell)
	{
		if (cell == null)
		{
			return string.Empty;
		}
		object value = cell.Value;
		if (!(value is int num))
		{
			if (!(value is double num2))
			{
				if (!(value is DateTime value2))
				{
					if (!(value is string text))
					{
						if (!(value is bool flag))
						{
							if (!(value is TimeSpan timeSpan))
							{
								if (value is DateYearMonth value3)
								{
									return new DateYearMonthOperand(value3)
									{
										Cell = cell
									};
								}
								return string.Empty;
							}
							return timeSpan;
						}
						return flag;
					}
					return text;
				}
				return new DateOperand(value2)
				{
					Cell = cell
				};
			}
			return num2;
		}
		return num;
	}
}
