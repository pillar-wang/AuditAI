﻿﻿﻿using Leqisoft.Model;

namespace LeqiAudit.Tests;

public class LessThanFilterTests
{
    [Fact]
    public void Apply_ValuesLessThanThreshold_ReturnsMatchingKeys()
    {
        var filter = new LessThanFilter<int>(0, 50);
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject(10) },
            { 2, FilterValue.FromObject(50) },
            { 3, FilterValue.FromObject(100) },
            { 4, FilterValue.FromObject(30) },
        };

        var result = filter.Apply(values);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(4, result);
    }

    [Fact]
    public void Apply_StringComparison_ReturnsMatchingKeys()
    {
        var filter = new LessThanFilter<string>(0, "mango");
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("apple") },
            { 2, FilterValue.FromObject("banana") },
            { 3, FilterValue.FromObject("orange") },
        };

        var result = filter.Apply(values);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_NumericStrings_ComparesLexicographically()
    {
        var filter = new LessThanFilter<string>(0, "100");
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("010") },  // "010" < "100" lexicographically
            { 2, FilterValue.FromObject("999") },  // "999" > "100" lexicographically
            { 3, FilterValue.FromObject("011") },  // "011" < "100" lexicographically
        };

        // Lexicographic comparison: "010" < "100", "011" < "100", "999" > "100"
        var result = filter.Apply(values);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(3, result);
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new LessThanFilter<int>(0, 10);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_IncomparableTypes_ReturnsEmpty()
    {
        var filter = new LessThanFilter<int>(0, 10);
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("string instead of int") },
        };

        var result = filter.Apply(values);

        Assert.Empty(result);
    }
}