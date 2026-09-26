using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.Model;

namespace Auditai.UI.LedgerView;

public enum MultiDimRecordType
{
	Opening = 0,
	Transaction = 1
}

public sealed class MultiDimRow
{
	public MultiDimRecordType RecordType { get; set; }

	public Account Account { get; set; }

	/// <summary>凭证，期初行为 null</summary>
	public Voucher Voucher { get; set; }

	/// <summary>凭证日期，期初行为 null</summary>
	public DateTime? Day { get; set; }

	/// <summary>"记-12" 之类：Type.Name + "-" + Number；期初行为空串</summary>
	public string VoucherText { get; set; }

	public string Digest { get; set; }

	/// <summary>期初行为 0</summary>
	public decimal Debit { get; set; }

	/// <summary>期初行为 0</summary>
	public decimal Credit { get; set; }

	/// <summary>所属组期初净额（每行都带，便于显示）</summary>
	public decimal GroupOpening { get; set; }

	/// <summary>逐笔累计（含期初净额），期初行等于组期初</summary>
	public decimal RunningBalance { get; set; }

	/// <summary>期初是否确定可知；跨轴且无组合期初/期初行的组为 false（界面以 "—" 展示）</summary>
	public bool OpeningKnown { get; set; } = true;

	/// <summary>类别名→项名；缺失类别键不存在</summary>
	public Dictionary<string, string> DimensionValues { get; } = new Dictionary<string, string>();

	public string GetDimension(string className)
	{
		if (className == null)
		{
			return string.Empty;
		}
		if (!DimensionValues.TryGetValue(className, out string value))
		{
			return string.Empty;
		}
		return value ?? string.Empty;
	}
}

public sealed class MultiDimGroup
{
	public Account Account { get; set; }

	/// <summary>类别名→项名；缺失类别键不存在</summary>
	public Dictionary<string, string> DimensionValues { get; } = new Dictionary<string, string>();

	public decimal OpeningBalance { get; set; }

	public decimal Debit { get; set; }

	public decimal Credit { get; set; }

	/// <summary>= Opening + Debit - Credit</summary>
	public decimal EndingBalance { get; set; }

	/// <summary>期初是否确定可知；未知时 OpeningBalance 保持 0、期初/期末不参与展示</summary>
	public bool OpeningKnown { get; set; } = true;

	public List<MultiDimRow> Rows { get; } = new List<MultiDimRow>();
}

public sealed class DimensionCondition
{
	public AuxiliaryClass Class { get; set; }

	/// <summary>空列表 = 该类别按值不过滤</summary>
	public List<AuxiliaryItem> Items { get; set; } = new List<AuxiliaryItem>();
}

public static class MultiDimensionQuery
{
	// 组合键分隔符用控制字符，避免与类别/项名称内容冲突
	private const string KeySeparator = "\u0001";

	// 分量内 Code 与 Name 的拼接分隔符，同样用控制字符避免歧义
	private const string ItemKeySeparator = "\u0002";

	public static bool HasAuxiliaryData(Ledger ledger)
	{
		return ledger != null && ledger.AuxiliaryClasses.Count > 0;
	}

	public static List<MultiDimGroup> Query(
		Ledger ledger,
		IEnumerable<Account> accounts,
		DateTime start,
		DateTime end,
		IEnumerable<DimensionCondition> conditions,
		string voucherKeyword = null,
		decimal? minAmount = null,
		decimal? maxAmount = null)
	{
		List<MultiDimGroup> result = new List<MultiDimGroup>();
		if (ledger == null || ledger.AuxiliaryClasses.Count == 0)
		{
			return result;
		}

		TrialBalanceSheet tbs = ledger.GetTrialBalanceSheet(start, end);
		List<Account> leafAccounts = ExpandLeafAccounts(ledger, accounts);
		List<AuxiliaryClass> classes = ledger.AuxiliaryClasses;
		Dictionary<AuxiliaryClass, HashSet<AuxiliaryItem>> condMap = BuildConditionMap(conditions);

		// 组合期初表按科目分流：表内出现过的科目以期初组合表为权威来源（余额口径同为借正贷负原值）
		Dictionary<Account, List<ComboOpeningBalance>> comboByAccount = ledger.ComboOpeningBalances
			.Where((ComboOpeningBalance co) => co.Account != null && co.Items.Count > 0)
			.GroupBy((ComboOpeningBalance co) => co.Account)
			.ToDictionary((IGrouping<Account, ComboOpeningBalance> g) => g.Key, (IGrouping<Account, ComboOpeningBalance> g) => g.ToList());
		HashSet<Account> comboAccounts = new HashSet<Account>(comboByAccount.Keys);

		// account -> (组合键 -> 组)
		Dictionary<Account, Dictionary<string, GroupBuilder>> groupMap = new Dictionary<Account, Dictionary<string, GroupBuilder>>();

		GroupBuilder GetOrCreateGroup(Account account, List<AuxiliaryItem> combo)
		{
			if (!groupMap.TryGetValue(account, out Dictionary<string, GroupBuilder> inner))
			{
				inner = new Dictionary<string, GroupBuilder>();
				groupMap[account] = inner;
			}
			// 分量用 Code+\u0002+Name 区分同名不同 Code 的项；null 占位分量保持空串
			string key = string.Join(KeySeparator, combo.Select((AuxiliaryItem i) => i == null ? string.Empty : (i.Code ?? string.Empty) + ItemKeySeparator + (i.Name ?? string.Empty)));
			if (!inner.TryGetValue(key, out GroupBuilder builder))
			{
				builder = new GroupBuilder
				{
					Group = new MultiDimGroup { Account = account },
					SortItems = new List<AuxiliaryItem>(combo)
				};
				for (int i = 0; i < classes.Count; i++)
				{
					if (combo[i] != null)
					{
						builder.Group.DimensionValues[classes[i].Name] = combo[i].Name;
					}
				}
				inner[key] = builder;
			}
			return builder;
		}

		// 期初行：余额带符号、以科目方向为正；同一 (科目, 类别, 项) 只有一行
		foreach (Account account in leafAccounts)
		{
			if (comboAccounts.Contains(account))
			{
				// 权威科目：按组合期初表的多轴向量建组，跳过 tbs.Start 单轴遍历
				foreach (ComboOpeningBalance co in comboByAccount[account])
				{
					if (co.Balance == 0m)
					{
						// 零余额不生成期初行（与单轴路径跳零一致）；该组若被发生行命中仍由权威科目标志得到确定 0 期初
						continue;
					}
					List<AuxiliaryItem> vector = new List<AuxiliaryItem>(classes.Count);
					for (int k = 0; k < classes.Count; k++)
					{
						vector.Add(null);
					}
					bool badCombo = false;
					foreach (AuxiliaryItem item in co.Items)
					{
						int idx = classes.IndexOf(item != null ? item.Class : null);
						if (idx < 0 || vector[idx] != null)
						{
							// 类别不在账套维度中或同类别出现多项（导入已校验），防御性整体跳过
							badCombo = true;
							break;
						}
						vector[idx] = item;
					}
					if (badCombo)
					{
						continue;
					}
					// 维度值过滤与单轴路径同语义：每个有条件类别必须命中勾选值（类目间 AND）；
					// 无条件类别即便该组合缺位也放行（发生行同款规则）
					bool filteredOut = false;
					for (int k = 0; k < classes.Count; k++)
					{
						if (condMap.TryGetValue(classes[k], out HashSet<AuxiliaryItem> allowed)
							&& (vector[k] == null || !allowed.Contains(vector[k])))
						{
							filteredOut = true;
							break;
						}
					}
					if (filteredOut)
					{
						continue;
					}
					GroupBuilder comboBuilder = GetOrCreateGroup(account, vector);
					comboBuilder.ComboAuthoritative = true;
					MultiDimRow comboRow = new MultiDimRow
					{
						RecordType = MultiDimRecordType.Opening,
						Account = account,
						Voucher = null,
						Day = null,
						VoucherText = string.Empty,
						Digest = string.Empty,
						Debit = 0m,
						Credit = 0m
					};
					for (int k = 0; k < classes.Count; k++)
					{
						if (vector[k] != null)
						{
							comboRow.DimensionValues[classes[k].Name] = vector[k].Name;
						}
					}
					comboBuilder.Group.Rows.Add(comboRow);
					comboBuilder.OpeningSum += co.Balance;
				}
				continue;
			}
			if (!tbs.Start.TryGetValue(account, out AccountBalance accountBalance))
			{
				continue;
			}
			foreach (AuxiliaryClass c in classes)
			{
				if (!accountBalance.ClassBalances.TryGetValue(c, out ClassBalance classBalance))
				{
					continue;
				}
				condMap.TryGetValue(c, out HashSet<AuxiliaryItem> allowed);
				foreach (KeyValuePair<AuxiliaryItem, decimal> pair in classBalance.ItemBalances)
				{
					if (pair.Value == 0m)
					{
						continue;
					}
					if (allowed != null && !allowed.Contains(pair.Key))
					{
						continue;
					}
					List<AuxiliaryItem> combo = new List<AuxiliaryItem>(classes.Count);
					foreach (AuxiliaryClass cc in classes)
					{
						combo.Add(cc == c ? pair.Key : null);
					}
					GroupBuilder builder = GetOrCreateGroup(account, combo);
					MultiDimRow row = new MultiDimRow
					{
						RecordType = MultiDimRecordType.Opening,
						Account = account,
						Voucher = null,
						Day = null,
						VoucherText = string.Empty,
						Digest = string.Empty,
						Debit = 0m,
						Credit = 0m
					};
					row.DimensionValues[c.Name] = pair.Key.Name;
					builder.Group.Rows.Add(row);
					builder.OpeningSum += pair.Value;
				}
			}
		}

		// 发生行：凭证级过滤（日期、凭证号关键字、|Amount| 区间）后按类别笛卡尔积展开
		DateTime startDate = start.Date;
		DateTime endDate = end.Date;
		bool filterKeyword = !string.IsNullOrEmpty(voucherKeyword);
		Dictionary<Account, List<Voucher>> vouchersByAccount = new Dictionary<Account, List<Voucher>>();
		foreach (Voucher v in ledger.Vouchers)
		{
			if (v.Account == null)
			{
				continue;
			}
			DateTime day = v.Day.Date;
			if (day < startDate || day > endDate)
			{
				continue;
			}
			if (v.Details.Count == 0)
			{
				// 无维度属性的分录不在多维核算中展示
				continue;
			}
			if (filterKeyword)
			{
				string text = v.Type.Name + "-" + v.Number;
				bool hit = (v.Number != null && v.Number.IndexOf(voucherKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
					|| text.IndexOf(voucherKeyword, StringComparison.OrdinalIgnoreCase) >= 0;
				if (!hit)
				{
					continue;
				}
			}
			if (minAmount.HasValue || maxAmount.HasValue)
			{
				decimal abs = Math.Abs(v.Amount);
				if (minAmount.HasValue && abs < minAmount.Value)
				{
					continue;
				}
				if (maxAmount.HasValue && abs > maxAmount.Value)
				{
					continue;
				}
			}
			if (!vouchersByAccount.TryGetValue(v.Account, out List<Voucher> list))
			{
				list = new List<Voucher>();
				vouchersByAccount[v.Account] = list;
			}
			list.Add(v);
		}

		foreach (Account account in leafAccounts)
		{
			if (!vouchersByAccount.TryGetValue(account, out List<Voucher> vouchers))
			{
				continue;
			}
			foreach (Voucher v in vouchers)
			{
				// 有条件的类别候选集为空 → 整笔分录跳过（类目间 AND）；
				// 无条件的类别取该类别全部去重项，为空则以 null 占位（该列显示空）
				List<List<AuxiliaryItem>> candidates = new List<List<AuxiliaryItem>>(classes.Count);
				bool skipVoucher = false;
				foreach (AuxiliaryClass c in classes)
				{
					List<AuxiliaryItem> d;
					if (condMap.TryGetValue(c, out HashSet<AuxiliaryItem> allowed))
					{
						d = v.Details.Where((AuxiliaryItem x) => x.Class == c && allowed.Contains(x)).Distinct().ToList();
						if (d.Count == 0)
						{
							skipVoucher = true;
							break;
						}
					}
					else
					{
						d = v.Details.Where((AuxiliaryItem x) => x.Class == c).Distinct().ToList();
						if (d.Count == 0)
						{
							d.Add(null);
						}
					}
					candidates.Add(d);
				}
				if (skipVoucher)
				{
					continue;
				}

				// 金额口径与 GetDebits 一致：每个维度组合记整笔金额
				List<List<AuxiliaryItem>> combos = new List<List<AuxiliaryItem>> { new List<AuxiliaryItem>() };
				foreach (List<AuxiliaryItem> d in candidates)
				{
					List<List<AuxiliaryItem>> next = new List<List<AuxiliaryItem>>(combos.Count * d.Count);
					foreach (List<AuxiliaryItem> prefix in combos)
					{
						foreach (AuxiliaryItem item in d)
						{
							List<AuxiliaryItem> combo = new List<AuxiliaryItem>(prefix);
							combo.Add(item);
							next.Add(combo);
						}
					}
					combos = next;
				}

				foreach (List<AuxiliaryItem> combo in combos)
				{
					GroupBuilder builder = GetOrCreateGroup(account, combo);
					if (comboAccounts.Contains(account))
					{
						// 权威科目的组即使无期初行命中，期初也是确定的 0
						builder.ComboAuthoritative = true;
					}
					MultiDimRow row = new MultiDimRow
					{
						RecordType = MultiDimRecordType.Transaction,
						Account = account,
						Voucher = v,
						Day = v.Day,
						VoucherText = v.Type.Name + "-" + v.Number,
						Digest = v.Digest ?? string.Empty,
						Debit = v.IsDebit ? v.Amount : 0m,
						Credit = v.IsDebit ? 0m : v.Amount
					};
					for (int i = 0; i < classes.Count; i++)
					{
						if (combo[i] != null)
						{
							row.DimensionValues[classes[i].Name] = combo[i].Name;
						}
					}
					builder.Group.Rows.Add(row);
				}
			}
		}

		// 组排序：科目 Code → 组合值依次按各类别项 Code（自然排序）
		List<GroupBuilder> builders = new List<GroupBuilder>();
		foreach (Dictionary<string, GroupBuilder> inner in groupMap.Values)
		{
			builders.AddRange(inner.Values);
		}
		builders.Sort((GroupBuilder x, GroupBuilder y) =>
		{
			int cmp = StringNumberComparer.Instance.Compare(x.Group.Account.Code, y.Group.Account.Code);
			if (cmp != 0)
			{
				return cmp;
			}
			for (int i = 0; i < x.SortItems.Count && i < y.SortItems.Count; i++)
			{
				cmp = StringNumberComparer.Instance.Compare(x.SortItems[i]?.Code ?? string.Empty, y.SortItems[i]?.Code ?? string.Empty);
				if (cmp != 0)
				{
					return cmp;
				}
			}
			return 0;
		});

		foreach (GroupBuilder builder in builders)
		{
			MultiDimGroup group = builder.Group;
			group.Rows.Sort(CompareRows);
			decimal debit = 0m;
			decimal credit = 0m;
			foreach (MultiDimRow row in group.Rows)
			{
				debit += row.Debit;
				credit += row.Credit;
			}
			group.Debit = debit;
			group.Credit = credit;
			// 期初已知：①权威科目组合表覆盖（未列示组合=确定0）；②组内有期初行；③单轴/无轴组（边际即联合）
			group.OpeningKnown = builder.ComboAuthoritative
				|| group.Rows.Any((MultiDimRow r) => r.RecordType == MultiDimRecordType.Opening)
				|| builder.SortItems.Count((AuxiliaryItem i) => i != null) <= 1;
			if (group.OpeningKnown)
			{
				group.OpeningBalance = builder.OpeningSum;
				decimal running = group.OpeningBalance;
				foreach (MultiDimRow row in group.Rows)
				{
					running += row.Debit - row.Credit;
					row.RunningBalance = running;
					row.GroupOpening = group.OpeningBalance;
				}
			}
			// 未知组 OpeningBalance 保持 0：期末仍按公式计算但无实际语义，界面不展示
			group.EndingBalance = group.OpeningBalance + group.Debit - group.Credit;
			foreach (MultiDimRow row in group.Rows)
			{
				row.OpeningKnown = group.OpeningKnown;
			}
			result.Add(group);
		}
		return result;
	}

	// 期初行在前，发生行按 日期 → 凭证字 → 凭证号（自然排序），与 GetSubsidiaryLedgerImpl 一致
	private static int CompareRows(MultiDimRow x, MultiDimRow y)
	{
		int cmp = ((int)x.RecordType).CompareTo((int)y.RecordType);
		if (cmp != 0 || x.RecordType == MultiDimRecordType.Opening)
		{
			return cmp;
		}
		cmp = x.Voucher.Day.CompareTo(y.Voucher.Day);
		if (cmp != 0)
		{
			return cmp;
		}
		cmp = Comparer<string>.Default.Compare(x.Voucher.Type.Name, y.Voucher.Type.Name);
		if (cmp != 0)
		{
			return cmp;
		}
		return StringNumberComparer.Instance.Compare(x.Voucher.Number, y.Voucher.Number);
	}

	// 输入为空 = 全部叶子科目；含父科目时展开为其叶子后代，按引用去重
	private static List<Account> ExpandLeafAccounts(Ledger ledger, IEnumerable<Account> accounts)
	{
		HashSet<Account> set = new HashSet<Account>();
		List<Account> input = accounts?.ToList();
		if (input == null || input.Count == 0)
		{
			// null/空 = 全部叶子科目（Accounts 为树根集合，递归收集叶子）
			CollectLeaves(ledger.Accounts, set);
		}
		else
		{
			foreach (Account account in input)
			{
				if (account == null)
				{
					continue;
				}
				if (account.Children.Count == 0)
				{
					set.Add(account);
				}
				else
				{
					CollectLeaves(account.DescendantsAndSelf, set);
				}
			}
		}
		List<Account> list = set.ToList();
		list.Sort((Account x, Account y) => StringNumberComparer.Instance.Compare(x.Code, y.Code));
		return list;
	}

	private static void CollectLeaves(IEnumerable<Account> source, HashSet<Account> target)
	{
		foreach (Account account in source)
		{
			if (account == null)
			{
				continue;
			}
			if (account.Children.Count == 0)
			{
				target.Add(account);
			}
			else
			{
				CollectLeaves(account.Children, target);
			}
		}
	}

	private static Dictionary<AuxiliaryClass, HashSet<AuxiliaryItem>> BuildConditionMap(IEnumerable<DimensionCondition> conditions)
	{
		Dictionary<AuxiliaryClass, HashSet<AuxiliaryItem>> map = new Dictionary<AuxiliaryClass, HashSet<AuxiliaryItem>>();
		if (conditions == null)
		{
			return map;
		}
		foreach (DimensionCondition condition in conditions)
		{
			if (condition?.Class == null || condition.Items == null || condition.Items.Count == 0)
			{
				continue;
			}
			map[condition.Class] = new HashSet<AuxiliaryItem>(condition.Items);
		}
		return map;
	}

	private sealed class GroupBuilder
	{
		public MultiDimGroup Group;

		public List<AuxiliaryItem> SortItems;

		public decimal OpeningSum;

		/// <summary>该科目存在组合期初表数据：未列示的组合期初即为确定的 0</summary>
		public bool ComboAuthoritative;
	}
}
