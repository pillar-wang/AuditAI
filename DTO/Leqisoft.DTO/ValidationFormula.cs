﻿﻿﻿﻿﻿﻿namespace Leqisoft.DTO;

public class ValidationFormula
{
	public Id64 Id { get; set; }

	public string LeftExpr { get; set; }

	public int Operator { get; set; }

	public string RightExpr { get; set; }

	public string Note { get; set; }

	public int Dirty { get; set; }

	public int Status { get; set; }

	public Id64 TableId { get; set; }

	/// <summary>
	/// 标识关联的文档域 Id。为 0/null 表示该公式绑定到表格（TableId）而非文档域。
	/// </summary>
	public Id64 DocumentFieldId { get; set; }
}
