using System;
using System.Collections.Generic;
using System.Drawing;

namespace Leqisoft.Model;

public class FormulaDisplayRef
{
	public FormulaDisplayRefKind Kind { get; set; }

	public List<Tuple<int, int>> Intervals { get; set; } = new List<Tuple<int, int>>();


	public Table Table { get; set; }

	public Cell Cell { get; set; }

	public Column Column { get; set; }

	public Cell Cell2 { get; set; }

	public int TitleOrFootRow { get; set; }

	public int TitleOrFootCol { get; set; }

	public Color Color { get; set; }
}
