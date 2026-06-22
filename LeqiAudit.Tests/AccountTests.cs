﻿﻿﻿namespace LeqiAudit.Tests;

public class AccountTests
{
    [Fact]
    public void Constructor_InitializesEmptyChildren()
    {
        var account = new Leqisoft.Model.Account();
        Assert.NotNull(account.Children);
        Assert.Empty(account.Children);
    }

    [Fact]
    public void GetFullName_NoParent_ReturnsName()
    {
        var account = new Leqisoft.Model.Account { Name = "现金" };
        Assert.Equal("现金", account.GetFullName());
    }

    [Fact]
    public void GetFullName_WithParent_JoinsWithDash()
    {
        var parent = new Leqisoft.Model.Account { Name = "流动资产" };
        var child = new Leqisoft.Model.Account { Name = "现金", Parent = parent };
        Assert.Equal("流动资产-现金", child.GetFullName());
    }

    [Fact]
    public void GetFullName_MultipleLevels()
    {
        var root = new Leqisoft.Model.Account { Name = "资产" };
        var mid = new Leqisoft.Model.Account { Name = "流动资产", Parent = root };
        var leaf = new Leqisoft.Model.Account { Name = "现金", Parent = mid };
        Assert.Equal("资产-流动资产-现金", leaf.GetFullName());
    }

    [Fact]
    public void GetLevel_RootLevel_ReturnsZero()
    {
        var account = new Leqisoft.Model.Account();
        Assert.Equal(0, account.GetLevel());
    }

    [Fact]
    public void GetLevel_Child_ReturnsOne()
    {
        var parent = new Leqisoft.Model.Account();
        var child = new Leqisoft.Model.Account { Parent = parent };
        Assert.Equal(1, child.GetLevel());
    }

    [Fact]
    public void GetLevel_Nested_ReturnsDepth()
    {
        var l1 = new Leqisoft.Model.Account();
        var l2 = new Leqisoft.Model.Account { Parent = l1 };
        var l3 = new Leqisoft.Model.Account { Parent = l2 };
        Assert.Equal(2, l3.GetLevel());
    }

    [Fact]
    public void Ancestors_Root_ReturnsEmpty()
    {
        var root = new Leqisoft.Model.Account();
        Assert.Empty(root.Ancestors);
    }

    [Fact]
    public void Ancestors_Child_ReturnsParentAndUp()
    {
        var grandparent = new Leqisoft.Model.Account { Name = "资产" };
        var parent = new Leqisoft.Model.Account { Name = "流动资产", Parent = grandparent };
        var child = new Leqisoft.Model.Account { Name = "现金", Parent = parent };

        var ancestors = child.Ancestors.ToList();
        Assert.Equal(2, ancestors.Count);
        Assert.Equal("流动资产", ancestors[0].Name);
        Assert.Equal("资产", ancestors[1].Name);
    }

    [Fact]
    public void AncestorsAndSelf_IncludesSelf()
    {
        var parent = new Leqisoft.Model.Account { Name = "流动资产" };
        var child = new Leqisoft.Model.Account { Name = "现金", Parent = parent };

        var result = child.AncestorsAndSelf.ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal("现金", result[0].Name);
        Assert.Equal("流动资产", result[1].Name);
    }

    [Fact]
    public void Descendants_NoChildren_ReturnsEmpty()
    {
        var account = new Leqisoft.Model.Account();
        Assert.Empty(account.Descendants);
    }

    [Fact]
    public void Descendants_WithChildren_ReturnsAllDescendants()
    {
        var parent = new Leqisoft.Model.Account();
        var child1 = new Leqisoft.Model.Account();
        var child2 = new Leqisoft.Model.Account();
        var grandchild = new Leqisoft.Model.Account();
        child1.Children.Add(grandchild);
        parent.Children.Add(child1);
        parent.Children.Add(child2);

        var descendants = parent.Descendants;
        Assert.Equal(3, descendants.Count);
    }

    [Fact]
    public void DescendantsAndSelf_IncludesSelf()
    {
        var parent = new Leqisoft.Model.Account();
        parent.Children.Add(new Leqisoft.Model.Account());

        var result = parent.DescendantsAndSelf;

        // DescendantsAndSelf = [self] + Children + Children.SelectMany(c => c.DescendantsAndSelf)
        // 直接子节点会被 Children 和 DescendantsAndSelf 各算一次
        Assert.Equal(3, result.Count);
        Assert.Equal(parent, result[0]);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var account = new Leqisoft.Model.Account
        {
            Code = "1001",
            Name = "现金",
            IsDebit = true
        };
        var str = account.ToString();
        Assert.Contains("1001", str);
        Assert.Contains("现金", str);
    }
}
