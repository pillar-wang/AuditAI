﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿namespace Auditai.Util;

public class LoginInfo
{
	public LoginMode LoginMode { get; set; }

	public long userId { get; set; }

	public string userName { get; set; }

	public string password { get; set; }

	public string validateCode { get; set; }
}
