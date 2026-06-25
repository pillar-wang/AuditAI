using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Auditai.DTO;

namespace Auditai.Model;

public class FormulaEvaluationVisitor : FormulaBaseVisitor<Operand>
{
	protected class FunInfo
	{
		public MethodInfo method;

		public bool isAcceptExpr;

		public System.Reflection.ParameterInfo lastParameter;

		public bool isParamArray;
	}

	protected readonly FormulaEvaluationEnvironment _env;

	protected static Dictionary<string, FunInfo> _funcDefineInfo;

	static FormulaEvaluationVisitor()
	{
		_funcDefineInfo = new Dictionary<string, FunInfo>(StringComparer.OrdinalIgnoreCase);
		MethodInfo[] methods = typeof(FunctionEvaluator).GetMethods(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			if (methodInfo.IsConstructor)
			{
				continue;
			}
			string name = methodInfo.Name;
			switch (name)
			{
			case "Equals":
			case "GetHashCode":
			case "Finalize":
			case "GetType":
			case "MemberwiseClone":
			case "ToString":
				continue;
			}
			if (!_funcDefineInfo.ContainsKey(name))
			{
				FunInfo funInfo = new FunInfo();
				_funcDefineInfo.Add(name, funInfo);
				funInfo.method = methodInfo;
				funInfo.isAcceptExpr = Attribute.IsDefined(funInfo.method, typeof(AcceptExprAttribute));
				funInfo.lastParameter = funInfo.method.GetParameters().LastOrDefault();
				funInfo.isParamArray = funInfo.lastParameter != null && Attribute.IsDefined(funInfo.lastParameter, typeof(ParamArrayAttribute));
			}
		}
	}

	public FormulaEvaluationVisitor(FormulaEvaluationEnvironment env)
	{
		_env = env;
	}

	public override Operand Visit(IParseTree tree)
	{
		return base.Visit(tree);
	}

	public override Operand VisitFormula([NotNull] FormulaParser.FormulaContext context)
	{
		return Visit(context.expr());
	}

	public override Operand VisitAddsub([NotNull] FormulaParser.AddsubContext context)
	{
		if (context.PLUS() != null)
		{
			return Visit(context.expr(0)).Add(Visit(context.expr(1)));
		}
		return Visit(context.expr(0)).Subtract(Visit(context.expr(1)));
	}

	public override Operand VisitAnd([NotNull] FormulaParser.AndContext context)
	{
		return Visit(context.expr(0)).And(Visit(context.expr(1)));
	}

	public override Operand VisitConcat([NotNull] FormulaParser.ConcatContext context)
	{
		return Visit(context.expr(0)).Concatenate(Visit(context.expr(1)));
	}

	public override Operand VisitEq([NotNull] FormulaParser.EqContext context)
	{
		return Visit(context.expr(0)).Equal(Visit(context.expr(1)));
	}

	public override Operand VisitFloat([NotNull] FormulaParser.FloatContext context)
	{
		return new NumberOperand(double.Parse(context.GetText()));
	}

	public override Operand VisitFunc([NotNull] FormulaParser.FuncContext context)
	{
		ITerminalNode terminalNode = context.FuncName();
		if (terminalNode == null)
		{
			throw new FormulaSyntaxException("缺少函数名称", context.Start.Column);
		}
		string text = terminalNode.GetText();
		if (!_funcDefineInfo.TryGetValue(text, out var value))
		{
			throw new FormulaFunctionNotExistException();
		}
		MethodInfo method = value.method;
		bool isAcceptExpr = value.isAcceptExpr;
		System.Reflection.ParameterInfo lastParameter = value.lastParameter;
		bool isParamArray = value.isParamArray;
		FunctionEvaluator obj = new FunctionEvaluator(_env, this);
		try
		{
			object obj2;
			if (isAcceptExpr && isParamArray)
			{
				obj2 = method.Invoke(obj, new object[1] { context.expr() });
			}
			else if (isAcceptExpr && !isParamArray)
			{
				object[] parameters = context.expr();
				obj2 = method.Invoke(obj, parameters);
			}
			else if (!isAcceptExpr && isParamArray)
			{
				obj2 = method.Invoke(obj, new object[1] { context.expr().Select(Visit).ToArray() });
			}
			else
			{
				object[] parameters = context.expr().Select(Visit).ToArray();
				obj2 = method.Invoke(obj, parameters);
			}
			return (obj2 is Operand operand) ? operand : ValueOperand.FromObject(obj2);
		}
		catch (TargetInvocationException ex)
		{
			ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			throw;
		}
		catch (TargetParameterCountException)
		{
			throw new FormulaParameterCountException();
		}
		catch (ArgumentException)
		{
			throw new FormulaTypeMismatchException();
		}
	}

	public override Operand VisitGt([NotNull] FormulaParser.GtContext context)
	{
		return Visit(context.expr(0)).GreaterThan(Visit(context.expr(1)));
	}

	public override Operand VisitGte([NotNull] FormulaParser.GteContext context)
	{
		return Visit(context.expr(0)).GreaterThanOrEqual(Visit(context.expr(1)));
	}

	public override Operand VisitInt([NotNull] FormulaParser.IntContext context)
	{
		return new NumberOperand(double.Parse(context.Int().GetText()));
	}

	public override Operand VisitLt([NotNull] FormulaParser.LtContext context)
	{
		return Visit(context.expr(0)).LessThan(Visit(context.expr(1)));
	}

	public override Operand VisitLte([NotNull] FormulaParser.LteContext context)
	{
		return Visit(context.expr(0)).LessThanOrEqual(Visit(context.expr(1)));
	}

	public override Operand VisitMuldiv([NotNull] FormulaParser.MuldivContext context)
	{
		if (context.MULTIPLY() != null)
		{
			return Visit(context.expr(0)).Multiply(Visit(context.expr(1)));
		}
		return Visit(context.expr(0)).Divide(Visit(context.expr(1)));
	}

	public override Operand VisitPower([NotNull] FormulaParser.PowerContext context)
	{
		NumberOperand numberOperand = Visit(context.expr(0)).ToNumber();
		NumberOperand numberOperand2 = Visit(context.expr(1)).ToNumber();
		return Math.Pow(numberOperand.Value, numberOperand2.Value);
	}

	public override Operand VisitNe([NotNull] FormulaParser.NeContext context)
	{
		return Visit(context.expr(0)).NotEqual(Visit(context.expr(1)));
	}

	public override Operand VisitNeg([NotNull] FormulaParser.NegContext context)
	{
		return Visit(context.expr()).Negate();
	}

	public override Operand VisitOr([NotNull] FormulaParser.OrContext context)
	{
		return Visit(context.expr(0)).Or(Visit(context.expr(1)));
	}

	public override Operand VisitParen([NotNull] FormulaParser.ParenContext context)
	{
		return Visit(context.expr());
	}

	public override Operand VisitRefCell([NotNull] FormulaParser.RefCellContext context)
	{
		if (_env == null)
		{
			return ErrorOperand.BadReference;
		}
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		try
		{
			return new CellOperand(_env.Resolver.ResolveTableCell(arg, arg2));
		}
		catch (FormulaBadReferenceException)
		{
			return ErrorOperand.BadReference;
		}
	}

	public override Operand VisitRefColumn([NotNull] FormulaParser.RefColumnContext context)
	{
		Id64 id = Id64.Parse(context.Int(0).GetText());
		Id64 arg = Id64.Parse(context.Int(1).GetText());
		if (arg.Value == long.MaxValue)
		{
			try
			{
				Table table = ((TreeTableNode)_env.Resolver.ResolveTreeNode(id)).Table;
				List<Cell> cells = table.Rows.Select((Row r, int i) => new Cell
				{
					Value = (double)(i + 1),
					Row = r
				}).ToList();
				return new CellsOperand(cells, table);
			}
			catch (FormulaBadReferenceException)
			{
				return ErrorOperand.BadReference;
			}
		}
		try
		{
			return new ColumnOperand(_env.Resolver.ResolveTableColumn(id, arg));
		}
		catch (FormulaBadReferenceException)
		{
			return ErrorOperand.BadReference;
		}
	}

	public override Operand VisitRefColumnWildcard([NotNull] FormulaParser.RefColumnWildcardContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		try
		{
			Column column = _env.Resolver.ResolveTableColumn(arg, arg2);
			return new CellOperand(column.Table[_env.RowIndex, column.Index]);
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new FormulaColumnWildcardNoRowException();
		}
		catch (FormulaBadReferenceException)
		{
			return ErrorOperand.BadReference;
		}
	}

	public override Operand VisitRefHeaderCellWildcard([NotNull] FormulaParser.RefHeaderCellWildcardContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		try
		{
			Cell cell = _env.Resolver.ResolveTableCell(arg, arg2);
			return new CellOperand(cell._Table[_env.RowIndex, cell.Column.Index]);
		}
		catch (ArgumentOutOfRangeException)
		{
			throw new FormulaColumnWildcardNoRowException();
		}
		catch (FormulaBadReferenceException)
		{
			return ErrorOperand.BadReference;
		}
	}

	public override Operand VisitRefRange([NotNull] FormulaParser.RefRangeContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		Id64 arg3 = Id64.Parse(context.Int(2).GetText());
		try
		{
			return new RangeOperand(_env.Resolver.ResolveTableCell(arg, arg2), _env.Resolver.ResolveTableCell(arg, arg3));
		}
		catch (FormulaBadReferenceException)
		{
			return ErrorOperand.BadReference;
		}
	}

	public override Operand VisitRefTreeNode([NotNull] FormulaParser.RefTreeNodeContext context)
	{
		Id64 arg = Id64.Parse(context.Int().GetText());
		return new TreeNodeOperand(_env.Resolver.ResolveTreeNode(arg));
	}

	public override Operand VisitRefHeaderCell([NotNull] FormulaParser.RefHeaderCellContext context)
	{
		Id64 arg = Id64.Parse(context.Int(0).GetText());
		Id64 arg2 = Id64.Parse(context.Int(1).GetText());
		try
		{
			Cell headerCell = _env.Resolver.ResolveTableCell(arg, arg2);
			return new HeaderCellOperand(headerCell);
		}
		catch (FormulaBadReferenceException)
		{
			return ErrorOperand.BadReference;
		}
	}

	public override Operand VisitString([NotNull] FormulaParser.StringContext context)
	{
		string text = context.String().GetText();
		text = text.Substring(1, text.Length - 2);
		text = Unescape(text);
		return new StringOperand(text);
	}

	private static string Unescape(string s)
	{
		return s.Replace("\"\"", "\"");
	}
}
