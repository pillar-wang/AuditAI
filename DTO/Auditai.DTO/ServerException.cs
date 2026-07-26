using System;

namespace Auditai.DTO;

public class ServerException : Exception
{
	public string ExceptionMessage { get; set; }

	public string ExceptionType { get; set; }

	public string ExceptionStackTrace { get; set; }

	public override string Message => ExceptionMessage ?? base.Message;

	public override string ToString()
	{
		return "[" + ExceptionType + "] " + ExceptionMessage + "\n" + ExceptionStackTrace;
	}
}
