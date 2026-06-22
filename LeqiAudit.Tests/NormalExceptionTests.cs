﻿﻿﻿namespace LeqiAudit.Tests;

public class NormalExceptionTests
{
    [Fact]
    public void Constructor_SetsMessage()
    {
        var ex = new Leqisoft.DTO.NormalException("test error");
        Assert.Equal("test error", ex.Message);
    }

    [Fact]
    public void IsException()
    {
        var ex = new Leqisoft.DTO.NormalException("msg");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
