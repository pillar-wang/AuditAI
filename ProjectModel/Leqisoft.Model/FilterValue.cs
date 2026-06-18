using System;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class FilterValue
{
	public FilterDataType DataType { get; set; }

	public double Number { get; set; }

	public string Text { get; set; }

	public DateTime Date { get; set; }

	public bool Bool { get; set; }

	public TimeSpan Time { get; set; }

	public string Display { get; set; }

	public DateYearMonth DateYearMonth { get; set; }

	public static FilterValue FromObject(object value, string display)
	{
		if (!(value is double) && !(value is decimal) && !(value is float) && !(value is int) && !(value is long))
		{
			if (!(value is string text))
			{
				if (!(value is DateTime dateTime))
				{
					if (!(value is bool @bool))
					{
						if (!(value is TimeSpan timeSpan))
						{
							if (value is DateYearMonth dateYearMonth)
							{
								return new FilterValue
								{
									DataType = FilterDataType.DateYearMonth,
									DateYearMonth = dateYearMonth,
									Display = display
								};
							}
							throw new ArgumentOutOfRangeException();
						}
						return new FilterValue
						{
							DataType = FilterDataType.Time,
							Time = TimeSpan.FromSeconds(Math.Floor(timeSpan.TotalSeconds)),
							Display = display
						};
					}
					return new FilterValue
					{
						DataType = FilterDataType.Bool,
						Bool = @bool,
						Display = display
					};
				}
				return new FilterValue
				{
					DataType = FilterDataType.Date,
					Date = dateTime.Date,
					Display = display
				};
			}
			return new FilterValue
			{
				DataType = FilterDataType.Text,
				Text = text,
				Display = display
			};
		}
		return new FilterValue
		{
			DataType = FilterDataType.Number,
			Number = Convert.ToDouble(value),
			Display = display
		};
	}

	public override bool Equals(object obj)
	{
		if (obj is FilterValue filterValue)
		{
			if (DataType == filterValue.DataType)
			{
				return DataType switch
				{
					FilterDataType.None => true, 
					FilterDataType.Number => Number == filterValue.Number, 
					FilterDataType.Text => Text == filterValue.Text, 
					FilterDataType.Date => Date == filterValue.Date, 
					FilterDataType.Bool => Bool == filterValue.Bool, 
					FilterDataType.Time => Time == filterValue.Time, 
					FilterDataType.DateYearMonth => DateYearMonth.IsYearMonthEqual(filterValue.DateYearMonth), 
					_ => throw new ArgumentOutOfRangeException(), 
				};
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return DataType switch
		{
			FilterDataType.None => 0, 
			FilterDataType.Number => Number.GetHashCode(), 
			FilterDataType.Text => Text.GetHashCode(), 
			FilterDataType.Date => Date.GetHashCode(), 
			FilterDataType.Bool => Bool.GetHashCode(), 
			FilterDataType.Time => Time.GetHashCode(), 
			FilterDataType.DateYearMonth => DateYearMonth.GetHashCode(), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
