using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1FlexGrid;
using Auditai.Model;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls.CollectTable;

public class SelectorEditor
{
	private const string FORMATSTRING_MONEY = "#,0.00;-#,0.00;#";

	private const string CN_CODE = "Code";

	private const string CN_NAME = "Name";

	private const string CN_DEBIT = "CurrentDebit";

	private const string CN_CREDIT = "CurrentCredit";

	private const string CN_DATE = "Date";

	private const string CN_TYPE = "Type";

	private const string CN_NUMBER = "Number";

	private const string CN_DIGEST = "Digest";

	private const string CN_OPPOSITE = "Opposite";

	private const string CN_DC = "DC";

	private const string CN_BALANCE = "Balance";

	private const string CN_INDEX = "Index";

	private const string CN_CHECK = "Check";

	private const string CN_SUMMARY_PROJECT = "ProjectName";

	private const string CN_BALANCE_BEGINDC = "BeginDC";

	private const string CN_BALANCE_BEGINBALANCE = "BeginBalance";

	private const string CN_BALANCE_ENDDC = "EndDC";

	private const string CN_BALANCE_ENDBALANCE = "EndBalance";

	private List<Tuple<DateTime, DateTime, TrialBalanceSheet>> _trialBalanceSheetCache = new List<Tuple<DateTime, DateTime, TrialBalanceSheet>>();

	private C1CommandHolder commandHolder = new C1CommandHolder();

	private readonly C1ContextMenu contextMenu = new C1ContextMenu();

	private C1CommandLink lnkUpLevel = new C1CommandLink();

	private C1Command cmdUpLevel = new C1Command();

	private C1CommandLink lnkDownLevel = new C1CommandLink();

	private C1Command cmdDownLevel = new C1Command();

	private C1CommandLink lnkSwitchAccNameStyle = new C1CommandLink();

	private C1Command cmdSwitchAccNameStyle = new C1Command();

	private List<C1CommandLink> _listToLevelLinks = new List<C1CommandLink>();

	private readonly C1Command _cmdSortAsc;

	private readonly C1CommandLink _lnkSortAsc;

	private readonly C1Command _cmdSortDesc;

	private readonly C1CommandLink _lnkSortDesc;

	private readonly C1Command _cmdCancelSort;

	private readonly C1CommandLink _lnkCancelSort;

	private readonly C1ContextMenu _ctxColHeader;

	private readonly Color HightLightColor = Color.LightYellow;

	private TableCollectorAbstract _collector;

	private AccNameStyleEnum CurrentAccNameStyle = AccNameStyleEnum.SecondFullName;

	private int maxLevel = -1;

	private bool penddingCheckEvent;

	public C1FlexGridEx View { get; private set; }

	private TrialBalanceSheet GetTrialBalanceSheetWithCache(Ledger ledger, DateTime start, DateTime end)
	{
		Tuple<DateTime, DateTime, TrialBalanceSheet> tuple = _trialBalanceSheetCache.Find((Tuple<DateTime, DateTime, TrialBalanceSheet> tp) => tp.Item1.Equals(start) && tp.Item2.Equals(end));
		if (tuple != null)
		{
			return tuple.Item3;
		}
		TrialBalanceSheet trialBalanceSheet = ledger.GetTrialBalanceSheet(start, end);
		_trialBalanceSheetCache.Add(Tuple.Create(start, end, trialBalanceSheet));
		return trialBalanceSheet;
	}

	private TrialBalanceSheet GetTrialBalanceSheetWithCache(Setting setting)
	{
		return GetTrialBalanceSheetWithCache(setting.Ledger, setting.Start, setting.End);
	}

	public SelectorEditor(Form owner, TableCollectorAbstract collector)
	{
		View = new C1FlexGridEx();
		_collector = collector;
		_ctxColHeader = new C1ContextMenu();
		_cmdSortDesc = new C1Command
		{
			Text = "降序排列",
			Image = Resources.ctxDescending
		};
		_cmdSortDesc.CommandStateQuery += _cmdSortDesc_CommandStateQuery;
		_cmdSortDesc.Click += _cmdSortDesc_Click;
		_lnkSortDesc = new C1CommandLink(_cmdSortDesc);
		_ctxColHeader.CommandLinks.Add(_lnkSortDesc);
		_cmdSortAsc = new C1Command
		{
			Text = "升序排列",
			Image = Resources.ctxAscending
		};
		_cmdSortAsc.CommandStateQuery += _cmdSortAsc_CommandStateQuery;
		_cmdSortAsc.Click += _cmdSortAsc_Click;
		_lnkSortAsc = new C1CommandLink(_cmdSortAsc);
		_ctxColHeader.CommandLinks.Add(_lnkSortAsc);
		_cmdCancelSort = new C1Command
		{
			Text = "取消排序"
		};
		_cmdCancelSort.CommandStateQuery += _cmdCancelSort_CommandStateQuery;
		_cmdCancelSort.Click += _cmdCancelSort_Click;
		_lnkCancelSort = new C1CommandLink(_cmdCancelSort);
		_ctxColHeader.CommandLinks.Add(_lnkCancelSort);
		Initialize(owner);
		View.MouseDown += View_MouseDown;
		View.MouseDoubleClick += View_MouseDoubleClick;
		Populate();
	}

	public void SetCollector(TableCollectorAbstract collector)
	{
		_collector = collector;
	}

	public TableCollectResult GetResult()
	{
		_collector.Source = SelectedRows();
		int year = _collector.TitlePeriod.Item1.Year;
		return _collector.Collect(year);
	}

	public List<object> SelectedRows()
	{
		List<object> list = new List<object>();
		for (int i = View.Rows.Fixed; i < View.Rows.Count; i++)
		{
			CheckEnum cellCheck = View.GetCellCheck(i, 0);
			if (cellCheck == CheckEnum.Checked && View.Rows[i].IsVisible)
			{
				object userData = View.Rows[i].UserData;
				if (userData != null)
				{
					list.Add(userData);
				}
			}
		}
		return list;
	}

	public void SelectAll()
	{
		bool flag = penddingCheckEvent;
		if (!flag)
		{
			penddingCheckEvent = true;
		}
		try
		{
			for (int i = 0; i < View.Rows.Count; i++)
			{
				View.SetCellCheck(i, 0, CheckEnum.Checked);
				View.Rows[i].StyleNew.BackColor = HightLightColor;
			}
		}
		finally
		{
			if (!flag)
			{
				penddingCheckEvent = false;
			}
		}
	}

	public void Populate(int? level = null)
	{
		if (_collector.Setting.Account == null)
		{
			View.Rows.Count = 0;
			return;
		}
		int year = _collector.TitlePeriod.Item1.Year;
		int year2 = _collector.Setting.Ledger.StartDate.Year;
		int year3 = _collector.Setting.Ledger.GetEndDate().Year;
		switch (_collector.CollectObject)
		{
		case CollectObjectEnum.Balance:
		{
			int year4 = ((year < year2 || year > year3) ? _collector.Setting.Ledger.GetEndDate().Year : year);
			DateTime start = _collector.Setting.Start.CopyToSpecificYear(year4);
			DateTime end = _collector.Setting.End.CopyToSpecificYear(year4);
			object auxiliary2 = _collector.Setting.Auxiliary;
			if (!(auxiliary2 is AuxiliaryClass auxClass))
			{
				AuxiliaryItem auxiliaryItem2 = auxiliary2 as AuxiliaryItem;
				if (auxiliaryItem2 == null)
				{
					PopulateBalance(_collector.Setting.Account, start, end, level);
				}
			}
			else
			{
				PopulateBalance(_collector.Setting.Account, start, end, auxClass);
			}
			break;
		}
		case CollectObjectEnum.Subsidiary:
		{
			DateTime start2 = new DateTime(year, 1, 1);
			DateTime end2 = new DateTime(year + 1, 12, DateTime.DaysInMonth(year + 1, 12));
			object auxiliary3 = _collector.Setting.Auxiliary;
			if (!(auxiliary3 is AuxiliaryClass auxiliaryClass2))
			{
				if (auxiliary3 is AuxiliaryItem auxiliaryItem3)
				{
					PopulateSubsidiary(_collector.Setting.Account, start2, end2, auxiliaryItem3);
				}
				else
				{
					PopulateSubsidiary(_collector.Setting.Account, start2, end2);
				}
			}
			else
			{
				PopulateSubsidiary(_collector.Setting.Account, start2, end2, auxiliaryClass2);
			}
			break;
		}
		case CollectObjectEnum.Summary:
		{
			object auxiliary = _collector.Setting.Auxiliary;
			if (!(auxiliary is AuxiliaryClass auxiliaryClass))
			{
				if (auxiliary is AuxiliaryItem auxiliaryItem)
				{
					PopulateSummary(_collector.Setting.Account, auxiliaryItem, (_collector as TableCollectorSummary).AnalysisProject);
				}
				else
				{
					PopulateSummary(_collector.Setting.Account, (_collector as TableCollectorSummary).AnalysisProject);
				}
			}
			else
			{
				PopulateSummary(_collector.Setting.Account, auxiliaryClass, (_collector as TableCollectorSummary).AnalysisProject);
			}
			break;
		}
		default:
			View.Rows.Count = 1;
			return;
		}
		if (_collector.CollectObject == CollectObjectEnum.Balance && _collector.Setting.Account != null)
		{
			maxLevel = GetMaxLevel(_collector.Setting.Account);
		}
		else
		{
			maxLevel = -1;
		}
		SelectAll();
		PopulateIndex();
		AutoSizeCols();
	}

	private void Initialize(Form owner)
	{
		View.AllowSorting = AllowSortingEnum.None;
		View.Rows.DefaultSize = 30;
		View.CellChecked += _grid_CellChecked;
		contextMenu.CommandLinks.Add(View.FilterManager.GenLnkFilter());
		contextMenu.CommandLinks.Add(View.FilterManager.GenLnkSample());
		contextMenu.CommandLinks.Add(View.FilterManager.GenLnkSelect());
		contextMenu.CommandLinks.Add(View.FilterManager.GenLnkCancelCurrentColumn());
		cmdUpLevel.Text = "上提一级";
		cmdUpLevel.Click += delegate
		{
			CellRange selection2 = View.Selection;
			if (selection2.TopRow == selection2.BottomRow)
			{
				C1.Win.C1FlexGrid.Row row2 = View.Rows[View.Row];
				if (CanUp(row2.Index))
				{
					UpLevel(row2);
				}
			}
			else
			{
				List<C1.Win.C1FlexGrid.Row> list2 = new List<C1.Win.C1FlexGrid.Row>();
				for (int j = selection2.TopRow; j <= selection2.BottomRow; j++)
				{
					list2.Add(View.Rows[j]);
				}
				GetBatchDownInfo(list2, out var ret, out var ret2);
				if (ret2 != null)
				{
					foreach (C1.Win.C1FlexGrid.Row item in ret2)
					{
						if (item.Index > -1 && CanUp(item.Index))
						{
							UpLevel(item);
						}
					}
					return;
				}
				if (ret != null)
				{
					foreach (C1.Win.C1FlexGrid.Row item2 in ret)
					{
						if (item2.Index > -1 && CanUp(item2.Index))
						{
							UpLevel(item2);
						}
					}
				}
			}
		};
		cmdUpLevel.CommandStateQuery += CmdUpLevel_CommandStateQuery;
		lnkUpLevel.Command = cmdUpLevel;
		contextMenu.CommandLinks.Add(lnkUpLevel);
		cmdDownLevel.Text = "下沉一级";
		cmdDownLevel.Click += delegate
		{
			CellRange selection = View.Selection;
			if (selection.BottomRow == selection.TopRow)
			{
				C1.Win.C1FlexGrid.Row row = View.Rows[View.Row];
				if (CanDown(row.Index))
				{
					DownLevel(row);
				}
				return;
			}
			List<C1.Win.C1FlexGrid.Row> list = new List<C1.Win.C1FlexGrid.Row>();
			for (int i = selection.TopRow; i <= selection.BottomRow; i++)
			{
				list.Add(View.Rows[i]);
			}
			foreach (C1.Win.C1FlexGrid.Row item3 in list)
			{
				if (item3.Index > -1 && CanDown(item3.Index))
				{
					DownLevel(item3);
				}
			}
		};
		cmdDownLevel.CommandStateQuery += CmdDownLevel_CommandStateQuery;
		lnkDownLevel.Command = cmdDownLevel;
		contextMenu.CommandLinks.Add(lnkDownLevel);
		cmdSwitchAccNameStyle.Text = "切换科目名称样式";
		cmdSwitchAccNameStyle.Click += delegate
		{
			SwitchAccNameStyle();
		};
		cmdSwitchAccNameStyle.CommandStateQuery += delegate(object s1, CommandStateQueryEventArgs e1)
		{
			e1.Visible = _collector.CollectObject == CollectObjectEnum.Balance;
		};
		lnkSwitchAccNameStyle.Command = cmdSwitchAccNameStyle;
		lnkSwitchAccNameStyle.Delimiter = true;
		contextMenu.CommandLinks.Add(lnkSwitchAccNameStyle);
		commandHolder.Owner = owner;
		commandHolder.Commands.Add(contextMenu);
		contextMenu.Popup += ContextMenu_Popup;
	}

	private void SwitchAccNameStyle()
	{
		if (_collector.CollectObject != 0 || !(_collector is TableCollectorBalance tableCollectorBalance))
		{
			return;
		}
		if (tableCollectorBalance.AccNameStyle == AccNameStyleEnum.Normal)
		{
			CurrentAccNameStyle = AccNameStyleEnum.SecondFullName;
			tableCollectorBalance.AccNameStyle = AccNameStyleEnum.SecondFullName;
			for (int i = View.Rows.Fixed; i < View.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row = View.Rows[i];
				if (row.UserData is Account account)
				{
					View.Rows[i]["Name"] = ex1.GetSecondFullName(account);
				}
				else if (row.UserData is Tuple<Account, AuxiliaryItem> tuple)
				{
					row["Name"] = ex1.GetSecondFullName(tuple.Item1, tuple.Item2);
				}
			}
		}
		else
		{
			if (tableCollectorBalance.AccNameStyle != AccNameStyleEnum.SecondFullName)
			{
				return;
			}
			CurrentAccNameStyle = AccNameStyleEnum.Normal;
			tableCollectorBalance.AccNameStyle = AccNameStyleEnum.Normal;
			for (int j = View.Rows.Fixed; j < View.Rows.Count; j++)
			{
				C1.Win.C1FlexGrid.Row row2 = View.Rows[j];
				if (row2.UserData is Account account2)
				{
					row2["Name"] = account2.Name;
				}
				else if (row2.UserData is Tuple<Account, AuxiliaryItem> tuple2)
				{
					row2["Name"] = tuple2.Item2.Name;
				}
			}
		}
	}

	private void ContextMenu_Popup(object sender, EventArgs e)
	{
		List<C1CommandLink> list = new List<C1CommandLink>();
		foreach (C1CommandLink commandLink in contextMenu.CommandLinks)
		{
			if (commandLink.Command.UserData is Tuple<Account, AuxiliaryClass>)
			{
				list.Add(commandLink);
			}
		}
		foreach (C1CommandLink item in list)
		{
			contextMenu.CommandLinks.Remove(item);
		}
		if (View.Row < 0 || View.Row >= View.Rows.Count)
		{
			return;
		}
		C1.Win.C1FlexGrid.Row row = View.Rows[View.Row];
		object userData = row.UserData;
		Account account = userData as Account;
		if (account != null)
		{
			Dictionary<AuxiliaryClass, ClassBalance> classBalances = GetTrialBalanceSheetWithCache(_collector.Setting).End[account].ClassBalances;
			foreach (AuxiliaryClass auxClass2 in classBalances.Keys)
			{
				C1CommandLink c1CommandLink2 = new C1CommandLink();
				C1Command c1Command = new C1Command();
				c1Command.Text = "展开" + auxClass2.Name + "核算";
				c1Command.UserData = Tuple.Create(account, auxClass2);
				c1Command.Click += delegate
				{
					DownAux(account, auxClass2);
				};
				c1CommandLink2.Command = c1Command;
				contextMenu.CommandLinks.Add(c1CommandLink2);
			}
		}
		else
		{
			userData = row.UserData;
			Tuple<Account, AuxiliaryItem> tp = userData as Tuple<Account, AuxiliaryItem>;
			if (tp != null)
			{
				Dictionary<AuxiliaryClass, ClassBalance> classBalances2 = GetTrialBalanceSheetWithCache(_collector.Setting).End[tp.Item1].ClassBalances;
				foreach (AuxiliaryClass auxClass in classBalances2.Keys)
				{
					if (auxClass != tp.Item2.Class)
					{
						C1CommandLink c1CommandLink3 = new C1CommandLink();
						C1Command c1Command2 = new C1Command();
						c1Command2.Text = "切换" + auxClass.Name + "核算";
						c1Command2.UserData = Tuple.Create(tp.Item1, auxClass);
						c1Command2.Click += delegate
						{
							SwitchAux(tp.Item1, auxClass);
						};
						c1CommandLink3.Command = c1Command2;
						contextMenu.CommandLinks.Add(c1CommandLink3);
					}
				}
			}
		}
		foreach (C1CommandLink listToLevelLink in _listToLevelLinks)
		{
			contextMenu.CommandLinks.Remove(listToLevelLink);
		}
		_listToLevelLinks.Clear();
		for (int i = _collector.Setting.Account.GetLevel(); i < maxLevel; i++)
		{
			C1CommandLink c1CommandLink4 = new C1CommandLink();
			C1Command c1Command3 = new C1Command();
			c1Command3.Text = $"显示至{i + 1}级";
			c1Command3.UserData = i + 1;
			if (i == _collector.Setting.Account.GetLevel())
			{
				c1CommandLink4.Delimiter = true;
			}
			c1Command3.Click += delegate(object s1, ClickEventArgs e1)
			{
				if (s1 is C1Command c1Command4)
				{
					Populate((int)c1Command4.UserData);
				}
			};
			c1CommandLink4.Command = c1Command3;
			contextMenu.CommandLinks.Add(c1CommandLink4);
			_listToLevelLinks.Add(c1CommandLink4);
		}
	}

	private bool CanUp(int r)
	{
		if (r < View.Rows.Fixed || r >= View.Rows.Count)
		{
			return false;
		}
		if (View.Rows[r].UserData is Account { Parent: not null })
		{
			return true;
		}
		if (View.Rows[r].UserData is Tuple<Account, AuxiliaryItem>)
		{
			return true;
		}
		return false;
	}

	private bool CanDown(int r)
	{
		if (r < View.Rows.Fixed || r >= View.Rows.Count)
		{
			return false;
		}
		if (View.Rows[r].UserData is Account account && account.Children.Count > 0)
		{
			return true;
		}
		return false;
	}

	private int GetMaxLevel(Account acc)
	{
		int max = 0;
		foreachNode(acc, 1);
		return max;
		void foreachNode(Account a, int level)
		{
			if (level > max)
			{
				max = level;
			}
			foreach (Account child in a.Children)
			{
				foreachNode(child, level + 1);
			}
		}
	}

	private void PopulateBalance(Account account, DateTime start, DateTime end, int? level = null)
	{
		bool shouldShowAux = !level.HasValue || level.Value == maxLevel;
		View.Cols.Count = 0;
		View.Rows.Count = 1;
		View.Rows.Fixed = 1;
		C1.Win.C1FlexGrid.CellStyle cellStyle = View.Styles.Add("checkColStyle");
		cellStyle.ImageAlign = ImageAlignEnum.CenterCenter;
		View.BeginUpdate();
		TrialBalanceSheet sheet;
		int Index;
		try
		{
			C1.Win.C1FlexGrid.Column column = View.Cols.Add();
			column.Name = "Check";
			column.Style = cellStyle;
			column.DataType = typeof(string);
			column = View.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = View.Cols.Add();
			column.Name = "Code";
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column = View.Cols.Add();
			column.Name = "Name";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column = View.Cols.Add();
			column.Name = "BeginDC";
			column.Caption = "期初余额方向";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = View.Cols.Add();
			column.Name = "BeginBalance";
			column.Caption = "期初余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = View.Cols.Add();
			column.Name = "CurrentDebit";
			column.Caption = "借方发生额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = View.Cols.Add();
			column.Name = "CurrentCredit";
			column.Caption = "贷方发生额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = View.Cols.Add();
			column.Name = "EndDC";
			column.Caption = "期末余额方向";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = View.Cols.Add();
			column.Name = "EndBalance";
			column.Caption = "期末余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			View.Rows.Fixed = 1;
			View.Cols.Fixed = 1;
			View.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			View.SetCellCheck(0, 0, CheckEnum.Unchecked);
			View.SetCellStyle(0, 0, cellStyle);
			if (account == null)
			{
				return;
			}
			sheet = _collector.Setting.Ledger.GetTrialBalanceSheet(start, end);
			Index = 1;
			if (level.HasValue)
			{
				populateLevel(account, level.Value);
			}
			else
			{
				populateLevel(account, GetMaxLevel(account));
			}
			foreach (C1.Win.C1FlexGrid.Column item in (IEnumerable)View.Cols)
			{
				if (item.Name == "Check")
				{
					item.AllowEditing = true;
				}
				else
				{
					item.AllowEditing = false;
				}
			}
			PopulateIndex();
			AutoSizeCols();
			void populateLevel(Account acc1, int target)
			{
				populateImpl(acc1, 1);
				void populateImpl(Account acc2, int lev)
				{
					if (lev <= target)
					{
						if (lev == target)
						{
							addAccount(acc2);
						}
						else
						{
							if (acc2.Children.Count != 0)
							{
								foreach (Account child in acc2.Children)
								{
									populateImpl(child, lev + 1);
								}
								return;
							}
							addAccount(acc2);
						}
					}
				}
			}
		}
		finally
		{
			View.EndUpdate();
		}
		void addAccount(Account acc3)
		{
			if ((_collector.Setting.Account == acc3 && _collector.Setting.Auxiliary is AuxiliaryClass) || (_collector.Setting.Account != acc3 && sheet.End[acc3].ClassBalances.Count > 0 && shouldShowAux))
			{
				AuxiliaryClass key = ((_collector.Setting.Account != acc3 || !(_collector.Setting.Auxiliary is AuxiliaryClass auxiliaryClass)) ? TableCollectorAbstract.GetFirstOrDefaultAuxiliary(acc3.Ledger, acc3, sheet) : auxiliaryClass);
				ClassBalance classBalance = sheet.End[acc3].ClassBalances[key];
				{
					foreach (KeyValuePair<AuxiliaryItem, decimal> item2 in classBalance.ItemBalances.OrderBy((KeyValuePair<AuxiliaryItem, decimal> t) => t.Key.Code))
					{
						AuxiliaryItem key2 = item2.Key;
						SubsidiaryLedger subsidiaryLedger = _collector.Setting.Ledger.GetSubsidiaryLedger(acc3, start, end, key2);
						SubsidiaryLedgerTotal subsidiaryLedgerTotal = subsidiaryLedger.Months.LastOrDefault()?.GrandTotal;
						C1.Win.C1FlexGrid.Row row = View.Rows.Add();
						row.UserData = Tuple.Create(acc3, key2);
						row["Code"] = acc3.Code + "-" + key2.Code;
						row["Name"] = ((CurrentAccNameStyle == AccNameStyleEnum.SecondFullName) ? ex1.GetSecondFullName(acc3, key2) : key2.Name);
						row["BeginDC"] = GetDCChar(acc3.IsDebit, subsidiaryLedger.BeginBalance);
						row["BeginBalance"] = Math.Abs(subsidiaryLedger.BeginBalance);
						row["CurrentDebit"] = subsidiaryLedgerTotal?.Debit ?? 0m;
						row["CurrentCredit"] = subsidiaryLedgerTotal?.Credit ?? 0m;
						decimal num = subsidiaryLedgerTotal?.Balance ?? subsidiaryLedger.BeginBalance;
						row["EndDC"] = GetDCChar(acc3.IsDebit, num);
						row["EndBalance"] = Math.Abs(num);
						(row.Style ?? row.StyleNew).ForeColor = Color.Purple;
						View.SetCellCheck(row.Index, 0, CheckEnum.Unchecked);
					}
					return;
				}
			}
			C1.Win.C1FlexGrid.Row row2 = View.Rows.Add();
			row2.UserData = acc3;
			row2["Index"] = Index++.ToString();
			row2["Code"] = acc3.Code;
			row2["Name"] = ((CurrentAccNameStyle == AccNameStyleEnum.SecondFullName) ? ex1.GetSecondFullName(acc3) : acc3.Name);
			row2["BeginDC"] = GetDCChar(acc3.IsDebit, sheet.Start[acc3].Total);
			row2["BeginBalance"] = Math.Abs(sheet.Start[acc3].Total);
			row2["CurrentDebit"] = (sheet.Debit.ContainsKey(acc3) ? ((object)sheet.Debit[acc3].Total) : null);
			row2["CurrentCredit"] = (sheet.Credit.ContainsKey(acc3) ? ((object)sheet.Credit[acc3].Total) : null);
			row2["EndDC"] = GetDCChar(acc3.IsDebit, sheet.End[acc3].Total);
			row2["EndBalance"] = Math.Abs(sheet.End[acc3].Total);
			View.SetCellCheck(row2.Index, 0, CheckEnum.Unchecked);
		}
	}

	private void PopulateBalance(Account account, DateTime start, DateTime end, AuxiliaryClass auxClass)
	{
		View.BeginUpdate();
		try
		{
			View.Cols.Count = 0;
			View.Rows.Count = 1;
			View.Rows.Fixed = 1;
			C1.Win.C1FlexGrid.CellStyle cellStyle = View.Styles.Add("checkColStyle");
			cellStyle.ImageAlign = ImageAlignEnum.CenterCenter;
			C1.Win.C1FlexGrid.Column column = View.Cols.Add();
			column.Name = "Check";
			column.Style = cellStyle;
			column.DataType = typeof(string);
			column = View.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = View.Cols.Add();
			column.Name = "Code";
			column.Caption = "科目代码";
			column.DataType = typeof(string);
			column = View.Cols.Add();
			column.Name = "Name";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column = View.Cols.Add();
			column.Name = "BeginDC";
			column.Caption = "期初余额方向";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = View.Cols.Add();
			column.Name = "BeginBalance";
			column.Caption = "期初余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = View.Cols.Add();
			column.Name = "CurrentDebit";
			column.Caption = "借方发生额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = View.Cols.Add();
			column.Name = "CurrentCredit";
			column.Caption = "贷方发生额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = View.Cols.Add();
			column.Name = "EndDC";
			column.Caption = "期末余额方向";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = View.Cols.Add();
			column.Name = "EndBalance";
			column.Caption = "期末余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			View.Rows.Fixed = 1;
			View.Cols.Fixed = 1;
			View.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			View.SetCellCheck(0, 0, CheckEnum.Unchecked);
			View.SetCellStyle(0, 0, cellStyle);
			if (account == null)
			{
				return;
			}
			TrialBalanceSheet trialBalanceSheet = _collector.Setting.Ledger.GetTrialBalanceSheet(start, end);
			Dictionary<AuxiliaryClass, ClassBalance> classBalances = trialBalanceSheet.End[account].ClassBalances;
			if (classBalances.Count > 0)
			{
				foreach (KeyValuePair<AuxiliaryItem, decimal> item in classBalances[auxClass].ItemBalances.OrderBy((KeyValuePair<AuxiliaryItem, decimal> t) => t.Key.Code))
				{
					AuxiliaryItem key = item.Key;
					SubsidiaryLedger subsidiaryLedger = _collector.Setting.Ledger.GetSubsidiaryLedger(account, start, end, key);
					SubsidiaryLedgerTotal subsidiaryLedgerTotal = subsidiaryLedger.Months.LastOrDefault()?.GrandTotal;
					C1.Win.C1FlexGrid.Row row = View.Rows.Add();
					row.UserData = Tuple.Create(account, key);
					row["Code"] = account.Code + "-" + key.Code;
					row["Name"] = ((CurrentAccNameStyle == AccNameStyleEnum.SecondFullName) ? ex1.GetSecondFullName(account, key) : key.Name);
					row["BeginDC"] = GetDCChar(account.IsDebit, subsidiaryLedger.BeginBalance);
					row["BeginBalance"] = Math.Abs(subsidiaryLedger.BeginBalance);
					row["CurrentDebit"] = subsidiaryLedgerTotal?.Debit ?? 0m;
					row["CurrentCredit"] = subsidiaryLedgerTotal?.Credit ?? 0m;
					decimal num = subsidiaryLedgerTotal?.Balance ?? subsidiaryLedger.BeginBalance;
					row["EndDC"] = GetDCChar(account.IsDebit, num);
					row["EndBalance"] = Math.Abs(num);
					(row.Style ?? row.StyleNew).ForeColor = Color.Purple;
					View.SetCellCheck(row.Index, 0, CheckEnum.Unchecked);
				}
			}
			PopulateIndex();
			AutoSizeCols();
			foreach (C1.Win.C1FlexGrid.Column item2 in (IEnumerable)View.Cols)
			{
				if (item2.Name == "Check")
				{
					item2.AllowEditing = true;
				}
				else
				{
					item2.AllowEditing = false;
				}
			}
		}
		finally
		{
			View.EndUpdate();
		}
	}

	private void PopulateSubsidiary(Account account, DateTime start, DateTime end)
	{
		PopulateSubsidiary(account, start, end, (account == null) ? null : _collector.Setting.Ledger.GetSubsidiaryLedger(account, start, end));
	}

	private void PopulateSubsidiary(Account account, DateTime start, DateTime end, AuxiliaryItem auxiliaryItem)
	{
		PopulateSubsidiary(account, start, end, (account == null) ? null : _collector.Setting.Ledger.GetSubsidiaryLedger(account, start, end, auxiliaryItem));
	}

	private void PopulateSubsidiary(Account account, DateTime start, DateTime end, AuxiliaryClass auxiliaryClass)
	{
		PopulateSubsidiary(account, start, end, (account == null) ? null : _collector.Setting.Ledger.GetSubsidiaryLedger(account, start, end, auxiliaryClass));
	}

	private void PopulateSubsidiary(Account account, DateTime start, DateTime end, SubsidiaryLedger subsidiaryLedger)
	{
		View.Cols.Count = 0;
		View.Rows.Count = 1;
		View.Rows.Fixed = 1;
		C1.Win.C1FlexGrid.CellStyle cellStyle = View.Styles.Add("checkColStyle");
		cellStyle.ImageAlign = ImageAlignEnum.CenterCenter;
		View.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.Column column = View.Cols.Add();
			column.Name = "Check";
			column.Style = cellStyle;
			column.DataType = typeof(string);
			column = View.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = View.Cols.Add();
			column.Name = "Date";
			column.Caption = "日期";
			column.DataType = typeof(DateTime);
			column = View.Cols.Add();
			column.Name = "Type";
			column.Caption = "字";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = View.Cols.Add();
			column.Name = "Number";
			column.Caption = "号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = View.Cols.Add();
			column.Name = "Digest";
			column.Caption = "摘要";
			column.DataType = typeof(string);
			column = View.Cols.Add();
			column.Name = "Name";
			column.Caption = "科目名称";
			column.DataType = typeof(string);
			column = View.Cols.Add();
			column.Name = "Opposite";
			column.Caption = "对方科目";
			column.DataType = typeof(string);
			column = View.Cols.Add();
			column.Name = "CurrentDebit";
			column.Caption = "借方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = View.Cols.Add();
			column.Name = "CurrentCredit";
			column.Caption = "贷方金额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			column = View.Cols.Add();
			column.Name = "DC";
			column.Caption = "方向";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = View.Cols.Add();
			column.Name = "Balance";
			column.Caption = "余额";
			column.DataType = typeof(decimal);
			column.Format = "#,0.00;-#,0.00;#";
			View.Rows.Fixed = 1;
			View.Cols.Fixed = 1;
			View.Styles.Fixed.TextAlign = TextAlignEnum.CenterCenter;
			View.SetCellCheck(0, 0, CheckEnum.Unchecked);
			View.SetCellStyle(0, 0, cellStyle);
			if (account == null)
			{
				return;
			}
			int num = 1;
			foreach (MonthSubsidiaryLedger item in from t in subsidiaryLedger.Months
				orderby t.Year, t.Month
				select t)
			{
				List<SubsidiaryLedgerEntry> list = item.Entries.OrderBy((SubsidiaryLedgerEntry t) => t.Voucher.Day).ThenBy((SubsidiaryLedgerEntry s) => s.Voucher.Type.Name).ThenBy((SubsidiaryLedgerEntry m) => m.Voucher.Number, StringNumberComparer.Instance)
					.ToList();
				foreach (SubsidiaryLedgerEntry item2 in list)
				{
					C1.Win.C1FlexGrid.Row row = View.Rows.Add();
					row.UserData = item2.Voucher;
					row["Index"] = num++.ToString();
					row["Date"] = item2.Voucher.Day;
					row["Type"] = item2.Voucher.Type.Name;
					row["Number"] = item2.Voucher.Number;
					row["Name"] = item2.Voucher.GetDisplayAccountNameWithDetail();
					row["Digest"] = item2.Voucher.Digest;
					row["Opposite"] = string.Join(",", item2.Voucher.OppositeAccounts.Select((Account t) => t.Name).Distinct());
					row["CurrentDebit"] = (item2.Voucher.IsDebit ? item2.Voucher.Amount : 0m);
					row["CurrentCredit"] = (item2.Voucher.IsDebit ? 0m : item2.Voucher.Amount);
					row["DC"] = GetDCChar(account.IsDebit, item2.Balance);
					row["Balance"] = Math.Abs(item2.Balance);
					View.SetCellCheck(row.Index, 0, CheckEnum.Unchecked);
					View.SetCellStyle(row.Index, 0, cellStyle);
				}
			}
			PopulateIndex();
			AutoSizeCols();
			foreach (C1.Win.C1FlexGrid.Column item3 in (IEnumerable)View.Cols)
			{
				if (item3.Name == "Check")
				{
					item3.AllowEditing = true;
				}
				else
				{
					item3.AllowEditing = false;
				}
			}
		}
		finally
		{
			View.EndUpdate();
		}
	}

	private void PopulateSummary(Account account, AnalysisProject analysis)
	{
		View.Rows.Count = 1;
		View.Rows.Fixed = 1;
		View.Cols.Count = 0;
		View.Rows.DefaultSize = 30;
		C1.Win.C1FlexGrid.CellStyle cellStyle = View.Styles.Add("checkColStyle");
		cellStyle.ImageAlign = ImageAlignEnum.CenterCenter;
		View.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.Column column = View.Cols.Add();
			column.Name = "Check";
			column.DataType = typeof(string);
			column.Style = cellStyle;
			column = View.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = View.Cols.Add();
			column.Name = "ProjectName";
			column.Caption = "项目";
			column.DataType = typeof(string);
			for (int i = 1; i <= 12; i++)
			{
				column = View.Cols.Add();
				column.Name = $"mon{i}";
				column.Caption = $"{i}月";
				column.DataType = typeof(decimal);
				column.Format = "#,0.00;-#,0.00;#";
			}
			View.SetCellCheck(0, 0, CheckEnum.Unchecked);
			View.SetCellStyle(0, 0, cellStyle);
			if (account == null)
			{
				return;
			}
			foreach (object item in GetLastLevelAccountChildOrAuxItemChild(account))
			{
				bool flag = false;
				string empty = string.Empty;
				SubsidiaryLedger subsidiaryLedger = null;
				if (!(item is Account account2))
				{
					if (!(item is Tuple<Account, AuxiliaryItem> tuple))
					{
						continue;
					}
					flag = true;
					empty = tuple.Item2.Name;
					subsidiaryLedger = _collector.Setting.Ledger.GetSubsidiaryLedger(tuple.Item1, _collector.Setting.Start, _collector.Setting.End, tuple.Item2);
				}
				else
				{
					empty = account2.Name;
					subsidiaryLedger = _collector.Setting.Ledger.GetSubsidiaryLedger(account2, _collector.Setting.Start, _collector.Setting.End);
				}
				C1.Win.C1FlexGrid.Row row = View.Rows.Add();
				row.UserData = item;
				row["ProjectName"] = empty;
				int month;
				for (month = 1; month <= 12; month++)
				{
					MonthSubsidiaryLedger monthSubsidiaryLedger = subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == month);
					if (monthSubsidiaryLedger != null)
					{
						decimal totalValue = monthSubsidiaryLedger.Total.GetTotalValue(analysis);
						row[$"mon{month}"] = totalValue;
					}
				}
				View.SetCellCheck(row.Index, 0, CheckEnum.Unchecked);
				if (flag)
				{
					C1.Win.C1FlexGrid.CellStyle cellStyle2 = row.Style ?? row.StyleNew;
					cellStyle2.ForeColor = Color.Purple;
				}
			}
			PopulateIndex();
			AutoSizeCols();
			foreach (C1.Win.C1FlexGrid.Column item2 in (IEnumerable)View.Cols)
			{
				if (item2.Name == "Check")
				{
					item2.AllowEditing = true;
				}
				else
				{
					item2.AllowEditing = false;
				}
			}
		}
		finally
		{
			View.EndUpdate();
		}
		List<object> GetLastLevelAccountChildOrAuxItemChild(Account parent)
		{
			TrialBalanceSheet sheet = GetTrialBalanceSheetWithCache(_collector.Setting);
			List<object> list = new List<object>();
			GetFinalAccountOrAuxItem(parent, list);
			return list;
			void GetFinalAccountOrAuxItem(Account acc, List<object> finals)
			{
				Dictionary<AuxiliaryClass, ClassBalance> classBalances = sheet.End[acc].ClassBalances;
				if ((acc == _collector.Setting.Account && _collector.Setting.Auxiliary is AuxiliaryClass) || (acc != _collector.Setting.Account && classBalances.Count > 0))
				{
					AuxiliaryClass key = ((acc != _collector.Setting.Account || !(_collector.Setting.Auxiliary is AuxiliaryClass auxiliaryClass)) ? TableCollectorAbstract.GetFirstOrDefaultAuxiliary(acc.Ledger, acc, sheet) : auxiliaryClass);
					Dictionary<AuxiliaryItem, decimal>.KeyCollection keys = classBalances[key].ItemBalances.Keys;
					{
						foreach (AuxiliaryItem item3 in keys)
						{
							finals.Add(Tuple.Create(acc, item3));
						}
						return;
					}
				}
				if (acc.Children.Count == 0)
				{
					finals.Add(acc);
					return;
				}
				foreach (Account child in acc.Children)
				{
					GetFinalAccountOrAuxItem(child, finals);
				}
			}
		}
	}

	private void PopulateSummary(Account account, AuxiliaryClass auxiliaryClass, AnalysisProject analysis)
	{
		View.Rows.Count = 1;
		View.Rows.Fixed = 1;
		View.Cols.Count = 0;
		View.Cols.Fixed = 1;
		View.Rows.DefaultSize = 30;
		C1.Win.C1FlexGrid.CellStyle cellStyle = View.Styles.Add("checkColStyle");
		cellStyle.ImageAlign = ImageAlignEnum.CenterCenter;
		View.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.Column column = View.Cols.Add();
			column.Name = "Check";
			column.DataType = typeof(string);
			column.Style = cellStyle;
			column = View.Cols.Add();
			column.Name = "Index";
			column.Caption = "序号";
			column.DataType = typeof(string);
			column.TextAlign = TextAlignEnum.CenterCenter;
			column = View.Cols.Add();
			column.Name = "ProjectName";
			column.Caption = "项目";
			column.DataType = typeof(string);
			for (int i = 1; i <= 12; i++)
			{
				column = View.Cols.Add();
				column.Name = $"mon{i}";
				column.Caption = $"{i}月";
				column.DataType = typeof(decimal);
				column.Format = "#,0.00;-#,0.00;#";
			}
			View.SetCellCheck(0, 0, CheckEnum.Unchecked);
			View.SetCellStyle(0, 0, cellStyle);
			if (account == null)
			{
				return;
			}
			foreach (object lastLevelChild in GetLastLevelChildren(account, auxiliaryClass))
			{
				bool flag = false;
				string empty = string.Empty;
				SubsidiaryLedger subsidiaryLedger = null;
				if (!(lastLevelChild is Account account2))
				{
					if (!(lastLevelChild is Tuple<Account, AuxiliaryItem> tuple))
					{
						continue;
					}
					flag = true;
					empty = tuple.Item2.Name;
					subsidiaryLedger = _collector.Setting.Ledger.GetSubsidiaryLedger(tuple.Item1, _collector.Setting.Start, _collector.Setting.End, tuple.Item2);
				}
				else
				{
					empty = account2.Name;
					subsidiaryLedger = _collector.Setting.Ledger.GetSubsidiaryLedger(account2, _collector.Setting.Start, _collector.Setting.End);
				}
				C1.Win.C1FlexGrid.Row row = View.Rows.Add();
				row.UserData = lastLevelChild;
				row["ProjectName"] = empty;
				int month;
				for (month = 1; month <= 12; month++)
				{
					MonthSubsidiaryLedger monthSubsidiaryLedger = subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == month);
					if (monthSubsidiaryLedger != null)
					{
						decimal totalValue = monthSubsidiaryLedger.Total.GetTotalValue(analysis);
						row[$"mon{month}"] = totalValue;
					}
				}
				View.SetCellCheck(row.Index, 0, CheckEnum.Unchecked);
				if (flag)
				{
					C1.Win.C1FlexGrid.CellStyle cellStyle2 = row.Style ?? row.StyleNew;
					cellStyle2.ForeColor = Color.Purple;
				}
			}
			PopulateIndex();
			AutoSizeCols();
			foreach (C1.Win.C1FlexGrid.Column item in (IEnumerable)View.Cols)
			{
				if (item.Name == "Check")
				{
					item.AllowEditing = true;
				}
				else
				{
					item.AllowEditing = false;
				}
			}
		}
		finally
		{
			View.EndUpdate();
		}
		List<object> GetLastLevelChildren(Account parent, AuxiliaryClass auxClass)
		{
			List<object> list = new List<object>();
			TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(_collector.Setting);
			if (trialBalanceSheetWithCache.End.ContainsKey(parent) && trialBalanceSheetWithCache.End[parent].ClassBalances.ContainsKey(auxClass))
			{
				Dictionary<AuxiliaryItem, decimal>.KeyCollection keys = trialBalanceSheetWithCache.End[parent].ClassBalances[auxClass].ItemBalances.Keys;
				foreach (AuxiliaryItem item2 in keys)
				{
					list.Add(Tuple.Create(parent, item2));
				}
			}
			return list;
		}
	}

	private void PopulateSummary(Account account, AuxiliaryItem auxiliaryItem, AnalysisProject analysis)
	{
		View.Rows.Count = 1;
		View.Rows.Fixed = 1;
		View.Cols.Count = 0;
		View.Cols.Fixed = 1;
		View.Rows.DefaultSize = 30;
		C1.Win.C1FlexGrid.CellStyle cellStyle = View.Styles.Add("checkColStyle");
		cellStyle.ImageAlign = ImageAlignEnum.CenterCenter;
		View.BeginUpdate();
		try
		{
			C1.Win.C1FlexGrid.Column column = View.Cols.Add();
			column.Name = "Check";
			column.Style = cellStyle;
			column.DataType = typeof(string);
			column = View.Cols.Add();
			column.Name = "ProjectName";
			column.Caption = "项目";
			column.DataType = typeof(string);
			for (int i = 1; i <= 12; i++)
			{
				column = View.Cols.Add();
				column.Name = $"mon{i}";
				column.Caption = $"{i}月";
				column.DataType = typeof(decimal);
				column.Format = "#,0.00;-#,0.00;#";
			}
			View.SetCellCheck(0, 0, CheckEnum.Unchecked);
			View.SetCellStyle(0, 0, cellStyle);
			if (account == null)
			{
				return;
			}
			foreach (object lastLevelChild in GetLastLevelChildren(account, auxiliaryItem))
			{
				bool flag = false;
				string empty = string.Empty;
				SubsidiaryLedger subsidiaryLedger = null;
				if (!(lastLevelChild is Account account2))
				{
					if (!(lastLevelChild is Tuple<Account, AuxiliaryItem> tuple))
					{
						continue;
					}
					empty = tuple.Item2.Name;
					subsidiaryLedger = _collector.Setting.Ledger.GetSubsidiaryLedger(tuple.Item1, _collector.Setting.Start, _collector.Setting.End, tuple.Item2);
				}
				else
				{
					empty = account2.Name;
					subsidiaryLedger = _collector.Setting.Ledger.GetSubsidiaryLedger(account2, _collector.Setting.Start, _collector.Setting.End);
				}
				C1.Win.C1FlexGrid.Row row = View.Rows.Add();
				row.UserData = lastLevelChild;
				row["ProjectName"] = empty;
				int month;
				for (month = 1; month <= 12; month++)
				{
					MonthSubsidiaryLedger monthSubsidiaryLedger = subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == month);
					if (monthSubsidiaryLedger != null)
					{
						decimal totalValue = monthSubsidiaryLedger.Total.GetTotalValue(analysis);
						row[$"mon{month}"] = totalValue;
					}
				}
				View.SetCellCheck(row.Index, 0, CheckEnum.Unchecked);
				if (flag)
				{
					C1.Win.C1FlexGrid.CellStyle cellStyle2 = row.Style ?? row.StyleNew;
					cellStyle2.ForeColor = Color.Purple;
				}
			}
			PopulateIndex();
			AutoSizeCols();
			foreach (C1.Win.C1FlexGrid.Column item in (IEnumerable)View.Cols)
			{
				if (item.Name == "Check")
				{
					item.AllowEditing = true;
				}
				else
				{
					item.AllowEditing = false;
				}
			}
		}
		finally
		{
			View.EndUpdate();
		}
		static List<object> GetLastLevelChildren(Account parent, AuxiliaryItem auxItem)
		{
			return new List<object> { Tuple.Create(parent, auxItem) };
		}
	}

	private void AutoSizeCols()
	{
		for (int i = 0; i < View.Cols.Count; i++)
		{
			View.AutoSizeCol(i);
		}
	}

	private object GetDCChar(bool isDebit, decimal balance)
	{
		if (balance == 0m)
		{
			return "平";
		}
		if (balance > 0m)
		{
			if (!isDebit)
			{
				return "贷";
			}
			return "借";
		}
		if (!isDebit)
		{
			return "借";
		}
		return "贷";
	}

	private void UpLevel(C1.Win.C1FlexGrid.Row currentRow)
	{
		CheckEnum cellCheck = View.GetCellCheck(currentRow.Index, 0);
		View.BeginUpdate();
		try
		{
			if (currentRow.UserData is Account { Parent: not null } account)
			{
				TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(_collector.Setting);
				if (_collector.CollectObject == CollectObjectEnum.Balance)
				{
					BalanceInsertAccount(currentRow.Index, cellCheck, account.Parent, trialBalanceSheetWithCache);
				}
				else if (_collector.CollectObject == CollectObjectEnum.Summary)
				{
					SummaryInsertAccount(currentRow.Index, cellCheck, account.Parent);
				}
				List<C1.Win.C1FlexGrid.Row> list = new List<C1.Win.C1FlexGrid.Row>();
				for (int i = View.Rows.Fixed; i < View.Rows.Count; i++)
				{
					C1.Win.C1FlexGrid.Row row = View.Rows[i];
					if ((row.UserData is Account account2 && isAncestor(account.Parent, account2)) || (row.UserData is Tuple<Account, AuxiliaryItem> tuple && (tuple.Item1 == account.Parent || isAncestor(account.Parent, tuple.Item1))))
					{
						list.Add(row);
					}
				}
				foreach (C1.Win.C1FlexGrid.Row item in list)
				{
					View.Rows.Remove(item);
				}
			}
			else if (currentRow.UserData is Tuple<Account, AuxiliaryItem> tuple2)
			{
				TrialBalanceSheet trialBalanceSheetWithCache2 = GetTrialBalanceSheetWithCache(_collector.Setting);
				if (_collector.CollectObject == CollectObjectEnum.Balance)
				{
					BalanceInsertAccount(currentRow.Index, cellCheck, tuple2.Item1, trialBalanceSheetWithCache2);
				}
				else if (_collector.CollectObject == CollectObjectEnum.Summary)
				{
					SummaryInsertAccount(currentRow.Index, cellCheck, tuple2.Item1);
				}
				List<C1.Win.C1FlexGrid.Row> list2 = new List<C1.Win.C1FlexGrid.Row>();
				for (int j = View.Rows.Fixed; j < View.Rows.Count; j++)
				{
					C1.Win.C1FlexGrid.Row row2 = View.Rows[j];
					if ((row2.UserData is Account account3 && isAncestor(tuple2.Item1, account3)) || (row2.UserData is Tuple<Account, AuxiliaryItem> tuple3 && (tuple3.Item1 == tuple2.Item1 || isAncestor(tuple2.Item1, tuple3.Item1))))
					{
						list2.Add(row2);
					}
				}
				foreach (C1.Win.C1FlexGrid.Row item2 in list2)
				{
					View.Rows.Remove(item2);
				}
				if (tuple2.Item1 == _collector.Setting.Account)
				{
					_collector.Setting.Auxiliary = null;
				}
			}
			PopulateIndex();
			AutoSizeCols();
		}
		catch (Exception ex5)
		{
			ex5.Log();
			System.Windows.Forms.MessageBox.Show(ex5.Message);
		}
		finally
		{
			View.EndUpdate();
		}
	}

	private bool isAncestor(Account ancestor, Account account)
	{
		foreach (Account ancestor2 in account.Ancestors)
		{
			if (ancestor == ancestor2)
			{
				return true;
			}
		}
		return false;
	}

	private void GetBatchDownInfo(IEnumerable<C1.Win.C1FlexGrid.Row> rows, out List<C1.Win.C1FlexGrid.Row> ret1, out List<C1.Win.C1FlexGrid.Row> ret2)
	{
		Dictionary<int, List<C1.Win.C1FlexGrid.Row>> dictionary = new Dictionary<int, List<C1.Win.C1FlexGrid.Row>>();
		List<C1.Win.C1FlexGrid.Row> list = new List<C1.Win.C1FlexGrid.Row>();
		foreach (C1.Win.C1FlexGrid.Row row in rows)
		{
			if (row.UserData is Account acc2)
			{
				int key = getLevel(acc2);
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, new List<C1.Win.C1FlexGrid.Row>());
				}
				dictionary[key].Add(row);
			}
			else if (row.UserData is Tuple<Account, AuxiliaryItem>)
			{
				list.Add(row);
			}
		}
		ret1 = ((dictionary.Count == 0) ? null : (from r in dictionary.OrderByDescending((KeyValuePair<int, List<C1.Win.C1FlexGrid.Row>> kv) => kv.Key).First().Value
			group r by (r.UserData as Account).Parent into g
			select g.First()).ToList());
		ret2 = ((list.Count == 0) ? null : (from r in list
			group r by (r.UserData as Tuple<Account, AuxiliaryItem>).Item1 into g
			select g.First()).ToList());
		static int getLevel(Account acc)
		{
			int num = 0;
			for (Account parent = acc.Parent; parent != null; parent = parent.Parent)
			{
				num++;
			}
			return num;
		}
	}

	private void DownLevel(C1.Win.C1FlexGrid.Row currentRow)
	{
		if (!(currentRow.UserData is Account account))
		{
			return;
		}
		CheckEnum cellCheck = View.GetCellCheck(currentRow.Index, 0);
		TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(_collector.Setting);
		View.BeginUpdate();
		try
		{
			if (account.Children.Count > 0)
			{
				int index = currentRow.Index;
				View.Rows.Remove(currentRow);
				foreach (Account child in account.Children)
				{
					if (_collector.CollectObject == CollectObjectEnum.Balance)
					{
						BalanceInsertAccount(index++, cellCheck, child, trialBalanceSheetWithCache);
					}
					else if (_collector.CollectObject == CollectObjectEnum.Summary)
					{
						SummaryInsertAccount(index++, cellCheck, child);
					}
				}
			}
			else if (trialBalanceSheetWithCache.End[account].ClassBalances.Count > 0)
			{
				int index2 = currentRow.Index;
				View.Rows.Remove(currentRow);
				AuxiliaryClass firstOrDefaultAuxiliary = TableCollectorAbstract.GetFirstOrDefaultAuxiliary(account.Ledger, account, trialBalanceSheetWithCache);
				ClassBalance classBalance = trialBalanceSheetWithCache.End[account].ClassBalances[firstOrDefaultAuxiliary];
				foreach (AuxiliaryItem key in classBalance.ItemBalances.Keys)
				{
					if (_collector.CollectObject == CollectObjectEnum.Balance)
					{
						BalanceInsertAuxItem(index2++, cellCheck, account, key);
					}
					else if (_collector.CollectObject == CollectObjectEnum.Summary)
					{
						SummaryInsertAuxItem(index2++, cellCheck, account, key);
					}
				}
				if (account == _collector.Setting.Account)
				{
					_collector.Setting.Auxiliary = firstOrDefaultAuxiliary;
				}
			}
			PopulateIndex();
			AutoSizeCols();
		}
		catch (Exception ex5)
		{
			ex5.Log();
			System.Windows.Forms.MessageBox.Show(ex5.Message);
		}
		finally
		{
			View.EndUpdate();
		}
	}

	private void DownAux(Account account, AuxiliaryClass auxiliaryClass)
	{
		if (View.Row < View.Rows.Fixed || View.Row >= View.Rows.Count)
		{
			return;
		}
		C1.Win.C1FlexGrid.Row row = View.Rows[View.Row];
		if (!(row.UserData is Account))
		{
			return;
		}
		CheckEnum cellCheck = View.GetCellCheck(row.Index, 0);
		TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(_collector.Setting);
		View.BeginUpdate();
		try
		{
			if (trialBalanceSheetWithCache.End[account].ClassBalances.Count > 0)
			{
				int index = row.Index;
				View.Rows.Remove(row);
				Dictionary<AuxiliaryItem, decimal> itemBalances = trialBalanceSheetWithCache.End[account].ClassBalances[auxiliaryClass].ItemBalances;
				foreach (AuxiliaryItem key in itemBalances.Keys)
				{
					if (_collector.CollectObject == CollectObjectEnum.Balance)
					{
						BalanceInsertAuxItem(index++, cellCheck, account, key);
					}
					else if (_collector.CollectObject == CollectObjectEnum.Summary)
					{
						SummaryInsertAuxItem(index++, cellCheck, account, key);
					}
				}
			}
			if (_collector.Setting.Account == account)
			{
				_collector.Setting.Auxiliary = auxiliaryClass;
			}
			PopulateIndex();
			AutoSizeCols();
		}
		catch (Exception ex5)
		{
			ex5.Log();
			System.Windows.Forms.MessageBox.Show(ex5.Message);
		}
		finally
		{
			View.EndUpdate();
		}
	}

	private void SwitchAux(Account account, AuxiliaryClass auxiliaryClass)
	{
		if (View.Row < View.Rows.Fixed || View.Row >= View.Rows.Count)
		{
			return;
		}
		C1.Win.C1FlexGrid.Row row = View.Rows[View.Row];
		if (!(row.UserData is Tuple<Account, AuxiliaryItem>))
		{
			return;
		}
		CheckEnum cellCheck = View.GetCellCheck(row.Index, 0);
		TrialBalanceSheet trialBalanceSheetWithCache = GetTrialBalanceSheetWithCache(_collector.Setting);
		View.BeginUpdate();
		try
		{
			int index = row.Index;
			ClassBalance classBalance = trialBalanceSheetWithCache.End[account].ClassBalances[auxiliaryClass];
			foreach (AuxiliaryItem key in classBalance.ItemBalances.Keys)
			{
				if (_collector.CollectObject == CollectObjectEnum.Balance)
				{
					BalanceInsertAuxItem(index++, cellCheck, account, key);
				}
				else if (_collector.CollectObject == CollectObjectEnum.Summary)
				{
					SummaryInsertAuxItem(index++, cellCheck, account, key);
				}
			}
			List<C1.Win.C1FlexGrid.Row> list = new List<C1.Win.C1FlexGrid.Row>();
			for (int i = View.Rows.Fixed; i < View.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row2 = View.Rows[i];
				if (row2.UserData is Tuple<Account, AuxiliaryItem> tuple2 && tuple2.Item1 == account && tuple2.Item2.Class != auxiliaryClass)
				{
					list.Add(row2);
				}
			}
			foreach (C1.Win.C1FlexGrid.Row item in list)
			{
				View.Rows.Remove(item);
			}
			PopulateIndex();
			AutoSizeCols();
			if (account == _collector.Setting.Account)
			{
				_collector.Setting.Auxiliary = auxiliaryClass;
			}
		}
		catch (Exception ex5)
		{
			ex5.Log();
			System.Windows.Forms.MessageBox.Show(ex5.Message);
		}
		finally
		{
			View.EndUpdate();
		}
	}

	private void BalanceInsertAccount(int index, CheckEnum checkEnum, Account account, TrialBalanceSheet sheet)
	{
		C1.Win.C1FlexGrid.Row row = View.Rows.Insert(index);
		row.UserData = account;
		row["Code"] = account.Code;
		row["Name"] = ((CurrentAccNameStyle == AccNameStyleEnum.SecondFullName) ? ex1.GetSecondFullName(account) : account.Name);
		row["BeginDC"] = GetDCChar(account.IsDebit, sheet.Start[account].Total);
		row["BeginBalance"] = Math.Abs(sheet.Start[account].Total);
		row["CurrentDebit"] = (sheet.Debit.ContainsKey(account) ? ((object)sheet.Debit[account].Total) : null);
		row["CurrentCredit"] = (sheet.Credit.ContainsKey(account) ? ((object)sheet.Credit[account].Total) : null);
		row["EndDC"] = GetDCChar(account.IsDebit, sheet.End[account].Total);
		row["EndBalance"] = Math.Abs(sheet.End[account].Total);
		View.SetCellCheck(row.Index, 0, checkEnum);
		if (checkEnum == CheckEnum.Checked)
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle = row.Style ?? row.StyleNew;
			cellStyle.BackColor = HightLightColor;
		}
		else
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle2 = row.Style ?? row.StyleNew;
			cellStyle2.BackColor = Color.White;
		}
	}

	private void BalanceInsertAuxItem(int index, CheckEnum checkEnum, Account account, AuxiliaryItem auxiliaryItem)
	{
		Setting setting = _collector.Setting;
		SubsidiaryLedger subsidiaryLedger = setting.Ledger.GetSubsidiaryLedger(account, setting.Start, setting.End, auxiliaryItem);
		SubsidiaryLedgerTotal subsidiaryLedgerTotal = subsidiaryLedger.Months.LastOrDefault()?.GrandTotal;
		C1.Win.C1FlexGrid.Row row = View.Rows.Insert(index);
		row.UserData = Tuple.Create(account, auxiliaryItem);
		row["Code"] = account.Code + "-" + auxiliaryItem.Code;
		row["Name"] = ((CurrentAccNameStyle == AccNameStyleEnum.SecondFullName) ? ex1.GetSecondFullName(account, auxiliaryItem) : auxiliaryItem.Name);
		row["BeginDC"] = GetDCChar(account.IsDebit, subsidiaryLedger.BeginBalance);
		row["BeginBalance"] = Math.Abs(subsidiaryLedger.BeginBalance);
		row["CurrentDebit"] = subsidiaryLedgerTotal?.Debit ?? 0m;
		row["CurrentCredit"] = subsidiaryLedgerTotal?.Credit ?? 0m;
		decimal num = subsidiaryLedgerTotal?.Balance ?? subsidiaryLedger.BeginBalance;
		row["EndDC"] = GetDCChar(account.IsDebit, num);
		row["EndBalance"] = Math.Abs(num);
		View.SetCellCheck(row.Index, 0, checkEnum);
		if (checkEnum == CheckEnum.Checked)
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle = row.Style ?? row.StyleNew;
			cellStyle.BackColor = HightLightColor;
			cellStyle.ForeColor = Color.Purple;
		}
		else
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle2 = row.Style ?? row.StyleNew;
			cellStyle2.BackColor = Color.White;
			cellStyle2.ForeColor = Color.Purple;
		}
	}

	private void SummaryInsertAccount(int index, CheckEnum checkEnum, Account account)
	{
		Setting setting = _collector.Setting;
		SubsidiaryLedger subsidiaryLedger = setting.Ledger.GetSubsidiaryLedger(account, setting.Start, setting.End);
		C1.Win.C1FlexGrid.Row row = View.Rows.Insert(index);
		row.UserData = account;
		row["ProjectName"] = account.Name;
		int month;
		for (month = 1; month <= 12; month++)
		{
			MonthSubsidiaryLedger monthSubsidiaryLedger = subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == month);
			if (monthSubsidiaryLedger != null)
			{
				decimal totalValue = monthSubsidiaryLedger.Total.GetTotalValue((_collector as TableCollectorSummary).AnalysisProject);
				row[$"mon{month}"] = totalValue;
			}
		}
		View.SetCellCheck(row.Index, 0, checkEnum);
		if (checkEnum == CheckEnum.Checked)
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle = row.Style ?? row.StyleNew;
			cellStyle.BackColor = HightLightColor;
		}
		else
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle2 = row.Style ?? row.StyleNew;
			cellStyle2.BackColor = Color.White;
		}
	}

	private void SummaryInsertAuxItem(int index, CheckEnum checkEnum, Account account, AuxiliaryItem auxiliaryItem)
	{
		string name = auxiliaryItem.Name;
		Setting setting = _collector.Setting;
		SubsidiaryLedger subsidiaryLedger = setting.Ledger.GetSubsidiaryLedger(account, setting.Start, setting.End, auxiliaryItem);
		C1.Win.C1FlexGrid.Row row = View.Rows.Insert(index);
		row.UserData = Tuple.Create(account, auxiliaryItem);
		row["ProjectName"] = name;
		int month;
		for (month = 1; month <= 12; month++)
		{
			MonthSubsidiaryLedger monthSubsidiaryLedger = subsidiaryLedger.Months.Find((MonthSubsidiaryLedger m) => m.Month == month);
			if (monthSubsidiaryLedger != null)
			{
				decimal totalValue = monthSubsidiaryLedger.Total.GetTotalValue((_collector as TableCollectorSummary).AnalysisProject);
				row[$"mon{month}"] = totalValue;
			}
		}
		View.SetCellCheck(row.Index, 0, checkEnum);
		if (checkEnum == CheckEnum.Checked)
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle = row.Style ?? row.StyleNew;
			cellStyle.BackColor = HightLightColor;
			cellStyle.ForeColor = Color.Purple;
		}
		else
		{
			C1.Win.C1FlexGrid.CellStyle cellStyle2 = row.Style ?? row.StyleNew;
			cellStyle2.BackColor = Color.White;
			cellStyle2.ForeColor = Color.Purple;
		}
	}

	private void CmdUpLevel_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = View.Selection.BottomRow > View.Selection.TopRow || CanUp(View.Row);
		if (e.Visible)
		{
			lnkUpLevel.Delimiter = true;
		}
		else
		{
			lnkUpLevel.Delimiter = false;
		}
	}

	private void CmdDownLevel_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		e.Visible = View.Selection.BottomRow > View.Selection.TopRow || CanDown(View.Row);
		if (!cmdUpLevel.Visible && e.Visible)
		{
			lnkDownLevel.Delimiter = true;
		}
		else
		{
			lnkDownLevel.Delimiter = false;
		}
	}

	private void _cmdCancelSort_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		string name = View.Cols[View.Col].Name;
		e.Visible = _collector.CollectObject == CollectObjectEnum.Subsidiary && (name.Equals("CurrentDebit") || name.Equals("CurrentCredit") || name.Equals("Balance"));
	}

	private void _cmdSortAsc_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		string name = View.Cols[View.Col].Name;
		e.Visible = _collector.CollectObject == CollectObjectEnum.Subsidiary && (name.Equals("CurrentDebit") || name.Equals("CurrentCredit") || name.Equals("Balance"));
	}

	private void _cmdSortDesc_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		string name = View.Cols[View.Col].Name;
		e.Visible = _collector.CollectObject == CollectObjectEnum.Subsidiary && (name.Equals("CurrentDebit") || name.Equals("CurrentCredit") || name.Equals("Balance"));
	}

	private void _cmdCancelSort_Click(object sender, ClickEventArgs e)
	{
		View.Sort(SortFlags.None, View.Col);
		Populate();
	}

	private void _cmdSortAsc_Click(object sender, ClickEventArgs e)
	{
		View.Sort(SortFlags.Ascending, View.Col);
	}

	private void _cmdSortDesc_Click(object sender, ClickEventArgs e)
	{
		View.Sort(SortFlags.Descending, View.Col);
	}

	private void _grid_CellChecked(object sender, RowColEventArgs e)
	{
		C1.Win.C1FlexGrid.Row row = View.Rows[e.Row];
		(row.Style ?? row.StyleNew).BackColor = ((View.GetCellCheck(e.Row, e.Col) == CheckEnum.Checked) ? HightLightColor : Color.White);
		if (penddingCheckEvent || e.Row >= View.Rows.Fixed)
		{
			return;
		}
		bool flag = penddingCheckEvent;
		if (!flag)
		{
			penddingCheckEvent = true;
		}
		try
		{
			CheckEnum cellCheck = View.GetCellCheck(e.Row, 0);
			for (int i = 0; i < View.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row2 = View.Rows[i];
				if (row2.IsVisible)
				{
					View.SetCellCheck(i, 0, cellCheck);
					(row2.Style ?? row2.StyleNew).BackColor = ((cellCheck == CheckEnum.Checked) ? HightLightColor : Color.White);
				}
			}
		}
		finally
		{
			if (!flag)
			{
				penddingCheckEvent = false;
			}
		}
	}

	private void View_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			switch (View.HitTest().Type)
			{
			case HitTestTypeEnum.Cell:
				contextMenu.ShowContextMenu(View, e.Location);
				break;
			case HitTestTypeEnum.ColumnHeader:
				_ctxColHeader.ShowContextMenu(View, e.Location);
				break;
			}
		}
	}

	private void View_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left || View.HitTest().Type != HitTestTypeEnum.ColumnHeader)
		{
			return;
		}
		C1.Win.C1FlexGrid.Column column = View.Cols[View.Col];
		string name = column.Name;
		if (_collector.CollectObject == CollectObjectEnum.Subsidiary && (name.Equals("CurrentDebit") || name.Equals("CurrentCredit") || name.Equals("Balance")))
		{
			if (column.Sort == SortFlags.None)
			{
				column.Sort = SortFlags.Descending;
			}
			else if (column.Sort == SortFlags.Descending)
			{
				column.Sort = SortFlags.Ascending;
			}
			else if (column.Sort == SortFlags.Ascending)
			{
				column.Sort = SortFlags.None;
				Populate();
			}
			View.Sort(SortFlags.UseColSort, column.Index);
		}
	}

	private void PopulateIndex()
	{
		if (View.Cols.Contains("Index"))
		{
			for (int i = View.Rows.Fixed; i < View.Rows.Count; i++)
			{
				C1.Win.C1FlexGrid.Row row = View.Rows[i];
				row["Index"] = i.ToString();
			}
		}
	}
}
