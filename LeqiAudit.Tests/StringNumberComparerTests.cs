﻿﻿﻿namespace LeqiAudit.Tests;

public class StringNumberComparerTests
{
    private readonly Leqisoft.Model.StringNumberComparer _comparer = Leqisoft.Model.StringNumberComparer.Instance;

    [Fact]
    public void Compare_BothNumeric_ComparesByValue()
    {
        Assert.True(_comparer.Compare("5", "10") < 0);
        Assert.True(_comparer.Compare("10", "5") > 0);
        Assert.Equal(0, _comparer.Compare("7", "7"));
    }

    [Fact]
    public void Compare_FirstNumericSecondString_NumericFirst()
    {
        Assert.True(_comparer.Compare("5", "abc") < 0);
    }

    [Fact]
    public void Compare_FirstStringSecondNumeric_StringLast()
    {
        Assert.True(_comparer.Compare("abc", "5") > 0);
    }

    [Fact]
    public void Compare_BothString_UsesLexicographic()
    {
        Assert.True(_comparer.Compare("abc", "def") < 0);
        Assert.True(_comparer.Compare("xyz", "abc") > 0);
        Assert.Equal(0, _comparer.Compare("hello", "hello"));
    }

    [Fact]
    public void Compare_DecimalStrings_ComparesAsNumbers()
    {
        Assert.Equal(0, _comparer.Compare("3.14", "3.14"));
        Assert.True(_comparer.Compare("1.5", "2.5") < 0);
    }

    [Fact]
    public void Instance_IsSingleton()
    {
        Assert.Same(Leqisoft.Model.StringNumberComparer.Instance, Leqisoft.Model.StringNumberComparer.Instance);
    }
}
