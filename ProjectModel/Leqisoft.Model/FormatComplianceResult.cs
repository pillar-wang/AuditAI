﻿﻿﻿﻿﻿﻿﻿﻿﻿using Leqisoft.DTO;

namespace Leqisoft.Model;

public class FormatComplianceResult
{
	public Id64 RuleId { get; set; }

	public bool Passed { get; set; }

	public string Message { get; set; }

	public int Position { get; set; }
}
