using System;

namespace Leqisoft.UI.Platform;

public class ChatRecord
{
	public string ChatId { get; set; }

	public string FromId { get; set; }

	public string Message { get; set; }

	public DateTime CreateTime { get; set; }

	public ChatRecord()
	{
	}

	public ChatRecord(string chatId, string fromId, string message)
	{
		ChatId = chatId;
		FromId = fromId;
		Message = message;
		CreateTime = DateTime.Now;
	}
}
