using System;
using System.IO;
using System.Linq;

namespace Auditai.PlatformResource;

public class PlatformTypeParser
{
	public static byte[] AuditPlatformKey = Guid.Parse("19B5C045-AB13-41EA-8573-D179BB50D52E").ToByteArray();

	public static byte[] EnterpriseReportPlatformKey = Guid.Parse("13533F52-6F09-4408-9A48-3F6FB5133D2A").ToByteArray();

	public static byte[] EnterpriseManagerPlatform = Guid.Parse("D298FECF-B89E-4743-B2C7-7A8AD6497EB9").ToByteArray();

	public static byte[] TableDevelopPlatform = Guid.Parse("7DF38648-2D8A-40CC-818C-9DBCB58F4C47").ToByteArray();

	public static byte[] ProductionCostAccountingSystem = Guid.Parse("99E56828-1BDB-4242-BA2D-9259D7A323C3").ToByteArray();

	public static byte[] ContractLedgerManagementSystem = Guid.Parse("DC860F51-68E2-4DF3-984C-0F76E64F736F").ToByteArray();

	public static byte[] RDExpenseLedgerSystem = Guid.Parse("42D443EF-A0EF-4BE8-8251-240D96C94999").ToByteArray();

	public static byte[] SalesOrderManagementSystem = Guid.Parse("6599A6A9-232F-44B1-B120-2C134D381739").ToByteArray();

	public static byte[] PSIManagementSystem = Guid.Parse("F508131C-364A-4CF4-815D-D02D49FC054F").ToByteArray();

	public static byte[] ProjectLedgerManagementSystem = Guid.Parse("3FC095FE-1C1B-4C19-AF3C-93ABBBC43BB8").ToByteArray();

	public static PlatformType ParsePlatformType(byte[] keyData)
	{
		if (keyData.SequenceEqual(AuditPlatformKey))
		{
			return PlatformType.AuditPlatform;
		}
		if (keyData.SequenceEqual(EnterpriseReportPlatformKey))
		{
			return PlatformType.EnterpriseReportPlatform;
		}
		if (keyData.SequenceEqual(EnterpriseManagerPlatform))
		{
			return PlatformType.EnterpriseManagerPlatform;
		}
		if (keyData.SequenceEqual(TableDevelopPlatform))
		{
			return PlatformType.TableDevelopPlatform;
		}
		if (keyData.SequenceEqual(ProductionCostAccountingSystem))
		{
			return PlatformType.ProductionCostAccountingSystem;
		}
		if (keyData.SequenceEqual(ContractLedgerManagementSystem))
		{
			return PlatformType.ContractLedgerManagementSystem;
		}
		if (keyData.SequenceEqual(RDExpenseLedgerSystem))
		{
			return PlatformType.RDExpenseLedgerSystem;
		}
		if (keyData.SequenceEqual(SalesOrderManagementSystem))
		{
			return PlatformType.SalesOrderManagementSystem;
		}
		if (keyData.SequenceEqual(PSIManagementSystem))
		{
			return PlatformType.PSIManagementSystem;
		}
		if (keyData.SequenceEqual(ProjectLedgerManagementSystem))
		{
			return PlatformType.ProjectLedgerManagementSystem;
		}
		ClientCustomizeData.LoadSettingFile();
		if (ClientCustomizeData.Current != null && ClientCustomizeData.Current.ClientTypeId.ToByteArray().SequenceEqual(keyData))
		{
			return PlatformType.Custom;
		}
		return PlatformType.UnKnown;
	}

	public static PlatformType ParsePlatformType(string keyFilePath)
	{
		return ParsePlatformType(File.ReadAllBytes(keyFilePath));
	}
}
