using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using Auditai.UI.CommonControls;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class MemberListEditor : ISetTheme
{
	private const string SIGN_OUTLINE = "(离线)";

	private const string SIGN_ONLINE = "(在线)";

	private const string CN_NAME = "name";

	private bool _attachEvent;

	private readonly C1FlexGrid _grid;

	private Timer _trigger;

	private FlickerManager _twinkleManager = new FlickerManager();

	public MemTab CurrentMemTab { get; private set; }

	public event EventHandler<MemTab> CurrentChanged;

	public MemberListEditor(C1FlexGrid grid)
	{
		_grid = grid;
		AttachEvent();
		_trigger = new Timer
		{
			Interval = 500
		};
		ThemeManager.GetInstance().Register(this);
	}

	public void SetFlickerTimer(Timer timer)
	{
		_trigger = timer;
	}

	public void Populate(IEnumerable<MemTab> memTabs)
	{
		DettachEvent();
		try
		{
			Initialize();
			if (memTabs == null)
			{
				return;
			}
			IEnumerable<MemTab> source = memTabs.Where((MemTab m) => m is Group);
			foreach (MemTab item in source.OrderBy((MemTab m) => m.Id))
			{
				appendMember(item);
			}
			IEnumerable<MemTab> source2 = memTabs.Where((MemTab m) => m is Member);
			foreach (MemTab item2 in source2.OrderBy((MemTab m) => !(m as Member).IsOnline))
			{
				appendMember(item2);
			}
		}
		finally
		{
			AttachEvent();
		}
		if (_grid.Rows.Count > 0)
		{
			ChatWith((_grid.Rows[0].UserData as MemTab).Id);
		}
		void appendMember(MemTab memTab)
		{
			CellStyle cellStyle = _grid.Styles.Add("imageCell");
			cellStyle.TextAlign = TextAlignEnum.LeftCenter;
			cellStyle.ImageAlign = ImageAlignEnum.LeftCenter;
			Row row = _grid.Rows.Add();
			row.UserData = memTab;
			_grid.SetCellStyle(row.Index, 0, cellStyle);
			RowFlickerProxy rowFlickerProxy = new RowFlickerProxy(row, contentFlick: false);
			rowFlickerProxy.SetTimer(_trigger);
			if (!(memTab is Group))
			{
				if (memTab is Member member)
				{
					if (member.IsOnline)
					{
						row["name"] = memTab.Name + "(在线)";
						rowFlickerProxy.UpdateOrignImage(memTab.Image);
						rowFlickerProxy.UpdateTwinkleImage(memTab.Image);
						rowFlickerProxy.UpdateEmptyImage(Resources.Empty32);
						_grid.SetCellImage(row.Index, "name", memTab.Image);
					}
					else
					{
						row["name"] = memTab.Name + "(离线)";
						rowFlickerProxy.UpdateOrignImage(memTab.GrayImage);
						rowFlickerProxy.UpdateTwinkleImage(memTab.GrayImage);
						rowFlickerProxy.UpdateEmptyImage(Resources.Empty32);
						_grid.SetCellImage(row.Index, "name", memTab.GrayImage);
					}
				}
			}
			else
			{
				row["name"] = memTab.Name;
				rowFlickerProxy.UpdateOrignImage(memTab.Image);
				rowFlickerProxy.UpdateTwinkleImage(memTab.Image);
				rowFlickerProxy.UpdateEmptyImage(Resources.Empty32);
				_grid.SetCellImage(row.Index, "name", memTab.Image);
			}
			_twinkleManager.Add(row, rowFlickerProxy);
		}
	}

	public bool ChatWith(string userId)
	{
		DettachEvent();
		try
		{
			if (userId == null)
			{
				return false;
			}
			Row row = FindRowById(userId);
			if (row == null)
			{
				return false;
			}
			_grid.Row = row.Index;
			_twinkleManager.Stop(row);
			CurrentMemTab = row.UserData as MemTab;
			this.CurrentChanged?.Invoke(this, CurrentMemTab);
			return true;
		}
		finally
		{
			AttachEvent();
		}
	}

	public bool AnyFlicker()
	{
		return _twinkleManager.Any((AbstractFlickerProxy t) => t.Status());
	}

	public bool StartFlicker(string memId)
	{
		if (memId == null)
		{
			return false;
		}
		Row key = FindRowById(memId);
		return _twinkleManager.Start(key);
	}

	public bool StopFlicker(string memId)
	{
		if (memId == null)
		{
			return false;
		}
		Row key = FindRowById(memId);
		return _twinkleManager.Stop(key);
	}

	public void UpdateOnlineStatus(string userId, bool online)
	{
		Row row = FindRowById(userId);
		if (row != null && row.UserData is Member member)
		{
			if (online)
			{
				row["name"] = member.Name + "(在线)";
				_grid.SetCellImage(row.Index, "name", member.Image);
				_twinkleManager.Get(row).UpdateOrignImage(member.Image);
				_twinkleManager.Get(row).UpdateTwinkleImage(member.Image);
			}
			else
			{
				row["name"] = member.Name + "(离线)";
				_grid.SetCellImage(row.Index, "name", member.GrayImage);
				_twinkleManager.Get(row).UpdateOrignImage(member.GrayImage);
				_twinkleManager.Get(row).UpdateTwinkleImage(member.GrayImage);
			}
		}
	}

	public void SetTheme()
	{
		_grid.BeginUpdate();
		try
		{
			_grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
			_grid.SelectionMode = SelectionModeEnum.Row;
			_grid.FocusRect = FocusRectEnum.None;
			_grid.Dock = DockStyle.Fill;
			_grid.ExtendLastCol = true;
			_grid.AllowEditing = false;
			_grid.Rows.DefaultSize = 40;
			_grid.Styles.EmptyArea.BackColor = Color.White;
			_grid.Styles.EmptyArea.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			_grid.Styles.Normal.BackColor = Color.White;
			_grid.Styles.Normal.Border.Color = Color.Transparent;
			_grid.Styles.Normal.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			_grid.Styles.Highlight.ForeColor = Color.Black;
			_grid.Styles.Highlight.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			_grid.Styles.Focus.ForeColor = Color.Black;
			_grid.Styles.Focus.Border.Style = C1.Win.C1FlexGrid.BorderStyleEnum.None;
			_grid.Styles.SelectedColumnHeader.Clear();
		}
		finally
		{
			_grid.EndUpdate();
		}
	}

	private void View_AfterRowColChange(object sender, RangeEventArgs e)
	{
		int topRow = e.NewRange.TopRow;
		if (topRow >= _grid.Rows.Fixed && topRow < _grid.Rows.Count && _grid.Rows[topRow].UserData is MemTab currentMemTab)
		{
			CurrentMemTab = currentMemTab;
			_twinkleManager.Stop(topRow);
			this.CurrentChanged?.Invoke(this, CurrentMemTab);
		}
	}

	private void Initialize()
	{
		_twinkleManager.Dispose();
		_twinkleManager = new FlickerManager();
		_grid.Rows.Count = 0;
		_grid.Cols.Count = 1;
		_grid.Cols[0].Name = "name";
		SetTheme();
	}

	private void AttachEvent()
	{
		if (!_attachEvent)
		{
			_grid.AfterRowColChange += View_AfterRowColChange;
			_attachEvent = true;
		}
	}

	private void DettachEvent()
	{
		if (_attachEvent)
		{
			_grid.AfterRowColChange -= View_AfterRowColChange;
			_attachEvent = false;
		}
	}

	private Row FindRowById(string memId)
	{
		foreach (Row item in (IEnumerable)_grid.Rows)
		{
			if (item.UserData is MemTab memTab && memTab.Id == memId)
			{
				return item;
			}
		}
		return null;
	}
}
