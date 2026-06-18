﻿﻿﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using Leqisoft.Model.Crawlers.Properties;
using ParadoxReader;

namespace Leqisoft.Model.Crawlers;

public class XinJiYuan : CrawlerBase
{
	private int _progress = 0;

	public const int PROGRESS_MAX = 10;

	public override string Brand => "新纪元账证查询软件";

	public override string FriendlyName => "新纪元账证查询软件\r\n(*.001文件)";

	public override string Name => "XinJiYuan";

	public override string DatabaseUser { get; set; } = "sa";


	public override ScanLocalMode ScanLocalMode => ScanLocalMode.FullDatabaseInfo;

	public override LSDb.DbProvider DbProvider => LSDb.DbProvider.Paradox;

	public override int MaxProgress => 10;

	public override Bitmap Icon => Resource.Icon;

	public override Bitmap Logo => Resource.Logo;

	public event EventHandler<GetLedgerProgressEventArgs> ProgressChanged;

	public override bool CanHandle(DatabaseInfo dbInfo)
	{
		throw new NotImplementedException();
	}

	public override Ledger GetLedger(LedgerInfo ledgerInfo)
	{
		string tempPath = Path.GetTempPath();
		ExtractXjy(ledgerInfo.DbInfo.DataSource, tempPath);
		return FromFolder(tempPath, ledgerInfo);
	}

	public override DatabaseInfo ScanLocal()
	{
		throw new NotImplementedException();
	}

	public override IEnumerable<LedgerInfo> ScanRemote(DatabaseInfo dbInfo)
	{
		List<LedgerInfo> list = new List<LedgerInfo>();
		foreach (string file in Util.GetFiles(dbInfo.DataSource, "*.001"))
		{
			try
			{
				ExtractXjy(file, Path.GetTempPath());
				FileIniDataParser fileIniDataParser = new FileIniDataParser();
				IniData iniData = fileIniDataParser.ReadFile(Path.Combine(Path.GetTempPath(), "ztsjbf.ini"), Encoding.GetEncoding("gb2312"));
				list.Add(new LedgerInfo
				{
					Year = int.Parse(iniData["备份"]["帐套年度"]),
					CompanyName = iniData["备份"]["帐套名称"],
					LedgerNumber = iniData["备份"]["帐套编号"],
					DbInfo = new DatabaseInfo
					{
						DatabaseType = LSDb.DbProvider.Jet,
						DataSource = file
					}
				});
			}
			catch
			{
			}
		}
		return list;
	}

	protected void OnProgressChanged()
	{
		this.ProgressChanged?.Invoke(this, new GetLedgerProgressEventArgs(++_progress, string.Empty));
	}

	public string Convert001File(string srcFile, string desPath)
	{
		_progress = 0;
		string extension = Path.GetExtension(srcFile);
		if (extension != ".001")
		{
			throw new ArgumentException("不支持的文件格式");
		}
		string tempPath = Path.GetTempPath();
		OnProgressChanged();
		ExtractXjy(srcFile, tempPath);
		string text = Path.Combine(desPath, Path.GetFileNameWithoutExtension(srcFile) + ".db");
		Ledger ledger = FromFolder(tempPath, new LedgerInfo
		{
			DbInfo = new DatabaseInfo
			{
				DataSource = srcFile
			}
		});
		OnProgressChanged();
		ledger.SaveAsSqlite(text);
		OnProgressChanged();
		return text;
	}

	private Ledger FromFolder(string folder, LedgerInfo ledgerInfo)
	{
		FileIniDataParser fileIniDataParser = new FileIniDataParser();
		IniData iniData = null;
		try
		{
			iniData = fileIniDataParser.ReadFile(Path.Combine(folder, "ztsjbf.ini"), Encoding.GetEncoding("gb2312"));
		}
		catch (ParsingException innerException)
		{
			throw new Exception("无法解析该文件", innerException);
		}
		ledgerInfo.Year = int.Parse(iniData["备份"]["帐套年度"]);
		ledgerInfo.CompanyName = iniData["备份"]["帐套名称"];
		ledgerInfo.LedgerNumber = iniData["备份"]["帐套编号"];
		Ledger ledger = new Ledger(ledgerInfo);
		ledger.StartDate = new DateTime(ledgerInfo.Year, 1, 1);
		OnProgressChanged();
		Dictionary<string, Currency> dictionary = new Dictionary<string, Currency>();
		ParadoxTable paradoxTable = new ParadoxTable(folder, "wbdm");
		foreach (ParadoxRecord item3 in paradoxTable.Enumerate())
		{
			Currency currency = new Currency
			{
				Name = item3.GetValue("Wbmc").ToString()
			};
			ledger.ForeignCurrencies.Add(currency);
			dictionary.Add(item3.GetValue("Wbdm").ToString(), currency);
		}
		if (dictionary.Count == 0)
		{
			Currency currency2 = new Currency
			{
				Name = "人民币"
			};
			ledger.ForeignCurrencies.Add(currency2);
			dictionary.Add("RMB", currency2);
		}
		OnProgressChanged();
		Dictionary<string, Account> dictionary2 = new Dictionary<string, Account>();
		Dictionary<int, List<Account>> dictionary3 = new Dictionary<int, List<Account>>();
		paradoxTable = new ParadoxTable(folder, "km");
		bool flag = paradoxTable.FieldNameMap.ContainsKey("KM_YEFX");
		foreach (ParadoxRecord item4 in paradoxTable.Enumerate())
		{
			Account account = new Account();
			account.Code = item4.GetValue("Kmdm").ToString();
			account.Name = item4.GetValue("Kmmc").ToString();
			if (flag)
			{
				account.IsDebit = item4.GetValue("KM_YEFX").ToString() == "1";
			}
			ledger.Accounts.Add(account);
			dictionary2.Add(account.Code, account);
			if (int.TryParse(item4.GetValue("Kmjb").ToString(), out var result))
			{
				if (!dictionary3.ContainsKey(result))
				{
					dictionary3.Add(result, new List<Account>());
				}
				dictionary3[result].Add(account);
			}
		}
		OnProgressChanged();
		paradoxTable = new ParadoxTable(folder, "kmye");
		foreach (ParadoxRecord item5 in paradoxTable.Enumerate())
		{
			string text = item5.GetValue("Kmdm").ToString();
			if (text == null || !dictionary2.ContainsKey(text))
			{
				continue;
			}
			string s = item5.GetValue("Ncye").ToString();
			decimal result2;
			decimal num = (decimal.TryParse(s, out result2) ? result2 : 0m);
			Account account2 = dictionary2[text];
			if (flag)
			{
				account2.Balance = (account2.IsDebit ? num : (-num));
				continue;
			}
			if (num == 0m)
			{
				if (item5.GetValue("Yefx").ToString()?.Trim() == "加")
				{
					account2.IsDebit = false;
				}
				else
				{
					account2.IsDebit = true;
				}
			}
			else
			{
				account2.IsDebit = num >= 0m;
			}
			account2.Balance = Math.Abs(num);
		}
		OnProgressChanged();
		foreach (KeyValuePair<int, List<Account>> item6 in dictionary3)
		{
			if (item6.Key == 1)
			{
				continue;
			}
			int key = item6.Key - 1;
			List<Account> list = dictionary3[key];
			foreach (Account acc2 in item6.Value)
			{
				Account account3 = list.Find((Account a) => acc2.Code.StartsWith(a.Code));
				if (account3 != null)
				{
					acc2.Parent = account3;
					acc2.Parent.Children.Add(acc2);
				}
			}
		}
		OnProgressChanged();
		int num2 = 0;
		Dictionary<string, VoucherType> dictionary4 = new Dictionary<string, VoucherType>();
		Dictionary<Voucher, string> dictionary5 = new Dictionary<Voucher, string>();
		paradoxTable = new ParadoxTable(folder, "jzpz");
		foreach (ParadoxRecord item7 in paradoxTable.Enumerate())
		{
			Voucher voucher = new Voucher
			{
				Id = num2++
			};
			string text2 = item7.GetValue("Pzlx_Mc").ToString();
			if (!dictionary4.ContainsKey(text2))
			{
				VoucherType voucherType = new VoucherType
				{
					Name = text2
				};
				dictionary4.Add(text2, voucherType);
				ledger.VoucherTypes.Add(voucherType);
			}
			voucher.Type = dictionary4[text2];
			voucher.Number = item7.GetValue("Pzbh").ToString();
			string text3 = item7.GetValue("Pzrq").ToString();
			voucher.Day = new DateTime(ledger.Year, int.Parse(text3.Substring(2, 2).Trim()), int.Parse(text3.Substring(4, 2).Trim()));
			voucher.Digest = item7.GetValue("Zy").ToString();
			string key2 = item7.GetValue("Kmdm").ToString();
			voucher.Account = dictionary2[key2];
			voucher.IsDebit = item7.GetValue("Jd").ToString() == "借";
			voucher.Amount = (decimal.TryParse(item7.GetValue("Rmb").ToString(), out var result3) ? result3 : 0m);
			voucher.Maker = item7.GetValue("Sr").ToString();
			voucher.Checker = item7.GetValue("Sh").ToString();
			voucher.Booker = item7.GetValue("Jzr").ToString();
			string key3 = item7.GetValue("Wbdm").ToString();
			voucher.Currency = (dictionary.ContainsKey(key3) ? dictionary[key3] : dictionary.First().Value);
			voucher.Foreign = new ForeignRecord();
			ledger.Vouchers.Add(voucher);
			object value2;
			if (item7.TryGetValue("Xmdm", out var value) && !string.IsNullOrEmpty(value.ToString()))
			{
				dictionary5.Add(voucher, value.ToString());
			}
			else if (item7.TryGetValue("FDetailID", out value2) && !string.IsNullOrEmpty(value2.ToString()))
			{
				dictionary5.Add(voucher, value2.ToString());
			}
		}
		OnProgressChanged();
		Dictionary<string, Item> dictionary6 = new Dictionary<string, Item>();
		Dictionary<string, ItemClass> dictionary7 = new Dictionary<string, ItemClass>();
		Dictionary<string, string> dictionary8 = new Dictionary<string, string>();
		paradoxTable = new ParadoxTable(folder, "xm");
		foreach (ParadoxRecord item8 in paradoxTable.Enumerate())
		{
			int result4;
			int num3 = (int.TryParse(item8.GetValue("Xmjb").ToString(), out result4) ? result4 : (-1));
			if (num3 == 1)
			{
				ItemClass itemClass = new ItemClass
				{
					Code = item8.GetValue("Xmdm").ToString(),
					Name = item8.GetValue("Xmmc").ToString()
				};
				dictionary8.Add(itemClass.Code, item8.GetValue("Xmdm").ToString());
				dictionary7.Add(itemClass.Code, itemClass);
				ledger.ItemClasses.Add(itemClass);
			}
			else
			{
				Item item2 = new Item
				{
					Code = item8.GetValue("Xmdm").ToString(),
					Name = item8.GetValue("Xmmc").ToString()
				};
				dictionary8.Add(item2.Code, item8.GetValue("Xmdm").ToString());
				dictionary6.Add(item2.Code, item2);
				ledger.Items.Add(item2);
			}
		}
		OnProgressChanged();
		foreach (KeyValuePair<string, ItemClass> classItem in dictionary7)
		{
			IEnumerable<KeyValuePair<string, Item>> enumerable = dictionary6.Where((KeyValuePair<string, Item> item) => item.Key.StartsWith(classItem.Key));
			foreach (KeyValuePair<string, Item> item9 in enumerable)
			{
				item9.Value.ItemClass = classItem.Value;
				item9.Value.ItemClass.Items.Add(item9.Value);
			}
		}
		OnProgressChanged();
		Dictionary<string, Dictionary<string, decimal>> dictionary9 = new Dictionary<string, Dictionary<string, decimal>>();
		paradoxTable = new ParadoxTable(folder, "xmye");
		foreach (ParadoxRecord item10 in paradoxTable.Enumerate())
		{
			try
			{
				string key4 = item10.GetValue("Xmdm").ToString().TrimEnd('.');
				if (!dictionary9.ContainsKey(key4))
				{
					dictionary9.Add(key4, new Dictionary<string, decimal>());
				}
				dictionary9[key4].Add(item10.GetValue("Kmdm").ToString(), decimal.TryParse(item10.GetValue("Ncye").ToString(), out var result5) ? result5 : 0m);
			}
			catch
			{
			}
		}
		OnProgressChanged();
		foreach (KeyValuePair<string, ItemClass> item11 in dictionary7)
		{
			foreach (Item item12 in item11.Value.Items)
			{
				if (!dictionary9.ContainsKey(item12.Code.TrimEnd('.')))
				{
					continue;
				}
				Dictionary<string, decimal> dictionary10 = dictionary9[item12.Code.TrimEnd('.')];
				foreach (KeyValuePair<string, decimal> item13 in dictionary10)
				{
					if (dictionary2.ContainsKey(item13.Key))
					{
						Account account4 = dictionary2[item13.Key];
						account4.ItemBalance.Add(item12, new ItemBalance
						{
							Balance = (account4.IsDebit ? item13.Value : (-item13.Value))
						});
					}
				}
			}
		}
		OnProgressChanged();
		Dictionary<string, List<string>> dictionary11 = new Dictionary<string, List<string>>();
		paradoxTable = new ParadoxTable(folder, "t_itemdetail");
		foreach (ParadoxRecord item14 in paradoxTable.Enumerate())
		{
			string key5 = item14.GetValue("FDetailID").ToString();
			dictionary11.Add(key5, new List<string>());
			for (int i = 2; i < item14.DataValues.Length; i++)
			{
				string text4 = item14.DataValues[i]?.ToString();
				if (!string.IsNullOrEmpty(text4))
				{
					dictionary11[key5].Add(text4);
				}
			}
		}
		OnProgressChanged();
		foreach (KeyValuePair<Voucher, string> item15 in dictionary5)
		{
			if (!dictionary11.ContainsKey(item15.Value))
			{
				continue;
			}
			List<string> list2 = dictionary11[item15.Value];
			foreach (string item16 in list2)
			{
				if (dictionary6.ContainsKey(item16))
				{
					item15.Key.Details.Add(dictionary6[item16]);
				}
			}
		}
		OnProgressChanged();
		foreach (ItemClass itemClass2 in ledger.ItemClasses)
		{
			itemClass2.Code = dictionary8[itemClass2.Code];
		}
		foreach (Item item17 in ledger.Items)
		{
			item17.Code = dictionary8[item17.Code];
		}
		OnProgressChanged();
		foreach (Account account5 in ledger.Accounts)
		{
			account5.Code = Regex.Replace(account5.Code, "\\s", "");
		}
		ledger.Accounts = ledger.Accounts.OrderBy((Account a) => a.Code).ToList();
		OnProgressChanged();
		foreach (Account account6 in ledger.Accounts)
		{
			decimal _balance2 = default(decimal);
			lastLevelSummary(account6, ref _balance2);
			account6.Balance = (account6.IsDebit ? _balance2 : (-_balance2));
		}
		return ledger;
		static void lastLevelSummary(Account acc, ref decimal _balance)
		{
			if (acc.Children.Count == 0)
			{
				_balance += (acc.IsDebit ? acc.Balance : (-acc.Balance));
				return;
			}
			foreach (Account child in acc.Children)
			{
				lastLevelSummary(child, ref _balance);
			}
		}
	}

	private void printTable(ParadoxTable paradoxTable)
	{
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		foreach (KeyValuePair<string, int> item in paradoxTable.FieldNameMap)
		{
			arrayList2.Add(item.Key);
		}
		arrayList.Add(arrayList2);
		foreach (ParadoxRecord item2 in paradoxTable.Enumerate())
		{
			ArrayList arrayList3 = new ArrayList();
			foreach (KeyValuePair<string, int> item3 in paradoxTable.FieldNameMap)
			{
				arrayList3.Add(item2.DataValues[item3.Value]);
			}
			arrayList.Add(arrayList3);
		}
	}

	public static void Main()
	{
		XinJiYuan xinJiYuan = new XinJiYuan();
		DatabaseInfo dbInfo = new DatabaseInfo
		{
			DataSource = "C:\\Xsj_Soft\\Xsjzb\\Bak\\MyData\\CwV131_忠县畅达建设投资有限公司.gdb2017"
		};
		List<LedgerInfo> list = xinJiYuan.ScanRemote(dbInfo).ToList();
		if (list.Count == 0)
		{
		}
		else
		{
			foreach (LedgerInfo item in list)
			{
			}
			foreach (LedgerInfo item2 in list)
			{
				try
				{
					xinJiYuan.GetLedger(item2);
				}
				catch (Exception)
				{
				}
			}
		}
	}

	public void ExtractXjy(string file001, string outputPath)
	{
		ExtractXjy_XjyFormat(file001, outputPath);
	}

	public void ExtractXjy_XjyFormat(string file001, string outputPath)
	{
		Encoding encoding = Encoding.GetEncoding("gb2312");
		using FileStream input = new FileStream(file001, FileMode.Open);
		using BinaryReader binaryReader = new BinaryReader(input);
		string @string = encoding.GetString(binaryReader.ReadBytes(binaryReader.ReadInt32()));
		binaryReader.ReadInt32();
		int num = binaryReader.ReadInt32();
		for (int i = 0; i <= num; i++)
		{
			OnProgressChanged();
			string string2 = encoding.GetString(binaryReader.ReadBytes(binaryReader.ReadInt32()));
			binaryReader.ReadInt32();
			binaryReader.ReadInt32();
			string path = Path.Combine(outputPath, Path.GetFileName(string2));
			using FileStream dstStream = new FileStream(path, FileMode.Create);
			int num2 = binaryReader.ReadInt32();
			do
			{
				using (MemoryStream baseInputStream = new MemoryStream(binaryReader.ReadBytes(num2)))
				{
					using InflaterInputStream srcStream = new InflaterInputStream(baseInputStream);
					srcStream.CopyTo(dstStream);
				}
				num2 = binaryReader.ReadInt32();
			}
			while (num2 > 0);
		}
		binaryReader.ReadInt32();
	}

	public bool ExtractXjy_ZipFormat(string file001, string outputPath)
	{
		string[] array = new string[2] { "6CC3F6BC08D6F5C29EE3F0DD5FA23015", "" };
		string[] array2 = array;
		foreach (string text in array2)
		{
			try
			{
				using (ZipInputStream zipInputStream = new ZipInputStream(new FileStream(file001, FileMode.Open, FileAccess.Read)))
				{
					if (!string.IsNullOrEmpty(text))
					{
						zipInputStream.Password = text;
					}
					ZipEntry nextEntry = zipInputStream.GetNextEntry();
					do
					{
						string path = Path.Combine(outputPath, nextEntry.Name);
						using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
						int num = 2048;
						byte[] array3 = new byte[2048];
						do
						{
							num = zipInputStream.Read(array3, 0, array3.Length);
							fileStream.Write(array3, 0, num);
						}
						while (num > 0);
					}
					while ((nextEntry = zipInputStream.GetNextEntry()) != null);
				}
				return true;
			}
			catch (Exception)
			{
			}
		}
		return false;
	}
}
