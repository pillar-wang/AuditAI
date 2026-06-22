﻿﻿﻿namespace LeqiAudit.Tests;

public class LedgerTests
{
    [Fact]
    public void Constructor_InitializesCollections()
    {
        var ledger = new Leqisoft.Model.Ledger();

        Assert.NotNull(ledger.Accounts);
        Assert.Empty(ledger.Accounts);
        Assert.NotNull(ledger.Vouchers);
        Assert.Empty(ledger.Vouchers);
        Assert.NotNull(ledger.VoucherTypes);
        Assert.Empty(ledger.VoucherTypes);
        Assert.NotNull(ledger.AuxiliaryClasses);
        Assert.Empty(ledger.AuxiliaryClasses);
        Assert.NotNull(ledger.AuxiliaryItems);
        Assert.Empty(ledger.AuxiliaryItems);
        Assert.NotNull(ledger.Currencies);
        Assert.Empty(ledger.Currencies);
        Assert.NotNull(ledger.InitialBalance);
        Assert.Empty(ledger.InitialBalance);
    }

    [Fact]
    public void SetStartDate_SetsStartDate()
    {
        var ledger = new Leqisoft.Model.Ledger();
        var date = new DateTime(2024, 1, 1);

        ledger.SetStartDate(date);

        Assert.Equal(date, ledger.StartDate);
    }

    [Fact]
    public void SetCompanyName_SetsCompanyName()
    {
        var ledger = new Leqisoft.Model.Ledger();
        ledger.SetCompanyName("测试公司");
        Assert.Equal("测试公司", ledger.CompanyName);
    }

    [Fact]
    public void RootAccounts_NoAccounts_ReturnsEmpty()
    {
        var ledger = new Leqisoft.Model.Ledger();
        Assert.Empty(ledger.RootAccounts);
    }

    [Fact]
    public void RootAccounts_WithAccounts_ReturnsOnlyParentless()
    {
        var ledger = new Leqisoft.Model.Ledger();
        var root1 = new Leqisoft.Model.Account();
        var root2 = new Leqisoft.Model.Account();
        var child = new Leqisoft.Model.Account { Parent = root1 };
        ledger.Accounts.Add(root1);
        ledger.Accounts.Add(root2);
        ledger.Accounts.Add(child);

        var roots = ledger.RootAccounts.ToList();
        Assert.Equal(2, roots.Count);
        Assert.Contains(root1, roots);
        Assert.Contains(root2, roots);
    }

    [Fact]
    public void RootAccounts_ChildWithNullParent_IsRoot()
    {
        var ledger = new Leqisoft.Model.Ledger();
        var account = new Leqisoft.Model.Account { Parent = null };
        ledger.Accounts.Add(account);
        Assert.Single(ledger.RootAccounts);
    }

    [Fact]
    public void GetLevelOrderAccounts_Empty_ReturnsEmpty()
    {
        var ledger = new Leqisoft.Model.Ledger();
        var result = ledger.GetLevelOrderAccounts().ToList();
        Assert.Empty(result);
    }
}