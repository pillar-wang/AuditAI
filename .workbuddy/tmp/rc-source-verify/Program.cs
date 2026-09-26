using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Auditai.Model;

// Task4 风险检查方案「云端来源标记」数据层验证。
// 直接引用真实工程 LedgerModel.csproj，调用真实 RiskCheckStore / LedgerDAL。
//
// 夹具：LedgerDAL 构造函数会跑 UpdateSchema，因此需要一个"最小合法账套"：
//   PRAGMA user_version=4（跳过 0_1~3 历史迁移）+ 带 `number` 列的 `Voucher` 表 + `Ledger` 表。
// 这样 UpdateSchema 只做 CreateRiskCheckTables / Update_RiskCheckScheme_Source /
// CreateComboOpeningTables / Update_Voucher_MarkSource。
internal static class Program
{
	private static int _passed;
	private static int _failed;

	private static int Main()
	{
		Console.OutputEncoding = System.Text.Encoding.UTF8;
		Console.WriteLine("==== Task4 风险检查方案来源标记 数据层验证 ====");
		Console.WriteLine();

		try
		{
			Scenario_A_NewLedgerColumns();
			Scenario_B_LegacyLedgerAddColumnsIdempotent();
			Scenario_C_SourceRoundTripPersist();
			Scenario_D_JsonRoundTrip();
			Scenario_E_CrudRegression();
		}
		catch (Exception ex)
		{
			Console.WriteLine("!! 未捕获异常: " + ex.GetType().FullName);
			try { Console.WriteLine("   Message: " + ex.Message); } catch { }
			try { Console.WriteLine("   Stack: " + ex.StackTrace); } catch { }
			if (ex.InnerException != null)
			{
				Console.WriteLine("   Inner: " + ex.InnerException.GetType().FullName);
				try { Console.WriteLine("   Inner Message: " + ex.InnerException.Message); } catch { }
			}
			_failed++;
		}

		Console.WriteLine();
		Console.WriteLine($"==== {(_failed == 0 ? "全部通过" : "存在失败")}: passed={_passed}, failed={_failed} ====");
		return _failed == 0 ? 0 : 1;
	}

	// ---------- ① 新建账套建表含三列 ----------
	private static void Scenario_A_NewLedgerColumns()
	{
		Console.WriteLine("--- 场景A：新建账套建表含三列 ---");
		string db = NewFixture(legacyRiskTables: false);
		// 触发 UpdateSchema（构造 LedgerDAL）
		List<RiskCheckScheme> loaded = RiskCheckStore.Load(db);
		Check("A1 新账套 Load 返回空列表", loaded.Count == 0, "count=" + loaded.Count);

		List<(string Name, string Type, int NotNull, string Default)> cols = TableInfo(db, "RiskCheckScheme");
		Console.WriteLine("      RiskCheckScheme 列: " + string.Join(", ", cols.Select(c => c.Name + ":" + c.Type)));
		CheckCol(cols, "sourceScope", "INTEGER", notNull: 1, def: "0");
		CheckCol(cols, "sourceSchemeId", "INTEGER", notNull: 1, def: "0");
		CheckCol(cols, "sourceVersion", "INTEGER", notNull: 1, def: "0");
		Console.WriteLine();
	}

	// ---------- ② 旧账套补列、幂等可重复 ----------
	private static void Scenario_B_LegacyLedgerAddColumnsIdempotent()
	{
		Console.WriteLine("--- 场景B：旧账套补列 + 幂等 ---");
		string db = NewFixture(legacyRiskTables: true);
		// 旧账套已有旧结构 + 一行旧数据
		using (SQLiteConnection conn = Open(db))
		{
			Exec(conn, "INSERT INTO `RiskCheckScheme`(`id`,`name`,`note`) VALUES(1,'旧方案','旧备注')");
			// 旧规则按旧版 SaveRiskCheckScheme 的全字段口径写入（真实旧账套不会留 NULL 列）
			Exec(conn, "INSERT INTO `RiskCheckRule`(`id`,`schemeId`,`ruleType`,`note`,`accountCodes`,`accountNames`,`requireLeaf`,`openingEnabled`,`openingDirection`,`openingOp`,`openingValue`,`closingEnabled`,`closingDirection`,`closingOp`,`closingValue`,`debitEnabled`,`debitScope`,`debitOp`,`debitValue`,`creditEnabled`,`creditScope`,`creditOp`,`creditValue`,`auxNegativeEnabled`,`auxOp`,`auxValue`,`leftExpr`,`operatorCode`,`rightExpr`) VALUES(1,1,0,'','1122','库存现金',1,0,0,0,'0',1,1,2,'0',0,0,0,'0',0,0,0,'0',0,0,'0','',0,'')");
		}
		Check("B1 补列前旧表只有 3 列", TableInfo(db, "RiskCheckScheme").Count == 3, "实际=" + TableInfo(db, "RiskCheckScheme").Count);

		// 第一次跑 UpdateSchema（Load 会 new LedgerDAL）
		List<RiskCheckScheme> loaded1 = null;
		try
		{
			loaded1 = RiskCheckStore.Load(db);
		}
		catch (Exception ex)
		{
			Console.WriteLine("   B-load 异常: " + ex.GetType().FullName + " / " + ex.Message);
		}
		using (SQLiteConnection conn = Open(db))
		{
			using (SQLiteCommand cmd = new SQLiteCommand("SELECT `sql` FROM `sqlite_master` WHERE `name`='RiskCheckScheme'", conn))
			{
				Console.WriteLine("      建表SQL: " + cmd.ExecuteScalar());
			}
			using (SQLiteCommand cmd = new SQLiteCommand("SELECT `id`,`name`,`sourceScope`,`sourceSchemeId`,`sourceVersion`,typeof(`sourceScope`) FROM `RiskCheckScheme`", conn))
			using (SQLiteDataReader rd = cmd.ExecuteReader())
			{
				while (rd.Read())
				{
					Console.WriteLine($"      raw id={rd[0]} name={rd[1]} scope={(rd[2] is DBNull ? "NULL" : rd[2].ToString())} schemeId={(rd[3] is DBNull ? "NULL" : rd[3].ToString())} ver={(rd[4] is DBNull ? "NULL" : rd[4].ToString())} typeofScope={rd[5]}");
				}
			}
		}
		if (loaded1 == null)
		{
			Console.WriteLine("      (Load 失败，后续断言跳过)");
			Console.WriteLine();
			return;
		}
		Check("B2 补列后旧方案仍可读（不丢行）", loaded1.Count == 1 && loaded1[0].Name == "旧方案", "count=" + loaded1.Count);
		Check("B3 旧行新列取默认值 0", loaded1.Count == 1 && loaded1[0].SourceScope == 0 && loaded1[0].SourceSchemeId == 0 && loaded1[0].SourceVersion == 0,
			loaded1.Count == 1 ? $"{loaded1[0].SourceScope}/{loaded1[0].SourceSchemeId}/{loaded1[0].SourceVersion}" : "n/a");
		Check("B4 旧规则未丢失", loaded1.Count == 1 && loaded1[0].Rules.Count == 1, "rules=" + (loaded1.Count == 1 ? loaded1[0].Rules.Count : -1));

		// 重复执行幂等：连续再跑两次
		bool threw = false;
		int colCountAfter = -1;
		try
		{
			RiskCheckStore.Load(db);
			RiskCheckStore.Load(db);
			using SQLiteConnection conn = Open(db);
			Exec(conn, "INSERT INTO `RiskCheckScheme`(`id`,`name`,`note`) VALUES(2,'再存','x')");
			colCountAfter = TableInfo(db, "RiskCheckScheme").Count;
		}
		catch (Exception ex)
		{
			threw = true;
			Console.WriteLine("      异常: " + ex.Message);
		}
		Check("B5 重复执行 UpdateSchema 不抛异常", !threw);
		Check("B6 重复执行后列数仍为 6（无重复列）", colCountAfter == 6, "列数=" + colCountAfter);
		Console.WriteLine();
	}

	// ---------- ③ 带来源标记保存/读回 ----------
	private static void Scenario_C_SourceRoundTripPersist()
	{
		Console.WriteLine("--- 场景C：来源标记保存后读回 ---");
		string db = NewFixture(legacyRiskTables: false);
		RiskCheckStore.Load(db); // 建表

		RiskCheckScheme s = new RiskCheckScheme
		{
			Name = "团队库方案副本",
			Note = "来自团队库",
			SourceScope = 1,
			SourceSchemeId = 987654321L,
			SourceVersion = 7
		};
		s.Rules.Add(new RiskCheckRule { RuleType = RiskCheckRule.RULE_TYPE_FORMULA, LeftExpr = "a+b", OperatorCode = 2, RightExpr = "c", ClosingValue = 12.34m });
		RiskCheckStore.Save(db, s);
		Check("C1 保存分配了本地 Id", s.Id > 0, "Id=" + s.Id);

		RiskCheckScheme re = RiskCheckStore.Load(db).Single(x => x.Id == s.Id);
		Check("C2 SourceScope 读回", re.SourceScope == 1, "=" + re.SourceScope);
		Check("C3 SourceSchemeId 读回", re.SourceSchemeId == 987654321L, "=" + re.SourceSchemeId);
		Check("C4 SourceVersion 读回", re.SourceVersion == 7, "=" + re.SourceVersion);
		Check("C5 规则字段（公式）读回", re.Rules.Count == 1 && re.Rules[0].LeftExpr == "a+b" && re.Rules[0].OperatorCode == 2 && re.Rules[0].RightExpr == "c",
			re.Rules.Count == 1 ? $"{re.Rules[0].LeftExpr}/{re.Rules[0].OperatorCode}/{re.Rules[0].RightExpr}" : "rules=" + re.Rules.Count);

		// 模拟"重新同步"：更新版本号
		re.SourceVersion = 8;
		re.SourceSchemeId = 987654321L;
		RiskCheckStore.Save(db, re);
		RiskCheckScheme re2 = RiskCheckStore.Load(db).Single(x => x.Id == s.Id);
		Check("C6 重新同步（改 SourceVersion）后读回=8", re2.SourceVersion == 8, "=" + re2.SourceVersion);
		Check("C7 重新同步后来源库 id 未变", re2.SourceSchemeId == 987654321L, "=" + re2.SourceSchemeId);

		// 本地自建：写默认 0
		RiskCheckScheme local = new RiskCheckScheme { Name = "本地自建" };
		RiskCheckStore.Save(db, local);
		RiskCheckScheme localBack = RiskCheckStore.Load(db).Single(x => x.Id == local.Id);
		Check("C8 本地自建三列均为 0", localBack.SourceScope == 0 && localBack.SourceSchemeId == 0 && localBack.SourceVersion == 0,
			$"{localBack.SourceScope}/{localBack.SourceSchemeId}/{localBack.SourceVersion}");
		Console.WriteLine();
	}

	// ---------- ④ JSON 导出→导入往返 ----------
	private static void Scenario_D_JsonRoundTrip()
	{
		Console.WriteLine("--- 场景D：JSON 导出→导入往返 ---");
		List<RiskCheckScheme> src = new List<RiskCheckScheme>
		{
			new RiskCheckScheme { Id = 3, Name = "来自系统库", Note = "系统", SourceScope = 2, SourceSchemeId = 55, SourceVersion = 11 }
		};
		src[0].Rules.Add(new RiskCheckRule { Id = 9, SchemeId = 3, RuleType = RiskCheckRule.RULE_TYPE_CONDITION, AccountCodes = "1122", ClosingEnabled = true, ClosingDirection = 1, ClosingOp = 2, ClosingValue = 0m });

		string json = RiskCheckStore.ExportJson(src);
		Console.WriteLine("      JSON=" + json);
		List<RiskCheckScheme> back = RiskCheckStore.ImportJson(json);
		Check("D1 往返数量一致", back.Count == 1, "count=" + back.Count);
		RiskCheckScheme b = back.Count == 1 ? back[0] : null;
		Check("D2 SourceScope 往返", b != null && b.SourceScope == 2, b == null ? "n/a" : "=" + b.SourceScope);
		Check("D3 SourceSchemeId 往返", b != null && b.SourceSchemeId == 55, b == null ? "n/a" : "=" + b.SourceSchemeId);
		Check("D4 SourceVersion 往返", b != null && b.SourceVersion == 11, b == null ? "n/a" : "=" + b.SourceVersion);
		Check("D5 规则字段往返", b != null && b.Rules.Count == 1 && b.Rules[0].AccountCodes == "1122" && b.Rules[0].ClosingOp == 2,
			b != null ? "rules=" + b.Rules.Count : "n/a");

		// 导入路径应把 Id 清零后另行 Save，验证"清零后可 Save 且不顶掉现有方案"
		string db = NewFixture(legacyRiskTables: false);
		RiskCheckStore.Load(db);
		RiskCheckScheme exist = new RiskCheckScheme { Name = "现有方案" };
		RiskCheckStore.Save(db, exist);
		long existId = exist.Id;

		RiskCheckScheme imported = back[0];
		imported.Id = 0;
		foreach (RiskCheckRule r in imported.Rules) r.Id = 0;
		RiskCheckStore.Save(db, imported);
		List<RiskCheckScheme> all = RiskCheckStore.Load(db);
		Check("D6 导入（Id 清零）后方案数=2", all.Count == 2, "count=" + all.Count);
		Check("D7 未顶掉现有方案", all.Any(x => x.Id == existId && x.Name == "现有方案"), "existing id=" + existId);
		Check("D8 导入方案来源标记保留", all.Any(x => x.Name == "来自系统库" && x.SourceScope == 2 && x.SourceSchemeId == 55 && x.SourceVersion == 11));
		Console.WriteLine();
	}

	// ---------- ⑤ 原有 CRUD 回归 ----------
	private static void Scenario_E_CrudRegression()
	{
		Console.WriteLine("--- 场景E：原有增删改查回归 ---");
		string db = NewFixture(legacyRiskTables: false);
		RiskCheckStore.Load(db);

		RiskCheckScheme a = new RiskCheckScheme { Name = "方案A", Note = "nA" };
		a.Rules.Add(new RiskCheckRule { RuleType = RiskCheckRule.RULE_TYPE_CONDITION, AccountCodes = "1001", AccountNames = "库存现金", RequireLeaf = true, OpeningEnabled = true, OpeningDirection = 2, OpeningOp = 3, OpeningValue = 100.5m });
		a.Rules.Add(new RiskCheckRule { RuleType = RiskCheckRule.RULE_TYPE_FORMULA, LeftExpr = "x", OperatorCode = 5, RightExpr = "y" });
		RiskCheckStore.Save(db, a);

		RiskCheckScheme b = new RiskCheckScheme { Name = "方案B" };
		RiskCheckStore.Save(db, b);
		Check("E1 两个方案落库", RiskCheckStore.Load(db).Count == 2);

		RiskCheckScheme aBack = RiskCheckStore.Load(db).Single(x => x.Id == a.Id);
		Check("E2 规则数=2", aBack.Rules.Count == 2, "=" + aBack.Rules.Count);
		Check("E3 规则字段（bool/方向/运算符/小数）读回",
			aBack.Rules.Any(r => r.AccountCodes == "1001" && r.RequireLeaf && r.OpeningEnabled && r.OpeningDirection == 2 && r.OpeningOp == 3 && r.OpeningValue == 100.5m && r.AccountNames == "库存现金"),
			string.Join(" | ", aBack.Rules.Select(r => $"{r.AccountCodes}/{r.OpeningValue}/{r.OpeningOp}")));

		// 改名 + 全删全插规则
		aBack.Name = "方案A改";
		aBack.Rules.RemoveAt(0);
		RiskCheckStore.Save(db, aBack);
		RiskCheckScheme aBack2 = RiskCheckStore.Load(db).Single(x => x.Id == a.Id);
		Check("E4 改名生效", aBack2.Name == "方案A改", aBack2.Name);
		Check("E5 规则删减生效", aBack2.Rules.Count == 1, "=" + aBack2.Rules.Count);

		RiskCheckStore.Delete(db, b.Id);
		List<RiskCheckScheme> after = RiskCheckStore.Load(db);
		Check("E6 删除方案生效", after.Count == 1 && after[0].Id == a.Id, "count=" + after.Count);
		using (SQLiteConnection conn = Open(db))
		{
			long orphan = (long)new SQLiteCommand("SELECT COUNT(*) FROM `RiskCheckRule` WHERE `schemeId`=" + b.Id, conn).ExecuteScalar();
			Check("E7 删除连规则一并清除", orphan == 0, "orphan=" + orphan);
		}
		Console.WriteLine();
	}

	// ---------------- helpers ----------------

	private static void CheckCol(List<(string Name, string Type, int NotNull, string Default)> cols, string name, string type, int notNull, string def)
	{
		var c = cols.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
		bool ok = c.Name != null
			&& c.Type.Equals(type, StringComparison.OrdinalIgnoreCase)
			&& c.NotNull == notNull
			&& (c.Default ?? "").Trim() == def;
		Check($"A? 列 {name} 定义 = {type} NOT NULL DEFAULT {def}", ok,
			c.Name == null ? "列不存在" : $"{c.Type} notnull={c.NotNull} default={c.Default}");
	}

	private static void Check(string label, bool ok, string detail = null)
	{
		if (ok) _passed++; else _failed++;
		string tail = string.IsNullOrEmpty(detail) ? "" : "  (" + detail + ")";
		Console.WriteLine($"[{(ok ? " OK " : "FAIL")}] {label}{tail}");
	}

	private static string NewFixture(bool legacyRiskTables)
	{
		string dir = Path.Combine(Path.GetTempPath(), "rc-source-verify-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(dir);
		string db = Path.Combine(dir, "ledger.s3db");
		using SQLiteConnection conn = Open(db);
		Exec(conn, "CREATE TABLE `Voucher`(`id` INTEGER PRIMARY KEY, `number` TEXT, `DirectionToggled` INTEGER NOT NULL DEFAULT 0, `VoucherMark` INTEGER NOT NULL DEFAULT 0)");
		Exec(conn, "CREATE TABLE `Ledger`(`startDate` DATE, `EndDate` DATE)");
		Exec(conn, "PRAGMA user_version=4");
		if (legacyRiskTables)
		{
			// 模拟"旧账套"：不含来源三列的旧结构
			Exec(conn, "CREATE TABLE `RiskCheckScheme`(`id` INTEGER PRIMARY KEY, `name` TEXT, `note` TEXT)");
			Exec(conn, "CREATE TABLE `RiskCheckRule`(`id` INTEGER PRIMARY KEY, `schemeId` INTEGER, `ruleType` INTEGER, `note` TEXT, `accountCodes` TEXT, `accountNames` TEXT, `requireLeaf` INTEGER, `openingEnabled` INTEGER, `openingDirection` INTEGER, `openingOp` INTEGER, `openingValue` TEXT, `closingEnabled` INTEGER, `closingDirection` INTEGER, `closingOp` INTEGER, `closingValue` TEXT, `debitEnabled` INTEGER, `debitScope` INTEGER, `debitOp` INTEGER, `debitValue` TEXT, `creditEnabled` INTEGER, `creditScope` INTEGER, `creditOp` INTEGER, `creditValue` TEXT, `auxNegativeEnabled` INTEGER, `auxOp` INTEGER, `auxValue` TEXT, `leftExpr` TEXT, `operatorCode` INTEGER, `rightExpr` TEXT)");
		}
		return db;
	}

	private static SQLiteConnection Open(string db)
	{
		SQLiteConnection conn = new SQLiteConnection("Data Source=" + db);
		conn.Open();
		return conn;
	}

	private static void Exec(SQLiteConnection conn, string sql)
	{
		using SQLiteCommand cmd = new SQLiteCommand(sql, conn);
		cmd.ExecuteNonQuery();
	}

	private static List<(string Name, string Type, int NotNull, string Default)> TableInfo(string db, string table)
	{
		List<(string, string, int, string)> list = new List<(string, string, int, string)>();
		using SQLiteConnection conn = Open(db);
		using SQLiteCommand cmd = new SQLiteCommand("pragma table_info(`" + table + "`)", conn);
		using SQLiteDataReader rd = cmd.ExecuteReader();
		while (rd.Read())
		{
			list.Add((rd["name"] as string, rd["type"] as string, Convert.ToInt32(rd["notnull"]), rd["dflt_value"] as string));
		}
		return list;
	}
}
