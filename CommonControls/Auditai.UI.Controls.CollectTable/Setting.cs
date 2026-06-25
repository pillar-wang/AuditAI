using System;
using Auditai.Model;

namespace Auditai.UI.Controls.CollectTable;

public class Setting
{
	public Table Table { get; set; }

	public Ledger Ledger { get; set; }

	public Account Account { get; set; }

	public object Auxiliary { get; set; }

	public DateTime Start { get; set; }

	public DateTime End { get; set; }

	public SubAccountFilterMode SubAccountFitlerMode { get; set; }

	public bool CollectAllAccount { get; set; }

	public bool IsNeedSelectDetailAccount { get; set; }

	public int CollectMaxLevel { get; set; }

	public CollectFillTargetType FillTargetType { get; set; }

	public CollectItemShouldSelectFilter CheckCollectItemShouldBeSelectedFilter { get; set; }

	public CollectItemShouldSelectFilter CollectingFilter { get; set; }

	public bool IsOnlyMyMark { get; set; }

	public bool IsCancelEmptyAccountSelectStatus { get; set; } = true;

}
