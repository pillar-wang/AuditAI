using System.Collections.Generic;

namespace Auditai.Model;

/// <summary>多类别辅助核算组合期初余额：科目下一个维度组合（每类别一项）的有符号余额（科目方向为正）</summary>
public class ItemComboBalance
{
	public List<Item> Items { get; } = new List<Item>();

	public decimal Balance { get; set; }
}
