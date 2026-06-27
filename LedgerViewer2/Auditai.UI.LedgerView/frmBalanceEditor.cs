using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.DTO;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.LedgerView.Properties;

namespace Auditai.UI.LedgerView;

public class frmBalanceEditor : C1RibbonForm
{
	private delegate void InvokeDelegate();

	private const string CN_INDEX = "index";

	private const string CN_CODE = "kmdm";

	private const string CN_NAME = "kmmc";

	private const string CN_BEGIN_DEBIT = "debit";

	private const string CN_BEGIN_CREDIT = "credit";

	private const string FORMATSTRING_MONEY = "#,0.00;-#,0.00;#";

	private C1ContextMenu ctxCell = new C1ContextMenu();

	private C1CommandLink lnkCopy = new C1CommandLink();

	private C1Command cmdCopy = new C1Command();

	private C1CommandLink lnkPaste = new C1CommandLink();

	private C1Command cmdPaste = new C1Command();

	private C1CommandLink lnkOnlyLastLevel = new C1CommandLink();

	private C1Command cmdOnlyLastLevel = new C1Command();

	private C1ContextMenu ctxEmpty = new C1ContextMenu();

	private Ledger ledger;

	private Dictionary<Account, Dictionary<AuxiliaryItem, decimal>> auxOldValueCache;

	private RibbonImageProcess imageProcess = new RibbonImageProcess();

	private bool onlyDisplayLastLevel;

	private object modifyUserData;

	private LedgerViewer _owner;

	private TableFindFactory tableReplaceFactory = new TableFindFactory();

	private GridCommandsManager commandManager;

	private bool lockEvent;

	private bool _eventAttached;

#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	private C1SplitContainer c1SplitContainer1;

	private C1SplitterPanel pnlToolBar;

	private C1SplitterPanel pnlGrid;

	private C1FlexGridEx grdBalance;

	private C1CommandDock c1CommandDock1;

	private C1ToolBar c1ToolBar1;

	private C1CommandLink lnkBalanceValidate;

	private C1Command cmdBalanceValidate;

	private C1CommandLink lnkSaveData;

	private C1Command cmdSaveData;

	private C1CommandHolder c1CommandHolder1;

	private C1CommandLink lnkCancelSave;

	private C1Command cmdCancelSave;

	public List<Tuple<Account, AuxiliaryItem, decimal>> UpdateAuxiliary { get; private set; }

	public List<Tuple<Account, decimal>> UpdateAccount { get; private set; }

	public frmBalanceEditor(Ledger ledger, object modifyUserData, LedgerViewer owner)
	{
		_owner = owner;
		InitializeComponent();
		base.WindowState = FormWindowState.Maximized;
		base.StartPosition = FormStartPosition.CenterScreen;
		this.modifyUserData = modifyUserData;
		foreach (C1CommandLink commandLink in c1ToolBar1.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		this.ledger = ledger;
		Populate(onlyDisplayLastLevel);
		cmdCopy.Text = "复制";
		lnkCopy.Command = cmdCopy;
		cmdCopy.Click += delegate
		{
			grdBalance.Copy();
		};
		cmdCopy.Image = ContextResources.ctxCopy;
		ctxCell.CommandLinks.Add(lnkCopy);
		cmdPaste.Text = "粘贴";
		lnkPaste.Command = cmdPaste;
		cmdPaste.Click += delegate
		{
			grdBalance.Paste();
		};
		cmdPaste.Image = ContextResources.ctxPaste;
		ctxCell.CommandLinks.Add(lnkPaste);
		ctxCell.CommandLinks.Add(grdBalance.FilterManager.GenLnkFilter());
		ctxCell.CommandLinks.Add(grdBalance.FilterManager.GenLnkSelect());
		ctxCell.CommandLinks.Add(grdBalance.FilterManager.GenLnkCancelCurrentColumn());
		cmdOnlyLastLevel.Checked = onlyDisplayLastLevel;
		cmdOnlyLastLevel.Text = "仅显示末级";
		cmdOnlyLastLevel.CheckAutoToggle = true;
		cmdOnlyLastLevel.Click += delegate
		{
			onlyDisplayLastLevel = cmdOnlyLastLevel.Checked;
			Populate(onlyDisplayLastLevel);
		};
		lnkOnlyLastLevel.Command = cmdOnlyLastLevel;
		ctxCell.CommandLinks.Add(lnkOnlyLastLevel);
		lnkOnlyLastLevel.Delimiter = true;
		ctxEmpty.CommandLinks.Add(grdBalance.FilterManager.GenLnkCancelAll());
		commandManager = new GridCommandsManager(grdBalance);
		grdBalance.MouseClick += GrdBalance_MouseClick;
		grdBalance.KeyDown += GrdBalance_KeyDown;
		AttachEvent();
		Auditai.UI.Controls.Theme.SetCurrentTree(this);
		SetTheme();
		grdBalance.Paint += delegate(object s1, PaintEventArgs e1)
		{
			Auditai.UI.Controls.Theme.DrawFormBorder(grdBalance, e1.Graphics);
		};
		base.Shown += FrmBalanceEditor_Shown;
	}

	private async void GrdBalance_KeyDown(object sender, KeyEventArgs e)
	{
		try
		{
			switch (e.KeyData)
			{
			case Keys.V | Keys.Control:
				try
				{
					await PasteClipboard(grdBalance);
					break;
				}
				catch (Exception exception)
				{
					exception.Log();
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "复制粘贴的数据存在异常，请尝试重新复制。");
					break;
				}
			case Keys.C | Keys.Control:
				grdBalance.Copy();
				break;
			case Keys.X | Keys.Control:
				CutSelection(grdBalance);
				break;
			case Keys.Delete:
				DeleteSelection(grdBalance);
				break;
			case Keys.Z | Keys.Control:
				if (commandManager.CanUndo)
				{
					commandManager.Undo();
				}
				break;
			case Keys.Y | Keys.Control:
				if (commandManager.CanRedo)
				{
					commandManager.Redo();
				}
				break;
			case Keys.F | Keys.Control:
				ShowReplaceForm(replace: false);
				break;
			case Keys.H | Keys.Control:
				ShowReplaceForm(replace: true);
				break;
			}
		}
		catch (Exception exception2)
		{
			exception2.Log();
		}
	}

	private void ShowReplaceForm(bool replace)
	{
		TableFindInstance tableFindInstance = tableReplaceFactory.Get();
		tableFindInstance.HideScopeControls();
		tableFindInstance.FindNextHandler += Frf_FindNextHandler;
		tableFindInstance.ReplaceHandler += Frf_ReplaceHandler;
		if (replace)
		{
			tableFindInstance.ShowReplace();
		}
		else
		{
			tableFindInstance.ShowFind();
		}
	}

	private void Frf_FindNextHandler(object sender, FindNextEventArgs e)
	{
		if (grdBalance.BodyRow < 0 || grdBalance.BodyCol < 0)
		{
			return;
		}
		C1.Win.C1FlexGrid.CellRange selection = grdBalance.Selection;
		if (selection.IsSingleCell)
		{
			int row = grdBalance.Row;
			int col = grdBalance.Col;
			for (int i = row; i < grdBalance.Rows.Count; i++)
			{
				if (!grdBalance.Rows[i].Visible)
				{
					continue;
				}
				for (int j = grdBalance.Cols.Fixed; j < grdBalance.Cols.Count; j++)
				{
					if (grdBalance.Cols[j].Visible && (i != row || j > col))
					{
						string text = grdBalance.GetData(i, j)?.ToString();
						if (text != null && FindImpl(text, e.FindValue, e.MatchMode, e.IsMatchCase))
						{
							grdBalance.Select(i, j);
							return;
						}
					}
				}
			}
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已查找到表格结尾");
			return;
		}
		for (int k = selection.TopRow; k <= selection.BottomRow; k++)
		{
			if (!grdBalance.Rows[k].Visible)
			{
				continue;
			}
			for (int l = selection.LeftCol; l <= selection.RightCol; l++)
			{
				if (grdBalance.Cols[l].Visible)
				{
					string text2 = grdBalance.GetData(k, l)?.ToString();
					if (text2 != null && FindImpl(text2, e.FindValue, e.MatchMode, e.IsMatchCase))
					{
						grdBalance.Select(k, l);
						return;
					}
				}
			}
		}
		Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前选区内无查找结果");
	}

	private void Frf_ReplaceHandler(object sender, ReplaceEventArgs e)
	{
		if (grdBalance == null || grdBalance.BodyRow < 0 || grdBalance.BodyCol < 0)
		{
			return;
		}
		grdBalance.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.CellRange selection = grdBalance.Selection;
			if (e.IsReplaceAll)
			{
				if (selection.IsSingleCell)
				{
					int num = 0;
					int row = grdBalance.Row;
					int col = grdBalance.Col;
					List<GridCellInfo> list = new List<GridCellInfo>();
					for (int i = row; i < grdBalance.Rows.Count; i++)
					{
						if (!grdBalance.Rows[i].Visible || !grdBalance.Rows[i].AllowEditing)
						{
							continue;
						}
						for (int j = grdBalance.Cols.Fixed; j < grdBalance.Cols.Count; j++)
						{
							if (!grdBalance.Cols[j].Visible || !grdBalance.Cols[j].AllowEditing || (i == row && j < col))
							{
								continue;
							}
							object data = grdBalance.GetData(i, j);
							if (data != null && FindImpl(data.ToString(), e.FindValue, e.MatchMode, e.IsMatchCase))
							{
								if (e.ReplaceMode == ReplaceMode.AllText)
								{
									string replaceValue = e.ReplaceValue;
									grdBalance.SetData(i, j, replaceValue);
									list.Add(new GridCellInfo(grdBalance, i, j, data, replaceValue));
								}
								else if (e.ReplaceMode == ReplaceMode.MatchText)
								{
									string text = data.ToString().Replace(e.FindValue, e.ReplaceValue);
									grdBalance.SetData(i, j, text);
									list.Add(new GridCellInfo(grdBalance, i, j, data, text));
								}
								num++;
							}
						}
					}
					if (list.Count > 0)
					{
						commandManager.NewCommand(new GridBatchCellUpdateValueCommand(list));
					}
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"共替换 {num} 处。");
					return;
				}
				int num2 = 0;
				for (int k = selection.TopRow; k <= selection.BottomRow; k++)
				{
					if (!grdBalance.Rows[k].Visible || !grdBalance.Rows[k].AllowEditing)
					{
						continue;
					}
					for (int l = selection.LeftCol; l <= selection.RightCol; l++)
					{
						if (!grdBalance.Cols[l].Visible || !grdBalance.Cols[l].AllowEditing)
						{
							continue;
						}
						string text2 = grdBalance.GetData(k, l)?.ToString();
						if (text2 != null && FindImpl(text2, e.FindValue, e.MatchMode, e.IsMatchCase))
						{
							if (e.ReplaceMode == ReplaceMode.AllText)
							{
								grdBalance.SetData(k, l, e.ReplaceValue);
							}
							else if (e.ReplaceMode == ReplaceMode.MatchText)
							{
								grdBalance.SetData(k, l, text2.Replace(e.FindValue, e.ReplaceValue));
							}
							num2++;
						}
					}
				}
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"共替换 {num2} 处。");
				return;
			}
			if (selection.IsSingleCell)
			{
				int row2 = grdBalance.Row;
				int col2 = grdBalance.Col;
				for (int m = row2; m < grdBalance.Rows.Count; m++)
				{
					if (!grdBalance.Rows[m].Visible)
					{
						continue;
					}
					for (int n = grdBalance.Cols.Fixed; n < grdBalance.Cols.Count; n++)
					{
						if (!grdBalance.Cols[n].Visible || (m == row2 && n <= col2))
						{
							continue;
						}
						object data2 = grdBalance.GetData(m, n);
						if (data2 != null && FindImpl(data2.ToString(), e.FindValue, e.MatchMode, e.IsMatchCase))
						{
							grdBalance.Select(m, n);
							if (e.ReplaceMode == ReplaceMode.AllText)
							{
								string replaceValue2 = e.ReplaceValue;
								grdBalance.SetData(m, n, replaceValue2);
								commandManager.NewCommand(new GridCellUpdateValueCommand(new GridCellInfo(grdBalance, m, n, data2, replaceValue2)));
							}
							else if (e.ReplaceMode == ReplaceMode.MatchText)
							{
								string text3 = data2.ToString().Replace(e.FindValue, e.ReplaceValue);
								grdBalance.SetData(m, n, text3);
								commandManager.NewCommand(new GridCellUpdateValueCommand(new GridCellInfo(grdBalance, m, n, data2, text3)));
							}
							return;
						}
					}
				}
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "已查找到表格结尾");
				return;
			}
			for (int num3 = selection.TopRow; num3 <= selection.BottomRow; num3++)
			{
				if (!grdBalance.Rows[num3].Visible)
				{
					continue;
				}
				for (int num4 = selection.LeftCol; num4 <= selection.RightCol; num4++)
				{
					if (!grdBalance.Cols[num4].Visible)
					{
						continue;
					}
					object data3 = grdBalance.GetData(num3, num4);
					if (data3 != null && FindImpl(data3.ToString(), e.FindValue, e.MatchMode, e.IsMatchCase))
					{
						grdBalance.Select(num3, num4);
						if (e.ReplaceMode == ReplaceMode.AllText)
						{
							string replaceValue3 = e.ReplaceValue;
							grdBalance.SetData(num3, num4, replaceValue3);
							commandManager.NewCommand(new GridCellUpdateValueCommand(new GridCellInfo(grdBalance, num3, num4, data3, replaceValue3)));
						}
						else if (e.ReplaceMode == ReplaceMode.MatchText)
						{
							string text4 = data3.ToString().Replace(e.FindValue, e.ReplaceValue);
							grdBalance.SetData(num3, num4, text4);
							commandManager.NewCommand(new GridCellUpdateValueCommand(new GridCellInfo(grdBalance, num3, num4, data3, text4)));
						}
						return;
					}
				}
			}
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "当前选区内无查找结果");
		}
		finally
		{
			grdBalance.EndUpdate();
		}
	}

	private bool FindImpl(string src, string findWhat, MatchMode mode, bool caseSensitive)
	{
		StringComparison comparisonType = (caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
		return mode switch
		{
			MatchMode.Exact => src.Equals(findWhat, comparisonType), 
			MatchMode.Start => src.StartsWith(findWhat, comparisonType), 
			MatchMode.End => src.EndsWith(findWhat, comparisonType), 
			MatchMode.Any => src.IndexOf(findWhat, comparisonType) >= 0, 
			_ => throw new ArgumentOutOfRangeException("mode"), 
		};
	}

	private async Task PasteClipboard(C1FlexGrid grid)
	{
		List<GridCellInfo> cellInfos = new List<GridCellInfo>();
		grid.BeginUpdate();
		try
		{
			ProgressForm<List<List<object>>> progressForm = new ProgressForm<List<List<object>>>(delegate
			{
				Application.DoEvents();
				return new ProgressInfo
				{
					MainCaption = "正在进行粘贴准备，可能时间较长，请耐心等待...",
					MainProgress = ((ClipboardUtil.IsStreamReady && ClipboardUtil.RowsCount > 0) ? ((int)((double)ClipboardUtil.RowsCountAlreadyRead * 100.0 / (double)ClipboardUtil.RowsCount)) : 0)
				};
			}, () => Task.Run(() => ClipboardUtil.GetClipboardAsTable()), TimeSpan.FromSeconds(1.0));
			progressForm.ShowDialog();
			List<List<object>> list = await progressForm.Task;
			if (list == null)
			{
				return;
			}
			if (list.Count == 1 && list[0].Count == 1 && !grid.Selection.IsSingleCell)
			{
				object item = list[0][0];
				list = new List<List<object>>();
				int num = 0;
				for (int i = grid.Selection.TopRow; i <= grid.Selection.BottomRow; i++)
				{
					if (grid.Rows[i].Visible)
					{
						num++;
					}
				}
				int num2 = 0;
				for (int j = grid.Selection.LeftCol; j <= grid.Selection.RightCol; j++)
				{
					if (grid.Cols[j].Visible)
					{
						num2++;
					}
				}
				for (int k = 0; k < num; k++)
				{
					List<object> list2 = new List<object>();
					for (int l = 0; l < num2; l++)
					{
						list2.Add(item);
					}
					list.Add(list2);
				}
			}
			int m = grid.Row;
			int col = grid.Col;
			foreach (List<object> item2 in list)
			{
				for (; m < grid.Rows.Count && !grid.Rows[m].Visible; m++)
				{
				}
				if (m >= grid.Rows.Count)
				{
					break;
				}
				if (!grid.Rows[m].AllowEditing)
				{
					continue;
				}
				int n = col;
				foreach (object item3 in item2)
				{
					for (; n < grid.Cols.Count && !grid.Cols[n].Visible; n++)
					{
					}
					if (n >= grid.Cols.Count)
					{
						break;
					}
					if (grid.Cols[n].AllowEditing)
					{
						object obj = item3;
						if (grid.Cols[n].DataType == typeof(decimal))
						{
							decimal result;
							decimal num3 = (decimal.TryParse(item3?.ToString(), out result) ? result : 0m);
							obj = Math.Round(num3, 2, MidpointRounding.AwayFromZero);
						}
						else if (grid.Cols[n].DataType == typeof(DateTime))
						{
							obj = ParseDate(item3);
						}
						object oldValue = grid[m, n];
						grid.SetData(m, n, obj);
						cellInfos.Add(new GridCellInfo(grid, m, n, oldValue, obj));
						n++;
					}
				}
				m++;
			}
			commandManager.NewCommand(new GridBatchCellUpdateValueCommand(cellInfos));
		}
		catch (TableModelException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.Message);
		}
		finally
		{
			grid.EndUpdate();
		}
		static object ParseDate(object value)
		{
			try
			{
				if (value == null)
				{
					return null;
				}
				string input = value.ToString();
				if (DateTime.TryParse(input, out var result2))
				{
					return result2;
				}
				Match match = Regex.Match(input, "([0-9]{4})([0-9]{2})([0-9]{2})");
				if (match.Success)
				{
					return new DateTime(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value));
				}
				return value;
			}
			catch
			{
				return value;
			}
		}
	}

	private void DeleteSelection(C1FlexGrid grid)
	{
		List<GridCellInfo> list = new List<GridCellInfo>();
		grid.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.CellRange selection = grid.Selection;
			for (int i = selection.TopRow; i <= selection.BottomRow; i++)
			{
				if (!grid.Rows[i].Visible || !grid.Rows[i].AllowEditing)
				{
					continue;
				}
				for (int j = selection.LeftCol; j <= selection.RightCol; j++)
				{
					if (grid.Cols[j].Visible && grid.Cols[j].AllowEditing)
					{
						object oldValue = grid[i, j];
						grid[i, j] = null;
						list.Add(new GridCellInfo(grid, i, j, oldValue, null));
					}
				}
			}
			commandManager.NewCommand(new GridBatchCellUpdateValueCommand(list));
		}
		finally
		{
			grid.EndUpdate();
		}
	}

	private void CutSelection(C1FlexGrid grid)
	{
		List<GridCellInfo> list = new List<GridCellInfo>();
		C1.Win.C1FlexGrid.CellRange selection = grid.Selection;
		for (int i = selection.TopRow; i <= selection.BottomRow; i++)
		{
			for (int j = selection.LeftCol; j <= selection.RightCol; j++)
			{
				object oldValue = grid[i, j];
				list.Add(new GridCellInfo(grid, i, j, oldValue, null));
			}
		}
		commandManager.NewCommand(new GridBatchCellUpdateValueCommand(list));
		grid.Cut();
	}

	private void GrdBalance_MouseClick(object sender, MouseEventArgs e)
	{
		if (sender is C1FlexGrid c1FlexGrid && e.Button == MouseButtons.Right)
		{
			switch (c1FlexGrid.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.Cell:
				ctxCell.ShowContextMenu(c1FlexGrid, e.Location);
				break;
			case HitTestTypeEnum.None:
				ctxEmpty.ShowContextMenu(c1FlexGrid, e.Location);
				break;
			}
		}
	}

	private void FrmBalanceEditor_Shown(object sender, EventArgs e)
	{
		base.Icon = Auditai.UI.Controls.Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.sideModifyBegin);
		ScrollToPosition(modifyUserData);
	}

	private void SetTheme()
	{
		grdBalance.Styles.Fixed.Border.Color = Color.DarkGray;
		if (Auditai.UI.Controls.Theme.SelectedAuditaiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			imageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			imageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		imageProcess.ProcessImage();
	}

	private void AttachEvent()
	{
		if (!lockEvent && !_eventAttached)
		{
			grdBalance.CellChanged += GrdBalance_CellChanged;
			_eventAttached = true;
		}
	}

	private void DettachEvent()
	{
		if (!lockEvent && _eventAttached)
		{
			grdBalance.CellChanged -= GrdBalance_CellChanged;
			_eventAttached = false;
		}
	}

	private bool ValidateData()
	{
		decimal num = default(decimal);
		decimal num2 = default(decimal);
		Dictionary<Account, decimal> dictionary = new Dictionary<Account, decimal>();
		for (int i = grdBalance.Rows.Fixed; i < grdBalance.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = grdBalance.Rows[i];
			if (!(row.UserData is Account account))
			{
				continue;
			}
			bool isDebit;
			decimal balanceValue = GetBalanceValue(row, out isDebit);
			if (isDebit)
			{
				if (account.Children.Count == 0)
				{
					num += balanceValue;
				}
				dictionary.Add(account, balanceValue);
			}
			else
			{
				if (account.Children.Count == 0)
				{
					num2 += balanceValue;
				}
				dictionary.Add(account, -balanceValue);
			}
		}
		if (num != num2)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, $"借贷金额不平衡,借方金额：{num}，贷方金额：{num2}");
			return false;
		}
		return true;
	}

	private void Populate(bool onlyLastLevelDisplay)
	{
		grdBalance.BeginUpdate();
		try
		{
			DettachEvent();
			lockEvent = true;
			grdBalance.Rows.Count = 1;
			grdBalance.Rows.Fixed = 1;
			grdBalance.Cols.Count = 1;
			grdBalance.Cols.Fixed = 1;
			grdBalance.Rows.DefaultSize = 30;
			grdBalance.AllowResizing = AllowResizingEnum.Both;
			C1.Win.C1FlexGrid.Column column = grdBalance.Cols[0];
			column.Name = "index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column.AllowEditing = false;
			column = grdBalance.Cols.Add();
			column.Name = "kmdm";
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column.AllowEditing = false;
			column = grdBalance.Cols.Add();
			column.Name = "kmmc";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column.AllowEditing = false;
			column = grdBalance.Cols.Add();
			column.Name = "debit";
			column.Caption = "年初借方余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = grdBalance.Cols.Add();
			column.Name = "credit";
			column.Caption = "年初贷方余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			grdBalance.Tree.Column = 1;
			TrialBalanceSheet sheet = _owner.CacheManager.GetTrialBalanceSheetWithCache(ledger);
			auxOldValueCache = new Dictionary<Account, Dictionary<AuxiliaryItem, decimal>>();
			foreach (Account item in ledger.RootAccounts.OrderBy((Account a) => a.Code))
			{
				AddChildren(item, null);
			}
			PopulateIndex(grdBalance);
			for (int i = 0; i < grdBalance.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row = grdBalance.Rows[i];
				if (!row.IsNode)
				{
					continue;
				}
				if (onlyLastLevelDisplay)
				{
					if (row.Node.Children > 0)
					{
						row.Visible = false;
					}
					else
					{
						row.Visible = true;
					}
				}
				else
				{
					row.Node.Collapsed = true;
				}
			}
			if (onlyLastLevelDisplay)
			{
				grdBalance.Tree.Style = TreeStyleFlags.None;
			}
			else
			{
				grdBalance.Tree.Style = TreeStyleFlags.Simple;
			}
			grdBalance.AutoSizeCols();
			void AddChildren(Account account, Node parentNode)
			{
				Node node = ((parentNode != null) ? parentNode.AddNode(NodeTypeEnum.LastChild, account.Code) : grdBalance.Rows.AddNode(0));
				node.Key = account;
				node.Row["kmdm"] = account.Code;
				node.Row["kmmc"] = " ".PadLeft(node.Level * 4) + account.Name;
				decimal total = sheet.Start[account].Total;
				node.Row["debit"] = GetDebitDisplayValue(account.IsDebit, total);
				node.Row["credit"] = GetCreditDisplayValue(account.IsDebit, total);
				if (account.Children.Count > 0)
				{
					(node.Row.Style ?? node.Row.StyleNew).BackColor = Color.LightYellow;
					node.Row.AllowEditing = false;
					{
						foreach (Account child in account.Children)
						{
							AddChildren(child, node);
						}
						return;
					}
				}
				Dictionary<AuxiliaryClass, ClassBalance> classBalances = sheet.End[account].ClassBalances;
				if (classBalances.Count > 0)
				{
					(node.Row.Style ?? node.Row.StyleNew).BackColor = Color.LightYellow;
					node.Row.AllowEditing = false;
					foreach (KeyValuePair<AuxiliaryClass, ClassBalance> item2 in classBalances)
					{
						SubsidiaryLedger subsidiaryLedger = ledger.GetSubsidiaryLedger(account, ledger.StartDate, ledger.GetEndDate(), item2.Key);
						SubsidiaryLedgerTotal subsidiaryLedgerTotal = subsidiaryLedger.Months.LastOrDefault()?.GrandTotal;
						Node node2 = node.AddNode(NodeTypeEnum.LastChild, account.Code + "-" + item2.Key.Code);
						node2.Key = Tuple.Create(account, item2.Key);
						node2.Row["kmdm"] = account.Code + "-" + item2.Key.Code;
						node2.Row["kmmc"] = item2.Key.Name;
						node2.Row["debit"] = GetDebitDisplayValue(account.IsDebit, subsidiaryLedger.BeginBalance);
						node2.Row["credit"] = GetCreditDisplayValue(account.IsDebit, subsidiaryLedger.BeginBalance);
						(node2.Row.Style ?? node2.Row.StyleNew).ForeColor = Color.Purple;
						(node2.Row.Style ?? node2.Row.StyleNew).BackColor = Color.LightYellow;
						node2.Row.AllowEditing = false;
						if (item2.Value.ItemBalances.Count > 0)
						{
							if (!auxOldValueCache.ContainsKey(account))
							{
								auxOldValueCache.Add(account, new Dictionary<AuxiliaryItem, decimal>());
							}
							Dictionary<AuxiliaryItem, decimal> dictionary = auxOldValueCache[account];
							foreach (KeyValuePair<AuxiliaryItem, decimal> item3 in item2.Value.ItemBalances.OrderBy((KeyValuePair<AuxiliaryItem, decimal> t) => t.Key.Code))
							{
								subsidiaryLedger = ledger.GetSubsidiaryLedger(account, ledger.StartDate, ledger.GetEndDate(), item3.Key);
								dictionary.Add(item3.Key, subsidiaryLedger.BeginBalance);
								Node node3 = node2.AddNode(NodeTypeEnum.LastChild, string.Empty);
								node3.Key = Tuple.Create(account, item3.Key);
								node3.Row["kmdm"] = account.Code + "-" + item3.Key.Code;
								node3.Row["kmmc"] = item3.Key.Name;
								node3.Row["debit"] = GetDebitDisplayValue(account.IsDebit, subsidiaryLedger.BeginBalance);
								node3.Row["credit"] = GetCreditDisplayValue(account.IsDebit, subsidiaryLedger.BeginBalance);
								(node3.Row.Style ?? node3.Row.StyleNew).ForeColor = Color.Purple;
							}
						}
					}
				}
			}
		}
		finally
		{
			lockEvent = false;
			AttachEvent();
			grdBalance.EndUpdate();
		}
	}

	private void ScrollToPosition(object userData)
	{
		if (userData is Account account)
		{
			for (int i = 0; i < grdBalance.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row = grdBalance.Rows[i];
				if (row.UserData == account)
				{
					if (row.IsNode)
					{
						expandNodeTree(row.Node);
						grdBalance.Row = row.Index;
						grdBalance.Col = grdBalance.Cols["credit"].Index;
						grdBalance.ShowCell(row.Index, grdBalance.Cols["credit"].Index);
					}
					break;
				}
			}
		}
		else if (userData is Tuple<Account, AuxiliaryItem> tuple)
		{
			for (int j = 0; j < grdBalance.Rows.Count; j++)
			{
				C1.Win.C1FlexGrid.Row row2 = grdBalance.Rows[j];
				if (row2.UserData is Tuple<Account, AuxiliaryItem> tuple2 && tuple2.Item1 == tuple.Item1 && tuple2.Item2 == tuple.Item2 && row2.IsNode)
				{
					expandNodeTree(row2.Node);
					grdBalance.Row = row2.Index;
					grdBalance.Col = grdBalance.Cols["credit"].Index;
					grdBalance.ShowCell(row2.Index, grdBalance.Cols["credit"].Index);
					break;
				}
			}
		}
		grdBalance.AutoSizeCols();
		static void expandNodeTree(Node node)
		{
			while (node.Parent != null)
			{
				node = node.Parent;
			}
			Node[] nodes2 = node.Nodes;
			foreach (Node node2 in nodes2)
			{
				node2.Expanded = true;
			}
			expandNodeTreeImpl(node);
		}
		static void expandNodeTreeImpl(Node pNode)
		{
			pNode.Expanded = true;
			Node[] nodes = pNode.Nodes;
			foreach (Node pNode2 in nodes)
			{
				expandNodeTreeImpl(pNode2);
			}
		}
	}

	private void PopulateIndex(C1FlexGrid grid)
	{
		if (grid != null)
		{
			for (int i = grid.Rows.Fixed; i < grid.Rows.Count; i++)
			{
				grid.Cols[0][i] = i.ToString();
			}
		}
	}

	private decimal GetBalanceValue(C1.Win.C1FlexGrid.Row row, out bool isDebit)
	{
		object obj = row["debit"];
		object obj2 = row["credit"];
		decimal num = default(decimal);
		decimal num2 = default(decimal);
		if (decimal.TryParse(obj?.ToString(), out var result))
		{
			num = result;
		}
		if (decimal.TryParse(obj2?.ToString(), out var result2))
		{
			num2 = result2;
		}
		decimal num3 = num - num2;
		if (num3 >= 0m)
		{
			isDebit = true;
			return num3;
		}
		isDebit = false;
		return -num3;
	}

	private decimal GetDebitDisplayValue(bool accIsDebit, decimal beginTotal1)
	{
		if (!accIsDebit)
		{
			if (!(beginTotal1 > 0m))
			{
				return -beginTotal1;
			}
			return 0m;
		}
		if (!(beginTotal1 > 0m))
		{
			return 0m;
		}
		return beginTotal1;
	}

	private decimal GetCreditDisplayValue(bool accIsDebit, decimal beginValue)
	{
		if (!accIsDebit)
		{
			if (!(beginValue > 0m))
			{
				return 0m;
			}
			return beginValue;
		}
		if (!(beginValue > 0m))
		{
			return -beginValue;
		}
		return 0m;
	}

	private void GrdBalance_CellChanged(object sender, RowColEventArgs e)
	{
		if (e.Col != grdBalance.Cols["debit"].Index && e.Col != grdBalance.Cols["credit"].Index)
		{
			return;
		}
		DettachEvent();
		try
		{
			Node node = grdBalance.Rows[e.Row].Node;
			Node node2 = null;
			while ((node2 = node.Parent) != null)
			{
				decimal num = default(decimal);
				decimal num2 = default(decimal);
				Node[] nodes = node2.Nodes;
				foreach (Node node3 in nodes)
				{
					bool isDebit;
					decimal balanceValue = GetBalanceValue(node3.Row, out isDebit);
					if (isDebit)
					{
						num += balanceValue;
					}
					else
					{
						num2 += balanceValue;
					}
				}
				decimal num3 = num - num2;
				if (num3 >= 0m)
				{
					node2.Row["debit"] = num3;
					node2.Row["credit"] = 0m;
				}
				else
				{
					node2.Row["debit"] = 0m;
					node2.Row["credit"] = -num3;
				}
				if (node2.Key is Tuple<Account, AuxiliaryClass>)
				{
					node2.Parent.Row["debit"] = node2.Row["debit"];
					node2.Parent.Row["credit"] = node2.Row["credit"];
					break;
				}
				node = node2;
			}
		}
		finally
		{
			AttachEvent();
		}
	}

	private void cmdSaveData_Click(object sender, ClickEventArgs e)
	{
		if (!ValidateData())
		{
			return;
		}
		List<Tuple<Account, decimal>> list = new List<Tuple<Account, decimal>>();
		List<Tuple<Account, AuxiliaryItem, decimal>> list2 = new List<Tuple<Account, AuxiliaryItem, decimal>>();
		for (int i = grdBalance.Rows.Fixed; i < grdBalance.Rows.Count; i++)
		{
			C1.Win.C1FlexGrid.Row row = grdBalance.Rows[i];
			if (row.UserData is Account account)
			{
				decimal total = ledger.InitialBalance[account].Total;
				bool isDebit;
				decimal balanceValue = GetBalanceValue(row, out isDebit);
				decimal num = ((!account.IsDebit) ? (isDebit ? (-balanceValue) : balanceValue) : (isDebit ? balanceValue : (-balanceValue)));
				if (total != num)
				{
					list.Add(Tuple.Create(account, num));
				}
			}
			else if (row.UserData is Tuple<Account, AuxiliaryItem> tuple)
			{
				decimal num2 = auxOldValueCache[tuple.Item1][tuple.Item2];
				bool isDebit2;
				decimal balanceValue2 = GetBalanceValue(row, out isDebit2);
				decimal num3 = ((!tuple.Item1.IsDebit) ? (isDebit2 ? (-balanceValue2) : balanceValue2) : (isDebit2 ? balanceValue2 : (-balanceValue2)));
				if (num2 != num3)
				{
					list2.Add(Tuple.Create(tuple.Item1, tuple.Item2, num3));
				}
			}
		}
		UpdateAuxiliary = list2;
		UpdateAccount = list;
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void cmdBalanceValidate_Click(object sender, ClickEventArgs e)
	{
		if (ValidateData())
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "校验通过");
		}
	}

	private void cmdCancelSave_Click(object sender, ClickEventArgs e)
	{
		Populate(onlyDisplayLastLevel);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.c1SplitContainer1 = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlToolBar = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1CommandDock1 = new C1.Win.C1Command.C1CommandDock();
		this.c1ToolBar1 = new C1.Win.C1Command.C1ToolBar();
		this.lnkBalanceValidate = new C1.Win.C1Command.C1CommandLink();
		this.cmdBalanceValidate = new C1.Win.C1Command.C1Command();
		this.lnkCancelSave = new C1.Win.C1Command.C1CommandLink();
		this.cmdCancelSave = new C1.Win.C1Command.C1Command();
		this.lnkSaveData = new C1.Win.C1Command.C1CommandLink();
		this.cmdSaveData = new C1.Win.C1Command.C1Command();
		this.pnlGrid = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.grdBalance = new Auditai.UI.Controls.C1FlexGridEx();
		this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).BeginInit();
		this.c1SplitContainer1.SuspendLayout();
		this.pnlToolBar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1CommandDock1).BeginInit();
		this.c1CommandDock1.SuspendLayout();
		this.pnlGrid.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdBalance).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).BeginInit();
		base.SuspendLayout();
		this.c1SplitContainer1.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1SplitContainer1.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.c1SplitContainer1.CollapsingCueColor = System.Drawing.Color.FromArgb(133, 133, 150);
		this.c1SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1SplitContainer1.ForeColor = System.Drawing.Color.FromArgb(0, 0, 0);
		this.c1SplitContainer1.HeaderHeight = 27;
		this.c1SplitContainer1.Location = new System.Drawing.Point(0, 0);
		this.c1SplitContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.c1SplitContainer1.Name = "c1SplitContainer1";
		this.c1SplitContainer1.Panels.Add(this.pnlToolBar);
		this.c1SplitContainer1.Panels.Add(this.pnlGrid);
		this.c1SplitContainer1.Size = new System.Drawing.Size(933, 637);
		this.c1SplitContainer1.SplitterWidth = 5;
		this.c1SplitContainer1.TabIndex = 0;
		this.pnlToolBar.Controls.Add(this.c1CommandDock1);
		this.pnlToolBar.Height = 63;
		this.pnlToolBar.KeepRelativeSize = false;
		this.pnlToolBar.Location = new System.Drawing.Point(0, 0);
		this.pnlToolBar.MinHeight = 52;
		this.pnlToolBar.MinWidth = 52;
		this.pnlToolBar.Name = "pnlToolBar";
		this.pnlToolBar.Resizable = false;
		this.pnlToolBar.Size = new System.Drawing.Size(933, 63);
		this.pnlToolBar.SizeRatio = 9.906;
		this.pnlToolBar.TabIndex = 0;
		this.pnlToolBar.Width = 933;
		this.c1CommandDock1.Controls.Add(this.c1ToolBar1);
		this.c1CommandDock1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1CommandDock1.Id = 1;
		this.c1CommandDock1.Location = new System.Drawing.Point(0, 0);
		this.c1CommandDock1.Name = "c1CommandDock1";
		this.c1CommandDock1.Size = new System.Drawing.Size(933, 63);
		this.c1ToolBar1.AccessibleName = "Tool Bar";
		this.c1ToolBar1.AutoSize = false;
		this.c1ToolBar1.Border.Width = 0;
		this.c1ToolBar1.ButtonLayoutHorz = C1.Win.C1Command.ButtonLayoutEnum.TextBelow;
		this.c1ToolBar1.ButtonLookHorz = C1.Win.C1Command.ButtonLookFlags.TextAndImage;
		this.c1ToolBar1.CommandHolder = null;
		this.c1ToolBar1.CommandLinks.AddRange(new C1.Win.C1Command.C1CommandLink[3] { this.lnkBalanceValidate, this.lnkCancelSave, this.lnkSaveData });
		this.c1ToolBar1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1ToolBar1.Location = new System.Drawing.Point(0, 0);
		this.c1ToolBar1.MinButtonSize = 42;
		this.c1ToolBar1.Movable = false;
		this.c1ToolBar1.Name = "c1ToolBar1";
		this.c1ToolBar1.Size = new System.Drawing.Size(823, 61);
		this.c1ToolBar1.Text = "c1ToolBar2";
		this.c1ToolBar1.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
		this.c1ToolBar1.VisualStyleBase = C1.Win.C1Command.VisualStyle.System;
		this.lnkBalanceValidate.Command = this.cmdBalanceValidate;
		this.lnkBalanceValidate.Text = "平衡检查";
		this.cmdBalanceValidate.Image = Auditai.UI.LedgerView.Properties.Resources.balanceCheck;
		this.cmdBalanceValidate.Name = "cmdBalanceValidate";
		this.cmdBalanceValidate.ShortcutText = "";
		this.cmdBalanceValidate.Text = "新命令";
		this.cmdBalanceValidate.Click += new C1.Win.C1Command.ClickEventHandler(cmdBalanceValidate_Click);
		this.lnkCancelSave.Command = this.cmdCancelSave;
		this.lnkCancelSave.SortOrder = 1;
		this.cmdCancelSave.Image = Auditai.UI.LedgerView.Properties.Resources.largeCancelSave;
		this.cmdCancelSave.Name = "cmdCancelSave";
		this.cmdCancelSave.ShortcutText = "";
		this.cmdCancelSave.Text = "取消保存";
		this.cmdCancelSave.Click += new C1.Win.C1Command.ClickEventHandler(cmdCancelSave_Click);
		this.lnkSaveData.Command = this.cmdSaveData;
		this.lnkSaveData.SortOrder = 2;
		this.cmdSaveData.Image = Auditai.UI.LedgerView.Properties.Resources.saveData;
		this.cmdSaveData.Name = "cmdSaveData";
		this.cmdSaveData.ShortcutText = "";
		this.cmdSaveData.Text = "保存数据";
		this.cmdSaveData.Click += new C1.Win.C1Command.ClickEventHandler(cmdSaveData_Click);
		this.pnlGrid.Controls.Add(this.grdBalance);
		this.pnlGrid.Height = 573;
		this.pnlGrid.Location = new System.Drawing.Point(0, 64);
		this.pnlGrid.MinHeight = 52;
		this.pnlGrid.MinWidth = 52;
		this.pnlGrid.Name = "pnlGrid";
		this.pnlGrid.Size = new System.Drawing.Size(933, 573);
		this.pnlGrid.TabIndex = 1;
		this.pnlGrid.Width = 933;
		this.grdBalance.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		this.grdBalance.ColumnInfo = "10,1,0,0,0,100,Columns:";
		this.grdBalance.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdBalance.DrawMode = C1.Win.C1FlexGrid.DrawModeEnum.OwnerDraw;
		this.grdBalance.Location = new System.Drawing.Point(0, 0);
		this.grdBalance.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		this.grdBalance.Name = "grdBalance";
		this.grdBalance.Rows.DefaultSize = 20;
		this.grdBalance.Size = new System.Drawing.Size(933, 573);
		this.grdBalance.TabIndex = 0;
		this.c1CommandHolder1.Commands.Add(this.cmdBalanceValidate);
		this.c1CommandHolder1.Commands.Add(this.cmdSaveData);
		this.c1CommandHolder1.Commands.Add(this.cmdCancelSave);
		this.c1CommandHolder1.Owner = this;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(933, 637);
		base.Controls.Add(this.c1SplitContainer1);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.Name = "frmBalanceEditor";
		this.Text = "修改期初数";
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).EndInit();
		this.c1SplitContainer1.ResumeLayout(false);
		this.pnlToolBar.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1CommandDock1).EndInit();
		this.c1CommandDock1.ResumeLayout(false);
		this.pnlGrid.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdBalance).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1CommandHolder1).EndInit();
		base.ResumeLayout(false);
	}
}
