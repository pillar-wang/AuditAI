﻿﻿﻿using Leqisoft.Model;

namespace LeqiAudit.Tests;

public class ConvertExTests
{
    [Theory]
    [InlineData(0.0, 0)]
    [InlineData(123.45, 123.45)]
    [InlineData(-99.99, -99.99)]
    [InlineData(1e10, 10000000000)]
    [InlineData(1e-10, 0.0000000001)]
    public void ToDecimalSafe_NormalValues_Converts(double input, double expected)
    {
        var result = input.ToDecimalSafe();
        Assert.Equal((decimal)expected, result);
    }

    [Fact]
    public void ToDecimalSafe_LessThanMinDouble_ReturnsDecimalMinValue()
    {
        var result = (-8e28).ToDecimalSafe();
        Assert.Equal(decimal.MinValue, result);
    }

    [Fact]
    public void ToDecimalSafe_GreaterThanMaxDouble_ReturnsDecimalMaxValue()
    {
        var result = 8e28.ToDecimalSafe();
        Assert.Equal(decimal.MaxValue, result);
    }

    [Fact]
    public void ToDecimalSafe_ClampsToMinValue()
    {
        var result = double.MinValue.ToDecimalSafe();
        Assert.Equal(decimal.MinValue, result);
    }

    [Fact]
    public void ToDecimalSafe_ClampsToMaxValue()
    {
        var result = double.MaxValue.ToDecimalSafe();
        Assert.Equal(decimal.MaxValue, result);
    }
}