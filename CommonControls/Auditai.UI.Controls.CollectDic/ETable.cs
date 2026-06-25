using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Auditai.UI.Controls.CollectDic;

[JsonObject("tb")]
public class ETable
{
	[JsonIgnore]
	private List<ERow> _rows;

	[JsonProperty("rs")]
	public List<ERow> Rows => _rows;

	public ETable()
	{
		_rows = new List<ERow>();
	}

	public void Add(ERow row)
	{
		_rows.Add(row);
	}

	public ERow FindRow(string col, Predicate<ECell> pre)
	{
		return _rows.Find((ERow r) => pre(r[col]));
	}

	public ERow FindRow(Predicate<ERow> pre)
	{
		return _rows.Find((ERow r) => pre(r));
	}
}
