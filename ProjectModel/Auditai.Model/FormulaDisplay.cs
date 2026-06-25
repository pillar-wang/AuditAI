using System;
using System.Collections.Generic;
using System.Drawing;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Auditai.Model;

public class FormulaDisplay
{
	private string _text;

	private CommonTokenStream _cts;

	private FormulaDisplayParser _parser;

	public FormulaDisplay(string text)
	{
		_text = text;
		ICharStream input = CharStreams.fromstring(_text);
		FormulaDisplayLexer formulaDisplayLexer = new FormulaDisplayLexer(input);
		formulaDisplayLexer.AddErrorListener(LexerErrorListener.Instance);
		formulaDisplayLexer.TokenFactory = HalfwidthTokenFactory.Instance;
		_cts = new CommonTokenStream(formulaDisplayLexer);
		_parser = new FormulaDisplayParser(_cts);
	}

	public string ToFormula(FormulaContext context)
	{
		_parser.ErrorHandler = FormulaDisplayErrorStrategy.Instance;
		FormulaDisplayParser.FormulaContext t = _parser.formula();
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		ToFormulaRewriter listener = new ToFormulaRewriter(tokenStreamRewriter, context);
		try
		{
			ParseTreeWalker.Default.Walk(listener, t);
		}
		catch (NullReferenceException)
		{
			throw new FormulaBadReferenceException();
		}
		return tokenStreamRewriter.GetText();
	}

	public string ToTicketFormula(FormulaContext context)
	{
		_parser.ErrorHandler = FormulaDisplayErrorStrategy.Instance;
		FormulaDisplayParser.FormulaContext t = _parser.formula();
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		ToTicketFormulaRewriter listener = new ToTicketFormulaRewriter(tokenStreamRewriter, context);
		try
		{
			ParseTreeWalker.Default.Walk(listener, t);
		}
		catch (NullReferenceException)
		{
			throw new FormulaBadReferenceException();
		}
		return tokenStreamRewriter.GetText();
	}

	public string ToControlFormula(FormulaContext context)
	{
		_parser.ErrorHandler = FormulaDisplayErrorStrategy.Instance;
		FormulaDisplayParser.ExprsContext t = _parser.exprs();
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		ToTicketFormulaRewriter listener = new ToTicketFormulaRewriter(tokenStreamRewriter, context);
		try
		{
			ParseTreeWalker.Default.Walk(listener, t);
		}
		catch (NullReferenceException)
		{
			throw new FormulaBadReferenceException();
		}
		return tokenStreamRewriter.GetText();
	}

	public Tuple<string, int> GetFuncNameAtPos(int pos)
	{
		FormulaDisplayParser.FormulaContext t = _parser.formula();
		GetFuncNameAtPosListener getFuncNameAtPosListener = new GetFuncNameAtPosListener(pos);
		ParseTreeWalker.Default.Walk(getFuncNameAtPosListener, t);
		return Tuple.Create(getFuncNameAtPosListener.FuncName, getFuncNameAtPosListener.NthParameter);
	}

	public List<Tuple<int, int, Color>> GetTokenColorIntervals()
	{
		List<Tuple<int, int, Color>> list = new List<Tuple<int, int, Color>>();
		_parser.exprs();
		_cts.Seek(0);
		for (int i = 0; i < _cts.Size; i++)
		{
			IToken token = _cts.Get(i);
			switch (token.Type)
			{
			case 3:
			case 4:
			case 6:
			case 8:
			case 18:
			case 19:
				list.Add(Tuple.Create(token.StartIndex, token.StopIndex - token.StartIndex + 1, GetTokenColor(token.Type)));
				break;
			}
		}
		return list;
	}

	private static Color GetTokenColor(int tokenType)
	{
		switch (tokenType)
		{
		case 3:
		case 4:
			return Color.Blue;
		case 6:
		case 8:
		case 18:
		case 19:
			return Color.Red;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public Tuple<int, int> GetRefAtPos(int pos)
	{
		FormulaDisplayParser.ExprsContext t2 = _parser.exprs();
		GetRefAtPosListener getRefAtPosListener = new GetRefAtPosListener();
		ParseTreeWalker.Default.Walk(getRefAtPosListener, t2);
		return getRefAtPosListener.RefIntervals.Find((Tuple<int, int> t) => t.Item1 + 1 <= pos && pos <= t.Item2 + 1);
	}

	public Tuple<List<FormulaDisplayRef>, Color> GetReferences(FormulaContext context)
	{
		FormulaDisplayParser.ExprsContext t = _parser.exprs();
		GetRefIntervalsListener getRefIntervalsListener = new GetRefIntervalsListener(context);
		try
		{
			ParseTreeWalker.Default.Walk(getRefIntervalsListener, t);
		}
		catch
		{
		}
		return Tuple.Create(getRefIntervalsListener.Refs, getRefIntervalsListener.GetNextColor());
	}
}
