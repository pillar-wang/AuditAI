using System.Collections.Generic;

namespace Auditai.Model;

/// <summary>多类别辅助核算组合期初余额（科目×多个辅助项的联合余额，有符号、科目方向为正）</summary>
public class ComboOpeningBalance
{
	public Account Account { get; set; }

	/// <summary>同一组合内每个类别至多一项</summary>
	public List<AuxiliaryItem> Items { get; } = new List<AuxiliaryItem>();

	public decimal Balance { get; set; }
}
