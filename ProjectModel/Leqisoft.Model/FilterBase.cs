using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Leqisoft.Model;

[JsonObject]
public abstract class FilterBase
{
	public static JsonSerializerSettings SerializerSettings { get; } = new JsonSerializerSettings
	{
		Converters = { (JsonConverter)new FilterJsonConverter() }
	};


	public static JsonSerializer Serializer { get; } = JsonSerializer.Create(SerializerSettings);


	[JsonProperty]
	public string ColumnId { get; set; }

	[JsonProperty]
	public FilterRelation Relation { get; set; }

	[JsonProperty]
	public FilterKind Kind { get; set; }

	public abstract HashSet<int> Execute(List<FilterValue> data);

	public abstract string ToFormula(string colText);

	public static FilterBase CreateFromKind(FilterKind kind)
	{
		return (FilterBase)Activator.CreateInstance(TypeFromKind(kind));
	}

	private static Type TypeFromKind(FilterKind kind)
	{
		switch (kind)
		{
		case FilterKind.Eq:
		case FilterKind.Gt:
		case FilterKind.Lt:
		case FilterKind.Ne:
		case FilterKind.Gte:
		case FilterKind.Lte:
		case FilterKind.Between:
		case FilterKind.Outside:
			return typeof(NumberFilter);
		case FilterKind.Min:
			return typeof(MinFilter);
		case FilterKind.Max:
			return typeof(MaxFilter);
		case FilterKind.TextEq:
		case FilterKind.Contains:
		case FilterKind.StartsWith:
		case FilterKind.EndsWith:
		case FilterKind.TextNe:
		case FilterKind.NotContains:
		case FilterKind.NotStartsWith:
		case FilterKind.NotEndsWith:
			return typeof(TextFilter);
		case FilterKind.DateEq:
		case FilterKind.Before:
		case FilterKind.After:
		case FilterKind.DateNe:
		case FilterKind.BeforeEq:
		case FilterKind.AfterEq:
		case FilterKind.DateBetween:
		case FilterKind.DateOutside:
		case FilterKind.Today:
		case FilterKind.Year:
		case FilterKind.Season:
		case FilterKind.Month:
		case FilterKind.Week:
			return typeof(DateFilter);
		case FilterKind.DateMin:
			return typeof(DateMinFilter);
		case FilterKind.DateMax:
			return typeof(DateMaxFilter);
		case FilterKind.Random:
			return typeof(RandomFilter);
		case FilterKind.Equidistance:
			return typeof(EquidistanceFilter);
		case FilterKind.Pps:
			return typeof(PpsFilter);
		case FilterKind.Duplicate:
			return typeof(DuplicateFilter);
		case FilterKind.ExceptExcess:
			return typeof(ExceptExcessFilter);
		case FilterKind.Unique:
			return typeof(UniqueFilter);
		case FilterKind.Excess:
			return typeof(ExcessFilter);
		case FilterKind.Select:
			return typeof(SelectFilter);
		case FilterKind.True:
			return typeof(TrueFilter);
		case FilterKind.NotTrue:
			return typeof(NotTrueFilter);
		case FilterKind.Blank:
			return typeof(BlankFilter);
		case FilterKind.NotBlank:
			return typeof(NotBlankFilter);
		case FilterKind.TimeEq:
		case FilterKind.TimeBefore:
		case FilterKind.TimeAfter:
		case FilterKind.TimeNe:
		case FilterKind.TimeBeforeEq:
		case FilterKind.TimeAfterEq:
		case FilterKind.TimeBetween:
		case FilterKind.TimeOutside:
			return typeof(TimeFilter);
		case FilterKind.DateYearMonthEq:
		case FilterKind.DateYearMonthBefore:
		case FilterKind.DateYearMonthAfter:
		case FilterKind.DateYearMonthNe:
		case FilterKind.DateYearMonthBeforeEq:
		case FilterKind.DateYearMonthAfterEq:
		case FilterKind.DateYearMonthBetween:
		case FilterKind.DateYearMonthOutside:
		case FilterKind.DateYearMonthMin:
		case FilterKind.DateYearMonthMax:
		case FilterKind.DateYearMonthCurrentMonth:
		case FilterKind.DateYearMonthYear:
		case FilterKind.DateYearMonthSeason:
		case FilterKind.DateYearMonthMonth:
			return typeof(DateYearMonthFilter);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
