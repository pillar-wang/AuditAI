using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.7.2")]
public interface IDotPlainListener : IParseTreeListener
{
	void EnterLine([NotNull] DotPlainParser.LineContext context);

	void ExitLine([NotNull] DotPlainParser.LineContext context);

	void EnterFile([NotNull] DotPlainParser.FileContext context);

	void ExitFile([NotNull] DotPlainParser.FileContext context);
}
