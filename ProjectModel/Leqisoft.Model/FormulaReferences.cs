using System;
using System.Collections.Generic;

namespace Leqisoft.Model;

public class FormulaReferences
{
	public HashSet<Cell> CellReferences { get; } = new HashSet<Cell>(CellIdComparer.Instance);


	public HashSet<RangeOperand> RangeReferences { get; } = new HashSet<RangeOperand>(RangeOperandEqualityComparer.Default);


	public HashSet<Column> ColumnReferences { get; } = new HashSet<Column>(ColumnIdComparer.Instance);


	public HashSet<Column> ColumnWildcardReferences { get; } = new HashSet<Column>(ColumnIdComparer.Instance);


	public HashSet<Tuple<Table, int, int>> TitleReferences { get; } = new HashSet<Tuple<Table, int, int>>();


	public HashSet<Tuple<Table, int, int>> FootReferences { get; } = new HashSet<Tuple<Table, int, int>>();


	public HashSet<Cell> HeaderCellReferences { get; } = new HashSet<Cell>(CellIdComparer.Instance);


	public HashSet<Cell> HeaderCellWildcardReferences { get; } = new HashSet<Cell>();


	public void UnionWith(FormulaReferences other)
	{
		CellReferences.UnionWith(other.CellReferences);
		RangeReferences.UnionWith(other.RangeReferences);
		ColumnReferences.UnionWith(other.ColumnReferences);
		ColumnWildcardReferences.UnionWith(other.ColumnWildcardReferences);
		TitleReferences.UnionWith(other.TitleReferences);
		FootReferences.UnionWith(other.FootReferences);
		HeaderCellReferences.UnionWith(other.HeaderCellReferences);
		HeaderCellWildcardReferences.UnionWith(other.HeaderCellWildcardReferences);
	}
}
