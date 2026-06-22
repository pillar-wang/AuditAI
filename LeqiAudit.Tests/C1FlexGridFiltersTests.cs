﻿using Leqisoft.Model;
using Newtonsoft.Json;

namespace LeqiAudit.Tests;

public class C1FlexGridFiltersSerializationTests
{
    [Fact]
    public void Serialize_Deserialize_RoundTrip()
    {
        var filters = new C1FlexGridFilters();
        filters.First(new EqualsFilter<string>(0, "test"));
        filters.And(new GreaterThanFilter<int>(1, 10));
        filters.Or(new StringContainsFilter(2, FilterValue.FromObject("abc")));

        var json = filters.Serialize();

        Assert.NotNull(json);
        Assert.Contains("EqualsFilter", json);
        Assert.Contains("GreaterThanFilter", json);
        Assert.Contains("StringContainsFilter", json);

        var restored = new C1FlexGridFilters();
        restored.Deserialize(json);

        Assert.NotNull(restored.filters);
        Assert.Equal(3, restored.filters.Count);
        Assert.IsType<EqualsFilter<string>>(restored.filters[0]);
        Assert.IsType<GreaterThanFilter<int>>(restored.filters[1]);
        Assert.IsType<StringContainsFilter>(restored.filters[2]);
    }

    [Fact]
    public void Serialize_RandomFilter_RoundTrips()
    {
        var filters = new C1FlexGridFilters();
        filters.First(new RandomFilter(0, 5));

        var json = filters.Serialize();
        var restored = new C1FlexGridFilters();
        restored.Deserialize(json);

        Assert.Single(restored.filters);
        Assert.IsType<RandomFilter>(restored.filters[0]);
    }

    [Fact]
    public void Deserialize_EmptyArray_ReturnsEmptyList()
    {
        var filters = new C1FlexGridFilters();
        filters.Deserialize("[]");
        Assert.NotNull(filters.filters);
        Assert.Empty(filters.filters);
    }

    [Fact]
    public void Clear_RemovesFilters()
    {
        var filters = new C1FlexGridFilters();
        filters.First(new EqualsFilter<string>(0, "x"));
        Assert.Single(filters.filters);

        filters.Clear();
        Assert.Empty(filters.filters);
    }

    [Fact]
    public void First_ClearsAndAdds()
    {
        var filters = new C1FlexGridFilters();
        filters.First(new EqualsFilter<string>(0, "a"));
        filters.First(new EqualsFilter<string>(0, "b"));

        Assert.Single(filters.filters);
        var eq = Assert.IsType<EqualsFilter<string>>(filters.filters[0]);
        Assert.Equal("b", eq.Value);
    }

    [Fact]
    public void IsExtract_RandomFilter_ReturnsTrue()
    {
        var filters = new C1FlexGridFilters();
        Assert.True(filters.IsExtract(new RandomFilter(0, 5)));
    }

    [Fact]
    public void IsExtract_EquidistanceFilter_ReturnsTrue()
    {
        var filters = new C1FlexGridFilters();
        Assert.True(filters.IsExtract(new EquidistanceFilter(0, 3)));
    }

    [Fact]
    public void IsExtract_PPSFilter_ReturnsTrue()
    {
        var filters = new C1FlexGridFilters();
        Assert.True(filters.IsExtract(new PPSFilter(0, 3)));
    }

    [Fact]
    public void IsExtract_NormalFilter_ReturnsFalse()
    {
        var filters = new C1FlexGridFilters();
        Assert.False(filters.IsExtract(new EqualsFilter<string>(0, "x")));
    }

    [Fact]
    public void Apply_NoFilters_ReturnsAllKeys()
    {
        var filters = new C1FlexGridFilters();
        var source = new List<Dictionary<int, FilterValue>>
        {
            new Dictionary<int, FilterValue>
            {
                { 1, FilterValue.FromObject("a") },
                { 2, FilterValue.FromObject("b") },
            }
        };

        var result = filters.Apply(source);
        Assert.Equal(2, result.Count);
        Assert.Equal(2, filters.ResultCount);
    }

    [Fact]
    public void Apply_AndFilter_Intersects()
    {
        var filters = new C1FlexGridFilters();
        filters.First(new EqualsFilter<string>(0, "a"));

        var source = new List<Dictionary<int, FilterValue>>
        {
            new Dictionary<int, FilterValue>
            {
                { 1, FilterValue.FromObject("a") },
                { 2, FilterValue.FromObject("b") },
                { 3, FilterValue.FromObject("a") },
            }
        };

        var result = filters.Apply(source);
        Assert.Equal(2, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(3, result);
    }

    [Fact]
    public void OrFilter_InFirstFilter_DefaultsToAnd()
    {
        // First filter with Or relation = starts with empty result set (Union behavior)
        var filters = new C1FlexGridFilters();
        var equals = new EqualsFilter<string>(0, "a") { relation = FilterRelation.Or };
        filters.First(equals);

        var source = new List<Dictionary<int, FilterValue>>
        {
            new Dictionary<int, FilterValue>
            {
                { 1, FilterValue.FromObject("a") },
                { 2, FilterValue.FromObject("b") },
            }
        };

        var result = filters.Apply(source);
        Assert.Single(result);
        Assert.Contains(1, result);
    }
}