using System.Data;
using Dapper;
using Google.Protobuf.WellKnownTypes;

namespace Leqisoft.DTO;

public class Int64ValueDapperHandler : SqlMapper.TypeHandler<Int64Value>
{
	public override Int64Value Parse(object value)
	{
		if (value != null)
		{
			return new Int64Value
			{
				Value = (long)value
			};
		}
		return null;
	}

	public override void SetValue(IDbDataParameter parameter, Int64Value value)
	{
		parameter.Value = value?.Value;
	}
}
