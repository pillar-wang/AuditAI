using System;

namespace CrawlerForm;

public class NoticeMessage
{
	public string Title { get; set; }

	public string Message { get; set; }

	public Exception Exception { get; set; }

	public NoticeMessage(string message, Exception ex)
	{
		Message = message;
		Exception = ex;
	}

	public NoticeMessage(string title, string message)
	{
		Title = title;
		Message = message;
	}
}
