using Google.Protobuf;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class CrossAttributes
{
	public CrossRole Role { get; set; }

	public string Caption { get; set; } = string.Empty;


	public Id64 SrcColumn { get; set; }

	public void Deserialize(byte[] bin)
	{
		Leqisoft.DTO.CrossAttributes crossAttributes = Leqisoft.DTO.CrossAttributes.Parser.ParseFrom(bin);
		Role = (CrossRole)crossAttributes.Role;
		Caption = crossAttributes.Caption;
		SrcColumn = new Id64(crossAttributes.SrcColumn);
	}

	public byte[] Serialize()
	{
		Leqisoft.DTO.CrossAttributes message = new Leqisoft.DTO.CrossAttributes
		{
			Role = (int)Role,
			Caption = Caption,
			SrcColumn = SrcColumn.Value
		};
		return message.ToByteArray();
	}
}
