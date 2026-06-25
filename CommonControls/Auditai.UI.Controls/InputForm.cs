using System;
using Auditai.DTO;

namespace Auditai.UI.Controls;

public static class InputForm
{
	public static decimal? Numeric(string title, string prompt, decimal? _default, Func<string, bool> validCallback)
	{
		InputBoxImpl inputBoxImpl = new InputBoxImpl(title, prompt);
		inputBoxImpl.ValidCallback = validCallback;
		if (_default.HasValue)
		{
			inputBoxImpl.SetInputTextValue(_default.ToString());
		}
		inputBoxImpl.ShowDialog();
		if (inputBoxImpl.Valid)
		{
			if (decimal.TryParse(inputBoxImpl.Value?.ToString(), out var result))
			{
				return result;
			}
			return null;
		}
		return null;
	}

	public static decimal? Numeric(string title, string prompt, decimal? _default = null)
	{
		InputBoxImpl inputBoxImpl = new InputBoxImpl(title, prompt);
		if (_default.HasValue)
		{
			inputBoxImpl.SetInputTextValue(_default.ToString());
		}
		inputBoxImpl.ShowDialog();
		if (inputBoxImpl.Valid)
		{
			if (decimal.TryParse(inputBoxImpl.Value?.ToString(), out var result))
			{
				return result;
			}
			return null;
		}
		return null;
	}

	public static DateTime? DateInput(string title, string prompt, DateTime? defaultValue = null)
	{
		InputBoxImpl inputBoxImpl = new InputBoxImpl(title, prompt, InputFormEnum.Date);
		if (defaultValue.HasValue)
		{
			inputBoxImpl.SetInputDateValue(defaultValue.Value);
		}
		inputBoxImpl.ShowDialog();
		if (inputBoxImpl.Valid)
		{
			return Convert.ToDateTime(inputBoxImpl.Value);
		}
		return null;
	}

	public static DateYearMonth? DateYearMonthInput(string title, string prompt, DateYearMonth? defaultValue = null)
	{
		InputBoxImpl inputBoxImpl = new InputBoxImpl(title, prompt, InputFormEnum.DateYearMonth);
		if (defaultValue.HasValue)
		{
			inputBoxImpl.SetInputDateYearMonthValue(defaultValue.Value);
		}
		inputBoxImpl.ShowDialog();
		if (inputBoxImpl.Valid)
		{
			return new DateYearMonth(Convert.ToDateTime(inputBoxImpl.Value));
		}
		return null;
	}

	public static string Text(string title, string prompt, string value = null, int inputBoxWidth = 128)
	{
		InputBoxImpl inputBoxImpl = new InputBoxImpl(title, prompt, InputFormEnum.Text);
		inputBoxImpl.SetInputLeftWidth(inputBoxWidth);
		inputBoxImpl.SetInputTextValue(value);
		inputBoxImpl.ShowDialog();
		if (inputBoxImpl.Valid)
		{
			return inputBoxImpl.Value.ToString();
		}
		return null;
	}

	public static string Password(string title, string prompt, string value = null, int inputBoxWidth = 128)
	{
		InputBoxImpl inputBoxImpl = new InputBoxImpl(title, prompt, InputFormEnum.Text);
		inputBoxImpl.SetInputLeftWidth(inputBoxWidth);
		inputBoxImpl.SetInputTextValue(value);
		inputBoxImpl.SetInputTextForPassword();
		inputBoxImpl.ShowDialog();
		if (inputBoxImpl.Valid)
		{
			return inputBoxImpl.Value.ToString();
		}
		return null;
	}

	public static string MultiText(string title, string prompt)
	{
		InputBoxImpl inputBoxImpl = new InputBoxImpl(title, prompt, InputFormEnum.MultiText);
		inputBoxImpl.ShowDialog();
		if (inputBoxImpl.Valid)
		{
			return inputBoxImpl.Value.ToString();
		}
		return null;
	}

	public static decimal? NumRange(string title, string prompt, out decimal min, out decimal max)
	{
		min = default(decimal);
		max = default(decimal);
		InputBoxImpl inputBoxImpl = new InputBoxImpl(title, prompt, InputFormEnum.NumRange);
		inputBoxImpl.ShowDialog();
		if (inputBoxImpl.Valid)
		{
			Tuple<decimal, decimal> tuple = inputBoxImpl.Value as Tuple<decimal, decimal>;
			min = tuple.Item1;
			max = tuple.Item2;
			return tuple.Item1;
		}
		return null;
	}

	public static DateTime? DateRange(string title, string prompt, out DateTime min, out DateTime max)
	{
		min = DateTime.MinValue;
		max = DateTime.MaxValue;
		InputBoxImpl inputBoxImpl = new InputBoxImpl(title, prompt, InputFormEnum.DateRange);
		inputBoxImpl.ShowDialog();
		if (inputBoxImpl.Valid)
		{
			Tuple<DateTime, DateTime> tuple = inputBoxImpl.Value as Tuple<DateTime, DateTime>;
			min = tuple.Item1;
			max = tuple.Item2;
			return min;
		}
		return null;
	}

	public static DateYearMonth? DateYearMonthRange(string title, string prompt, out DateYearMonth min, out DateYearMonth max)
	{
		min = new DateYearMonth(DateTime.MinValue);
		max = new DateYearMonth(DateTime.MaxValue);
		InputBoxImpl inputBoxImpl = new InputBoxImpl(title, prompt, InputFormEnum.DateYearMonthRange);
		inputBoxImpl.ShowDialog();
		if (inputBoxImpl.Valid)
		{
			Tuple<DateTime, DateTime> tuple = inputBoxImpl.Value as Tuple<DateTime, DateTime>;
			min = new DateYearMonth(tuple.Item1);
			max = new DateYearMonth(tuple.Item2);
			return min;
		}
		return null;
	}

	public static TimeSpan? Time(string title, string prompt)
	{
		InputBoxImpl inputBoxImpl = new InputBoxImpl(title, prompt, InputFormEnum.Time);
		inputBoxImpl.ShowDialog();
		if (inputBoxImpl.Valid)
		{
			return TimeSpan.FromSeconds(Math.Floor(((DateTime)inputBoxImpl.Value).TimeOfDay.TotalSeconds));
		}
		return null;
	}

	public static Tuple<TimeSpan, TimeSpan> TimeRange(string title, string prompt)
	{
		InputBoxImpl inputBoxImpl = new InputBoxImpl(title, prompt, InputFormEnum.TimeRange);
		inputBoxImpl.ShowDialog();
		if (inputBoxImpl.Valid)
		{
			TimeSpan timeOfDay = ((DateTime)((Tuple<object, object>)inputBoxImpl.Value).Item1).TimeOfDay;
			TimeSpan timeOfDay2 = ((DateTime)((Tuple<object, object>)inputBoxImpl.Value).Item2).TimeOfDay;
			return Tuple.Create(TimeSpan.FromSeconds(Math.Floor(timeOfDay.TotalSeconds)), TimeSpan.FromSeconds(Math.Floor(timeOfDay2.TotalSeconds)));
		}
		return null;
	}
}
