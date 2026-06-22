﻿﻿﻿namespace LeqiAudit.Tests;

public class ServerExceptionTests
{
    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var ex = new Leqisoft.DTO.ServerException
        {
            ExceptionType = "NullReferenceException",
            ExceptionMessage = "Object reference not set",
            ExceptionStackTrace = "  at Foo.Bar()"
        };

        var result = ex.ToString();

        Assert.Contains("NullReferenceException", result);
        Assert.Contains("Object reference not set", result);
        Assert.Contains("Foo.Bar()", result);
    }

    [Fact]
    public void Message_ReturnsToString()
    {
        var ex = new Leqisoft.DTO.ServerException
        {
            ExceptionType = "Type",
            ExceptionMessage = "Msg",
            ExceptionStackTrace = "Stack"
        };

        Assert.Equal(ex.ToString(), ex.Message);
    }
}
