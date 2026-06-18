using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class TooltipListener : FormulaBaseListener
{
	public struct FormulaTooltipSegment
	{
		public string PreText { get; set; }

		public string Display { get; set; }

		public int AnchorNumber { get; set; }

		public object Ref { get; set; }
	}

	private string _text;

	private readonly FormulaReferenceResolver _resolver;

	private Table _contextTable;

	private int _anchorNumber;

	internal int _nextPreTextStart;

	private ParseTreeProperty<bool> _treeProperty = new ParseTreeProperty<bool>();

	private ValidationResult _vr;

	public List<FormulaTooltipSegment> Result { get; } = new List<FormulaTooltipSegment>();


	public TooltipListener(string text, FormulaReferenceResolver resolver, Table contextTable, ValidationResult vr)
	{
		_text = text;
		_contextTable = contextTable;
		_resolver = resolver;
		_vr = vr;
	}

	public override void ExitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Cell cell = _resolver.ResolveTableCell(arg, arg2);
		Result.Add(new FormulaTooltipSegment
		{
			AnchorNumber = _anchorNumber++,
			Display = $"{GetTableNamePart(cell._Table)}[{cell.Column.GetUniqueFormulaName()},{cell.Row.Index + 1}]",
			Ref = cell,
			PreText = _text.Substring(_nextPreTextStart, context.Start.StartIndex - _nextPreTextStart)
		});
		_nextPreTextStart = context.Stop.StopIndex + 1;
	}

	public override void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Column column = _resolver.ResolveTableColumn(arg, arg2);
		FormulaTooltipSegment formulaTooltipSegment = default(FormulaTooltipSegment);
		formulaTooltipSegment.AnchorNumber = _anchorNumber++;
		formulaTooltipSegment.Display = GetTableNamePart(column.Table) + "[" + column.GetUniqueFormulaName() + "]";
		formulaTooltipSegment.PreText = _text.Substring(_nextPreTextStart, context.Start.StartIndex - _nextPreTextStart);
		FormulaTooltipSegment item = formulaTooltipSegment;
		if (_treeProperty.Get(context) && _vr != null)
		{
			item.Ref = _vr.Refs.CellReferences.FirstOrDefault((Cell c) => c.Column == column);
		}
		else
		{
			item.Ref = column;
		}
		Result.Add(item);
		_nextPreTextStart = context.Stop.StopIndex + 1;
	}

	public override void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Column column = _resolver.ResolveTableColumn(arg, arg2);
		FormulaTooltipSegment formulaTooltipSegment = default(FormulaTooltipSegment);
		formulaTooltipSegment.AnchorNumber = _anchorNumber++;
		formulaTooltipSegment.Ref = column;
		formulaTooltipSegment.PreText = _text.Substring(_nextPreTextStart, context.Start.StartIndex - _nextPreTextStart);
		formulaTooltipSegment.Display = GetTableNamePart(column.Table) + "[" + column.GetUniqueFormulaName() + ",*]";
		FormulaTooltipSegment item = formulaTooltipSegment;
		Result.Add(item);
		_nextPreTextStart = context.Stop.StopIndex + 1;
	}

	public override void ExitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Cell cell = _resolver.ResolveTableCell(arg, arg2);
		Result.Add(new FormulaTooltipSegment
		{
			AnchorNumber = _anchorNumber++,
			Display = GetTableNamePart(cell._Table) + "[" + cell.GetUniqueFormulaName() + ",*]",
			Ref = cell,
			PreText = _text.Substring(_nextPreTextStart, context.Start.StartIndex - _nextPreTextStart)
		});
		_nextPreTextStart = context.Stop.StopIndex + 1;
	}

	public override void ExitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Id64 arg3 = Id64.Parse(context.Int(2).GetText());
		Cell cell = _resolver.ResolveTableCell(arg, arg2);
		Cell cell2 = _resolver.ResolveTableCell(arg, arg3);
		Result.Add(new FormulaTooltipSegment
		{
			AnchorNumber = _anchorNumber++,
			Display = $"{GetTableNamePart(cell._Table)}[{cell.Column.GetUniqueFormulaName()},{cell.Row.Index + 1}:{cell2.Column.GetUniqueFormulaName()},{cell2.Row.Index + 1}]",
			Ref = cell,
			PreText = _text.Substring(_nextPreTextStart, context.Start.StartIndex - _nextPreTextStart)
		});
		_nextPreTextStart = context.Stop.StopIndex + 1;
	}

	public override void ExitRefTreeNode([NotNull] FormulaParser.RefTreeNodeContext context)
	{
		Id64 arg = Id64.Parse(context.Int().GetText());
		TreeNodeBase treeNodeBase = _resolver.ResolveTreeNode(arg);
		Result.Add(new FormulaTooltipSegment
		{
			AnchorNumber = _anchorNumber++,
			Display = "{" + treeNodeBase.FormulaUniqueName + "}",
			Ref = treeNodeBase,
			PreText = _text.Substring(_nextPreTextStart, context.Start.StartIndex - _nextPreTextStart)
		});
		_nextPreTextStart = context.Stop.StopIndex + 1;
	}

	public override void ExitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Cell cell = _resolver.ResolveTableCell(arg, arg2);
		Result.Add(new FormulaTooltipSegment
		{
			AnchorNumber = _anchorNumber++,
			Display = GetTableNamePart(cell._Table) + "[" + cell.GetUniqueFormulaName() + "]",
			Ref = cell,
			PreText = _text.Substring(_nextPreTextStart, context.Start.StartIndex - _nextPreTextStart)
		});
		_nextPreTextStart = context.Stop.StopIndex + 1;
	}

	public override void EnterFunc([NotNull] FormulaParser.FuncContext context)
	{
		ITerminalNode terminalNode = context.FuncName();
		if (terminalNode != null)
		{
			string text = terminalNode.GetText();
			if ((text.Equals("LqSumIf", StringComparison.OrdinalIgnoreCase) || text.Equals("SumIf", StringComparison.OrdinalIgnoreCase)) && context.expr(1) is FormulaParser.RefColumnContext)
			{
				_treeProperty.Put(context.expr(1), value: true);
			}
		}
	}

	public override void ExitFunc([NotNull] FormulaParser.FuncContext context)
	{
	}

	private string GetTableNamePart(Table t)
	{
		if (_contextTable == null)
		{
			return "{" + t.GetCanonicalName() + "}";
		}
		if (t != _contextTable)
		{
			return "{" + t.GetCanonicalName() + "}";
		}
		return string.Empty;
	}
}
