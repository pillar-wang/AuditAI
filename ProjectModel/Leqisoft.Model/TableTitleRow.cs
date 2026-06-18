﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Leqisoft.Model;

public class TableTitleRow
{
	private static int MIN_ROW_HEIGHT = 3;

	private static int MAX_ROW_HEIGHT = 9999;

	[JsonIgnore]
	internal Table _table;

	[JsonIgnore]
	private int _height;

	public int Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = Math.Min(MAX_ROW_HEIGHT, Math.Max(MIN_ROW_HEIGHT, value));
		}
	}

	public List<TableTitleCell> Cells { get; set; } = new List<TableTitleCell>();


	public TableTitleCell Left => Cells.Count > 0 ? Cells[0] : null;

	public TableTitleCell Center => Cells.Count > 1 ? Cells[1] : null;

	public TableTitleCell Right => Cells.Count > 2 ? Cells[2] : null;


	public bool IsEmpty()
	{
		return Cells.All((TableTitleCell c) => string.IsNullOrWhiteSpace(c.GetDisplayValue()));
	}
}
