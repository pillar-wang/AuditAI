namespace LedgerImport;

public enum FailureReasonEnum
{
	ColumnNotFound,
	BalanceAccountCodeEmpty,
	VoucherAccountCodeEmpty,
	AccountNameEmpty,
	RepeatAccountCode,
	NotFoundAnyAccount,
	NotFoundParentAccount,
	AccountLevelCannotCreate,
	AccountParentCannotCreateBecauseName,
	BeginBalanceNotBeZero,
	AccountBalanceNotEqualsSumChild,
	AuxiliaryAccountNotBeLastLevel,
	VoucherAccountCodeNotFound,
	VoucherAccountNotBeLastLevel,
	VoucherTypeEmpty,
	VoucherNumberEmpty,
	VoucherDateTypeNotCorrect,
	VoucherDebitAmountTypeNotCorrect,
	VoucherCreditAmountTypeNotCorrect,
	SpecifyVoucherDebitNotEqualsCredit,
	VoucherSumDebitNotEqualsSumCredit,
	AuxiliaryBeginBalanceNotEqualsAccountBegin,
	VoucherNotRelatedAuxiliaryAccount,
	VoucherAuxiliaryNotFound,
	AuxiliaryDataNotBeCorrect,
	AccCodeNotBeInBalanceRule,
	BalanceNotBeLastLevel,
	SpecificMessage,
	AuxBalanceWithoutVoucherAux
}
