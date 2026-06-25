using System;
using Auditai.DTO;

namespace Auditai.Model;

public abstract class FormulaReferenceResolver
{
	protected internal abstract Func<Id64, TreeNodeBase> ResolveTreeNode { get; }

	protected internal abstract Func<Id64, Id64, Cell> ResolveTableCell { get; }

	protected internal abstract Func<Id64, Id64, Column> ResolveTableColumn { get; }

	protected internal abstract Func<Id64, Id64, Id64, RangeOperand> ResolveTableRange { get; }

	protected internal abstract Func<string, TreeNodeBase> ResolveTreeNodeString { get; }
}
