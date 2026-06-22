﻿﻿﻿namespace LeqiAudit.Tests;

public class ModelToStringTests
{
    [Fact]
    public void AuxiliaryClass_ToString_Formats()
    {
        var ac = new Leqisoft.Model.AuxiliaryClass
        {
            Code = "C001",
            Name = "部门核算"
        };
        Assert.Equal("C001 部门核算", ac.ToString());
    }

    [Fact]
    public void AuxiliaryClass_ToString_NoName()
    {
        var ac = new Leqisoft.Model.AuxiliaryClass
        {
            Code = "C001"
        };
        Assert.Equal("C001 ", ac.ToString());
    }

    [Fact]
    public void AuxiliaryItem_ToString_Formats()
    {
        var item = new Leqisoft.Model.AuxiliaryItem
        {
            Code = "I001",
            Name = "财务部"
        };
        Assert.Equal("I001 财务部", item.ToString());
    }

    [Fact]
    public void MonthSubsidiaryLedger_ToString_Formats()
    {
        var msl = new Leqisoft.Model.MonthSubsidiaryLedger(2024, 6);
        var str = msl.ToString();
        Assert.Contains("2024年6月", str);
        Assert.Contains("0笔凭证", str);
    }

    [Fact]
    public void SubsidiaryLedgerEntry_ToString_Formats()
    {
        var voucher = new Leqisoft.Model.Voucher
        {
            Day = new DateTime(2024, 6, 15),
            Number = "1",
            Amount = 500m,
            IsDebit = true,
            Account = new Leqisoft.Model.Account { Name = "现金" },
            Type = new Leqisoft.Model.VoucherType { Name = "记" }
        };

        var entry = new Leqisoft.Model.SubsidiaryLedgerEntry(voucher, 1000m);
        var str = entry.ToString();
        Assert.Contains("余额1000", str);
    }

    [Fact]
    public void SubsidiaryLedgerTotal_ToString_Formats()
    {
        var total = new Leqisoft.Model.SubsidiaryLedgerTotal
        {
            Debit = 1000m,
            Credit = 500m,
            Balance = 500m
        };

        var str = total.ToString();
        Assert.Contains("借方发生额 1000", str);
        Assert.Contains("贷方发生额 500", str);
        Assert.Contains("余额 500", str);
    }

    [Fact]
    public void VoucherType_ToString_UsesName()
    {
        var vt = new Leqisoft.Model.VoucherType
        {
            Name = "记"
        };
        Assert.Equal("记", vt.ToString());
    }

    [Fact]
    public void Currency_ToString_UsesName()
    {
        var c = new Leqisoft.Model.Currency { Name = "人民币" };
        var str = c.ToString();
        // ToString 默认返回类型名，因为 Currency 没有重写 ToString
        Assert.Equal("Leqisoft.Model.Currency", str);
    }

    [Fact]
    public void SubsidiaryLedger_Constructor_InitializesMonths()
    {
        var sl = new Leqisoft.Model.SubsidiaryLedger();
        Assert.NotNull(sl.Months);
        Assert.Empty(sl.Months);
    }

    [Fact]
    public void TrialBalanceSheet_Constructor()
    {
        var tbs = new Leqisoft.Model.TrialBalanceSheet();
        Assert.Null(tbs.Start);
        Assert.Null(tbs.Debit);
        Assert.Null(tbs.Credit);
        Assert.Null(tbs.End);
    }

    [Fact]
    public void ReceivableAgeValue_Defaults()
    {
        var rav = new Leqisoft.Model.ReceivableAgeValue();
        Assert.Equal(0, rav.End);
        Assert.False(rav.IsDebit);
        Assert.Equal(0, rav.Opposite);
        Assert.Null(rav.Values);
    }

    [Fact]
    public void ReceivableAgeEntry_InitializesCollections()
    {
        var rae = new Leqisoft.Model.ReceivableAgeEntry();
        Assert.NotNull(rae.Value);
        Assert.NotNull(rae.Aux);
        Assert.Empty(rae.Aux);
    }

    [Fact]
    public void ReceivableAgeSheet_InitializesEntries()
    {
        var ras = new Leqisoft.Model.ReceivableAgeSheet();
        Assert.NotNull(ras.Entries);
        Assert.Empty(ras.Entries);
        Assert.Equal(0, ras.YearCount);
    }
}