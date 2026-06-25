namespace Auditai.Model;

public enum FormulaTriggerKind
{
	None,
	Cell_Cell,
	Cell_Column,
	Range_Cell,
	Range_Column,
	ColumnWildcard_Cell,
	ColumnWildcard_Column,
	Column_Cell,
	Column_Column,
	Cell_HeaderCell,
	Range_HeaderCell,
	ColumnWildcard_HeaderCell,
	Column_HeaderCell,
	HeaderCell_Cell,
	HeaderCell_Column,
	HeaderCell_HeaderCell,
	HeaderCellWildcard_Cell,
	HeaderCellWildcard_HeaderCell,
	HeaderCellWildcard_Column
}
