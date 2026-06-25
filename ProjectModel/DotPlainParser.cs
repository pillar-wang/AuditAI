using System;
using System.CodeDom.Compiler;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.7.2")]
public class DotPlainParser : Parser
{
	public class LineContext : ParserRuleContext
	{
		public override int RuleIndex => 0;

		public ITerminalNode NL()
		{
			return GetToken(6, 0);
		}

		public ITerminalNode GRAPH()
		{
			return GetToken(1, 0);
		}

		public ITerminalNode NODE()
		{
			return GetToken(2, 0);
		}

		public ITerminalNode EDGE()
		{
			return GetToken(3, 0);
		}

		public ITerminalNode[] SP()
		{
			return GetTokens(5);
		}

		public ITerminalNode SP(int i)
		{
			return GetToken(5, i);
		}

		public ITerminalNode[] FIELD()
		{
			return GetTokens(7);
		}

		public ITerminalNode FIELD(int i)
		{
			return GetToken(7, i);
		}

		public LineContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IDotPlainListener dotPlainListener)
			{
				dotPlainListener.EnterLine(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IDotPlainListener dotPlainListener)
			{
				dotPlainListener.ExitLine(this);
			}
		}
	}

	public class FileContext : ParserRuleContext
	{
		public override int RuleIndex => 1;

		public ITerminalNode STOP()
		{
			return GetToken(4, 0);
		}

		public ITerminalNode NL()
		{
			return GetToken(6, 0);
		}

		public LineContext[] line()
		{
			return GetRuleContexts<LineContext>();
		}

		public LineContext line(int i)
		{
			return GetRuleContext<LineContext>(i);
		}

		public FileContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IDotPlainListener dotPlainListener)
			{
				dotPlainListener.EnterFile(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IDotPlainListener dotPlainListener)
			{
				dotPlainListener.ExitFile(this);
			}
		}
	}

	protected static DFA[] decisionToDFA;

	protected static PredictionContextCache sharedContextCache;

	public const int GRAPH = 1;

	public const int NODE = 2;

	public const int EDGE = 3;

	public const int STOP = 4;

	public const int SP = 5;

	public const int NL = 6;

	public const int FIELD = 7;

	public const int RULE_line = 0;

	public const int RULE_file = 1;

	public static readonly string[] ruleNames;

	private static readonly string[] _LiteralNames;

	private static readonly string[] _SymbolicNames;

	public static readonly IVocabulary DefaultVocabulary;

	private static char[] _serializedATN;

	public static readonly ATN _ATN;

	[NotNull]
	public override IVocabulary Vocabulary => DefaultVocabulary;

	public override string GrammarFileName => "DotPlain.g4";

	public override string[] RuleNames => ruleNames;

	public override string SerializedAtn => new string(_serializedATN);

	static DotPlainParser()
	{
		sharedContextCache = new PredictionContextCache();
		ruleNames = new string[2] { "line", "file" };
		_LiteralNames = new string[7] { null, "'graph'", "'node'", "'edge'", "'stop'", "' '", "'\r\n'" };
		_SymbolicNames = new string[8] { null, "GRAPH", "NODE", "EDGE", "STOP", "SP", "NL", "FIELD" };
		DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);
		_serializedATN = new char[210]
		{
			'\u0003', '悋', 'Ꜫ', '脳', '맭', '䅼', '㯧', '瞆', '奤', '\u0003',
			'\t', '\u0019', '\u0004', '\u0002', '\t', '\u0002', '\u0004', '\u0003', '\t', '\u0003',
			'\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\a', '\u0002', '\n', '\n',
			'\u0002', '\f', '\u0002', '\u000e', '\u0002', '\r', '\v', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0002', '\u0003', '\u0003', '\u0006', '\u0003', '\u0012', '\n', '\u0003', '\r',
			'\u0003', '\u000e', '\u0003', '\u0013', '\u0003', '\u0003', '\u0003', '\u0003', '\u0003', '\u0003',
			'\u0003', '\u0003', '\u0002', '\u0002', '\u0004', '\u0002', '\u0004', '\u0002', '\u0003', '\u0003',
			'\u0002', '\u0003', '\u0005', '\u0002', '\u0018', '\u0002', '\u0006', '\u0003', '\u0002', '\u0002',
			'\u0002', '\u0004', '\u0011', '\u0003', '\u0002', '\u0002', '\u0002', '\u0006', '\v', '\t',
			'\u0002', '\u0002', '\u0002', '\a', '\b', '\a', '\a', '\u0002', '\u0002', '\b',
			'\n', '\a', '\t', '\u0002', '\u0002', '\t', '\a', '\u0003', '\u0002', '\u0002',
			'\u0002', '\n', '\r', '\u0003', '\u0002', '\u0002', '\u0002', '\v', '\t', '\u0003',
			'\u0002', '\u0002', '\u0002', '\v', '\f', '\u0003', '\u0002', '\u0002', '\u0002', '\f',
			'\u000e', '\u0003', '\u0002', '\u0002', '\u0002', '\r', '\v', '\u0003', '\u0002', '\u0002',
			'\u0002', '\u000e', '\u000f', '\a', '\b', '\u0002', '\u0002', '\u000f', '\u0003', '\u0003',
			'\u0002', '\u0002', '\u0002', '\u0010', '\u0012', '\u0005', '\u0002', '\u0002', '\u0002', '\u0011',
			'\u0010', '\u0003', '\u0002', '\u0002', '\u0002', '\u0012', '\u0013', '\u0003', '\u0002', '\u0002',
			'\u0002', '\u0013', '\u0011', '\u0003', '\u0002', '\u0002', '\u0002', '\u0013', '\u0014', '\u0003',
			'\u0002', '\u0002', '\u0002', '\u0014', '\u0015', '\u0003', '\u0002', '\u0002', '\u0002', '\u0015',
			'\u0016', '\a', '\u0006', '\u0002', '\u0002', '\u0016', '\u0017', '\a', '\b', '\u0002',
			'\u0002', '\u0017', '\u0005', '\u0003', '\u0002', '\u0002', '\u0002', '\u0004', '\v', '\u0013'
		};
		_ATN = new ATNDeserializer().Deserialize(_serializedATN);
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++)
		{
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

	public DotPlainParser(ITokenStream input)
		: this(input, Console.Out, Console.Error)
	{
	}

	public DotPlainParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	[RuleVersion(0)]
	public LineContext line()
	{
		LineContext lineContext = new LineContext(Context, base.State);
		EnterRule(lineContext, 0, 0);
		try
		{
			EnterOuterAlt(lineContext, 1);
			base.State = 4;
			int num = base.TokenStream.LA(1);
			if (((uint)num & 0xFFFFFFC0u) != 0 || ((1L << num) & 0xE) == 0L)
			{
				ErrorHandler.RecoverInline(this);
			}
			else
			{
				ErrorHandler.ReportMatch(this);
				Consume();
			}
			base.State = 9;
			ErrorHandler.Sync(this);
			for (num = base.TokenStream.LA(1); num == 5; num = base.TokenStream.LA(1))
			{
				base.State = 5;
				Match(5);
				base.State = 6;
				Match(7);
				base.State = 11;
				ErrorHandler.Sync(this);
			}
			base.State = 12;
			Match(6);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (lineContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return lineContext;
	}

	[RuleVersion(0)]
	public FileContext file()
	{
		FileContext fileContext = new FileContext(Context, base.State);
		EnterRule(fileContext, 2, 1);
		try
		{
			EnterOuterAlt(fileContext, 1);
			base.State = 15;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			do
			{
				base.State = 14;
				line();
				base.State = 17;
				ErrorHandler.Sync(this);
				num = base.TokenStream.LA(1);
			}
			while ((num & -64) == 0 && ((1L << num) & 0xE) != 0L);
			base.State = 19;
			Match(4);
			base.State = 20;
			Match(6);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (fileContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return fileContext;
	}
}
