using System.Data;
using Dapper;

namespace Auditai.DTO;

public class Id64DapperHandler : SqlMapper.TypeHandler<Id64>
{
	public override Id64 Parse(object value)
	{
		return new Id64((long)value);
	}

	public override void SetValue(IDbDataParameter parameter, Id64 value)
	{
		parameter.Value = value.Value;
	}
}
