using System;
using Google.Protobuf.Reflection;

namespace Leqisoft.DTO;

public static class PulltableReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PulltableReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("Cg9wdWxsdGFibGUucHJvdG8SDExlcWlzb2Z0LkRUTyIdCgxPcHRpb25hbEJv" + "b2wSDQoFdmFsdWUYASABKAgiHgoNT3B0aW9uYWxJbnQzMhINCgV2YWx1ZRgB" + "IAEoBSIeCg1PcHRpb25hbEludDY0Eg0KBXZhbHVlGAEgASgDIh4KDU9wdGlv" + "bmFsRmxvYXQSDQoFdmFsdWUYASABKAIiHgoNT3B0aW9uYWxCeXRlcxINCgV2" + "YWx1ZRgBIAEoDCIfCg5PcHRpb25hbFN0cmluZxINCgV2YWx1ZRgBIAEoCSIt" + "CgxOdWxsYWJsZUJvb2wSDgoGaXNOdWxsGAEgASgIEg0KBXZhbHVlGAIgASgI" + "Ii4KDU51bGxhYmxlSW50MzISDgoGaXNOdWxsGAEgASgIEg0KBXZhbHVlGAIg" + "ASgFIi4KDU51bGxhYmxlSW50NjQSDgoGaXNOdWxsGAEgASgIEg0KBXZhbHVl" + "GAIgASgDIi4KDU51bGxhYmxlRmxvYXQSDgoGaXNOdWxsGAEgASgIEg0KBXZh" + "bHVlGAIgASgCIi8KDk51bGxhYmxlU3RyaW5nEg4KBmlzTnVsbBgBIAEoCBIN" + "CgV2YWx1ZRgCIAEoCSIuCg1OdWxsYWJsZUJ5dGVzEg4KBmlzTnVsbBgBIAEo" + "CBINCgV2YWx1ZRgCIAEoDCLqCwoJUHVsbFRhYmxlEigKCG5ld0NlbGxzGAEg" + "AygLMhYuTGVxaXNvZnQuRFRPLlB1bGxDZWxsEigKCGRlbENlbGxzGAIgAygL" + "MhYuTGVxaXNvZnQuRFRPLlB1bGxDZWxsEigKCG1vZENlbGxzGAMgAygLMhYu" + "TGVxaXNvZnQuRFRPLlB1bGxDZWxsEiwKCm5ld0NvbHVtbnMYBCADKAsyGC5M" + "ZXFpc29mdC5EVE8uUHVsbENvbHVtbhIsCgpkZWxDb2x1bW5zGAUgAygLMhgu" + "TGVxaXNvZnQuRFRPLlB1bGxDb2x1bW4SLAoKbW9kQ29sdW1ucxgGIAMoCzIY" + "LkxlcWlzb2Z0LkRUTy5QdWxsQ29sdW1uEiYKB25ld1Jvd3MYByADKAsyFS5M" + "ZXFpc29mdC5EVE8uUHVsbFJvdxImCgdkZWxSb3dzGAggAygLMhUuTGVxaXNv" + "ZnQuRFRPLlB1bGxSb3cSJgoHbW9kUm93cxgJIAMoCzIVLkxlcWlzb2Z0LkRU" + "Ty5QdWxsUm93Ei8KCmNlbGxTdHlsZXMYCiADKAsyGy5MZXFpc29mdC5EVE8u" + "UHVsbENlbGxTdHlsZRIqCgluZXdNZXJnZXMYCyADKAsyFy5MZXFpc29mdC5E" + "VE8uUHVsbE1lcmdlEioKCWRlbE1lcmdlcxgMIAMoCzIXLkxlcWlzb2Z0LkRU" + "Ty5QdWxsTWVyZ2USMAoMbmV3Q2VsbFByb3BzGA0gAygLMhouTGVxaXNvZnQu" + "RFRPLlB1bGxDZWxsUHJvcBIwCgxtb2RDZWxsUHJvcHMYDiADKAsyGi5MZXFp" + "c29mdC5EVE8uUHVsbENlbGxQcm9wEjAKDGRlbENlbGxQcm9wcxgPIAMoCzIa" + "LkxlcWlzb2Z0LkRUTy5QdWxsQ2VsbFByb3ASKwoFdGl0bGUYECABKAsyHC5M" + "ZXFpc29mdC5EVE8uT3B0aW9uYWxTdHJpbmcSKgoEbm90ZRgRIAEoCzIcLkxl" + "cWlzb2Z0LkRUTy5PcHRpb25hbFN0cmluZxIzCg1oZWFkZXJIZWlnaHRzGBIg" + "ASgLMhwuTGVxaXNvZnQuRFRPLk9wdGlvbmFsU3RyaW5nEjMKDmRlZmF1bHRT" + "dHlsZUlkGBMgASgLMhsuTGVxaXNvZnQuRFRPLk51bGxhYmxlSW50NjQSLwoJ" + "cGFnZVNldHVwGBQgASgLMhwuTGVxaXNvZnQuRFRPLk9wdGlvbmFsU3RyaW5n" + "EjkKE2NvbnNvbGlkYXRlU2V0dGluZ3MYFSABKAsyHC5MZXFpc29mdC5EVE8u" + "T3B0aW9uYWxTdHJpbmcSMAoLYm9yZGVyU3R5bGUYFiABKAsyGy5MZXFpc29m" + "dC5EVE8uT3B0aW9uYWxJbnQzMhIvCgpmcm96ZW5Db2xzGBcgASgLMhsuTGVx" + "aXNvZnQuRFRPLk9wdGlvbmFsSW50MzISLwoKaGVhZGVyTW9kZRgYIAEoCzIb" + "LkxlcWlzb2Z0LkRUTy5PcHRpb25hbEludDMyEjMKDWNvbGxlY3RTb3VyY2UY" + "GSABKAsyHC5MZXFpc29mdC5EVE8uT3B0aW9uYWxTdHJpbmcSKwoGbG9ja2Vy" + "GBogASgLMhsuTGVxaXNvZnQuRFRPLk9wdGlvbmFsSW50NjQSMAoKZmlsdGVy" + "SW5mbxgbIAEoCzIcLkxlcWlzb2Z0LkRUTy5PcHRpb25hbFN0cmluZxIqCgRm" + "b290GBwgASgLMhwuTGVxaXNvZnQuRFRPLk9wdGlvbmFsU3RyaW5nEjYKEXJv" + "d093bmVyTG9hZFNoYXJlGB0gASgLMhsuTGVxaXNvZnQuRFRPLk9wdGlvbmFs" + "Qnl0ZXMSLAoGdGlja2V0GB4gASgLMhwuTGVxaXNvZnQuRFRPLk9wdGlvbmFs" + "U3RyaW5nEjQKDmNvbnRyb2xGb3JtdWxhGB8gASgLMhwuTGVxaXNvZnQuRFRP" + "Lk9wdGlvbmFsU3RyaW5nEg4KBnJlc3VsdBggIAEoCRIPCgd2ZXJzaW9uGCEg" + "ASgFIuwECgpQdWxsQ29sdW1uEgoKAmlkGAEgASgDEi0KB2NhcHRpb24YAiAB" + "KAsyHC5MZXFpc29mdC5EVE8uT3B0aW9uYWxTdHJpbmcSKgoFaW5kZXgYAyAB" + "KAsyGy5MZXFpc29mdC5EVE8uT3B0aW9uYWxJbnQzMhIqCgV3aWR0aBgEIAEo" + "CzIbLkxlcWlzb2Z0LkRUTy5PcHRpb25hbEludDMyEisKB3Zpc2libGUYBSAB" + "KAsyGi5MZXFpc29mdC5EVE8uT3B0aW9uYWxCb29sEiwKB3N0eWxlSWQYBiAB" + "KAsyGy5MZXFpc29mdC5EVE8uTnVsbGFibGVJbnQ2NBIyCgxjYXB0aW9uU3R5" + "bGUYByABKAsyHC5MZXFpc29mdC5EVE8uT3B0aW9uYWxTdHJpbmcSOAoSY29u" + "c29saWRhdGVBdHRyaWJzGAggASgLMhwuTGVxaXNvZnQuRFRPLk9wdGlvbmFs" + "U3RyaW5nEi0KB2Zvcm11bGEYCSABKAsyHC5MZXFpc29mdC5EVE8uT3B0aW9u" + "YWxTdHJpbmcSNAoPc3VidG90YWxBdHRyaWJzGAogASgLMhsuTGVxaXNvZnQu" + "RFRPLk9wdGlvbmFsSW50MzISMQoLcGVybWlzc2lvbnMYCyABKAsyHC5MZXFp" + "c29mdC5EVE8uT3B0aW9uYWxTdHJpbmcSNAoOY2FwdGlvbkZvcm11bGEYDCAB" + "KAsyHC5MZXFpc29mdC5EVE8uT3B0aW9uYWxTdHJpbmcSNAoPY3Jvc3NBdHRy" + "aWJ1dGVzGA0gASgLMhsuTGVxaXNvZnQuRFRPLk9wdGlvbmFsQnl0ZXMi1AIK" + "B1B1bGxSb3cSCgoCaWQYASABKAMSKgoFaW5kZXgYAiABKAsyGy5MZXFpc29m" + "dC5EVE8uT3B0aW9uYWxJbnQzMhIrCgZoZWlnaHQYAyABKAsyGy5MZXFpc29m" + "dC5EVE8uT3B0aW9uYWxJbnQzMhIrCgd2aXNpYmxlGAQgASgLMhouTGVxaXNv" + "ZnQuRFRPLk9wdGlvbmFsQm9vbBIrCgZsb2NrZXIYBSABKAsyGy5MZXFpc29m" + "dC5EVE8uT3B0aW9uYWxJbnQ2NBIpCgRyb2xlGAYgASgLMhsuTGVxaXNvZnQu" + "RFRPLk9wdGlvbmFsSW50MzISMQoLcGVybWlzc2lvbnMYByABKAsyHC5MZXFp" + "c29mdC5EVE8uT3B0aW9uYWxTdHJpbmcSLAoHY3JlYXRvchgIIAEoCzIbLkxl" + "cWlzb2Z0LkRUTy5PcHRpb25hbEludDY0ItsCCghQdWxsQ2VsbBIKCgJpZBgB" + "IAEoAxIoCgNjSWQYAiABKAsyGy5MZXFpc29mdC5EVE8uT3B0aW9uYWxJbnQ2" + "NBIoCgNySWQYAyABKAsyGy5MZXFpc29mdC5EVE8uT3B0aW9uYWxJbnQ2NBIq" + "CgV2YWx1ZRgEIAEoCzIbLkxlcWlzb2Z0LkRUTy5PcHRpb25hbEJ5dGVzEi0K" + "B2Zvcm11bGEYBSABKAsyHC5MZXFpc29mdC5EVE8uT3B0aW9uYWxTdHJpbmcS" + "KgoFc3R5bGUYBiABKAsyGy5MZXFpc29mdC5EVE8uTnVsbGFibGVJbnQ2NBIz" + "Cg1jb2xsZWN0U291cmNlGAcgASgLMhwuTGVxaXNvZnQuRFRPLk9wdGlvbmFs" + "U3RyaW5nEjMKDWhlYWRlckZvcm11bGEYCCABKAsyHC5MZXFpc29mdC5EVE8u" + "T3B0aW9uYWxTdHJpbmcipwUKDVB1bGxDZWxsU3R5bGUSCgoCaWQYASABKAMS" + "MAoKZm9udEZhbWlseRgCIAEoCzIcLkxlcWlzb2Z0LkRUTy5OdWxsYWJsZVN0" + "cmluZxItCghmb250U2l6ZRgDIAEoCzIbLkxlcWlzb2Z0LkRUTy5OdWxsYWJs" + "ZUZsb2F0Ei4KCWZvcmVDb2xvchgEIAEoCzIbLkxlcWlzb2Z0LkRUTy5OdWxs" + "YWJsZUludDMyEi4KCWJhY2tDb2xvchgFIAEoCzIbLkxlcWlzb2Z0LkRUTy5O" + "dWxsYWJsZUludDMyEioKBWFsaWduGAYgASgLMhsuTGVxaXNvZnQuRFRPLk51" + "bGxhYmxlSW50MzISKwoGbWFyZ2luGAcgASgLMhsuTGVxaXNvZnQuRFRPLk51" + "bGxhYmxlSW50MzISKAoEYm9sZBgIIAEoCzIaLkxlcWlzb2Z0LkRUTy5OdWxs" + "YWJsZUJvb2wSKgoGaXRhbGljGAkgASgLMhouTGVxaXNvZnQuRFRPLk51bGxh" + "YmxlQm9vbBItCgl1bmRlcmxpbmUYCiABKAsyGi5MZXFpc29mdC5EVE8uTnVs" + "bGFibGVCb29sEi0KCGRhdGFUeXBlGAsgASgLMhsuTGVxaXNvZnQuRFRPLk51" + "bGxhYmxlSW50MzISLAoGZm9ybWF0GAwgASgLMhwuTGVxaXNvZnQuRFRPLk51" + "bGxhYmxlU3RyaW5nEisKBmxvY2tlchgNIAEoCzIbLkxlcWlzb2Z0LkRUTy5O" + "dWxsYWJsZUludDY0EjIKDGRlZmF1bHRWYWx1ZRgOIAEoCzIcLkxlcWlzb2Z0" + "LkRUTy5OdWxsYWJsZVN0cmluZxItCgdjb21tZW50GA8gASgLMhwuTGVxaXNv" + "ZnQuRFRPLk51bGxhYmxlU3RyaW5nIncKCVB1bGxNZXJnZRIKCgJpZBgBIAEo" + "AxIsCgd0b3BMZWZ0GAIgASgLMhsuTGVxaXNvZnQuRFRPLk9wdGlvbmFsSW50" + "NjQSMAoLYm90dG9tUmlnaHQYAyABKAsyGy5MZXFpc29mdC5EVE8uT3B0aW9u" + "YWxJbnQ2NCJQCgxQdWxsQ2VsbFByb3ASDgoGY2VsbElkGAEgASgDEjAKC2F0" + "dGFjaG1lbnRzGAIgASgLMhsuTGVxaXNvZnQuRFRPLk9wdGlvbmFsQnl0ZXNi" + "BnByb3RvMw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[19]
		{
			new GeneratedClrTypeInfo(typeof(OptionalBool), OptionalBool.Parser, new string[1] { "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(OptionalInt32), OptionalInt32.Parser, new string[1] { "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(OptionalInt64), OptionalInt64.Parser, new string[1] { "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(OptionalFloat), OptionalFloat.Parser, new string[1] { "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(OptionalBytes), OptionalBytes.Parser, new string[1] { "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(OptionalString), OptionalString.Parser, new string[1] { "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(NullableBool), NullableBool.Parser, new string[2] { "IsNull", "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(NullableInt32), NullableInt32.Parser, new string[2] { "IsNull", "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(NullableInt64), NullableInt64.Parser, new string[2] { "IsNull", "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(NullableFloat), NullableFloat.Parser, new string[2] { "IsNull", "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(NullableString), NullableString.Parser, new string[2] { "IsNull", "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(NullableBytes), NullableBytes.Parser, new string[2] { "IsNull", "Value" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PullTable), PullTable.Parser, new string[33]
			{
				"NewCells", "DelCells", "ModCells", "NewColumns", "DelColumns", "ModColumns", "NewRows", "DelRows", "ModRows", "CellStyles",
				"NewMerges", "DelMerges", "NewCellProps", "ModCellProps", "DelCellProps", "Title", "Note", "HeaderHeights", "DefaultStyleId", "PageSetup",
				"ConsolidateSettings", "BorderStyle", "FrozenCols", "HeaderMode", "CollectSource", "Locker", "FilterInfo", "Foot", "RowOwnerLoadShare", "Ticket",
				"ControlFormula", "Result", "Version"
			}, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PullColumn), PullColumn.Parser, new string[13]
			{
				"Id", "Caption", "Index", "Width", "Visible", "StyleId", "CaptionStyle", "ConsolidateAttribs", "Formula", "SubtotalAttribs",
				"Permissions", "CaptionFormula", "CrossAttributes"
			}, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PullRow), PullRow.Parser, new string[8] { "Id", "Index", "Height", "Visible", "Locker", "Role", "Permissions", "Creator" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PullCell), PullCell.Parser, new string[8] { "Id", "CId", "RId", "Value", "Formula", "Style", "CollectSource", "HeaderFormula" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PullCellStyle), PullCellStyle.Parser, new string[15]
			{
				"Id", "FontFamily", "FontSize", "ForeColor", "BackColor", "Align", "Margin", "Bold", "Italic", "Underline",
				"DataType", "Format", "Locker", "DefaultValue", "Comment"
			}, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PullMerge), PullMerge.Parser, new string[3] { "Id", "TopLeft", "BottomRight" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PullCellProp), PullCellProp.Parser, new string[2] { "CellId", "Attachments" }, null, null, null, null)
		}));
	}
}
