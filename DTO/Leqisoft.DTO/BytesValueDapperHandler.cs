using System.Data;
using Dapper;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Leqisoft.DTO;

public class BytesValueDapperHandler : SqlMapper.TypeHandler<BytesValue>
{
	public override BytesValue Parse(object value)
	{
		if (value != null)
		{
			return new BytesValue
			{
				Value = ByteString.CopyFrom((byte[])value)
			};
		}
		return null;
	}

	public override void SetValue(IDbDataParameter parameter, BytesValue value)
	{
		parameter.Value = value?.Value?.ToByteArray();
	}
}
