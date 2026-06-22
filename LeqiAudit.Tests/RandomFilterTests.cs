﻿﻿﻿using Leqisoft.Model;

namespace LeqiAudit.Tests;

public class RandomFilterTests
{
    [Fact]
    public void Apply_ZeroCount_ReturnsEmpty()
    {
        var filter = new RandomFilter(0, 0);
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
        };

        var result = filter.Apply(values);

        Assert.Empty(result);
    }

    [Fact]
    public void Apply_CountGreaterThanValues_ReturnsAllKeys()
    {
        var filter = new RandomFilter(0, 10);
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
        };

        var result = filter.Apply(values);

        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
    }

    [Fact]
    public void Apply_SampleCount_ReturnsCorrectNumberOfItems()
    {
        var filter = new RandomFilter(0, 3);
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
            { 3, FilterValue.FromObject("c") },
            { 4, FilterValue.FromObject("d") },
            { 5, FilterValue.FromObject("e") },
        };

        var result = filter.Apply(values);

        Assert.Equal(3, result.Count);
        // All results should be valid keys
        foreach (var key in result)
        {
            Assert.Contains(key, values.Keys);
        }
    }

    [Fact]
    public void Apply_EmptyDictionary_ReturnsEmpty()
    {
        var filter = new RandomFilter(0, 5);
        var result = filter.Apply(new Dictionary<int, FilterValue>());
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_AllValuesCount_ReturnsAllKeys()
    {
        var filter = new RandomFilter(0, 3);
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
            { 3, FilterValue.FromObject("c") },
        };

        var result = filter.Apply(values);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Apply_MultipleCalls_AllReturnValidKeys()
    {
        // With 5 values, pick 3 - verify all results are valid
        var filter = new RandomFilter(0, 3);
        var values = new Dictionary<int, FilterValue>
        {
            { 1, FilterValue.FromObject("a") },
            { 2, FilterValue.FromObject("b") },
            { 3, FilterValue.FromObject("c") },
            { 4, FilterValue.FromObject("d") },
            { 5, FilterValue.FromObject("e") },
        };

        for (int i = 0; i < 20; i++)
        {
            var result = filter.Apply(values);
            Assert.Equal(3, result.Count);
            foreach (var key in result)
            {
                Assert.Contains(key, values.Keys);
            }
        }
    }
}