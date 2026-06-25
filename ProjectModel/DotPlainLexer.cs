using System;
using System.CodeDom.Compiler;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;

[GeneratedCode("ANTLR", "4.7.2")]
public class DotPlainLexer : Lexer
{
	protected static DFA[] decisionToDFA;

	protected static PredictionContextCache sharedContextCache;

	public const int GRAPH = 1;

	public const int NODE = 2;

	public const int EDGE = 3;

	public const int STOP = 4;

	public const int SP = 5;

	public const int NL = 6;

	public const int FIELD = 7;

	public static string[] channelNames;

	public static string[] modeNames;

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

	public override string[] ChannelNames => channelNames;

	public override string[] ModeNames => modeNames;

	public override string SerializedAtn => new string(_serializedATN);

	public DotPlainLexer(ICharStream input)
		: this(input, Console.Out, Console.Error)
	{
	}

	public DotPlainLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	static DotPlainLexer()
	{
		sharedContextCache = new PredictionContextCache();
		channelNames = new string[2] { "DEFAULT_TOKEN_CHANNEL", "HIDDEN" };
		modeNames = new string[1] { "DEFAULT_MODE" };
		ruleNames = new string[7] { "GRAPH", "NODE", "EDGE", "STOP", "SP", "NL", "FIELD" };
		_LiteralNames = new string[7] { null, "'graph'", "'node'", "'edge'", "'stop'", "' '", "'\r\n'" };
		_SymbolicNames = new string[8] { null, "GRAPH", "NODE", "EDGE", "STOP", "SP", "NL", "FIELD" };
		DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);
		_serializedATN = new char[509]
		{
			'\u0003', '悋', 'Ꜫ', '脳', '맭', '䅼', '㯧', '瞆', '奤', '\u0002',
			'\t', '9', '\b', '\u0001', '\u0004', '\u0002', '\t', '\u0002', '\u0004', '\u0003',
			'\t', '\u0003', '\u0004', '\u0004', '\t', '\u0004', '\u0004', '\u0005', '\t', '\u0005',
			'\u0004', '\u0006', '\t', '\u0006', '\u0004', '\a', '\t', '\a', '\u0004', '\b',
			'\t', '\b', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0003', '\u0003', '\u0003', '\u0003', '\u0003',
			'\u0003', '\u0003', '\u0003', '\u0003', '\u0003', '\u0004', '\u0003', '\u0004', '\u0003', '\u0004',
			'\u0003', '\u0004', '\u0003', '\u0004', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0006', '\u0003', '\u0006', '\u0003', '\a',
			'\u0003', '\a', '\u0003', '\a', '\u0003', '\b', '\u0006', '\b', '-', '\n',
			'\b', '\r', '\b', '\u000e', '\b', '.', '\u0003', '\b', '\u0003', '\b',
			'\u0006', '\b', '3', '\n', '\b', '\r', '\b', '\u000e', '\b', '4',
			'\u0003', '\b', '\u0005', '\b', '8', '\n', '\b', '\u0002', '\u0002', '\t',
			'\u0003', '\u0003', '\u0005', '\u0004', '\a', '\u0005', '\t', '\u0006', '\v', '\a',
			'\r', '\b', '\u000f', '\t', '\u0003', '\u0002', '\u0004', '\u0005', '\u0002', '\f',
			'\f', '\u000f', '\u000f', '"', '"', '\u0003', '\u0002', '$', '$', '\u0002',
			';', '\u0002', '\u0003', '\u0003', '\u0002', '\u0002', '\u0002', '\u0002', '\u0005', '\u0003',
			'\u0002', '\u0002', '\u0002', '\u0002', '\a', '\u0003', '\u0002', '\u0002', '\u0002', '\u0002',
			'\t', '\u0003', '\u0002', '\u0002', '\u0002', '\u0002', '\v', '\u0003', '\u0002', '\u0002',
			'\u0002', '\u0002', '\r', '\u0003', '\u0002', '\u0002', '\u0002', '\u0002', '\u000f', '\u0003',
			'\u0002', '\u0002', '\u0002', '\u0003', '\u0011', '\u0003', '\u0002', '\u0002', '\u0002', '\u0005',
			'\u0017', '\u0003', '\u0002', '\u0002', '\u0002', '\a', '\u001c', '\u0003', '\u0002', '\u0002',
			'\u0002', '\t', '!', '\u0003', '\u0002', '\u0002', '\u0002', '\v', '&', '\u0003',
			'\u0002', '\u0002', '\u0002', '\r', '(', '\u0003', '\u0002', '\u0002', '\u0002', '\u000f',
			'7', '\u0003', '\u0002', '\u0002', '\u0002', '\u0011', '\u0012', '\a', 'i', '\u0002',
			'\u0002', '\u0012', '\u0013', '\a', 't', '\u0002', '\u0002', '\u0013', '\u0014', '\a',
			'c', '\u0002', '\u0002', '\u0014', '\u0015', '\a', 'r', '\u0002', '\u0002', '\u0015',
			'\u0016', '\a', 'j', '\u0002', '\u0002', '\u0016', '\u0004', '\u0003', '\u0002', '\u0002',
			'\u0002', '\u0017', '\u0018', '\a', 'p', '\u0002', '\u0002', '\u0018', '\u0019', '\a',
			'q', '\u0002', '\u0002', '\u0019', '\u001a', '\a', 'f', '\u0002', '\u0002', '\u001a',
			'\u001b', '\a', 'g', '\u0002', '\u0002', '\u001b', '\u0006', '\u0003', '\u0002', '\u0002',
			'\u0002', '\u001c', '\u001d', '\a', 'g', '\u0002', '\u0002', '\u001d', '\u001e', '\a',
			'f', '\u0002', '\u0002', '\u001e', '\u001f', '\a', 'i', '\u0002', '\u0002', '\u001f',
			' ', '\a', 'g', '\u0002', '\u0002', ' ', '\b', '\u0003', '\u0002', '\u0002',
			'\u0002', '!', '"', '\a', 'u', '\u0002', '\u0002', '"', '#', '\a',
			'v', '\u0002', '\u0002', '#', '$', '\a', 'q', '\u0002', '\u0002', '$',
			'%', '\a', 'r', '\u0002', '\u0002', '%', '\n', '\u0003', '\u0002', '\u0002',
			'\u0002', '&', '\'', '\a', '"', '\u0002', '\u0002', '\'', '\f', '\u0003',
			'\u0002', '\u0002', '\u0002', '(', ')', '\a', '\u000f', '\u0002', '\u0002', ')',
			'*', '\a', '\f', '\u0002', '\u0002', '*', '\u000e', '\u0003', '\u0002', '\u0002',
			'\u0002', '+', '-', '\n', '\u0002', '\u0002', '\u0002', ',', '+', '\u0003',
			'\u0002', '\u0002', '\u0002', '-', '.', '\u0003', '\u0002', '\u0002', '\u0002', '.',
			',', '\u0003', '\u0002', '\u0002', '\u0002', '.', '/', '\u0003', '\u0002', '\u0002',
			'\u0002', '/', '8', '\u0003', '\u0002', '\u0002', '\u0002', '0', '2', '\a',
			'$', '\u0002', '\u0002', '1', '3', '\n', '\u0003', '\u0002', '\u0002', '2',
			'1', '\u0003', '\u0002', '\u0002', '\u0002', '3', '4', '\u0003', '\u0002', '\u0002',
			'\u0002', '4', '2', '\u0003', '\u0002', '\u0002', '\u0002', '4', '5', '\u0003',
			'\u0002', '\u0002', '\u0002', '5', '6', '\u0003', '\u0002', '\u0002', '\u0002', '6',
			'8', '\a', '$', '\u0002', '\u0002', '7', ',', '\u0003', '\u0002', '\u0002',
			'\u0002', '7', '0', '\u0003', '\u0002', '\u0002', '\u0002', '8', '\u0010', '\u0003',
			'\u0002', '\u0002', '\u0002', '\u0006', '\u0002', '.', '4', '7', '\u0002'
		};
		_ATN = new ATNDeserializer().Deserialize(_serializedATN);
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++)
		{
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
}
