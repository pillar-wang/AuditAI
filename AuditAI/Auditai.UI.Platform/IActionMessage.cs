using System;
using Auditai.DTO;

namespace Auditai.UI.Platform;

public class IActionMessage : IMessage
{
	public Action<object> ActionDeal;

	public object Content { get; set; }
}
