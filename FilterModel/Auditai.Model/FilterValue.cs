using System;
using Newtonsoft.Json;

namespace Auditai.Model;

[Serializable]
public class FilterValue : IComparable<FilterValue>
{
	public const int VALUE_CANNOT_COMPARE = -2;

	[JsonProperty("value")]
	public object Value { get; set; }

	[JsonProperty("display")]
	public string DisplayValue { get; set; }

	public FilterValue()
	{
		DisplayValue = string.Empty;
	}

	public FilterValue Trim()
	{
		if (Value is string text)
		{
			Value = text.Trim();
		}
		return this;
	}

	public static FilterValue FromObject(object value)
	{
		if (value is FilterValue result)
		{
			return result;
		}
		FilterValue filterValue = new FilterValue();
		filterValue.Value = value;
		filterValue.DisplayValue = ((value == null) ? string.Empty : value.ToString());
		return filterValue;
	}

	public static FilterValue FromObject(object value, string displayValue)
	{
		if (value is FilterValue filterValue)
		{
			filterValue.DisplayValue = displayValue ?? string.Empty;
			return filterValue;
		}
		FilterValue filterValue2 = new FilterValue();
		filterValue2.Value = value;
		filterValue2.DisplayValue = displayValue ?? string.Empty;
		return filterValue2;
	}

	public int CompareTo(FilterValue other)
	{
		if (Value == null)
		{
			if (other.Value != null)
			{
				return -1;
			}
			return 0;
		}
		if (other.Value == null)
		{
			if (Value != null)
			{
				return 1;
			}
			return 0;
		}
		if (TryConvertDecimal(Value, out var dec) && TryConvertDecimal(other.Value, out var dec2))
		{
			return dec.CompareTo(dec2);
		}
		Type type = other.Value.GetType();
		Type type2 = Value.GetType();
		if (type2 != type)
		{
			return -2;
		}
		int result = -2;
		object value = Value;
		if (!(value is bool flag))
		{
			if (!(value is char c))
			{
				if (!(value is string text))
				{
					if (value is DateTime { Date: var date })
					{
						result = date.CompareTo(other.Value);
					}
				}
				else
				{
					result = text.CompareTo(other.Value);
				}
			}
			else
			{
				result = c.CompareTo(other.Value);
			}
		}
		else
		{
			result = flag.CompareTo(other.Value);
		}
		return result;
	}

	public static bool TryConvertDecimal(object value, out decimal dec)
	{
		if (!(value is byte value2))
		{
			if (!(value is short value3))
			{
				if (!(value is int value4))
				{
					if (!(value is long value5))
					{
						if (!(value is float value6))
						{
							if (!(value is double value7))
							{
								if (value is decimal num)
								{
									dec = num;
									return true;
								}
								dec = default(decimal);
								return false;
							}
							dec = new decimal(value7);
							return true;
						}
						dec = new decimal(value6);
						return true;
					}
					dec = new decimal(value5);
					return true;
				}
				dec = new decimal(value4);
				return true;
			}
			dec = new decimal(value3);
			return true;
		}
		dec = new decimal(value2);
		return true;
	}

	public override bool Equals(object value)
	{
		if (this == value)
		{
			return true;
		}
		if (value is FilterValue filterValue)
		{
			if (Value == null)
			{
				return filterValue.Value == null;
			}
			if (TryConvertDecimal(Value, out var dec) && TryConvertDecimal(filterValue.Value, out var dec2))
			{
				return dec.Equals(dec2);
			}
			return Value.Equals(filterValue.Value);
		}
		if (Value == null)
		{
			return value == null;
		}
		if (TryConvertDecimal(Value, out var dec3) && TryConvertDecimal(value, out var dec4))
		{
			return dec3.Equals(dec4);
		}
		return Value.Equals(value);
	}

	public override int GetHashCode()
	{
		return Value?.GetHashCode() ?? 0;
	}

	public override string ToString()
	{
		return Value?.ToString();
	}
}
