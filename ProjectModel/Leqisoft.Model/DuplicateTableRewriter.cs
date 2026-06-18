using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class DuplicateTableRewriter : FormulaBaseListener
{
	private TokenStreamRewriter _rewriter;

	private readonly FormulaReferenceResolver _resolver;

	private Dictionary<Id64, Table> _dic;

	private bool _transProject;

	public DuplicateTableRewriter(TokenStreamRewriter rewriter, FormulaReferenceResolver resolver, Dictionary<Id64, Table> dic, bool transProject)
	{
		_rewriter = rewriter;
		_resolver = resolver;
		_dic = dic;
		_transProject = transProject;
	}

	public override void ExitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (_dic.TryGetValue(id, out var value))
		{
			Id64 arg = Id64.Parse(context.Int(1).GetText());
			Cell cell = _resolver.ResolveTableCell(id, arg);
			_rewriter.Replace(context.Int(0).Symbol, value.Id.ToString());
			_rewriter.Replace(context.Int(1).Symbol, value[cell.Row.Index, cell.Column.Index].Id.ToString());
		}
		else if (_transProject)
		{
			throw new InvalidOperationException();
		}
	}

	public override void ExitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (_dic.TryGetValue(id, out var value))
		{
			Id64 arg = Id64.Parse(context.Int(1).GetText());
			Cell cell = _resolver.ResolveTableCell(id, arg);
			_rewriter.Replace(context.Int(0).Symbol, value.Id.ToString());
			_rewriter.Replace(context.Int(1).Symbol, value[cell.Row.Index, cell.Column.Index].Id.ToString());
		}
		else if (_transProject)
		{
			throw new InvalidOperationException();
		}
	}

	public override void ExitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (_dic.TryGetValue(id, out var value))
		{
			Id64 arg = Id64.Parse(context.Int(1).GetText());
			Column column = _resolver.ResolveTableColumn(id, arg);
			_rewriter.Replace(context.Int(0).Symbol, value.Id.ToString());
			_rewriter.Replace(context.Int(1).Symbol, value.Columns[column.Index].Id.ToString());
		}
		else if (_transProject)
		{
			throw new InvalidOperationException();
		}
	}

	public override void ExitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (_dic.TryGetValue(id, out var value))
		{
			Id64 arg = Id64.Parse(context.Int(1).GetText());
			Column column = _resolver.ResolveTableColumn(id, arg);
			_rewriter.Replace(context.Int(0).Symbol, value.Id.ToString());
			_rewriter.Replace(context.Int(1).Symbol, value.Columns[column.Index].Id.ToString());
		}
		else if (_transProject)
		{
			throw new InvalidOperationException();
		}
	}

	public override void ExitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (_dic.TryGetValue(id, out var value))
		{
			Id64 arg = Id64.Parse(context.Int(1).GetText());
			Cell cell = _resolver.ResolveTableCell(id, arg);
			_rewriter.Replace(context.Int(0).Symbol, value.Id.ToString());
			_rewriter.Replace(context.Int(1).Symbol, value[cell.Row.Index, cell.Column.Index].Id.ToString());
		}
		else if (_transProject)
		{
			throw new InvalidOperationException();
		}
	}

	public override void ExitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		if (_dic.TryGetValue(id, out var value))
		{
			Id64 arg = Id64.Parse(context.Int(1).GetText());
			Id64 arg2 = Id64.Parse(context.Int(2).GetText());
			Cell cell = _resolver.ResolveTableCell(id, arg);
			Cell cell2 = _resolver.ResolveTableCell(id, arg2);
			_rewriter.Replace(context.Int(0).Symbol, value.Id.ToString());
			_rewriter.Replace(context.Int(1).Symbol, value[cell.Row.Index, cell.Column.Index].Id.ToString());
			_rewriter.Replace(context.Int(2).Symbol, value[cell2.Row.Index, cell2.Column.Index].Id.ToString());
		}
		else if (_transProject)
		{
			throw new InvalidOperationException();
		}
	}

	public override void ExitRefTreeNode([NotNull] FormulaParser.RefTreeNodeContext context)
	{
		Id64 key = Id64.Parse(context.Int().GetText());
		if (_dic.TryGetValue(key, out var value))
		{
			_rewriter.Replace(context.Int().Symbol, value.Id.ToString());
		}
		else if (_transProject)
		{
			throw new InvalidOperationException();
		}
	}
}
