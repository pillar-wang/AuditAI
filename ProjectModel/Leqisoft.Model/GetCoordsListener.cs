using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Antlr4.Runtime.Misc;

namespace Leqisoft.Model;

public class GetCoordsListener : GvDotBaseListener
{
	private enum AttrKind
	{
		None,
		Graph,
		Node,
		Edge
	}

	private int _currentSubgraph = -1;

	private AttrKind _currentAttr;

	private int _currentNode = -1;

	private Tuple<int, int> _currentEdge;

	public Size GraphSize { get; private set; }

	public List<Tuple<int, Rectangle>> SubgraphBounds { get; } = new List<Tuple<int, Rectangle>>();


	public List<Tuple<int, Point>> NodePositions { get; } = new List<Tuple<int, Point>>();


	public List<Tuple<int, int, Point[]>> Edges { get; } = new List<Tuple<int, int, Point[]>>();


	public override void EnterSubgraph([NotNull] GvDotParser.SubgraphContext context)
	{
		_currentSubgraph = int.Parse(context.id().GetText().Substring("cluster".Length));
		base.EnterSubgraph(context);
	}

	public override void ExitSubgraph([NotNull] GvDotParser.SubgraphContext context)
	{
		_currentSubgraph = -1;
		base.ExitSubgraph(context);
	}

	public override void EnterAttr_stmt([NotNull] GvDotParser.Attr_stmtContext context)
	{
		if (context.GRAPH() != null)
		{
			_currentAttr = AttrKind.Graph;
		}
		else if (context.NODE() != null)
		{
			_currentAttr = AttrKind.Node;
		}
		else if (context.EDGE() != null)
		{
			_currentAttr = AttrKind.Edge;
		}
		base.EnterAttr_stmt(context);
	}

	public override void ExitAttr_stmt([NotNull] GvDotParser.Attr_stmtContext context)
	{
		_currentAttr = AttrKind.None;
		base.ExitAttr_stmt(context);
	}

	public override void EnterA_list([NotNull] GvDotParser.A_listContext context)
	{
		if (_currentAttr == AttrKind.Graph)
		{
			if (_currentSubgraph == -1)
			{
				GraphSize = GetGraphSize(context);
			}
			else
			{
				SubgraphBounds.Add(Tuple.Create(_currentSubgraph, GetSubgraphRect(context)));
			}
		}
		else if (_currentEdge != null)
		{
			AddEdge(context);
		}
		else if (_currentNode != -1)
		{
			NodePositions.Add(Tuple.Create(_currentNode, GetNodePosition(context)));
		}
		base.EnterA_list(context);
	}

	public override void EnterNode_stmt([NotNull] GvDotParser.Node_stmtContext context)
	{
		string text = context.node_id().GetText();
		if (!text.StartsWith("_hidden"))
		{
			_currentNode = int.Parse(text);
		}
		base.EnterNode_stmt(context);
	}

	public override void ExitNode_stmt([NotNull] GvDotParser.Node_stmtContext context)
	{
		_currentNode = -1;
		base.ExitNode_stmt(context);
	}

	public override void EnterEdge_stmt([NotNull] GvDotParser.Edge_stmtContext context)
	{
		string text = context.node_id().GetText();
		string text2 = context.edgeRHS().node_id(0).GetText();
		if (!text.StartsWith("_hidden") && !text2.StartsWith("_hidden"))
		{
			_currentEdge = Tuple.Create(int.Parse(text), int.Parse(text2));
		}
		base.EnterEdge_stmt(context);
	}

	public override void ExitEdge_stmt([NotNull] GvDotParser.Edge_stmtContext context)
	{
		_currentEdge = null;
		base.ExitEdge_stmt(context);
	}

	private Size GetGraphSize(GvDotParser.A_listContext context)
	{
		GvDotParser.IdContext[] array = context.id();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetText() == "bb")
			{
				int[] array2 = (from c in array[i + 1].GetText().Trim('"').Split(',')
					select PointToPixel(double.Parse(c))).ToArray();
				return new Size(array2[2], array2[3]);
			}
		}
		return Size.Empty;
	}

	private Rectangle GetSubgraphRect(GvDotParser.A_listContext context)
	{
		GvDotParser.IdContext[] array = context.id();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetText() == "bb")
			{
				int[] array2 = (from c in array[i + 1].GetText().Trim('"').Split(',')
					select PointToPixel(double.Parse(c))).ToArray();
				return new Rectangle(array2[0], GraphSize.Height - array2[3], array2[2] - array2[0], array2[3] - array2[1]);
			}
		}
		return Rectangle.Empty;
	}

	private void AddEdge(GvDotParser.A_listContext context)
	{
		GvDotParser.IdContext[] array = context.id();
		for (int i = 0; i < array.Length - 1; i++)
		{
			if (array[i].GetText() == "style" && array[i + 1].GetText() == "invis")
			{
				return;
			}
		}
		for (int k = 0; k < array.Length; k++)
		{
			if (array[k].GetText() == "pos")
			{
				string[] array2 = array[k + 1].GetText().Trim('"').Replace("\\\r\n", "")
					.Split(' ');
				Point[] array3 = array2.Where((string s, int j) => j % 3 == 1).Select(delegate(string s)
				{
					string[] array5 = s.Split(',');
					return new Point(PointToPixel(double.Parse(array5[0])), GraphSize.Height - PointToPixel(double.Parse(array5[1])));
				}).ToArray();
				string[] array4 = array2[0].Split(',');
				array3[array3.Length - 1] = new Point(PointToPixel(double.Parse(array4[1])), GraphSize.Height - PointToPixel(double.Parse(array4[2])));
				Edges.Add(Tuple.Create(_currentEdge.Item1, _currentEdge.Item2, array3));
			}
		}
	}

	private Point GetNodePosition(GvDotParser.A_listContext context)
	{
		GvDotParser.IdContext[] array = context.id();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].GetText() == "pos")
			{
				string[] array2 = array[i + 1].GetText().Trim('"').Split(',');
				return new Point(PointToPixel(double.Parse(array2[0])) - FormulaGraph.NodeSize.Width / 2, GraphSize.Height - PointToPixel(double.Parse(array2[1])) - FormulaGraph.NodeSize.Height / 2);
			}
		}
		return Point.Empty;
	}

	private static int PointToPixel(double point)
	{
		return (int)(point / 72.0 * 96.0);
	}
}
