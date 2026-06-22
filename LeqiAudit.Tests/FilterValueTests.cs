﻿﻿﻿using Leqisoft.Model;

namespace LeqiAudit.Tests;

public class FilterValueTests
{
    [Fact]
    public void FromObject_NullValue_ReturnsFilterValueWithEmptyDisplay()
    {
        var fv = FilterValue.FromObject(null);
        Assert.Null(fv.Value);
        Assert.Equal(string.Empty, fv.DisplayValue);
    }

    [Fact]
    public void FromObject_StringValue_SetsValueAndDisplay()
    {
        var fv = FilterValue.FromObject("hello");
        Assert.Equal("hello", fv.Value);
        Assert.Equal("hello", fv.DisplayValue);
    }

    [Fact]
    public void FromObject_IntValue_SetsValueAndDisplay()
    {
        var fv = FilterValue.FromObject(42);
        Assert.Equal(42, fv.Value);
        Assert.Equal("42", fv.DisplayValue);
    }

    [Fact]
    public void FromObject_FilterValue_ReturnsSameInstance()
    {
        var original = new FilterValue { Value = 1, DisplayValue = "1" };
        var result = FilterValue.FromObject(original);
        Assert.Same(original, result);
    }

    [Fact]
    public void TryConvertDecimal_Int_ReturnsTrue()
    {
        var result = FilterValue.TryConvertDecimal(123, out var dec);
        Assert.True(result);
        Assert.Equal(123m, dec);
    }

    [Fact]
    public void TryConvertDecimal_Double_ReturnsTrue()
    {
        var result = FilterValue.TryConvertDecimal(45.67, out var dec);
        Assert.True(result);
        Assert.Equal(45.67m, dec);
    }

    [Fact]
    public void TryConvertDecimal_String_ReturnsFalse()
    {
        var result = FilterValue.TryConvertDecimal("not a number", out _);
        Assert.False(result);
    }

    [Fact]
    public void CompareTo_NullValue_ReturnsMinusOne()
    {
        var fv = new FilterValue { Value = null };
        var other = new FilterValue { Value = "a" };
        Assert.Equal(-1, fv.CompareTo(other));
    }

    [Fact]
    public void CompareTo_OtherNull_ReturnsOne()
    {
        var fv = new FilterValue { Value = "a" };
        var other = new FilterValue { Value = null };
        Assert.Equal(1, fv.CompareTo(other));
    }

    [Fact]
    public void CompareTo_BothNull_ReturnsZero()
    {
        var fv = new FilterValue { Value = null };
        var other = new FilterValue { Value = null };
        Assert.Equal(0, fv.CompareTo(other));
    }

    [Fact]
    public void CompareTo_NumericValues_ComparesCorrectly()
    {
        var a = FilterValue.FromObject(10);
        var b = FilterValue.FromObject(20);
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(b.CompareTo(a) > 0);
        Assert.Equal(0, a.CompareTo(FilterValue.FromObject(10)));
    }

    [Fact]
    public void CompareTo_StringValues_ComparesCorrectly()
    {
        var a = FilterValue.FromObject("apple");
        var b = FilterValue.FromObject("banana");
        Assert.True(a.CompareTo(b) < 0);
    }

    [Fact]
    public void CompareTo_DifferentTypes_ReturnsMinusTwo()
    {
        var a = FilterValue.FromObject(123);
        var b = FilterValue.FromObject("abc");
        Assert.Equal(-2, a.CompareTo(b));
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var a = FilterValue.FromObject(42);
        var b = FilterValue.FromObject(42);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var a = FilterValue.FromObject(42);
        var b = FilterValue.FromObject(43);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        var a = new FilterValue { Value = null };
        var b = new FilterValue { Value = null };
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Trim_StringValue_TrimsWhitespace()
    {
        var fv = FilterValue.FromObject("  hello  ");
        fv.Trim();
        Assert.Equal("hello", fv.Value);
    }

    [Fact]
    public void Trim_NonStringValue_DoesNotChange()
    {
        var fv = FilterValue.FromObject(42);
        fv.Trim();
        Assert.Equal(42, fv.Value);
    }
}