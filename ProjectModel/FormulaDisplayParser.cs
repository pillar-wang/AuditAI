using System;
using System.CodeDom.Compiler;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.7.2")]
public class FormulaDisplayParser : Parser
{
	public class FuncNameContext : ParserRuleContext
	{
		public override int RuleIndex => 0;

		public ITerminalNode FuncName()
		{
			return GetToken(6, 0);
		}

		public ITerminalNode AND()
		{
			return GetToken(3, 0);
		}

		public ITerminalNode OR()
		{
			return GetToken(4, 0);
		}

		public FuncNameContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterFuncName(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitFuncName(this);
			}
		}
	}

	public class ColumnNameContext : ParserRuleContext
	{
		public override int RuleIndex => 1;

		public ITerminalNode ColumnName()
		{
			return GetToken(29, 0);
		}

		public ITerminalNode Int()
		{
			return GetToken(28, 0);
		}

		public ColumnNameContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterColumnName(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitColumnName(this);
			}
		}
	}

	public class RelOpContext : ParserRuleContext
	{
		public override int RuleIndex => 2;

		public ITerminalNode LT()
		{
			return GetToken(15, 0);
		}

		public ITerminalNode GT()
		{
			return GetToken(16, 0);
		}

		public ITerminalNode EQ()
		{
			return GetToken(17, 0);
		}

		public ITerminalNode AND()
		{
			return GetToken(3, 0);
		}

		public ITerminalNode OR()
		{
			return GetToken(4, 0);
		}

		public RelOpContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterRelOp(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitRelOp(this);
			}
		}
	}

	public class ExprContext : ParserRuleContext
	{
		public override int RuleIndex => 3;

		public ExprContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public ExprContext()
		{
		}

		public virtual void CopyFrom(ExprContext context)
		{
			base.CopyFrom(context);
		}
	}

	public class AddContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode Plus()
		{
			return GetToken(12, 0);
		}

		public AddContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterAdd(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitAdd(this);
			}
		}
	}

	public class TableCellContext : ExprContext
	{
		public ITerminalNode LBrace()
		{
			return GetToken(21, 0);
		}

		public ITerminalNode TableName()
		{
			return GetToken(23, 0);
		}

		public ITerminalNode RBrace()
		{
			return GetToken(24, 0);
		}

		public ITerminalNode LBracket()
		{
			return GetToken(22, 0);
		}

		public ColumnNameContext columnName()
		{
			return GetRuleContext<ColumnNameContext>(0);
		}

		public ITerminalNode Comma1()
		{
			return GetToken(26, 0);
		}

		public ITerminalNode Int()
		{
			return GetToken(28, 0);
		}

		public ITerminalNode RBracket()
		{
			return GetToken(30, 0);
		}

		public TableCellContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterTableCell(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitTableCell(this);
			}
		}
	}

	public class ColumnCellContext : ExprContext
	{
		public ITerminalNode LBracket()
		{
			return GetToken(22, 0);
		}

		public ColumnNameContext columnName()
		{
			return GetRuleContext<ColumnNameContext>(0);
		}

		public ITerminalNode Comma1()
		{
			return GetToken(26, 0);
		}

		public ITerminalNode Int()
		{
			return GetToken(28, 0);
		}

		public ITerminalNode RBracket()
		{
			return GetToken(30, 0);
		}

		public ColumnCellContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterColumnCell(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitColumnCell(this);
			}
		}
	}

	public class SubContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode Minus()
		{
			return GetToken(9, 0);
		}

		public SubContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterSub(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitSub(this);
			}
		}
	}

	public class StringContext : ExprContext
	{
		public ITerminalNode String()
		{
			return GetToken(2, 0);
		}

		public StringContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterString(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitString(this);
			}
		}
	}

	public class MulContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode Multiply()
		{
			return GetToken(10, 0);
		}

		public MulContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterMul(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitMul(this);
			}
		}
	}

	public class ColumnContext : ExprContext
	{
		public ITerminalNode LBracket()
		{
			return GetToken(22, 0);
		}

		public ColumnNameContext columnName()
		{
			return GetRuleContext<ColumnNameContext>(0);
		}

		public ITerminalNode RBracket()
		{
			return GetToken(30, 0);
		}

		public ColumnContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterColumn(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitColumn(this);
			}
		}
	}

	public class RangeContext : ExprContext
	{
		public ITerminalNode LBracket()
		{
			return GetToken(22, 0);
		}

		public ColumnNameContext[] columnName()
		{
			return GetRuleContexts<ColumnNameContext>();
		}

		public ColumnNameContext columnName(int i)
		{
			return GetRuleContext<ColumnNameContext>(i);
		}

		public ITerminalNode[] Comma1()
		{
			return GetTokens(26);
		}

		public ITerminalNode Comma1(int i)
		{
			return GetToken(26, i);
		}

		public ITerminalNode[] Int()
		{
			return GetTokens(28);
		}

		public ITerminalNode Int(int i)
		{
			return GetToken(28, i);
		}

		public ITerminalNode Colon()
		{
			return GetToken(27, 0);
		}

		public ITerminalNode RBracket()
		{
			return GetToken(30, 0);
		}

		public RangeContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterRange(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitRange(this);
			}
		}
	}

	public class ConcatContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode Ampersand()
		{
			return GetToken(13, 0);
		}

		public ConcatContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterConcat(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitConcat(this);
			}
		}
	}

	public class DivContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode Slash()
		{
			return GetToken(11, 0);
		}

		public DivContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterDiv(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitDiv(this);
			}
		}
	}

	public class NumberContext : ExprContext
	{
		public ITerminalNode Number()
		{
			return GetToken(1, 0);
		}

		public NumberContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterNumber(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitNumber(this);
			}
		}
	}

	public class NegContext : ExprContext
	{
		public ITerminalNode Minus()
		{
			return GetToken(9, 0);
		}

		public ExprContext expr()
		{
			return GetRuleContext<ExprContext>(0);
		}

		public NegContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterNeg(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitNeg(this);
			}
		}
	}

	public class ParenContext : ExprContext
	{
		public ITerminalNode LParen()
		{
			return GetToken(18, 0);
		}

		public ExprContext expr()
		{
			return GetRuleContext<ExprContext>(0);
		}

		public ITerminalNode RParen()
		{
			return GetToken(19, 0);
		}

		public ParenContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterParen(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitParen(this);
			}
		}
	}

	public class FuncContext : ExprContext
	{
		public FuncNameContext funcName()
		{
			return GetRuleContext<FuncNameContext>(0);
		}

		public ITerminalNode LParen()
		{
			return GetToken(18, 0);
		}

		public ITerminalNode RParen()
		{
			return GetToken(19, 0);
		}

		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode[] Comma()
		{
			return GetTokens(8);
		}

		public ITerminalNode Comma(int i)
		{
			return GetToken(8, i);
		}

		public FuncContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterFunc(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitFunc(this);
			}
		}
	}

	public class TableColumnWildcardContext : ExprContext
	{
		public ITerminalNode LBrace()
		{
			return GetToken(21, 0);
		}

		public ITerminalNode TableName()
		{
			return GetToken(23, 0);
		}

		public ITerminalNode RBrace()
		{
			return GetToken(24, 0);
		}

		public ITerminalNode LBracket()
		{
			return GetToken(22, 0);
		}

		public ColumnNameContext columnName()
		{
			return GetRuleContext<ColumnNameContext>(0);
		}

		public ITerminalNode Comma1()
		{
			return GetToken(26, 0);
		}

		public ITerminalNode Asterisk()
		{
			return GetToken(25, 0);
		}

		public ITerminalNode RBracket()
		{
			return GetToken(30, 0);
		}

		public TableColumnWildcardContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterTableColumnWildcard(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitTableColumnWildcard(this);
			}
		}
	}

	public class ColumnWildcardContext : ExprContext
	{
		public ITerminalNode LBracket()
		{
			return GetToken(22, 0);
		}

		public ColumnNameContext columnName()
		{
			return GetRuleContext<ColumnNameContext>(0);
		}

		public ITerminalNode Comma1()
		{
			return GetToken(26, 0);
		}

		public ITerminalNode Asterisk()
		{
			return GetToken(25, 0);
		}

		public ITerminalNode RBracket()
		{
			return GetToken(30, 0);
		}

		public ColumnWildcardContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterColumnWildcard(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitColumnWildcard(this);
			}
		}
	}

	public class RelContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public RelOpContext relOp()
		{
			return GetRuleContext<RelOpContext>(0);
		}

		public RelContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterRel(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitRel(this);
			}
		}
	}

	public class TableColumnContext : ExprContext
	{
		public ITerminalNode LBrace()
		{
			return GetToken(21, 0);
		}

		public ITerminalNode TableName()
		{
			return GetToken(23, 0);
		}

		public ITerminalNode RBrace()
		{
			return GetToken(24, 0);
		}

		public ITerminalNode LBracket()
		{
			return GetToken(22, 0);
		}

		public ColumnNameContext columnName()
		{
			return GetRuleContext<ColumnNameContext>(0);
		}

		public ITerminalNode RBracket()
		{
			return GetToken(30, 0);
		}

		public TableColumnContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterTableColumn(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitTableColumn(this);
			}
		}
	}

	public class TableRangeContext : ExprContext
	{
		public ITerminalNode LBrace()
		{
			return GetToken(21, 0);
		}

		public ITerminalNode TableName()
		{
			return GetToken(23, 0);
		}

		public ITerminalNode RBrace()
		{
			return GetToken(24, 0);
		}

		public ITerminalNode LBracket()
		{
			return GetToken(22, 0);
		}

		public ColumnNameContext[] columnName()
		{
			return GetRuleContexts<ColumnNameContext>();
		}

		public ColumnNameContext columnName(int i)
		{
			return GetRuleContext<ColumnNameContext>(i);
		}

		public ITerminalNode[] Comma1()
		{
			return GetTokens(26);
		}

		public ITerminalNode Comma1(int i)
		{
			return GetToken(26, i);
		}

		public ITerminalNode[] Int()
		{
			return GetTokens(28);
		}

		public ITerminalNode Int(int i)
		{
			return GetToken(28, i);
		}

		public ITerminalNode Colon()
		{
			return GetToken(27, 0);
		}

		public ITerminalNode RBracket()
		{
			return GetToken(30, 0);
		}

		public TableRangeContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterTableRange(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitTableRange(this);
			}
		}
	}

	public class PowerContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode Hat()
		{
			return GetToken(14, 0);
		}

		public PowerContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterPower(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitPower(this);
			}
		}
	}

	public class TreeNodeContext : ExprContext
	{
		public ITerminalNode LBrace()
		{
			return GetToken(21, 0);
		}

		public ITerminalNode TableName()
		{
			return GetToken(23, 0);
		}

		public ITerminalNode RBrace()
		{
			return GetToken(24, 0);
		}

		public TreeNodeContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterTreeNode(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitTreeNode(this);
			}
		}
	}

	public class FormulaContext : ParserRuleContext
	{
		public override int RuleIndex => 4;

		public ExprContext expr()
		{
			return GetRuleContext<ExprContext>(0);
		}

		public ITerminalNode Eof()
		{
			return GetToken(-1, 0);
		}

		public ITerminalNode EQ()
		{
			return GetToken(17, 0);
		}

		public FormulaContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterFormula(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitFormula(this);
			}
		}
	}

	public class ExprsContext : ParserRuleContext
	{
		public override int RuleIndex => 5;

		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode[] Semicolon()
		{
			return GetTokens(20);
		}

		public ITerminalNode Semicolon(int i)
		{
			return GetToken(20, i);
		}

		public ExprsContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.EnterExprs(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaDisplayParserListener formulaDisplayParserListener)
			{
				formulaDisplayParserListener.ExitExprs(this);
			}
		}
	}

	protected static DFA[] decisionToDFA;

	protected static PredictionContextCache sharedContextCache;

	public const int Number = 1;

	public const int String = 2;

	public const int AND = 3;

	public const int OR = 4;

	public const int WS = 5;

	public const int FuncName = 6;

	public const int Dot = 7;

	public const int Comma = 8;

	public const int Minus = 9;

	public const int Multiply = 10;

	public const int Slash = 11;

	public const int Plus = 12;

	public const int Ampersand = 13;

	public const int Hat = 14;

	public const int LT = 15;

	public const int GT = 16;

	public const int EQ = 17;

	public const int LParen = 18;

	public const int RParen = 19;

	public const int Semicolon = 20;

	public const int LBrace = 21;

	public const int LBracket = 22;

	public const int TableName = 23;

	public const int RBrace = 24;

	public const int Asterisk = 25;

	public const int Comma1 = 26;

	public const int Colon = 27;

	public const int Int = 28;

	public const int ColumnName = 29;

	public const int RBracket = 30;

	public const int RULE_funcName = 0;

	public const int RULE_columnName = 1;

	public const int RULE_relOp = 2;

	public const int RULE_expr = 3;

	public const int RULE_formula = 4;

	public const int RULE_exprs = 5;

	public static readonly string[] ruleNames;

	private static readonly string[] _LiteralNames;

	private static readonly string[] _SymbolicNames;

	public static readonly IVocabulary DefaultVocabulary;

	private static char[] _serializedATN;

	public static readonly ATN _ATN;

	[NotNull]
	public override IVocabulary Vocabulary => DefaultVocabulary;

	public override string GrammarFileName => "FormulaDisplayParser.g4";

	public override string[] RuleNames => ruleNames;

	public override string SerializedAtn => new string(_serializedATN);

	static FormulaDisplayParser()
	{
		sharedContextCache = new PredictionContextCache();
		ruleNames = new string[6] { "funcName", "columnName", "relOp", "expr", "formula", "exprs" };
		_LiteralNames = new string[31]
		{
			null, null, null, null, null, null, null, "'.'", null, null,
			null, null, null, "'&'", "'^'", "'<'", "'>'", null, null, null,
			"';'", "'{'", "'['", null, "'}'", "'*'", "','", "':'", null, null,
			"']'"
		};
		_SymbolicNames = new string[31]
		{
			null, "Number", "String", "AND", "OR", "WS", "FuncName", "Dot", "Comma", "Minus",
			"Multiply", "Slash", "Plus", "Ampersand", "Hat", "LT", "GT", "EQ", "LParen", "RParen",
			"Semicolon", "LBrace", "LBracket", "TableName", "RBrace", "Asterisk", "Comma1", "Colon", "Int", "ColumnName",
			"RBracket"
		};
		DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);
		_serializedATN = new char[1503]
		{
			'\u0003', '悋', 'Ꜫ', '脳', '맭', '䅼', '㯧', '瞆', '奤', '\u0003',
			' ', '¥', '\u0004', '\u0002', '\t', '\u0002', '\u0004', '\u0003', '\t', '\u0003',
			'\u0004', '\u0004', '\t', '\u0004', '\u0004', '\u0005', '\t', '\u0005', '\u0004', '\u0006',
			'\t', '\u0006', '\u0004', '\a', '\t', '\a', '\u0003', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0003', '\u0003', '\u0003', '\u0003', '\u0004', '\u0003', '\u0004', '\u0003', '\u0004',
			'\u0003', '\u0004', '\u0003', '\u0004', '\u0003', '\u0004', '\u0003', '\u0004', '\u0003', '\u0004',
			'\u0003', '\u0004', '\u0003', '\u0004', '\u0003', '\u0004', '\u0005', '\u0004', '\u001e', '\n',
			'\u0004', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003',
			'\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\a', '\u0005', '(',
			'\n', '\u0005', '\f', '\u0005', '\u000e', '\u0005', '+', '\v', '\u0005', '\u0005',
			'\u0005', '-', '\n', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0005', '\u0005', 'z', '\n', '\u0005', '\u0003',
			'\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003',
			'\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003',
			'\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003',
			'\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003',
			'\u0005', '\u0003', '\u0005', '\a', '\u0005', '\u0092', '\n', '\u0005', '\f', '\u0005',
			'\u000e', '\u0005', '\u0095', '\v', '\u0005', '\u0003', '\u0006', '\u0005', '\u0006', '\u0098',
			'\n', '\u0006', '\u0003', '\u0006', '\u0003', '\u0006', '\u0003', '\u0006', '\u0003', '\a',
			'\u0003', '\a', '\u0003', '\a', '\a', '\a', '\u00a0', '\n', '\a', '\f',
			'\a', '\u000e', '\a', '£', '\v', '\a', '\u0003', '\a', '\u0002', '\u0003',
			'\b', '\b', '\u0002', '\u0004', '\u0006', '\b', '\n', '\f', '\u0002', '\u0004',
			'\u0004', '\u0002', '\u0005', '\u0006', '\b', '\b', '\u0003', '\u0002', '\u001e', '\u001f',
			'\u0002', '½', '\u0002', '\u000e', '\u0003', '\u0002', '\u0002', '\u0002', '\u0004', '\u0010',
			'\u0003', '\u0002', '\u0002', '\u0002', '\u0006', '\u001d', '\u0003', '\u0002', '\u0002', '\u0002',
			'\b', 'y', '\u0003', '\u0002', '\u0002', '\u0002', '\n', '\u0097', '\u0003', '\u0002',
			'\u0002', '\u0002', '\f', '\u009c', '\u0003', '\u0002', '\u0002', '\u0002', '\u000e', '\u000f',
			'\t', '\u0002', '\u0002', '\u0002', '\u000f', '\u0003', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u0010', '\u0011', '\t', '\u0003', '\u0002', '\u0002', '\u0011', '\u0005', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u0012', '\u001e', '\a', '\u0011', '\u0002', '\u0002', '\u0013', '\u001e',
			'\a', '\u0012', '\u0002', '\u0002', '\u0014', '\u001e', '\a', '\u0013', '\u0002', '\u0002',
			'\u0015', '\u0016', '\a', '\u0011', '\u0002', '\u0002', '\u0016', '\u001e', '\a', '\u0013',
			'\u0002', '\u0002', '\u0017', '\u0018', '\a', '\u0012', '\u0002', '\u0002', '\u0018', '\u001e',
			'\a', '\u0013', '\u0002', '\u0002', '\u0019', '\u001a', '\a', '\u0011', '\u0002', '\u0002',
			'\u001a', '\u001e', '\a', '\u0012', '\u0002', '\u0002', '\u001b', '\u001e', '\a', '\u0005',
			'\u0002', '\u0002', '\u001c', '\u001e', '\a', '\u0006', '\u0002', '\u0002', '\u001d', '\u0012',
			'\u0003', '\u0002', '\u0002', '\u0002', '\u001d', '\u0013', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u001d', '\u0014', '\u0003', '\u0002', '\u0002', '\u0002', '\u001d', '\u0015', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u001d', '\u0017', '\u0003', '\u0002', '\u0002', '\u0002', '\u001d', '\u0019',
			'\u0003', '\u0002', '\u0002', '\u0002', '\u001d', '\u001b', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u001d', '\u001c', '\u0003', '\u0002', '\u0002', '\u0002', '\u001e', '\a', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u001f', ' ', '\b', '\u0005', '\u0001', '\u0002', ' ', 'z',
			'\a', '\u0003', '\u0002', '\u0002', '!', 'z', '\a', '\u0004', '\u0002', '\u0002',
			'"', '#', '\u0005', '\u0002', '\u0002', '\u0002', '#', ',', '\a', '\u0014',
			'\u0002', '\u0002', '$', ')', '\u0005', '\b', '\u0005', '\u0002', '%', '&',
			'\a', '\n', '\u0002', '\u0002', '&', '(', '\u0005', '\b', '\u0005', '\u0002',
			'\'', '%', '\u0003', '\u0002', '\u0002', '\u0002', '(', '+', '\u0003', '\u0002',
			'\u0002', '\u0002', ')', '\'', '\u0003', '\u0002', '\u0002', '\u0002', ')', '*',
			'\u0003', '\u0002', '\u0002', '\u0002', '*', '-', '\u0003', '\u0002', '\u0002', '\u0002',
			'+', ')', '\u0003', '\u0002', '\u0002', '\u0002', ',', '$', '\u0003', '\u0002',
			'\u0002', '\u0002', ',', '-', '\u0003', '\u0002', '\u0002', '\u0002', '-', '.',
			'\u0003', '\u0002', '\u0002', '\u0002', '.', '/', '\a', '\u0015', '\u0002', '\u0002',
			'/', 'z', '\u0003', '\u0002', '\u0002', '\u0002', '0', '1', '\a', '\u0017',
			'\u0002', '\u0002', '1', '2', '\a', '\u0019', '\u0002', '\u0002', '2', '3',
			'\a', '\u001a', '\u0002', '\u0002', '3', '4', '\a', '\u0018', '\u0002', '\u0002',
			'4', '5', '\u0005', '\u0004', '\u0003', '\u0002', '5', '6', '\a', '\u001c',
			'\u0002', '\u0002', '6', '7', '\a', '\u001e', '\u0002', '\u0002', '7', '8',
			'\a', ' ', '\u0002', '\u0002', '8', 'z', '\u0003', '\u0002', '\u0002', '\u0002',
			'9', ':', '\a', '\u0018', '\u0002', '\u0002', ':', ';', '\u0005', '\u0004',
			'\u0003', '\u0002', ';', '<', '\a', '\u001c', '\u0002', '\u0002', '<', '=',
			'\a', '\u001e', '\u0002', '\u0002', '=', '>', '\a', ' ', '\u0002', '\u0002',
			'>', 'z', '\u0003', '\u0002', '\u0002', '\u0002', '?', '@', '\a', '\u0017',
			'\u0002', '\u0002', '@', 'A', '\a', '\u0019', '\u0002', '\u0002', 'A', 'B',
			'\a', '\u001a', '\u0002', '\u0002', 'B', 'C', '\a', '\u0018', '\u0002', '\u0002',
			'C', 'D', '\u0005', '\u0004', '\u0003', '\u0002', 'D', 'E', '\a', ' ',
			'\u0002', '\u0002', 'E', 'z', '\u0003', '\u0002', '\u0002', '\u0002', 'F', 'G',
			'\a', '\u0018', '\u0002', '\u0002', 'G', 'H', '\u0005', '\u0004', '\u0003', '\u0002',
			'H', 'I', '\a', ' ', '\u0002', '\u0002', 'I', 'z', '\u0003', '\u0002',
			'\u0002', '\u0002', 'J', 'K', '\a', '\u0017', '\u0002', '\u0002', 'K', 'L',
			'\a', '\u0019', '\u0002', '\u0002', 'L', 'M', '\a', '\u001a', '\u0002', '\u0002',
			'M', 'N', '\a', '\u0018', '\u0002', '\u0002', 'N', 'O', '\u0005', '\u0004',
			'\u0003', '\u0002', 'O', 'P', '\a', '\u001c', '\u0002', '\u0002', 'P', 'Q',
			'\a', '\u001b', '\u0002', '\u0002', 'Q', 'R', '\a', ' ', '\u0002', '\u0002',
			'R', 'z', '\u0003', '\u0002', '\u0002', '\u0002', 'S', 'T', '\a', '\u0018',
			'\u0002', '\u0002', 'T', 'U', '\u0005', '\u0004', '\u0003', '\u0002', 'U', 'V',
			'\a', '\u001c', '\u0002', '\u0002', 'V', 'W', '\a', '\u001b', '\u0002', '\u0002',
			'W', 'X', '\a', ' ', '\u0002', '\u0002', 'X', 'z', '\u0003', '\u0002',
			'\u0002', '\u0002', 'Y', 'Z', '\a', '\u0017', '\u0002', '\u0002', 'Z', '[',
			'\a', '\u0019', '\u0002', '\u0002', '[', '\\', '\a', '\u001a', '\u0002', '\u0002',
			'\\', ']', '\a', '\u0018', '\u0002', '\u0002', ']', '^', '\u0005', '\u0004',
			'\u0003', '\u0002', '^', '_', '\a', '\u001c', '\u0002', '\u0002', '_', '`',
			'\a', '\u001e', '\u0002', '\u0002', '`', 'a', '\a', '\u001d', '\u0002', '\u0002',
			'a', 'b', '\u0005', '\u0004', '\u0003', '\u0002', 'b', 'c', '\a', '\u001c',
			'\u0002', '\u0002', 'c', 'd', '\a', '\u001e', '\u0002', '\u0002', 'd', 'e',
			'\a', ' ', '\u0002', '\u0002', 'e', 'z', '\u0003', '\u0002', '\u0002', '\u0002',
			'f', 'g', '\a', '\u0018', '\u0002', '\u0002', 'g', 'h', '\u0005', '\u0004',
			'\u0003', '\u0002', 'h', 'i', '\a', '\u001c', '\u0002', '\u0002', 'i', 'j',
			'\a', '\u001e', '\u0002', '\u0002', 'j', 'k', '\a', '\u001d', '\u0002', '\u0002',
			'k', 'l', '\u0005', '\u0004', '\u0003', '\u0002', 'l', 'm', '\a', '\u001c',
			'\u0002', '\u0002', 'm', 'n', '\a', '\u001e', '\u0002', '\u0002', 'n', 'o',
			'\a', ' ', '\u0002', '\u0002', 'o', 'z', '\u0003', '\u0002', '\u0002', '\u0002',
			'p', 'q', '\a', '\u0017', '\u0002', '\u0002', 'q', 'r', '\a', '\u0019',
			'\u0002', '\u0002', 'r', 'z', '\a', '\u001a', '\u0002', '\u0002', 's', 't',
			'\a', '\u0014', '\u0002', '\u0002', 't', 'u', '\u0005', '\b', '\u0005', '\u0002',
			'u', 'v', '\a', '\u0015', '\u0002', '\u0002', 'v', 'z', '\u0003', '\u0002',
			'\u0002', '\u0002', 'w', 'x', '\a', '\v', '\u0002', '\u0002', 'x', 'z',
			'\u0005', '\b', '\u0005', '\n', 'y', '\u001f', '\u0003', '\u0002', '\u0002', '\u0002',
			'y', '!', '\u0003', '\u0002', '\u0002', '\u0002', 'y', '"', '\u0003', '\u0002',
			'\u0002', '\u0002', 'y', '0', '\u0003', '\u0002', '\u0002', '\u0002', 'y', '9',
			'\u0003', '\u0002', '\u0002', '\u0002', 'y', '?', '\u0003', '\u0002', '\u0002', '\u0002',
			'y', 'F', '\u0003', '\u0002', '\u0002', '\u0002', 'y', 'J', '\u0003', '\u0002',
			'\u0002', '\u0002', 'y', 'S', '\u0003', '\u0002', '\u0002', '\u0002', 'y', 'Y',
			'\u0003', '\u0002', '\u0002', '\u0002', 'y', 'f', '\u0003', '\u0002', '\u0002', '\u0002',
			'y', 'p', '\u0003', '\u0002', '\u0002', '\u0002', 'y', 's', '\u0003', '\u0002',
			'\u0002', '\u0002', 'y', 'w', '\u0003', '\u0002', '\u0002', '\u0002', 'z', '\u0093',
			'\u0003', '\u0002', '\u0002', '\u0002', '{', '|', '\f', '\t', '\u0002', '\u0002',
			'|', '}', '\a', '\u0010', '\u0002', '\u0002', '}', '\u0092', '\u0005', '\b',
			'\u0005', '\n', '~', '\u007f', '\f', '\b', '\u0002', '\u0002', '\u007f', '\u0080',
			'\a', '\f', '\u0002', '\u0002', '\u0080', '\u0092', '\u0005', '\b', '\u0005', '\t',
			'\u0081', '\u0082', '\f', '\a', '\u0002', '\u0002', '\u0082', '\u0083', '\a', '\r',
			'\u0002', '\u0002', '\u0083', '\u0092', '\u0005', '\b', '\u0005', '\b', '\u0084', '\u0085',
			'\f', '\u0006', '\u0002', '\u0002', '\u0085', '\u0086', '\a', '\u000e', '\u0002', '\u0002',
			'\u0086', '\u0092', '\u0005', '\b', '\u0005', '\a', '\u0087', '\u0088', '\f', '\u0005',
			'\u0002', '\u0002', '\u0088', '\u0089', '\a', '\v', '\u0002', '\u0002', '\u0089', '\u0092',
			'\u0005', '\b', '\u0005', '\u0006', '\u008a', '\u008b', '\f', '\u0004', '\u0002', '\u0002',
			'\u008b', '\u008c', '\a', '\u000f', '\u0002', '\u0002', '\u008c', '\u0092', '\u0005', '\b',
			'\u0005', '\u0005', '\u008d', '\u008e', '\f', '\u0003', '\u0002', '\u0002', '\u008e', '\u008f',
			'\u0005', '\u0006', '\u0004', '\u0002', '\u008f', '\u0090', '\u0005', '\b', '\u0005', '\u0004',
			'\u0090', '\u0092', '\u0003', '\u0002', '\u0002', '\u0002', '\u0091', '{', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u0091', '~', '\u0003', '\u0002', '\u0002', '\u0002', '\u0091', '\u0081',
			'\u0003', '\u0002', '\u0002', '\u0002', '\u0091', '\u0084', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u0091', '\u0087', '\u0003', '\u0002', '\u0002', '\u0002', '\u0091', '\u008a', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u0091', '\u008d', '\u0003', '\u0002', '\u0002', '\u0002', '\u0092', '\u0095',
			'\u0003', '\u0002', '\u0002', '\u0002', '\u0093', '\u0091', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u0093', '\u0094', '\u0003', '\u0002', '\u0002', '\u0002', '\u0094', '\t', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u0095', '\u0093', '\u0003', '\u0002', '\u0002', '\u0002', '\u0096', '\u0098',
			'\a', '\u0013', '\u0002', '\u0002', '\u0097', '\u0096', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u0097', '\u0098', '\u0003', '\u0002', '\u0002', '\u0002', '\u0098', '\u0099', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u0099', '\u009a', '\u0005', '\b', '\u0005', '\u0002', '\u009a', '\u009b',
			'\a', '\u0002', '\u0002', '\u0003', '\u009b', '\v', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u009c', '¡', '\u0005', '\b', '\u0005', '\u0002', '\u009d', '\u009e', '\a', '\u0016',
			'\u0002', '\u0002', '\u009e', '\u00a0', '\u0005', '\b', '\u0005', '\u0002', '\u009f', '\u009d',
			'\u0003', '\u0002', '\u0002', '\u0002', '\u00a0', '£', '\u0003', '\u0002', '\u0002', '\u0002',
			'¡', '\u009f', '\u0003', '\u0002', '\u0002', '\u0002', '¡', '¢', '\u0003', '\u0002',
			'\u0002', '\u0002', '¢', '\r', '\u0003', '\u0002', '\u0002', '\u0002', '£', '¡',
			'\u0003', '\u0002', '\u0002', '\u0002', '\n', '\u001d', ')', ',', 'y', '\u0091',
			'\u0093', '\u0097', '¡'
		};
		_ATN = new ATNDeserializer().Deserialize(_serializedATN);
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++)
		{
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

	public FormulaDisplayParser(ITokenStream input)
		: this(input, Console.Out, Console.Error)
	{
	}

	public FormulaDisplayParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	[RuleVersion(0)]
	public FuncNameContext funcName()
	{
		FuncNameContext funcNameContext = new FuncNameContext(Context, base.State);
		EnterRule(funcNameContext, 0, 0);
		try
		{
			EnterOuterAlt(funcNameContext, 1);
			base.State = 12;
			int num = base.TokenStream.LA(1);
			if (((uint)num & 0xFFFFFFC0u) != 0 || ((1L << num) & 0x58) == 0L)
			{
				ErrorHandler.RecoverInline(this);
			}
			else
			{
				ErrorHandler.ReportMatch(this);
				Consume();
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (funcNameContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return funcNameContext;
	}

	[RuleVersion(0)]
	public ColumnNameContext columnName()
	{
		ColumnNameContext columnNameContext = new ColumnNameContext(Context, base.State);
		EnterRule(columnNameContext, 2, 1);
		try
		{
			EnterOuterAlt(columnNameContext, 1);
			base.State = 14;
			int num = base.TokenStream.LA(1);
			if (num != 28 && num != 29)
			{
				ErrorHandler.RecoverInline(this);
			}
			else
			{
				ErrorHandler.ReportMatch(this);
				Consume();
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (columnNameContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return columnNameContext;
	}

	[RuleVersion(0)]
	public RelOpContext relOp()
	{
		RelOpContext relOpContext = new RelOpContext(Context, base.State);
		EnterRule(relOpContext, 4, 2);
		try
		{
			base.State = 27;
			ErrorHandler.Sync(this);
			switch (Interpreter.AdaptivePredict(base.TokenStream, 0, Context))
			{
			case 1:
				EnterOuterAlt(relOpContext, 1);
				base.State = 16;
				Match(15);
				break;
			case 2:
				EnterOuterAlt(relOpContext, 2);
				base.State = 17;
				Match(16);
				break;
			case 3:
				EnterOuterAlt(relOpContext, 3);
				base.State = 18;
				Match(17);
				break;
			case 4:
				EnterOuterAlt(relOpContext, 4);
				base.State = 19;
				Match(15);
				base.State = 20;
				Match(17);
				break;
			case 5:
				EnterOuterAlt(relOpContext, 5);
				base.State = 21;
				Match(16);
				base.State = 22;
				Match(17);
				break;
			case 6:
				EnterOuterAlt(relOpContext, 6);
				base.State = 23;
				Match(15);
				base.State = 24;
				Match(16);
				break;
			case 7:
				EnterOuterAlt(relOpContext, 7);
				base.State = 25;
				Match(3);
				break;
			case 8:
				EnterOuterAlt(relOpContext, 8);
				base.State = 26;
				Match(4);
				break;
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (relOpContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return relOpContext;
	}

	[RuleVersion(0)]
	public ExprContext expr()
	{
		return expr(0);
	}

	private ExprContext expr(int _p)
	{
		ParserRuleContext context = Context;
		int state = base.State;
		ExprContext exprContext = new ExprContext(Context, state);
		ExprContext exprContext2 = exprContext;
		int state2 = 6;
		EnterRecursionRule(exprContext, 6, 3, _p);
		try
		{
			EnterOuterAlt(exprContext, 1);
			base.State = 119;
			ErrorHandler.Sync(this);
			switch (Interpreter.AdaptivePredict(base.TokenStream, 3, Context))
			{
			case 1:
				exprContext = (ExprContext)(Context = new NumberContext(exprContext));
				exprContext2 = exprContext;
				base.State = 30;
				Match(1);
				break;
			case 2:
				exprContext = (ExprContext)(Context = new StringContext(exprContext));
				exprContext2 = exprContext;
				base.State = 31;
				Match(2);
				break;
			case 3:
			{
				exprContext = (ExprContext)(Context = new FuncContext(exprContext));
				exprContext2 = exprContext;
				base.State = 32;
				funcName();
				base.State = 33;
				Match(18);
				base.State = 42;
				ErrorHandler.Sync(this);
				int num = base.TokenStream.LA(1);
				if ((num & -64) == 0 && ((1L << num) & 0x64025E) != 0L)
				{
					base.State = 34;
					expr(0);
					base.State = 39;
					ErrorHandler.Sync(this);
					for (num = base.TokenStream.LA(1); num == 8; num = base.TokenStream.LA(1))
					{
						base.State = 35;
						Match(8);
						base.State = 36;
						expr(0);
						base.State = 41;
						ErrorHandler.Sync(this);
					}
				}
				base.State = 44;
				Match(19);
				break;
			}
			case 4:
				exprContext = (ExprContext)(Context = new TableCellContext(exprContext));
				exprContext2 = exprContext;
				base.State = 46;
				Match(21);
				base.State = 47;
				Match(23);
				base.State = 48;
				Match(24);
				base.State = 49;
				Match(22);
				base.State = 50;
				columnName();
				base.State = 51;
				Match(26);
				base.State = 52;
				Match(28);
				base.State = 53;
				Match(30);
				break;
			case 5:
				exprContext = (ExprContext)(Context = new ColumnCellContext(exprContext));
				exprContext2 = exprContext;
				base.State = 55;
				Match(22);
				base.State = 56;
				columnName();
				base.State = 57;
				Match(26);
				base.State = 58;
				Match(28);
				base.State = 59;
				Match(30);
				break;
			case 6:
				exprContext = (ExprContext)(Context = new TableColumnContext(exprContext));
				exprContext2 = exprContext;
				base.State = 61;
				Match(21);
				base.State = 62;
				Match(23);
				base.State = 63;
				Match(24);
				base.State = 64;
				Match(22);
				base.State = 65;
				columnName();
				base.State = 66;
				Match(30);
				break;
			case 7:
				exprContext = (ExprContext)(Context = new ColumnContext(exprContext));
				exprContext2 = exprContext;
				base.State = 68;
				Match(22);
				base.State = 69;
				columnName();
				base.State = 70;
				Match(30);
				break;
			case 8:
				exprContext = (ExprContext)(Context = new TableColumnWildcardContext(exprContext));
				exprContext2 = exprContext;
				base.State = 72;
				Match(21);
				base.State = 73;
				Match(23);
				base.State = 74;
				Match(24);
				base.State = 75;
				Match(22);
				base.State = 76;
				columnName();
				base.State = 77;
				Match(26);
				base.State = 78;
				Match(25);
				base.State = 79;
				Match(30);
				break;
			case 9:
				exprContext = (ExprContext)(Context = new ColumnWildcardContext(exprContext));
				exprContext2 = exprContext;
				base.State = 81;
				Match(22);
				base.State = 82;
				columnName();
				base.State = 83;
				Match(26);
				base.State = 84;
				Match(25);
				base.State = 85;
				Match(30);
				break;
			case 10:
				exprContext = (ExprContext)(Context = new TableRangeContext(exprContext));
				exprContext2 = exprContext;
				base.State = 87;
				Match(21);
				base.State = 88;
				Match(23);
				base.State = 89;
				Match(24);
				base.State = 90;
				Match(22);
				base.State = 91;
				columnName();
				base.State = 92;
				Match(26);
				base.State = 93;
				Match(28);
				base.State = 94;
				Match(27);
				base.State = 95;
				columnName();
				base.State = 96;
				Match(26);
				base.State = 97;
				Match(28);
				base.State = 98;
				Match(30);
				break;
			case 11:
				exprContext = (ExprContext)(Context = new RangeContext(exprContext));
				exprContext2 = exprContext;
				base.State = 100;
				Match(22);
				base.State = 101;
				columnName();
				base.State = 102;
				Match(26);
				base.State = 103;
				Match(28);
				base.State = 104;
				Match(27);
				base.State = 105;
				columnName();
				base.State = 106;
				Match(26);
				base.State = 107;
				Match(28);
				base.State = 108;
				Match(30);
				break;
			case 12:
				exprContext = (ExprContext)(Context = new TreeNodeContext(exprContext));
				exprContext2 = exprContext;
				base.State = 110;
				Match(21);
				base.State = 111;
				Match(23);
				base.State = 112;
				Match(24);
				break;
			case 13:
				exprContext = (ExprContext)(Context = new ParenContext(exprContext));
				exprContext2 = exprContext;
				base.State = 113;
				Match(18);
				base.State = 114;
				expr(0);
				base.State = 115;
				Match(19);
				break;
			case 14:
				exprContext = (ExprContext)(Context = new NegContext(exprContext));
				exprContext2 = exprContext;
				base.State = 117;
				Match(9);
				base.State = 118;
				expr(8);
				break;
			}
			Context.Stop = base.TokenStream.LT(-1);
			base.State = 145;
			ErrorHandler.Sync(this);
			int num2 = Interpreter.AdaptivePredict(base.TokenStream, 5, Context);
			while (true)
			{
				switch (num2)
				{
				case 1:
					if (ParseListeners != null)
					{
						TriggerExitRuleEvent();
					}
					exprContext2 = exprContext;
					base.State = 143;
					ErrorHandler.Sync(this);
					switch (Interpreter.AdaptivePredict(base.TokenStream, 4, Context))
					{
					case 1:
						exprContext = new PowerContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 3);
						base.State = 121;
						if (!Precpred(Context, 7))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 7)");
						}
						base.State = 122;
						Match(14);
						base.State = 123;
						expr(8);
						break;
					case 2:
						exprContext = new MulContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 3);
						base.State = 124;
						if (!Precpred(Context, 6))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 6)");
						}
						base.State = 125;
						Match(10);
						base.State = 126;
						expr(7);
						break;
					case 3:
						exprContext = new DivContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 3);
						base.State = 127;
						if (!Precpred(Context, 5))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 5)");
						}
						base.State = 128;
						Match(11);
						base.State = 129;
						expr(6);
						break;
					case 4:
						exprContext = new AddContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 3);
						base.State = 130;
						if (!Precpred(Context, 4))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 4)");
						}
						base.State = 131;
						Match(12);
						base.State = 132;
						expr(5);
						break;
					case 5:
						exprContext = new SubContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 3);
						base.State = 133;
						if (!Precpred(Context, 3))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 3)");
						}
						base.State = 134;
						Match(9);
						base.State = 135;
						expr(4);
						break;
					case 6:
						exprContext = new ConcatContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 3);
						base.State = 136;
						if (!Precpred(Context, 2))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 2)");
						}
						base.State = 137;
						Match(13);
						base.State = 138;
						expr(3);
						break;
					case 7:
						exprContext = new RelContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 3);
						base.State = 139;
						if (!Precpred(Context, 1))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 1)");
						}
						base.State = 140;
						relOp();
						base.State = 141;
						expr(2);
						break;
					}
					break;
				case 0:
				case 2:
					goto end_IL_0aa5;
				}
				base.State = 147;
				ErrorHandler.Sync(this);
				num2 = Interpreter.AdaptivePredict(base.TokenStream, 5, Context);
				continue;
				end_IL_0aa5:
				break;
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (exprContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			UnrollRecursionContexts(context);
		}
		return exprContext;
	}

	[RuleVersion(0)]
	public FormulaContext formula()
	{
		FormulaContext formulaContext = new FormulaContext(Context, base.State);
		EnterRule(formulaContext, 8, 4);
		try
		{
			EnterOuterAlt(formulaContext, 1);
			base.State = 149;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			if (num == 17)
			{
				base.State = 148;
				Match(17);
			}
			base.State = 151;
			expr(0);
			base.State = 152;
			Match(-1);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (formulaContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return formulaContext;
	}

	[RuleVersion(0)]
	public ExprsContext exprs()
	{
		ExprsContext exprsContext = new ExprsContext(Context, base.State);
		EnterRule(exprsContext, 10, 5);
		try
		{
			EnterOuterAlt(exprsContext, 1);
			base.State = 154;
			expr(0);
			base.State = 159;
			ErrorHandler.Sync(this);
			for (int num = base.TokenStream.LA(1); num == 20; num = base.TokenStream.LA(1))
			{
				base.State = 155;
				Match(20);
				base.State = 156;
				expr(0);
				base.State = 161;
				ErrorHandler.Sync(this);
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (exprsContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return exprsContext;
	}

	public override bool Sempred(RuleContext _localctx, int ruleIndex, int predIndex)
	{
		if (ruleIndex == 3)
		{
			return expr_sempred((ExprContext)_localctx, predIndex);
		}
		return true;
	}

	private bool expr_sempred(ExprContext _localctx, int predIndex)
	{
		return predIndex switch
		{
			0 => Precpred(Context, 7), 
			1 => Precpred(Context, 6), 
			2 => Precpred(Context, 5), 
			3 => Precpred(Context, 4), 
			4 => Precpred(Context, 3), 
			5 => Precpred(Context, 2), 
			6 => Precpred(Context, 1), 
			_ => true, 
		};
	}
}
