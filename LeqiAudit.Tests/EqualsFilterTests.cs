﻿﻿﻿using Leqisoft.Model;

namespace LeqiAudit.Tests;

public class EqualsFilterTests
{
    [Fact]
    public void Apply_MatchingStringValue_ReturnsMatchingKeys()
    {
        var filter = new EqualsFilter<string>(0, "hello");
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("hello") },
            { 2, FilterValue.FromObject("world") },
            { 3, FilterValue.FromObject("hello") },
        };

        var result = filter.Apply(values);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(3, result);
    }

    [Fact]
    public void Apply_MatchingNumericValue_ReturnsMatchingKeys()
    {
        var filter = new EqualsFilter<int>(0, 100);
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(100) },
            { 2, FilterValue.FromObject(200) },
            { 3, FilterValue.FromObject(100) },
        };

        var result = filter.Apply(values);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(3, result);
    }

    [Fact]
    public void Apply_NoMatch_ReturnsEmpty()
    {
        var filter = new EqualsFilter<int>(0, 999);
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(100) },
            { 2, FilterValue.FromObject(200) },
        };

        var result = filter.Apply(values);

        Assert.Empty(result);
    }

    [Fact]
    public void Apply_NullValue_MatchesNullCandidate()
    {
        var filter = new EqualsFilter<string>(0, null!);
        var values = new Dictionary<int, FilterValue>
        {
            { 1, new FilterValue { Value = null, DisplayValue = "" } },
            { 2, FilterValue.FromObject("not null") },
        };

        var result = filter.Apply(values);

        Assert.Single(result);
        Assert.Contains(1, result);
    }

    [Fact]
    public void Apply_BoolValues_MatchesCorrectly()
    {
        var filter = new EqualsFilter<bool>(0, true);
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(true) },
            { 2, FilterValue.FromObject(false) },
        };

        var result = filter.Apply(values);

        Assert.Single(result);
        Assert.Contains(1, result);
    }

    [Fact]
    public void Apply_DateTimeValues_ComparesByDateOnly()
    {
        var date = new DateTime(2024, 6, 15, 10, 30, 0);
        var filter = new EqualsFilter<DateTime>(0, date);
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(new DateTime(2024, 6, 15, 8, 0, 0)) }, // same date
            { 2, FilterValue.FromObject(new DateTime(2024, 6, 16)) },           // different date
        };

        var result = filter.Apply(values);

        Assert.Single(result);
        Assert.Contains(1, result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new EqualsFilter<int>(0, 1);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }
}