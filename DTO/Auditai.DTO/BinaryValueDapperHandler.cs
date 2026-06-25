using System.Data;
using Dapper;

namespace Auditai.DTO;

public class BinaryValueDapperHandler : SqlMapper.TypeHandler<BinaryValue>
{
	public override BinaryValue Parse(object value)
	{
		return new BinaryValue((byte[])value);
	}

	public override void SetValue(IDbDataParameter parameter, BinaryValue value)
	{
		parameter.Value = value.GetBytes();
	}
}
