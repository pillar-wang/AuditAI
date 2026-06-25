using System;
using System.CodeDom.Compiler;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.7.2")]
public class GvDotParser : Parser
{
	public class GraphContext : ParserRuleContext
	{
		public override int RuleIndex => 0;

		public Stmt_listContext stmt_list()
		{
			return GetRuleContext<Stmt_listContext>(0);
		}

		public ITerminalNode GRAPH()
		{
			return GetToken(12, 0);
		}

		public ITerminalNode DIGRAPH()
		{
			return GetToken(13, 0);
		}

		public ITerminalNode STRICT()
		{
			return GetToken(11, 0);
		}

		public IdContext id()
		{
			return GetRuleContext<IdContext>(0);
		}

		public GraphContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterGraph(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitGraph(this);
			}
		}
	}

	public class Stmt_listContext : ParserRuleContext
	{
		public override int RuleIndex => 1;

		public StmtContext[] stmt()
		{
			return GetRuleContexts<StmtContext>();
		}

		public StmtContext stmt(int i)
		{
			return GetRuleContext<StmtContext>(i);
		}

		public Stmt_listContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterStmt_list(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitStmt_list(this);
			}
		}
	}

	public class StmtContext : ParserRuleContext
	{
		public override int RuleIndex => 2;

		public Node_stmtContext node_stmt()
		{
			return GetRuleContext<Node_stmtContext>(0);
		}

		public Edge_stmtContext edge_stmt()
		{
			return GetRuleContext<Edge_stmtContext>(0);
		}

		public Attr_stmtContext attr_stmt()
		{
			return GetRuleContext<Attr_stmtContext>(0);
		}

		public IdContext[] id()
		{
			return GetRuleContexts<IdContext>();
		}

		public IdContext id(int i)
		{
			return GetRuleContext<IdContext>(i);
		}

		public SubgraphContext subgraph()
		{
			return GetRuleContext<SubgraphContext>(0);
		}

		public StmtContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterStmt(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitStmt(this);
			}
		}
	}

	public class Attr_stmtContext : ParserRuleContext
	{
		public override int RuleIndex => 3;

		public Attr_listContext attr_list()
		{
			return GetRuleContext<Attr_listContext>(0);
		}

		public ITerminalNode GRAPH()
		{
			return GetToken(12, 0);
		}

		public ITerminalNode NODE()
		{
			return GetToken(14, 0);
		}

		public ITerminalNode EDGE()
		{
			return GetToken(15, 0);
		}

		public Attr_stmtContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterAttr_stmt(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitAttr_stmt(this);
			}
		}
	}

	public class Attr_listContext : ParserRuleContext
	{
		public override int RuleIndex => 4;

		public A_listContext[] a_list()
		{
			return GetRuleContexts<A_listContext>();
		}

		public A_listContext a_list(int i)
		{
			return GetRuleContext<A_listContext>(i);
		}

		public Attr_listContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterAttr_list(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitAttr_list(this);
			}
		}
	}

	public class A_listContext : ParserRuleContext
	{
		public override int RuleIndex => 5;

		public IdContext[] id()
		{
			return GetRuleContexts<IdContext>();
		}

		public IdContext id(int i)
		{
			return GetRuleContext<IdContext>(i);
		}

		public A_listContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterA_list(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitA_list(this);
			}
		}
	}

	public class Edge_stmtContext : ParserRuleContext
	{
		public override int RuleIndex => 6;

		public EdgeRHSContext edgeRHS()
		{
			return GetRuleContext<EdgeRHSContext>(0);
		}

		public Node_idContext node_id()
		{
			return GetRuleContext<Node_idContext>(0);
		}

		public SubgraphContext subgraph()
		{
			return GetRuleContext<SubgraphContext>(0);
		}

		public Attr_listContext attr_list()
		{
			return GetRuleContext<Attr_listContext>(0);
		}

		public Edge_stmtContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterEdge_stmt(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitEdge_stmt(this);
			}
		}
	}

	public class EdgeRHSContext : ParserRuleContext
	{
		public override int RuleIndex => 7;

		public EdgeopContext[] edgeop()
		{
			return GetRuleContexts<EdgeopContext>();
		}

		public EdgeopContext edgeop(int i)
		{
			return GetRuleContext<EdgeopContext>(i);
		}

		public Node_idContext[] node_id()
		{
			return GetRuleContexts<Node_idContext>();
		}

		public Node_idContext node_id(int i)
		{
			return GetRuleContext<Node_idContext>(i);
		}

		public SubgraphContext[] subgraph()
		{
			return GetRuleContexts<SubgraphContext>();
		}

		public SubgraphContext subgraph(int i)
		{
			return GetRuleContext<SubgraphContext>(i);
		}

		public EdgeRHSContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterEdgeRHS(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitEdgeRHS(this);
			}
		}
	}

	public class EdgeopContext : ParserRuleContext
	{
		public override int RuleIndex => 8;

		public EdgeopContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterEdgeop(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitEdgeop(this);
			}
		}
	}

	public class Node_stmtContext : ParserRuleContext
	{
		public override int RuleIndex => 9;

		public Node_idContext node_id()
		{
			return GetRuleContext<Node_idContext>(0);
		}

		public Attr_listContext attr_list()
		{
			return GetRuleContext<Attr_listContext>(0);
		}

		public Node_stmtContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterNode_stmt(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitNode_stmt(this);
			}
		}
	}

	public class Node_idContext : ParserRuleContext
	{
		public override int RuleIndex => 10;

		public IdContext id()
		{
			return GetRuleContext<IdContext>(0);
		}

		public PortContext port()
		{
			return GetRuleContext<PortContext>(0);
		}

		public Node_idContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterNode_id(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitNode_id(this);
			}
		}
	}

	public class PortContext : ParserRuleContext
	{
		public override int RuleIndex => 11;

		public IdContext[] id()
		{
			return GetRuleContexts<IdContext>();
		}

		public IdContext id(int i)
		{
			return GetRuleContext<IdContext>(i);
		}

		public PortContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterPort(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitPort(this);
			}
		}
	}

	public class SubgraphContext : ParserRuleContext
	{
		public override int RuleIndex => 12;

		public Stmt_listContext stmt_list()
		{
			return GetRuleContext<Stmt_listContext>(0);
		}

		public ITerminalNode SUBGRAPH()
		{
			return GetToken(16, 0);
		}

		public IdContext id()
		{
			return GetRuleContext<IdContext>(0);
		}

		public SubgraphContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterSubgraph(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitSubgraph(this);
			}
		}
	}

	public class IdContext : ParserRuleContext
	{
		public override int RuleIndex => 13;

		public ITerminalNode ID()
		{
			return GetToken(19, 0);
		}

		public ITerminalNode STRING()
		{
			return GetToken(18, 0);
		}

		public ITerminalNode HTML_STRING()
		{
			return GetToken(20, 0);
		}

		public ITerminalNode NUMBER()
		{
			return GetToken(17, 0);
		}

		public IdContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}

		public override void EnterRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.EnterId(this);
			}
		}

		public override void ExitRule(IParseTreeListener listener)
		{
			if (listener is IGvDotListener gvDotListener)
			{
				gvDotListener.ExitId(this);
			}
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

	public const int STRICT = 11;

	public const int GRAPH = 12;

	public const int DIGRAPH = 13;

	public const int NODE = 14;

	public const int EDGE = 15;

	public const int SUBGRAPH = 16;

	public const int NUMBER = 17;

	public const int STRING = 18;

	public const int ID = 19;

	public const int HTML_STRING = 20;

	public const int COMMENT = 21;

	public const int LINE_COMMENT = 22;

	public const int PREPROC = 23;

	public const int WS = 24;

	public const int RULE_graph = 0;

	public const int RULE_stmt_list = 1;

	public const int RULE_stmt = 2;

	public const int RULE_attr_stmt = 3;

	public const int RULE_attr_list = 4;

	public const int RULE_a_list = 5;

	public const int RULE_edge_stmt = 6;

	public const int RULE_edgeRHS = 7;

	public const int RULE_edgeop = 8;

	public const int RULE_node_stmt = 9;

	public const int RULE_node_id = 10;

	public const int RULE_port = 11;

	public const int RULE_subgraph = 12;

	public const int RULE_id = 13;

	public static readonly string[] ruleNames;

	private static readonly string[] _LiteralNames;

	private static readonly string[] _SymbolicNames;

	public static readonly IVocabulary DefaultVocabulary;

	private static char[] _serializedATN;

	public static readonly ATN _ATN;

	[NotNull]
	public override IVocabulary Vocabulary => DefaultVocabulary;

	public override string GrammarFileName => "GvDot.g4";

	public override string[] RuleNames => ruleNames;

	public override string SerializedAtn => new string(_serializedATN);

	static GvDotParser()
	{
		sharedContextCache = new PredictionContextCache();
		ruleNames = new string[14]
		{
			"graph", "stmt_list", "stmt", "attr_stmt", "attr_list", "a_list", "edge_stmt", "edgeRHS", "edgeop", "node_stmt",
			"node_id", "port", "subgraph", "id"
		};
		_LiteralNames = new string[11]
		{
			null, "'{'", "'}'", "';'", "'='", "'['", "']'", "','", "'->'", "'--'",
			"':'"
		};
		_SymbolicNames = new string[25]
		{
			null, null, null, null, null, null, null, null, null, null,
			null, "STRICT", "GRAPH", "DIGRAPH", "NODE", "EDGE", "SUBGRAPH", "NUMBER", "STRING", "ID",
			"HTML_STRING", "COMMENT", "LINE_COMMENT", "PREPROC", "WS"
		};
		DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);
		_serializedATN = new char[1160]
		{
			'\u0003', '悋', 'Ꜫ', '脳', '맭', '䅼', '㯧', '瞆', '奤', '\u0003',
			'\u001a', '\u0082', '\u0004', '\u0002', '\t', '\u0002', '\u0004', '\u0003', '\t', '\u0003',
			'\u0004', '\u0004', '\t', '\u0004', '\u0004', '\u0005', '\t', '\u0005', '\u0004', '\u0006',
			'\t', '\u0006', '\u0004', '\a', '\t', '\a', '\u0004', '\b', '\t', '\b',
			'\u0004', '\t', '\t', '\t', '\u0004', '\n', '\t', '\n', '\u0004', '\v',
			'\t', '\v', '\u0004', '\f', '\t', '\f', '\u0004', '\r', '\t', '\r',
			'\u0004', '\u000e', '\t', '\u000e', '\u0004', '\u000f', '\t', '\u000f', '\u0003', '\u0002',
			'\u0005', '\u0002', ' ', '\n', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0005',
			'\u0002', '$', '\n', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002', '\u0003', '\u0002',
			'\u0003', '\u0002', '\u0003', '\u0003', '\u0003', '\u0003', '\u0005', '\u0003', ',', '\n',
			'\u0003', '\a', '\u0003', '.', '\n', '\u0003', '\f', '\u0003', '\u000e', '\u0003',
			'1', '\v', '\u0003', '\u0003', '\u0004', '\u0003', '\u0004', '\u0003', '\u0004', '\u0003',
			'\u0004', '\u0003', '\u0004', '\u0003', '\u0004', '\u0003', '\u0004', '\u0003', '\u0004', '\u0005',
			'\u0004', ';', '\n', '\u0004', '\u0003', '\u0005', '\u0003', '\u0005', '\u0003', '\u0005',
			'\u0003', '\u0006', '\u0003', '\u0006', '\u0005', '\u0006', 'B', '\n', '\u0006', '\u0003',
			'\u0006', '\u0006', '\u0006', 'E', '\n', '\u0006', '\r', '\u0006', '\u000e', '\u0006',
			'F', '\u0003', '\a', '\u0003', '\a', '\u0003', '\a', '\u0005', '\a', 'L',
			'\n', '\a', '\u0003', '\a', '\u0005', '\a', 'O', '\n', '\a', '\u0006',
			'\a', 'Q', '\n', '\a', '\r', '\a', '\u000e', '\a', 'R', '\u0003',
			'\b', '\u0003', '\b', '\u0005', '\b', 'W', '\n', '\b', '\u0003', '\b',
			'\u0003', '\b', '\u0005', '\b', '[', '\n', '\b', '\u0003', '\t', '\u0003',
			'\t', '\u0003', '\t', '\u0005', '\t', '`', '\n', '\t', '\u0006', '\t',
			'b', '\n', '\t', '\r', '\t', '\u000e', '\t', 'c', '\u0003', '\n',
			'\u0003', '\n', '\u0003', '\v', '\u0003', '\v', '\u0005', '\v', 'j', '\n',
			'\v', '\u0003', '\f', '\u0003', '\f', '\u0005', '\f', 'n', '\n', '\f',
			'\u0003', '\r', '\u0003', '\r', '\u0003', '\r', '\u0003', '\r', '\u0005', '\r',
			't', '\n', '\r', '\u0003', '\u000e', '\u0003', '\u000e', '\u0005', '\u000e', 'x',
			'\n', '\u000e', '\u0005', '\u000e', 'z', '\n', '\u000e', '\u0003', '\u000e', '\u0003',
			'\u000e', '\u0003', '\u000e', '\u0003', '\u000e', '\u0003', '\u000f', '\u0003', '\u000f', '\u0003',
			'\u000f', '\u0002', '\u0002', '\u0010', '\u0002', '\u0004', '\u0006', '\b', '\n', '\f',
			'\u000e', '\u0010', '\u0012', '\u0014', '\u0016', '\u0018', '\u001a', '\u001c', '\u0002', '\u0006',
			'\u0003', '\u0002', '\u000e', '\u000f', '\u0004', '\u0002', '\u000e', '\u000e', '\u0010', '\u0011',
			'\u0003', '\u0002', '\n', '\v', '\u0003', '\u0002', '\u0013', '\u0016', '\u0002', '\u0089',
			'\u0002', '\u001f', '\u0003', '\u0002', '\u0002', '\u0002', '\u0004', '/', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u0006', ':', '\u0003', '\u0002', '\u0002', '\u0002', '\b', '<',
			'\u0003', '\u0002', '\u0002', '\u0002', '\n', 'D', '\u0003', '\u0002', '\u0002', '\u0002',
			'\f', 'P', '\u0003', '\u0002', '\u0002', '\u0002', '\u000e', 'V', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u0010', 'a', '\u0003', '\u0002', '\u0002', '\u0002', '\u0012', 'e',
			'\u0003', '\u0002', '\u0002', '\u0002', '\u0014', 'g', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u0016', 'k', '\u0003', '\u0002', '\u0002', '\u0002', '\u0018', 'o', '\u0003', '\u0002',
			'\u0002', '\u0002', '\u001a', 'y', '\u0003', '\u0002', '\u0002', '\u0002', '\u001c', '\u007f',
			'\u0003', '\u0002', '\u0002', '\u0002', '\u001e', ' ', '\a', '\r', '\u0002', '\u0002',
			'\u001f', '\u001e', '\u0003', '\u0002', '\u0002', '\u0002', '\u001f', ' ', '\u0003', '\u0002',
			'\u0002', '\u0002', ' ', '!', '\u0003', '\u0002', '\u0002', '\u0002', '!', '#',
			'\t', '\u0002', '\u0002', '\u0002', '"', '$', '\u0005', '\u001c', '\u000f', '\u0002',
			'#', '"', '\u0003', '\u0002', '\u0002', '\u0002', '#', '$', '\u0003', '\u0002',
			'\u0002', '\u0002', '$', '%', '\u0003', '\u0002', '\u0002', '\u0002', '%', '&',
			'\a', '\u0003', '\u0002', '\u0002', '&', '\'', '\u0005', '\u0004', '\u0003', '\u0002',
			'\'', '(', '\a', '\u0004', '\u0002', '\u0002', '(', '\u0003', '\u0003', '\u0002',
			'\u0002', '\u0002', ')', '+', '\u0005', '\u0006', '\u0004', '\u0002', '*', ',',
			'\a', '\u0005', '\u0002', '\u0002', '+', '*', '\u0003', '\u0002', '\u0002', '\u0002',
			'+', ',', '\u0003', '\u0002', '\u0002', '\u0002', ',', '.', '\u0003', '\u0002',
			'\u0002', '\u0002', '-', ')', '\u0003', '\u0002', '\u0002', '\u0002', '.', '1',
			'\u0003', '\u0002', '\u0002', '\u0002', '/', '-', '\u0003', '\u0002', '\u0002', '\u0002',
			'/', '0', '\u0003', '\u0002', '\u0002', '\u0002', '0', '\u0005', '\u0003', '\u0002',
			'\u0002', '\u0002', '1', '/', '\u0003', '\u0002', '\u0002', '\u0002', '2', ';',
			'\u0005', '\u0014', '\v', '\u0002', '3', ';', '\u0005', '\u000e', '\b', '\u0002',
			'4', ';', '\u0005', '\b', '\u0005', '\u0002', '5', '6', '\u0005', '\u001c',
			'\u000f', '\u0002', '6', '7', '\a', '\u0006', '\u0002', '\u0002', '7', '8',
			'\u0005', '\u001c', '\u000f', '\u0002', '8', ';', '\u0003', '\u0002', '\u0002', '\u0002',
			'9', ';', '\u0005', '\u001a', '\u000e', '\u0002', ':', '2', '\u0003', '\u0002',
			'\u0002', '\u0002', ':', '3', '\u0003', '\u0002', '\u0002', '\u0002', ':', '4',
			'\u0003', '\u0002', '\u0002', '\u0002', ':', '5', '\u0003', '\u0002', '\u0002', '\u0002',
			':', '9', '\u0003', '\u0002', '\u0002', '\u0002', ';', '\a', '\u0003', '\u0002',
			'\u0002', '\u0002', '<', '=', '\t', '\u0003', '\u0002', '\u0002', '=', '>',
			'\u0005', '\n', '\u0006', '\u0002', '>', '\t', '\u0003', '\u0002', '\u0002', '\u0002',
			'?', 'A', '\a', '\a', '\u0002', '\u0002', '@', 'B', '\u0005', '\f',
			'\a', '\u0002', 'A', '@', '\u0003', '\u0002', '\u0002', '\u0002', 'A', 'B',
			'\u0003', '\u0002', '\u0002', '\u0002', 'B', 'C', '\u0003', '\u0002', '\u0002', '\u0002',
			'C', 'E', '\a', '\b', '\u0002', '\u0002', 'D', '?', '\u0003', '\u0002',
			'\u0002', '\u0002', 'E', 'F', '\u0003', '\u0002', '\u0002', '\u0002', 'F', 'D',
			'\u0003', '\u0002', '\u0002', '\u0002', 'F', 'G', '\u0003', '\u0002', '\u0002', '\u0002',
			'G', '\v', '\u0003', '\u0002', '\u0002', '\u0002', 'H', 'K', '\u0005', '\u001c',
			'\u000f', '\u0002', 'I', 'J', '\a', '\u0006', '\u0002', '\u0002', 'J', 'L',
			'\u0005', '\u001c', '\u000f', '\u0002', 'K', 'I', '\u0003', '\u0002', '\u0002', '\u0002',
			'K', 'L', '\u0003', '\u0002', '\u0002', '\u0002', 'L', 'N', '\u0003', '\u0002',
			'\u0002', '\u0002', 'M', 'O', '\a', '\t', '\u0002', '\u0002', 'N', 'M',
			'\u0003', '\u0002', '\u0002', '\u0002', 'N', 'O', '\u0003', '\u0002', '\u0002', '\u0002',
			'O', 'Q', '\u0003', '\u0002', '\u0002', '\u0002', 'P', 'H', '\u0003', '\u0002',
			'\u0002', '\u0002', 'Q', 'R', '\u0003', '\u0002', '\u0002', '\u0002', 'R', 'P',
			'\u0003', '\u0002', '\u0002', '\u0002', 'R', 'S', '\u0003', '\u0002', '\u0002', '\u0002',
			'S', '\r', '\u0003', '\u0002', '\u0002', '\u0002', 'T', 'W', '\u0005', '\u0016',
			'\f', '\u0002', 'U', 'W', '\u0005', '\u001a', '\u000e', '\u0002', 'V', 'T',
			'\u0003', '\u0002', '\u0002', '\u0002', 'V', 'U', '\u0003', '\u0002', '\u0002', '\u0002',
			'W', 'X', '\u0003', '\u0002', '\u0002', '\u0002', 'X', 'Z', '\u0005', '\u0010',
			'\t', '\u0002', 'Y', '[', '\u0005', '\n', '\u0006', '\u0002', 'Z', 'Y',
			'\u0003', '\u0002', '\u0002', '\u0002', 'Z', '[', '\u0003', '\u0002', '\u0002', '\u0002',
			'[', '\u000f', '\u0003', '\u0002', '\u0002', '\u0002', '\\', '_', '\u0005', '\u0012',
			'\n', '\u0002', ']', '`', '\u0005', '\u0016', '\f', '\u0002', '^', '`',
			'\u0005', '\u001a', '\u000e', '\u0002', '_', ']', '\u0003', '\u0002', '\u0002', '\u0002',
			'_', '^', '\u0003', '\u0002', '\u0002', '\u0002', '`', 'b', '\u0003', '\u0002',
			'\u0002', '\u0002', 'a', '\\', '\u0003', '\u0002', '\u0002', '\u0002', 'b', 'c',
			'\u0003', '\u0002', '\u0002', '\u0002', 'c', 'a', '\u0003', '\u0002', '\u0002', '\u0002',
			'c', 'd', '\u0003', '\u0002', '\u0002', '\u0002', 'd', '\u0011', '\u0003', '\u0002',
			'\u0002', '\u0002', 'e', 'f', '\t', '\u0004', '\u0002', '\u0002', 'f', '\u0013',
			'\u0003', '\u0002', '\u0002', '\u0002', 'g', 'i', '\u0005', '\u0016', '\f', '\u0002',
			'h', 'j', '\u0005', '\n', '\u0006', '\u0002', 'i', 'h', '\u0003', '\u0002',
			'\u0002', '\u0002', 'i', 'j', '\u0003', '\u0002', '\u0002', '\u0002', 'j', '\u0015',
			'\u0003', '\u0002', '\u0002', '\u0002', 'k', 'm', '\u0005', '\u001c', '\u000f', '\u0002',
			'l', 'n', '\u0005', '\u0018', '\r', '\u0002', 'm', 'l', '\u0003', '\u0002',
			'\u0002', '\u0002', 'm', 'n', '\u0003', '\u0002', '\u0002', '\u0002', 'n', '\u0017',
			'\u0003', '\u0002', '\u0002', '\u0002', 'o', 'p', '\a', '\f', '\u0002', '\u0002',
			'p', 's', '\u0005', '\u001c', '\u000f', '\u0002', 'q', 'r', '\a', '\f',
			'\u0002', '\u0002', 'r', 't', '\u0005', '\u001c', '\u000f', '\u0002', 's', 'q',
			'\u0003', '\u0002', '\u0002', '\u0002', 's', 't', '\u0003', '\u0002', '\u0002', '\u0002',
			't', '\u0019', '\u0003', '\u0002', '\u0002', '\u0002', 'u', 'w', '\a', '\u0012',
			'\u0002', '\u0002', 'v', 'x', '\u0005', '\u001c', '\u000f', '\u0002', 'w', 'v',
			'\u0003', '\u0002', '\u0002', '\u0002', 'w', 'x', '\u0003', '\u0002', '\u0002', '\u0002',
			'x', 'z', '\u0003', '\u0002', '\u0002', '\u0002', 'y', 'u', '\u0003', '\u0002',
			'\u0002', '\u0002', 'y', 'z', '\u0003', '\u0002', '\u0002', '\u0002', 'z', '{',
			'\u0003', '\u0002', '\u0002', '\u0002', '{', '|', '\a', '\u0003', '\u0002', '\u0002',
			'|', '}', '\u0005', '\u0004', '\u0003', '\u0002', '}', '~', '\a', '\u0004',
			'\u0002', '\u0002', '~', '\u001b', '\u0003', '\u0002', '\u0002', '\u0002', '\u007f', '\u0080',
			'\t', '\u0005', '\u0002', '\u0002', '\u0080', '\u001d', '\u0003', '\u0002', '\u0002', '\u0002',
			'\u0015', '\u001f', '#', '+', '/', ':', 'A', 'F', 'K', 'N',
			'R', 'V', 'Z', '_', 'c', 'i', 'm', 's', 'w', 'y'
		};
		_ATN = new ATNDeserializer().Deserialize(_serializedATN);
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++)
		{
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

	public GvDotParser(ITokenStream input)
		: this(input, Console.Out, Console.Error)
	{
	}

	public GvDotParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	[RuleVersion(0)]
	public GraphContext graph()
	{
		GraphContext graphContext = new GraphContext(Context, base.State);
		EnterRule(graphContext, 0, 0);
		try
		{
			EnterOuterAlt(graphContext, 1);
			base.State = 29;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			if (num == 11)
			{
				base.State = 28;
				Match(11);
			}
			base.State = 31;
			num = base.TokenStream.LA(1);
			if (num != 12 && num != 13)
			{
				ErrorHandler.RecoverInline(this);
			}
			else
			{
				ErrorHandler.ReportMatch(this);
				Consume();
			}
			base.State = 33;
			ErrorHandler.Sync(this);
			num = base.TokenStream.LA(1);
			if ((num & -64) == 0 && ((1L << num) & 0x1E0000) != 0L)
			{
				base.State = 32;
				id();
			}
			base.State = 35;
			Match(1);
			base.State = 36;
			stmt_list();
			base.State = 37;
			Match(2);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (graphContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return graphContext;
	}

	[RuleVersion(0)]
	public Stmt_listContext stmt_list()
	{
		Stmt_listContext stmt_listContext = new Stmt_listContext(Context, base.State);
		EnterRule(stmt_listContext, 2, 1);
		try
		{
			EnterOuterAlt(stmt_listContext, 1);
			base.State = 45;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			while ((num & -64) == 0 && ((1L << num) & 0x1FD002) != 0L)
			{
				base.State = 39;
				stmt();
				base.State = 41;
				ErrorHandler.Sync(this);
				num = base.TokenStream.LA(1);
				if (num == 3)
				{
					base.State = 40;
					Match(3);
				}
				base.State = 47;
				ErrorHandler.Sync(this);
				num = base.TokenStream.LA(1);
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (stmt_listContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return stmt_listContext;
	}

	[RuleVersion(0)]
	public StmtContext stmt()
	{
		StmtContext stmtContext = new StmtContext(Context, base.State);
		EnterRule(stmtContext, 4, 2);
		try
		{
			base.State = 56;
			ErrorHandler.Sync(this);
			switch (Interpreter.AdaptivePredict(base.TokenStream, 4, Context))
			{
			case 1:
				EnterOuterAlt(stmtContext, 1);
				base.State = 48;
				node_stmt();
				break;
			case 2:
				EnterOuterAlt(stmtContext, 2);
				base.State = 49;
				edge_stmt();
				break;
			case 3:
				EnterOuterAlt(stmtContext, 3);
				base.State = 50;
				attr_stmt();
				break;
			case 4:
				EnterOuterAlt(stmtContext, 4);
				base.State = 51;
				id();
				base.State = 52;
				Match(4);
				base.State = 53;
				id();
				break;
			case 5:
				EnterOuterAlt(stmtContext, 5);
				base.State = 55;
				subgraph();
				break;
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (stmtContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return stmtContext;
	}

	[RuleVersion(0)]
	public Attr_stmtContext attr_stmt()
	{
		Attr_stmtContext attr_stmtContext = new Attr_stmtContext(Context, base.State);
		EnterRule(attr_stmtContext, 6, 3);
		try
		{
			EnterOuterAlt(attr_stmtContext, 1);
			base.State = 58;
			int num = base.TokenStream.LA(1);
			if (((uint)num & 0xFFFFFFC0u) != 0 || ((1L << num) & 0xD000) == 0L)
			{
				ErrorHandler.RecoverInline(this);
			}
			else
			{
				ErrorHandler.ReportMatch(this);
				Consume();
			}
			base.State = 59;
			attr_list();
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (attr_stmtContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return attr_stmtContext;
	}

	[RuleVersion(0)]
	public Attr_listContext attr_list()
	{
		Attr_listContext attr_listContext = new Attr_listContext(Context, base.State);
		EnterRule(attr_listContext, 8, 4);
		try
		{
			EnterOuterAlt(attr_listContext, 1);
			base.State = 66;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			do
			{
				base.State = 61;
				Match(5);
				base.State = 63;
				ErrorHandler.Sync(this);
				num = base.TokenStream.LA(1);
				if ((num & -64) == 0 && ((1L << num) & 0x1E0000) != 0L)
				{
					base.State = 62;
					a_list();
				}
				base.State = 65;
				Match(6);
				base.State = 68;
				ErrorHandler.Sync(this);
				num = base.TokenStream.LA(1);
			}
			while (num == 5);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (attr_listContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return attr_listContext;
	}

	[RuleVersion(0)]
	public A_listContext a_list()
	{
		A_listContext a_listContext = new A_listContext(Context, base.State);
		EnterRule(a_listContext, 10, 5);
		try
		{
			EnterOuterAlt(a_listContext, 1);
			base.State = 78;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			do
			{
				base.State = 70;
				id();
				base.State = 73;
				ErrorHandler.Sync(this);
				num = base.TokenStream.LA(1);
				if (num == 4)
				{
					base.State = 71;
					Match(4);
					base.State = 72;
					id();
				}
				base.State = 76;
				ErrorHandler.Sync(this);
				num = base.TokenStream.LA(1);
				if (num == 7)
				{
					base.State = 75;
					Match(7);
				}
				base.State = 80;
				ErrorHandler.Sync(this);
				num = base.TokenStream.LA(1);
			}
			while ((num & -64) == 0 && ((1L << num) & 0x1E0000) != 0L);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (a_listContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return a_listContext;
	}

	[RuleVersion(0)]
	public Edge_stmtContext edge_stmt()
	{
		Edge_stmtContext edge_stmtContext = new Edge_stmtContext(Context, base.State);
		EnterRule(edge_stmtContext, 12, 6);
		try
		{
			EnterOuterAlt(edge_stmtContext, 1);
			base.State = 84;
			ErrorHandler.Sync(this);
			switch (base.TokenStream.LA(1))
			{
			case 17:
			case 18:
			case 19:
			case 20:
				base.State = 82;
				node_id();
				break;
			case 1:
			case 16:
				base.State = 83;
				subgraph();
				break;
			default:
				throw new NoViableAltException(this);
			}
			base.State = 86;
			edgeRHS();
			base.State = 88;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			if (num == 5)
			{
				base.State = 87;
				attr_list();
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (edge_stmtContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return edge_stmtContext;
	}

	[RuleVersion(0)]
	public EdgeRHSContext edgeRHS()
	{
		EdgeRHSContext edgeRHSContext = new EdgeRHSContext(Context, base.State);
		EnterRule(edgeRHSContext, 14, 7);
		try
		{
			EnterOuterAlt(edgeRHSContext, 1);
			base.State = 95;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			do
			{
				base.State = 90;
				edgeop();
				base.State = 93;
				ErrorHandler.Sync(this);
				switch (base.TokenStream.LA(1))
				{
				case 17:
				case 18:
				case 19:
				case 20:
					base.State = 91;
					node_id();
					break;
				case 1:
				case 16:
					base.State = 92;
					subgraph();
					break;
				default:
					throw new NoViableAltException(this);
				}
				base.State = 97;
				ErrorHandler.Sync(this);
				num = base.TokenStream.LA(1);
			}
			while (num == 8 || num == 9);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (edgeRHSContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return edgeRHSContext;
	}

	[RuleVersion(0)]
	public EdgeopContext edgeop()
	{
		EdgeopContext edgeopContext = new EdgeopContext(Context, base.State);
		EnterRule(edgeopContext, 16, 8);
		try
		{
			EnterOuterAlt(edgeopContext, 1);
			base.State = 99;
			int num = base.TokenStream.LA(1);
			if (num != 8 && num != 9)
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
			RecognitionException e = (edgeopContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return edgeopContext;
	}

	[RuleVersion(0)]
	public Node_stmtContext node_stmt()
	{
		Node_stmtContext node_stmtContext = new Node_stmtContext(Context, base.State);
		EnterRule(node_stmtContext, 18, 9);
		try
		{
			EnterOuterAlt(node_stmtContext, 1);
			base.State = 101;
			node_id();
			base.State = 103;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			if (num == 5)
			{
				base.State = 102;
				attr_list();
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (node_stmtContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return node_stmtContext;
	}

	[RuleVersion(0)]
	public Node_idContext node_id()
	{
		Node_idContext node_idContext = new Node_idContext(Context, base.State);
		EnterRule(node_idContext, 20, 10);
		try
		{
			EnterOuterAlt(node_idContext, 1);
			base.State = 105;
			id();
			base.State = 107;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			if (num == 10)
			{
				base.State = 106;
				port();
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (node_idContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return node_idContext;
	}

	[RuleVersion(0)]
	public PortContext port()
	{
		PortContext portContext = new PortContext(Context, base.State);
		EnterRule(portContext, 22, 11);
		try
		{
			EnterOuterAlt(portContext, 1);
			base.State = 109;
			Match(10);
			base.State = 110;
			id();
			base.State = 113;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			if (num == 10)
			{
				base.State = 111;
				Match(10);
				base.State = 112;
				id();
			}
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (portContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return portContext;
	}

	[RuleVersion(0)]
	public SubgraphContext subgraph()
	{
		SubgraphContext subgraphContext = new SubgraphContext(Context, base.State);
		EnterRule(subgraphContext, 24, 12);
		try
		{
			EnterOuterAlt(subgraphContext, 1);
			base.State = 119;
			ErrorHandler.Sync(this);
			int num = base.TokenStream.LA(1);
			if (num == 16)
			{
				base.State = 115;
				Match(16);
				base.State = 117;
				ErrorHandler.Sync(this);
				num = base.TokenStream.LA(1);
				if ((num & -64) == 0 && ((1L << num) & 0x1E0000) != 0L)
				{
					base.State = 116;
					id();
				}
			}
			base.State = 121;
			Match(1);
			base.State = 122;
			stmt_list();
			base.State = 123;
			Match(2);
		}
		catch (RecognitionException exception)
		{
			RecognitionException e = (subgraphContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return subgraphContext;
	}

	[RuleVersion(0)]
	public IdContext id()
	{
		IdContext idContext = new IdContext(Context, base.State);
		EnterRule(idContext, 26, 13);
		try
		{
			EnterOuterAlt(idContext, 1);
			base.State = 125;
			int num = base.TokenStream.LA(1);
			if (((uint)num & 0xFFFFFFC0u) != 0 || ((1L << num) & 0x1E0000) == 0L)
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
			RecognitionException e = (idContext.exception = exception);
			ErrorHandler.ReportError(this, e);
			ErrorHandler.Recover(this, e);
		}
		finally
		{
			ExitRule();
		}
		return idContext;
	}
}
