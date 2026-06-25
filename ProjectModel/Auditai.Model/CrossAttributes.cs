using Google.Protobuf;
using Auditai.DTO;

namespace Auditai.Model;

public class CrossAttributes
{
	public CrossRole Role { get; set; }

	public string Caption { get; set; } = string.Empty;


	public Id64 SrcColumn { get; set; }

	public void Deserialize(byte[] bin)
	{
		Auditai.DTO.CrossAttributes crossAttributes = Auditai.DTO.CrossAttributes.Parser.ParseFrom(bin);
		Role = (CrossRole)crossAttributes.Role;
		Caption = crossAttributes.Caption;
		SrcColumn = new Id64(crossAttributes.SrcColumn);
	}

	public byte[] Serialize()
	{
		Auditai.DTO.CrossAttributes message = new Auditai.DTO.CrossAttributes
		{
			Role = (int)Role,
			Caption = Caption,
			SrcColumn = SrcColumn.Value
		};
		return message.ToByteArray();
	}
}
