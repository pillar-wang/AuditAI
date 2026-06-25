﻿﻿﻿﻿﻿﻿﻿namespace Auditai.DTO;

public class FormatComplianceRule
{
	public Id64 Id { get; set; }

	public int RuleType { get; set; }

	public string Pattern { get; set; }

	public string Note { get; set; }

	public int Dirty { get; set; }

	public int Status { get; set; }
}
