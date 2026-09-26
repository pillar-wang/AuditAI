using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Dapper;
using Newtonsoft.Json;

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
		// 修复：原实现用列表索引当作外键 id（如 AuxiliaryClasses[(int)row.classId]），
		// 而 DB id 通常从 1 开始、列表索引从 0 开始，会取错实体或越界。
		// 改为按 id 建立字典，并用 TryGetValue 兜底脏数据。
		Dictionary<int, AuxiliaryClass> classMap = new Dictionary<int, AuxiliaryClass>();
		foreach (dynamic row in cnn.Query("SELECT `id`,`code`,`name` FROM `ItemClass` ORDER BY `id`"))
		{
			AuxiliaryClass auxiliaryClass = new AuxiliaryClass
			{
				Code = (string)row.code,
				Name = (string)row.name
			};
			ret.AuxiliaryClasses.Add(auxiliaryClass);
			classMap[(int)row.id] = auxiliaryClass;
		}
		Dictionary<int, AuxiliaryItem> itemMap = new Dictionary<int, AuxiliaryItem>();
		foreach (dynamic row in cnn.Query("SELECT `id`,`classId`,`code`,`name` FROM `Item` ORDER BY `id`"))
		{
			AuxiliaryItem auxiliaryItem = new AuxiliaryItem
			{
				Class = classMap.TryGetValue((int)row.classId, out var cls) ? cls : null,
				Code = (string)row.code,
				Name = (string)row.name
			};
			itemMap[(int)row.id] = auxiliaryItem;
			ret.AuxiliaryItems.Add(auxiliaryItem);
		}
		foreach (AuxiliaryItem auxiliaryItem2 in ret.AuxiliaryItems)
		{
			if (auxiliaryItem2.Class != null)
			{
				auxiliaryItem2.Class.Items.Add(auxiliaryItem2);
			}
		}
		Dictionary<int, VoucherType> voucherTypeMap = new Dictionary<int, VoucherType>();
		foreach (dynamic row in cnn.Query("SELECT `id`,`name` FROM `VoucherType` ORDER BY `id`"))
		{
			VoucherType voucherType = new VoucherType
			{
				Name = (string)row.name
			};
			ret.VoucherTypes.Add(voucherType);
			voucherTypeMap[(int)row.id] = voucherType;
		}
		Dictionary<int, Currency> currencyMap = new Dictionary<int, Currency>();
		foreach (dynamic row in cnn.Query("SELECT `id`,`name` FROM `ForeignCurrency` ORDER BY `id`"))
		{
			Currency currency = new Currency
			{
				Name = (string)row.name
			};
			ret.Currencies.Add(currency);
			currencyMap[(int)row.id] = currency;
		}
		Dictionary<int, Voucher> dictionary = new Dictionary<int, Voucher>();
		IEnumerable<object> enumerable2 = cnn.Query("SELECT `id`,`type`,`number`,`day`,`digest`,`dc`,`amount`,`quantity`,`unitPrice`,`accountId`,`foreignId`,`foreignAmount`,`exchangeRate`,`maker`,`checker`,`booker`,`OppositeAccounts`,`DirectionToggled`,`VoucherMark`,`VoucherMarkSource` FROM `Voucher` ORDER BY `id`");
		Dictionary<string, Account> tempAccountMap = ret.Accounts.ToDictionary((Account a) => a.Code, (Account a) => a);
		foreach (dynamic item2 in enumerable2)
		{
			Voucher voucher = new Voucher();
			voucher.Id = (int)item2.id;
			voucher.Type = voucherTypeMap.TryGetValue((int)item2.type, out var vt) ? vt : null;
			voucher.Number = (string)item2.number;
			voucher.Day = (DateTime)item2.day;
			voucher.Digest = (string)item2.digest;
			voucher.IsDebit = Convert.ToBoolean(item2.dc);
			voucher.Amount = (decimal)item2.amount;
			voucher.Quantity = (double)item2.quantity;
			voucher.UnitPrice = ((double)item2.unitPrice).ToDecimalSafe();
			voucher.Account = dicAcc[(int)item2.accountId];
			voucher.Currency = currencyMap.TryGetValue((int)item2.foreignId, out var cur) ? cur : null;
			voucher.ForeignAmount = (decimal)item2.foreignAmount;
			voucher.ExchangeRate = (double)item2.exchangeRate;
			voucher.Maker = (string)item2.maker;
			voucher.Checker = (string)item2.checker;
			voucher.Booker = (string)item2.booker;
			voucher.DirectionToggled = item2.DirectionToggled != 0;
			voucher.VoucherMark = item2.VoucherMark != 0;
			// 防御式：动态行理论上必含 VoucherMarkSource（UpdateSchema 已补列），列缺失时兜底 0
			voucher.VoucherMarkSource = (item2 is IDictionary<string, object> markRow && markRow.ContainsKey("VoucherMarkSource"))
				? Convert.ToInt32(markRow["VoucherMarkSource"])
				: 0;
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
			// 修复：原 dictionary[(int)item3.voucherId] 在凭证引用缺失时 KeyNotFoundException。
			// TryGetValue 跳过脏关联行。
			if (dictionary.TryGetValue((int)item3.voucherId, out var voucher4) &&
				itemMap.TryGetValue((int)item3.itemId, out var auxItem))
			{
				voucher4.Details.Add(auxItem);
			}
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
			if (!dicAcc.TryGetValue(item4.AccountId, out var accountKey) ||
				!itemMap.TryGetValue(item4.ItemId, out var auxItem2) ||
				auxItem2.Class == null ||
				!ret.InitialBalance.TryGetValue(accountKey, out var accountBalance))
			{
				continue;
			}
			if (!accountBalance.ClassBalances.TryGetValue(auxItem2.Class, out var value))
			{
				value = new ClassBalance();
				accountBalance.ClassBalances.Add(auxItem2.Class, value);
			}
			value.Total += item4.Balance;
			value.ItemBalances.Add(auxItem2, item4.Balance);
		}
		// 多类别辅助核算组合期初余额（科目×多个辅助项的联合余额）
		try
		{
			Dictionary<int, List<int>> comboRelMap = new Dictionary<int, List<int>>();
			foreach (dynamic relRow in cnn.Query("SELECT `comboId`,`itemId` FROM `ItemComboBalanceRel` ORDER BY `comboId`"))
			{
				int comboRelId = (int)relRow.comboId;
				if (!comboRelMap.TryGetValue(comboRelId, out var comboRelItems))
				{
					comboRelItems = new List<int>();
					comboRelMap.Add(comboRelId, comboRelItems);
				}
				comboRelItems.Add((int)relRow.itemId);
			}
			foreach (dynamic comboRow in cnn.Query("SELECT `id`,`accountId`,`balance` FROM `ItemComboBalance` ORDER BY `id`"))
			{
				if (!dicAcc.TryGetValue((int)comboRow.accountId, out var comboAccount))
				{
					Debug.WriteLine("ComboOpeningBalance skipped, accountId not found: " + (int)comboRow.accountId);
					continue;
				}
				if (!comboRelMap.TryGetValue((int)comboRow.id, out var comboItemIds))
				{
					comboItemIds = new List<int>();
				}
				List<AuxiliaryItem> comboItems = new List<AuxiliaryItem>();
				bool comboItemMissing = false;
				foreach (int comboItemId in comboItemIds)
				{
					if (!itemMap.TryGetValue(comboItemId, out var comboAuxItem))
					{
						Debug.WriteLine("ComboOpeningBalance skipped, itemId not found: " + comboItemId);
						comboItemMissing = true;
						break;
					}
					comboItems.Add(comboAuxItem);
				}
				if (comboItemMissing)
				{
					continue;
				}
				ComboOpeningBalance comboOpeningBalance = new ComboOpeningBalance
				{
					Account = comboAccount,
					Balance = (decimal)comboRow.balance
				};
				comboOpeningBalance.Items.AddRange(comboItems);
				ret.ComboOpeningBalances.Add(comboOpeningBalance);
			}
		}
		catch (Exception ex)
		{
			// 表缺失（如只读老库建表失败）时不阻断打开账套，组合期初余额留空
			Debug.WriteLine("Load ComboOpeningBalances failed: " + ex.Message);
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
					VoucherMarkSource = v.VoucherMarkSource,
					Dirty = v.Dirty
				};
			sQLiteConnection.Execute("DELETE FROM `Voucher` WHERE id=@id", source2.Where(v => v.Dirty == -1), sQLiteTransaction);
			sQLiteConnection.Execute("UPDATE `Voucher` SET `id`=@id,`number`=@number,`type`=@type,`day`=@day,`digest`=@digest,`dc`=@dc,`amount`=@amount,`quantity`=@quantity,`unitPrice`=@unitPrice,`accountId`=@accountId,`foreignId`=@foreignId,`foreignAmount`=@foreignAmount,`exchangeRate`=@exchangeRate,`maker`=@maker,`checker`=@checker,`booker`=@booker,`OppositeAccounts`=@OppositeAccounts,`DirectionToggled`=@DirectionToggled,`VoucherMark`=@VoucherMark,`VoucherMarkSource`=@VoucherMarkSource WHERE id=@id", source2.Where(v => v.Dirty == 2), sQLiteTransaction);
			sQLiteConnection.Execute("INSERT INTO `Voucher`(`id`,`number`,`type`,`day`,`digest`,`dc`,`amount`,`quantity`,`unitPrice`,`accountId`,`foreignId`,`foreignAmount`,`exchangeRate`,`maker`,`checker`,`booker`,`OppositeAccounts`,`DirectionToggled`,`VoucherMark`,`VoucherMarkSource`) VALUES(@id,@number,@type,@day,@digest,@dc,@amount,@quantity,@unitPrice,@accountId,@foreignId,@foreignAmount,@exchangeRate,@maker,@checker,@booker,@OppositeAccounts,@DirectionToggled,@VoucherMark,@VoucherMarkSource)", source2.Where(v => v.Dirty == 1), sQLiteTransaction);
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
			sQLiteConnection.Execute("DELETE FROM `ItemComboBalanceRel`");
			sQLiteConnection.Execute("DELETE FROM `ItemComboBalance`");
			var comboParam = ledger.ComboOpeningBalances.Select((ComboOpeningBalance co, int comboIdx) => new
			{
				id = comboIdx,
				accountId = dicAcc[co.Account],
				balance = co.Balance
			});
			sQLiteConnection.Execute("INSERT INTO `ItemComboBalance`(`id`,`accountId`,`balance`) VALUES(@id,@accountId,@balance)", comboParam, sQLiteTransaction);
			var comboRelParam = ledger.ComboOpeningBalances.SelectMany((ComboOpeningBalance co, int comboIdx) => co.Items.Select((AuxiliaryItem it) => new
			{
				comboId = comboIdx,
				itemId = itemDic[it]
			}));
			sQLiteConnection.Execute("INSERT INTO `ItemComboBalanceRel`(`comboId`,`itemId`) VALUES(@comboId,@itemId)", comboRelParam, sQLiteTransaction);
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
			VoucherMark = v.VoucherMark,
			VoucherMarkSource = v.VoucherMarkSource
		});
		sQLiteConnection.Execute("INSERT INTO `Voucher`(`id`,`number`,`type`,`day`,`digest`,`dc`,`amount`,`quantity`,`unitPrice`,`accountId`,`foreignId`,`foreignAmount`,`exchangeRate`,`maker`,`checker`,`booker`,`OppositeAccounts`,`DirectionToggled`,`VoucherMark`,`VoucherMarkSource`) VALUES(@id,@number,@type,@day,@digest,@dc,@amount,@quantity,@unitPrice,@accountId,@foreignId,@foreignAmount,@exchangeRate,@maker,@checker,@booker,@OppositeAccounts,@DirectionToggled,@VoucherMark,@VoucherMarkSource)", param8, sQLiteTransaction);
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
		sQLiteConnection.Execute("DELETE FROM `ItemComboBalanceRel`");
		sQLiteConnection.Execute("DELETE FROM `ItemComboBalance`");
		var comboParam = ledger.ComboOpeningBalances.Select((ComboOpeningBalance co, int comboIdx) => new
		{
			id = comboIdx,
			accountId = dicAcc[co.Account],
			balance = co.Balance
		});
		sQLiteConnection.Execute("INSERT INTO `ItemComboBalance`(`id`,`accountId`,`balance`) VALUES(@id,@accountId,@balance)", comboParam, sQLiteTransaction);
		var comboRelParam = ledger.ComboOpeningBalances.SelectMany((ComboOpeningBalance co, int comboIdx) => co.Items.Select((AuxiliaryItem it) => new
		{
			comboId = comboIdx,
			itemId = itemDic[it]
		}));
		sQLiteConnection.Execute("INSERT INTO `ItemComboBalanceRel`(`comboId`,`itemId`) VALUES(@comboId,@itemId)", comboRelParam, sQLiteTransaction);
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
		CreateRiskCheckTables(sQLiteConnection);
		try
		{
			CreateComboOpeningTables(sQLiteConnection);
		}
		catch (Exception ex)
		{
			// 建表失败（如账套文件只读）不阻断打开账套，仅记录日志
			Debug.WriteLine("CreateComboOpeningTables failed: " + ex.Message);
		}
		Update_Voucher_MarkSource(sQLiteConnection);
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
			// 修复：原 drop index 无 IF EXISTS，索引不存在（新库）时抛异常导致整个构造失败。
			c.Execute("drop index if exists `idx_v_number`", null, sQLiteTransaction);
			c.Execute("alter table `Voucher` drop column `number`", null, sQLiteTransaction);
			c.Execute("alter table `Voucher` rename column `number1` to `number`", null, sQLiteTransaction);
			c.Execute("create index if not exists `idx_v_number` on `Voucher`(`number`)", null, sQLiteTransaction);
		}
		sQLiteTransaction.Commit();
	}

	private void Update_Voucher_MarkSource(SQLiteConnection c)
	{
		try
		{
			var columns = (from row in c.Query("pragma table_info(`Voucher`)")
				select new
				{
					Name = (string)row.name
				}).ToList();
			if (!columns.Any(col => col.Name.Equals("VoucherMarkSource", StringComparison.OrdinalIgnoreCase)))
			{
				c.Execute("ALTER TABLE `Voucher` ADD COLUMN `VoucherMarkSource` INTEGER NOT NULL DEFAULT 0");
			}
		}
		catch (Exception exception)
		{
			// 补列失败（如账套文件只读）不阻断打开账套，仅记录日志
			Debug.WriteLine("Update_Voucher_MarkSource failed: " + exception.Message);
		}
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

	private static void CreateRiskCheckTables(SQLiteConnection c)
	{
		c.Execute("CREATE TABLE IF NOT EXISTS `RiskCheckScheme`(`id` INTEGER PRIMARY KEY, `name` TEXT, `note` TEXT)");
		c.Execute("CREATE TABLE IF NOT EXISTS `RiskCheckRule`(`id` INTEGER PRIMARY KEY, `schemeId` INTEGER, `ruleType` INTEGER, `note` TEXT, `accountCodes` TEXT, `accountNames` TEXT, `requireLeaf` INTEGER, `openingEnabled` INTEGER, `openingDirection` INTEGER, `openingOp` INTEGER, `openingValue` TEXT, `closingEnabled` INTEGER, `closingDirection` INTEGER, `closingOp` INTEGER, `closingValue` TEXT, `debitEnabled` INTEGER, `debitScope` INTEGER, `debitOp` INTEGER, `debitValue` TEXT, `creditEnabled` INTEGER, `creditScope` INTEGER, `creditOp` INTEGER, `creditValue` TEXT, `auxNegativeEnabled` INTEGER, `auxOp` INTEGER, `auxValue` TEXT, `leftExpr` TEXT, `operatorCode` INTEGER, `rightExpr` TEXT)");
		c.Execute("CREATE INDEX IF NOT EXISTS `idx_rcr_scheme` ON `RiskCheckRule`(`schemeId`)");
	}

	private static void CreateComboOpeningTables(SQLiteConnection c)
	{
		c.Execute("CREATE TABLE IF NOT EXISTS `ItemComboBalance`(`id` INTEGER PRIMARY KEY, `accountId` INTEGER NOT NULL REFERENCES `Account`(`id`), `balance` MONEY NOT NULL DEFAULT 0)");
		c.Execute("CREATE TABLE IF NOT EXISTS `ItemComboBalanceRel`(`comboId` INTEGER NOT NULL REFERENCES `ItemComboBalance`(`id`), `itemId` INTEGER NOT NULL REFERENCES `Item`(`id`))");
		c.Execute("CREATE INDEX IF NOT EXISTS `idx_icb_account` ON `ItemComboBalance`(`accountId`)");
		c.Execute("CREATE INDEX IF NOT EXISTS `idx_icbr_combo` ON `ItemComboBalanceRel`(`comboId`)");
	}

	public List<RiskCheckScheme> GetRiskCheckSchemes(SQLiteConnection conn)
	{
		Dictionary<long, RiskCheckScheme> dic = new Dictionary<long, RiskCheckScheme>();
		List<RiskCheckScheme> list = new List<RiskCheckScheme>();
		foreach (var row in conn.Query("SELECT `id`,`name`,`note` FROM `RiskCheckScheme` ORDER BY `id`"))
		{
			RiskCheckScheme riskCheckScheme = new RiskCheckScheme
			{
				Id = (long)row.id,
				Name = ReadText(row.name),
				Note = ReadText(row.note)
			};
			dic.Add(riskCheckScheme.Id, riskCheckScheme);
			list.Add(riskCheckScheme);
		}
		foreach (var row2 in conn.Query("SELECT `id`,`schemeId`,`ruleType`,`note`,`accountCodes`,`accountNames`,`requireLeaf`,`openingEnabled`,`openingDirection`,`openingOp`,`openingValue`,`closingEnabled`,`closingDirection`,`closingOp`,`closingValue`,`debitEnabled`,`debitScope`,`debitOp`,`debitValue`,`creditEnabled`,`creditScope`,`creditOp`,`creditValue`,`auxNegativeEnabled`,`auxOp`,`auxValue`,`leftExpr`,`operatorCode`,`rightExpr` FROM `RiskCheckRule` ORDER BY `schemeId`,`id`"))
		{
			if (!dic.TryGetValue((long)row2.schemeId, out var value))
			{
				continue;
			}
			value.Rules.Add(new RiskCheckRule
			{
				Id = (long)row2.id,
				SchemeId = (long)row2.schemeId,
				RuleType = (int)row2.ruleType,
				Note = ReadText(row2.note),
				AccountCodes = ReadText(row2.accountCodes),
				AccountNames = ReadText(row2.accountNames),
				RequireLeaf = Convert.ToBoolean(row2.requireLeaf),
				OpeningEnabled = Convert.ToBoolean(row2.openingEnabled),
				OpeningDirection = (int)row2.openingDirection,
				OpeningOp = (int)row2.openingOp,
				OpeningValue = ReadDecimal(row2.openingValue),
				ClosingEnabled = Convert.ToBoolean(row2.closingEnabled),
				ClosingDirection = (int)row2.closingDirection,
				ClosingOp = (int)row2.closingOp,
				ClosingValue = ReadDecimal(row2.closingValue),
				DebitEnabled = Convert.ToBoolean(row2.debitEnabled),
				DebitScope = (int)row2.debitScope,
				DebitOp = (int)row2.debitOp,
				DebitValue = ReadDecimal(row2.debitValue),
				CreditEnabled = Convert.ToBoolean(row2.creditEnabled),
				CreditScope = (int)row2.creditScope,
				CreditOp = (int)row2.creditOp,
				CreditValue = ReadDecimal(row2.creditValue),
				AuxNegativeEnabled = Convert.ToBoolean(row2.auxNegativeEnabled),
				AuxOp = (int)row2.auxOp,
				AuxValue = ReadDecimal(row2.auxValue),
				LeftExpr = ReadText(row2.leftExpr),
				OperatorCode = (int)row2.operatorCode,
				RightExpr = ReadText(row2.rightExpr)
			});
		}
		return list;
	}

	public void SaveRiskCheckScheme(SQLiteConnection conn, RiskCheckScheme scheme)
	{
		using SQLiteTransaction sQLiteTransaction = conn.BeginTransaction();
		try
		{
			if (scheme.Id <= 0)
			{
				scheme.Id = conn.ExecuteScalar<long>("SELECT IFNULL(MAX(`id`),0)+1 FROM `RiskCheckScheme`", null, sQLiteTransaction);
			}
			conn.Execute("INSERT OR REPLACE INTO `RiskCheckScheme`(`id`,`name`,`note`) VALUES(@id,@name,@note)", new
			{
				id = scheme.Id,
				name = scheme.Name,
				note = scheme.Note
			}, sQLiteTransaction);
			conn.Execute("DELETE FROM `RiskCheckRule` WHERE `schemeId`=@id", new { id = scheme.Id }, sQLiteTransaction);
			if (scheme.Rules.Count > 0)
			{
				long num = conn.ExecuteScalar<long>("SELECT IFNULL(MAX(`id`),0) FROM `RiskCheckRule`", null, sQLiteTransaction);
				foreach (RiskCheckRule rule in scheme.Rules)
				{
					if (rule.Id <= 0)
					{
						num++;
						rule.Id = num;
					}
					rule.SchemeId = scheme.Id;
				}
				var param = scheme.Rules.Select((RiskCheckRule r) => new
				{
					id = r.Id,
					schemeId = scheme.Id,
					ruleType = r.RuleType,
					note = r.Note,
					accountCodes = r.AccountCodes,
					accountNames = r.AccountNames,
					requireLeaf = r.RequireLeaf,
					openingEnabled = r.OpeningEnabled,
					openingDirection = r.OpeningDirection,
					openingOp = r.OpeningOp,
					openingValue = DecimalToText(r.OpeningValue),
					closingEnabled = r.ClosingEnabled,
					closingDirection = r.ClosingDirection,
					closingOp = r.ClosingOp,
					closingValue = DecimalToText(r.ClosingValue),
					debitEnabled = r.DebitEnabled,
					debitScope = r.DebitScope,
					debitOp = r.DebitOp,
					debitValue = DecimalToText(r.DebitValue),
					creditEnabled = r.CreditEnabled,
					creditScope = r.CreditScope,
					creditOp = r.CreditOp,
					creditValue = DecimalToText(r.CreditValue),
					auxNegativeEnabled = r.AuxNegativeEnabled,
					auxOp = r.AuxOp,
					auxValue = DecimalToText(r.AuxValue),
					leftExpr = r.LeftExpr,
					operatorCode = r.OperatorCode,
					rightExpr = r.RightExpr
				});
				conn.Execute("INSERT INTO `RiskCheckRule`(`id`,`schemeId`,`ruleType`,`note`,`accountCodes`,`accountNames`,`requireLeaf`,`openingEnabled`,`openingDirection`,`openingOp`,`openingValue`,`closingEnabled`,`closingDirection`,`closingOp`,`closingValue`,`debitEnabled`,`debitScope`,`debitOp`,`debitValue`,`creditEnabled`,`creditScope`,`creditOp`,`creditValue`,`auxNegativeEnabled`,`auxOp`,`auxValue`,`leftExpr`,`operatorCode`,`rightExpr`) VALUES(@id,@schemeId,@ruleType,@note,@accountCodes,@accountNames,@requireLeaf,@openingEnabled,@openingDirection,@openingOp,@openingValue,@closingEnabled,@closingDirection,@closingOp,@closingValue,@debitEnabled,@debitScope,@debitOp,@debitValue,@creditEnabled,@creditScope,@creditOp,@creditValue,@auxNegativeEnabled,@auxOp,@auxValue,@leftExpr,@operatorCode,@rightExpr)", param, sQLiteTransaction);
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

	public void DeleteRiskCheckScheme(SQLiteConnection conn, long schemeId)
	{
		using SQLiteTransaction sQLiteTransaction = conn.BeginTransaction();
		try
		{
			conn.Execute("DELETE FROM `RiskCheckRule` WHERE `schemeId`=@schemeId", new { schemeId }, sQLiteTransaction);
			conn.Execute("DELETE FROM `RiskCheckScheme` WHERE `id`=@schemeId", new { schemeId }, sQLiteTransaction);
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

	public List<RiskCheckScheme> GetRiskCheckSchemes(string dbPath)
	{
		using SQLiteConnection sQLiteConnection = OpenConnection(dbPath);
		CreateRiskCheckTables(sQLiteConnection);
		CreateComboOpeningTables(sQLiteConnection);
		return GetRiskCheckSchemes(sQLiteConnection);
	}

	public void SaveRiskCheckScheme(string dbPath, RiskCheckScheme scheme)
	{
		using SQLiteConnection sQLiteConnection = OpenConnection(dbPath);
		CreateRiskCheckTables(sQLiteConnection);
		CreateComboOpeningTables(sQLiteConnection);
		SaveRiskCheckScheme(sQLiteConnection, scheme);
	}

	public void DeleteRiskCheckScheme(string dbPath, long schemeId)
	{
		using SQLiteConnection sQLiteConnection = OpenConnection(dbPath);
		CreateRiskCheckTables(sQLiteConnection);
		CreateComboOpeningTables(sQLiteConnection);
		DeleteRiskCheckScheme(sQLiteConnection, schemeId);
	}

	public static string ExportRiskCheckSchemesJson(List<RiskCheckScheme> schemes)
	{
		return JsonConvert.SerializeObject(schemes);
	}

	public static List<RiskCheckScheme> ImportRiskCheckSchemesJson(string json)
	{
		return JsonConvert.DeserializeObject<List<RiskCheckScheme>>(json);
	}

	private static SQLiteConnection OpenConnection(string dbPath)
	{
		return new SQLiteConnection(new SQLiteConnectionStringBuilder
		{
			DataSource = dbPath
		}.ConnectionString).OpenAndReturn();
	}

	private static string ReadText(object value)
	{
		if (value == null || value is DBNull)
		{
			return null;
		}
		return (string)value;
	}

	private static decimal ReadDecimal(object value)
	{
		if (value == null || value is DBNull)
		{
			return 0m;
		}
		return decimal.Parse((string)value, CultureInfo.InvariantCulture);
	}

	private static string DecimalToText(decimal value)
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}
}
