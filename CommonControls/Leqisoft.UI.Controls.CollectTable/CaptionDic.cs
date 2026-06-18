using System;

namespace Leqisoft.UI.Controls.CollectTable;

internal static class CaptionDic
{
	internal static readonly CaptionTypeDic Balance = new CaptionTypeDic
	{
		["科目代码"] = typeof(string),
		["科目名称"] = typeof(string),
		["期初借方余额"] = typeof(decimal),
		["期初贷方余额"] = typeof(decimal),
		["本月借方发生额"] = typeof(decimal),
		["本月贷方发生额"] = typeof(decimal),
		["本期借方发生额"] = typeof(decimal),
		["本期贷方发生额"] = typeof(decimal),
		["上期借方发生额"] = typeof(decimal),
		["上期贷方发生额"] = typeof(decimal),
		["期末借方余额"] = typeof(decimal),
		["期末贷方余额"] = typeof(decimal)
	};

	internal static readonly CaptionTypeDic AuxBalance = new CaptionTypeDic
	{
		["科目代码"] = typeof(string),
		["科目名称"] = typeof(string),
		["期初借方余额"] = typeof(decimal),
		["期初贷方余额"] = typeof(decimal),
		["本期借方发生额"] = typeof(decimal),
		["本期贷方发生额"] = typeof(decimal),
		["上期借方发生额"] = typeof(decimal),
		["上期贷方发生额"] = typeof(decimal),
		["期末借方余额"] = typeof(decimal),
		["期末贷方余额"] = typeof(decimal)
	};

	internal static readonly CaptionTypeDic Subsidiary = new CaptionTypeDic
	{
		["日期"] = typeof(DateTime),
		["字"] = typeof(string),
		["号"] = typeof(string),
		["字号"] = typeof(string),
		["摘要"] = typeof(string),
		["科目名称"] = typeof(string),
		["对方科目"] = typeof(string),
		["借方金额"] = typeof(decimal),
		["贷方金额"] = typeof(decimal),
		["方向"] = typeof(string),
		["余额"] = typeof(decimal)
	};

	internal static readonly CaptionTypeDic Summary = new CaptionTypeDic
	{
		["科目代码"] = typeof(string),
		["科目名称"] = typeof(string),
		["1月"] = typeof(decimal),
		["2月"] = typeof(decimal),
		["3月"] = typeof(decimal),
		["4月"] = typeof(decimal),
		["5月"] = typeof(decimal),
		["6月"] = typeof(decimal),
		["7月"] = typeof(decimal),
		["8月"] = typeof(decimal),
		["9月"] = typeof(decimal),
		["10月"] = typeof(decimal),
		["11月"] = typeof(decimal),
		["12月"] = typeof(decimal)
	};

	internal static readonly CaptionTypeDic LedgerAge = new CaptionTypeDic
	{
		["科目名称"] = typeof(string),
		["期初借方余额"] = typeof(decimal),
		["期初贷方余额"] = typeof(decimal),
		["本期借方发生额"] = typeof(decimal),
		["本期贷方发生额"] = typeof(decimal),
		["期末借方余额"] = typeof(decimal),
		["期末贷方余额"] = typeof(decimal),
		["1年以内"] = typeof(decimal),
		["1-2年"] = typeof(decimal),
		["2-3年"] = typeof(decimal),
		["3-4年"] = typeof(decimal),
		["4-5年"] = typeof(decimal),
		["5年以上"] = typeof(decimal)
	};
}
