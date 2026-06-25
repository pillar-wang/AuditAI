﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Auditai.LocalDataStore;
using Auditai.Model;
using Auditai.UI.Controls.CollectDic;
using Auditai.UI.Controls.SmartCollector;
using Auditai.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Auditai.UI.Controls;

public static class DictionarySync
{
	private const string TABLE_1_COLLECTOBJECT = "collectobject";

	private const string TABLE_1_TABLENAMES = "tablenames";

	private const string TABLE_2_ACCOUNTNAME = "accountName";

	private const string TABLE_2_LENDINGLOGIC = "lendingLogic";

	private const string TABLE_3_BALANCECOLS = "balancecols";

	private const string TABLE_3_LENDINGLOGIC = "lendinglogic";

	private const string TABLE_3_TABLECOLS = "tablecols";

	private const string TABLE_4_SUBSIDIARYCOLS = "subsidiarycols";

	private const string TABLE_4_TABLECOLS = "tablecols";

	private const string TABLE_5_MONTHCOLS = "monthcols";

	private const string TABLE_5_TABLECOLS = "tablecols";

	private const string TABLE_6_TABLENAMES = "tablenames";

	private const string TABLE_6_ACCOUNTNAME = "accountName";

	private const string TABLE_6_LENDINGLOGIC = "lendinglogic";

	private const string CELL_1_TABLENAMES = "tablenames";

	private const string CELL_1_CONDITIONCOLS = "conditioncols";

	private const string CELL_1_FILLCOLS = "fillcols";

	private const string CELL_1_COLLECTOBJECT = "collectobject";

	private const string CELL_2_CONDITIONCOLNAME = "conditioncolname";

	private const string CELL_2_BALANCEACCOUNTNAME = "balanceaccount";

	private const string CELL_2_LENDINGLOGIC = "lendinglogic";

	private const string CELL_3_FILLCOLS = "fillcols";

	private const string CELL_3_BALANCECOL = "balancecol";

	private const string FILE_TABLECOLLECTDIC = "./config/TableCollectDic.json";

	private const string FILE_CELLCOLLECTDIC = "./config/CellCollectDic.json";

	private const string FILE_LEDGERVALIDATEDIC = "./config/LedgerValidateDic.json";

	private const string VALIDATE_ACCOUNTPOSITIVE_ACCOUNTNAME = "account";

	private const string VALIDATE_ACCOUNTPOSITIVE_RISKWARN = "warn";

	private const string VALIDATE_ACCOUNTREVERSE_ACCOUNTNAME = "account";

	private const string VALIDATE_ACCOUNTREVERSE_RISKWARN = "warn";

	private const string VALIDATE_BALANCE_ACCOUNTNAME = "account";

	private const string VALIDATE_BALANCE_CONDITION = "condition";

	private const string VALIDATE_BALANCE_RISKWARN = "warn";

	private const string VALIDATE_VOUCHERPOSITIVE_ACCOUNTNAME = "account";

	private const string VALIDATE_VOUCHERPOSITIVE_DIRECTION = "direction";

	private const string VALIDATE_VOUCHERPOSITIVE_OPPOSITE = "opposite";

	private const string VALIDATE_VOUCHERPOSITIVE_RISKWARN = "warn";

	private const string VALIDATE_VOUCHERREVERSE_ACCOUNTNAME = "account";

	private const string VALIDATE_VOUCHERREVERSE_DIRECTION = "direction";

	private const string VALIDATE_VOUCHERREVERSE_OPPOSITE = "opposite";

	private const string VALIDATE_VOUCHERREVERSE_RISKWARN = "warn";

	public static TableCollector TableCollector { get; set; }

	public static CellCollector CellCollector { get; set; }

	public static LedgerValidator LedgerValidator { get; set; }

	public static Tuple<DateTime, DateTime> GetAuditYear(Table table)
	{
		Tuple<DateTime, DateTime> tuple = matchYear(table.Title.TitleCell.GetDisplayValue());
		if (tuple != null)
		{
			return tuple;
		}
		Tuple<DateTime, DateTime> tuple2 = null;
		foreach (TableTitleRow row in table.Title.Rows)
		{
			foreach (TableTitleCell cell in row.Cells)
			{
				Tuple<DateTime, DateTime> tuple3;
				try
				{
					tuple3 = matchYear(cell.GetDisplayValue());
				}
				catch
				{
					tuple3 = null;
				}
				if (tuple3 != null && (tuple2 == null || tuple3.Item1 < tuple2.Item1))
				{
					tuple2 = tuple3;
				}
			}
		}
		return tuple2;
		static bool _isValidYear(int year)
		{
			if (year >= 2000)
			{
				return year <= DateTime.Now.Year;
			}
			return false;
		}
		static Tuple<DateTime, DateTime> matchYear(string input)
		{
			Match match = Regex.Match(input, "索\\s*引\\s*号|编\\s*号");
			if (match.Success)
			{
				return null;
			}
			Match match2 = Regex.Match(input, "([0-9]{4})[./年]([0-9]{1,2})[./月]([0-9]{1,2})[./日]?(-|－|到|至|～|--|~~)([0-9]{4})[./年]([0-9]{1,2})[./月]([0-9]{1,2})[./日]?");
			if (match2.Success)
			{
				int num = int.Parse(match2.Groups[1].Value);
				int month = int.Parse(match2.Groups[2].Value);
				int num2 = int.Parse(match2.Groups[3].Value);
				int num3 = int.Parse(match2.Groups[5].Value);
				int month2 = int.Parse(match2.Groups[6].Value);
				int num4 = int.Parse(match2.Groups[7].Value);
				if (num != num3)
				{
					return null;
				}
				if (!_isValidYear(num) || !_isValidYear(num3))
				{
					return null;
				}
				return Tuple.Create(new DateTime(num, month, 1), new DateTime(num3, month2, DateTime.DaysInMonth(num3, month2)).AddDays(1.0).AddMilliseconds(-1.0));
			}
			Match match3 = Regex.Match(input, "([0-9]{4})[./年]([0-2]{1,2})[./月]?(-|－|到|至|～|--|~~)([0-9]{4})[./年]([0-9]{1,2})[./月]?");
			if (match3.Success)
			{
				int num5 = int.Parse(match3.Groups[1].Value);
				int month3 = int.Parse(match3.Groups[2].Value);
				int num6 = int.Parse(match3.Groups[4].Value);
				int month4 = int.Parse(match3.Groups[5].Value);
				if (num5 != num6)
				{
					return null;
				}
				if (!_isValidYear(num5) || !_isValidYear(num6))
				{
					return null;
				}
				return Tuple.Create(new DateTime(num5, month3, 1), new DateTime(num6, month4, DateTime.DaysInMonth(num6, month4)).AddDays(1.0).AddMilliseconds(-1.0));
			}
			Match match4 = Regex.Match(input, "([0-9]{4})[./年]([0-9]{1,2})[./月]?(-|－|到|至|～|--|~~)([0-9]{1,2})[./月]?");
			if (match4.Success)
			{
				int year2 = int.Parse(match4.Groups[1].Value);
				if (!_isValidYear(year2))
				{
					return null;
				}
				int month5 = int.Parse(match4.Groups[2].Value);
				int month6 = int.Parse(match4.Groups[4].Value);
				return Tuple.Create(new DateTime(year2, month5, 1), new DateTime(year2, month6, DateTime.DaysInMonth(year2, month6)).AddDays(1.0).AddMilliseconds(-1.0));
			}
			Match match5 = Regex.Match(input, "([0-9]{4})[./年]([0-9]{1,2})(-|－|到|至|～|--|~~)([0-9]{1,2})[./月]?");
			if (match5.Success)
			{
				int year3 = int.Parse(match5.Groups[1].Value);
				if (!_isValidYear(year3))
				{
					return null;
				}
				int month7 = int.Parse(match5.Groups[2].Value);
				int month8 = int.Parse(match5.Groups[4].Value);
				return Tuple.Create(new DateTime(year3, month7, 1), new DateTime(year3, month8, DateTime.DaysInMonth(year3, month8)).AddDays(1.0).AddMilliseconds(-1.0));
			}
			Match match6 = Regex.Match(input, "([0-9]{4})[-－]([0-9]{1,2})[-－]([0-9]{1,2})[-－]?(到|至|～|~~)([0-9]{4})[-－]([0-9]{1,2})[-－]([0-9]{1,2})[-－]");
			if (match6.Success)
			{
				int num7 = int.Parse(match6.Groups[1].Value);
				int month9 = int.Parse(match6.Groups[2].Value);
				int num8 = int.Parse(match6.Groups[3].Value);
				int num9 = int.Parse(match6.Groups[5].Value);
				int month10 = int.Parse(match6.Groups[6].Value);
				int num10 = int.Parse(match6.Groups[7].Value);
				if (num7 != num9)
				{
					return null;
				}
				if (!_isValidYear(num7) || !_isValidYear(num9))
				{
					return null;
				}
				return Tuple.Create(new DateTime(num7, month9, 1), new DateTime(num9, month10, DateTime.DaysInMonth(num9, month10)).AddDays(1.0).AddMilliseconds(-1.0));
			}
			Match match7 = Regex.Match(input, "([0-9]{4})[-－]([0-2]{1,2})[-－]?(到|至|～|~~)([0-9]{4})[-－]([0-9]{1,2})[-－]?");
			if (match7.Success)
			{
				int num11 = int.Parse(match7.Groups[1].Value);
				int month11 = int.Parse(match7.Groups[2].Value);
				int num12 = int.Parse(match7.Groups[4].Value);
				int month12 = int.Parse(match7.Groups[5].Value);
				if (num11 != num12)
				{
					return null;
				}
				if (!_isValidYear(num11) || !_isValidYear(num12))
				{
					return null;
				}
				return Tuple.Create(new DateTime(num11, month11, 1), new DateTime(num12, month12, DateTime.DaysInMonth(num12, month12)).AddDays(1.0).AddMilliseconds(-1.0));
			}
			Match match8 = Regex.Match(input, "([0-9]{4})[-－]([0-9]{1,2})[-－]?(到|至|～|~~)([0-9]{1,2})[-－]?");
			if (match8.Success)
			{
				int year4 = int.Parse(match8.Groups[1].Value);
				if (!_isValidYear(year4))
				{
					return null;
				}
				int month13 = int.Parse(match8.Groups[2].Value);
				int month14 = int.Parse(match8.Groups[4].Value);
				return Tuple.Create(new DateTime(year4, month13, 1), new DateTime(year4, month14, DateTime.DaysInMonth(year4, month14)).AddDays(1.0).AddMilliseconds(-1.0));
			}
			Match match9 = Regex.Match(input, "([0-9]{4})[-－]([0-9]{1,2})(到|至|～|~~)([0-9]{1,2})[-－]?");
			if (match9.Success)
			{
				int year5 = int.Parse(match9.Groups[1].Value);
				if (!_isValidYear(year5))
				{
					return null;
				}
				int month15 = int.Parse(match9.Groups[2].Value);
				int month16 = int.Parse(match9.Groups[4].Value);
				return Tuple.Create(new DateTime(year5, month15, 1), new DateTime(year5, month16, DateTime.DaysInMonth(year5, month16)).AddDays(1.0).AddMilliseconds(-1.0));
			}
			Match match10 = Regex.Match(input, "([0-9]{4})[-－./年]([0-9]{1,2})[-－./月]([0-9]{1,2})[-－./日]?");
			if (match10.Success)
			{
				int year6 = int.Parse(match10.Groups[1].Value);
				if (!_isValidYear(year6))
				{
					return null;
				}
				int month17 = int.Parse(match10.Groups[2].Value);
				int num13 = int.Parse(match10.Groups[3].Value);
				return Tuple.Create(new DateTime(year6, 1, 1), new DateTime(year6, month17, DateTime.DaysInMonth(year6, month17)).AddDays(1.0).AddMilliseconds(-1.0));
			}
			Match match11 = Regex.Match(input, "([0-9]{4})[-－./年]([0-9]{1,2})[-－./月]?");
			if (match11.Success)
			{
				int year7 = int.Parse(match11.Groups[1].Value);
				if (!_isValidYear(year7))
				{
					return null;
				}
				int month18 = int.Parse(match11.Groups[2].Value);
				return Tuple.Create(new DateTime(year7, 1, 1), new DateTime(year7, month18, DateTime.DaysInMonth(year7, month18)).AddDays(1.0).AddMilliseconds(-1.0));
			}
			Match match12 = Regex.Match(input, "([0-9]{4})年(度)?");
			if (match12.Success)
			{
				int year8 = int.Parse(match12.Groups[1].Value);
				if (!_isValidYear(year8))
				{
					return null;
				}
				return Tuple.Create(new DateTime(year8, 1, 1), new DateTime(year8, 12, DateTime.DaysInMonth(year8, 12)).AddDays(1.0).AddMilliseconds(-1.0));
			}
			return null;
		}
	}

	#region debug-point A:static-ctor
	static DictionarySync()
	{
		if (!Directory.Exists("./config"))
		{
			Directory.CreateDirectory("./config");
		}
		// ★ 关键修复：静态构造函数中永远不触发远程调用
		// 无论本地模式还是服务器模式，都先设置默认版本号，
		// 字典数据通过本地 JSON 文件加载，异步更新延迟到 UI 交互时触发
		TableCollector = LoadFromLocalFile<TableCollector>("./config/TableCollectDic.json") ?? new TableCollector();
		CellCollector = LoadFromLocalFile<CellCollector>("./config/CellCollectDic.json") ?? new CellCollector();
		LedgerValidator = LoadFromLocalFile<LedgerValidator>("./config/LedgerValidateDic.json") ?? new LedgerValidator();
		
		// 确保版本号不为0（为0会触发远程更新）
		if (TableCollector.Version == 0) TableCollector.Version = 1;
		if (CellCollector.Version == 0) CellCollector.Version = 1;
		if (LedgerValidator.Version == 0) LedgerValidator.Version = 1;
		
		}
#endregion
	
	// 从本地文件加载字典（仅同步读取，不触发网络请求）
	private static T LoadFromLocalFile<T>(string filePath) where T : class
	{
		try
		{
			if (File.Exists(filePath))
			{
				return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
			}
		}
		catch
		{
			// 文件损坏或格式错误，忽略
		}
		return null;
	}

	#region debug-point B:check-update
	public static async Task CheckTableCollectVersionAndUpdate()
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return;
		}
		TableCollector tableCollector = await PullTableCollectDic();
		if (tableCollector != null)
		{
			TableCollector = tableCollector;
			File.WriteAllText("./config/TableCollectDic.json", JsonConvert.SerializeObject(TableCollector));
		}
	}
#endregion

	public static async Task CheckCellCollectDicVersionAndUpdate()
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return;
		}
		CellCollector cellCollector = await PullCellCollectDic();
		if (cellCollector != null)
		{
			CellCollector = cellCollector;
			File.WriteAllText("./config/CellCollectDic.json", JsonConvert.SerializeObject(CellCollector));
		}
	}

	public static async Task CheckLedgerValidateDicVersionAndUpdate()
	{
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return;
		}
		LedgerValidator ledgerValidator = await PullLedgerValidateDic();
		if (ledgerValidator != null)
		{
			LedgerValidator = ledgerValidator;
			File.WriteAllText("./config/LedgerValidateDic.json", JsonConvert.SerializeObject(LedgerValidator));
		}
	}

	private static async void InitializeTableCollector()
	{
		// 本地模式下不执行异步初始化
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return;
		}
		try
		{
			if (File.Exists("./config/TableCollectDic.json"))
			{
				TableCollector = JsonConvert.DeserializeObject<TableCollector>(File.ReadAllText("./config/TableCollectDic.json"));
			}
			else
			{
				TableCollector = new TableCollector();
			}
		}
		catch (Exception)
		{
			TableCollector = new TableCollector();
		}
		if (TableCollector.Version == 0) TableCollector.Version = 1;
		try
		{
			await CheckTableCollectVersionAndUpdate();
		}
		catch (Exception)
		{
		}
	}

	private static async void InitializeCellCollector()
	{
		// 本地模式下不执行异步初始化
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return;
		}
		try
		{
			if (File.Exists("./config/CellCollectDic.json"))
			{
				CellCollector = JsonConvert.DeserializeObject<CellCollector>(File.ReadAllText("./config/CellCollectDic.json"));
			}
			else
			{
				CellCollector = new CellCollector();
			}
		}
		catch (Exception)
		{
			CellCollector = new CellCollector();
		}
		if (CellCollector.Version == 0) CellCollector.Version = 1;
		try
		{
			await CheckCellCollectDicVersionAndUpdate();
		}
		catch (Exception)
		{
		}
	}

	private static async void InitializeLedgerValidator()
	{
		// 本地模式下不执行异步初始化
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return;
		}
		try
		{
			if (File.Exists("./config/LedgerValidateDic.json"))
			{
				LedgerValidator = JsonConvert.DeserializeObject<LedgerValidator>(File.ReadAllText("./config/LedgerValidateDic.json"));
			}
			else
			{
				LedgerValidator = new LedgerValidator();
			}
		}
		catch (Exception)
		{
			LedgerValidator = new LedgerValidator();
		}
		if (LedgerValidator.Version == 0) LedgerValidator.Version = 1;
		try
		{
			await CheckLedgerValidateDicVersionAndUpdate();
		}
		catch (Exception)
		{
		}
	}

	private static async Task<TableCollector> PullTableCollectDic()
	{
		// 本地模式下不从远程拉取字典
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return null;
		}
		TableCollector tableCollector = TableCollector;
		JObject ret = null;
		try
		{
			
			ret = await WebApiClient.TableCollectDic(tableCollector.Version);
		}
		catch (TimeoutException)
		{
			if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode && tableCollector.Version == 0)
			{
				MessageBox.Show(MessageBoxIcon.None, "更新字典失败！网络超时，请重试");
			}
		}
		catch (Exception ex2)
		{
			
			if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode && tableCollector.Version == 0)
			{
				
				MessageBox.Show(MessageBoxIcon.None, "更新字典失败！" + ex2.Message + ",请重试");
			}
		}
		if (ret == null)
		{
			return null;
		}
		JToken jToken = ret["update"];
		if (jToken == null || jToken.ToString() == "0")
		{
			return null;
		}
		ETable eTable = new ETable();
		ETable eTable2 = new ETable();
		ETable eTable3 = new ETable();
		foreach (JToken item in (IEnumerable<JToken>)ret["ct_tablenametoobject"])
		{
			ERow eRow = new ERow();
			ECell eCell = new ECell();
			eCell.IsSingle = true;
			eCell.Value = item.Value<string>("collectobject");
			ECell eCell2 = new ECell();
			eCell2.IsSingle = false;
			eCell2.Value = item.Value<string>("tablenames");
			eCell2.Values = item.Value<string>("tablenames").ToPatternList().ToList();
			eRow["collectobject"] = eCell;
			eRow["tablenames"] = eCell2;
			if (eCell.Value.EndsWith("_明细科目识别"))
			{
				eTable3.Add(eRow);
				continue;
			}
			eTable.Add(eRow);
			if (eCell.Value.EndsWith("_全部科目"))
			{
				eTable2.Add(eRow);
			}
		}
		tableCollector.TableTitleToCollectObjectMap = eTable;
		tableCollector.TableTitleToSelectAllAccountMap = eTable2;
		tableCollector.TableTitleToSelectDetailAccountMap = eTable3;
		ETable eTable4 = new ETable();
		ETable eTable5 = new ETable();
		foreach (JToken item2 in (IEnumerable<JToken>)ret["ct_tablenametoaccount"])
		{
			string text = item2.Value<string>("accountname");
			if (text.IndexOf('|') != -1)
			{
				ERow eRow2 = new ERow();
				string[] array = text.Split('|');
				string text2 = array[0];
				string text3 = ((array.Length > 1) ? array[1] : "");
				ECell eCell3 = new ECell();
				eCell3.IsSingle = false;
				eCell3.Value = text2;
				eCell3.Values = text2.ToPatternList().ToList();
				ECell eCell4 = new ECell();
				eCell4.IsSingle = false;
				eCell4.Value = text3;
				eCell4.Values = text3.ToPatternList().ToList();
				ECell eCell5 = new ECell();
				eCell5.IsSingle = true;
				eCell5.Value = item2.Value<string>("lendinglogic");
				eRow2["tablenames"] = eCell3;
				eRow2["accountName"] = eCell4;
				eRow2["lendinglogic"] = eCell5;
				eTable5.Add(eRow2);
			}
			else
			{
				ERow eRow3 = new ERow();
				ECell eCell6 = new ECell();
				eCell6.IsSingle = false;
				eCell6.Value = item2.Value<string>("accountname");
				eCell6.Values = item2.Value<string>("accountname").ToPatternList().ToList();
				ECell eCell7 = new ECell();
				eCell7.IsSingle = true;
				eCell7.Value = item2.Value<string>("lendinglogic");
				eRow3["accountName"] = eCell6;
				eRow3["lendingLogic"] = eCell7;
				eTable4.Add(eRow3);
			}
		}
		tableCollector.AccountNameToBorrowLogicMap = eTable4;
		tableCollector.TableTitleToSelectSomeAccountMap = eTable5;
		ETable eTable6 = new ETable();
		foreach (JToken item3 in (IEnumerable<JToken>)ret["ct_tablecoltobalance"])
		{
			ERow eRow4 = new ERow();
			ECell eCell8 = new ECell();
			eCell8.IsSingle = true;
			eCell8.Value = item3.Value<string>("balancecol");
			ECell eCell9 = new ECell();
			eCell9.IsSingle = true;
			eCell9.Value = item3.Value<string>("lendinglogic");
			ECell eCell10 = new ECell();
			eCell10.IsSingle = false;
			eCell10.Value = item3.Value<string>("tablecolnames");
			eCell10.Values = item3.Value<string>("tablecolnames").ToPatternList().ToList();
			eRow4["balancecols"] = eCell8;
			eRow4["lendinglogic"] = eCell9;
			eRow4["tablecols"] = eCell10;
			eTable6.Add(eRow4);
		}
		tableCollector.TableColToBalanceColMap = eTable6;
		ETable eTable7 = new ETable();
		foreach (JToken item4 in (IEnumerable<JToken>)ret["ct_tablecoltosubsidiary"])
		{
			ERow eRow5 = new ERow();
			ECell eCell11 = new ECell();
			eCell11.IsSingle = true;
			eCell11.Value = item4.Value<string>("subsidiarycol");
			ECell eCell12 = new ECell();
			eCell12.IsSingle = false;
			eCell12.Value = item4.Value<string>("tablecolnames");
			eCell12.Values = item4.Value<string>("tablecolnames").ToPatternList().ToList();
			eRow5["subsidiarycols"] = eCell11;
			eRow5["tablecols"] = eCell12;
			eTable7.Add(eRow5);
		}
		tableCollector.TableColToSubsidiaryColMap = eTable7;
		ETable eTable8 = new ETable();
		foreach (JToken item5 in (IEnumerable<JToken>)ret["ct_tablecoltosummary"])
		{
			ERow eRow6 = new ERow();
			ECell eCell13 = new ECell();
			eCell13.IsSingle = true;
			eCell13.Value = item5.Value<string>("summarycol");
			ECell eCell14 = new ECell();
			eCell14.IsSingle = false;
			eCell14.Value = item5.Value<string>("tablecolnames");
			eCell14.Values = item5.Value<string>("tablecolnames").ToPatternList().ToList();
			eRow6["monthcols"] = eCell13;
			eRow6["tablecols"] = eCell14;
			eTable8.Add(eRow6);
		}
		tableCollector.TableColToSummaryColMap = eTable8;
		tableCollector.Version = (int)ret["tabledicversion"];
		return tableCollector;
	}

	private static async Task<CellCollector> PullCellCollectDic()
	{
		// 本地模式下不从远程拉取字典
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return null;
		}
		CellCollector cellCollector = CellCollector;
		JObject ret = null;
		try
		{
			ret = await WebApiClient.CellCollectDic(cellCollector.Version);
		}
		catch (TimeoutException)
		{
			if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode && cellCollector.Version == 0)
			{
				MessageBox.Show(MessageBoxIcon.None, "更新字典失败！网络超时,请重试");
			}
		}
		catch (Exception ex2)
		{
			if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode && cellCollector.Version == 0)
			{
				MessageBox.Show(MessageBoxIcon.None, "更新字典失败！" + ex2.Message + ",请重试");
			}
		}
		if (ret == null)
		{
			return null;
		}
		JToken jToken = ret["update"];
		if (jToken == null || jToken.ToString() == "0")
		{
			return null;
		}
		ETable eTable = new ETable();
		foreach (JToken item in (IEnumerable<JToken>)ret["cc_tablenametoobject"])
		{
			ECell eCell = new ECell();
			eCell.IsSingle = false;
			eCell.Value = item.Value<string>("tablenames");
			eCell.Values = item.Value<string>("tablenames").ToPatternList().ToList();
			ECell eCell2 = new ECell();
			eCell2.IsSingle = false;
			eCell2.Value = item.Value<string>("conditioncols");
			eCell2.Values = item.Value<string>("conditioncols").ToPatternList().ToList();
			ECell eCell3 = new ECell();
			eCell3.IsSingle = false;
			eCell3.Value = item.Value<string>("fillingcols");
			eCell3.Values = item.Value<string>("fillingcols").ToPatternList().ToList();
			ECell eCell4 = new ECell();
			eCell4.IsSingle = true;
			eCell4.Value = item.Value<string>("collectobject");
			ERow eRow = new ERow();
			eRow["tablenames"] = eCell;
			eRow["conditioncols"] = eCell2;
			eRow["fillcols"] = eCell3;
			eRow["collectobject"] = eCell4;
			eTable.Add(eRow);
		}
		cellCollector.CollectObject = eTable;
		ETable eTable2 = new ETable();
		foreach (JToken item2 in (IEnumerable<JToken>)ret["cc_conditioncoltoaccount"])
		{
			ECell eCell5 = new ECell();
			eCell5.IsSingle = true;
			eCell5.Value = item2.Value<string>("conditioncol").ToPattern();
			ECell eCell6 = new ECell();
			eCell6.IsSingle = false;
			eCell6.Value = item2.Value<string>("accountnames");
			eCell6.Values = item2.Value<string>("accountnames").ToPatternList().ToList();
			ECell eCell7 = new ECell();
			eCell7.IsSingle = true;
			eCell7.Value = item2.Value<string>("lendinglogic");
			ERow eRow2 = new ERow();
			eRow2["conditioncolname"] = eCell5;
			eRow2["balanceaccount"] = eCell6;
			eRow2["lendinglogic"] = eCell7;
			eTable2.Add(eRow2);
		}
		cellCollector.ConditionColToBalance = eTable2;
		ETable eTable3 = new ETable();
		foreach (JToken item3 in (IEnumerable<JToken>)ret["cc_fillingcoltobalance"])
		{
			ECell eCell8 = new ECell();
			eCell8.IsSingle = false;
			eCell8.Value = item3.Value<string>("fillingcols");
			eCell8.Values = item3.Value<string>("fillingcols").ToPatternList().ToList();
			ECell eCell9 = new ECell();
			eCell9.IsSingle = true;
			eCell9.Value = item3.Value<string>("balancecol");
			ERow eRow3 = new ERow();
			eRow3["fillcols"] = eCell8;
			eRow3["balancecol"] = eCell9;
			eTable3.Add(eRow3);
		}
		cellCollector.FillingColToBalance = eTable3;
		cellCollector.Version = (int)ret["celldicversion"];
		return cellCollector;
	}

	private static async Task<LedgerValidator> PullLedgerValidateDic()
	{
		// 本地模式下不从远程拉取字典
		if (Auditai.LocalDataStore.StorageRouter.IsLocalMode)
		{
			return null;
		}
		LedgerValidator ledgerValidator = LedgerValidator;
		JObject ret = null;
		try
		{
			ret = await WebApiClient.LedgerValidateDic(ledgerValidator.Version);
		}
		catch (TimeoutException)
		{
			if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode && ledgerValidator.Version == 0)
			{
				MessageBox.Show(MessageBoxIcon.None, "更新字典失败！网络超时,请重试");
			}
		}
		catch (Exception ex2)
		{
			if (!Auditai.LocalDataStore.StorageRouter.IsLocalMode && ledgerValidator.Version == 0)
			{
				MessageBox.Show(MessageBoxIcon.None, "更新字典失败！" + ex2.Message + ",请重试");
			}
		}
		if (ret == null)
		{
			return null;
		}
		JToken jToken = ret["update"];
		if (jToken == null || jToken.ToString() == "0")
		{
			return null;
		}
		ETable eTable = new ETable();
		foreach (JToken item in (IEnumerable<JToken>)ret["vd_accountvalidatepositive"])
		{
			ECell eCell = new ECell();
			eCell.IsSingle = false;
			eCell.Value = item.Value<string>("accountname");
			eCell.Values = item.Value<string>("accountname").ToPatternList().ToList();
			ECell eCell2 = new ECell();
			eCell2.IsSingle = true;
			eCell2.Value = item.Value<string>("riskwarn");
			ERow eRow = new ERow();
			eRow["account"] = eCell;
			eRow["warn"] = eCell2;
			eTable.Add(eRow);
		}
		ledgerValidator.AccountPositive = eTable;
		ETable eTable2 = new ETable();
		foreach (JToken item2 in (IEnumerable<JToken>)ret["vd_accountvalidatereverse"])
		{
			ECell eCell3 = new ECell();
			eCell3.IsSingle = false;
			eCell3.Value = item2.Value<string>("accountname");
			eCell3.Values = item2.Value<string>("accountname").ToPatternList().ToList();
			ECell eCell4 = new ECell();
			eCell4.IsSingle = true;
			eCell4.Value = item2.Value<string>("riskwarn");
			ERow eRow2 = new ERow();
			eRow2["account"] = eCell3;
			eRow2["warn"] = eCell4;
			eTable2.Add(eRow2);
		}
		ledgerValidator.AccountReverse = eTable2;
		ETable eTable3 = new ETable();
		foreach (JToken item3 in (IEnumerable<JToken>)ret["vd_balancevalidate"])
		{
			ECell eCell5 = new ECell();
			eCell5.IsSingle = false;
			eCell5.Value = item3.Value<string>("accountname");
			eCell5.Values = item3.Value<string>("accountname").ToPatternList().ToList();
			ECell eCell6 = new ECell();
			eCell6.IsSingle = true;
			eCell6.Value = item3.Value<string>("balancecondition");
			ECell eCell7 = new ECell();
			eCell7.IsSingle = true;
			eCell7.Value = item3.Value<string>("riskwarn");
			ERow eRow3 = new ERow();
			eRow3["account"] = eCell5;
			eRow3["condition"] = eCell6;
			eRow3["warn"] = eCell7;
			eTable3.Add(eRow3);
		}
		ledgerValidator.BalanceValidate = eTable3;
		ETable eTable4 = new ETable();
		foreach (JToken item4 in (IEnumerable<JToken>)ret["vd_vouchervalidatepositive"])
		{
			ECell eCell8 = new ECell();
			eCell8.IsSingle = true;
			eCell8.Value = item4.Value<string>("accountname");
			ECell eCell9 = new ECell();
			eCell9.IsSingle = true;
			eCell9.Value = item4.Value<string>("direction");
			ECell eCell10 = new ECell();
			eCell10.IsSingle = true;
			eCell10.Value = item4.Value<string>("oppositeaccountcondition");
			ECell eCell11 = new ECell();
			eCell11.IsSingle = true;
			eCell11.Value = item4.Value<string>("riskwarn");
			ERow eRow4 = new ERow();
			eRow4["account"] = eCell8;
			eRow4["direction"] = eCell9;
			eRow4["opposite"] = eCell10;
			eRow4["warn"] = eCell11;
			eTable4.Add(eRow4);
		}
		ledgerValidator.VoucherPositive = eTable4;
		ETable eTable5 = new ETable();
		foreach (JToken item5 in (IEnumerable<JToken>)ret["vd_vouchervalidatereverse"])
		{
			ECell eCell12 = new ECell();
			eCell12.IsSingle = true;
			eCell12.Value = item5.Value<string>("accountname");
			ECell eCell13 = new ECell();
			eCell13.IsSingle = true;
			eCell13.Value = item5.Value<string>("direction");
			ECell eCell14 = new ECell();
			eCell14.IsSingle = false;
			eCell14.Value = item5.Value<string>("oppositeaccountcondition");
			eCell14.Values = item5.Value<string>("oppositeaccountcondition").ToPatternList().ToList();
			ECell eCell15 = new ECell();
			eCell15.IsSingle = true;
			eCell15.Value = item5.Value<string>("riskwarn");
			ERow eRow5 = new ERow();
			eRow5["account"] = eCell12;
			eRow5["direction"] = eCell13;
			eRow5["opposite"] = eCell14;
			eRow5["warn"] = eCell15;
			eTable5.Add(eRow5);
		}
		ledgerValidator.VoucherReverse = eTable5;
		ledgerValidator.Version = (int)ret["ledgervalidatedicversion"];
		return ledgerValidator;
	}

	private static IEnumerable<string> ToPatternList(this string str)
	{
		string[] source = str.Split('，', ',');
		IEnumerable<string> removes = source.Where((string p) => Regex.IsMatch(p, "-(.+)-"));
		IEnumerable<string> source2 = source.Where((string p) => !Regex.IsMatch(p, "-(.+)-"));
		string text = string.Join("|", removes.Select((string p) => Regex.Match(p, "-(.+)-").Groups[1].Value));
		string removeStr = "((?!(" + text + ")).)*";
		return source2.Select((string p) => "^" + p.Replace("*", (removes.Count() > 0) ? removeStr : ".*") + "$");
	}

	private static string ToPattern(this string str)
	{
		return "^" + str.Replace("*", ".*") + "$";
	}
}
