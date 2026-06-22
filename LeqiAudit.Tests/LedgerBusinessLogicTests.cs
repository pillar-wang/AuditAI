﻿using Leqisoft.Model;

namespace LeqiAudit.Tests;

public class LedgerBusinessLogicTests
{
    private static Ledger CreateTestLedger()
    {
        var ledger = new Ledger();
        ledger.SetStartDate(new DateTime(2024, 1, 1));
        ledger.SetCompanyName("测试公司");

        // Accounts
        var cash = new Account { Name = "现金", IsDebit = true };
        var bank = new Account { Name = "银行存款", IsDebit = true };
        var accountsPayable = new Account { Name = "应付账款", IsDebit = false };
        var revenue = new Account { Name = "主营业务收入", IsDebit = false };
        var expense = new Account { Name = "管理费用", IsDebit = true };

        ledger.Accounts.Add(cash);
        ledger.Accounts.Add(bank);
        ledger.Accounts.Add(accountsPayable);
        ledger.Accounts.Add(revenue);
        ledger.Accounts.Add(expense);

        // Initial balance
        ledger.InitialBalance.Add(cash, new AccountBalance { Total = 10000m });
        ledger.InitialBalance.Add(bank, new AccountBalance { Total = 50000m });
        ledger.InitialBalance.Add(accountsPayable, new AccountBalance { Total = 30000m });
        ledger.InitialBalance.Add(revenue, new AccountBalance { Total = 0m });
        ledger.InitialBalance.Add(expense, new AccountBalance { Total = 0m });

        // Voucher type
        var voucherType = new VoucherType { Name = "记" };

        // Vouchers
        var v1 = new Voucher
        {
            Type = voucherType,
            Number = "1",
            Day = new DateTime(2024, 1, 15),
            Account = expense,
            IsDebit = true,
            Amount = 5000m
        };
        var v2 = new Voucher
        {
            Type = voucherType,
            Number = "1",
            Day = new DateTime(2024, 1, 15),
            Account = cash,
            IsDebit = false,
            Amount = 5000m
        };
        var v3 = new Voucher
        {
            Type = voucherType,
            Number = "2",
            Day = new DateTime(2024, 2, 1),
            Account = bank,
            IsDebit = false,
            Amount = 10000m
        };
        var v4 = new Voucher
        {
            Type = voucherType,
            Number = "2",
            Day = new DateTime(2024, 2, 1),
            Account = accountsPayable,
            IsDebit = true,
            Amount = 10000m
        };

        ledger.Vouchers.Add(v1);
        ledger.Vouchers.Add(v2);
        ledger.Vouchers.Add(v3);
        ledger.Vouchers.Add(v4);

        return ledger;
    }

    [Fact]
    public void GetDebits_January_ReturnsCorrectAmounts()
    {
        var ledger = CreateTestLedger();
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);

        var debits = ledger.GetDebits(start, end);

        // Only voucher v1 is a debit in January (expense, 5000)
        Assert.Equal(5000m, debits[ledger.Accounts[4]].Total); // expense
        // cash v2 is credit, so should not appear in debits
        Assert.Equal(0m, debits[ledger.Accounts[0]].Total); // cash
    }

    [Fact]
    public void GetCredits_January_ReturnsCorrectAmounts()
    {
        var ledger = CreateTestLedger();
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 1, 31);

        var credits = ledger.GetCredits(start, end);

        // Only voucher v2 is a credit in January (cash, 5000)
        Assert.Equal(5000m, credits[ledger.Accounts[0]].Total); // cash
        // expense v1 is debit, so should not appear in credits
        Assert.Equal(0m, credits[ledger.Accounts[4]].Total); // expense
    }

    [Fact]
    public void GetDebits_FullYear_IncludesAll()
    {
        var ledger = CreateTestLedger();
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);

        var debits = ledger.GetDebits(start, end);

        // v1 debit 5000 (expense) + v4 debit 10000 (accountsPayable)
        Assert.Equal(5000m, debits[ledger.Accounts[4]].Total); // expense
        Assert.Equal(10000m, debits[ledger.Accounts[2]].Total); // accountsPayable
        Assert.Equal(0m, debits[ledger.Accounts[1]].Total); // bank (credit only)
    }

    [Fact]
    public void GetCredits_FullYear_IncludesAll()
    {
        var ledger = CreateTestLedger();
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);

        var credits = ledger.GetCredits(start, end);

        // v2 credit 5000 (cash) + v3 credit 10000 (bank)
        Assert.Equal(5000m, credits[ledger.Accounts[0]].Total); // cash
        Assert.Equal(10000m, credits[ledger.Accounts[1]].Total); // bank
    }

    [Fact]
    public void GetDebits_EmptyPeriod_ReturnsAllZeros()
    {
        var ledger = CreateTestLedger();
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 12, 31);

        var debits = ledger.GetDebits(start, end);

        // No vouchers in 2023
        foreach (var account in ledger.Accounts)
        {
            Assert.Equal(0m, debits[account].Total);
        }
    }

    [Fact]
    public void GetOppositeAccount_SameVoucherNumber_ReturnsOppositeAccounts()
    {
        var ledger = CreateTestLedger();
        var voucher = ledger.Vouchers[0]; // expense debit 5000

        var opposite = ledger.GetOppositeAccount(voucher);

        // v1 and v2 have same type/number/day: expense 5000 debit, cash 5000 credit
        // So opposite accounts of expense should be [cash's top parent]
        Assert.NotEmpty(opposite);
        Assert.Contains(ledger.Accounts[0], opposite); // cash
    }

    [Fact]
    public void GetOppositeAccount_SoloVoucher_ReturnsEmpty()
    {
        var ledger = CreateTestLedger();
        // Create a solo voucher with unique number
        var soloVoucher = new Voucher
        {
            Type = new VoucherType { Name = "记" },
            Number = "99",
            Day = new DateTime(2024, 3, 1),
            Account = ledger.Accounts[0],
            IsDebit = true,
            Amount = 100m
        };

        var opposite = ledger.GetOppositeAccount(soloVoucher);
        Assert.Empty(opposite);
    }

    [Fact]
    public void GetOppositeAccount_NullVoucherType_ThrowsNullRef()
    {
        var ledger = CreateTestLedger();
        var voucher = new Voucher
        {
            Type = null,
            Number = "999",
            Day = new DateTime(2024, 3, 1),
            Account = ledger.Accounts[0],
            IsDebit = true,
            Amount = 100m
        };

        // 原始代码没有处理 null Type，预期 NullReferenceException
        Assert.Throws<NullReferenceException>(() => ledger.GetOppositeAccount(voucher));
    }

    [Fact]
    public void GetSubsidiaryLedger_BasicAccount_ReturnsLedger()
    {
        var ledger = CreateTestLedger();
        var cash = ledger.Accounts[0];
        var start = new DateTime(2024, 1, 1);
        var end = new DateTime(2024, 12, 31);

        var subLedger = ledger.GetSubsidiaryLedger(cash, start, end);

        Assert.NotNull(subLedger);
        Assert.Equal(cash, subLedger.Account);
        Assert.Equal(start, subLedger.Start);
        Assert.Equal(end, subLedger.End);
        Assert.Equal(10000m, subLedger.BeginBalance); // initial balance of cash

        // Should have 1 month with data (January has v2 entry)
        var monthsWithEntries = subLedger.Months.Where(m => m.Entries.Count > 0).ToList();
        Assert.Single(monthsWithEntries);
        Assert.Equal(1, monthsWithEntries[0].Month);
    }
}