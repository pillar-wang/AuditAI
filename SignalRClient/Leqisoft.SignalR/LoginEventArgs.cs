using System.Collections.Generic;

namespace Leqisoft.SignalR;

public class LoginEventArgs
{
	public IEnumerable<UserState> OnlineUsers { get; set; }
}
