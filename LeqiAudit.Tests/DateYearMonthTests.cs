﻿﻿﻿namespace LeqiAudit.Tests;

public class DateYearMonthTests
{
    [Fact]
    public void DefaultConstructor_SetsMinValue()
    {
        var ym = new Leqisoft.DTO.DateYearMonth();
        Assert.Equal(DateTime.MinValue, ym.Date);
    }

    [Fact]
    public void ParameterizedConstructor_SetsDate()
    {
        var dt = new DateTime(2024, 6, 15);
        var ym = new Leqisoft.DTO.DateYearMonth(dt);
        Assert.Equal(dt, ym.Date);
    }

    [Fact]
    public void IsZero_ReturnsTrueForMinValue()
    {
        var ym = new Leqisoft.DTO.DateYearMonth();
        Assert.True(ym.IsZero);
    }

    [Fact]
    public void IsZero_ReturnsFalseForRealDate()
    {
        var ym = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 1));
        Assert.False(ym.IsZero);
    }

    [Fact]
    public void ToString_DefaultFormat()
    {
        var ym = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 15));
        var result = ym.ToString();
        Assert.Equal("2024年6月", result);
    }

    [Fact]
    public void ToString_WithCustomFormat()
    {
        var ym = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 15))
        {
            ToStringFormat = "yyyy/MM"
        };
        var result = ym.ToString();
        Assert.Equal("2024/06", result);
    }

    [Fact]
    public void Equals_SameYearMonth_ReturnsTrue()
    {
        var a = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 1));
        var b = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 30));
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentMonth_ReturnsFalse()
    {
        var a = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 1));
        var b = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 7, 1));
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_NonDateYearMonth_ReturnsFalse()
    {
        var ym = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 1));
        Assert.False(ym.Equals("some string"));
    }

    [Fact]
    public void IsYearMonthEqual_SameYearMonth_ReturnsTrue()
    {
        var a = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 1));
        var b = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 30));
        Assert.True(a.IsYearMonthEqual(b));
    }

    [Fact]
    public void IsYearMonthEqual_WithDateTime_ReturnsTrue()
    {
        var ym = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 1));
        Assert.True(ym.IsYearMonthEqual(new DateTime(2024, 6, 30)));
    }

    [Fact]
    public void IsYearMonthEqual_WithDateTime_ReturnsFalse()
    {
        var ym = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 1));
        Assert.False(ym.IsYearMonthEqual(new DateTime(2024, 7, 1)));
    }

    [Fact]
    public void AddMonths_ReturnsNewInstance()
    {
        var ym = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 1));
        var result = ym.AddMonths(3);
        Assert.Equal(2024, result.Date.Year);
        Assert.Equal(9, result.Date.Month);
    }

    [Fact]
    public void AddYears_ReturnsNewInstance()
    {
        var ym = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 1));
        var result = ym.AddYears(1);
        Assert.Equal(2025, result.Date.Year);
        Assert.Equal(6, result.Date.Month);
    }

    [Fact]
    public void CompareTo_SameDate_ReturnsZero()
    {
        var a = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 1));
        var b = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 30));
        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void CompareTo_EarlierDate_ReturnsNegative()
    {
        var a = new Leqisoft.DTO.DateYearMonth(new DateTime(2023, 1, 1));
        var b = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 1, 1));
        Assert.True(a.CompareTo(b) < 0);
    }

    [Fact]
    public void CompareTo_LaterDate_ReturnsPositive()
    {
        var a = new Leqisoft.DTO.DateYearMonth(new DateTime(2025, 1, 1));
        var b = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 1, 1));
        Assert.True(a.CompareTo(b) > 0);
    }

    [Fact]
    public void CompareTo_WithDateTime_WorksCorrectly()
    {
        var ym = new Leqisoft.DTO.DateYearMonth(new DateTime(2024, 6, 1));
        Assert.True(ym.CompareTo(new DateTime(2024, 7, 1)) < 0);
        Assert.True(ym.CompareTo(new DateTime(2024, 5, 1)) > 0);
        Assert.Equal(0, ym.CompareTo(new DateTime(2024, 6, 30)));
    }
}
