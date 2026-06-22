﻿using Leqisoft.DTO;

namespace LeqiAudit.Tests;

public class SignAlignTests
{
    [Fact]
    public void Left_HasValueZero()
    {
        Assert.Equal(0, (int)SignAlign.Left);
    }

    [Fact]
    public void Center_HasValueOne()
    {
        Assert.Equal(1, (int)SignAlign.Center);
    }

    [Fact]
    public void Right_HasValueTwo()
    {
        Assert.Equal(2, (int)SignAlign.Right);
    }
}

public class NavigateViewTypeTests
{
    [Fact]
    public void Tree_IsZero()
    {
        Assert.Equal(0, (int)NavigateViewType.Tree);
    }

    [Fact]
    public void Chart_IsOne()
    {
        Assert.Equal(1, (int)NavigateViewType.Chart);
    }
}