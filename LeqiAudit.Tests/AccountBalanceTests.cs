﻿﻿﻿namespace LeqiAudit.Tests;

public class AccountBalanceTests
{
    [Fact]
    public void Constructor_InitializesClassBalances()
    {
        var ab = new Leqisoft.Model.AccountBalance();
        Assert.NotNull(ab.ClassBalances);
        Assert.Empty(ab.ClassBalances);
        Assert.Equal(0m, ab.Total);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var ab = new Leqisoft.Model.AccountBalance
        {
            Total = 123.45m
        };
        ab.ClassBalances.Add(new Leqisoft.Model.AuxiliaryClass(), new Leqisoft.Model.ClassBalance());

        var str = ab.ToString();
        Assert.Contains("123.45", str);
        Assert.Contains("1", str);
    }

    [Fact]
    public void ToString_WithNoClassBalances()
    {
        var ab = new Leqisoft.Model.AccountBalance { Total = 0m };
        var str = ab.ToString();
        Assert.Contains("0", str);
        Assert.Contains("0", str);
    }

    [Fact]
    public void Clone_DeepCopies()
    {
        var ab = new Leqisoft.Model.AccountBalance { Total = 100m };
        var auxClass = new Leqisoft.Model.AuxiliaryClass { Code = "C1" };
        ab.ClassBalances.Add(auxClass, new Leqisoft.Model.ClassBalance { Total = 50m });

        var cloned = (Leqisoft.Model.AccountBalance)ab.Clone();

        Assert.NotSame(ab, cloned);
        Assert.Equal(100m, cloned.Total);
        Assert.NotSame(ab.ClassBalances, cloned.ClassBalances);
        Assert.True(cloned.ClassBalances.ContainsKey(auxClass));
        Assert.Equal(50m, cloned.ClassBalances[auxClass].Total);

        // Modify original to verify deep copy
        ab.Total = 200m;
        ab.ClassBalances[auxClass].Total = 99m;
        Assert.Equal(100m, cloned.Total);
        Assert.Equal(50m, cloned.ClassBalances[auxClass].Total);
    }

    [Fact]
    public void OnlyCloneKey_PreservesKeysZeroesValues()
    {
        var ab = new Leqisoft.Model.AccountBalance { Total = 100m };
        var auxClass = new Leqisoft.Model.AuxiliaryClass { Code = "C1" };
        ab.ClassBalances.Add(auxClass, new Leqisoft.Model.ClassBalance { Total = 50m });

        var cloned = ab.OnlyCloneKey();

        Assert.NotSame(ab, cloned);
        Assert.Equal(0m, cloned.Total);
        Assert.True(cloned.ClassBalances.ContainsKey(auxClass));
        Assert.Equal(0m, cloned.ClassBalances[auxClass].Total);
    }
}