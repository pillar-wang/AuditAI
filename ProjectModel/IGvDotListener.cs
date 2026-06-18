using System;
using System.CodeDom.Compiler;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

[GeneratedCode("ANTLR", "4.7.2")]
public interface IGvDotListener : IParseTreeListener
{
	void EnterGraph([NotNull] GvDotParser.GraphContext context);

	void ExitGraph([NotNull] GvDotParser.GraphContext context);

	void EnterStmt_list([NotNull] GvDotParser.Stmt_listContext context);

	void ExitStmt_list([NotNull] GvDotParser.Stmt_listContext context);

	void EnterStmt([NotNull] GvDotParser.StmtContext context);

	void ExitStmt([NotNull] GvDotParser.StmtContext context);

	void EnterAttr_stmt([NotNull] GvDotParser.Attr_stmtContext context);

	void ExitAttr_stmt([NotNull] GvDotParser.Attr_stmtContext context);

	void EnterAttr_list([NotNull] GvDotParser.Attr_listContext context);

	void ExitAttr_list([NotNull] GvDotParser.Attr_listContext context);

	void EnterA_list([NotNull] GvDotParser.A_listContext context);

	void ExitA_list([NotNull] GvDotParser.A_listContext context);

	void EnterEdge_stmt([NotNull] GvDotParser.Edge_stmtContext context);

	void ExitEdge_stmt([NotNull] GvDotParser.Edge_stmtContext context);

	void EnterEdgeRHS([NotNull] GvDotParser.EdgeRHSContext context);

	void ExitEdgeRHS([NotNull] GvDotParser.EdgeRHSContext context);

	void EnterEdgeop([NotNull] GvDotParser.EdgeopContext context);

	void ExitEdgeop([NotNull] GvDotParser.EdgeopContext context);

	void EnterNode_stmt([NotNull] GvDotParser.Node_stmtContext context);

	void ExitNode_stmt([NotNull] GvDotParser.Node_stmtContext context);

	void EnterNode_id([NotNull] GvDotParser.Node_idContext context);

	void ExitNode_id([NotNull] GvDotParser.Node_idContext context);

	void EnterPort([NotNull] GvDotParser.PortContext context);

	void ExitPort([NotNull] GvDotParser.PortContext context);

	void EnterSubgraph([NotNull] GvDotParser.SubgraphContext context);

	void ExitSubgraph([NotNull] GvDotParser.SubgraphContext context);

	void EnterId([NotNull] GvDotParser.IdContext context);

	void ExitId([NotNull] GvDotParser.IdContext context);
}
