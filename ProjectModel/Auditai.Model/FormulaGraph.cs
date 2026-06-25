using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Auditai.Model.Properties;

namespace Auditai.Model;

public class FormulaGraph
{
	public static readonly Size NodeSize = new Size(128, 100);

	public static readonly AdjustableArrowCap Cap = new AdjustableArrowCap(4f, 3f);

	private readonly StringFormat _stringFormat = new StringFormat
	{
		Trimming = StringTrimming.EllipsisCharacter,
		Alignment = StringAlignment.Center,
		LineAlignment = StringAlignment.Center,
		FormatFlags = StringFormatFlags.LineLimit
	};

	private const string GVPATH = ".\\graphviz\\dot.exe";

	private const int HEADER_HEIGHT = 30;

	private int _id;

	public List<GraphGroup> Groups { get; } = new List<GraphGroup>();


	public List<GraphNode> Nodes { get; } = new List<GraphNode>();


	public List<Tuple<GraphNode, GraphNode>> Edges { get; } = new List<Tuple<GraphNode, GraphNode>>();


	public List<Tuple<GraphNode, GraphNode, Point[]>> RenderEdges { get; } = new List<Tuple<GraphNode, GraphNode, Point[]>>();


	public List<Point[]> Lines { get; private set; } = new List<Point[]>();


	public Color ThemeColor { get; set; }

	public Color DarkColor { get; set; }

	public bool ShowNumber { get; set; }

	public Size Size { get; set; }

	public GraphDirectory CreateDirectoryNode()
	{
		GraphDirectory graphDirectory = new GraphDirectory
		{
			Id = _id
		};
		Nodes.Add(graphDirectory);
		_id++;
		return graphDirectory;
	}

	public GraphNode CreateNode()
	{
		GraphNode graphNode = new GraphNode
		{
			Id = _id
		};
		Nodes.Add(graphNode);
		_id++;
		return graphNode;
	}

	public Bitmap Render()
	{
		foreach (GraphGroup group in Groups)
		{
			foreach (GraphNode child in group.Children)
			{
				if (child is GraphDirectory d2)
				{
					SetGraphDirectoryExpandedHeight(d2);
				}
			}
		}
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("digraph G {");
		sb.AppendLine("newrank=true;");
		sb.AppendLine("ranksep=0.1;");
		sb.AppendLine("splines=ortho;");
		sb.AppendLine("fontname=\"microsoft yahei\";");
		sb.AppendLine("fontsize=10;");
		sb.AppendLine("margin=0;");
		sb.AppendLine($"node [fontname=\"microsoft yahei\",fontsize=9,width={(double)NodeSize.Width / 96.0},height={(double)NodeSize.Height / 96.0},fixedsize=true];");
		sb.AppendLine("edge [style=invis,penwidth=0.75,arrowsize=0.75];");
		int hiddenNodeIndex = 0;
		foreach (GraphGroup group2 in Groups)
		{
			sb.AppendLine($"subgraph \"cluster{group2.TreeGroup.Index}\"");
			sb.AppendLine("{label=\"" + group2.TreeGroup.Name + "\";");
			sb.AppendLine("color=gray;");
			if (group2.Children.Count > 0)
			{
				foreach (GraphNode child2 in group2.Children)
				{
					sb.Append($"\"{child2.Id}\"->");
					if (child2 is GraphDirectory { IsExpanded: not false } graphDirectory)
					{
						for (int i = 0; i < graphDirectory.ExpandedHeight; i++)
						{
							hiddenNodeIndex++;
							sb.Append($"_hidden{hiddenNodeIndex}->");
						}
					}
				}
				sb.Remove(sb.Length - 2, 2);
				sb.AppendLine(";");
				foreach (GraphNode child3 in group2.Children)
				{
					if (child3 is GraphDirectory { IsExpanded: not false } graphDirectory2)
					{
						Walk(graphDirectory2);
					}
				}
			}
			else
			{
				sb.Append($"_hidden{hiddenNodeIndex + 1}->_hidden{hiddenNodeIndex + 2}");
				hiddenNodeIndex += 2;
			}
			sb.AppendLine("}");
		}
		sb.AppendLine("edge [constraint=false;style=solid];");
		List<Tuple<GraphNode, GraphNode>> list = new List<Tuple<GraphNode, GraphNode>>();
		foreach (Tuple<GraphNode, GraphNode> edge in Edges)
		{
			GraphNode n2 = GetDrawnNode(edge.Item1);
			GraphNode n3 = GetDrawnNode(edge.Item2);
			if (n2 != n3 && list.All((Tuple<GraphNode, GraphNode> e) => e.Item1 != n2 || e.Item2 != n3))
			{
				list.Add(Tuple.Create(n2, n3));
				sb.AppendLine($"\"{n3.Id}\"->\"{n2.Id}\";");
			}
		}
		for (int j = 1; j <= hiddenNodeIndex; j++)
		{
			sb.AppendLine($"_hidden{j} [style=invis];");
		}
		foreach (GraphGroup group3 in Groups)
		{
			foreach (GraphNode child4 in group3.Children)
			{
				SetNodeAttribute(child4);
			}
		}
		sb.AppendLine("}");
		return RenderDot(sb.ToString());
		static Bitmap GetDirIcon(GraphDirectory d)
		{
			if (d.IsExpanded)
			{
				return Resource.FolderExpanded;
			}
			return Resource.FolderCollapsed;
		}
		static GraphNode GetDrawnNode(GraphNode n)
		{
			if (n.Parent == null)
			{
				return n;
			}
			if (n.Parent.IsExpanded)
			{
				return n;
			}
			return GetDrawnNode(n.Parent);
		}
		static Bitmap GetNodeBitmap(GraphNode n)
		{
			if (n is GraphDirectory)
			{
				return Resource.GraphDir;
			}
			TreeNodeBase modelNode = n.ModelNode;
			if (modelNode is TreeTableNode)
			{
				return Resource.GraphTable;
			}
			if (modelNode is TreeDocumentNode)
			{
				return Resource.GraphDoc;
			}
			if (modelNode is TreeImageNode)
			{
				return Resource.GraphImage;
			}
			if (modelNode is TreePdfNode)
			{
				return Resource.GraphPdf;
			}
			return null;
		}
		List<int> GetSubgraphDividers(IEnumerable<Rectangle> bounds)
		{
			List<int> list2 = new List<int> { 0 };
			List<int> list3 = new List<int>();
			foreach (Rectangle bound in bounds)
			{
				list3.Add(bound.Left);
				list3.Add(bound.Right);
			}
			list3.Sort();
			for (int k = 1; k < list3.Count - 1; k += 2)
			{
				list2.Add((list3[k] + list3[k + 1]) / 2);
			}
			list2.Add(Size.Width);
			return list2;
		}
		Bitmap RenderDot(string dot)
		{
			using Process process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					CreateNoWindow = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					FileName = ".\\graphviz\\dot.exe",
					Arguments = "-Tdot",
					UseShellExecute = false,
					WindowStyle = ProcessWindowStyle.Hidden
				}
			};
			process.Start();
			UTF8Encoding uTF8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
			using (Stream stream = process.StandardInput.BaseStream)
			{
				byte[] bytes = uTF8Encoding.GetBytes(dot);
				stream.Write(bytes, 0, bytes.Length);
			}
			ICharStream input = CharStreams.fromStream(process.StandardOutput.BaseStream);
			GvDotLexer tokenSource = new GvDotLexer(input);
			CommonTokenStream input2 = new CommonTokenStream(tokenSource);
			GvDotParser gvDotParser = new GvDotParser(input2);
			GvDotParser.GraphContext t2 = gvDotParser.graph();
			GetCoordsListener getCoordsListener = new GetCoordsListener();
			ParseTreeWalker.Default.Walk(getCoordsListener, t2);
			Size = getCoordsListener.GraphSize;
			foreach (Tuple<int, Point> nodePosition in getCoordsListener.NodePositions)
			{
				Nodes[nodePosition.Item1].Rect = new Rectangle(nodePosition.Item2, NodeSize);
			}
			RenderEdges.Clear();
			RenderEdges.AddRange(getCoordsListener.Edges.Select((Tuple<int, int, Point[]> t) => Tuple.Create(Nodes[t.Item1], Nodes[t.Item2], t.Item3)));
			Bitmap bitmap = new Bitmap(Size.Width, Size.Height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				using SolidBrush brush = new SolidBrush(ThemeColor);
				using Font font = new Font("microsoft yahei", 9f);
				using Font font2 = new Font("microsoft yahei", 10f, FontStyle.Bold);
				using Pen pen = new Pen(DarkColor)
				{
					CustomEndCap = Cap
				};
				graphics.DrawRectangle(Pens.Gray, new Rectangle(Point.Empty, bitmap.Size - new Size(1, 1)));
				List<int> subgraphDividers = GetSubgraphDividers(getCoordsListener.SubgraphBounds.Select((Tuple<int, Rectangle> tup) => tup.Item2));
				foreach (int item in subgraphDividers)
				{
					graphics.DrawLine(Pens.DarkGray, item, 0, item, 30);
				}
				foreach (Tuple<int, Point> nodePosition2 in getCoordsListener.NodePositions)
				{
					graphics.FillRectangle(brush, new Rectangle(nodePosition2.Item2, NodeSize));
					using Bitmap bitmap2 = GetNodeBitmap(Nodes[nodePosition2.Item1]);
					graphics.DrawImage(bitmap2, nodePosition2.Item2 + new Size((NodeSize.Width - bitmap2.Width) / 2, 20));
					if (Nodes[nodePosition2.Item1] is GraphDirectory d3)
					{
						using Bitmap bitmap3 = GetDirIcon(d3);
						graphics.DrawImage(bitmap3, nodePosition2.Item2 + new Size(NodeSize.Width - bitmap3.Width - 5, (NodeSize.Height - bitmap3.Height) / 2));
					}
					graphics.DrawString((ShowNumber ? (Nodes[nodePosition2.Item1].ModelNode.Number + " ") : "") + Nodes[nodePosition2.Item1].ModelNode.Name, font, Brushes.Black, new Rectangle(nodePosition2.Item2 + new Size(0, 45), NodeSize - new Size(0, 45)), _stringFormat);
				}
				foreach (Tuple<GraphNode, GraphNode, Point[]> renderEdge in RenderEdges)
				{
					graphics.DrawLines(pen, renderEdge.Item3);
				}
				graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
				foreach (Tuple<int, Rectangle> subgraphBound in getCoordsListener.SubgraphBounds)
				{
					Rectangle rectangle = Rectangle.Empty;
					for (int l = 0; l < subgraphDividers.Count - 1; l++)
					{
						if (subgraphDividers[l] <= subgraphBound.Item2.Left && subgraphBound.Item2.Right <= subgraphDividers[l + 1])
						{
							rectangle = new Rectangle(subgraphDividers[l], 0, subgraphDividers[l + 1] - subgraphDividers[l], 30);
							break;
						}
					}
					graphics.DrawString(Groups[subgraphBound.Item1].TreeGroup.Name, font2, Brushes.Black, rectangle, _stringFormat);
				}
				graphics.DrawLine(Pens.DarkGray, 0, 30, Size.Width, 30);
			}
			return bitmap;
		}
		static int SetGraphDirectoryExpandedHeight(GraphDirectory d)
		{
			int num = 0;
			if (d.IsExpanded)
			{
				foreach (GraphNode child5 in d.Children)
				{
					num++;
					if (child5 is GraphDirectory graphDirectoryExpandedHeight)
					{
						num += SetGraphDirectoryExpandedHeight(graphDirectoryExpandedHeight);
					}
				}
			}
			d.ExpandedHeight = num;
			return num;
		}
		void SetNodeAttribute(GraphNode n)
		{
			if (n is GraphDirectory graphDirectory4)
			{
				if (graphDirectory4.IsExpanded)
				{
					sb.AppendLine($"\"{graphDirectory4.Id}\" [shape=folder];");
					{
						foreach (GraphNode child6 in graphDirectory4.Children)
						{
							SetNodeAttribute(child6);
						}
						return;
					}
				}
				sb.AppendLine($"\"{graphDirectory4.Id}\" [style=filled,shape=folder];");
			}
			else
			{
				sb.AppendLine($"\"{n.Id}\" [style=filled,shape=box];");
			}
		}
		void Walk(GraphDirectory d)
		{
			sb.Append($"\"{d.Id}\"->");
			foreach (GraphNode child7 in d.Children)
			{
				sb.Append($"\"{child7.Id}\"->");
				if (child7 is GraphDirectory { IsExpanded: not false } graphDirectory5)
				{
					for (int m = 0; m < graphDirectory5.ExpandedHeight; m++)
					{
						hiddenNodeIndex++;
						sb.Append($"_hidden{hiddenNodeIndex}->");
					}
				}
			}
			sb.Remove(sb.Length - 2, 2);
			sb.AppendLine(";");
			foreach (GraphNode child8 in d.Children)
			{
				if (child8 is GraphDirectory { IsExpanded: not false } graphDirectory6)
				{
					Walk(graphDirectory6);
				}
			}
		}
	}
}
