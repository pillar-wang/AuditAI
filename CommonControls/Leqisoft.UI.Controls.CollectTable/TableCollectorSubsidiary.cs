﻿using System;
using System.Collections.Generic;
using System.Linq;
using Leqisoft.DTO;
using Leqisoft.Model;
using Leqisoft.UI.Controls.SmartCollector;
using Newtonsoft.Json;

namespace Leqisoft.UI.Controls.CollectTable;

public class TableCollectorSubsidiary : TableCollectorAbstract
{
	public TableCollectorSubsidiary(Ledger ledger, Leqisoft.Model.Table table)
		: base(ledger, table)
	{
		base.CollectObject = CollectObjectEnum.Subsidiary;
	}

	public override void ThrowExceptionIfUnExpectAuditYear(int auditYear)
	{
	}

	public override void ThrowExceptionIfUnExpectSource()
	{
		if (base.Source == null)
		{
			throw new ArgumentNullException("数据源不能为空");
		}
		if (!base.Source.All((object v) => v is Voucher))
		{
			throw new ArgumentException("数据源类型不合法！");
		}
	}

	public override bool IsAbleUseCurrentSettingToCollectData()
	{
		if (base.Setting.Account == null && !base.Setting.CollectAllAccount && !base.Setting.IsOnlyMyMark)
		{
			return false;
		}
		return true;
	}

	public override TableCollectResult Collect(int auditYear)
	{
		ThrowExceptionIfUnExpectAuditYear(auditYear);
		if (base.Source == null)
		{
			base.Source = GetSourceFromSetting();
		}
		ThrowExceptionIfUnExpectSource();
		if (base.Setting.Account == null && !base.Setting.CollectAllAccount && !base.Setting.IsOnlyMyMark)
		{
			throw new InvalidCollectSettingException("采集器没有设置科目！");
		}
		Dictionary<Voucher, SubsidiaryLedgerEntry> dic = GetVoucherSubsidiaryLedgerEntry();
		TableCollectResult tableCollectResult = new TableCollectResult(this);
		if (base.IsSaveCollectResultByColumnId)
		{
			tableCollectResult.ValuesOnColumn = new Dictionary<long, List<object>>();
		}
		foreach (KeyValuePair<long, string> map in base.Maps)
		{
			if (base.IsSaveCollectResultByColumnId || base.Setting.Table.Columns.Any((Leqisoft.Model.Column c) => c.Id == new Id64(map.Key)))
			{
				IEnumerable<Voucher> source = base.Source.Cast<Voucher>();
				List<object> value = (from v in source
					where dic.ContainsKey(v)
					select dic[v].GetSubsidiaryValue(map.Value)).ToList();
				if (!base.IsSaveCollectResultByColumnId)
				{
					tableCollectResult.Values.Add(base.Setting.Table.Columns.GetById(new Id64(map.Key)), value);
				}
				else
				{
					tableCollectResult.ValuesOnColumn.Add(map.Key, value);
				}
			}
		}
		return tableCollectResult;
	}

	protected override void Intelligence(Leqisoft.Model.Table table, string borrowLogic)
	{
		base.Maps.Clear();
		TableCollector tableCollector = DictionarySync.TableCollector;
		foreach (Leqisoft.Model.Column column in table.Columns)
		{
			string text = tableCollector.IntelligenceSubsidiaryCol(column.CaptionDisplay);
			if (text != null)
			{
				base.Maps.Add(column.Id.Value, text);
			}
		}
	}

	public override string Serialize()
	{
		ExportArgs exportArgs = new ExportArgs
		{
			CollectObject = CollectObjectEnum.Subsidiary,
			MonthStart = base.Setting.Start.Month,
			MonthEnd = base.Setting.End.Month,
			Mapping = base.Maps,
			FillTargetType = base.Setting.FillTargetType,
			CollectAllCount = base.Setting.CollectAllAccount,
			IsOnlyMyMark = base.Setting.IsOnlyMyMark
		};
		if (base.Setting.Account != null)
		{
			exportArgs.AccountName = TableCollectorAbstract.GetFullPath(base.Setting.Account);
		}
		object auxiliary = base.Setting.Auxiliary;
		if (!(auxiliary is AuxiliaryClass auxiliaryClass))
		{
			if (auxiliary is AuxiliaryItem auxiliaryItem)
			{
				exportArgs.AuxName = auxiliaryItem.Name;
			}
		}
		else
		{
			exportArgs.AuxName = auxiliaryClass.Name;
		}
		return JsonConvert.SerializeObject(exportArgs);
	}

	private List<object> GetSourceFromSetting()
	{
		List<object> list = new List<object>();
		if (base.Setting.IsOnlyMyMark)
		{
			if (base.Setting.CollectAllAccount || base.Setting.Account == null)
			{
				list.AddRange(GetAllVouchersInMyRemark());
			}
			else
			{
				list.AddRange(GetAccountVouchersInMyRemark(base.Setting.Account, base.Setting.Auxiliary));
			}
			return list;
		}
		if (base.Setting.CollectAllAccount)
		{
			list.AddRange(GetAllVouchers());
			return list;
		}
		Func<Voucher, bool> voucherAuxiliaryFilterFunc = GetVoucherAuxiliaryFilterFunc(base.Setting.Auxiliary);
		SubsidiaryLedger subsidiaryLedger = GetSubsidiaryLedger();
		foreach (MonthSubsidiaryLedger item in subsidiaryLedger.Months.OrderBy((MonthSubsidiaryLedger t) => t.Month))
		{
			IOrderedEnumerable<SubsidiaryLedgerEntry> source = (from t in item.Entries
				orderby t.Voucher.Day.Year, t.Voucher.Day.Month
				select t).ThenBy((SubsidiaryLedgerEntry m) => m.Voucher.Number, StringNumberComparer.Instance).ThenBy((SubsidiaryLedgerEntry s) => s.Voucher.Type.Name);
			if (voucherAuxiliaryFilterFunc != null)
			{
				list.AddRange(source.Select((SubsidiaryLedgerEntry e) => e.Voucher).Where(voucherAuxiliaryFilterFunc));
			}
			else
			{
				list.AddRange(source.Select((SubsidiaryLedgerEntry e) => e.Voucher));
			}
		}
		return list;
	}

	private IEnumerable<Voucher> GetAllVouchers()
	{
		return (from u in base.Setting.Ledger.Vouchers
			orderby u.Day.Year, u.Day.Month
			select u).ThenBy((Voucher u) => u.Number, StringNumberComparer.Instance).ThenBy((Voucher u) => u.Type.Name);
	}

	private IEnumerable<Voucher> GetAllVouchersInMyRemark()
	{
		return (from v in base.Setting.Ledger.Vouchers
			where v.VoucherMark
			select v into u
			orderby u.Day.Year, u.Day.Month
			select u).ThenBy((Voucher u) => u.Number, StringNumberComparer.Instance).ThenBy((Voucher u) => u.Type.Name);
	}

	private IEnumerable<Voucher> GetAccountVouchersInMyRemark(Account parentAccount, object auxiliary)
	{
		Func<Voucher, bool> func = GetVoucherAuxiliaryFilterFunc(auxiliary);
		if (func == null)
		{
			func = (Voucher u) => true;
		}
		return (from u in (from v in base.Setting.Ledger.Vouchers
				where v.VoucherMark
				where IsDescendant(v.Account, parentAccount)
				select v).Where(func)
			orderby u.Day.Year, u.Day.Month
			select u).ThenBy((Voucher u) => u.Number, StringNumberComparer.Instance).ThenBy((Voucher u) => u.Type.Name);
		static bool IsDescendant(Account child, Account parent)
		{
			for (Account account = child; account != null; account = account.Parent)
			{
				if (account == parent)
				{
					return true;
				}
			}
			return false;
		}
	}

	private Func<Voucher, bool> GetVoucherAuxiliaryFilterFunc(object auxiliary)
	{
		Func<Voucher, bool> result = null;
		if (auxiliary != null)
		{
			AuxiliaryClass auxClass = auxiliary as AuxiliaryClass;
			if (auxClass != null)
			{
				result = delegate(Voucher u)
				{
					if (u.Details != null)
					{
						foreach (AuxiliaryItem detail in u.Details)
						{
							if (detail.Class == auxClass)
							{
								return true;
							}
						}
					}
					return false;
				};
			}
			else
			{
				AuxiliaryItem auxItem = auxiliary as AuxiliaryItem;
				if (auxItem != null)
				{
					result = delegate(Voucher u)
					{
						if (u.Details != null)
						{
							foreach (AuxiliaryItem detail2 in u.Details)
							{
								if (detail2 == auxItem)
								{
									return true;
								}
							}
						}
						return false;
					};
				}
			}
		}
		return result;
	}

	private Dictionary<Voucher, SubsidiaryLedgerEntry> GetVoucherSubsidiaryLedgerEntry()
	{
		if (base.Setting.CollectAllAccount || base.Setting.IsOnlyMyMark)
		{
			Dictionary<Voucher, SubsidiaryLedgerEntry> dictionary = new Dictionary<Voucher, SubsidiaryLedgerEntry>();
			{
				foreach (object item in base.Source)
				{
					Voucher voucher = (Voucher)item;
					SubsidiaryLedgerEntry value = new SubsidiaryLedgerEntry(voucher, 0m);
					dictionary.Add(voucher, value);
				}
				return dictionary;
			}
		}
		SubsidiaryLedger subsidiaryLedger = GetSubsidiaryLedger();
		return subsidiaryLedger.Months.SelectMany((MonthSubsidiaryLedger m) => m.Entries).ToDictionary((SubsidiaryLedgerEntry t) => t.Voucher, (SubsidiaryLedgerEntry t) => t);
	}

	private SubsidiaryLedger GetSubsidiaryLedger()
	{
		Ledger ledger = base.Setting.Ledger;
		Account account = base.Setting.Account;
		DateTime start = new DateTime(1, 1, 1);
		DateTime end = new DateTime(9999, 12, DateTime.DaysInMonth(9999, 12));
		object auxiliary = base.Setting.Auxiliary;
		if (!(auxiliary is AuxiliaryClass auxClass))
		{
			if (auxiliary is AuxiliaryItem auxItem)
			{
				return ledger.GetSubsidiaryLedger(account, start, end, auxItem);
			}
			return ledger.GetSubsidiaryLedger(account, start, end);
		}
		return ledger.GetSubsidiaryLedger(account, start, end, auxClass);
	}
}
