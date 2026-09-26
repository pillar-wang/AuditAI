using System.Collections.Generic;

namespace Auditai.Model;

public class RiskCheckScheme
{
	public long Id { get; set; }

	public string Name { get; set; }

	public string Note { get; set; }

	// 来源范围：0=本地自建、1=来自团队库、2=来自系统库
	public int SourceScope { get; set; }

	// 云端方案 id（来源库中的方案主键）；0=无来源（本地自建）
	public long SourceSchemeId { get; set; }

	// 导入时云端方案的版本号，用于日后与云端比对、判断是否需要"重新同步"
	public int SourceVersion { get; set; }

	public List<RiskCheckRule> Rules { get; } = new List<RiskCheckRule>();
}
