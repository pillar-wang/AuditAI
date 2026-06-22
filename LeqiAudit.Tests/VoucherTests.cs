﻿﻿﻿namespace LeqiAudit.Tests;

public class VoucherTests
{
    [Fact]
    public void Constructor_InitializesDetails()
    {
        var v = new Leqisoft.Model.Voucher();
        Assert.NotNull(v.Details);
        Assert.Empty(v.Details);
    }

    [Fact]
    public void ToggleDirection_FlipsIsDebitAndAmount()
    {
        var v = new Leqisoft.Model.Voucher
        {
            IsDebit = true,
            Amount = 100m,
            Dirty = 0
        };

        v.ToggleDirection();

        Assert.False(v.IsDebit);
        Assert.Equal(-100m, v.Amount);
        Assert.True(v.DirectionToggled);
        Assert.Equal(2, v.Dirty);
    }

    [Fact]
    public void ToggleDirection_Twice_ReturnsToOriginal()
    {
        var v = new Leqisoft.Model.Voucher
        {
            IsDebit = true,
            Amount = 100m
        };

        v.ToggleDirection();
        v.ToggleDirection();

        Assert.True(v.IsDebit);
        Assert.Equal(100m, v.Amount);
    }

    [Fact]
    public void ToggleMark_FlipsMarkAndSetsDirty()
    {
        var v = new Leqisoft.Model.Voucher { VoucherMark = false, Dirty = 0 };
        v.ToggleMark();
        Assert.True(v.VoucherMark);
        Assert.Equal(2, v.Dirty);
    }

    [Fact]
    public void ToggleMark_Twice_ReturnsToOriginal()
    {
        var v = new Leqisoft.Model.Voucher { VoucherMark = false };
        v.ToggleMark();
        v.ToggleMark();
        Assert.False(v.VoucherMark);
    }

    [Fact]
    public void GetDisplayAccountCodeWithDetail_NoDetails_ReturnsCode()
    {
        var account = new Leqisoft.Model.Account { Code = "1001" };
        var v = new Leqisoft.Model.Voucher { Account = account };

        Assert.Equal("1001", v.GetDisplayAccountCodeWithDetail());
    }

    [Fact]
    public void GetDisplayAccountCodeWithDetail_WithDetails_JoinsWithPipe()
    {
        var auxClass = new Leqisoft.Model.AuxiliaryClass();
        var account = new Leqisoft.Model.Account { Code = "1001" };
        var detail = new Leqisoft.Model.AuxiliaryItem { Code = "001", Class = auxClass };
        var v = new Leqisoft.Model.Voucher { Account = account };
        v.Details.Add(detail);

        Assert.Equal("1001-001", v.GetDisplayAccountCodeWithDetail());
    }

    [Fact]
    public void GetDisplayAuxiliaryClassName_NoDetails_ReturnsEmpty()
    {
        var v = new Leqisoft.Model.Voucher();
        Assert.Equal("", v.GetDisplayAuxiliaryClassName());
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var type = new Leqisoft.Model.VoucherType { Name = "记" };
        var account = new Leqisoft.Model.Account { Name = "现金" };
        var v = new Leqisoft.Model.Voucher
        {
            Type = type,
            Number = "1",
            Day = new DateTime(2024, 6, 15),
            Account = account,
            IsDebit = true,
            Amount = 500m
        };

        var str = v.ToString();
        Assert.Contains("记", str);
        Assert.Contains("1", str);
        Assert.Contains("500", str);
    }
}
