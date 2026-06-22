﻿﻿﻿using Leqisoft.DTO;

namespace LeqiAudit.Tests;

public class TeamLevelExtensionsTests
{
    [Fact]
    public void ToFriendlyString_Standard_ReturnsChinese()
    {
        Assert.Equal("标准版", TeamLevel.Standard.ToFriendlyString());
    }

    [Fact]
    public void ToFriendlyString_Professional_ReturnsChinese()
    {
        Assert.Equal("专业版", TeamLevel.Professional.ToFriendlyString());
    }

    [Fact]
    public void ToFriendlyString_Ultimate_ReturnsChinese()
    {
        Assert.Equal("旗舰版", TeamLevel.Ultimate.ToFriendlyString());
    }

    [Fact]
    public void ToFriendlyString_None_ReturnsEmpty()
    {
        Assert.Equal("", TeamLevel.None.ToFriendlyString());
    }
}
