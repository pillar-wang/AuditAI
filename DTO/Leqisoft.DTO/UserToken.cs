using System;

namespace Leqisoft.DTO;

public class UserToken
{
	public long UserId { get; set; }

	public string LastToken { get; set; }

	public string TokenValue { get; set; }

	public string UpdateToken { get; set; }

	public DateTime UpdateTime { get; set; }

	public MachineCookie Cookie { get; set; }
}
