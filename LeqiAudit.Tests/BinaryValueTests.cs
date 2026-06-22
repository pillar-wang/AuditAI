﻿﻿﻿namespace LeqiAudit.Tests;

public class BinaryValueTests
{
    [Fact]
    public void String_RoundTrip()
    {
        var bv = new Leqisoft.DTO.BinaryValue("hello");
        Assert.Equal("hello", bv.Value);
        var bytes = bv.GetBytes();
        var restored = new Leqisoft.DTO.BinaryValue(bytes);
        Assert.Equal("hello", restored.Value);
    }

    [Fact]
    public void Number_RoundTrip()
    {
        var bv = new Leqisoft.DTO.BinaryValue(3.14);
        Assert.Equal(3.14, (double)bv.Value, precision: 2);
        var bytes = bv.GetBytes();
        var restored = new Leqisoft.DTO.BinaryValue(bytes);
        Assert.Equal(3.14, (double)restored.Value, precision: 2);
    }

    [Fact]
    public void Boolean_RoundTrip()
    {
        var bv = new Leqisoft.DTO.BinaryValue(true);
        Assert.True((bool)bv.Value);
        var bytes = bv.GetBytes();
        var restored = new Leqisoft.DTO.BinaryValue(bytes);
        Assert.True((bool)restored.Value);
    }

    [Fact]
    public void DateTime_RoundTrip()
    {
        var dt = new DateTime(2024, 6, 15, 14, 30, 0);
        var bv = new Leqisoft.DTO.BinaryValue(dt);
        Assert.Equal(dt, (DateTime)bv.Value);
        var bytes = bv.GetBytes();
        var restored = new Leqisoft.DTO.BinaryValue(bytes);
        Assert.Equal(dt, (DateTime)restored.Value);
    }

    [Fact]
    public void TimeSpan_RoundTrip()
    {
        var ts = new TimeSpan(10, 30, 0);
        var bv = new Leqisoft.DTO.BinaryValue(ts);
        Assert.Equal(ts, (TimeSpan)bv.Value);
        var bytes = bv.GetBytes();
        var restored = new Leqisoft.DTO.BinaryValue(bytes);
        Assert.Equal(ts, (TimeSpan)restored.Value);
    }

    [Fact]
    public void FromObject_String()
    {
        var bv = Leqisoft.DTO.BinaryValue.FromObject("test");
        Assert.Equal("test", bv.Value);
    }

    [Fact]
    public void FromObject_Int()
    {
        var bv = Leqisoft.DTO.BinaryValue.FromObject(42);
        Assert.Equal(42.0, (double)bv.Value);
    }

    [Fact]
    public void FromObject_Double()
    {
        var bv = Leqisoft.DTO.BinaryValue.FromObject(3.14);
        Assert.Equal(3.14, (double)bv.Value, precision: 2);
    }

    [Fact]
    public void FromObject_Bool()
    {
        var bv = Leqisoft.DTO.BinaryValue.FromObject(false);
        Assert.False((bool)bv.Value);
    }

    [Fact]
    public void FromObject_ThrowsForUnsupportedType()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Leqisoft.DTO.BinaryValue.FromObject(Guid.NewGuid()));
    }

    [Fact]
    public void AdditionalData_RoundTrip()
    {
        var bv = new Leqisoft.DTO.BinaryValue("data");
        bv.SetAdditionalData(new byte[] { 1, 2, 3 });
        var bytes = bv.GetBytes();
        var restored = new Leqisoft.DTO.BinaryValue(bytes);
        Assert.Equal("data", restored.Value);
        Assert.Equal(new byte[] { 1, 2, 3 }, restored.AdditionalData);
    }

    [Fact]
    public void SetAdditionalDataExistFlag_SetsBit()
    {
        var result = Leqisoft.DTO.BinaryValue.SetAdditionalDataExistFlag(0x05, isHasAdditionalData: true);
        Assert.Equal(0x85, result);
    }

    [Fact]
    public void SetAdditionalDataExistFlag_ClearsBit()
    {
        var result = Leqisoft.DTO.BinaryValue.SetAdditionalDataExistFlag(0x85, isHasAdditionalData: false);
        Assert.Equal(0x05, result);
    }

    [Fact]
    public void GetAdditionalDataExistFlag_Returns1WhenSet()
    {
        Assert.Equal(1, Leqisoft.DTO.BinaryValue.GetAdditionalDataExistFlag(0x85));
    }

    [Fact]
    public void GetAdditionalDataExistFlag_Returns0WhenNotSet()
    {
        Assert.Equal(0, Leqisoft.DTO.BinaryValue.GetAdditionalDataExistFlag(0x05));
    }

    [Fact]
    public void ToString_ReturnsValueString()
    {
        var bv = new Leqisoft.DTO.BinaryValue("hello");
        Assert.Equal("hello", bv.ToString());
    }
}
