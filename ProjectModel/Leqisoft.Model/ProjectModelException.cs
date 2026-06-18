using System;

namespace Leqisoft.Model;

public class ProjectModelException : ApplicationException
{
	public ProjectModelException(string message)
		: base(message)
	{
	}
}
