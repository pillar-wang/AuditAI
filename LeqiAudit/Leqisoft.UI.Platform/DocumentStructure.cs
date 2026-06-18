﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using TXTextControl;

namespace Leqisoft.UI.Platform;

public class DocumentStructure : ISetTheme
{
	private readonly DocumentEditor _de;

	private readonly TextControl _tx;

	private readonly C1ContextMenu ctx = new C1ContextMenu();

	private readonly C1Command cmdExpandAll = new C1Command();

	private readonly C1CommandLink lnkExpandAll;

	private readonly C1Command cmdCollapseAll = new C1Command();

	private readonly C1CommandLink lnkCollapseAll;

	private readonly C1Command cmdRefresh = new C1Command();

	private readonly C1CommandLink lnkRefresh;

	private readonly C1Command cmdRefreshDocumentAll = new C1Command();

	private readonly C1CommandLink lnkRefreshDocumentAll;

	private readonly C1Command cmdRefreshAllTables = new C1Command();

	private readonly C1CommandLink lnkRefreshAllTables;

	private readonly C1Command cmdAutoNumber = new C1Command();

	private readonly C1CommandLink lnkAutoNumber;

	private readonly LinkLabel _linkLabel = new LinkLabel
	{
		Text = "点击此处生成文档结构图",
		Dock = DockStyle.Fill,
		TextAlign = ContentAlignment.MiddleCenter
	};

	/// <summary>范围选择模式状态</summary>
	private enum RangeSelectionMode
	{
		None,
		SelectingStart,
		SelectingEnd,
		Selected
	}

	/// <summary>当前范围选择模式</summary>
	private RangeSelectionMode _rangeSelectionMode = RangeSelectionMode.None;

	/// <summary>起始节点 Key</summary>
	private object _rangeStartKey;

	/// <summary>结束节点 Key</summary>
	private object _rangeEndKey;

	/// <summary>范围选择完成事件</summary>
	public event Action<int, int> RangeSelected;

	public C1FlexGrid View { get; }

	public bool IsBlank => View.Rows.Count == 0;

	public DocumentStructure(DocumentEditor de)
	{
		C1FlexGrid c1FlexGrid = new C1FlexGrid();
		c1FlexGrid.Dock = DockStyle.Fill;
		c1FlexGrid.Rows.Fixed = 0;
		c1FlexGrid.Rows.Count = 0;
		c1FlexGrid.Rows.DefaultSize = 30;
		c1FlexGrid.Cols.Fixed = 0;
		c1FlexGrid.Cols.Count = 1;
		c1FlexGrid.ExtendLastCol = true;
		c1FlexGrid.AllowEditing = false;
		c1FlexGrid.SelectionMode = SelectionModeEnum.Row;
		c1FlexGrid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		c1FlexGrid.FocusRect = FocusRectEnum.None;
		c1FlexGrid.Tree.Style = TreeStyleFlags.Symbols;
		c1FlexGrid.Styles.Normal.Border.Width = 0;
		c1FlexGrid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
		View = c1FlexGrid;
		_de = de;
		_tx = de._tx;
		View.Tree.Column = 0;
		View.Cols[0].TextAlign = TextAlignEnum.LeftCenter;
		View.MouseClick += View_MouseClick;
		View.OwnerDrawCell += View_CellPaint;
		cmdExpandAll.CommandStateQuery += CmdExpandAll_CommandStateQuery;
		cmdExpandAll.Click += CmdExpandAll_Click;
		lnkExpandAll = new C1CommandLink(cmdExpandAll);
		ctx.CommandLinks.Add(lnkExpandAll);
		cmdCollapseAll.CommandStateQuery += CmdCollapseAll_CommandStateQuery;
		cmdCollapseAll.Click += CmdCollapseAll_Click;
		lnkCollapseAll = new C1CommandLink(cmdCollapseAll);
		ctx.CommandLinks.Add(lnkCollapseAll);
		cmdRefresh.CommandStateQuery += CmdRefresh_CommandStateQuery;
		cmdRefresh.Click += CmdRefresh_Click;
		lnkRefresh = new C1CommandLink(cmdRefresh);
		ctx.CommandLinks.Add(lnkRefresh);
		cmdRefreshDocumentAll.CommandStateQuery += CmdRefreshDocumentAll_CommandStateQuery;
		cmdRefreshDocumentAll.Click += CmdRefreshDocumentAll_Click;
		lnkRefreshDocumentAll = new C1CommandLink(cmdRefreshDocumentAll);
		ctx.CommandLinks.Add(lnkRefreshDocumentAll);
		cmdRefreshAllTables.CommandStateQuery += CmdRefreshAllTables_CommandStateQuery;
		cmdRefreshAllTables.Click += CmdRefreshAllTables_Click;
		lnkRefreshAllTables = new C1CommandLink(cmdRefreshAllTables);
		ctx.CommandLinks.Add(lnkRefreshAllTables);
		cmdAutoNumber.CommandStateQuery += CmdAutoNumber_CommandStateQuery;
		cmdAutoNumber.Click += CmdAutoNumber_Click;
		lnkAutoNumber = new C1CommandLink(cmdAutoNumber);
		ctx.CommandLinks.Add(lnkAutoNumber);
		_linkLabel.LinkClicked += _linkLabel_LinkClicked;
		View.Controls.Add(_linkLabel);
		View.MouseDoubleClick += View_MouseDoubleClick;
		Theme.SetCurrentTree(View);
		SetTheme();
		ThemeManager.GetInstance().Register(this);
	}

	public async Task Populate()
	{
		int selStart = _tx.Selection.Start;
		int selLen = _tx.Selection.Length;
		View.BeginUpdate();
		View.Rows.Count = 0;
		int pc = ((dynamic)_tx).Paragraphs.Count;
		List<NumberingHelper.Numbering> list = new List<NumberingHelper.Numbering>();
		ProgressForm<object> progressForm = new ProgressForm<object>(delegate(IProgress<ProgressInfo> iProg)
		{
			try
			{
				_de.DetachEvents();
				for (int j = 1; j <= pc; j++)
				{
					if (j % 50 == 0)
					{
						iProg.Report(new ProgressInfo
						{
							MainCaption = $"正在生成文档结构图... ({j}/{pc})",
							MainProgress = (int)((double)j * 100.0 / (double)pc)
						});
						Application.DoEvents();
					}
					var paragraph = ((dynamic)_tx).Paragraphs[j];
					_tx.Select(paragraph.Start - 1, 0);
					if (_tx.Tables.GetItem() == null)
					{
						var item = ((dynamic)_tx).DocumentTargets.GetItem();
						string text = paragraph.Text;
						NumberingHelper.Numbering i = NumberingHelper.Matches(text);
						if (i != null)
						{
							NumberingHelper.Numbering numbering = list.FirstOrDefault((NumberingHelper.Numbering nb) => nb.Series == i.Series);
							if (numbering == null)
							{
								list.RemoveAll((NumberingHelper.Numbering lessPrior) => lessPrior.Priority > i.Priority);
								list.Add(i);
								Node node = View.Rows.AddNode(list.Count - 1);
								node.Data = text;
								node.Key = item ?? new DocumentTargetPosition(paragraph.Start);
							}
							else
							{
								int num = list.IndexOf(numbering);
								list.RemoveRange(num + 1, list.Count - num - 1);
								numbering.Number = i.Number;
								Node node2 = View.Rows.AddNode(num);
								node2.Data = text;
								node2.Key = item ?? new DocumentTargetPosition(paragraph.Start);
							}
						}
					}
				}
			}
			finally
			{
				_de.AttachEvents();
			}
			View.Tree.Show(0);
			return Task.FromResult<object>(null);
		});
		progressForm.ShowDialog();
		await progressForm.Task;
		View.EndUpdate();
		_tx.Select(selStart, selLen);
	}

	public int AutoNumber()
	{
		int pc = ((dynamic)_tx).Paragraphs.Count;
		List<NumberingHelper.Numbering> list = new List<NumberingHelper.Numbering>();
		List<AutoNumberChange> changes = new List<AutoNumberChange>();
		try
		{
			_de.DetachEvents();
			for (int j = 1; j <= pc; j++)
			{
				var paragraph = ((dynamic)_tx).Paragraphs[j];
				_tx.Select(paragraph.Start - 1, 0);
				if (_tx.Tables.GetItem() != null)
				{
					continue;
				}
				string text = paragraph.Text;
				NumberingHelper.Numbering i = NumberingHelper.Matches(text);
				if (i == null)
				{
					continue;
				}
				int originalNumber = i.Number;
				int expectedNumber;
				NumberingHelper.Numbering numbering = list.FirstOrDefault(nb => nb.Series == i.Series);
				if (numbering == null)
				{
					// 新系列：从 1 开始
					list.RemoveAll(lessPrior => lessPrior.Priority > i.Priority);
					expectedNumber = 1;
					i.Number = expectedNumber;
					list.Add(i);
				}
				else
				{
					// 同系列再次出现：上次期望编号 + 1
					int num = list.IndexOf(numbering);
					list.RemoveRange(num + 1, list.Count - num - 1);
					expectedNumber = numbering.Number + 1;
					numbering.Number = expectedNumber;
				}
				if (expectedNumber != originalNumber)
				{
					string oldPrefix = new NumberingHelper.Numbering { Series = i.Series, Number = originalNumber }.ToString();
					string newPrefix = new NumberingHelper.Numbering { Series = i.Series, Number = expectedNumber }.ToString();
					int prefixIndex = text.IndexOf(oldPrefix);
					if (prefixIndex >= 0)
					{
						changes.Add(new AutoNumberChange { Start = paragraph.Start, PrefixIndex = prefixIndex, OldLen = oldPrefix.Length, NewPrefix = newPrefix });
					}
				}
			}
			// 逆序应用修改，保证前面的段落位置不变
			// 使用 BeginUndoAction/EndUndoAction 将所有替换合并为一次撤销
			_tx.BeginUndoAction("自动编号");
			try
			{
				for (int k = changes.Count - 1; k >= 0; k--)
				{
					AutoNumberChange change = changes[k];
					int selStart = change.Start - 1 + change.PrefixIndex;
					_tx.Select(selStart, change.OldLen);
					_tx.Selection.Text = change.NewPrefix;
				}
			}
			finally
			{
				_tx.EndUndoAction();
			}
		}
		finally
		{
			_de.AttachEvents();
		}
		return changes.Count;
	}

	private class AutoNumberChange
	{
		public int Start { get; set; }
		public int PrefixIndex { get; set; }
		public int OldLen { get; set; }
		public string NewPrefix { get; set; }
	}

	private class DocumentTargetPosition
	{
		public int Start { get; }
		public DocumentTargetPosition(int start) => Start = start;
	}

	private async void CmdRefresh_Click(object sender, ClickEventArgs e)
	{
		_de.MakeIds();
		await Populate();
	}

	private void CmdRefresh_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdRefresh.Text = "刷新";
	}

	private void CmdRefreshDocumentAll_Click(object sender, ClickEventArgs e)
	{
		try
		{
			// 全文刷新：所有表格 + 所有域
			_de.RefreshDocumentAll();
			// 刷新完成后同步更新文档结构图
			_de.MakeIds();
			_ = Populate();
		}
		catch (Exception ex) { ex.Log("DocumentStructure.CmdRefreshDocumentAll_Click"); }
	}

	private void CmdRefreshDocumentAll_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdRefreshDocumentAll.Text = "全文刷新";
	}

	private void CmdRefreshAllTables_Click(object sender, ClickEventArgs e)
	{
		try
		{
			// 全表刷新：仅刷新所有表格（带格式，从源数据重新加载）
			_de.RefreshAllTables();
			// 刷新完成后同步更新文档结构图
			_de.MakeIds();
			_ = Populate();
		}
		catch (Exception ex) { ex.Log("DocumentStructure.CmdRefreshAllTables_Click"); }
	}

	private void CmdRefreshAllTables_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdRefreshAllTables.Text = "全表刷新";
	}

	private async void CmdAutoNumber_Click(object sender, ClickEventArgs e)
	{
		try
		{
			if (System.Windows.Forms.MessageBox.Show("将自动重排文档中所有编号，是否继续？", "自动编号",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			{
				return;
			}
			int changed = AutoNumber();
			if (changed == 0)
			{
				System.Windows.Forms.MessageBox.Show("编号已正确，无需调整。", "自动编号",
					MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				// 修改文本后需要重新生成书签再刷新结构图
				_de.MakeIds();
				await Populate();
			}
		}
		catch (Exception ex)
		{
			ex.Log("DocumentStructure.CmdAutoNumber_Click");
		}
	}

	private void CmdAutoNumber_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdAutoNumber.Text = "自动编号";
	}

	private void CmdCollapseAll_Click(object sender, ClickEventArgs e)
	{
		View.Tree.Show(0);
	}

	private void CmdCollapseAll_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdCollapseAll.Text = "全部收缩";
	}

	private void CmdExpandAll_Click(object sender, ClickEventArgs e)
	{
		View.Tree.Show(View.Tree.MaximumLevel);
	}

	private void CmdExpandAll_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdExpandAll.Text = "全部展开";
	}

	private void View_MouseClick(object sender, MouseEventArgs e)
	{
		// 在范围选择模式下，不执行原有的折叠/展开逻辑
		if (_rangeSelectionMode != RangeSelectionMode.None)
		{
			HandleRangeSelectionClick(e);
			return;
		}
		HitTestInfo hitTestInfo = View.HitTest(e.Location);
		if (e.Button == MouseButtons.Left)
		{
			if (hitTestInfo.Type != HitTestTypeEnum.Cell)
			{
				return;
			}
			C1.Win.C1FlexGrid.Row row = View.Rows[hitTestInfo.Row];
			Node node = row.Node;
			node.Collapsed = !node.Collapsed;
			try
			{
				JumpToPosition(node.Key);
			}
			catch
			{
			}
			return;
		}
		if (e.Button == MouseButtons.Right)
		{
			ctx.ShowContextMenu(View, e.Location);
		}
	}

	private void View_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		try
		{
			HitTestInfo hitTestInfo = View.HitTest(e.Location);
			if (hitTestInfo.Type != HitTestTypeEnum.Cell) return;
			C1.Win.C1FlexGrid.Row row = View.Rows[hitTestInfo.Row];
			Node node = row.Node;
			JumpToPosition(node.Key);
		}
		catch
		{
		}
	}

	private void JumpToPosition(object key)
	{
		try
		{
			int start = -1;
			if (key is DocumentTarget dt)
			{
				start = dt.Start;
			}
			else if (key is DocumentTargetPosition dtp)
			{
				start = dtp.Start;
			}

			if (start >= 0)
			{
				_tx.Select(start - 1, 0);
				_tx.ScrollLocation = _tx.InputPosition.Location;
				_tx.Focus();
			}
		}
		catch
		{
		}
	}

	/// <summary>
	/// 进入范围选择模式，提示用户点击选择起始和结束位置
	/// </summary>
	public void EnterRangeSelectionMode()
	{
		_rangeSelectionMode = RangeSelectionMode.SelectingStart;
		_rangeStartKey = null;
		_rangeEndKey = null;
		// 刷新视图以显示提示
		View.Focus();
	}

	/// <summary>
	/// 退出范围选择模式
	/// </summary>
	public void ExitRangeSelectionMode()
	{
		_rangeSelectionMode = RangeSelectionMode.None;
		_rangeStartKey = null;
		_rangeEndKey = null;
		View.Invalidate();
	}

	/// <summary>
	/// 处理范围选择模式下的点击事件
	/// </summary>
	private void HandleRangeSelectionClick(MouseEventArgs e)
	{
		// 获取点击位置的节点
		var hitTest = View.HitTest(e.Location);
		if (hitTest.Row < 0 || hitTest.Row >= View.Rows.Count)
			return;

		var row = View.Rows[hitTest.Row];
		if (row == null) return;
		var nodeKey = row.UserData;

		if (_rangeSelectionMode == RangeSelectionMode.SelectingStart)
		{
			_rangeStartKey = nodeKey;
			_rangeSelectionMode = RangeSelectionMode.SelectingEnd;
			// 刷新高亮
			View.Invalidate();
		}
		else if (_rangeSelectionMode == RangeSelectionMode.SelectingEnd)
		{
			_rangeEndKey = nodeKey;
			_rangeSelectionMode = RangeSelectionMode.Selected;

			// 获取起止位置
			int startPos = GetPositionFromKey(_rangeStartKey);
			int endPos = GetPositionFromKey(_rangeEndKey);

			// 自动交换顺序
			if (startPos > endPos)
			{
				var temp = startPos;
				startPos = endPos;
				endPos = temp;
			}

			// 触发事件
			RangeSelected?.Invoke(startPos, endPos);

			// 退出范围选择模式
			_rangeSelectionMode = RangeSelectionMode.None;
			View.Invalidate();
		}
	}

	/// <summary>
	/// 从节点 Key 获取文档位置
	/// </summary>
	private int GetPositionFromKey(object key)
	{
		if (key is TXTextControl.DocumentTarget dt)
			return dt.Start;
		if (key is DocumentTargetPosition dtp)
			return dtp.Start;
		return -1;
	}

	/// <summary>
	/// 获取指定范围内的所有表格书签信息
	/// </summary>
	/// <param name="startPos">起始位置</param>
	/// <param name="endPos">结束位置</param>
	/// <returns>范围内的表格书签列表（位置, LeqiBookmark）</returns>
	public List<(int Position, LeqiBookmark Bookmark)> GetTablesInRange(int startPos, int endPos)
	{
		var result = new List<(int, LeqiBookmark)>();
		var tx = _tx; // TextControl 实例

		if (tx == null) return result;

		foreach (TXTextControl.DocumentTarget dt in tx.DocumentTargets)
		{
			try
			{
				string targetName = dt.TargetName;
				if (!targetName.StartsWith("lsbm"))
					continue;

				if (LeqiBookmark.TryParse(targetName, out var bookmark))
				{
					// 只关注有 TableId 的书签（表格书签）
					if (string.IsNullOrEmpty(bookmark.TableId))
						continue;

					int pos = dt.Start;
					if (pos >= startPos && pos <= endPos)
					{
						result.Add((pos, bookmark));
					}
				}
			}
			catch { }
		}

		// 按位置排序
		result.Sort((a, b) => a.Item1.CompareTo(b.Item1));
		return result;
	}

	/// <summary>
	/// 在节点上绘制范围选择高亮
	/// </summary>
	private void View_CellPaint(object sender, C1.Win.C1FlexGrid.OwnerDrawCellEventArgs e)
	{
		if (_rangeSelectionMode == RangeSelectionMode.None) return;

		// 检查当前行是否为起止节点
		var row = View.Rows[e.Row];
		if (row == null) return;
		var key = row.UserData;

		bool isStart = key != null && key.Equals(_rangeStartKey);
		bool isEnd = key != null && key.Equals(_rangeEndKey);

		if (isStart || isEnd)
		{
			// 绘制高亮背景
			e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(150, 51, 153, 255)), e.Bounds);
			// 绘制标签
			string label = isStart ? "起" : "止";
			using (var brush = new SolidBrush(Color.White))
			using (var font = new Font("微软雅黑", 9, FontStyle.Bold))
			{
				var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
				e.Graphics.DrawString(label, font, brush, e.Bounds, sf);
			}
		}
	}

	private async void _linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		View.Controls.Remove(_linkLabel);
		_linkLabel.Dispose();
		await Populate();
	}

	public void SetTheme()
	{
		try
		{
			View.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
			View.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
			View.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			View.Styles.Normal.Border.Width = 0;
			View.Styles.Alternate.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			View.Styles.Alternate.Border.Width = 0;
			View.Styles.Fixed.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			View.Styles.Fixed.Border.Width = 0;
			View.Styles.Highlight.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			View.Styles.Highlight.Border.Width = 0;
			View.Styles.Focus.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			View.Styles.Focus.Border.Width = 0;
			Color desktopColor = _de._tx.DisplayColors.DesktopColor;
			View.Styles.Normal.BackColor = desktopColor;
			View.Styles.Alternate.BackColor = desktopColor;
			View.Styles.EmptyArea.BackColor = desktopColor;
			View.Styles.EmptyArea.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			View.Styles.EmptyArea.Border.Width = 0;
			_linkLabel.BackColor = desktopColor;
		}
		catch (Exception)
		{
		}
	}
}
