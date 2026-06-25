namespace Auditai.Model;

public interface ILegderVirtualTableSetting
{
	string GetBalanceVirtualTableName();

	string GetVoucherVirtualTableName();

	long GetBalanceVirtualTableId();

	long GetVoucherVirtualTableId();

	long GetBalanceVirtualTableColumnId(string columnName);

	long GetVoucherVirtualTableColumnId(string columnName);

	string GetBalanceVirtualTableColumnName(long columnId);

	string GetVoucherVirtualTableColumnName(long columnId);

	LedgerVirtualTable GetBalanceEmptyVirtualTable();

	LedgerVirtualTable GetVoucherEmptyVirtualTable();

	Column GetBalanceEmptyVirtualTableColumn(string columnName);

	Column GetVoucherEmptyVirtualTableColumn(string columnName);
}
