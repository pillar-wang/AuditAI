using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Leqisoft.Model;
using Leqisoft.SignalR;
using Leqisoft.UI.Controls;

namespace Leqisoft.UI.Platform;

public class FormulaMap : ISetTheme
{
	public class SelectablePictureBox : PictureBox
	{
		public SelectablePictureBox()
		{
			SetStyle(ControlStyles.Selectable, value: true);
			base.TabStop = true;
		}
	}

	private FormulaGraph _graph;

	private Rectangle? _highlightNode;

	private Point[] _highlightLine;

	private List<Point[]> _highlightNodeLines = new List<Point[]>();

	private bool _flip;

	private Timer _timer = new Timer
	{
		Interval = 500
	};

	private List<Tuple<System.Drawing.Image, Point>> _avatars = new List<Tuple<System.Drawing.Image, Point>>();

	private SelectablePictureBox _pb = new SelectablePictureBox
	{
		SizeMode = PictureBoxSizeMode.AutoSize
	};

	private static readonly Pen _pen = new Pen(Color.Red)
	{
		CustomEndCap = FormulaGraph.Cap
	};

	public Panel View { get; } = new Panel
	{
		Dock = DockStyle.Fill,
		AutoScroll = true,
		Visible = false
	};


	public bool Used => _pb.Image != null;

	public FormulaMap()
	{
		View.Controls.Add(_pb);
		_pb.Paint += _pb_Paint;
		_pb.MouseMove += _pb_MouseMove;
		_pb.MouseClick += _pb_MouseClick;
		_pb.Disposed += _pb_Disposed;
		ThemeManager.GetInstance().Register(this);
		_timer.Tick += _timer_Tick;
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		_flip = !_flip;
		UserState myState = SignalRClient.UserState;
		Group group = MemberManager.GetInstance().GetGroup(SignalRClient.UserState.ProjectId);
		if (group == null)
		{
			return;
		}
		IEnumerable<Member> enumerable = from m in @group.Members()
			where m.UserState != null && m.UserState.ProjectId == myState.ProjectId && m.UserState.TreeNodeId != null && m.Id != User.Current.Id.ToString()
			select m;
		if (enumerable != null)
		{
			_avatars.Clear();
			foreach (Member item in enumerable)
			{
				long nId = long.Parse(item.UserState.TreeNodeId);
				GraphNode n2 = _graph.Nodes.FirstOrDefault((GraphNode n) => n.ModelNode.Id.Value == nId);
				n2 = GetRoot(n2);
				if (n2 != null && !n2.Rect.IsEmpty)
				{
					_avatars.Add(Tuple.Create(item.Image, n2.Rect.Location + new Size((FormulaGraph.NodeSize.Width - item.Image.Width) / 2, 20)));
				}
			}
		}
		_pb.Invalidate();
		static GraphNode GetRoot(GraphNode n)
		{
			if (n == null)
			{
				return null;
			}
			if (n.Parent == null)
			{
				return n;
			}
			if (n.Parent.IsExpanded)
			{
				return n;
			}
			return GetRoot(n.Parent);
		}
	}

	private void _pb_Disposed(object sender, EventArgs e)
	{
		using (_pb.Image)
		{
		}
	}

	private void _pb_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		GraphNode graphNode = _graph.Nodes.FirstOrDefault((GraphNode n) => n.Rect.Contains(e.Location));
		if (graphNode == null)
		{
			return;
		}
		if (graphNode is GraphDirectory graphDirectory)
		{
			graphDirectory.IsExpanded = !graphDirectory.IsExpanded;
			if (!graphDirectory.IsExpanded)
			{
				graphDirectory.CollapseAll();
			}
			DisposeAndSetImage(_graph.Render());
		}
		else
		{
			Program.MainForm.HideFormulaMap();
			Program.MainForm.ProjectHierarchy.FindAndSelectNode(graphNode.ModelNode);
		}
	}

	private void _pb_MouseMove(object sender, MouseEventArgs e)
	{
		GraphNode hoverNode = _graph.Nodes.FirstOrDefault((GraphNode n) => n.Rect.Contains(e.Location));
		_highlightNode = hoverNode?.Rect;
		_highlightNodeLines.Clear();
		if (hoverNode != null)
		{
			_highlightNodeLines.AddRange(from edge in _graph.RenderEdges
				where edge.Item1 == hoverNode || edge.Item2 == hoverNode
				select edge.Item3);
		}
		_highlightLine = _graph.RenderEdges.FirstOrDefault((Tuple<GraphNode, GraphNode, Point[]> edge) => PointOnLine(edge.Item3, e.Location))?.Item3;
		_pb.Cursor = ((_highlightNode.HasValue || _highlightLine != null) ? Cursors.Hand : Cursors.Default);
		_pb.Invalidate();
	}

	private void _pb_Paint(object sender, PaintEventArgs e)
	{
		if (_highlightNode.HasValue)
		{
			e.Graphics.DrawRectangle(Pens.Red, _highlightNode.Value);
		}
		if (_highlightLine != null)
		{
			e.Graphics.DrawLines(_pen, _highlightLine);
		}
		foreach (Point[] highlightNodeLine in _highlightNodeLines)
		{
			e.Graphics.DrawLines(_pen, highlightNodeLine);
		}
		if (_flip)
		{
			using (SolidBrush brush = new SolidBrush(_graph.ThemeColor))
			{
				foreach (Tuple<System.Drawing.Image, Point> avatar in _avatars)
				{
					e.Graphics.FillRectangle(brush, new Rectangle(avatar.Item2, avatar.Item1.Size));
					e.Graphics.DrawImage(avatar.Item1, avatar.Item2);
				}
				return;
			}
		}
		using SolidBrush brush2 = new SolidBrush(_graph.ThemeColor);
		foreach (Tuple<System.Drawing.Image, Point> avatar2 in _avatars)
		{
			e.Graphics.FillRectangle(brush2, new Rectangle(avatar2.Item2, avatar2.Item1.Size));
		}
	}

	public void Draw()
	{
		if (!Used)
		{
			_timer.Start();
		}
		_graph = Project.Current.FormulaManager.ToGraph(Theme.SelectedLeqiTheme.ThemeContext.GradientColor, Theme.SelectedLeqiTheme.ThemeContext.DarkColor, Program.MainForm.ProjectHierarchy.NumberShown);
		Bitmap image = _graph.Render();
		DisposeAndSetImage(image);
	}

	private void DisposeAndSetImage(Bitmap image)
	{
		_highlightNode = null;
		_highlightLine = null;
		using (_pb.Image)
		{
		}
		_pb.Image = image;
	}

	private bool PointOnLine(Point[] line, Point pt)
	{
		for (int i = 0; i < line.Length - 1; i++)
		{
			Point point = line[i];
			Point point2 = line[i + 1];
			if (point.X == point2.X)
			{
				if (new Rectangle(point.X - 2, Math.Min(point.Y, point2.Y), 5, Math.Abs(point.Y - point2.Y)).Contains(pt))
				{
					return true;
				}
			}
			else if (point.Y == point2.Y && new Rectangle(Math.Min(point.X, point2.X), point.Y - 2, Math.Abs(point.X - point2.X), 5).Contains(pt))
			{
				return true;
			}
		}
		return false;
	}

	public void SetTheme()
	{
		View.BackColor = SystemColors.Control;
		if (Used)
		{
			Draw();
		}
	}
}
