﻿using Leqisoft.Model;

namespace LeqiAudit.Tests;

public class StringContainsFilterTests
{
    [Fact]
    public void Apply_MatchingSubstring_ReturnsMatchingKeys()
    {
        var filter = new StringContainsFilter(0, FilterValue.FromObject("bc"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("bcd") },
            { 3, FilterValue.FromObject("def") },
        };

        var result = filter.Apply(data);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_NoMatch_ReturnsEmpty()
    {
        var filter = new StringContainsFilter(0, FilterValue.FromObject("xyz"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("def") },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_CaseSensitive_RespectsCase()
    {
        var filter = new StringContainsFilter(0, FilterValue.FromObject("ABC"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("ABC") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_NullCandidate_DoesNotMatch()
    {
        var filter = new StringContainsFilter(0, FilterValue.FromObject("abc"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject((string)null!) },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new StringContainsFilter(0, FilterValue.FromObject("abc"));
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_SearchEmptyString_AllContainEmpty()
    {
        var filter = new StringContainsFilter(0, FilterValue.FromObject(""));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("") },
        };

        var result = filter.Apply(data);
        Assert.Equal(2, result.Count);
    }
}

public class StringStartsWithFilterTests
{
    [Fact]
    public void Apply_MatchingPrefix_ReturnsMatchingKeys()
    {
        var filter = new StringStartsWithFilter(0, FilterValue.FromObject("ab"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("abd") },
            { 3, FilterValue.FromObject("bcd") },
        };

        var result = filter.Apply(data);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_NoMatch_ReturnsEmpty()
    {
        var filter = new StringStartsWithFilter(0, FilterValue.FromObject("xy"));
        var result = filter.Apply(new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
        });
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_ExactMatch_ReturnsTrue()
    {
        var filter = new StringStartsWithFilter(0, FilterValue.FromObject("abc"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
    }

    [Fact]
    public void Apply_NullCandidate_ReturnsEmpty()
    {
        var filter = new StringStartsWithFilter(0, FilterValue.FromObject("a"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject((string)null!) },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new StringStartsWithFilter(0, FilterValue.FromObject("a"));
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class StringEndsWithFilterTests
{
    [Fact]
    public void Apply_MatchingSuffix_ReturnsMatchingKeys()
    {
        var filter = new StringEndsWithFilter(0, FilterValue.FromObject("bc"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject("xbc") },
            { 3, FilterValue.FromObject("bcd") },
        };

        var result = filter.Apply(data);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_NoMatch_ReturnsEmpty()
    {
        var filter = new StringEndsWithFilter(0, FilterValue.FromObject("xy"));
        var result = filter.Apply(new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
        });
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_NullCandidate_ReturnsEmpty()
    {
        var filter = new StringEndsWithFilter(0, FilterValue.FromObject("c"));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject((string)null!) },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }
}

public class EmptyFilterTests
{
    [Fact]
    public void Apply_NullValue_ReturnsKey()
    {
        var filter = new EmptyFilter(0);
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
        var filter = new EmptyFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
    }

    [Fact]
    public void Apply_FalseBool_ReturnsKey()
    {
        var filter = new EmptyFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(false) },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
    }

    [Fact]
    public void Apply_NotEmptyValues_Excludes()
    {
        var filter = new EmptyFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("hello") },
            { 2, FilterValue.FromObject(42) },
            { 3, FilterValue.FromObject(true) },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new EmptyFilter(0);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class NotEmptyFilterTests
{
    [Fact]
    public void Apply_NonEmptyString_ReturnsKey()
    {
        var filter = new NotEmptyFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("abc") },
            { 2, FilterValue.FromObject((string)null!) },
        };

        var result = filter.Apply(data);

        Assert.Single(result);
        Assert.Contains(1, result);
    }

    [Fact]
    public void Apply_AllEmpty_ReturnsEmpty()
    {
        var filter = new NotEmptyFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject((string)null!) },
            { 2, FilterValue.FromObject("") },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_MixedFilters_OnlyReturnsNonEmpty()
    {
        var filter = new NotEmptyFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("data") },
            { 2, FilterValue.FromObject("") },
            { 3, FilterValue.FromObject((string)null!) },
            { 4, FilterValue.FromObject(0) },
        };

        var result = filter.Apply(data);
        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(4, result);
    }
}

public class DuplicateValueFilterTests
{
    [Fact]
    public void Apply_DuplicateValues_ReturnsDuplicateKeys()
    {
        var filter = new DuplicateValueFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
            { 3, FilterValue.FromObject("a") },
            { 4, FilterValue.FromObject("c") },
        };

        var result = filter.Apply(data);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(3, result);
    }

    [Fact]
    public void Apply_AllUnique_ReturnsEmpty()
    {
        var filter = new DuplicateValueFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
            { 3, FilterValue.FromObject("c") },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_AllSame_ReturnsAllKeys()
    {
        var filter = new DuplicateValueFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("x") },
            { 2, FilterValue.FromObject("x") },
            { 3, FilterValue.FromObject("x") },
        };

        var result = filter.Apply(data);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new DuplicateValueFilter(0);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class UniqueValueFilterTests
{
    [Fact]
    public void Apply_UniqueValues_ReturnsKeys()
    {
        var filter = new UniqueValueFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
            { 3, FilterValue.FromObject("a") },
        };

        var result = filter.Apply(data);

        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_AllUnique_ReturnsAllKeys()
    {
        var filter = new UniqueValueFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
        };

        var result = filter.Apply(data);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Apply_AllDuplicate_ReturnsEmpty()
    {
        var filter = new UniqueValueFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("x") },
            { 2, FilterValue.FromObject("x") },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_NullValues_GroupedByNull()
    {
        var filter = new UniqueValueFilter(0);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject((string)null!) },
            { 2, FilterValue.FromObject((string)null!) },
            { 3, FilterValue.FromObject("a") },
        };

        var result = filter.Apply(data);
        Assert.Single(result);
        Assert.Contains(3, result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new UniqueValueFilter(0);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class GreaterThanFilterTests
{
    [Fact]
    public void Apply_Numbers_GreaterThanThreshold_ReturnsKeys()
    {
        var filter = new GreaterThanFilter<int>(0, 10);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(5) },
            { 2, FilterValue.FromObject(10) },
            { 3, FilterValue.FromObject(15) },
            { 4, FilterValue.FromObject(20) },
        };

        var result = filter.Apply(data);

        Assert.Equal(2, result.Count);
        Assert.Contains(3, result);
        Assert.Contains(4, result);
    }

    [Fact]
    public void Apply_NoMatch_ReturnsEmpty()
    {
        var filter = new GreaterThanFilter<int>(0, 100);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(5) },
            { 2, FilterValue.FromObject(50) },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_IncomparableType_ReturnsEmpty()
    {
        var filter = new GreaterThanFilter<string>(0, "abc");
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(42) }, // int vs string
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new GreaterThanFilter<int>(0, 5);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class LessThanOrEqualsFilterTests
{
    [Fact]
    public void Apply_Numbers_ReturnsCorrectKeys()
    {
        var filter = new LessThanOrEqualsFilter<int>(0, 10);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(5) },
            { 2, FilterValue.FromObject(10) },
            { 3, FilterValue.FromObject(15) },
        };

        var result = filter.Apply(data);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_Strings_ComparesCorrectly()
    {
        var filter = new LessThanOrEqualsFilter<string>(0, "banana");
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("apple") },
            { 2, FilterValue.FromObject("banana") },
            { 3, FilterValue.FromObject("cherry") },
        };

        var result = filter.Apply(data);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new LessThanOrEqualsFilter<int>(0, 5);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}

public class WithinRangeFilterTests
{
    [Fact]
    public void Apply_WithinRange_ReturnsKeys()
    {
        var filter = new WithinRangeFilter<int>(0, 5, 15);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(3) },
            { 2, FilterValue.FromObject(5) },
            { 3, FilterValue.FromObject(10) },
            { 4, FilterValue.FromObject(15) },
            { 5, FilterValue.FromObject(20) },
        };

        var result = filter.Apply(data);

        Assert.Equal(3, result.Count);
        Assert.Contains(2, result);
        Assert.Contains(3, result);
        Assert.Contains(4, result);
    }

    [Fact]
    public void Apply_DateRange_ReturnsKeys()
    {
        var filter = new WithinRangeFilter<DateTime>(0, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(new DateTime(2023, 6, 15)) },
            { 2, FilterValue.FromObject(new DateTime(2024, 6, 15)) },
            { 3, FilterValue.FromObject(new DateTime(2025, 1, 1)) },
        };

        var result = filter.Apply(data);

        Assert.Single(result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_NoMatch_ReturnsEmpty()
    {
        var filter = new WithinRangeFilter<int>(0, 100, 200);
        var data = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(1) },
            { 2, FilterValue.FromObject(300) },
        };

        var result = filter.Apply(data);
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_IncomparableTypes_ReturnsEmpty()
    {
        var filter = new WithinRangeFilter<int>(0, 1, 10);
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
        var filter = new WithinRangeFilter<int>(0, 1, 10);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}