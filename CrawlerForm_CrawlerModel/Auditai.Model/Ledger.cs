using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Auditai.Model;

public class Ledger
{
	public int Year { get; set; }

	public string CompanyName { get; set; }

	public string LedgerNumber { get; set; }

	public DateTime StartDate { get; set; }

	public Currency BaseCurrency { get; set; }

	public List<Account> Accounts { get; set; } = new List<Account>();


	public List<Item> Items { get; set; } = new List<Item>();


	public List<ItemClass> ItemClasses { get; set; } = new List<ItemClass>();


	public List<Voucher> Vouchers { get; set; } = new List<Voucher>();


	public List<Currency> ForeignCurrencies { get; set; } = new List<Currency>();


	public List<VoucherType> VoucherTypes { get; set; } = new List<VoucherType>();


	public Ledger(LedgerInfo lvm)
	{
		Year = lvm.Year;
		CompanyName = lvm.CompanyName;
		LedgerNumber = lvm.LedgerNumber;
	}

	public void SaveAsSqlite(string fileName)
	{
		foreach (Account account in Accounts)
		{
			account.Name = account.Name.Replace("\r", "").Replace("\n", "");
			account.Code = account.Code.Replace("\r", "").Replace("\n", "");
		}
		foreach (ItemClass itemClass in ItemClasses)
		{
			itemClass.Code = itemClass.Code.Replace("\r", "").Replace("\n", "");
			itemClass.Name = itemClass.Name.Replace("\r", "").Replace("\n", "");
		}
		foreach (Item item in Items)
		{
			item.Code = item.Code.Replace("\r", "").Replace("\n", "");
			item.Name = item.Name.Replace("\r", "").Replace("\n", "");
		}
		if (File.Exists(fileName))
		{
			File.Delete(fileName);
		}
		LSDb lSDb = LSDb.Create(LSDb.DbProvider.Sqlite);
		lSDb.DataSource = fileName;
		lSDb.ExecuteNonQuery("\r\ncreate table Ledger(\r\n\tcompanyName text,\r\n\tstartDate date);\r\ncreate table Account(\r\n\tid integer primary key,\r\n  parentId integer,\r\n\tcode text,\r\n\tname text,\r\n\tdc integer,\r\n\tbalance money,\r\n\tquantity real,\r\n\tunitPrice real);\r\ncreate table AccountRel(\r\n  ancestorId integer,\r\n  descendantId integer);\r\ncreate table ForeignCurrency(\r\n\tid integer primary key,\r\n\tname text);\r\ncreate table ItemClass(\r\n\tid integer primary key,\r\n\tcode text,\r\n\tname text);\r\ncreate table VoucherType(\r\n\tid integer primary key,\r\n\tname text);\r\ncreate table Item(\r\n\tid integer primary key,\r\n\tclassId integer references ItemClass(id),\r\n\tcode text,\r\n\tname text);\r\ncreate table ForeignBalance(\r\n\taccountId integer references Account(id),\r\n\tcurrencyId integer references ForeignCurrency(id),\r\n\tforeignBalance money,\r\n\tstandardBalance money,\r\n\texchangeRate real);\r\ncreate table ItemBalance(\r\n\taccountId integer references Account(id),\r\n\titemId integer references Item(id),\r\n\tbalance money,\r\n\tquantity real,\r\n\tunitPrice real);\r\ncreate table ItemForeignBalance(\r\n\taccountId integer references Account(id),\r\n\titemId integer references Item(id),\r\n\tcurrencyId integer references ForeignCurrency(id),\r\n\tforeignBalance money,\r\n\tstandardBalance money,\r\n\texchangeRate real);\r\ncreate table Voucher(\r\n\tid integer primary key,\r\n\tnumber text,\r\n\ttype integer references VoucherType(id),\r\n\tday date,\r\n\tdigest text,\r\n\tdc integer,\r\n\tamount money,\r\n\tquantity real,\r\n\tunitPrice real,\r\n\taccountId integer references Account(id),\r\n\tforeignId integer references ForeignCurrency(id),\r\n\tforeignAmount money,\r\n\texchangeRate real,\r\n  maker text,\r\n  checker text,\r\n  booker text);\r\ncreate table VoucherItemRel(\r\n\tvoucherId integer references Voucher(id),\r\n\titemId integer references Item(id));\r\n");
		lSDb.InsertOne("Ledger", new object[2] { CompanyName, StartDate });
		Dictionary<Account, int> AccountDic = Accounts.ToDictionary((Account x) => x, (Account x) => Accounts.IndexOf(x));
		lSDb.Insert("Account", AccountDic.Select((KeyValuePair<Account, int> x) => new object[8]
		{
			x.Value,
			(x.Key.Parent == null) ? (-1) : AccountDic[x.Key.Parent],
			x.Key.Code,
			x.Key.Name,
			x.Key.IsDebit,
			x.Key.Balance,
			DBNull.Value,
			DBNull.Value
		}.AsEnumerable()));
		List<IEnumerable<object>> list = new List<IEnumerable<object>>();
		foreach (KeyValuePair<Account, int> item2 in AccountDic)
		{
			list.Add(new object[2] { item2.Value, item2.Value });
			for (Account parent = item2.Key.Parent; parent != null; parent = parent.Parent)
			{
				list.Add(new object[2]
				{
					AccountDic[parent],
					item2.Value
				});
			}
		}
		lSDb.Insert("AccountRel", list);
		Dictionary<Currency, int> CurrencyDic = ForeignCurrencies.ToDictionary((Currency x) => x, (Currency x) => ForeignCurrencies.IndexOf(x));
		lSDb.Insert("ForeignCurrency", CurrencyDic.Select((KeyValuePair<Currency, int> x) => new object[2]
		{
			x.Value,
			x.Key.Name
		}.AsEnumerable()));
		Dictionary<ItemClass, int> ItemClassDic = ItemClasses.ToDictionary((ItemClass x) => x, (ItemClass x) => ItemClasses.IndexOf(x));
		lSDb.Insert("ItemClass", ItemClassDic.Select((KeyValuePair<ItemClass, int> x) => new object[3]
		{
			x.Value,
			x.Key.Code,
			x.Key.Name
		}.AsEnumerable()));
		Dictionary<VoucherType, int> VoucherTypeDic = VoucherTypes.ToDictionary((VoucherType x) => x, (VoucherType x) => VoucherTypes.IndexOf(x));
		lSDb.Insert("VoucherType", VoucherTypeDic.Select((KeyValuePair<VoucherType, int> x) => new object[2]
		{
			x.Value,
			x.Key.Name
		}.AsEnumerable()));
		Dictionary<Item, int> ItemDic = Items.ToDictionary((Item x) => x, (Item x) => Items.IndexOf(x));
		lSDb.Insert("Item", ItemDic.Select((KeyValuePair<Item, int> x) => new object[4]
		{
			x.Value,
			ItemClassDic[x.Key.ItemClass],
			x.Key.Code,
			x.Key.Name
		}.AsEnumerable()));
		lSDb.Insert("ForeignBalance", from x in AccountDic
			from y in x.Key.Foreign
			select new object[5]
			{
				x.Value,
				CurrencyDic[y.Key],
				y.Value.ForeignAmount,
				y.Value.StandardAmount,
				y.Value.ExchangeRate
			}.AsEnumerable());
		lSDb.Insert("ItemBalance", from x in AccountDic
			from y in x.Key.ItemBalance
			select new object[5]
			{
				x.Value,
				ItemDic[y.Key],
				y.Value.Balance,
				DBNull.Value,
				DBNull.Value
			}.AsEnumerable());
		lSDb.Insert("ItemForeignBalance", from x in AccountDic
			from y in from y in x.Key.ItemBalance
				from z in y.Value.Foreign
				select new
				{
					Item = y.Key,
					ForeignBalance = z
				}
			select new object[6]
			{
				x.Value,
				ItemDic[y.Item],
				CurrencyDic[y.ForeignBalance.Key],
				y.ForeignBalance.Value.ForeignAmount,
				y.ForeignBalance.Value.StandardAmount,
				y.ForeignBalance.Value.ExchangeRate
			}.AsEnumerable());
		int index = 0;
		Dictionary<Voucher, int> voucherIndexMap = Vouchers.ToDictionary((Voucher v) => v, (Voucher v) => index++);
		lSDb.Insert("Voucher", Vouchers.Select((Voucher x) => new object[16]
		{
			voucherIndexMap[x],
			x.Number,
			VoucherTypeDic[x.Type],
			x.Day,
			x.Digest,
			x.IsDebit,
			x.Amount,
			x.Quantity,
			x.UnitPrice,
			AccountDic[x.Account],
			CurrencyDic[x.Currency],
			x.Foreign.ForeignAmount,
			x.Foreign.ExchangeRate,
			x.Maker,
			x.Checker,
			x.Booker
		}.AsEnumerable()));
		lSDb.Insert("VoucherItemRel", from x in Vouchers
			from y in x.Details
			select new object[2]
			{
				voucherIndexMap[x],
				ItemDic[y]
			}.AsEnumerable());
		lSDb.ExecuteNonQuery("\r\ncreate index idx_ar_ancestorId on AccountRel(ancestorId);\r\ncreate index idx_ar_descendantId on AccountRel(descendantId);\r\ncreate index idx_i_classId on Item(classId);\r\ncreate index idx_fb_accountId on ForeignBalance(accountId);\r\ncreate index idx_fb_currencyId on ForeignBalance(currencyId);\r\ncreate index idx_ib_accountId on ItemBalance(accountId);\r\ncreate index idx_ib_itemId on ItemBalance(itemId);\r\ncreate index idx_ifb_accountId on ItemForeignBalance(accountId);\r\ncreate index idx_ifb_itemId on ItemForeignBalance(itemId);\r\ncreate index idx_ifb_currencyId on ItemForeignBalance(currencyId);\r\ncreate index idx_v_accountId on Voucher(accountId);\r\ncreate index idx_v_number on Voucher(number);\r\ncreate index idx_v_type on Voucher(type);\r\ncreate index idx_v_day on Voucher(day);\r\ncreate index idx_v_foreignId on Voucher(foreignId);\r\ncreate index idx_vir_voucherId on VoucherItemRel(voucherId);\r\ncreate index idx_vir_itemId on VoucherItemRel(itemId);\r\ncreate view vw_AccountItem as \r\n\tselect v.accountId,itemid from VoucherItemRel r\r\n\tleft join Voucher v on r.voucherId=v.id\r\n\tunion select accountId,itemId from ItemBalance\r\n");
	}
}
