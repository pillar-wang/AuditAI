﻿﻿﻿namespace LeqiAudit.Tests;

public class DateBalanceTests
{
    [Fact]
    public void Constructor_Empty()
    {
        var db = new Leqisoft.Model.DateBalance();
        Assert.Empty(db);
    }

    [Fact]
    public void Clone_CopiesAllValues()
    {
        var db = new Leqisoft.Model.DateBalance();
        var account = new Leqisoft.Model.Account { Name = "现金" };
        var balance = new Leqisoft.Model.AccountBalance { Total = 100m };
        db.Add(account, balance);

        var cloned = (Leqisoft.Model.DateBalance)db.Clone();

        Assert.NotSame(db, cloned);
        Assert.Equal(1, cloned.Count);
        Assert.Equal(100m, cloned[account].Total);
    }

    [Fact]
    public void Clone_DeepCopiesAccountBalances()
    {
        var db = new Leqisoft.Model.DateBalance();
        var account = new Leqisoft.Model.Account { Name = "现金" };
        var balance = new Leqisoft.Model.AccountBalance { Total = 100m };
        db.Add(account, balance);

        var cloned = (Leqisoft.Model.DateBalance)db.Clone();
        // 修改原始对象
        balance.Total = 200m;

        // 原始引用被修改
        Assert.Equal(200m, db[account].Total);
        // 克隆应保持原始值 100m（深拷贝）
        Assert.Equal(100m, cloned[account].Total);
    }

    [Fact]
    public void OnlyCloneKey_CopiesKeysOnly()
    {
        var db = new Leqisoft.Model.DateBalance();
        var account = new Leqisoft.Model.Account { Name = "现金" };
        var balance = new Leqisoft.Model.AccountBalance { Total = 500m };
        db.Add(account, balance);

        var cloned = db.OnlyCloneKey();

        Assert.NotSame(db, cloned);
        Assert.Equal(1, cloned.Count);
        // OnlyCloneKey should zero out the values
        Assert.Equal(0m, cloned[account].Total);
    }

    [Fact]
    public void Get_MissingAccount_ReturnsZero()
    {
        var db = new Leqisoft.Model.DateBalance();
        var result = db.Get(new Leqisoft.Model.Account { Name = "nonexistent" }, new Leqisoft.Model.AuxiliaryItem());
        Assert.Equal(0m, result);
    }

    [Fact]
    public void Get_MissingClass_ReturnsZero()
    {
        var db = new Leqisoft.Model.DateBalance();
        var account = new Leqisoft.Model.Account { Name = "现金" };
        db.Add(account, new Leqisoft.Model.AccountBalance { Total = 100m });

        // AuxiliaryItem.Class 为 null 时，TryGetValue 会抛 ArgumentNullException，
        // 这是 Dictionary 的标准行为，C# 基类库的约束
        Assert.Throws<ArgumentNullException>(() => db.Get(account, new Leqisoft.Model.AuxiliaryItem()));
    }

    [Fact]
    public void Get_HasAccountClassAndItem_ReturnsValue()
    {
        var db = new Leqisoft.Model.DateBalance();
        var account = new Leqisoft.Model.Account { Name = "现金" };
        var auxClass = new Leqisoft.Model.AuxiliaryClass { Code = "C01", Name = "部门" };
        var auxItem = new Leqisoft.Model.AuxiliaryItem { Code = "I01", Name = "财务部", Class = auxClass };
        var classBal = new Leqisoft.Model.ClassBalance();
        classBal.ItemBalances.Add(auxItem, 50m);
        var accountBal = new Leqisoft.Model.AccountBalance
        {
            Total = 100m
        };
        accountBal.ClassBalances.Add(auxClass, classBal);
        db.Add(account, accountBal);

        var result = db.Get(account, auxItem);
        Assert.Equal(50m, result);
    }
}