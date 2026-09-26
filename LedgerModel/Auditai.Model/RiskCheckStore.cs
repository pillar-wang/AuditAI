using System.Collections.Generic;

namespace Auditai.Model;

// 账务数据风险检查——方案存取门面。
// LedgerDAL 是 internal，LedgerViewer2 等 UI 程序集无法直接访问，统一经本门面调用。
// dbPath 即账套文件路径（Ledger.LoadFromFile 的 SQLite 数据库文件）。
public static class RiskCheckStore
{
	// 读取全部方案（含各方案的检查项列表）；连接不存在时幂等建表，返回空列表
	public static List<RiskCheckScheme> Load(string ledgerDbPath)
	{
		return new LedgerDAL(ledgerDbPath).GetRiskCheckSchemes(ledgerDbPath);
	}

	// 新增/更新方案（规则全删全插）。scheme.Id<=0 时自动分配 MAX+1，
	// 分配结果回写到传入对象（rule.Id / rule.SchemeId 同样回写），调用方保存后直接读 scheme.Id。
	public static void Save(string ledgerDbPath, RiskCheckScheme scheme)
	{
		new LedgerDAL(ledgerDbPath).SaveRiskCheckScheme(ledgerDbPath, scheme);
	}

	// 删除方案及其全部检查项
	public static void Delete(string ledgerDbPath, long schemeId)
	{
		new LedgerDAL(ledgerDbPath).DeleteRiskCheckScheme(ledgerDbPath, schemeId);
	}

	// 序列化为 JSON（导出）
	public static string ExportJson(List<RiskCheckScheme> schemes)
	{
		return LedgerDAL.ExportRiskCheckSchemesJson(schemes);
	}

	// 从 JSON 反序列化（导入）；调用方应将 scheme.Id / rule.Id 置 0 后 Save，避免顶掉现有数据
	public static List<RiskCheckScheme> ImportJson(string json)
	{
		return LedgerDAL.ImportRiskCheckSchemesJson(json);
	}
}
