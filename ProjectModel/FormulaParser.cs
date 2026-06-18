using System;
using System.CodeDom.Compiler;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.7.2")]
public class FormulaParser : Parser
{
	public class ExprContext : ParserRuleContext
	{
		public override int RuleIndex => 0;

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

	public class RefHeaderCellWildcardContext : ExprContext
	{
		public ITerminalNode[] Int()
		{
			return GetTokens(21);
		}

		public ITerminalNode Int(int i)
		{
			return GetToken(21, i);
		}

		public RefHeaderCellWildcardContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterRefHeaderCellWildcard(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitRefHeaderCellWildcard(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitRefHeaderCellWildcard(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class StringContext : ExprContext
	{
		public ITerminalNode String()
		{
			return GetToken(22, 0);
		}

		public StringContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterString(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitString(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitString(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class RefCellContext : ExprContext
	{
		public ITerminalNode[] Int()
		{
			return GetTokens(21);
		}

		public ITerminalNode Int(int i)
		{
			return GetToken(21, i);
		}

		public RefCellContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterRefCell(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitRefCell(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitRefCell(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class LtContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public LtContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterLt(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitLt(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitLt(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class AddsubContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode PLUS()
		{
			return GetToken(27, 0);
		}

		public ITerminalNode MINUS()
		{
			return GetToken(28, 0);
		}

		public AddsubContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterAddsub(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitAddsub(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitAddsub(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class RefTicketRangeContext : ExprContext
	{
		public ITerminalNode[] Int()
		{
			return GetTokens(21);
		}

		public ITerminalNode Int(int i)
		{
			return GetToken(21, i);
		}

		public RefTicketRangeContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterRefTicketRange(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitRefTicketRange(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitRefTicketRange(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class FloatContext : ExprContext
	{
		public ITerminalNode[] Int()
		{
			return GetTokens(21);
		}

		public ITerminalNode Int(int i)
		{
			return GetToken(21, i);
		}

		public FloatContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterFloat(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitFloat(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitFloat(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class RefColumnWildcardContext : ExprContext
	{
		public ITerminalNode[] Int()
		{
			return GetTokens(21);
		}

		public ITerminalNode Int(int i)
		{
			return GetToken(21, i);
		}

		public RefColumnWildcardContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterRefColumnWildcard(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitRefColumnWildcard(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitRefColumnWildcard(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class NegContext : ExprContext
	{
		public ITerminalNode MINUS()
		{
			return GetToken(28, 0);
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
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterNeg(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitNeg(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitNeg(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class ParenContext : ExprContext
	{
		public ExprContext expr()
		{
			return GetRuleContext<ExprContext>(0);
		}

		public ITerminalNode RPAREN()
		{
			return GetToken(34, 0);
		}

		public ParenContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterParen(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitParen(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitParen(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class AndContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode AND()
		{
			return GetToken(23, 0);
		}

		public AndContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterAnd(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitAnd(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitAnd(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class GteContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public GteContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterGte(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitGte(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitGte(this);
			}
			return visitor.VisitChildren(this);
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

		public ITerminalNode HAT()
		{
			return GetToken(32, 0);
		}

		public PowerContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterPower(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitPower(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitPower(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class LteContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public LteContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterLte(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitLte(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitLte(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class OrContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode OR()
		{
			return GetToken(24, 0);
		}

		public OrContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterOr(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitOr(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitOr(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class RefTicketCellContext : ExprContext
	{
		public ITerminalNode[] Int()
		{
			return GetTokens(21);
		}

		public ITerminalNode Int(int i)
		{
			return GetToken(21, i);
		}

		public RefTicketCellContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterRefTicketCell(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitRefTicketCell(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitRefTicketCell(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class RefTicketColumnContext : ExprContext
	{
		public ITerminalNode Int()
		{
			return GetToken(21, 0);
		}

		public RefTicketColumnContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterRefTicketColumn(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitRefTicketColumn(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitRefTicketColumn(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class RefColumnContext : ExprContext
	{
		public ITerminalNode[] Int()
		{
			return GetTokens(21);
		}

		public ITerminalNode Int(int i)
		{
			return GetToken(21, i);
		}

		public RefColumnContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterRefColumn(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitRefColumn(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitRefColumn(this);
			}
			return visitor.VisitChildren(this);
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

		public ConcatContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterConcat(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitConcat(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitConcat(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class EqContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode EQ()
		{
			return GetToken(33, 0);
		}

		public EqContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterEq(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitEq(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitEq(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class GtContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public GtContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterGt(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitGt(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitGt(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class IntContext : ExprContext
	{
		public ITerminalNode Int()
		{
			return GetToken(21, 0);
		}

		public IntContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterInt(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitInt(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitInt(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class MuldivContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode MULTIPLY()
		{
			return GetToken(29, 0);
		}

		public ITerminalNode DIVIDE()
		{
			return GetToken(30, 0);
		}

		public MuldivContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterMuldiv(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitMuldiv(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitMuldiv(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class RefRangeContext : ExprContext
	{
		public ITerminalNode[] Int()
		{
			return GetTokens(21);
		}

		public ITerminalNode Int(int i)
		{
			return GetToken(21, i);
		}

		public RefRangeContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterRefRange(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitRefRange(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitRefRange(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class FuncContext : ExprContext
	{
		public ITerminalNode RPAREN()
		{
			return GetToken(34, 0);
		}

		public ITerminalNode FuncName()
		{
			return GetToken(25, 0);
		}

		public ITerminalNode AND()
		{
			return GetToken(23, 0);
		}

		public ITerminalNode OR()
		{
			return GetToken(24, 0);
		}

		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode[] COMMA()
		{
			return GetTokens(31);
		}

		public ITerminalNode COMMA(int i)
		{
			return GetToken(31, i);
		}

		public FuncContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterFunc(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitFunc(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitFunc(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class RefHeaderCellContext : ExprContext
	{
		public ITerminalNode[] Int()
		{
			return GetTokens(21);
		}

		public ITerminalNode Int(int i)
		{
			return GetToken(21, i);
		}

		public RefHeaderCellContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterRefHeaderCell(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitRefHeaderCell(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitRefHeaderCell(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class NeContext : ExprContext
	{
		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public NeContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterNe(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitNe(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitNe(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class RefTreeNodeContext : ExprContext
	{
		public ITerminalNode Int()
		{
			return GetToken(21, 0);
		}

		public RefTreeNodeContext(ExprContext context)
		{
			CopyFrom(context);
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterRefTreeNode(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitRefTreeNode(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitRefTreeNode(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class FormulaContext : ParserRuleContext
	{
		public override int RuleIndex => 1;

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
			return GetToken(33, 0);
		}

		public FormulaContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterFormula(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitFormula(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitFormula(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	public class ExprsContext : ParserRuleContext
	{
		public override int RuleIndex => 2;

		public ExprContext[] expr()
		{
			return GetRuleContexts<ExprContext>();
		}

		public ExprContext expr(int i)
		{
			return GetRuleContext<ExprContext>(i);
		}

		public ITerminalNode[] SEMICOLON()
		{
			return GetTokens(35);
		}

		public ITerminalNode SEMICOLON(int i)
		{
			return GetToken(35, i);
		}

		public ExprsContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.EnterExprs(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IFormulaListener formulaListener)
			{
				formulaListener.ExitExprs(this);
			}
		}

		public override TResult Accept<TResult>(IParseTreeVisitor<TResult> visitor)
		{
			if (visitor is IFormulaVisitor<TResult> formulaVisitor)
			{
				return formulaVisitor.VisitExprs(this);
			}
			return visitor.VisitChildren(this);
		}
	}

	protected static DFA[] decisionToDFA;

	protected static PredictionContextCache sharedContextCache;

	public const int T__0 = 1;

	public const int T__1 = 2;

	public const int T__2 = 3;

	public const int T__3 = 4;

	public const int T__4 = 5;

	public const int T__5 = 6;

	public const int T__6 = 7;

	public const int T__7 = 8;

	public const int T__8 = 9;

	public const int T__9 = 10;

	public const int T__10 = 11;

	public const int T__11 = 12;

	public const int T__12 = 13;

	public const int T__13 = 14;

	public const int T__14 = 15;

	public const int T__15 = 16;

	public const int T__16 = 17;

	public const int T__17 = 18;

	public const int T__18 = 19;

	public const int T__19 = 20;

	public const int Int = 21;

	public const int String = 22;

	public const int AND = 23;

	public const int OR = 24;

	public const int FuncName = 25;

	public const int WS = 26;

	public const int PLUS = 27;

	public const int MINUS = 28;

	public const int MULTIPLY = 29;

	public const int DIVIDE = 30;

	public const int COMMA = 31;

	public const int HAT = 32;

	public const int EQ = 33;

	public const int RPAREN = 34;

	public const int SEMICOLON = 35;

	public const int RULE_expr = 0;

	public const int RULE_formula = 1;

	public const int RULE_exprs = 2;

	public static readonly string[] ruleNames;

	private static readonly string[] _LiteralNames;

	private static readonly string[] _SymbolicNames;

	public static readonly IVocabulary DefaultVocabulary;

	private static char[] _serializedATN;

	public static readonly ATN _ATN;

	[NotNull]
	public override IVocabulary Vocabulary => DefaultVocabulary;

	public override string GrammarFileName => "Formula.g4";

	public override string[] RuleNames => ruleNames;

	public override string SerializedAtn => new string(_serializedATN);

	static FormulaParser()
	{
		sharedContextCache = new PredictionContextCache();
		ruleNames = new string[3] { "expr", "formula", "exprs" };
		_LiteralNames = new string[36]
		{
			null, "'.'", "'('", "'[1:'", "':'", "']'", "'[2:'", "'[3:'", "'[4:'", "'[5:'",
			"'[6:'", "'[7:'", "'[8:'", "'[9:'", "'[10:'", "'&'", "'<'", "'>'", "'<='", "'>='",
			"'<>'", null, null, null, null, null, null, "'+'", "'-'", "'*'",
			"'/'", "','", "'^'", null, "')'", "';'"
		};
		_SymbolicNames = new string[36]
		{
			null, null, null, null, null, null, null, null, null, null,
			null, null, null, null, null, null, null, null, null, null,
			null, "Int", "String", "AND", "OR", "FuncName", "WS", "PLUS", "MINUS", "MULTIPLY",
			"DIVIDE", "COMMA", "HAT", "EQ", "RPAREN", "SEMICOLON"
		};
		DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);
		_serializedATN = new char[1342]
		{
			'\u0003', '悋', 'Ꜫ', '脳', '맭', '䅼', '㯧', '瞆', '奤', '\u0003',
			'%', '\u008f', '\u0004', '\u0002', '\t', '\u0002', '\u0004', '\u0003', '\t', '\u0003',
			'\u0004', '\u0004', '\t', '\u0004', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\a', '\u0002', '\u0014', '\n',
			'\u0002', '\f', '\u0002', '\u000e', '\u0002', '\u0017', '\v', '\u0002', '\u0005', '\u0002',
			'\u0019', '\n', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003',
			'\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003',
			'\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003',
			'\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003',
			'\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003',
			'\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003',
			'\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003',
			'\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003',
			'\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003',
			'\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003',
			'\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003',
			'\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003',
			'\u0002', '\u0005', '\u0002', 'V', '\n', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\a', '\u0002',
			'|', '\n', '\u0002', '\f', '\u0002', '\u000e', '\u0002', '\u007f', '\v', '\u0002',
			'\u0003', '\u0003', '\u0005', '\u0003', '\u0082', '\n', '\u0003', '\u0003', '\u0003', '\u0003',
			'\u0003', '\u0003', '\u0003', '\u0003', '\u0004', '\u0003', '\u0004', '\u0003', '\u0004', '\a',
			'\u0004', '\u008a', '\n', '\u0004', '\f', '\u0004', '\u000e', '\u0004', '\u008d', '\v',
			'\u0004', '\u0003', '\u0004', '\u0002', '\u0003', '\u0002', '\u0005', '\u0002', '\u0004', '\u0006',
			'\u0002', '\u0005', '\u0003', '\u0002', '\u0019', '\u001b', '\u0003', '\u0002', '\u001f', ' ',
			'\u0003', '\u0002', '\u001d', '\u001e', '\u0002', 'ª', '\u0002', 'U', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u0004', '\u0081', '\u0003', '\u0002', '\u0002', '\u0002', '\u0006', '\u0086',
			'\u0003', '\u0002', '\u0002', '\u0002', '\b', '\t', '\b', '\u0002', '\u0001', '\u0002',
			'\t', 'V', '\a', '\u0017', '\u0002', '\u0002', '\n', '\v', '\a', '\u0017',
			'\u0002', '\u0002', '\v', '\f', '\a', '\u0003', '\u0002', '\u0002', '\f', 'V',
			'\a', '\u0017', '\u0002', '\u0002', '\r', 'V', '\a', '\u0018', '\u0002', '\u0002',
			'\u000e', '\u000f', '\t', '\u0002', '\u0002', '\u0002', '\u000f', '\u0018', '\a', '\u0004',
			'\u0002', '\u0002', '\u0010', '\u0015', '\u0005', '\u0002', '\u0002', '\u0002', '\u0011', '\u0012',
			'\a', '!', '\u0002', '\u0002', '\u0012', '\u0014', '\u0005', '\u0002', '\u0002', '\u0002',
			'\u0013', '\u0011', '\u0003', '\u0002', '\u0002', '\u0002', '\u0014', '\u0017', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u0015', '\u0013', '\u0003', '\u0002', '\u0002', '\u0002', '\u0015', '\u0016',
			'\u0003', '\u0002', '\u0002', '\u0002', '\u0016', '\u0019', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u0017', '\u0015', '\u0003', '\u0002', '\u0002', '\u0002', '\u0018', '\u0010', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u0018', '\u0019', '\u0003', '\u0002', '\u0002', '\u0002', '\u0019', '\u001a',
			'\u0003', '\u0002', '\u0002', '\u0002', '\u001a', 'V', '\a', '$', '\u0002', '\u0002',
			'\u001b', '\u001c', '\a', '\u0005', '\u0002', '\u0002', '\u001c', '\u001d', '\a', '\u0017',
			'\u0002', '\u0002', '\u001d', '\u001e', '\a', '\u0006', '\u0002', '\u0002', '\u001e', '\u001f',
			'\a', '\u0017', '\u0002', '\u0002', '\u001f', 'V', '\a', '\a', '\u0002', '\u0002',
			' ', '!', '\a', '\b', '\u0002', '\u0002', '!', '"', '\a', '\u0017',
			'\u0002', '\u0002', '"', '#', '\a', '\u0006', '\u0002', '\u0002', '#', '$',
			'\a', '\u0017', '\u0002', '\u0002', '$', 'V', '\a', '\a', '\u0002', '\u0002',
			'%', '&', '\a', '\t', '\u0002', '\u0002', '&', '\'', '\a', '\u0017',
			'\u0002', '\u0002', '\'', '(', '\a', '\u0006', '\u0002', '\u0002', '(', ')',
			'\a', '\u0017', '\u0002', '\u0002', ')', '*', '\a', '\u0006', '\u0002', '\u0002',
			'*', '+', '\a', '\u0017', '\u0002', '\u0002', '+', 'V', '\a', '\a',
			'\u0002', '\u0002', ',', '-', '\a', '\n', '\u0002', '\u0002', '-', '.',
			'\a', '\u0017', '\u0002', '\u0002', '.', '/', '\a', '\u0006', '\u0002', '\u0002',
			'/', '0', '\a', '\u0017', '\u0002', '\u0002', '0', 'V', '\a', '\a',
			'\u0002', '\u0002', '1', '2', '\a', '\v', '\u0002', '\u0002', '2', '3',
			'\a', '\u0017', '\u0002', '\u0002', '3', 'V', '\a', '\a', '\u0002', '\u0002',
			'4', '5', '\a', '\f', '\u0002', '\u0002', '5', '6', '\a', '\u0017',
			'\u0002', '\u0002', '6', '7', '\a', '\u0006', '\u0002', '\u0002', '7', '8',
			'\a', '\u0017', '\u0002', '\u0002', '8', 'V', '\a', '\a', '\u0002', '\u0002',
			'9', ':', '\a', '\r', '\u0002', '\u0002', ':', ';', '\a', '\u0017',
			'\u0002', '\u0002', ';', '<', '\a', '\u0006', '\u0002', '\u0002', '<', '=',
			'\a', '\u0017', '\u0002', '\u0002', '=', 'V', '\a', '\a', '\u0002', '\u0002',
			'>', '?', '\a', '\u000e', '\u0002', '\u0002', '?', '@', '\a', '\u0017',
			'\u0002', '\u0002', '@', 'A', '\a', '\u0006', '\u0002', '\u0002', 'A', 'B',
			'\a', '\u0017', '\u0002', '\u0002', 'B', 'V', '\a', '\a', '\u0002', '\u0002',
			'C', 'D', '\a', '\u000f', '\u0002', '\u0002', 'D', 'E', '\a', '\u0017',
			'\u0002', '\u0002', 'E', 'F', '\a', '\u0006', '\u0002', '\u0002', 'F', 'G',
			'\a', '\u0017', '\u0002', '\u0002', 'G', 'H', '\a', '\u0006', '\u0002', '\u0002',
			'H', 'I', '\a', '\u0017', '\u0002', '\u0002', 'I', 'J', '\a', '\u0006',
			'\u0002', '\u0002', 'J', 'K', '\a', '\u0017', '\u0002', '\u0002', 'K', 'V',
			'\a', '\a', '\u0002', '\u0002', 'L', 'M', '\a', '\u0010', '\u0002', '\u0002',
			'M', 'N', '\a', '\u0017', '\u0002', '\u0002', 'N', 'V', '\a', '\a',
			'\u0002', '\u0002', 'O', 'P', '\a', '\u0004', '\u0002', '\u0002', 'P', 'Q',
			'\u0005', '\u0002', '\u0002', '\u0002', 'Q', 'R', '\a', '$', '\u0002', '\u0002',
			'R', 'V', '\u0003', '\u0002', '\u0002', '\u0002', 'S', 'T', '\a', '\u001e',
			'\u0002', '\u0002', 'T', 'V', '\u0005', '\u0002', '\u0002', '\u000f', 'U', '\b',
			'\u0003', '\u0002', '\u0002', '\u0002', 'U', '\n', '\u0003', '\u0002', '\u0002', '\u0002',
			'U', '\r', '\u0003', '\u0002', '\u0002', '\u0002', 'U', '\u000e', '\u0003', '\u0002',
			'\u0002', '\u0002', 'U', '\u001b', '\u0003', '\u0002', '\u0002', '\u0002', 'U', ' ',
			'\u0003', '\u0002', '\u0002', '\u0002', 'U', '%', '\u0003', '\u0002', '\u0002', '\u0002',
			'U', ',', '\u0003', '\u0002', '\u0002', '\u0002', 'U', '1', '\u0003', '\u0002',
			'\u0002', '\u0002', 'U', '4', '\u0003', '\u0002', '\u0002', '\u0002', 'U', '9',
			'\u0003', '\u0002', '\u0002', '\u0002', 'U', '>', '\u0003', '\u0002', '\u0002', '\u0002',
			'U', 'C', '\u0003', '\u0002', '\u0002', '\u0002', 'U', 'L', '\u0003', '\u0002',
			'\u0002', '\u0002', 'U', 'O', '\u0003', '\u0002', '\u0002', '\u0002', 'U', 'S',
			'\u0003', '\u0002', '\u0002', '\u0002', 'V', '}', '\u0003', '\u0002', '\u0002', '\u0002',
			'W', 'X', '\f', '\u000e', '\u0002', '\u0002', 'X', 'Y', '\a', '"',
			'\u0002', '\u0002', 'Y', '|', '\u0005', '\u0002', '\u0002', '\u000f', 'Z', '[',
			'\f', '\r', '\u0002', '\u0002', '[', '\\', '\t', '\u0003', '\u0002', '\u0002',
			'\\', '|', '\u0005', '\u0002', '\u0002', '\u000e', ']', '^', '\f', '\f',
			'\u0002', '\u0002', '^', '_', '\t', '\u0004', '\u0002', '\u0002', '_', '|',
			'\u0005', '\u0002', '\u0002', '\r', '`', 'a', '\f', '\v', '\u0002', '\u0002',
			'a', 'b', '\a', '\u0011', '\u0002', '\u0002', 'b', '|', '\u0005', '\u0002',
			'\u0002', '\f', 'c', 'd', '\f', '\n', '\u0002', '\u0002', 'd', 'e',
			'\a', '\u0012', '\u0002', '\u0002', 'e', '|', '\u0005', '\u0002', '\u0002', '\v',
			'f', 'g', '\f', '\t', '\u0002', '\u0002', 'g', 'h', '\a', '\u0013',
			'\u0002', '\u0002', 'h', '|', '\u0005', '\u0002', '\u0002', '\n', 'i', 'j',
			'\f', '\b', '\u0002', '\u0002', 'j', 'k', '\a', '#', '\u0002', '\u0002',
			'k', '|', '\u0005', '\u0002', '\u0002', '\t', 'l', 'm', '\f', '\a',
			'\u0002', '\u0002', 'm', 'n', '\a', '\u0014', '\u0002', '\u0002', 'n', '|',
			'\u0005', '\u0002', '\u0002', '\b', 'o', 'p', '\f', '\u0006', '\u0002', '\u0002',
			'p', 'q', '\a', '\u0015', '\u0002', '\u0002', 'q', '|', '\u0005', '\u0002',
			'\u0002', '\a', 'r', 's', '\f', '\u0005', '\u0002', '\u0002', 's', 't',
			'\a', '\u0016', '\u0002', '\u0002', 't', '|', '\u0005', '\u0002', '\u0002', '\u0006',
			'u', 'v', '\f', '\u0004', '\u0002', '\u0002', 'v', 'w', '\a', '\u0019',
			'\u0002', '\u0002', 'w', '|', '\u0005', '\u0002', '\u0002', '\u0005', 'x', 'y',
			'\f', '\u0003', '\u0002', '\u0002', 'y', 'z', '\a', '\u001a', '\u0002', '\u0002',
			'z', '|', '\u0005', '\u0002', '\u0002', '\u0004', '{', 'W', '\u0003', '\u0002',
			'\u0002', '\u0002', '{', 'Z', '\u0003', '\u0002', '\u0002', '\u0002', '{', ']',
			'\u0003', '\u0002', '\u0002', '\u0002', '{', '`', '\u0003', '\u0002', '\u0002', '\u0002',
			'{', 'c', '\u0003', '\u0002', '\u0002', '\u0002', '{', 'f', '\u0003', '\u0002',
			'\u0002', '\u0002', '{', 'i', '\u0003', '\u0002', '\u0002', '\u0002', '{', 'l',
			'\u0003', '\u0002', '\u0002', '\u0002', '{', 'o', '\u0003', '\u0002', '\u0002', '\u0002',
			'{', 'r', '\u0003', '\u0002', '\u0002', '\u0002', '{', 'u', '\u0003', '\u0002',
			'\u0002', '\u0002', '{', 'x', '\u0003', '\u0002', '\u0002', '\u0002', '|', '\u007f',
			'\u0003', '\u0002', '\u0002', '\u0002', '}', '{', '\u0003', '\u0002', '\u0002', '\u0002',
			'}', '~', '\u0003', '\u0002', '\u0002', '\u0002', '~', '\u0003', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u007f', '}', '\u0003', '\u0002', '\u0002', '\u0002', '\u0080', '\u0082',
			'\a', '#', '\u0002', '\u0002', '\u0081', '\u0080', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u0081', '\u0082', '\u0003', '\u0002', '\u0002', '\u0002', '\u0082', '\u0083', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u0083', '\u0084', '\u0005', '\u0002', '\u0002', '\u0002', '\u0084', '\u0085',
			'\a', '\u0002', '\u0002', '\u0003', '\u0085', '\u0005', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u0086', '\u008b', '\u0005', '\u0002', '\u0002', '\u0002', '\u0087', '\u0088', '\a', '%',
			'\u0002', '\u0002', '\u0088', '\u008a', '\u0005', '\u0002', '\u0002', '\u0002', '\u0089', '\u0087',
			'\u0003', '\u0002', '\u0002', '\u0002', '\u008a', '\u008d', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u008b', '\u0089', '\u0003', '\u0002', '\u0002', '\u0002', '\u008b', '\u008c', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u008c', '\a', '\u0003', '\u0002', '\u0002', '\u0002', '\u008d', '\u008b',
			'\u0003', '\u0002', '\u0002', '\u0002', '\t', '\u0015', '\u0018', 'U', '{', '}',
			'\u0081', '\u008b'
		};
		_ATN = new ATNDeserializer().Deserialize(_serializedATN);
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++)
		{
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

	public FormulaParser(ITokenStream input)
		: this(input, Console.Out, Console.Error)
	{
	}

	public FormulaParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
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
		int state2 = 0;
		EnterRecursionRule(exprContext, 0, 0, _p);
		try
		{
			EnterOuterAlt(exprContext, 1);
			base.State = 83;
			ErrorHandler.Sync(this);
			switch (Interpreter.AdaptivePredict(base.TokenStream, 2, Context))
			{
			case 1:
				exprContext = (ExprContext)(Context = new IntContext(exprContext));
				exprContext2 = exprContext;
				base.State = 7;
				Match(21);
				break;
			case 2:
				exprContext = (ExprContext)(Context = new FloatContext(exprContext));
				exprContext2 = exprContext;
				base.State = 8;
				Match(21);
				base.State = 9;
				Match(1);
				base.State = 10;
				Match(21);
				break;
			case 3:
				exprContext = (ExprContext)(Context = new StringContext(exprContext));
				exprContext2 = exprContext;
				base.State = 11;
				Match(22);
				break;
			case 4:
			{
				exprContext = (ExprContext)(Context = new FuncContext(exprContext));
				exprContext2 = exprContext;
				base.State = 12;
				int num = base.TokenStream.LA(1);
				if (((uint)num & 0xFFFFFFC0u) != 0 || ((1L << num) & 0x3800000) == 0L)
				{
					ErrorHandler.RecoverInline(this);
				}
				else
				{
					ErrorHandler.ReportMatch(this);
					Consume();
				}
				base.State = 13;
				Match(2);
				base.State = 22;
				ErrorHandler.Sync(this);
				num = base.TokenStream.LA(1);
				if ((num & -64) == 0 && ((1L << num) & 0x13E07FCC) != 0L)
				{
					base.State = 14;
					expr(0);
					base.State = 19;
					ErrorHandler.Sync(this);
					for (num = base.TokenStream.LA(1); num == 31; num = base.TokenStream.LA(1))
					{
						base.State = 15;
						Match(31);
						base.State = 16;
						expr(0);
						base.State = 21;
						ErrorHandler.Sync(this);
					}
				}
				base.State = 24;
				Match(34);
				break;
			}
			case 5:
				exprContext = (ExprContext)(Context = new RefCellContext(exprContext));
				exprContext2 = exprContext;
				base.State = 25;
				Match(3);
				base.State = 26;
				Match(21);
				base.State = 27;
				Match(4);
				base.State = 28;
				Match(21);
				base.State = 29;
				Match(5);
				break;
			case 6:
				exprContext = (ExprContext)(Context = new RefColumnContext(exprContext));
				exprContext2 = exprContext;
				base.State = 30;
				Match(6);
				base.State = 31;
				Match(21);
				base.State = 32;
				Match(4);
				base.State = 33;
				Match(21);
				base.State = 34;
				Match(5);
				break;
			case 7:
				exprContext = (ExprContext)(Context = new RefRangeContext(exprContext));
				exprContext2 = exprContext;
				base.State = 35;
				Match(7);
				base.State = 36;
				Match(21);
				base.State = 37;
				Match(4);
				base.State = 38;
				Match(21);
				base.State = 39;
				Match(4);
				base.State = 40;
				Match(21);
				base.State = 41;
				Match(5);
				break;
			case 8:
				exprContext = (ExprContext)(Context = new RefColumnWildcardContext(exprContext));
				exprContext2 = exprContext;
				base.State = 42;
				Match(8);
				base.State = 43;
				Match(21);
				base.State = 44;
				Match(4);
				base.State = 45;
				Match(21);
				base.State = 46;
				Match(5);
				break;
			case 9:
				exprContext = (ExprContext)(Context = new RefTreeNodeContext(exprContext));
				exprContext2 = exprContext;
				base.State = 47;
				Match(9);
				base.State = 48;
				Match(21);
				base.State = 49;
				Match(5);
				break;
			case 10:
				exprContext = (ExprContext)(Context = new RefHeaderCellContext(exprContext));
				exprContext2 = exprContext;
				base.State = 50;
				Match(10);
				base.State = 51;
				Match(21);
				base.State = 52;
				Match(4);
				base.State = 53;
				Match(21);
				base.State = 54;
				Match(5);
				break;
			case 11:
				exprContext = (ExprContext)(Context = new RefHeaderCellWildcardContext(exprContext));
				exprContext2 = exprContext;
				base.State = 55;
				Match(11);
				base.State = 56;
				Match(21);
				base.State = 57;
				Match(4);
				base.State = 58;
				Match(21);
				base.State = 59;
				Match(5);
				break;
			case 12:
				exprContext = (ExprContext)(Context = new RefTicketCellContext(exprContext));
				exprContext2 = exprContext;
				base.State = 60;
				Match(12);
				base.State = 61;
				Match(21);
				base.State = 62;
				Match(4);
				base.State = 63;
				Match(21);
				base.State = 64;
				Match(5);
				break;
			case 13:
				exprContext = (ExprContext)(Context = new RefTicketRangeContext(exprContext));
				exprContext2 = exprContext;
				base.State = 65;
				Match(13);
				base.State = 66;
				Match(21);
				base.State = 67;
				Match(4);
				base.State = 68;
				Match(21);
				base.State = 69;
				Match(4);
				base.State = 70;
				Match(21);
				base.State = 71;
				Match(4);
				base.State = 72;
				Match(21);
				base.State = 73;
				Match(5);
				break;
			case 14:
				exprContext = (ExprContext)(Context = new RefTicketColumnContext(exprContext));
				exprContext2 = exprContext;
				base.State = 74;
				Match(14);
				base.State = 75;
				Match(21);
				base.State = 76;
				Match(5);
				break;
			case 15:
				exprContext = (ExprContext)(Context = new ParenContext(exprContext));
				exprContext2 = exprContext;
				base.State = 77;
				Match(2);
				base.State = 78;
				expr(0);
				base.State = 79;
				Match(34);
				break;
			case 16:
				exprContext = (ExprContext)(Context = new NegContext(exprContext));
				exprContext2 = exprContext;
				base.State = 81;
				Match(28);
				base.State = 82;
				expr(13);
				break;
			}
			Context.Stop = base.TokenStream.LT(-1);
			base.State = 123;
			ErrorHandler.Sync(this);
			int num2 = Interpreter.AdaptivePredict(base.TokenStream, 4, Context);
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
					base.State = 121;
					ErrorHandler.Sync(this);
					switch (Interpreter.AdaptivePredict(base.TokenStream, 3, Context))
					{
					case 1:
						exprContext = new PowerContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 0);
						base.State = 85;
						if (!Precpred(Context, 12))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 12)");
						}
						base.State = 86;
						Match(32);
						base.State = 87;
						expr(13);
						break;
					case 2:
					{
						exprContext = new MuldivContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 0);
						base.State = 88;
						if (!Precpred(Context, 11))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 11)");
						}
						base.State = 89;
						int num = base.TokenStream.LA(1);
						if (num != 29 && num != 30)
						{
							ErrorHandler.RecoverInline(this);
						}
						else
						{
							ErrorHandler.ReportMatch(this);
							Consume();
						}
						base.State = 90;
						expr(12);
						break;
					}
					case 3:
					{
						exprContext = new AddsubContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 0);
						base.State = 91;
						if (!Precpred(Context, 10))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 10)");
						}
						base.State = 92;
						int num = base.TokenStream.LA(1);
						if (num != 27 && num != 28)
						{
							ErrorHandler.RecoverInline(this);
						}
						else
						{
							ErrorHandler.ReportMatch(this);
							Consume();
						}
						base.State = 93;
						expr(11);
						break;
					}
					case 4:
						exprContext = new ConcatContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 0);
						base.State = 94;
						if (!Precpred(Context, 9))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 9)");
						}
						base.State = 95;
						Match(15);
						base.State = 96;
						expr(10);
						break;
					case 5:
						exprContext = new LtContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 0);
						base.State = 97;
						if (!Precpred(Context, 8))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 8)");
						}
						base.State = 98;
						Match(16);
						base.State = 99;
						expr(9);
						break;
					case 6:
						exprContext = new GtContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 0);
						base.State = 100;
						if (!Precpred(Context, 7))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 7)");
						}
						base.State = 101;
						Match(17);
						base.State = 102;
						expr(8);
						break;
					case 7:
						exprContext = new EqContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 0);
						base.State = 103;
						if (!Precpred(Context, 6))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 6)");
						}
						base.State = 104;
						Match(33);
						base.State = 105;
						expr(7);
						break;
					case 8:
						exprContext = new LteContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 0);
						base.State = 106;
						if (!Precpred(Context, 5))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 5)");
						}
						base.State = 107;
						Match(18);
						base.State = 108;
						expr(6);
						break;
					case 9:
						exprContext = new GteContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 0);
						base.State = 109;
						if (!Precpred(Context, 4))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 4)");
						}
						base.State = 110;
						Match(19);
						base.State = 111;
						expr(5);
						break;
					case 10:
						exprContext = new NeContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 0);
						base.State = 112;
						if (!Precpred(Context, 3))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 3)");
						}
						base.State = 113;
						Match(20);
						base.State = 114;
						expr(4);
						break;
					case 11:
						exprContext = new AndContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 0);
						base.State = 115;
						if (!Precpred(Context, 2))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 2)");
						}
						base.State = 116;
						Match(23);
						base.State = 117;
						expr(3);
						break;
					case 12:
						exprContext = new OrContext(new ExprContext(context, state));
						PushNewRecursionContext(exprContext, state2, 0);
						base.State = 118;
						if (!Precpred(Context, 1))
						{
							throw new FailedPredicateException(this, "Precpred(Context, 1)");
						}
						base.State = 119;
						Match(24);
						base.State = 120;
						expr(2);
						break;
					}
					break;
				case 0:
				case 2:
					goto end_IL_0d01;
				}
				base.State = 125;
				ErrorHandler.Sync(this);
				num2 = Interpreter.AdaptivePredict(base.TokenStream, 4, Context);
				continue;
				end_IL_0d01:
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
		EnterRule(formulaContext, 2, 1);
		try
		{
			EnterOuterAlt(formulaContext, 1);
			base.State = 127;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			if (num == 33)
			{
				base.State = 126;
				Match(33);
			}
			base.State = 129;
			expr(0);
			base.State = 130;
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
		EnterRule(exprsContext, 4, 2);
		try
		{
			EnterOuterAlt(exprsContext, 1);
			base.State = 132;
			expr(0);
			base.State = 137;
			ErrorHandler.Sync(this);
			for (int num = base.TokenStream.LA(1); num == 35; num = base.TokenStream.LA(1))
			{
				base.State = 133;
				Match(35);
				base.State = 134;
				expr(0);
				base.State = 139;
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
		if (ruleIndex == 0)
		{
			return expr_sempred((ExprContext)_localctx, predIndex);
		}
		return true;
	}

	private bool expr_sempred(ExprContext _localctx, int predIndex)
	{
		return predIndex switch
		{
			0 => Precpred(Context, 12), 
			1 => Precpred(Context, 11), 
			2 => Precpred(Context, 10), 
			3 => Precpred(Context, 9), 
			4 => Precpred(Context, 8), 
			5 => Precpred(Context, 7), 
			6 => Precpred(Context, 6), 
			7 => Precpred(Context, 5), 
			8 => Precpred(Context, 4), 
			9 => Precpred(Context, 3), 
			10 => Precpred(Context, 2), 
			11 => Precpred(Context, 1), 
			_ => true, 
		};
	}
}
