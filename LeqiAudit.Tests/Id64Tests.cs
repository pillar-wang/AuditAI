﻿﻿﻿namespace LeqiAudit.Tests;

public class Id64Tests
{
    [Fact]
    public void Constructor_AssignsValue()
    {
        var id = new Leqisoft.DTO.Id64(42);
        Assert.Equal(42, id.Value);
    }

    [Fact]
    public void Constructor_FromUpperLower_CombinesCorrectly()
    {
        var id = new Leqisoft.DTO.Id64(0x00000001, 0x00000002);
        Assert.Equal(0x00000001_00000002, id.Value);
    }

    [Fact]
    public void Zero_IsZero()
    {
        Assert.Equal(0L, Leqisoft.DTO.Id64.Zero.Value);
        Assert.True(Leqisoft.DTO.Id64.Zero.IsZero());
    }

    [Fact]
    public void IsZero_ReturnsTrueForZero()
    {
        var id = new Leqisoft.DTO.Id64(0);
        Assert.True(id.IsZero());
    }

    [Fact]
    public void IsZero_ReturnsFalseForNonZero()
    {
        var id = new Leqisoft.DTO.Id64(1);
        Assert.False(id.IsZero());
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var a = new Leqisoft.DTO.Id64(100);
        var b = new Leqisoft.DTO.Id64(100);
        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var a = new Leqisoft.DTO.Id64(100);
        var b = new Leqisoft.DTO.Id64(200);
        Assert.False(a.Equals(b));
        Assert.False(a == b);
        Assert.True(a != b);
    }

    [Fact]
    public void Equals_NonId64Object_ReturnsFalse()
    {
        var id = new Leqisoft.DTO.Id64(42);
        Assert.False(id.Equals("not an id"));
    }

    [Fact]
    public void GetHashCode_ReflectsValue()
    {
        var id = new Leqisoft.DTO.Id64(42);
        Assert.Equal(42.GetHashCode(), id.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsValueAsString()
    {
        var id = new Leqisoft.DTO.Id64(12345);
        Assert.Equal("12345", id.ToString());
    }

    [Fact]
    public void Parse_ValidString_ReturnsId()
    {
        var id = Leqisoft.DTO.Id64.Parse("999");
        Assert.Equal(999L, id.Value);
    }

    [Fact]
    public void Parse_ThrowsOnInvalidString()
    {
        Assert.Throws<FormatException>(() => Leqisoft.DTO.Id64.Parse("abc"));
    }

    [Fact]
    public void ToBase64_And_ParseBase64_RoundTrip()
    {
        var original = new Leqisoft.DTO.Id64(0x1234567890ABCDEF);
        var base64 = original.ToBase64();
        var parsed = Leqisoft.DTO.Id64.ParseBase64(base64);
        Assert.Equal(original, parsed);
    }

    [Fact]
    public void ParseBase64_HandlesMissingPadding()
    {
        var original = new Leqisoft.DTO.Id64(42);
        var base64 = original.ToBase64().TrimEnd('=');
        var parsed = Leqisoft.DTO.Id64.ParseBase64(base64);
        Assert.Equal(original, parsed);
    }

    [Fact]
    public void FromNullableLong_HasValue_ReturnsId()
    {
        var id = Leqisoft.DTO.Id64.FromNullableLong(77);
        Assert.NotNull(id);
        Assert.Equal(77, id.Value.Value);
    }

    [Fact]
    public void FromNullableLong_Null_ReturnsNull()
    {
        var id = Leqisoft.DTO.Id64.FromNullableLong(null);
        Assert.Null(id);
    }
}
