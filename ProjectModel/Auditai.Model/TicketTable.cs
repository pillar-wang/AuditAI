using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;
using Newtonsoft.Json;

namespace Auditai.Model;

[JsonObject]
public class TicketTable
{
	private int _dataRowHeight = 30;

	private List<TicketRecord> _records = new List<TicketRecord>();

	[JsonIgnore]
	public Table Table { get; }

	public List<TicketColumn> Columns { get; } = new List<TicketColumn>();


	public List<TicketRow> Rows { get; } = new List<TicketRow>();


	public List<TicketCell> Cells { get; } = new List<TicketCell>();


	public List<TicketMerge> Merges { get; } = new List<TicketMerge>();


	public int DataRowStart { get; set; } = -1;


	public int DataRowCount { get; set; }

	public int TableRowsFrozenCount { get; set; }

	public int TableColsFrozenCount { get; set; }

	public int ColumnHeaderRowsCount { get; set; }

	public int DataRowHeight
	{
		get
		{
			return _dataRowHeight;
		}
		set
		{
			_dataRowHeight = Math.Max(1, value);
		}
	}

	public TicketKind Kind { get; set; }

	public TicketLevel Level { get; set; }

	public List<TicketMerge> DataRowMerges { get; } = new List<TicketMerge>();


	public List<TicketNav> Navs { get; set; } = new List<TicketNav>();


	public PageSetup PageSetup { get; set; } = new PageSetup();


	public TicketTitleFooter Title { get; set; } = new TicketTitleFooter();


	public TicketTitleFooter Footer { get; set; } = new TicketTitleFooter();


	public TicketTableFixedAndDynamicMixRange FixedAndDynamicMixRange { get; set; }

	[JsonIgnore]
	public Action<TicketTable> TicketReocrdRefreshCallback { get; set; }

	public bool IsDirtyDataOnlyIncludeCanOverrideByServerData { get; set; } = true;


	public bool IsHiddenColumn { get; set; }

	public bool IsAllowShowVirtualNode { get; set; } = true;


	[JsonIgnore]
	public List<TicketRecord> Records
	{
		get
		{
			if (IsCacheExpired)
			{
				RefreshCache();
				IsCacheExpired = false;
				if (TicketReocrdRefreshCallback != null)
				{
					TicketReocrdRefreshCallback(this);
				}
			}
			return _records;
		}
	}

	[JsonIgnore]
	public bool IsCacheExpired { get; set; } = true;


	private void RefreshCache()
	{
		_records.Clear();
		if (Kind == TicketKind.FixedOneRow)
		{
			foreach (Row row in Table.Rows)
			{
				if (row.Role != RowRole.Total && row.Role != RowRole.Fixed && row.Role != RowRole.Header)
				{
					_records.Add(new TicketRecord
					{
						Rows = new List<Row> { row }
					});
				}
			}
			return;
		}
		if (Kind == TicketKind.DynamicRow)
		{
			HashSet<Id64> fields = new HashSet<Id64>(from c in GetContainsKeyCells()
				where c.HasField()
				select c.Field);
			List<Column> source = Table.Columns.Where((Column c) => fields.Contains(c.Id)).ToList();
			List<int> keyColumnIndexes3 = source.Select((Column c) => c.Index).ToList();
			IEnumerable<IGrouping<List<Cell>, Row>> enumerable = Table.Rows.Where((Row row) => row.Role != RowRole.Total && row.Role != RowRole.Fixed && row.Role != RowRole.Header).GroupBy((Row row) => keyColumnIndexes3.Select((int i) => Table[row.Index, i]).ToList(), CellListByValueEqualsComparer.Instance);
			{
				foreach (IGrouping<List<Cell>, Row> item in enumerable)
				{
					_records.Add(new TicketRecord
					{
						Rows = item.ToList()
					});
				}
				return;
			}
		}
		if (Kind == TicketKind.FixedMultiRow)
		{
			List<TicketCell> source2 = GetContainsKeyCells();
			HashSet<Id64> keyFields2 = new HashSet<Id64>(from c in source2
				where c.HasField()
				group c by c.Field into g
				where g.Count() == 1
				select g.First().Field);
			List<Column> source3 = Table.Columns.Where((Column c) => keyFields2.Contains(c.Id)).ToList();
			List<int> keyColumnIndexes2 = source3.Select((Column c) => c.Index).ToList();
			IEnumerable<IGrouping<List<Cell>, Row>> enumerable2 = Table.Rows.Where((Row row) => row.Role != RowRole.Total && row.Role != RowRole.Fixed && row.Role != RowRole.Header).GroupBy((Row row) => keyColumnIndexes2.Select((int i) => Table[row.Index, i]).ToList(), CellListByValueEqualsComparer.Instance);
			List<Column> source4 = (from c in source2
				where c.HasText() && c.HasField()
				select GetFieldColumn(c.Field) into c
				where c != null
				select c).ToList();
			List<int> subkeyIndexes = source4.Select((Column c) => c.Index).ToList();
			{
				foreach (IGrouping<List<Cell>, Row> item2 in enumerable2)
				{
					IEnumerable<List<Row>> enumerable3 = from g in item2.GroupBy((Row row) => subkeyIndexes.Select((int i) => Table[row.Index, i]).ToList(), CellListByValueEqualsComparer.Instance)
						select g.ToList();
					for (int j = 0; j < enumerable3.Max((List<Row> g) => g.Count); j++)
					{
						List<Row> list = new List<Row>();
						foreach (List<Row> item3 in enumerable3)
						{
							if (j < item3.Count)
							{
								list.Add(item3[j]);
							}
						}
						_records.Add(new TicketRecord
						{
							Rows = list
						});
					}
				}
				return;
			}
		}
		if (Kind != TicketKind.FixedDataRowMixDynamicDataRow)
		{
			return;
		}
		List<TicketCell> source5 = GetContainsKeyCells();
		HashSet<Id64> keyFields = new HashSet<Id64>(from c in source5
			where c.HasField()
			group c by c.Field into g
			where g.Count() == 1
			select g.First().Field);
		List<Column> source6 = Table.Columns.Where((Column c) => keyFields.Contains(c.Id)).ToList();
		List<int> keyColumnIndexes = source6.Select((Column c) => c.Index).ToList();
		IEnumerable<IGrouping<List<Cell>, Row>> enumerable4 = Table.Rows.Where((Row row) => row.Role != RowRole.Total && row.Role != RowRole.Fixed && row.Role != RowRole.Header).GroupBy((Row row) => keyColumnIndexes.Select((int i) => Table[row.Index, i]).ToList(), CellListByValueEqualsComparer.Instance);
		foreach (IGrouping<List<Cell>, Row> item4 in enumerable4)
		{
			_records.Add(new TicketRecord
			{
				Rows = item4.ToList()
			});
		}
		List<TicketCell> GetContainsKeyCells()
		{
			List<TicketCell> list2 = new List<TicketCell>();
			list2.AddRange(Cells);
			list2.AddRange(Title.Cells);
			list2.AddRange(Footer.Cells);
			return list2;
		}
	}

	public TicketTable(Table table)
	{
		Table = table;
	}

	public TicketCell GetCell(int row, int col)
	{
		return Cells[row * Columns.Count + col];
	}

	public string Serialize()
	{
		return JsonConvert.SerializeObject(this);
	}

	public void Deserialize(string s)
	{
		Columns.Clear();
		Rows.Clear();
		Cells.Clear();
		Merges.Clear();
		DataRowMerges.Clear();
		Navs.Clear();
		Title.Clear();
		Footer.Clear();
		FixedAndDynamicMixRange = null;
		IsAllowShowVirtualNode = true;
		if (string.IsNullOrWhiteSpace(s))
		{
			return;
		}
		JsonConvert.PopulateObject(s, this);
		if (Level == TicketLevel.None)
		{
			if (Kind == TicketKind.FixedMultiRow)
			{
				Level = TicketLevel.Report;
			}
			else
			{
				Level = TicketLevel.Receipt;
			}
		}
	}

	public Column GetFieldColumn(Id64 columnId)
	{
		return Table.Columns.GetById(columnId);
	}

	public List<TicketCell> GetRowCells(int rowIndex)
	{
		return Cells.GetRange(rowIndex * Columns.Count, Columns.Count);
	}

	public bool HasDataRow()
	{
		return DataRowStart > -1;
	}

	public bool IsEmpty()
	{
		if (Cells.Count == 0 && Title.IsEmpty())
		{
			return Footer.IsEmpty();
		}
		return false;
	}

	public HashSet<Id64> GetReferencedTableColumnId()
	{
		HashSet<Id64> hashSet = new HashSet<Id64>();
		foreach (TicketCell item in from c in Cells.Union(Title.Cells).Union(Footer.Cells)
			where c.HasField()
			select c)
		{
			hashSet.Add(item.Field);
		}
		foreach (TicketColumn item2 in Columns.Where((TicketColumn c) => c.HasField()))
		{
			hashSet.Add(item2.Field);
		}
		return hashSet;
	}

	public bool IsCurrentLevelSupported()
	{
		return true;
	}

	public void Clear()
	{
		Columns.Clear();
		Rows.Clear();
		Cells.Clear();
		Merges.Clear();
		Title.Clear();
		Footer.Clear();
		DataRowStart = -1;
		DataRowCount = 0;
		DataRowHeight = 30;
		TableRowsFrozenCount = 0;
		Kind = TicketKind.None;
		DataRowMerges.Clear();
		Navs.Clear();
		_records.Clear();
		FixedAndDynamicMixRange = null;
		IsCacheExpired = true;
		IsDirtyDataOnlyIncludeCanOverrideByServerData = true;
		IsAllowShowVirtualNode = true;
	}

	public void SetSynced()
	{
		IsDirtyDataOnlyIncludeCanOverrideByServerData = true;
	}

	public string GetLevelString()
	{
		switch (Level)
		{
		case TicketLevel.None:
		case TicketLevel.Receipt:
			return "表单";
		case TicketLevel.Report:
			return "表单";
		default:
			return "";
		}
	}
}
