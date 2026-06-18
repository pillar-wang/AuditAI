using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using C1.Win.C1FlexGrid.Util.BaseControls;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Controls.CollectTable;
using Leqisoft.UI.LedgerView.Properties;

namespace Leqisoft.UI.LedgerView;

public class LedgerAgingEditor : ISetTheme
{
	public class RowVM
	{
		public int Row { get; set; }

		public Account Account { get; set; }

		public AuxiliaryItem Item { get; set; }

		public bool Visible { get; set; }

		public int Start { get; set; }

		public int End { get; set; }

		public int Level { get; set; }
	}

	private const string CN_INDEX = "CN_INDEX";

	private const string CN_CODE = "CN_CODE";

	private const string CN_NAME = "CN_NAME";

	private const string CN_YEARSTART = "CN_YEARSTART";

	private const string CN_DIRECTION = "CN_DIRECTION";

	private const string CN_TOTAL = "CN_TOTAL";

	private readonly C1ContextMenu ctxCell = new C1ContextMenu();

	private readonly C1ContextMenu ctxFixed = new C1ContextMenu();

	private readonly C1ContextMenu ctxEmpty = new C1ContextMenu();

	private readonly C1Command cmdCopy = new C1Command();

	private readonly C1CommandLink lnkCopy = new C1CommandLink();

	private readonly C1CommandLink _lnkFilter;

	private readonly C1CommandLink _lnkSample;

	private readonly C1CommandLink _lnkSelect;

	private readonly C1CommandLink _lnkCancel;

	private readonly C1Command _cmdUpLevel;

	private readonly C1CommandLink _lnkUpLevel;

	private readonly C1Command _cmdDownLevel;

	private readonly C1CommandLink _lnkDownLevel;

	private readonly LedgerViewer _owner;

	private object _auxiliary;

	private readonly C1SplitterPanel pnlAnalyzeTitle;

	private readonly C1SplitterPanel pnlAnalyzeHead;

	private readonly C1SplitterPanel pnlAnalyzeGrid;

	private readonly C1Label lblAgeTitle;

	public C1DateEdit dteAnalyzeDate;

	private readonly C1Label lblAnalyzeAccount;

	public C1FlexGridEx grid;

	private readonly C1SplitterPanel pnlSidebar;

	private ReceivableAgeSheet ras;

	public readonly BalanceSheetCacheManager SheetCacheManager = new BalanceSheetCacheManager();

	private C1ContextMenu ctxDirection = new C1ContextMenu();

	private Pen panelBorderPen = new Pen(Color.FromArgb(169, 169, 169), 1f);

	private RibbonImageProcess imageProcess = new RibbonImageProcess();

	private Ledger Ledger => _owner.Ledger;

	public bool PendingAllEvent { get; set; }

	public DateTime BaseDate { get; set; }

	public bool Direction { get; set; } = true;


	public int DisplayLevel { get; set; }

	public Account Account { get; set; }

	public List<RowVM> VM { get; set; } = new List<RowVM>();


	public C1SplitContainer View { get; private set; }

	public LedgerAgingEditor(LedgerViewer owner)
	{
		_owner = owner;
		_owner.AccountTreeEditor.ShowAuxiliaryNode += AccountTreeEditor_ShowAuxiliaryNode;
		_owner.AccountTreeEditor.HideAuxiliaryNode += AccountTreeEditor_HideAuxiliaryNode;
		View = new C1SplitContainer();
		pnlAnalyzeTitle = new C1SplitterPanel();
		pnlAnalyzeHead = new C1SplitterPanel();
		pnlAnalyzeGrid = new C1SplitterPanel();
		lblAgeTitle = new C1Label();
		dteAnalyzeDate = new C1DateEdit();
		lblAnalyzeAccount = new C1Label();
		grid = new C1FlexGridEx();
		grid.Name = "grid";
		Font font = new Font("Microsoft YaHei", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblAgeTitle.TextDetached = true;
		lblAgeTitle.BorderStyle = BorderStyle.None;
		lblAgeTitle.Dock = DockStyle.Fill;
		lblAgeTitle.Font = font;
		lblAgeTitle.Text = "账龄分析表";
		lblAgeTitle.TextAlign = ContentAlignment.MiddleCenter;
		pnlAnalyzeTitle.Controls.Add(lblAgeTitle);
		pnlAnalyzeTitle.Height = 30;
		pnlAnalyzeTitle.KeepRelativeSize = false;
		pnlAnalyzeTitle.Location = new Point(0, 0);
		pnlAnalyzeTitle.MinHeight = 30;
		pnlAnalyzeTitle.Resizable = false;
		pnlAnalyzeTitle.Size = new Size(927, 30);
		pnlAnalyzeTitle.SizeRatio = 4.769;
		Font font2 = new Font("Microsoft YaHei", 9f, FontStyle.Regular, GraphicsUnit.Point, 134);
		lblAnalyzeAccount.TextDetached = true;
		lblAnalyzeAccount.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
		lblAnalyzeAccount.BorderStyle = BorderStyle.None;
		lblAnalyzeAccount.Font = font2;
		lblAnalyzeAccount.Location = new Point(5, 4);
		lblAnalyzeAccount.Size = new Size(420, 17);
		lblAnalyzeAccount.Text = "科目名称：";
		lblAnalyzeAccount.TextAlign = ContentAlignment.MiddleLeft;
		dteAnalyzeDate.AllowSpinLoop = false;
		dteAnalyzeDate.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
		dteAnalyzeDate.Calendar.DayNameLength = 1;
		dteAnalyzeDate.CustomFormat = "yyyy-MM-dd";
		dteAnalyzeDate.FormatType = FormatTypeEnum.CustomFormat;
		dteAnalyzeDate.ImagePadding = new Padding(0);
		dteAnalyzeDate.Location = new Point(426, 2);
		dteAnalyzeDate.Size = new Size(75, 21);
		dteAnalyzeDate.VisibleButtons = DropDownControlButtonFlags.None;
		pnlAnalyzeHead.Height = 25;
		pnlAnalyzeHead.KeepRelativeSize = false;
		pnlAnalyzeHead.Location = new Point(0, 31);
		pnlAnalyzeHead.MinHeight = 25;
		pnlAnalyzeHead.Resizable = false;
		pnlAnalyzeHead.Size = new Size(927, 25);
		pnlAnalyzeHead.Controls.Add(dteAnalyzeDate);
		pnlAnalyzeHead.Controls.Add(lblAnalyzeAccount);
		pnlAnalyzeHead.Paint += delegate(object s1, PaintEventArgs e1)
		{
			e1.Graphics.DrawLine(panelBorderPen, 0, pnlAnalyzeHead.Height - 1, pnlAnalyzeHead.Width, pnlAnalyzeHead.Height - 1);
		};
		grid.AllowEditing = false;
		grid.AllowResizing = AllowResizingEnum.Both;
		grid.AllowSorting = AllowSortingEnum.None;
		grid.BorderStyle = C1.Win.C1FlexGrid.Util.BaseControls.BorderStyleEnum.None;
		grid.DrawMode = DrawModeEnum.OwnerDraw;
		grid.Font = font2;
		grid.Location = new Point(0, 0);
		grid.Rows.DefaultSize = 20;
		grid.Size = new Size(927, 573);
		grid.Tree.LineColor = Color.DimGray;
		grid.VisualStyle = C1.Win.C1FlexGrid.VisualStyle.Custom;
		grid.OwnerDrawCell += Grid_OwnerDrawCell;
		grid.MouseDoubleClick += Grid_MouseDoubleClick;
		C1ToolBar c1ToolBar = new C1ToolBar();
		C1CommandLink c1CommandLink = new C1CommandLink();
		C1Command c1Command = new C1Command();
		c1Command.Text = "借方余额";
		c1Command.UserData = true;
		c1Command.Image = null;
		c1Command.Click += CmdSidebarDirection_Click;
		c1CommandLink.Command = c1Command;
		ctxDirection.CommandLinks.Add(c1CommandLink);
		C1CommandLink c1CommandLink2 = new C1CommandLink();
		C1Command c1Command2 = new C1Command();
		c1Command2.Text = "贷方余额";
		c1Command2.UserData = false;
		c1Command2.Image = null;
		c1Command2.Click += CmdSidebarDirection_Click;
		c1CommandLink2.Command = c1Command2;
		ctxDirection.CommandLinks.Add(c1CommandLink2);
		C1CommandLink c1CommandLink3 = new C1CommandLink();
		C1Command c1Command3 = new C1Command();
		c1Command3.Text = "分析选项";
		c1Command3.Image = Resources.sidebarAnalyzyProject;
		c1Command3.Click += CmdDirection_Click;
		c1CommandLink3.Command = c1Command3;
		c1ToolBar.CommandLinks.Add(c1CommandLink3);
		C1CommandLink c1CommandLink4 = new C1CommandLink
		{
			Delimiter = true
		};
		C1Command c1Command4 = new C1Command();
		c1Command4.Text = "隐藏侧边栏";
		c1Command4.Image = Resources.sideHideSidebar;
		c1Command4.Click += delegate
		{
			_owner.OnHideSidebarClick();
		};
		c1CommandLink4.Command = c1Command4;
		C1SplitContainer c1SplitContainer = ComponentFactory.BuildSidebar(grid, c1ToolBar, out pnlSidebar);
		foreach (C1CommandLink commandLink in c1ToolBar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		pnlAnalyzeGrid.Height = 573;
		pnlAnalyzeGrid.Location = new Point(0, 57);
		pnlAnalyzeGrid.Size = new Size(927, 573);
		pnlAnalyzeGrid.Controls.Add(grid);
		grid.Dock = DockStyle.Fill;
		View.AutoSizeElement = AutoSizeElement.Both;
		View.BackColor = Color.FromArgb(240, 240, 240);
		View.CollapsingCueColor = Color.FromArgb(133, 133, 150);
		View.Dock = DockStyle.Fill;
		View.ForeColor = Color.FromArgb(0, 0, 0);
		View.Panels.Add(pnlAnalyzeTitle);
		View.Panels.Add(pnlAnalyzeHead);
		View.Panels.Add(pnlAnalyzeGrid);
		cmdCopy.Text = "复制";
		cmdCopy.Image = ContextResources.ctxCopy;
		cmdCopy.Click += CmdCopy_Click;
		cmdCopy.CommandStateQuery += CmdLayer_CommandStateQuery;
		lnkCopy.Command = cmdCopy;
		ctxCell.Popup += CtxMenu_Popup;
		ctxEmpty.CommandLinks.Add(grid.FilterManager.GenLnkCancelAll());
		ctxFixed.HideFirstDelimiter = true;
		C1Command c1Command5 = new C1Command();
		c1Command5.Text = "隐藏本列";
		c1Command5.UserData = grid;
		c1Command5.Click += _owner.ColHide_Click;
		C1CommandLink value = new C1CommandLink
		{
			Command = c1Command5
		};
		ctxFixed.CommandLinks.Add(value);
		C1Command c1Command6 = new C1Command();
		c1Command6.Text = "取消隐藏";
		c1Command6.UserData = grid;
		c1Command6.Click += _owner.CancelHide_Click;
		C1CommandLink value2 = new C1CommandLink
		{
			Command = c1Command6
		};
		ctxFixed.CommandLinks.Add(value2);
		grid.MouseClick += GrdLedgerAging_MouseClick;
		dteAnalyzeDate.ValueChanged += DetAgeAnalyze_ValueChanged;
		_lnkFilter = grid.FilterManager.GenLnkFilter();
		_lnkSample = grid.FilterManager.GenLnkSample();
		_lnkSelect = grid.FilterManager.GenLnkSelect();
		_lnkCancel = grid.FilterManager.GenLnkCancelCurrentColumn();
		_cmdUpLevel = new C1Command
		{
			Text = "上提一级"
		};
		_cmdUpLevel.Click += _cmdUpLevel_Click;
		_cmdUpLevel.CommandStateQuery += _cmdUpLevel_CommandStateQuery;
		_lnkUpLevel = new C1CommandLink(_cmdUpLevel);
		_cmdDownLevel = new C1Command
		{
			Text = "下沉一级"
		};
		_cmdDownLevel.Click += _cmdDownLevel_Click;
		_cmdDownLevel.CommandStateQuery += _cmdDownLevel_CommandStateQuery;
		_lnkDownLevel = new C1CommandLink(_cmdDownLevel);
	}

	public void ShowSideToolbar()
	{
		pnlSidebar?.Show();
	}

	public void HideSideToolbar()
	{
		pnlSidebar?.Hide();
	}

	private void CmdDirection_Click(object sender, ClickEventArgs e)
	{
		ctxDirection.ShowContextMenu(e.CallerLink.Owner as C1ToolBar, new Point(e.CallerLink.Bounds.Left, e.CallerLink.Bounds.Bottom));
	}

	private void CmdSidebarDirection_Click(object sender, ClickEventArgs e)
	{
		C1Command command = e.CallerLink.Command;
		if (command.UserData is bool direction)
		{
			Direction = direction;
			if (CanAnalyze())
			{
				PopulateImpl(_auxiliary);
			}
		}
	}

	public void SetDirectionButton(bool isDebit)
	{
		Direction = isDebit;
	}

	public void GenVM()
	{
		Dictionary<Account, AuxiliaryClass> dicAuxClass = _owner.AccountTreeEditor.GetAccountExpandingAuxClass();
		ras = Ledger.GetReceivableAgeSheet(BaseDate);
		VM.Clear();
		int row = 0;
		int level = 0;
		bool flag = Account.DescendantsAndSelf.Any((Account a) => ras.Entries[a].Aux.Any());
		Add(Account);
		void Add(Account a)
		{
			ReceivableAgeEntry receivableAgeEntry = ras.Entries[a];
			RowVM rowVM = new RowVM
			{
				Account = a,
				Start = row,
				Level = level
			};
			rowVM.Visible = GetRowVisible(a);
			VM.Add(rowVM);
			row++;
			level++;
			foreach (Account child in a.Children)
			{
				Add(child);
			}
			foreach (KeyValuePair<AuxiliaryItem, ReceivableAgeValue> item in receivableAgeEntry.Aux)
			{
				RowVM itemVM = new RowVM
				{
					Account = a,
					Item = item.Key,
					Start = row,
					Level = level
				};
				itemVM.Visible = _isItemVisible(itemVM);
				VM.Add(itemVM);
				row++;
				itemVM.End = row;
			}
			level--;
			rowVM.End = row;
		}
		bool GetRowVisible(Account a)
		{
			if (Account.DescendantsAndSelf.Any((Account ac) => ras.Entries[ac].Aux.Any()))
			{
				if (!dicAuxClass.ContainsKey(a))
				{
					return a.Children.Count == 0;
				}
				return false;
			}
			return a.Children.Count == 0;
		}
		bool _isItemVisible(RowVM itemVM)
		{
			if (dicAuxClass.TryGetValue(itemVM.Account, out var value))
			{
				return value == itemVM.Item.Class;
			}
			return false;
		}
	}

	public void Populate1()
	{
		grid.BeginUpdate();
		grid.FilterManager.Clear();
		grid.Cols.Count = 1;
		grid.Cols.Fixed = 1;
		grid.Rows.Count = 1;
		grid.Rows.Fixed = 1;
		grid.Rows.DefaultSize = 30;
		C1.Win.C1FlexGrid.Column column = grid.Cols[0];
		column.Name = "CN_INDEX";
		column.Caption = "序号";
		column.DataType = typeof(string);
		column.TextAlign = TextAlignEnum.CenterCenter;
		column = grid.Cols.Add();
		column.Name = "CN_CODE";
		column.Caption = "科目代码";
		column.DataType = typeof(string);
		column = grid.Cols.Add();
		column.Name = "CN_NAME";
		column.Caption = "科目名称";
		column.DataType = typeof(string);
		column = grid.Cols.Add();
		column.Name = "CN_YEARSTART";
		column.Caption = "1年以内";
		column.DataType = typeof(decimal);
		column.Format = "#,0.00;-#,0.00;#";
		for (int i = 1; i < ras.YearCount - 1; i++)
		{
			column = grid.Cols.Add();
			column.Name = string.Format("{0}_{1}", "CN_YEARSTART", i);
			column.Caption = $"{i}~{i + 1}年";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
		}
		if (ras.YearCount > 1)
		{
			column = grid.Cols.Add();
			column.Name = "CN_YEARSTART_LAST";
			column.Caption = $"{ras.YearCount - 1}年以上";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
		}
		column = grid.Cols.Add();
		column.Name = "CN_DIRECTION";
		column.Caption = "余额方向";
		column.DataType = typeof(string);
		column.TextAlign = TextAlignEnum.CenterCenter;
		column = grid.Cols.Add();
		column.Name = "CN_TOTAL";
		column.Caption = "期末余额";
		column.DataType = typeof(decimal);
		column.Format = "#,0.00;-#,0.00;#";
		column.StyleNew.BackColor = Color.LightYellow;
		grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
		foreach (RowVM item in VM)
		{
			if (!item.Visible)
			{
				continue;
			}
			C1.Win.C1FlexGrid.Row row = grid.Rows.Add();
			row.UserData = item;
			item.Row = row.Index;
			ReceivableAgeEntry receivableAgeEntry = ras.Entries[item.Account];
			if (item.Item == null)
			{
				row["CN_CODE"] = item.Account.Code;
				row["CN_NAME"] = ex1.GetSecondFullName(item.Account);
				for (int j = 0; j < ras.YearCount; j++)
				{
					row[grid.Cols.IndexOf("CN_YEARSTART") + j] = receivableAgeEntry.Value.Values[j];
				}
				row["CN_DIRECTION"] = Common.GetDCChar(receivableAgeEntry.Value.IsDebit, receivableAgeEntry.Value.End);
				row["CN_TOTAL"] = receivableAgeEntry.Value.End;
				continue;
			}
			row.StyleNew.ForeColor = Color.Purple;
			row["CN_CODE"] = item.Account.Code + "-" + item.Item.Code;
			row["CN_NAME"] = ex1.GetSecondFullName(item.Account, item.Item);
			ReceivableAgeValue receivableAgeValue = receivableAgeEntry.Aux[item.Item];
			for (int k = 0; k < ras.YearCount; k++)
			{
				row[grid.Cols.IndexOf("CN_YEARSTART") + k] = receivableAgeValue.Values[k];
			}
			row["CN_DIRECTION"] = Common.GetDCChar(receivableAgeValue.IsDebit, receivableAgeValue.End);
			row["CN_TOTAL"] = receivableAgeValue.End;
		}
		grid.AutoSizeCols(1, 2, 10);
		grid.EndUpdate();
	}

	public void Populate(object auxiliary)
	{
		SetTitle(Account);
		GenVM();
		Populate1();
	}

	public void SetTheme()
	{
		if (Leqisoft.UI.Controls.Theme.SelectedLeqiTheme.ThemeFlags.HasFlag(ThemeEnum.WhiteIcon))
		{
			imageProcess.SetImageStrategy(new WhiteImageStrategy());
		}
		else
		{
			imageProcess.SetImageStrategy(new DefaultImageStrategy());
		}
		imageProcess.ProcessImage();
	}

	private void CtxMenu_Popup(object sender, EventArgs e)
	{
		ctxCell.CommandLinks.Clear();
		ctxCell.CommandLinks.Add(lnkCopy);
		ctxCell.CommandLinks.Add(_lnkFilter);
		ctxCell.CommandLinks.Add(_lnkSample);
		ctxCell.CommandLinks.Add(_lnkSelect);
		ctxCell.CommandLinks.Add(_lnkCancel);
		if (grid.Rows[grid.Row].UserData is RowVM rowVM)
		{
			_cmdUpLevel.Visible = rowVM.Level > 0;
			_cmdDownLevel.Visible = rowVM.End > rowVM.Start + 1;
		}
		_lnkUpLevel.Delimiter = _cmdUpLevel.Visible;
		_lnkDownLevel.Delimiter = _cmdDownLevel.Visible && !_cmdUpLevel.Visible;
		ctxCell.CommandLinks.Add(_lnkUpLevel);
		ctxCell.CommandLinks.Add(_lnkDownLevel);
	}

	private void CmdCopy_Click(object sender, ClickEventArgs e)
	{
		Common.SetSelectionToClipboard(grid);
	}

	private void CmdLayer_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = grid.Row >= grid.Rows.Fixed && grid.Col >= grid.Cols.Fixed;
	}

	private void CmdLayer_Click(object sender, ClickEventArgs e)
	{
		DisplayLevel = (int)(sender as C1Command).UserData;
		if (Account != null)
		{
			if (!CanAnalyze())
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "账期小于1年，无法进行账龄分析！");
			}
			else
			{
				PopulateImpl(_auxiliary);
			}
		}
	}

	private void DetAgeAnalyze_ValueChanged(object sender, EventArgs e)
	{
		if (!DateTime.TryParse(dteAnalyzeDate.Text, out var result))
		{
			return;
		}
		BaseDate = result;
		if (Account != null)
		{
			if (!CanAnalyze())
			{
				Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "账期小于1年，无法进行账龄分析！");
				return;
			}
			SheetCacheManager.Cache(Ledger, BaseDate);
			Populate(_auxiliary);
		}
	}

	private void _cmdDownLevel_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		RowVM rowVM = grid.Rows[grid.Row].UserData as RowVM;
	}

	private void _cmdUpLevel_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
	}

	private void _cmdUpLevel_Click(object sender, ClickEventArgs e)
	{
		CellRange selection = grid.Selection;
		int topRow = selection.TopRow;
		int bottomRow = selection.BottomRow;
		for (int i = topRow; i <= bottomRow; i++)
		{
			RowVM vm = grid.Rows[i].UserData as RowVM;
			if (vm.Visible)
			{
				RowVM rowVM = VM.Take(vm.Start).LastOrDefault((RowVM v) => v.End > vm.Start);
				rowVM.Visible = true;
				for (int j = rowVM.Start + 1; j < rowVM.End; j++)
				{
					VM[j].Visible = false;
				}
			}
		}
		Populate1();
	}

	private void _cmdDownLevel_Click(object sender, ClickEventArgs e)
	{
		CellRange selection = grid.Selection;
		int topRow = selection.TopRow;
		int bottomRow = selection.BottomRow;
		Dictionary<Account, AuxiliaryClass> dicAuxClass = _owner.AccountTreeEditor.GetAccountExpandingAuxClass();
		for (int i = topRow; i <= bottomRow; i++)
		{
			RowVM vm = grid.Rows[i].UserData as RowVM;
			vm.Visible = false;
			for (int j = vm.Start + 1; j < vm.End; j++)
			{
				RowVM vmDown = VM[j];
				vmDown.Visible = _IsVisible();
				bool _IsVisible()
				{
					if (vm.Level + 1 == vmDown.Level)
					{
						if (vmDown.Item == null)
						{
							return true;
						}
						if (dicAuxClass.TryGetValue(vm.Account, out var value))
						{
							return vmDown.Item.Class == value;
						}
						return false;
					}
					return false;
				}
			}
		}
		Populate1();
	}

	private void AccountTreeEditor_HideAuxiliaryNode(object sender, Tuple<Account, AuxiliaryClass> e)
	{
		Populate(null);
	}

	private void AccountTreeEditor_ShowAuxiliaryNode(object sender, Tuple<Account, AuxiliaryClass> e)
	{
		Populate(null);
	}

	private void GrdLedgerAging_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			switch (grid.HitTest(e.Location).Type)
			{
			case HitTestTypeEnum.ColumnHeader:
				ctxFixed.ShowContextMenu(grid, e.Location);
				break;
			case HitTestTypeEnum.None:
				ctxEmpty.ShowContextMenu(grid, e.Location);
				break;
			case HitTestTypeEnum.Cell:
				ctxCell.ShowContextMenu(grid, e.Location);
				break;
			}
		}
	}

	private Dictionary<int, AgeAmount> AnalyzeAgeSheet(AgeSheet ageSheet, int inner)
	{
		Dictionary<int, AgeAmount> dictionary = new Dictionary<int, AgeAmount>();
		AgeAmountSheet ageAmountSheet = ageSheet.AgeAmountSheet;
		AgeAmount ageBegin = ageSheet.AgeBegin;
		for (int i = 0; i < inner; i++)
		{
			int key = BaseDate.Year - i;
			if (ageAmountSheet.ContainsKey(key))
			{
				dictionary.Add(i, ageAmountSheet[key]);
				continue;
			}
			dictionary.Add(i, new AgeAmount
			{
				IsDebit = ageSheet.IsDebit,
				Amount = 0m
			});
		}
		decimal amount = ageBegin.Amount;
		if (ageAmountSheet.Count > 0)
		{
			for (int j = ageSheet.MinYear; j <= BaseDate.Year - inner; j++)
			{
				if (ageAmountSheet.ContainsKey(j))
				{
					amount += ageAmountSheet[j].Amount;
				}
			}
		}
		dictionary.Add(-1, new AgeAmount
		{
			IsDebit = ageSheet.IsDebit,
			Amount = amount
		});
		return dictionary;
	}

	private void SetTitle(Account account)
	{
		lblAnalyzeAccount.Text = "科目名称：（" + account.Code + "）" + account.Name;
	}

	private decimal GetDisplayAmount(AgeAmount amount)
	{
		if (amount == null)
		{
			return 0m;
		}
		if (Direction)
		{
			if (!amount.IsDebit)
			{
				return -amount.Amount;
			}
			return amount.Amount;
		}
		if (!amount.IsDebit)
		{
			return amount.Amount;
		}
		return -amount.Amount;
	}

	private int GetLevel(Account account)
	{
		int num = 0;
		Account account2 = account;
		while ((account2 = account2.Parent) != null)
		{
			num++;
		}
		return num;
	}

	private List<Account> GetLevelOrLastChildren(Account account, int level)
	{
		List<Account> result = new List<Account>();
		generateResult(account, GetLevel(account));
		return result;
		void generateResult(Account acc, int lev)
		{
			if (lev == level)
			{
				result.Add(acc);
			}
			else
			{
				if (acc.Children.Count != 0)
				{
					foreach (Account child in acc.Children)
					{
						generateResult(child, lev + 1);
					}
					return;
				}
				result.Add(acc);
			}
		}
	}

	private void PopulateImpl(object auxiliary)
	{
		_auxiliary = auxiliary;
		DateTime _baseDate = BaseDate;
		if (!CanAnalyze())
		{
			Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "账期小于1年，无法进行账龄分析！");
			return;
		}
		grid.BeginUpdate();
		PendingAllEvent = true;
		try
		{
			grid.Cols.Count = 0;
			grid.Cols.Fixed = 1;
			grid.Rows.Count = 1;
			grid.Rows.Fixed = 1;
			grid.Rows.DefaultSize = 30;
			C1.Win.C1FlexGrid.Column column = grid.Cols.Add();
			column.Name = "CN_INDEX";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = grid.Cols.Add();
			column.Name = "CN_CODE";
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column = grid.Cols.Add();
			column.Name = "CN_NAME";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column = grid.Cols.Add();
			column.Name = "CN_YEARSTART";
			column.Caption = "1年以内";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			int innerYear = 1;
			for (int num = BaseDate.Year - 1; num >= Ledger.StartDate.Year; num--)
			{
				DateTime dateTime = BaseDate.CopyToSpecificYear(num);
				DateTime dateTime2 = BaseDate.CopyToSpecificYear(num - 1).AddDays(1.0);
				if (!(dateTime2 >= Ledger.StartDate))
				{
					break;
				}
				innerYear++;
				if (innerYear >= 5)
				{
					break;
				}
			}
			for (int i = 2; i <= innerYear; i++)
			{
				column = grid.Cols.Add();
				column.Caption = $"{i - 1}-{i}年";
				column.DataType = typeof(decimal);
				column.Format = "#,0.00;-#,0.00;#";
			}
			column = grid.Cols.Add();
			column.Caption = $"{innerYear}年以上";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = grid.Cols.Add();
			column.Name = "CN_TOTAL";
			column.Caption = (Direction ? "借方余额合计" : "贷方余额合计");
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			(column.Style ?? column.StyleNew).BackColor = Color.LightYellow;
			grid.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			grid.Rows.Fixed = 1;
			grid.Cols.Fixed = 1;
			grid.Tree.Column = 1;
			int index = 1;
			AgeSheetFactory ageSheetFactory = new AgeSheetFactory(this, Ledger);
			int startColIndex = grid.Cols["CN_YEARSTART"].Index;
			if (!(auxiliary is AuxiliaryClass auxiliaryClass2))
			{
				if (auxiliary is AuxiliaryItem auxiliaryItem2)
				{
					if (DisplayLevel != 0)
					{
						AddAuxiliaryItem(Account, auxiliaryItem2, null);
					}
				}
				else
				{
					int level = GetLevel(Account);
					if (level < DisplayLevel)
					{
						List<Account> levelOrLastChildren = GetLevelOrLastChildren(Account, DisplayLevel);
						if (levelOrLastChildren.Count == 1 && levelOrLastChildren[0] == Account)
						{
							AddAccount(Account, null);
						}
						else
						{
							C1.Win.C1FlexGrid.Row row = grid.Rows.Add();
							row.UserData = Account;
							row.IsNode = true;
							row["CN_CODE"] = Account.Code;
							row["CN_NAME"] = Account.Name;
							foreach (Account item in levelOrLastChildren)
							{
								AddAccount(item, row.Node);
							}
							for (int j = 0; j < innerYear; j++)
							{
								int num2 = startColIndex + j;
								row[num2] = GetChildrenSum(row.Node, num2);
							}
							int num3 = startColIndex + innerYear;
							row[num3] = GetChildrenSum(row.Node, num3);
							row["CN_TOTAL"] = GetChildrenSum(row.Node, grid.Cols["CN_TOTAL"].Index);
						}
					}
					else if (level == DisplayLevel)
					{
						AddAccount(Account, null);
					}
				}
			}
			else if (DisplayLevel == 0)
			{
				AddAuxiliaryClass(Account, auxiliaryClass2, null);
			}
			else
			{
				C1.Win.C1FlexGrid.Row row2 = grid.Rows.Add();
				row2.UserData = Tuple.Create(Account, auxiliaryClass2);
				row2.IsNode = true;
				row2["CN_CODE"] = auxiliaryClass2.Code;
				row2["CN_NAME"] = auxiliaryClass2.Name;
				foreach (AuxiliaryItem item2 in auxiliaryClass2.Items)
				{
					AddAuxiliaryItem(Account, item2, row2.Node);
				}
				for (int k = 0; k < innerYear; k++)
				{
					int num4 = startColIndex + k;
					row2[num4] = GetChildrenSum(row2.Node, num4);
				}
				int num5 = startColIndex + innerYear;
				row2[num5] = GetChildrenSum(row2.Node, num5);
				row2["CN_TOTAL"] = GetChildrenSum(row2.Node, grid.Cols["CN_TOTAL"].Index);
			}
			grid.Tree.Show(DisplayLevel);
			grid.AutoSizeCols(1, 2, 10);
			SetTitle(Account);
			Node AddAccount(Account acc, Node parent)
			{
				AgeSheet ageSheet2 = ageSheetFactory.GetAgeSheet(acc, _baseDate);
				Dictionary<int, AgeAmount> dictionary2 = AnalyzeAgeSheet(ageSheet2, innerYear);
				C1.Win.C1FlexGrid.Row row4 = ((parent == null) ? grid.Rows.Add() : parent.AddNode(NodeTypeEnum.LastChild, null).Row);
				row4.IsNode = true;
				row4.UserData = acc;
				row4["CN_INDEX"] = index++;
				row4["CN_CODE"] = acc.Code;
				row4["CN_NAME"] = acc.Name;
				decimal num7 = default(decimal);
				for (int n = 0; n < innerYear; n++)
				{
					if (dictionary2.ContainsKey(n))
					{
						decimal displayAmount3 = GetDisplayAmount(dictionary2[n]);
						if (displayAmount3 != 0m)
						{
							row4[startColIndex + n] = displayAmount3;
							num7 += displayAmount3;
						}
					}
				}
				decimal displayAmount4 = GetDisplayAmount(dictionary2[-1]);
				if (displayAmount4 != 0m)
				{
					row4[startColIndex + innerYear] = displayAmount4;
					num7 += displayAmount4;
				}
				row4["CN_TOTAL"] = num7;
				return row4.Node;
			}
			Node AddAuxiliaryClass(Account acc, AuxiliaryClass auxiliaryClass, Node parent)
			{
				AgeSheet ageSheet = ageSheetFactory.GetAgeSheet(acc, auxiliaryClass, _baseDate);
				Dictionary<int, AgeAmount> dictionary = AnalyzeAgeSheet(ageSheet, innerYear);
				C1.Win.C1FlexGrid.Row row3 = ((parent == null) ? grid.Rows.Add() : parent.AddNode(NodeTypeEnum.LastChild, null).Row);
				row3.IsNode = true;
				row3.UserData = Tuple.Create(acc, auxiliaryClass);
				row3["CN_INDEX"] = index++;
				row3["CN_CODE"] = auxiliaryClass.Code;
				row3["CN_NAME"] = auxiliaryClass.Name;
				decimal num6 = default(decimal);
				for (int l = 0; l < innerYear; l++)
				{
					if (dictionary.ContainsKey(l))
					{
						decimal displayAmount = GetDisplayAmount(dictionary[l]);
						if (displayAmount != 0m)
						{
							row3[startColIndex + l] = displayAmount;
							num6 += displayAmount;
						}
					}
				}
				decimal displayAmount2 = GetDisplayAmount(dictionary[-1]);
				if (displayAmount2 != 0m)
				{
					row3[startColIndex + innerYear] = displayAmount2;
					num6 += displayAmount2;
				}
				row3["CN_TOTAL"] = num6;
				return row3.Node;
			}
			Node AddAuxiliaryItem(Account acc, AuxiliaryItem auxiliaryItem, Node parent)
			{
				AgeSheet ageSheet3 = ageSheetFactory.GetAgeSheet(acc, auxiliaryItem, _baseDate);
				Dictionary<int, AgeAmount> dictionary3 = AnalyzeAgeSheet(ageSheet3, innerYear);
				C1.Win.C1FlexGrid.Row row5 = ((parent == null) ? grid.Rows.Add() : parent.AddNode(NodeTypeEnum.LastChild, null).Row);
				row5.IsNode = true;
				row5.UserData = acc;
				row5["CN_INDEX"] = index++;
				row5["CN_CODE"] = auxiliaryItem.Code;
				row5["CN_NAME"] = auxiliaryItem.Name;
				decimal num8 = default(decimal);
				for (int num9 = 0; num9 < innerYear; num9++)
				{
					if (dictionary3.ContainsKey(num9))
					{
						decimal displayAmount5 = GetDisplayAmount(dictionary3[num9]);
						if (displayAmount5 != 0m)
						{
							row5[startColIndex + num9] = displayAmount5;
							num8 += displayAmount5;
						}
					}
				}
				decimal displayAmount6 = GetDisplayAmount(dictionary3[-1]);
				if (displayAmount6 != 0m)
				{
					row5[startColIndex + innerYear] = displayAmount6;
					num8 += displayAmount6;
				}
				row5["CN_TOTAL"] = num8;
				return row5.Node;
			}
		}
		finally
		{
			PendingAllEvent = false;
			grid.EndUpdate();
		}
		static decimal GetChildrenSum(Node node, int c)
		{
			decimal result = default(decimal);
			Node[] nodes = node.Nodes;
			foreach (Node node2 in nodes)
			{
				object obj = node2.Row[c];
				if (obj != null)
				{
					result += decimal.Parse(obj.ToString());
				}
			}
			return result;
		}
	}

	public bool CanAnalyze()
	{
		DateTime baseDate = BaseDate;
		if (new DateTime(baseDate.Year, baseDate.Month, baseDate.Day).AddYears(-1).AddDays(1.0) < Ledger.StartDate)
		{
			return false;
		}
		return true;
	}

	private void Grid_OwnerDrawCell(object sender, OwnerDrawCellEventArgs e)
	{
		if (e.Col == 0 && e.Row >= grid.Rows.Fixed)
		{
			e.Text = (e.Row - grid.Rows.Fixed + 1).ToString();
		}
	}

	private void Grid_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (!PendingAllEvent && grid.MouseRow >= grid.Rows.Fixed && grid.MouseCol >= grid.Cols.Fixed)
		{
			RowVM rowVM = grid.Rows[grid.Row].UserData as RowVM;
			_owner.SwitchToView(ActiveView.Subsidiary);
			if (rowVM.Item == null)
			{
				_owner.SubsidiaryEditor.PopulateSubsidiarySheet(rowVM.Account, _owner.StartDate, _owner.EndDate);
				_owner.SubsidiaryEditor.UpdateTitle(rowVM.Account);
				_owner.AccountTreeEditor.UpdateNodeStatus(rowVM.Account);
			}
			else
			{
				_owner.SubsidiaryEditor.PopulateSubsidiarySheet(rowVM.Account, _owner.StartDate, _owner.EndDate, rowVM.Item);
				_owner.SubsidiaryEditor.UpdateTitle(rowVM.Account, rowVM.Item);
				_owner.AccountTreeEditor.UpdateNodeStatus(Tuple.Create(rowVM.Account, rowVM.Item));
			}
		}
	}
}
