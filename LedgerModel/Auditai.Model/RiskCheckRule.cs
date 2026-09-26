namespace Auditai.Model;

public class RiskCheckRule
{
	public const int RULE_TYPE_CONDITION = 0;

	public const int RULE_TYPE_FORMULA = 1;

	public const int DIRECTION_ANY = 0;

	public const int DIRECTION_DEBIT = 1;

	public const int DIRECTION_CREDIT = 2;

	public const int SCOPE_PERIOD_TOTAL = 0;

	public const int SCOPE_VOUCHER_SINGLE = 1;

	public long Id { get; set; }

	public long SchemeId { get; set; }

	public int RuleType { get; set; }

	public string Note { get; set; }

	public string AccountCodes { get; set; }

	public string AccountNames { get; set; }

	public bool RequireLeaf { get; set; }

	public bool OpeningEnabled { get; set; }

	// 方向：0 不限、1 借、2 贷
	// Op 编码对齐 ProjectModel 的 ValidationOperator.Code：0 "="、1 ">"、2 ">="、3 "<"、4 "<="、5 "<>"
	public int OpeningDirection { get; set; }

	public int OpeningOp { get; set; }

	public decimal OpeningValue { get; set; }

	public bool ClosingEnabled { get; set; }

	public int ClosingDirection { get; set; }

	public int ClosingOp { get; set; }

	public decimal ClosingValue { get; set; }

	public bool DebitEnabled { get; set; }

	// Scope：0 本期借方合计、1 单张凭证借方金额（Credit 同）
	public int DebitScope { get; set; }

	public int DebitOp { get; set; }

	public decimal DebitValue { get; set; }

	public bool CreditEnabled { get; set; }

	public int CreditScope { get; set; }

	public int CreditOp { get; set; }

	public decimal CreditValue { get; set; }

	public bool AuxNegativeEnabled { get; set; }

	public int AuxOp { get; set; }

	public decimal AuxValue { get; set; }

	public string LeftExpr { get; set; }

	public int OperatorCode { get; set; }

	public string RightExpr { get; set; }
}
