using System;

namespace Auditai.Model;

[AttributeUsage(AttributeTargets.Parameter)]
public class ParameterNameAttribute : Attribute
{
	public string Name { get; }

	public ParameterNameAttribute(string name)
	{
		Name = name;
	}
}
