using System;
using System.Runtime.CompilerServices;

namespace Leqisoft.Model;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class OrderAttribute : Attribute
{
	public int Order { get; }

	public OrderAttribute([CallerLineNumber] int order = 0)
	{
		Order = order;
	}
}
