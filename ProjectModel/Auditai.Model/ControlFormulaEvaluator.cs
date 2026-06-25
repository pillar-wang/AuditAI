using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Auditai.DTO;

namespace Auditai.Model;

public class ControlFormulaEvaluator
{
	private readonly CommonTokenStream _cts;

	private readonly FormulaParser.ExprsContext _tree;

	public FormulaEvaluationEnvironment Env { get; set; }

	public ControlFormulaEvaluator(string text)
	{
		ICharStream input = CharStreams.fromstring(text);
		FormulaLexer formulaLexer = new FormulaLexer(input);
		formulaLexer.AddErrorListener(LexerErrorListener.Instance);
		_cts = new CommonTokenStream(formulaLexer);
		FormulaParser formulaParser = new FormulaParser(_cts)
		{
			ErrorHandler = new BailErrorStrategy()
		};
		try
		{
			_tree = formulaParser.exprs();
		}
		catch (ParseCanceledException)
		{
			throw new FormulaSyntaxException("", 0);
		}
	}

	public void Evaluate()
	{
		FormulaEvaluationVisitor formulaEvaluationVisitor = new FormulaEvaluationVisitor(Env);
		FormulaParser.ExprContext[] array = _tree.expr();
		foreach (FormulaParser.ExprContext tree in array)
		{
			formulaEvaluationVisitor.Visit(tree);
		}
	}

	public string GetDisplayString(FormulaReferenceResolver resolver, Table contextTable = null)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		DisplayRewriter listener = new DisplayRewriter(tokenStreamRewriter, resolver, contextTable);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}

	public string DuplicateTableRewrite(FormulaReferenceResolver resolver, Dictionary<Id64, Table> dic, bool transProject)
	{
		TokenStreamRewriter tokenStreamRewriter = new TokenStreamRewriter(_cts);
		DuplicateTableRewriter listener = new DuplicateTableRewriter(tokenStreamRewriter, resolver, dic, transProject);
		ParseTreeWalker.Default.Walk(listener, _tree);
		return tokenStreamRewriter.GetText();
	}
}
