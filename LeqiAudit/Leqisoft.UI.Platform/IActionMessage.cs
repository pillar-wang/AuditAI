using System;
using Leqisoft.DTO;

namespace Leqisoft.UI.Platform;

public class IActionMessage : IMessage
{
	public Action<object> ActionDeal;

	public object Content { get; set; }
}
