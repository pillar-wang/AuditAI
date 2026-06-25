using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;

namespace Auditai.Model;

public class TreeTableNode : TreeNodeBase
{
	public Table Table { get; private set; } = new Table();


	public void SetTable(Table table)
	{
		Table = table;
	}

	public TreeTableNode()
	{
		Table.TreeNode = this;
	}

	public TreeTableNode DuplicateTable()
	{
		Table.LoadAndReturn();
		TreeTableNode treeTableNode = new TreeTableNode
		{
			Id = Project.Current.GetNextId(),
			Name = base.Name,
			IsEntityDirty = true,
			Visible = true
		};
		treeTableNode.Table._loaded = true;
		treeTableNode.Table.NeedSave = true;
		treeTableNode.Table.HeaderHeights = (int[])Table.HeaderHeights.Clone();
		treeTableNode.Table.Title.Deserialize(Table.Title.Serialize());
		treeTableNode.Table.BorderStyle = Table.BorderStyle;
		treeTableNode.Table.FilterInfo = Table.FilterInfo;
		treeTableNode.Table.CollectSource = string.Empty;
		treeTableNode.Table.Foot.Deserialize(Table.Foot.Serialize());
		treeTableNode.Table.Ticket.Deserialize(Table.Ticket.Serialize());
		treeTableNode.Table.ControlFormula = Table.ControlFormula;
		Dictionary<CellStyle, CellStyle> dictionary = new Dictionary<CellStyle, CellStyle>();
		foreach (CellStyle cellStyle3 in Table.CellStyles)
		{
			CellStyle cellStyle2 = (dictionary[cellStyle3] = cellStyle3.Duplicate());
			treeTableNode.Table.CellStyles.Add(cellStyle2);
		}
		treeTableNode.Table.DefaultStyle = dictionary[Table.DefaultStyle];
		foreach (Column column2 in Table.Columns)
		{
			Column column = column2.Duplicate();
			column.Table = treeTableNode.Table;
			if (column2.Style != null)
			{
				column.Style = dictionary[column2.Style];
			}
			treeTableNode.Table.Columns._list.Add(column);
		}
		foreach (Row row2 in Table.Rows)
		{
			Row row = row2.Duplicate();
			row.Table = treeTableNode.Table;
			treeTableNode.Table.Rows._list.Add(row);
			if (row.Role == RowRole.Header || row.Role == RowRole.Fixed)
			{
				treeTableNode.Table.HeaderRowCache.Add(row);
			}
		}
		foreach (Cell cell2 in Table.Cells)
		{
			Cell cell = cell2.Duplicate();
			cell.Row = treeTableNode.Table.Rows[cell2.Row.Index];
			cell.Column = treeTableNode.Table.Columns[cell2.Column.Index];
			if (cell2.Style != null)
			{
				cell.Style = dictionary[cell2.Style];
			}
			treeTableNode.Table.Cells._list.Add(cell);
		}
		foreach (CellMerge mergedCell in Table.MergedCells)
		{
			CellMerge item = new CellMerge
			{
				Id = Project.Current.GetNextId(),
				Status = SyncStatus.New,
				TopLeft = treeTableNode.Table[mergedCell.TopLeft.Row.Index, mergedCell.TopLeft.Column.Index],
				BottomRight = treeTableNode.Table[mergedCell.BottomRight.Row.Index, mergedCell.BottomRight.Column.Index]
			};
			treeTableNode.Table.MergedCells.Add(item);
		}
		return treeTableNode;
	}

	public void DuplicateFormulasForRecycleFile(Id64 recycledTableId, Table target, Dictionary<Id64, Table> tableDic, FormulaReferenceModelResolverForRecycleFile resolver, List<ValidationFormula> validationFormulasList)
	{
		DuplicateFormulas(recycledTableId, target, tableDic, transProject: false, resolver, validationFormulasList);
	}

	public void DuplicateFormulas(Table target, Dictionary<Id64, Table> dic, bool transProject)
	{
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(base.Project);
		List<ValidationFormula> validationFormulasList = base.Project.ValidationManager.Formulas.Where((ValidationFormula f) => f.TableId == Table.Id).ToList();
		DuplicateFormulas(Table.Id, target, dic, transProject, resolver, validationFormulasList);
	}

	protected void DuplicateFormulas(Id64 srcTableId, Table target, Dictionary<Id64, Table> dic, bool transProject, FormulaReferenceModelResolver resolver, List<ValidationFormula> validationFormulasList)
	{
		if (!string.IsNullOrWhiteSpace(Table.ControlFormula))
		{
			try
			{
				ControlFormulaEvaluator controlFormulaEvaluator = new ControlFormulaEvaluator(Table.ControlFormula);
				target.ControlFormula = controlFormulaEvaluator.DuplicateTableRewrite(resolver, dic, transProject);
			}
			catch (FormulaException)
			{
			}
			catch (InvalidOperationException)
			{
			}
		}
		foreach (Cell cell in Table.Cells)
		{
			if (cell.HasFormula)
			{
				try
				{
					FormulaEvaluator formulaEvaluator = new FormulaEvaluator(cell.Formula);
					target[cell.Row.Index, cell.Column.Index].Formula = formulaEvaluator.DuplicateTableRewrite(resolver, dic, transProject);
				}
				catch (FormulaException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
			if (cell.HasHeaderFormula)
			{
				try
				{
					FormulaEvaluator formulaEvaluator2 = new FormulaEvaluator(cell.HeaderFormula);
					target[cell.Row.Index, cell.Column.Index].HeaderFormula = formulaEvaluator2.DuplicateTableRewrite(resolver, dic, transProject);
				}
				catch (FormulaException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
		}
		foreach (Column column8 in Table.Columns)
		{
			if (column8.HasFormula)
			{
				try
				{
					FormulaEvaluator formulaEvaluator3 = new FormulaEvaluator(column8.Formula);
					target.Columns[column8.Index].Formula = formulaEvaluator3.DuplicateTableRewrite(resolver, dic, transProject);
				}
				catch (FormulaException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
			if (!string.IsNullOrWhiteSpace(column8.CaptionFormula))
			{
				try
				{
					FormulaEvaluator formulaEvaluator4 = new FormulaEvaluator(column8.CaptionFormula);
					target.Columns[column8.Index].CaptionFormula = formulaEvaluator4.DuplicateTableRewrite(resolver, dic, transProject);
				}
				catch (FormulaException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
		}
		CopyTitleCellFormula(Table.Title.TitleCell, target.Title.TitleCell);
		for (int i = 0; i < Table.Title.Rows.Count; i++)
		{
			TableTitleRow tableTitleRow = Table.Title.Rows[i];
			for (int j = 0; j < tableTitleRow.Cells.Count; j++)
			{
				CopyTitleCellFormula(tableTitleRow.Cells[j], target.Title.Rows[i].Cells[j]);
			}
		}
		for (int k = 0; k < Table.Foot.Rows.Count; k++)
		{
			TableTitleRow tableTitleRow2 = Table.Foot.Rows[k];
			for (int l = 0; l < tableTitleRow2.Cells.Count; l++)
			{
				CopyTitleCellFormula(tableTitleRow2.Cells[l], target.Foot.Rows[k].Cells[l]);
			}
		}
		Project project = ((srcTableId == Table.Id) ? base.Project : Project.Current);
		foreach (ValidationFormula validationFormulas in validationFormulasList)
		{
			ValidationFormula validationFormula = new ValidationFormula
			{
				Id = Project.Current.GetNextId(),
				Status = SyncStatus.New,
				IsDirty = false,
				LeftExpr = string.Empty,
				Note = validationFormulas.Note,
				Operator = validationFormulas.Operator,
				RightExpr = string.Empty,
				TableId = target.Id
			};
			if (!string.IsNullOrWhiteSpace(validationFormulas.LeftExpr))
			{
				try
				{
					FormulaEvaluator formulaEvaluator5 = new FormulaEvaluator(validationFormulas.LeftExpr);
					validationFormula.LeftExpr = formulaEvaluator5.DuplicateTableRewrite(resolver, dic, transProject);
				}
				catch (FormulaException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
			if (!string.IsNullOrWhiteSpace(validationFormulas.RightExpr))
			{
				try
				{
					FormulaEvaluator formulaEvaluator6 = new FormulaEvaluator(validationFormulas.RightExpr);
					validationFormula.RightExpr = formulaEvaluator6.DuplicateTableRewrite(resolver, dic, transProject);
				}
				catch (FormulaException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
			project.ValidationManager.Formulas.Add(validationFormula);
		}
		foreach (CellStyle cellStyle in target.CellStyles)
		{
			if (!cellStyle.Format.HasValue || (!cellStyle.Format.Value.HasComboList && !cellStyle.Format.Value.HasLedgerCollectFormula))
			{
				continue;
			}
			DataFormat value = cellStyle.Format.Value.Clone();
			if (cellStyle.Format.Value.HasComboList)
			{
				try
				{
					FormulaEvaluator formulaEvaluator7 = new FormulaEvaluator(cellStyle.Format.Value.ComboList);
					value.ComboList = formulaEvaluator7.DuplicateTableRewrite(resolver, dic, transProject);
				}
				catch (FormulaException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
			if (cellStyle.Format.Value.HasLedgerCollectFormula)
			{
				try
				{
					FormulaEvaluator formulaEvaluator8 = new FormulaEvaluator(cellStyle.Format.Value.LedgerCollectFormula);
					value.LedgerCollectFormula = formulaEvaluator8.DuplicateTableRewrite(resolver, dic, transProject);
				}
				catch (FormulaException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
			cellStyle.Format = value;
		}
		foreach (TicketCell cell2 in target.Ticket.Cells)
		{
			if (cell2.HasField())
			{
				if (srcTableId == Table.Id)
				{
					Column byId = Table.Columns.GetById(cell2.Field);
					if (byId != null)
					{
						cell2.Field = target.Columns[byId.Index].Id;
					}
				}
				else
				{
					try
					{
						Column column = resolver.ResolveTableColumn(srcTableId, cell2.Field);
						cell2.Field = target.Columns[column.Index].Id;
					}
					catch (Exception)
					{
					}
				}
			}
			if (cell2.HasFormula())
			{
				try
				{
					cell2.Formula = new FormulaEvaluator(cell2.Formula).DuplicateTableRewrite(resolver, dic, transProject);
				}
				catch (FormulaException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
		}
		foreach (TicketColumn column9 in target.Ticket.Columns)
		{
			if (column9.HasField())
			{
				if (srcTableId == Table.Id)
				{
					Column byId2 = Table.Columns.GetById(column9.Field);
					if (byId2 != null)
					{
						column9.Field = target.Columns[byId2.Index].Id;
					}
				}
				else
				{
					try
					{
						Column column2 = resolver.ResolveTableColumn(srcTableId, column9.Field);
						column9.Field = target.Columns[column2.Index].Id;
					}
					catch (Exception)
					{
					}
				}
			}
			if (column9.HasFormula())
			{
				try
				{
					column9.Formula = new FormulaEvaluator(column9.Formula).DuplicateTableRewrite(resolver, dic, transProject);
				}
				catch (FormulaException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
		}
		foreach (TicketNav nav in target.Ticket.Navs)
		{
			for (int m = 0; m < nav.Columns.Count; m++)
			{
				if (srcTableId == Table.Id)
				{
					Column byId3 = Table.Columns.GetById(nav.Columns[m]);
					if (byId3 != null)
					{
						nav.Columns[m] = target.Columns[byId3.Index].Id;
					}
				}
				else
				{
					try
					{
						Column column3 = resolver.ResolveTableColumn(srcTableId, nav.Columns[m]);
						nav.Columns[m] = target.Columns[column3.Index].Id;
					}
					catch (Exception)
					{
					}
				}
			}
		}
		if (target.Ticket.FixedAndDynamicMixRange != null)
		{
			if (target.Ticket.FixedAndDynamicMixRange.DataGroupKeyListForFixedDataRow != null)
			{
				foreach (TicketTableMixRangeDataGroupKey item in target.Ticket.FixedAndDynamicMixRange.DataGroupKeyListForFixedDataRow)
				{
					foreach (TicketTableMixRangeDataGroupKeyItem keyItem in item.KeyItems)
					{
						if (srcTableId == Table.Id)
						{
							Column byId4 = Table.Columns.GetById(keyItem.TableColumnId);
							if (byId4 != null)
							{
								keyItem.TableColumnId = target.Columns[byId4.Index].Id;
							}
						}
						else
						{
							try
							{
								Column column4 = resolver.ResolveTableColumn(srcTableId, keyItem.TableColumnId);
								keyItem.TableColumnId = target.Columns[column4.Index].Id;
							}
							catch (Exception)
							{
							}
						}
					}
				}
			}
			if (target.Ticket.FixedAndDynamicMixRange.DataGroupKeyListForDynamicDataRow != null)
			{
				foreach (TicketTableMixRangeDataGroupKey item2 in target.Ticket.FixedAndDynamicMixRange.DataGroupKeyListForDynamicDataRow)
				{
					foreach (TicketTableMixRangeDataGroupKeyItem keyItem2 in item2.KeyItems)
					{
						if (srcTableId == Table.Id)
						{
							Column byId5 = Table.Columns.GetById(keyItem2.TableColumnId);
							if (byId5 != null)
							{
								keyItem2.TableColumnId = target.Columns[byId5.Index].Id;
							}
						}
						else
						{
							try
							{
								Column column5 = resolver.ResolveTableColumn(srcTableId, keyItem2.TableColumnId);
								keyItem2.TableColumnId = target.Columns[column5.Index].Id;
							}
							catch (Exception)
							{
							}
						}
					}
				}
			}
		}
		RewriteTicketTitleFooterField(target.Ticket.Title);
		RewriteTicketTitleFooterField(target.Ticket.Footer);
		void CopyTitleCellFormula(TableTitleCell src, TableTitleCell dst)
		{
			if (!string.IsNullOrWhiteSpace(src.Formula))
			{
				try
				{
					FormulaEvaluator formulaEvaluator9 = new FormulaEvaluator(src.Formula);
					dst.Formula = formulaEvaluator9.DuplicateTableRewrite(resolver, dic, transProject);
				}
				catch (FormulaException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
			if (!string.IsNullOrWhiteSpace(src.ComboList))
			{
				try
				{
					FormulaEvaluator formulaEvaluator10 = new FormulaEvaluator(src.ComboList);
					dst.ComboList = formulaEvaluator10.DuplicateTableRewrite(resolver, dic, transProject);
				}
				catch (FormulaException)
				{
				}
				catch (InvalidOperationException)
				{
				}
			}
		}
		void RewriteTicketTitleFooterField(TicketTitleFooter dst)
		{
			foreach (TicketCell cell3 in dst.Cells)
			{
				if (cell3.HasField())
				{
					if (srcTableId == Table.Id)
					{
						Column byId6 = Table.Columns.GetById(cell3.Field);
						if (byId6 != null)
						{
							cell3.Field = target.Columns[byId6.Index].Id;
						}
					}
					else
					{
						try
						{
							Column column6 = resolver.ResolveTableColumn(srcTableId, cell3.Field);
							cell3.Field = target.Columns[column6.Index].Id;
						}
						catch (Exception)
						{
						}
					}
				}
				if (cell3.HasFormula())
				{
					try
					{
						cell3.Formula = new FormulaEvaluator(cell3.Formula).DuplicateTableRewrite(resolver, dic, transProject);
					}
					catch (FormulaException)
					{
					}
					catch (InvalidOperationException)
					{
					}
				}
			}
			foreach (TicketColumn column10 in dst.Columns)
			{
				if (column10.HasField())
				{
					if (srcTableId == Table.Id)
					{
						Column byId7 = Table.Columns.GetById(column10.Field);
						if (byId7 != null)
						{
							column10.Field = target.Columns[byId7.Index].Id;
						}
					}
					else
					{
						try
						{
							Column column7 = resolver.ResolveTableColumn(srcTableId, column10.Field);
							column10.Field = target.Columns[column7.Index].Id;
						}
						catch (Exception)
						{
						}
					}
				}
				if (column10.HasFormula())
				{
					try
					{
						column10.Formula = new FormulaEvaluator(column10.Formula).DuplicateTableRewrite(resolver, dic, transProject);
					}
					catch (FormulaException)
					{
					}
					catch (InvalidOperationException)
					{
					}
				}
			}
		}
	}

	public override void UpdateName(string name)
	{
		base.UpdateName(name);
		if (UserSet.Config.IsTitleFitTableName)
		{
			Table.Title.TitleCell.Value = name;
			Table.TagTitleDirty();
		}
	}

	protected internal override int GetCode()
	{
		return 1;
	}

	public override void Remove()
	{
		Table.LoadAndReturn();
		base.Project._dicTableNodes.Remove(base.Id);
		try
		{
			if (Table.LocalExists)
			{
				base.Project.SnapshotManager.SaveSnapshot(Table, isDeleting: true);
			}
		}
		catch
		{
		}
		List<ValidationFormula> list = base.Project.ValidationManager.Formulas.Where((ValidationFormula vf) => vf.TableId == base.Id).ToList();
		foreach (ValidationFormula item in list)
		{
			base.Project.ValidationManager.RemoveOne(item);
		}
		base.Project.FormulaManager.RemoveHostTable(Table.Id);
		base.Remove();
	}
}
