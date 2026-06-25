using System.Collections.Generic;

namespace Auditai.SignalR;

public class LoginEventArgs
{
	public IEnumerable<UserState> OnlineUsers { get; set; }
}
