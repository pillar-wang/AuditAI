using Leqisoft.Model;

namespace LeqiAudit.Tests;

public class GreaterThanOrEqualsFilterTests
{
    [Fact]
    public void Apply_NumbersGreaterOrEqual_ReturnsKeys()
    {
        var filter = new GreaterThanOrEqualsFilter<int>(0, 10);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(5) },
            { 2, FilterValue.FromObject(10) },
            { 3, FilterValue.FromObject(15) },
        };

        var result = filter.Apply(data);

        Assert.Equal(2, result.Count);
        Assert.Contains(2, result);
        Assert.Contains(3, result);
    }

    [Fact]
    public void Apply_NoMatch_ReturnsEmpty()
    {
        var filter = new GreaterThanOrEqualsFilter<int>(0, 100);
        var result = filter.Apply(new Dictionary<int, FilterValue> { { 1, FilterValue.FromObject(50) } });
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new GreaterThanOrEqualsFilter<int>(0, 5);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class NotEqualsFilterTests
{
    [Fact]
    public void Apply_NotEqual_ReturnsMatchingKeys()
    {
        var filter = new NotEqualsFilter<int>(0, 5);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(3) },
            { 2, FilterValue.FromObject(5) },
            { 3, FilterValue.FromObject(7) },
        };

        var result = filter.Apply(data);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(3, result);
    }

    [Fact]
    public void Apply_AllEqual_ReturnsEmpty()
    {
        var filter = new NotEqualsFilter<int>(0, 5);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(5) },
            { 2, FilterValue.FromObject(5) },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_Strings_ComparesByValue()
    {
        var filter = new NotEqualsFilter<string>(0, "abc");
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("xyz") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new NotEqualsFilter<int>(0, 5);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class StringEqualsFilterTests
{
    [Fact]
    public void Apply_ExactMatch_ReturnsKeys()
    {
        var filter = new StringEqualsFilter(0, FilterValue.FromObject("abc"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("ABC") },
            { 3, FilterValue.FromObject("xyz") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(1, result);
    }

    [Fact]
    public void Apply_NullCandidate_ExcludedWhenFilterHasValue()
    {
        var filter = new StringEqualsFilter(0, FilterValue.FromObject("abc"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject((string)null!) },
            { 2, FilterValue.FromObject("abc") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_NoMatch_ReturnsEmpty()
    {
        var filter = new StringEqualsFilter(0, FilterValue.FromObject("abc"));
        var result = filter.Apply(new Dictionary<int, FilterValue> { { 1, FilterValue.FromObject("xyz") } });
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_MatchesByDisplayValue()
    {
        var filter = new StringEqualsFilter(0, new FilterValue { DisplayValue = "100", Value = 100 });
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("100") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new StringEqualsFilter(0, FilterValue.FromObject("a"));
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class StringNotEqualsFilterTests
{
    [Fact]
    public void Apply_NotEqual_ReturnsKeys()
    {
        var filter = new StringNotEqualsFilter(0, FilterValue.FromObject("abc"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("xyz") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_NullCandidate_ValueNonNull_ReturnsKey()
    {
        var filter = new StringNotEqualsFilter(0, FilterValue.FromObject("abc"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject((string)null!) },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
    }

    [Fact]
    public void Apply_AllEqual_ReturnsEmpty()
    {
        var filter = new StringNotEqualsFilter(0, FilterValue.FromObject("abc"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("abc") },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }
}

public class StringNotContainsFilterTests
{
    [Fact]
    public void Apply_NotContains_ReturnsKeys()
    {
        var filter = new StringNotContainsFilter(0, FilterValue.FromObject("bc"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("def") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_AllMatch_ReturnsEmpty()
    {
        var filter = new StringNotContainsFilter(0, FilterValue.FromObject("a"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("ab") },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_NullCandidate_IncludedByNotContains()
    {
        var filter = new StringNotContainsFilter(0, FilterValue.FromObject("a"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject((string)null!) },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(1, result);
    }
}

public class StringNotBeginwithTests
{
    [Fact]
    public void Apply_NotStartingWith_ReturnsKeys()
    {
        var filter = new StringNotBeginwith(0, FilterValue.FromObject("ab"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("xyz") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_AllStartWith_ReturnsEmpty()
    {
        var filter = new StringNotBeginwith(0, FilterValue.FromObject("ab"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("abd") },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_NullCandidate_IncludedByNotBeginwith()
    {
        var filter = new StringNotBeginwith(0, FilterValue.FromObject("a"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject((string)null!) },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(1, result);
    }
}

public class StringNotEndwithTests
{
    [Fact]
    public void Apply_NotEndingWith_ReturnsKeys()
    {
        var filter = new StringNotEndwith(0, FilterValue.FromObject("bc"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("xyz") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_AllEndWith_ReturnsEmpty()
    {
        var filter = new StringNotEndwith(0, FilterValue.FromObject("c"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("bc") },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_NullCandidate_IncludedByNotEndwith()
    {
        var filter = new StringNotEndwith(0, FilterValue.FromObject("c"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject((string)null!) },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(1, result);
    }
}

public class EmptyOrZeroFilterTests
{
    [Fact]
    public void Apply_NullString_ReturnsKey()
    {
        var filter = new EmptyOrZeroFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject((string)null!) },
            { 2, FilterValue.FromObject("abc") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(1, result);
    }

    [Fact]
    public void Apply_EmptyString_ReturnsKey()
    {
        var filter = new EmptyOrZeroFilter(0);
        var result = filter.Apply(new Dictionary<int, FilterValue> { { 1, FilterValue.FromObject("") } });
        Assert.Single(result);
    }

    [Fact]
    public void Apply_ZeroDecimal_ReturnsKey()
    {
        var filter = new EmptyOrZeroFilter(0);
        var result = filter.Apply(new Dictionary<int, FilterValue> { { 1, FilterValue.FromObject(0m) } });
        Assert.Single(result);
    }

    [Fact]
    public void Apply_ZeroInt_ReturnsKey()
    {
        var filter = new EmptyOrZeroFilter(0);
        var result = filter.Apply(new Dictionary<int, FilterValue> { { 1, FilterValue.FromObject(0) } });
        Assert.Single(result);
    }

    [Fact]
    public void Apply_NonEmptyNonZero_ReturnsEmpty()
    {
        var filter = new EmptyOrZeroFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("hello") },
            { 2, FilterValue.FromObject(42) },
            { 3, FilterValue.FromObject(true) },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }
}

public class NotEmptyOrZeroFilterTests
{
    [Fact]
    public void Apply_NonEmptyOrNull_ReturnsKeys()
    {
        var filter = new NotEmptyOrZeroFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject((string)null!) },
        };

        var result = filter.Apply(data);
        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_NonZeroInt_ReturnsKey()
    {
        var filter = new NotEmptyOrZeroFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(0) },
            { 2, FilterValue.FromObject(42) },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_AllEmptyOrZero_ReturnsNullOnly()
    {
        var filter = new NotEmptyOrZeroFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("") },
            { 2, FilterValue.FromObject(0m) },
            { 3, FilterValue.FromObject((string)null!) },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(3, result);
    }
}

public class MinFilterTests
{
    [Fact]
    public void Apply_ReturnsMinValueKey()
    {
        var filter = new MinFilter<int>(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(10) },
            { 2, FilterValue.FromObject(3) },
            { 3, FilterValue.FromObject(7) },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_TiedMin_ReturnsAllMinKeys()
    {
        var filter = new MinFilter<int>(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(5) },
            { 2, FilterValue.FromObject(5) },
            { 3, FilterValue.FromObject(10) },
        };

        var result = filter.Apply(data);
        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_Strings_FindsMin()
    {
        var filter = new MinFilter<string>(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("banana") },
            { 2, FilterValue.FromObject("apple") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new MinFilter<int>(0);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class MaxFilterTests
{
    [Fact]
    public void Apply_ReturnsMaxValueKey()
    {
        var filter = new MaxFilter<int>(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(3) },
            { 2, FilterValue.FromObject(10) },
            { 3, FilterValue.FromObject(7) },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_TiedMax_ReturnsAllMaxKeys()
    {
        var filter = new MaxFilter<int>(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(10) },
            { 2, FilterValue.FromObject(10) },
            { 3, FilterValue.FromObject(5) },
        };

        var result = filter.Apply(data);
        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_Strings_FindsMax()
    {
        var filter = new MaxFilter<string>(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("apple") },
            { 2, FilterValue.FromObject("banana") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new MaxFilter<int>(0);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class OutsideRangeFilterTests
{
    [Fact]
    public void Apply_OutsideRange_ReturnsKeys()
    {
        var filter = new OutsideRangeFilter<int>(0, 5, 15);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(3) },
            { 2, FilterValue.FromObject(10) },
            { 3, FilterValue.FromObject(20) },
        };

        var result = filter.Apply(data);
        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(3, result);
    }

    [Fact]
    public void Apply_WithinRange_ReturnsEmpty()
    {
        var filter = new OutsideRangeFilter<int>(0, 5, 15);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(5) },
            { 2, FilterValue.FromObject(10) },
            { 3, FilterValue.FromObject(15) },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_IncomparableTypes_ReturnsEmpty()
    {
        var filter = new OutsideRangeFilter<int>(0, 1, 10);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new OutsideRangeFilter<int>(0, 1, 10);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class EquidistanceFilterTests
{
    [Fact]
    public void Apply_ReturnsEvenlySpacedKeys()
    {
        var filter = new EquidistanceFilter(0, 3);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
            { 3, FilterValue.FromObject("c") },
            { 4, FilterValue.FromObject("d") },
            { 5, FilterValue.FromObject("e") },
            { 6, FilterValue.FromObject("f") },
            { 7, FilterValue.FromObject("g") },
        };

        var result = filter.Apply(data);
        Assert.Equal(3, result.Count);
        // Should pick 3 evenly spaced: index 0, 2, 4
        Assert.Contains(1, result);
        Assert.Contains(3, result);
        Assert.Contains(5, result);
    }

    [Fact]
    public void Apply_SampleMoreThanData_CapsAtDataCount()
    {
        var filter = new EquidistanceFilter(0, 10);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
            { 3, FilterValue.FromObject("c") },
        };

        var result = filter.Apply(data);
        // num = Math.Max(1, 3/10) = 1, picks items at indices 0,1,2
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Apply_SingleItem_ReturnsSingle()
    {
        var filter = new EquidistanceFilter(0, 1);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(1, result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new EquidistanceFilter(0, 3);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class ExcludeRedundancyFilterTests
{
    [Fact]
    public void Apply_Duplicates_ReturnsFirstOfEach()
    {
        var filter = new ExcludeRedundancyFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
            { 3, FilterValue.FromObject("a") },
            { 4, FilterValue.FromObject("b") },
            { 5, FilterValue.FromObject("c") },
        };

        var result = filter.Apply(data);
        Assert.Equal(3, result.Count);
        Assert.Contains(1, result); // first "a"
        Assert.Contains(2, result); // first "b"
        Assert.Contains(5, result); // first "c"
    }

    [Fact]
    public void Apply_AllUnique_ReturnsAll()
    {
        var filter = new ExcludeRedundancyFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
        };

        var result = filter.Apply(data);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new ExcludeRedundancyFilter(0);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class RedundantValueFilterTests
{
    [Fact]
    public void Apply_Duplicates_ReturnsExtraOnly()
    {
        var filter = new RedundantValueFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("a") },
            { 3, FilterValue.FromObject("b") },
            { 4, FilterValue.FromObject("b") },
            { 5, FilterValue.FromObject("c") },
        };

        var result = filter.Apply(data);
        Assert.Equal(2, result.Count);
        Assert.Contains(2, result); // second "a"
        Assert.Contains(4, result); // second "b"
    }

    [Fact]
    public void Apply_AllUnique_ReturnsEmpty()
    {
        var filter = new RedundantValueFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_TripleDuplicates_ReturnsLastTwo()
    {
        var filter = new RedundantValueFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("a") },
            { 3, FilterValue.FromObject("a") },
        };

        var result = filter.Apply(data);
        Assert.Equal(2, result.Count);
        Assert.Contains(2, result);
        Assert.Contains(3, result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new RedundantValueFilter(0);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class PPSFilterTests
{
    [Fact]
    public void Apply_SampleCount_ReturnsCorrectNumberOfItems()
    {
        var filter = new PPSFilter(0, 3);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(10) },
            { 2, FilterValue.FromObject(20) },
            { 3, FilterValue.FromObject(30) },
            { 4, FilterValue.FromObject(40) },
            { 5, FilterValue.FromObject(50) },
        };

        var result = filter.Apply(data);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Apply_ZeroCount_ReturnsEmpty()
    {
        var filter = new PPSFilter(0, 0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(10) },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_CountGreaterThanValues_ReturnsAll()
    {
        var filter = new PPSFilter(0, 10);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(10) },
            { 2, FilterValue.FromObject(20) },
        };

        var result = filter.Apply(data);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Apply_AllStrings_UsesDefaultOneWeight()
    {
        var filter = new PPSFilter(0, 2);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
            { 3, FilterValue.FromObject("c") },
        };

        var result = filter.Apply(data);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new PPSFilter(0, 3);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class FilterRelationEnumTests
{
    [Fact]
    public void And_IsZero() { Assert.Equal(0, (int)FilterRelation.And); }
    [Fact]
    public void Or_IsOne() { Assert.Equal(1, (int)FilterRelation.Or); }
}

public class DifferentDataTypeExceptionTests
{
    [Fact]
    public void IsException()
    {
        var ex = new DifferentDataTypeException();
        Assert.IsAssignableFrom<Exception>(ex);
    }
}