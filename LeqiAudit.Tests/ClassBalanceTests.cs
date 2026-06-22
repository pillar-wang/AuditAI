﻿﻿﻿namespace LeqiAudit.Tests;

public class ClassBalanceTests
{
    [Fact]
    public void Constructor_InitializesItemBalances()
    {
        var cb = new Leqisoft.Model.ClassBalance();
        Assert.NotNull(cb.ItemBalances);
        Assert.Empty(cb.ItemBalances);
        Assert.Equal(0m, cb.Total);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var cb = new Leqisoft.Model.ClassBalance
        {
            Total = 888.88m
        };
        cb.ItemBalances.Add(new Leqisoft.Model.AuxiliaryItem { Code = "I001" }, 100m);

        var str = cb.ToString();
        Assert.Contains("888.88", str);
        Assert.Contains("1", str);
    }

    [Fact]
    public void Clone_DeepCopies()
    {
        var cb = new Leqisoft.Model.ClassBalance { Total = 300m };
        var item = new Leqisoft.Model.AuxiliaryItem { Code = "I001" };
        cb.ItemBalances.Add(item, 150m);

        var cloned = (Leqisoft.Model.ClassBalance)cb.Clone();

        Assert.NotSame(cb, cloned);
        Assert.Equal(300m, cloned.Total);
        Assert.NotSame(cb.ItemBalances, cloned.ItemBalances);
        Assert.Equal(150m, cloned.ItemBalances[item]);
    }

    [Fact]
    public void OnlyCloneKey_PreservesKeysZeroesValues()
    {
        var cb = new Leqisoft.Model.ClassBalance { Total = 300m };
        var item = new Leqisoft.Model.AuxiliaryItem { Code = "I001" };
        cb.ItemBalances.Add(item, 150m);

        var cloned = cb.OnlyCloneKey();

        Assert.NotSame(cb, cloned);
        Assert.Equal(0m, cloned.Total);
        Assert.True(cloned.ItemBalances.ContainsKey(item));
        Assert.Equal(0m, cloned.ItemBalances[item]);
    }
}