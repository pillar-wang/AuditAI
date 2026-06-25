using System;

namespace Auditai.Model;

public class ProjectModelException : ApplicationException
{
	public ProjectModelException(string message)
		: base(message)
	{
	}
}
