using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Reflection;
using System.IO;
using System.Linq;
using Auditai.Model;

// 风险检查方案保存（rule.Id 分配）端到端验证。
//
// 直接引用真实工程 LedgerModel.csproj，调用真实的 RiskCheckStore / LedgerDAL，
// 复刻 RiskCheckEditor 的真实调用序列：AddRule（rule.Id=0，追加到 scheme.Rules）→ SaveCurrentScheme（整方案保存）。
//
// 夹具：LedgerDAL 构造函数会跑 UpdateSchema，因此需要一个"最小合法账套"：
//   PRAGMA user_version=4（跳过 0_1~3 的历史迁移）+ 带 `number` 列的 `Voucher` 表。
// 这样 UpdateSchema 只做 CreateRiskCheckTables / CreateComboOpeningTables / Update_Voucher_MarkSource。
internal static class Program
{
	private static int _failed;
	private static int _passed;

	private static int Main()
	{
		Console.OutputEncoding = System.Text.Encoding.UTF8;
		string dir = Path.Combine(Path.GetTempPath(), "rc-verify-" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(dir);
		string db = Path.Combine(dir, "ledger.s3db");
		CreateLedgerFixture(db);

		Console.WriteLine("夹具账套: " + db);
		Console.WriteLine();

		RiskCheckScheme schemeA = new RiskCheckScheme { Name = "方案A" };
		Save(db, schemeA, "建方案A（0条）");

		AddRule(db, schemeA, "A 加第1条");
		AddRule(db, schemeA, "A 加第2条");
		AddRule(db, schemeA, "A 加第3条");

		RiskCheckScheme schemeB = new RiskCheckScheme { Name = "方案B" };
		Save(db, schemeB, "建方案B（0条）");
		AddRule(db, schemeB, "B 加第1条");
		AddRule(db, schemeB, "B 加第2条");

		AddRule(db, schemeA, "A 再加第4条");
		Save(db, schemeA, "A 重存（不新增）");

		schemeA.Rules.RemoveAt(0);
		Save(db, schemeA, "A 删1条后重存");

		// 模拟"重启账套后再新增"：从库中重新加载后继续加规则
		List<RiskCheckScheme> loaded = RiskCheckStore.Load(db);
		Console.WriteLine();
		Console.WriteLine("重新加载：方案数=" + loaded.Count);
		foreach (RiskCheckScheme s in loaded)
		{
			Console.WriteLine($"  方案 id={s.Id} {s.Name} 规则Id=[{Join(s.Rules)}]");
		}
		if (loaded.Count > 0)
		{
			AddRule(db, loaded[0], "重启后（" + loaded[0].Name + "）再加1条");
		}

		// 落库结果与内存一致性核对
		Console.WriteLine();
		Console.WriteLine("最终落库：");
		bool ok = true;
		foreach (RiskCheckScheme s in RiskCheckStore.Load(db))
		{
			string ids = Join(s.Rules);
			Console.WriteLine($"  schemeId={s.Id} {s.Name} 名称码点=[{string.Join(" ", s.Name.Select(ch => ((int)ch).ToString("X4")))}] -> [{ids}]");
			HashSet<long> seen = new HashSet<long>();
			foreach (RiskCheckRule r in s.Rules)
			{
				if (r.SchemeId != s.Id || !seen.Add(r.Id))
				{
					ok = false;
					Console.WriteLine($"    !! 规则 id={r.Id} schemeId={r.SchemeId} 与所属方案不一致/重复");
				}
			}
		}

		// 全库 Id 唯一性（表主键约束的独立复核）
		using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + db))
		{
			conn.Open();
			long total = (long)new SQLiteCommand("SELECT COUNT(*) FROM `RiskCheckRule`", conn).ExecuteScalar();
			long distinct = (long)new SQLiteCommand("SELECT COUNT(DISTINCT `id`) FROM `RiskCheckRule`", conn).ExecuteScalar();
			Console.WriteLine();
			Console.WriteLine($"全库规则行数={total}，不同 Id 数={distinct}");
			if (total != distinct)
			{
				ok = false;
			}
		}

		Console.WriteLine();
		string verdict = (_failed == 0 && ok) ? "全部通过" : "存在失败";
		Console.WriteLine($"==== {verdict}: passed={_passed}, failed={_failed} ====");
		ComboGuardScenario();
		Console.WriteLine("保留夹具目录: " + dir);
		return (_failed == 0 && ok) ? 0 : 1;
	}

	private static void CreateLedgerFixture(string db)
	{
		using SQLiteConnection conn = new SQLiteConnection("Data Source=" + db);
		conn.Open();
		Exec(conn, "CREATE TABLE `Voucher`(`id` INTEGER PRIMARY KEY, `number` TEXT, `DirectionToggled` INTEGER NOT NULL DEFAULT 0, `VoucherMark` INTEGER NOT NULL DEFAULT 0)");
		Exec(conn, "CREATE TABLE `Ledger`(`startDate` DATE, `EndDate` DATE)");
		Exec(conn, "PRAGMA user_version=4");
	}

	private static void Exec(SQLiteConnection conn, string sql)
	{
		using SQLiteCommand cmd = new SQLiteCommand(sql, conn);
		cmd.ExecuteNonQuery();
	}

	private static void AddRule(string db, RiskCheckScheme scheme, string label)
	{
		// 与 RiskCheckEditor.AddRule 一致：新建规则 Id=0，追加进内存列表，随后整体保存
		scheme.Rules.Add(new RiskCheckRule
		{
			RuleType = RiskCheckRule.RULE_TYPE_CONDITION,
			AccountCodes = "1122",
			ClosingEnabled = true,
			ClosingDirection = RiskCheckRule.DIRECTION_DEBIT,
			ClosingOp = 2,
			ClosingValue = 0m
		});
		Save(db, scheme, label);
	}

	private static void Save(string db, RiskCheckScheme scheme, string label)
	{
		string before = Join(scheme.Rules);
		try
		{
			RiskCheckStore.Save(db, scheme);
			_passed++;
			Console.WriteLine($"[ OK ] {label,-26} schemeId={scheme.Id,-2} 保存前Id=[{before}] -> 保存后Id=[{Join(scheme.Rules)}]");
		}
		catch (Exception ex)
		{
			_failed++;
			Console.WriteLine($"[FAIL] {label,-26} schemeId={scheme.Id,-2} 保存前Id=[{before}] -> {ex.GetType().Name}: {ex.Message}");
		}
	}

	private static string Join(List<RiskCheckRule> rules)
	{
		if (rules == null || rules.Count == 0)
		{
			return "";
		}
		string[] parts = new string[rules.Count];
		for (int i = 0; i < rules.Count; i++)
		{
			parts[i] = rules[i].Id.ToString();
		}
		return string.Join(",", parts);
	}

	/// <summary>
	/// 组合期初写保护验证：直接调用 LedgerDAL.WriteComboOpeningBalances（private static，经反射），
	/// 对照 ComboOpeningBalancesLoadFailed = true / false 两种情形下 ItemComboBalance 是否被清空。
	/// 期望：true（读取失败）→ 库中原值保留；false（正常）→ 按内存重写（此处内存为空，故清空）。
	/// </summary>
	private static void ComboGuardScenario()
	{
		Console.WriteLine();
		Console.WriteLine("--- 组合期初写保护 ---");

		Console.Error.WriteLine("[combo] 解析 LedgerDAL 类型");
		Type dalType = typeof(Ledger).Assembly.GetType("Auditai.Model.LedgerDAL");
		Console.Error.WriteLine("[combo] dalType=" + (dalType != null));
		MethodInfo write = dalType.GetMethod("WriteComboOpeningBalances",
			BindingFlags.Static | BindingFlags.NonPublic);
		if (write == null)
		{
			_failed++;
			Console.WriteLine("[FAIL] 找不到 LedgerDAL.WriteComboOpeningBalances");
			return;
		}

		foreach (bool loadFailed in new[] { true, false })
		{
			Console.Error.WriteLine("[combo] 迭代 loadFailed=" + loadFailed);
			string dir = Path.Combine(Path.GetTempPath(), "combo-verify-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(dir);
			string db = Path.Combine(dir, "ledger.s3db");
			Console.Error.WriteLine("[combo] 建目录完成: " + db);
			using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + db))
			{
				Console.Error.WriteLine("[combo] 连接已构造");
				conn.Open();
				Console.Error.WriteLine("[combo] 连接已打开");
				Console.Error.WriteLine("[combo] 建 ItemComboBalance");
				Exec(conn, "CREATE TABLE `ItemComboBalance`(`id` INTEGER PRIMARY KEY, `accountId` INTEGER NOT NULL, `balance` MONEY NOT NULL DEFAULT 0)");
				Exec(conn, "CREATE TABLE `ItemComboBalanceRel`(`comboId` INTEGER NOT NULL, `itemId` INTEGER NOT NULL)");
				Exec(conn, "INSERT INTO `ItemComboBalance`(`id`,`accountId`,`balance`) VALUES(0,7,123.45)");
				Console.Error.WriteLine("[combo] 插 seed 数据 Rel");
				Exec(conn, "INSERT INTO `ItemComboBalanceRel`(`comboId`,`itemId`) VALUES(0,9)");

				Console.Error.WriteLine("[combo] 构造 Ledger（不跑 ctor）");
				// 不用 new/Activator：Ledger 实例构造会连带初始化一批模型对象，在最小夹具下会崩。
				// 未初始化对象即可满足本次调用（只读 Flag + 遍历 ComboOpeningBalances）。
				object ledger = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Ledger));
				Console.Error.WriteLine("[combo] 设置 Flag");
				typeof(Ledger).GetProperty("ComboOpeningBalancesLoadFailed",
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					.SetValue(ledger, loadFailed);
				Console.Error.WriteLine("[combo] 设置 ComboOpeningBalances 后备字段");
				FieldInfo backing = typeof(Ledger).GetField("<ComboOpeningBalances>k__BackingField",
					BindingFlags.Instance | BindingFlags.NonPublic);
				if (backing == null)
				{
					_failed++;
					Console.WriteLine("[FAIL] 找不到 ComboOpeningBalances 的后备字段");
					return;
				}
				backing.SetValue(ledger, new List<ComboOpeningBalance>());
				Console.Error.WriteLine("[combo] Ledger 就绪");

				using (SQLiteTransaction tx = conn.BeginTransaction())
				{
					Console.Error.WriteLine("[combo] 调用 WriteComboOpeningBalances");
					write.Invoke(null, new object[]
					{
						conn, tx, ledger,
						new Dictionary<Account, int>(),
						new Dictionary<AuxiliaryItem, int>()
					});
					tx.Commit();
				}

				long remain = (long)new SQLiteCommand("SELECT COUNT(*) FROM `ItemComboBalance`", conn).ExecuteScalar();
				bool expectPreserved = loadFailed;
				bool ok = expectPreserved ? (remain == 1) : (remain == 0);
				Console.WriteLine($"[{(ok ? " OK " : "FAIL")}] LoadFailed={loadFailed,-5} → ItemComboBalance 剩余 {remain} 行" +
					(expectPreserved ? "（期望保留 1 行 = 跳过重写）" : "（期望 0 行 = 正常全删全插）"));
				if (!ok) _failed++;
			}
			try { Directory.Delete(dir, true); } catch { }
		}
	}
}
