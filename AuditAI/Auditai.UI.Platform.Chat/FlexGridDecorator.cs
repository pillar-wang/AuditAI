using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using C1.Win.C1FlexGrid;
using Auditai.DTO;
using Auditai.Model;
using Auditai.SignalR;
using Auditai.UI.CommonControls;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform.Chat;

internal class FlexGridDecorator
{
	public const int REPAINT_USER_HEADER_ICON_MAX_CELL_COUNT = 10000;

	public const int USER_HEADER_ICON_MAX_COUNT = 10;

	private readonly C1FlexGrid _grid;

	private Auditai.Model.Table _table;

	private bool _isDirty = true;

	private readonly CellDrawCollection DrawCollection = new CellDrawCollection();

	public bool Enable { get; set; } = true;


	public FlexGridDecorator(C1FlexGrid grid)
	{
		_grid = grid;
		_grid.OwnerDrawCell += Grid_OwnerDrawCell;
		SecondTrigger.Trigger.Tick += Trigger_Elapsed;
	}

	public void SetTable(Auditai.Model.Table table)
	{
		_table = table;
	}

	public void SetDirty()
	{
		_isDirty = true;
	}

	public void Prepare()
	{
		if (!_isDirty)
		{
			return;
		}
		DrawCollection.Clear();
		if (_table == null || _table.Cells.Count > 10000)
		{
			return;
		}
		UserState myState = SignalRClient.UserState;
		MemberManager instance = MemberManager.GetInstance();
		List<Member> list = (from m in instance.GetGroup(SignalRClient.UserState.ProjectId)?.Members()
			where m.UserState != null && m.UserState.ProjectId == myState.ProjectId && m.UserState.TreeNodeId == myState.TreeNodeId
			select m).ToList();
		if (list == null)
		{
			return;
		}
		foreach (Member item in list)
		{
			if (!long.TryParse(item.UserState.TableCellId, out var result) || item.Id == Auditai.Model.User.Current.Id.ToString())
			{
				continue;
			}
			if (DrawCollection.Values.Count >= 10)
			{
				break;
			}
			try
			{
				Auditai.Model.Cell cellById = _table.GetCellById(new Id64(result));
				if (cellById != null)
				{
					System.Drawing.Image orGenerateImage = item.GetOrGenerateImage16();
					DrawCollection.Add(cellById, orGenerateImage);
				}
			}
			catch (InvalidOperationException)
			{
			}
		}
		_isDirty = false;
	}

	private void Trigger_Elapsed(object sender, EventArgs e)
	{
		if (!Enable || _table == null)
		{
			return;
		}
		if (!_grid.Visible)
		{
			SetDirty();
		}
		else
		{
			if (Program.MainForm.IsInSyncingProject)
			{
				return;
			}
			try
			{
				Prepare();
				if (DrawCollection.IsEmpty)
				{
					return;
				}
				int @fixed = _grid.Rows.Fixed;
				int fixed2 = _grid.Cols.Fixed;
				foreach (Auditai.Model.Cell key in DrawCollection.Values.Keys)
				{
					int row = key.Row.Index + @fixed;
					int col = key.Column.Index + fixed2;
					_grid.Invalidate(row, col);
				}
			}
			catch (Exception exception)
			{
				exception.Log();
			}
		}
	}

	private void Grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		try
		{
			if (_table == null)
			{
				return;
			}
			int num = e.Row - _grid.Rows.Fixed;
			int num2 = e.Col - _grid.Cols.Fixed;
			if (num < 0 || num2 < 0 || num >= _grid.Rows.Count || num2 >= _grid.Cols.Count || num >= _table.Rows.Count || num2 >= _table.Columns.Count)
			{
				return;
			}
			Auditai.Model.Cell cell = _table.Cells.Get(num, num2);
			System.Drawing.Image image = DrawCollection[cell];
			if (image != null)
			{
				if (SecondTrigger.Display)
				{
					int height = _grid.Rows[e.Row].Height;
					int y = e.Bounds.Y;
					y += ((height > image.Height) ? ((height - image.Height) / 2) : 0);
					e.Graphics.DrawImage(image, e.Bounds.X, y);
				}
				else
				{
					int height2 = _grid.Rows[e.Row].Height;
					int y2 = e.Bounds.Y;
					y2 += ((height2 > image.Height) ? ((height2 - image.Height) / 2) : 0);
					e.Graphics.DrawImage(Resources.Empty16, e.Bounds.X, y2);
				}
			}
		}
		catch (Exception exception)
		{
			exception.Log();
		}
	}
}
