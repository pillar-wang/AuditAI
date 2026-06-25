using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Text.RegularExpressions;

namespace Auditai.Model;

public abstract class CrawlerBase
{
	private static readonly Regex _reSuperData = new Regex("SD\\d{5}(N?)_SDAccset", RegexOptions.IgnoreCase);

	public readonly List<string> ValidTables = new List<string>();

	public abstract string Brand { get; }

	public abstract string FriendlyName { get; }

	public abstract string Name { get; }

	public abstract string DatabaseUser { get; set; }

	public abstract ScanLocalMode ScanLocalMode { get; }

	public abstract LSDb.DbProvider DbProvider { get; }

	public abstract int MaxProgress { get; }

	public abstract Bitmap Icon { get; }

	public abstract Bitmap Logo { get; }

	public static event EventHandler<CrawlerScanProgressEventArgs> ScanProgressChanged;

	public abstract DatabaseInfo ScanLocal();

	public abstract IEnumerable<LedgerInfo> ScanRemote(DatabaseInfo dbInfo);

	public abstract bool CanHandle(DatabaseInfo dbInfo);

	public abstract Ledger GetLedger(LedgerInfo ledgerInfo);

	public IEnumerable<Ledger> GetLedgers(IEnumerable<LedgerInfo> ledgerInfos)
	{
		return ledgerInfos.Select((LedgerInfo x) => GetLedger(x)).ToList();
	}

	protected static void OnScanProgressChanged(string moduleName)
	{
		CrawlerBase.ScanProgressChanged?.Invoke(null, new CrawlerScanProgressEventArgs(moduleName));
	}

	public static void ScanLocalSqlServer(out string module, out DatabaseInfo info)
	{
		module = null;
		info = null;
		string[] array = Util.GetSqlServerInstanceNames();
		if (array == null)
		{
			array = new string[1] { string.Empty };
		}
		int timeout = 3;
		string[] array2 = array;
		int num = 0;
		if (num >= array2.Length)
		{
			return;
		}
		string text = array2[num];
		try
		{
			DatabaseInfo databaseInfo = new DatabaseInfo
			{
				DatabaseType = LSDb.DbProvider.SqlServer,
				DataSource = ".\\" + text,
				IntegratedSecurity = true,
				Name = "master"
			};
			LSDb lSDb = LSDb.Create(databaseInfo.DatabaseType);
			lSDb.DataSource = databaseInfo.DataSource;
			lSDb.IntegratedSecurity = databaseInfo.IntegratedSecurity;
			lSDb.Database = databaseInfo.Name;
			OnScanProgressChanged("金蝶K3");
			if (lSDb.TableExists("t_kdaccount_gl", timeout))
			{
				module = "Kingdee_K3_Sql";
				info = databaseInfo;
				return;
			}
			OnScanProgressChanged("管家婆财贸双全");
			if (lSDb.TableExists("GraspcwZt", timeout))
			{
				module = "Grasp_CMSQ_Sql";
				info = databaseInfo;
				return;
			}
			OnScanProgressChanged("用友U8");
			IEnumerable<string> databaseNames = lSDb.GetDatabaseNames(timeout);
			if (databaseNames == null)
			{
				return;
			}
			if (databaseNames.Contains("UFSystem"))
			{
				databaseInfo.DataSource = "UFSystem";
				lSDb.Database = databaseInfo.DataSource;
				if (lSDb.TableExists("UA_Account", timeout))
				{
					module = "Yonyou_U8V10_Sql";
					info = databaseInfo;
					return;
				}
			}
			OnScanProgressChanged("用友R9");
			if (databaseNames.Contains("anyisys"))
			{
				databaseInfo.DataSource = "anyisys";
				lSDb.Database = databaseInfo.DataSource;
				if (lSDb.TableExists("AnyiGL", timeout))
				{
					module = "Yonyou_R9_Sql";
					info = databaseInfo;
					return;
				}
			}
			OnScanProgressChanged("天思");
			if (databaseNames.Contains("TbrSystem"))
			{
				databaseInfo.DataSource = "TbrSystem";
				lSDb.Database = databaseInfo.DataSource;
				if (lSDb.TableExists("COMP", timeout))
				{
					module = "MasterService_T6_Sql";
					info = databaseInfo;
					return;
				}
			}
			OnScanProgressChanged("远光");
			if (databaseNames.Contains("XBHS30"))
			{
				databaseInfo.DataSource = "XBHS30";
				lSDb.Database = databaseInfo.DataSource;
				if (lSDb.TableExists("TFAccountBook", timeout))
				{
					module = "YGSoft_3_Sql";
					info = databaseInfo;
					return;
				}
			}
			OnScanProgressChanged("新中大银色快车");
			if (databaseNames.Contains("PUBDATA"))
			{
				databaseInfo.DataSource = "PUBDATA";
				lSDb.Database = databaseInfo.DataSource;
				if (lSDb.TableExists("unit", timeout))
				{
					module = "Newgrand_SE_Sql";
					info = databaseInfo;
					return;
				}
			}
			OnScanProgressChanged("速达");
			foreach (string item in databaseNames)
			{
				Match match = _reSuperData.Match(item);
				if (!match.Success)
				{
					continue;
				}
				databaseInfo.DataSource = item;
				lSDb.Database = databaseInfo.DataSource;
				if (lSDb.TableExists("accset", timeout))
				{
					if (match.Groups[1].Length == 0)
					{
						module = "SuperData_5000_Sql";
						info = databaseInfo;
					}
					else
					{
						module = "SuperData_3000_Sql";
						info = databaseInfo;
					}
					break;
				}
			}
		}
		catch
		{
		}
	}

	public static void ScanLocalDesktop(out string module, out string path)
	{
		module = null;
		path = null;
		List<string> list = (from d in DriveInfo.GetDrives()
			where d.DriveType == DriveType.Fixed
			select d.RootDirectory.FullName).Concat(new string[1] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) }).ToList();
		string location = Assembly.GetExecutingAssembly().Location;
		string directoryName = Path.GetDirectoryName(location);
		List<Tuple<string, string, string, string>> scanLocalLookup = GetScanLocalLookup();
		foreach (Tuple<string, string, string, string> item in scanLocalLookup)
		{
			OnScanProgressChanged(item.Item4);
			foreach (string item2 in list)
			{
				if (!ScanLocalDesktopImpl(item2, item.Item1, item.Item2) || !File.Exists(Path.Combine(directoryName, item.Item3 + ".dll")))
				{
					continue;
				}
				module = item.Item3;
				path = Path.Combine(item2, item.Item1);
				return;
			}
		}
	}

	public static bool ScanLocalDesktopImpl(string loc, string module, string filePattern)
	{
		try
		{
			FileInfo[] files = new DirectoryInfo(Path.Combine(loc, module)).GetFiles(filePattern, SearchOption.AllDirectories);
			return files.Length != 0;
		}
		catch (DirectoryNotFoundException)
		{
			return false;
		}
		catch (IOException)
		{
			return false;
		}
		catch (AuthenticationException)
		{
			return false;
		}
	}

	private static List<Tuple<string, string, string, string>> GetScanLocalLookup()
	{
		return new List<Tuple<string, string, string, string>>
		{
			Tuple.Create("Kingdee\\KIS", "*.ais", "Kingdee_KIS_Jet", "金蝶KIS"),
			Tuple.Create("A9CWXT标准版(单机)\\Database", "*.mdb", "A9_CW_Jet", "来势A9"),
			Tuple.Create("anyiv5e\\data", "data.mdb", "Anyiwang_Jet", "安易王"),
			Tuple.Create("gasoft\\AC98STD\\data", "*.gdb", "eAbax_6F_Jet", "金算盘6F"),
			Tuple.Create("gasoft\\AC98\\data", "*.gdb", "eAbax_6F_Jet", "金算盘6F"),
			Tuple.Create("4Fang\\4Finance\\Data", "DB4F_*.mdb", "SiFang_Jet", "四方"),
			Tuple.Create("eabax\\AC98STD\\data", "*.gdb", "eAbax_Erpb_Jet", "金算盘ERPB")
		};
	}

	private static void Main(string[] args)
	{
	}
}
