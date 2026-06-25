using System;
using System.Collections.Generic;

namespace Auditai.Model;

public class Permission : ICloneable
{
	public bool GrantAll { get; set; } = true;


	public List<long> Users { get; set; } = new List<long>();


	public object Clone()
	{
		return new Permission
		{
			GrantAll = GrantAll,
			Users = new List<long>(Users)
		};
	}
}
