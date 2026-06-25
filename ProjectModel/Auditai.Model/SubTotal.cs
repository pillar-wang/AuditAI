using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;
using Newtonsoft.Json;

namespace Auditai.Model;

public class SubTotal
{
	public Table _table { get; set; }

	public List<Id64> GroupColumns { get; set; }

	public List<Id64> DataColumns { get; set; }

	public AggregateEnum method { get; set; }

	public DirectionEnum Direction { get; set; }

	public string TotalName { get; set; }

	public string GetSerilize => Serialize();

	private Func<int, int, Column, double> SubMethod { get; set; }

	public SubTotal(Table table)
	{
		_table = table;
		GroupColumns = new List<Id64>();
		DataColumns = new List<Id64>();
		TotalName = "汇总";
		method = AggregateEnum.Sum;
		Direction = DirectionEnum.Bottom;
	}

	public SubTotal(List<Id64> GroupCols, List<Id64> DataCols, string TotalName = "汇总")
	{
		GroupColumns = GroupCols;
		DataColumns = DataCols;
		method = AggregateEnum.Sum;
		Direction = DirectionEnum.Bottom;
		this.TotalName = TotalName;
	}

	private Dictionary<int, int> GetTotal()
	{
		Dictionary<int, string> Dic = _table.Columns.GetById(GroupColumns[0]).GetCells().ToDictionary((Cell t) => t.Row.Index, (Cell t) => "");
		GroupColumns.ForEach(delegate(Id64 c)
		{
			_table.Columns.GetById(c).GetCells().Count(delegate(Cell t)
			{
				Dic[t.Row.Index] += ((t.Value == null) ? "" : t.Value.ToString());
				return true;
			});
		});
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		string text = Dic[0];
		dictionary.Add(0, 0);
		foreach (KeyValuePair<int, string> item in Dic)
		{
			if (item.Value == text)
			{
				dictionary[dictionary.LastOrDefault().Key] = item.Key;
				continue;
			}
			dictionary.Add(item.Key, item.Key);
			text = Dic[item.Key];
		}
		return dictionary;
	}

	private void SelectMethod(AggregateEnum method)
	{
		switch (method)
		{
		case AggregateEnum.Sum:
			SubMethod = Sum;
			break;
		case AggregateEnum.Avg:
			SubMethod = Avg;
			break;
		case AggregateEnum.Count:
			SubMethod = Count;
			break;
		case AggregateEnum.Max:
			SubMethod = Max;
			break;
		case AggregateEnum.Min:
			SubMethod = Min;
			break;
		default:
			SubMethod = Sum;
			break;
		}
	}

	private double Sum(int row1, int row2, Column column)
	{
		return (from t in column.GetCells()
			where t.Row.Index >= row1 && t.Row.Index <= row2
			select t).ToList().Sum((Cell t) => Convert.ToDouble((t.Value == null || t.Value.ToString() == "") ? ((object)0) : t.Value));
	}

	private double Avg(int row1, int row2, Column column)
	{
		return (from t in column.GetCells()
			where t.Row.Index >= row1 && t.Row.Index <= row2
			select t).ToList().Average((Cell t) => Convert.ToDouble((t.Value == null || t.Value.ToString() == "") ? ((object)0) : t.Value));
	}

	private double Count(int row1, int row2, Column column)
	{
		return row2 - row1 + 1;
	}

	private double Max(int row1, int row2, Column column)
	{
		return (from t in column.GetCells()
			where t.Row.Index >= row1 && t.Row.Index <= row2
			select t).ToList().Max((Cell t) => Convert.ToDouble((t.Value == null || t.Value.ToString() == "") ? ((object)0) : t.Value));
	}

	private double Min(int row1, int row2, Column column)
	{
		return (from t in column.GetCells()
			where t.Row.Index >= row1 && t.Row.Index <= row2
			select t).ToList().Min((Cell t) => Convert.ToDouble((t.Value == null || t.Value.ToString() == "") ? ((object)0) : t.Value));
	}

	public Dictionary<Tuple<int, int, Id64>, double> Apply()
	{
		SelectMethod(method);
		Dictionary<int, int> total = GetTotal();
		Dictionary<Tuple<int, int, Id64>, double> dictionary = total.ToDictionary((KeyValuePair<int, int> t) => Tuple.Create(t.Key, t.Value, DataColumns[0]), (KeyValuePair<int, int> t) => SubMethod(t.Key, t.Value, _table.Columns.GetById(DataColumns[0])));
		foreach (Id64 col in DataColumns.Skip(1))
		{
			Dictionary<Tuple<int, int, Id64>, double> dictionary2 = total.ToDictionary((KeyValuePair<int, int> t) => Tuple.Create(t.Key, t.Value, col), (KeyValuePair<int, int> t) => SubMethod(t.Key, t.Value, _table.Columns.GetById(col)));
			foreach (KeyValuePair<Tuple<int, int, Id64>, double> item in dictionary2)
			{
				dictionary.Add(item.Key, item.Value);
			}
		}
		string jsonData = Serialize();
		Tuple<List<Guid>, List<Guid>, AggregateEnum, DirectionEnum, string> tuple = OppositeSerialize(jsonData);
		return dictionary;
	}

	public string Serialize()
	{
		return JsonConvert.SerializeObject(Tuple.Create(GroupColumns, DataColumns, method, Direction, TotalName));
	}

	public Tuple<List<Guid>, List<Guid>, AggregateEnum, DirectionEnum, string> OppositeSerialize(string JsonData)
	{
		return JsonConvert.DeserializeObject<Tuple<List<Guid>, List<Guid>, AggregateEnum, DirectionEnum, string>>(JsonData);
	}
}
