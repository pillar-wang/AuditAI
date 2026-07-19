﻿using System;
using System.Data;
using Dapper;

namespace Auditai.DTO;

public class GuidDapperHandler : SqlMapper.TypeHandler<Guid>
{
	public override Guid Parse(object value)
	{
		switch (value)
		{
			case string s when Guid.TryParse(s, out var g):
				return g;
			case byte[] bytes when bytes.Length == 16:
				return new Guid(bytes);
			default:
				return Guid.Empty;
		}
	}

	public override void SetValue(IDbDataParameter parameter, Guid value)
	{
		parameter.Value = value.ToByteArray();
	}
}
