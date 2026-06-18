using System.IO;
using Antlr4.Runtime;

namespace Leqisoft.Model;

public class LexerErrorListener : IAntlrErrorListener<int>
{
	public static LexerErrorListener Instance { get; } = new LexerErrorListener();


	private LexerErrorListener()
	{
	}

	public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
	{
		throw new FormulaSyntaxException($"在位置 {charPositionInLine} 处无法识别符号“{recognizer.InputStream.ToString()[charPositionInLine]}”", charPositionInLine);
	}
}
