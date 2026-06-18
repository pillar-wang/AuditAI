using System;

namespace Leqisoft.Model;

[Serializable]
public class EqualsFilter<T> : ComparisonFilter<T> where T : IComparable<T>
{
	protected override Predicate<FilterValue> filter => delegate(FilterValue candidate)
	{
		FilterValue filterValue = FilterValue.FromObject(base.Value);
		if (filterValue.Value == null)
		{
			if (candidate.Value == null)
			{
				return true;
			}
			return false;
		}
		if (candidate.DisplayValue == filterValue.DisplayValue)
		{
			return true;
		}
		if (filterValue.Value.GetType() == typeof(bool))
		{
			if (candidate.Value.GetType() == typeof(bool))
			{
				return candidate.Value.Equals(filterValue.Value);
			}
			return false;
		}
		if (filterValue.Value.GetType() == typeof(DateTime))
		{
			if (candidate.Value.GetType() == typeof(DateTime))
			{
				return ((DateTime)candidate.Value).Date.Equals(((DateTime)filterValue.Value).Date);
			}
			return false;
		}
		decimal dec;
		decimal result;
		decimal result2;
		return FilterValue.TryConvertDecimal(filterValue.Value, out dec) ? (decimal.TryParse(filterValue.DisplayValue, out result) && decimal.TryParse(candidate.DisplayValue, out result2) && result2.Equals(result)) : (candidate.DisplayValue == filterValue.DisplayValue);
	};

	public EqualsFilter(int col, T value)
		: base(col, value)
	{
	}

	public EqualsFilter()
	{
	}
}
