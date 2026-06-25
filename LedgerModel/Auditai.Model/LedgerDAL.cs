using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Dapper;

namespace Auditai.Model;

internal class LedgerDAL
{
	protected class AccountBalanceData
	{
		public bool IsDebit = true;

		public decimal OldBalance;

		public decimal NewBalance;
	}

	private SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder();

	internal LedgerDAL(string fileName)
	{
		connectionStringBuilder.DataSource = fileName;
		UpdateSchema();
	}

	private SQLiteConnection GetConnection()
	{
		return new SQLiteConnection(connectionStringBuilder.ConnectionString).OpenAndReturn();
	}

	public List<Tuple<int, string, string>> GetTableData_ItemClass()
	{
		using SQLiteConnection cnn = GetConnection();
		IEnumerable<Tuple<int, string, string>> source = from row in cnn.Query("SELECT `id`,`code`,`name` FROM `ItemClass` ORDER BY `id`")
			select Tuple.Create((int)row.id, (string)row.code, (string)row.name);
		return source.ToList();
	}

	public void UpdateTableData_ItemClassIdAndCode(List<Tuple<int, int, string>> dataList)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		try
		{
			foreach (Tuple<int, int, string> data in dataList)
			{
				int item = data.Item1;
				int item2 = data.Item2;
				string item3 = data.Item3;
				sQLiteConnection.Execute("UPDATE `ItemClass` set `id`=@newId, `code`=@newCode WHERE `id`=@oldId", new
				{
					oldId = item,
					newId = item2,
					newCode = item3
				}, sQLiteTransaction);
				sQLiteConnection.Execute("UPDATE `Item` set `classId`=@newId WHERE `classId`=@oldId", new
				{
					oldId = item,
					newId = item2
				}, sQLiteTransaction);
			}
			sQLiteTransaction.Commit();
		}
		catch (Exception)
		{
			try
			{
				sQLiteTransaction.Rollback();
			}
			catch
			{
			}
			throw;
		}
	}

	public void UpdateTableData_ItemClassId(List<Tuple<int, int>> dataList)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		try
		{
			foreach (Tuple<int, int> data in dataList)
			{
				int item = data.Item1;
				int item2 = data.Item2;
				sQLiteConnection.Execute("UPDATE `ItemClass` set `id`=@newId WHERE `id`=@oldId", new
				{
					oldId = item,
					newId = item2
				}, sQLiteTransaction);
				sQLiteConnection.Execute("UPDATE `Item` set `classId`=@newId WHERE `classId`=@oldId", new
				{
					oldId = item,
					newId = item2
				}, sQLiteTransaction);
			}
			sQLiteTransaction.Commit();
		}
		catch (Exception)
		{
			try
			{
				sQLiteTransaction.Rollback();
			}
			catch
			{
			}
			throw;
		}
	}

	public Ledger GetLedger()
	{
		using SQLiteConnection cnn = GetConnection();
		Ledger ret = cnn.QueryFirst<Ledger>("SELECT `companyName`,`startDate`,`EndDate` FROM `Ledger`");
		var enumerable = from row in cnn.Query("SELECT `id`,`parentId`,`code`,`name`,`dc`,`balance` FROM `Account` ORDER BY `id`")
			select new
			{
				id = (int)row.id,
				parentId = (int)row.parentId,
				code = (string)row.code,
				name = (string)row.name,
				dc = Convert.ToBoolean(row.dc),
				balance = (decimal)row.balance
			};
		Dictionary<int, Account> dicAcc = new Dictionary<int, Account>();
		ret.Accounts.AddRange(enumerable.Select(account =>
		{
			Account account2 = new Account(ret)
			{
				Id = account.id,
				Code = account.code,
				Name = account.name,
				IsDebit = account.dc
			};
			dicAcc.Add(account.id, account2);
			return account2;
		}));
		foreach (var item in enumerable)
		{
			ret.InitialBalance.Add(dicAcc[item.id], new AccountBalance
			{
				Total = item.balance
			});
			if (item.parentId != -1)
			{
				dicAcc[item.parentId].Children.Add(dicAcc[item.id]);
				dicAcc[item.id].Parent = dicAcc[item.parentId];
			}
		}
		ret.AuxiliaryClasses.AddRange(cnn.Query<AuxiliaryClass>("SELECT `code`,`name` FROM `ItemClass` ORDER BY `id`"));
		ret.AuxiliaryItems.AddRange(from row in cnn.Query("SELECT `classId`,`code`,`name` FROM `Item` ORDER BY `id`")
			select new AuxiliaryItem
			{
				Class = ret.AuxiliaryClasses[(int)row.classId],
				Code = (string)row.code,
				Name = (string)row.name
			});
		foreach (AuxiliaryItem auxiliaryItem2 in ret.AuxiliaryItems)
		{
			auxiliaryItem2.Class.Items.Add(auxiliaryItem2);
		}
		ret.VoucherTypes.AddRange(cnn.Query<VoucherType>("SELECT `name` FROM `VoucherType` ORDER BY `id`"));
		ret.Currencies.AddRange(cnn.Query<Currency>("SELECT `name` FROM `ForeignCurrency` ORDER BY `id`"));
		Dictionary<int, Voucher> dictionary = new Dictionary<int, Voucher>();
		IEnumerable<object> enumerable2 = cnn.Query("SELECT `id`,`type`,`number`,`day`,`digest`,`dc`,`amount`,`quantity`,`unitPrice`,`accountId`,`foreignId`,`foreignAmount`,`exchangeRate`,`maker`,`checker`,`booker`,`OppositeAccounts`,`DirectionToggled`,`VoucherMark` FROM `Voucher` ORDER BY `id`");
		Dictionary<string, Account> tempAccountMap = ret.Accounts.ToDictionary((Account a) => a.Code, (Account a) => a);
		foreach (dynamic item2 in enumerable2)
		{
			Voucher voucher = new Voucher();
			voucher.Id = (int)item2.id;
			voucher.Type = ret.VoucherTypes[(int)item2.type];
			voucher.Number = (string)item2.number;
			voucher.Day = (DateTime)item2.day;
			voucher.Digest = (string)item2.digest;
			voucher.IsDebit = Convert.ToBoolean(item2.dc);
			voucher.Amount = (decimal)item2.amount;
			voucher.Quantity = (double)item2.quantity;
			voucher.UnitPrice = ((double)item2.unitPrice).ToDecimalSafe();
			voucher.Account = dicAcc[(int)item2.accountId];
			voucher.Currency = ret.Currencies[(int)item2.foreignId];
			voucher.ForeignAmount = (decimal)item2.foreignAmount;
			voucher.ExchangeRate = (double)item2.exchangeRate;
			voucher.Maker = (string)item2.maker;
			voucher.Checker = (string)item2.checker;
			voucher.Booker = (string)item2.booker;
			voucher.DirectionToggled = item2.DirectionToggled != 0;
			voucher.VoucherMark = item2.VoucherMark != 0;
			Voucher voucher2 = voucher;
			string text = (string)item2.OppositeAccounts;
			if (!string.IsNullOrWhiteSpace(text))
			{
				voucher2.OppositeAccounts.AddRange(from s in text.Split(',')
					select (!tempAccountMap.ContainsKey(s)) ? null : tempAccountMap[s]);
			}
			ret.Vouchers.Add(voucher2);
			dictionary.Add(voucher2.Id, voucher2);
		}
		foreach (dynamic item3 in cnn.Query("SELECT voucherId,itemId FROM VoucherItemRel"))
		{
			dictionary[(int)item3.voucherId].Details.Add(ret.AuxiliaryItems[(int)item3.itemId]);
		}
		var enumerable3 = from row in cnn.Query("SELECT accountId,itemId,balance FROM ItemBalance")
			select new
			{
				AccountId = (int)row.accountId,
				ItemId = (int)row.itemId,
				Balance = (decimal)row.balance
			};
		foreach (var item4 in enumerable3)
		{
			Account key = dicAcc[item4.AccountId];
			AuxiliaryItem auxiliaryItem = ret.AuxiliaryItems[item4.ItemId];
			AccountBalance accountBalance = ret.InitialBalance[key];
			if (!accountBalance.ClassBalances.TryGetValue(auxiliaryItem.Class, out var value))
			{
				value = new ClassBalance();
				accountBalance.ClassBalances.Add(auxiliaryItem.Class, value);
			}
			value.Total += item4.Balance;
			value.ItemBalances.Add(auxiliaryItem, item4.Balance);
		}
		return ret;
	}

	public void SaveLedgerIncremental(Ledger ledger)
	{
		using (SQLiteConnection sQLiteConnection = GetConnection())
		{
			using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
			Dictionary<Account, int> dicAcc = ledger.Accounts.ToDictionary((Account a) => a, (Account a) => a.Id);
			var source = from a in dicAcc
				where a.Key.Dirty != 0
				select a into kv
				select new
				{
					id = kv.Value,
					parentId = ((kv.Key.Parent == null) ? (-1) : dicAcc[kv.Key.Parent]),
					code = kv.Key.Code,
					name = kv.Key.Name,
					dc = kv.Key.IsDebit,
					balance = (ledger.InitialBalance.ContainsKey(kv.Key) ? ledger.InitialBalance[kv.Key].Total : 0m),
					Dirty = kv.Key.Dirty
				};
			sQLiteConnection.Execute("DELETE FROM `Account` WHERE id=@id", source.Where(a => a.Dirty == -1), sQLiteTransaction);
			sQLiteConnection.Execute("UPDATE `Account` SET `id`=@id,`parentId`=@parentId,`code`=@code,`name`=@name,`dc`=@dc,`balance`=@balance WHERE id=@id", source.Where(a => a.Dirty == 2), sQLiteTransaction);
			sQLiteConnection.Execute("INSERT INTO `Account`(`id`,`parentId`,`code`,`name`,`dc`,`balance`) VALUES(@id,@parentId,@code,@name,@dc,@balance)", source.Where(a => a.Dirty == 1), sQLiteTransaction);
			sQLiteConnection.Execute("DELETE FROM `AccountRel`");
			var param = from kv in dicAcc
				from desc in kv.Key.DescendantsAndSelf
				select new
				{
					anc = kv.Key,
					desc = desc
				} into pair
				select new
				{
					ancestorId = dicAcc[pair.anc],
					descendantId = dicAcc[pair.desc]
				};
			sQLiteConnection.Execute("INSERT INTO `AccountRel` (`ancestorId`,`descendantId`) VALUES(@ancestorId,@descendantId)", param, sQLiteTransaction);
			sQLiteConnection.Execute("DELETE FROM `ForeignCurrency`");
			Dictionary<Currency, int> currDic = ledger.Currencies.Select((Currency curr, int i) => new { curr, i }).ToDictionary(ci => ci.curr, ci => ci.i);
			var param2 = currDic.Select((KeyValuePair<Currency, int> kv) => new
			{
				id = kv.Value,
				name = kv.Key.Name
			});
			sQLiteConnection.Execute("INSERT INTO `ForeignCurrency` (`id`,`name`) VALUES(@id,@name)", param2, sQLiteTransaction);
			sQLiteConnection.Execute("DELETE FROM `ItemClass`");
			Dictionary<AuxiliaryClass, int> classDic = ledger.AuxiliaryClasses.Select((AuxiliaryClass cla, int i) => new { cla, i }).ToDictionary(ci => ci.cla, ci => ci.i);
			var param3 = classDic.Select((KeyValuePair<AuxiliaryClass, int> kv) => new
			{
				id = kv.Value,
				code = kv.Key.Code,
				name = kv.Key.Name
			});
			sQLiteConnection.Execute("INSERT INTO `ItemClass` (`id`,`code`,`name`) VALUES(@id,@code,@name)", param3, sQLiteTransaction);
			sQLiteConnection.Execute("DELETE FROM `Item`");
			Dictionary<AuxiliaryItem, int> itemDic = ledger.AuxiliaryItems.Select((AuxiliaryItem it, int i) => new { it, i }).ToDictionary(ii => ii.it, ii => ii.i);
			var param4 = itemDic.Select((KeyValuePair<AuxiliaryItem, int> kv) => new
			{
				id = kv.Value,
				classId = classDic[kv.Key.Class],
				code = kv.Key.Code,
				name = kv.Key.Name
			});
			sQLiteConnection.Execute("INSERT INTO `Item`(`id`,`classId`,`code`,`name`) VALUES(@id,@classId,@code,@name)", param4, sQLiteTransaction);
			sQLiteConnection.Execute("DELETE FROM `ItemBalance`");
			List<Tuple<int, int, decimal>> list = new List<Tuple<int, int, decimal>>();
			foreach (KeyValuePair<Account, AccountBalance> item in ledger.InitialBalance)
			{
				foreach (KeyValuePair<AuxiliaryClass, ClassBalance> classBalance in item.Value.ClassBalances)
				{
					foreach (KeyValuePair<AuxiliaryItem, decimal> itemBalance in classBalance.Value.ItemBalances)
					{
						list.Add(Tuple.Create(dicAcc[item.Key], itemDic[itemBalance.Key], itemBalance.Value));
					}
				}
			}
			ledger.InitialBalance.SelectMany((KeyValuePair<Account, AccountBalance> kv) => kv.Value.ClassBalances.SelectMany((KeyValuePair<AuxiliaryClass, ClassBalance> kv1) => kv1.Value.ItemBalances));
			sQLiteConnection.Execute("INSERT INTO `ItemBalance`(`accountId`,`itemId`,`balance`) VALUES(@Item1,@Item2,@Item3)", list, sQLiteTransaction);
			sQLiteConnection.Execute("DELETE FROM `Ledger`");
			var param5 = new
			{
				companyName = ledger.CompanyName,
				startDate = ledger.StartDate,
				endDate = ledger.EndDate
			};
			sQLiteConnection.Execute("INSERT INTO `Ledger`(`companyName`,`startDate`,`endDate`) VALUES(@companyName,@startDate,@endDate)", param5, sQLiteTransaction);
			sQLiteConnection.Execute("DELETE FROM `VoucherType`");
			Dictionary<VoucherType, int> vtDic = ledger.VoucherTypes.Select((VoucherType vt, int i) => new { vt, i }).ToDictionary(vti => vti.vt, vti => vti.i);
			var param6 = vtDic.Select((KeyValuePair<VoucherType, int> kv) => new
			{
				id = kv.Value,
				name = kv.Key.Name
			});
			sQLiteConnection.Execute("INSERT INTO `VoucherType`(`id`,`name`) VALUES(@id,@name)", param6, sQLiteTransaction);
			var source2 = from v in ledger.Vouchers
				where v.Dirty != 0
				select new
				{
					id = v.Id,
					number = v.Number,
					type = vtDic[v.Type],
					day = v.Day,
					digest = v.Digest,
					dc = v.IsDebit,
					amount = v.Amount,
					quantity = v.Quantity,
					unitPrice = v.UnitPrice,
					accountId = dicAcc[v.Account],
					foreignId = currDic[v.Currency],
					foreignAmount = v.ForeignAmount,
					exchangeRate = v.ExchangeRate,
					maker = v.Maker,
					checker = v.Checker,
					booker = v.Booker,
					OppositeAccounts = string.Join(",", v.OppositeAccounts.Select((Account a) => a.Code)),
					DirectionToggled = v.DirectionToggled,
					VoucherMark = v.VoucherMark,
					Dirty = v.Dirty
				};
			sQLiteConnection.Execute("DELETE FROM `Voucher` WHERE id=@id", source2.Where(v => v.Dirty == -1), sQLiteTransaction);
			sQLiteConnection.Execute("UPDATE `Voucher` SET `id`=@id,`number`=@number,`type`=@type,`day`=@day,`digest`=@digest,`dc`=@dc,`amount`=@amount,`quantity`=@quantity,`unitPrice`=@unitPrice,`accountId`=@accountId,`foreignId`=@foreignId,`foreignAmount`=@foreignAmount,`exchangeRate`=@exchangeRate,`maker`=@maker,`checker`=@checker,`booker`=@booker,`OppositeAccounts`=@OppositeAccounts,`DirectionToggled`=@DirectionToggled,`VoucherMark`=@VoucherMark WHERE id=@id", source2.Where(v => v.Dirty == 2), sQLiteTransaction);
			sQLiteConnection.Execute("INSERT INTO `Voucher`(`id`,`number`,`type`,`day`,`digest`,`dc`,`amount`,`quantity`,`unitPrice`,`accountId`,`foreignId`,`foreignAmount`,`exchangeRate`,`maker`,`checker`,`booker`,`OppositeAccounts`,`DirectionToggled`,`VoucherMark`) VALUES(@id,@number,@type,@day,@digest,@dc,@amount,@quantity,@unitPrice,@accountId,@foreignId,@foreignAmount,@exchangeRate,@maker,@checker,@booker,@OppositeAccounts,@DirectionToggled,@VoucherMark)", source2.Where(v => v.Dirty == 1), sQLiteTransaction);
			sQLiteConnection.Execute("DELETE FROM `VoucherItemRel`");
			foreach (Voucher voucher in ledger.Vouchers.Where((Voucher v) => v.Dirty != -1))
			{
				var param7 = voucher.Details.Select((AuxiliaryItem d) => new
				{
					voucherId = voucher.Id,
					itemId = itemDic[d]
				});
				sQLiteConnection.Execute("INSERT INTO `VoucherItemRel`(`voucherId`,`itemId`) VALUES(@voucherId,@itemId)", param7, sQLiteTransaction);
			}
			sQLiteTransaction.Commit();
		}
		foreach (Account account in ledger.Accounts)
		{
			account.Dirty = 0;
		}
		foreach (Voucher voucher2 in ledger.Vouchers)
		{
			voucher2.Dirty = 0;
		}
	}

	public void SaveLedgerTotal(Ledger ledger)
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		using SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
		sQLiteConnection.Execute("DELETE FROM `Account`");
		Dictionary<Account, int> dicAcc = ledger.Accounts.OrderBy((Account a) => a.Code).Select((Account a, int i) => new { a, i }).ToDictionary(tup => tup.a, tup => tup.i);
		var param = dicAcc.Select((KeyValuePair<Account, int> kv) => new
		{
			id = kv.Value,
			parentId = ((kv.Key.Parent == null) ? (-1) : dicAcc[kv.Key.Parent]),
			code = kv.Key.Code,
			name = kv.Key.Name,
			dc = kv.Key.IsDebit,
			balance = (ledger.InitialBalance.ContainsKey(kv.Key) ? ledger.InitialBalance[kv.Key].Total : 0m)
		});
		sQLiteConnection.Execute("INSERT INTO `Account`(`id`,`parentId`,`code`,`name`,`dc`,`balance`) VALUES(@id,@parentId,@code,@name,@dc,@balance)", param, sQLiteTransaction);
		sQLiteConnection.Execute("DELETE FROM `AccountRel`");
		var param2 = from kv in dicAcc
			from desc in kv.Key.DescendantsAndSelf
			select new
			{
				anc = kv.Key,
				desc = desc
			} into pair
			select new
			{
				ancestorId = dicAcc[pair.anc],
				descendantId = dicAcc[pair.desc]
			};
		sQLiteConnection.Execute("INSERT INTO `AccountRel` (`ancestorId`,`descendantId`) VALUES(@ancestorId,@descendantId)", param2, sQLiteTransaction);
		sQLiteConnection.Execute("DELETE FROM `ForeignCurrency`");
		Dictionary<Currency, int> currDic = ledger.Currencies.Select((Currency curr, int i) => new { curr, i }).ToDictionary(ci => ci.curr, ci => ci.i);
		var param3 = currDic.Select((KeyValuePair<Currency, int> kv) => new
		{
			id = kv.Value,
			name = kv.Key.Name
		});
		sQLiteConnection.Execute("INSERT INTO `ForeignCurrency` (`id`,`name`) VALUES(@id,@name)", param3, sQLiteTransaction);
		sQLiteConnection.Execute("DELETE FROM `ItemClass`");
		Dictionary<AuxiliaryClass, int> classDic = ledger.AuxiliaryClasses.Select((AuxiliaryClass cla, int i) => new { cla, i }).ToDictionary(ci => ci.cla, ci => ci.i);
		var param4 = classDic.Select((KeyValuePair<AuxiliaryClass, int> kv) => new
		{
			id = kv.Value,
			code = kv.Key.Code,
			name = kv.Key.Name
		});
		sQLiteConnection.Execute("INSERT INTO `ItemClass` (`id`,`code`,`name`) VALUES(@id,@code,@name)", param4, sQLiteTransaction);
		sQLiteConnection.Execute("DELETE FROM `Item`");
		Dictionary<AuxiliaryItem, int> itemDic = ledger.AuxiliaryItems.Select((AuxiliaryItem it, int i) => new { it, i }).ToDictionary(ii => ii.it, ii => ii.i);
		var param5 = itemDic.Select((KeyValuePair<AuxiliaryItem, int> kv) => new
		{
			id = kv.Value,
			classId = classDic[kv.Key.Class],
			code = kv.Key.Code,
			name = kv.Key.Name
		});
		sQLiteConnection.Execute("INSERT INTO `Item`(`id`,`classId`,`code`,`name`) VALUES(@id,@classId,@code,@name)", param5, sQLiteTransaction);
		sQLiteConnection.Execute("DELETE FROM `ItemBalance`");
		List<Tuple<int, int, decimal>> list = new List<Tuple<int, int, decimal>>();
		foreach (KeyValuePair<Account, AccountBalance> item in ledger.InitialBalance)
		{
			foreach (KeyValuePair<AuxiliaryClass, ClassBalance> classBalance in item.Value.ClassBalances)
			{
				foreach (KeyValuePair<AuxiliaryItem, decimal> itemBalance in classBalance.Value.ItemBalances)
				{
					list.Add(Tuple.Create(dicAcc[item.Key], itemDic[itemBalance.Key], itemBalance.Value));
				}
			}
		}
		ledger.InitialBalance.SelectMany((KeyValuePair<Account, AccountBalance> kv) => kv.Value.ClassBalances.SelectMany((KeyValuePair<AuxiliaryClass, ClassBalance> kv1) => kv1.Value.ItemBalances));
		sQLiteConnection.Execute("INSERT INTO `ItemBalance`(`accountId`,`itemId`,`balance`) VALUES(@Item1,@Item2,@Item3)", list, sQLiteTransaction);
		sQLiteConnection.Execute("DELETE FROM `Ledger`");
		var param6 = new
		{
			companyName = ledger.CompanyName,
			startDate = ledger.StartDate,
			endDate = ledger.EndDate
		};
		sQLiteConnection.Execute("INSERT INTO `Ledger`(`companyName`,`startDate`,`endDate`) VALUES(@companyName,@startDate,@endDate)", param6, sQLiteTransaction);
		sQLiteConnection.Execute("DELETE FROM `VoucherType`");
		Dictionary<VoucherType, int> vtDic = ledger.VoucherTypes.Select((VoucherType vt, int i) => new { vt, i }).ToDictionary(vti => vti.vt, vti => vti.i);
		var param7 = vtDic.Select((KeyValuePair<VoucherType, int> kv) => new
		{
			id = kv.Value,
			name = kv.Key.Name
		});
		sQLiteConnection.Execute("INSERT INTO `VoucherType`(`id`,`name`) VALUES(@id,@name)", param7, sQLiteTransaction);
		sQLiteConnection.Execute("DELETE FROM `Voucher`");
		var param8 = ledger.Vouchers.Select((Voucher v, int i) => new
		{
			id = i,
			number = v.Number,
			type = vtDic[v.Type],
			day = v.Day,
			digest = v.Digest,
			dc = v.IsDebit,
			amount = v.Amount,
			quantity = v.Quantity,
			unitPrice = v.UnitPrice,
			accountId = dicAcc[v.Account],
			foreignId = currDic[v.Currency],
			foreignAmount = v.ForeignAmount,
			exchangeRate = v.ExchangeRate,
			maker = v.Maker,
			checker = v.Checker,
			booker = v.Booker,
			OppositeAccounts = string.Join(",", v.OppositeAccounts.Select((Account a) => a.Code)),
			DirectionToggled = v.DirectionToggled,
			VoucherMark = v.VoucherMark
		});
		sQLiteConnection.Execute("INSERT INTO `Voucher`(`id`,`number`,`type`,`day`,`digest`,`dc`,`amount`,`quantity`,`unitPrice`,`accountId`,`foreignId`,`foreignAmount`,`exchangeRate`,`maker`,`checker`,`booker`,`OppositeAccounts`,`DirectionToggled`,`VoucherMark`) VALUES(@id,@number,@type,@day,@digest,@dc,@amount,@quantity,@unitPrice,@accountId,@foreignId,@foreignAmount,@exchangeRate,@maker,@checker,@booker,@OppositeAccounts,@DirectionToggled,@VoucherMark)", param8, sQLiteTransaction);
		sQLiteConnection.Execute("DELETE FROM `VoucherItemRel`");
		int j;
		for (j = 0; j < ledger.Vouchers.Count; j++)
		{
			Voucher voucher = ledger.Vouchers[j];
			var param9 = voucher.Details.Select((AuxiliaryItem d) => new
			{
				voucherId = j,
				itemId = itemDic[d]
			});
			sQLiteConnection.Execute("INSERT INTO `VoucherItemRel`(`voucherId`,`itemId`) VALUES(@voucherId,@itemId)", param9, sQLiteTransaction);
		}
		sQLiteTransaction.Commit();
	}

	private void UpdateSchema()
	{
		using SQLiteConnection sQLiteConnection = GetConnection();
		Update_Voucher_Number(sQLiteConnection);
		int num = sQLiteConnection.ExecuteScalar<int>("PRAGMA user_version;");
		if (num == 0)
		{
			num = 1;
			Update_0_1(sQLiteConnection);
		}
		if (num == 1)
		{
			num = 2;
			sQLiteConnection.Execute("ALTER TABLE `Ledger` ADD COLUMN `EndDate` DATE");
			sQLiteConnection.Execute("UPDATE `Ledger` SET `EndDate`=@endDate", new
			{
				endDate = new DateTime(sQLiteConnection.QueryFirst<DateTime>("SELECT `startDate` FROM `Ledger`").Year, 12, 31)
			});
		}
		if (num == 2)
		{
			num = 3;
			sQLiteConnection.Execute("ALTER TABLE `Voucher` ADD COLUMN `DirectionToggled` INTEGER NOT NULL DEFAULT 0");
			sQLiteConnection.Execute("ALTER TABLE `Voucher` ADD COLUMN `VoucherMark` INTEGER NOT NULL DEFAULT 0");
		}
		if (num == 3)
		{
			Update_AccountBalanceValue(sQLiteConnection);
			num = 4;
		}
		sQLiteConnection.Execute($"PRAGMA user_version={num}");
	}

	private void Update_Voucher_Number(SQLiteConnection c)
	{
		using SQLiteTransaction sQLiteTransaction = c.BeginTransaction();
		var source = (from row in c.Query("pragma table_info(`voucher`)", null, sQLiteTransaction)
			select new
			{
				Name = (string)row.name,
				Type = (string)row.type
			}).ToList();
		if (source.First(row => row.Name.Equals("number", StringComparison.OrdinalIgnoreCase)).Type.Equals("integer", StringComparison.OrdinalIgnoreCase))
		{
			c.Execute("alter table `Voucher` add column `number1` text", null, sQLiteTransaction);
			c.Execute("update `Voucher` set `number1`=`number`", null, sQLiteTransaction);
			c.Execute("drop index `idx_v_number`", null, sQLiteTransaction);
			c.Execute("alter table `Voucher` drop column `number`", null, sQLiteTransaction);
			c.Execute("alter table `Voucher` rename column `number1` to `number`", null, sQLiteTransaction);
			c.Execute("create index `idx_v_number` on `Voucher`(`number`)", null, sQLiteTransaction);
		}
		sQLiteTransaction.Commit();
	}

	private void Update_0_1(SQLiteConnection c)
	{
		using (SQLiteTransaction sQLiteTransaction = c.BeginTransaction())
		{
			c.Execute("ALTER TABLE `voucher` ADD COLUMN `OppositeAccounts` TEXT DEFAULT ''", null, sQLiteTransaction);
			var enumerable = from row in c.Query("SELECT id,parentId,code,name,dc,balance FROM Account ORDER BY `id`")
				select new
				{
					id = (int)row.id,
					parentId = (int)row.parentId,
					code = (string)row.code,
					name = (string)row.name,
					dc = Convert.ToBoolean(row.dc),
					balance = (decimal)row.balance
				};
			Dictionary<int, Account> accountDic = enumerable.ToDictionary(a => a.id, a => new Account
			{
				Code = a.code,
				Name = a.name,
				IsDebit = a.dc
			});
			foreach (var item in enumerable)
			{
				if (item.parentId != -1)
				{
					accountDic[item.parentId].Children.Add(accountDic[item.id]);
					accountDic[item.id].Parent = accountDic[item.parentId];
				}
			}
			List<VoucherType> voucherTypes = c.Query<VoucherType>("SELECT name FROM VoucherType ORDER BY id").ToList();
			List<Voucher> source = (from row in c.Query("SELECT id,type,number,day,dc,amount,accountId FROM Voucher ORDER BY id")
				select new Voucher
				{
					Id = (int)row.id,
					Type = voucherTypes[(int)row.type],
					Number = (string)row.number,
					Day = (DateTime)row.day,
					IsDebit = Convert.ToBoolean(row.dc),
					Amount = (decimal)row.amount,
					Account = accountDic[(int)row.accountId]
				}).ToList();
			IEnumerable<IGrouping<Tuple<string, string, DateTime>, Voucher>> enumerable2 = from v in source
				group v by Tuple.Create(v.Type.Name, v.Number, v.Day);
			Dictionary<int, string> dictionary = new Dictionary<int, string>();
			Dictionary<Account, string> accountParentCodeMap = accountDic.Values.ToDictionary((Account a) => a, (Account a) => topParent(a).Code);
			foreach (IGrouping<Tuple<string, string, DateTime>, Voucher> item2 in enumerable2)
			{
				if (item2.Count() == 1)
				{
					dictionary.Add(item2.First().Id, string.Empty);
					continue;
				}
				List<Voucher> list = new List<Voucher>();
				List<Voucher> list2 = new List<Voucher>();
				bool flag = false;
				bool flag2 = IsDebit(item2.First());
				foreach (Voucher item3 in item2)
				{
					if (IsDebit(item3) == flag2)
					{
						if (flag)
						{
							if (list.Sum((Voucher v) => v.Amount) == list2.Sum((Voucher v) => v.Amount))
							{
								string value = string.Join(",", list2.Select((Voucher v) => accountParentCodeMap[v.Account]).Distinct());
								string value2 = string.Join(",", list.Select((Voucher v) => accountParentCodeMap[v.Account]).Distinct());
								foreach (Voucher item4 in list)
								{
									dictionary.Add(item4.Id, value);
								}
								foreach (Voucher item5 in list2)
								{
									dictionary.Add(item5.Id, value2);
								}
								list = new List<Voucher>();
								list2 = new List<Voucher>();
								list.Add(item3);
								flag = false;
							}
							else
							{
								list.Add(item3);
								flag = false;
							}
						}
						else
						{
							list.Add(item3);
						}
					}
					else
					{
						flag = true;
						list2.Add(item3);
					}
				}
				if (list.Count <= 0)
				{
					continue;
				}
				string value3 = string.Join(",", list2.Select((Voucher v) => accountParentCodeMap[v.Account]).Distinct());
				string value4 = string.Join(",", list.Select((Voucher v) => accountParentCodeMap[v.Account]).Distinct());
				foreach (Voucher item6 in list)
				{
					dictionary.Add(item6.Id, value3);
				}
				foreach (Voucher item7 in list2)
				{
					dictionary.Add(item7.Id, value4);
				}
			}
			var param = dictionary.Select((KeyValuePair<int, string> kv) => new { kv.Key, kv.Value });
			c.Execute("UPDATE `voucher` SET `OppositeAccounts` =@Value WHERE `id`=@Key", param, sQLiteTransaction);
			sQLiteTransaction.Commit();
		}
		static bool IsDebit(Voucher cv)
		{
			if (!cv.IsDebit || !(cv.Amount >= 0m))
			{
				if (!cv.IsDebit)
				{
					return cv.Amount < 0m;
				}
				return false;
			}
			return true;
		}
		static Account topParent(Account ac)
		{
			while (ac.Parent != null)
			{
				ac = ac.Parent;
			}
			return ac;
		}
	}

	private void Update_AccountBalanceValue(SQLiteConnection connect)
	{
		var enumerable = from row in connect.Query("SELECT id,parentId,code,name,dc,balance FROM Account ORDER BY `id`")
			select new
			{
				id = (int)row.id,
				parentId = (int)row.parentId,
				code = (string)row.code,
				name = (string)row.name,
				dc = Convert.ToBoolean(row.dc),
				balance = (decimal)row.balance
			};
		Dictionary<int, Account> dictionary = enumerable.ToDictionary(a => a.id, a => new Account
		{
			Id = a.id,
			Code = a.code,
			IsDebit = a.dc
		});
		Dictionary<int, AccountBalanceData> accountBalanceDic = enumerable.ToDictionary(a => a.id, a => new AccountBalanceData
		{
			IsDebit = true,
			OldBalance = ((a.dc == true) ? a.balance : (-a.balance))
		});
		List<Account> list = new List<Account>();
		foreach (var item in enumerable)
		{
			if (item.parentId != -1)
			{
				dictionary[item.parentId].Children.Add(dictionary[item.id]);
				dictionary[item.id].Parent = dictionary[item.parentId];
			}
			else
			{
				list.Add(dictionary[item.id]);
			}
		}
		foreach (Account item2 in list)
		{
			UpdateUnLeafAccountBalanceValue(item2);
		}
		using (SQLiteTransaction sQLiteTransaction = connect.BeginTransaction())
		{
			foreach (int key in accountBalanceDic.Keys)
			{
				AccountBalanceData accountBalanceData = accountBalanceDic[key];
				if (!(accountBalanceData.NewBalance == accountBalanceData.OldBalance))
				{
					int accountDc = 1;
					decimal num = accountBalanceData.NewBalance;
					if (num < 0m)
					{
						num = -num;
						accountDc = 0;
					}
					connect.Execute("UPDATE `Account` SET `dc` =@accountDc, `balance`=@accountBalance WHERE `id`=@accountId", new
					{
						accountDc = accountDc,
						accountBalance = num,
						accountId = key
					}, sQLiteTransaction);
				}
			}
			sQLiteTransaction.Commit();
		}
		void UpdateUnLeafAccountBalanceValue(Account updateTarget)
		{
			if (updateTarget.Children.Count == 0)
			{
				AccountBalanceData accountBalanceData2 = accountBalanceDic[updateTarget.Id];
				accountBalanceData2.NewBalance = accountBalanceData2.OldBalance;
			}
			else
			{
				decimal newBalance = default(decimal);
				foreach (Account child in updateTarget.Children)
				{
					UpdateUnLeafAccountBalanceValue(child);
					decimal newBalance2 = accountBalanceDic[child.Id].NewBalance;
					newBalance += newBalance2;
				}
				accountBalanceDic[updateTarget.Id].NewBalance = newBalance;
			}
		}
	}

	public static void Main(string[] args)
	{
		LedgerDAL ledgerDAL = new LedgerDAL("C:\\Users\\Mr.Li\\Desktop\\北京A有限公司_2016-2017.db");
		using (SQLiteConnection c = ledgerDAL.GetConnection())
		{
			ledgerDAL.Update_0_1(c);
		}
	}
}
