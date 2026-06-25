using System;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Auditai.DTO;

public static class PushtableReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PushtableReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("Cg9wdXNodGFibGUucHJvdG8SDExlcWlzb2Z0LkRUTxoOd3JhcHBlcnMucHJv" + "dG8i0QUKCVB1c2hUYWJsZRIKCgJpZBgBIAEoAxIRCglwcm9qZWN0SWQYAiAB" + "KAwSDwoHdmVyc2lvbhgDIAEoBRIMCgRtYXNrGAQgASgFEikKB2NvbHVtbnMY" + "BSADKAsyGC5MZXFpc29mdC5EVE8uUHVzaENvbHVtbhIjCgRyb3dzGAYgAygL" + "MhUuTGVxaXNvZnQuRFRPLlB1c2hSb3cSJQoFY2VsbHMYByADKAsyFi5MZXFp" + "c29mdC5EVE8uUHVzaENlbGwSLwoKY2VsbFN0eWxlcxgIIAMoCzIbLkxlcWlz" + "b2Z0LkRUTy5QdXNoQ2VsbFN0eWxlEicKBm1lcmdlcxgJIAMoCzIXLkxlcWlz" + "b2Z0LkRUTy5QdXNoTWVyZ2USOQoPY2VsbEF0dGFjaG1lbnRzGAogAygLMiAu" + "TGVxaXNvZnQuRFRPLlB1c2hDZWxsQXR0YWNobWVudBINCgV0aXRsZRgQIAEo" + "CRIMCgRub3RlGBEgASgJEhUKDWhlYWRlckhlaWdodHMYEiABKAkSFgoOZGVm" + "YXVsdFN0eWxlSWQYEyABKAMSEQoJcGFnZVNldHVwGBQgASgJEhsKE2NvbnNv" + "bGlkYXRlU2V0dGluZ3MYFSABKAkSEwoLYm9yZGVyU3R5bGUYFiABKAUSEgoK" + "ZnJvemVuQ29scxgXIAEoBRISCgpoZWFkZXJNb2RlGBggASgFEhUKDWNvbGxl" + "Y3RTb3VyY2UYGSABKAkSDgoGbG9ja2VyGBogASgDEhIKCmZpbHRlckluZm8Y" + "GyABKAkSDAoEZm9vdBgcIAEoCRIZChFyb3dPd25lckV4Y2x1c2l2ZRgdIAEo" + "CBIUCgxyb3dPd25lckxvYWQYHiABKAgSGQoRcm93T3duZXJMb2FkU2hhcmUY" + "HyABKAwSDgoGdGlja2V0GCAgASgJEhYKDmNvbnRyb2xGb3JtdWxhGCEgASgJ" + "SgQICxAQIswCCgpQdXNoQ29sdW1uEgoKAmlkGAEgASgDEg4KBmFjdGlvbhgC" + "IAEoBRIMCgRtYXNrGAMgASgFEg0KBXdpZHRoGAQgASgFEg0KBWluZGV4GAUg" + "ASgFEg8KB2NhcHRpb24YBiABKAkSDwoHZm9ybXVsYRgHIAEoCRIPCgd2aXNp" + "YmxlGBAgASgIEiwKB3N0eWxlSWQYESABKAsyGy5nb29nbGUucHJvdG9idWYu" + "SW50NjRWYWx1ZRIUCgxjYXB0aW9uU3R5bGUYEiABKAkSGgoSY29uc29saWRh" + "dGVBdHRyaWJzGBMgASgJEhcKD3N1YnRvdGFsQXR0cmlicxgUIAEoBRITCgtw" + "ZXJtaXNzaW9ucxgVIAEoCRIWCg5jYXB0aW9uRm9ybXVsYRgWIAEoCRIXCg9j" + "cm9zc0F0dHJpYnV0ZXMYFyABKAxKBAgIEBAirQEKB1B1c2hSb3cSCgoCaWQY" + "ASABKAMSDgoGYWN0aW9uGAIgASgFEgwKBG1hc2sYAyABKAUSDgoGaGVpZ2h0" + "GAQgASgFEg0KBWluZGV4GAUgASgFEg8KB3Zpc2libGUYECABKAgSDgoGbG9j" + "a2VyGBEgASgDEgwKBHJvbGUYEiABKAUSEwoLcGVybWlzc2lvbnMYEyABKAkS" + "DwoHY3JlYXRvchgUIAEoA0oECAYQECLQAQoIUHVzaENlbGwSCgoCaWQYASAB" + "KAMSDgoGYWN0aW9uGAIgASgFEgwKBG1hc2sYAyABKAUSCwoDY0lkGAQgASgD" + "EgsKA3JJZBgFIAEoAxINCgV2YWx1ZRgGIAEoDBIPCgdmb3JtdWxhGAcgASgJ" + "EiwKB3N0eWxlSWQYCCABKAsyGy5nb29nbGUucHJvdG9idWYuSW50NjRWYWx1" + "ZRIVCg1jb2xsZWN0U291cmNlGBAgASgJEhUKDWhlYWRlckZvcm11bGEYESAB" + "KAlKBAgJEBAipAIKDVB1c2hDZWxsU3R5bGUSCgoCaWQYASABKAMSDAoEbWFz" + "axgCIAEoBRISCgpmb250RmFtaWx5GAMgASgJEhAKCGZvbnRTaXplGAQgASgC" + "EhEKCWZvcmVDb2xvchgFIAEoBRIRCgliYWNrQ29sb3IYBiABKAUSDQoFYWxp" + "Z24YByABKAUSDgoGbWFyZ2luGAggASgFEgwKBGJvbGQYCSABKAgSDgoGaXRh" + "bGljGAogASgIEhEKCXVuZGVybGluZRgLIAEoCBIQCghkYXRhVHlwZRgMIAEo" + "BRIOCgZmb3JtYXQYDSABKAkSDgoGbG9ja2VyGBAgASgDEhQKDGRlZmF1bHRW" + "YWx1ZRgRIAEoCRIPCgdjb21tZW50GBIgASgJSgQIDhAQIk0KCVB1c2hNZXJn" + "ZRIKCgJpZBgBIAEoAxIOCgZhY3Rpb24YAiABKAUSDwoHdG9wTGVmdBgDIAEo" + "AxITCgtib3R0b21SaWdodBgEIAEoAyJoChJQdXNoQ2VsbEF0dGFjaG1lbnQS" + "DwoHdGFibGVJZBgBIAEoAxIOCgZjZWxsSWQYAiABKAMSDgoGYWN0aW9uGAMg" + "ASgFEgwKBG1hc2sYBCABKAUSEwoLYXR0YWNobWVudHMYBSABKAxiBnByb3Rv" + "Mw==");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { WrappersReflection.Descriptor }, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[7]
		{
			new GeneratedClrTypeInfo(typeof(PushTable), PushTable.Parser, new string[28]
			{
				"Id", "ProjectId", "Version", "Mask", "Columns", "Rows", "Cells", "CellStyles", "Merges", "CellAttachments",
				"Title", "Note", "HeaderHeights", "DefaultStyleId", "PageSetup", "ConsolidateSettings", "BorderStyle", "FrozenCols", "HeaderMode", "CollectSource",
				"Locker", "FilterInfo", "Foot", "RowOwnerExclusive", "RowOwnerLoad", "RowOwnerLoadShare", "Ticket", "ControlFormula"
			}, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PushColumn), PushColumn.Parser, new string[15]
			{
				"Id", "Action", "Mask", "Width", "Index", "Caption", "Formula", "Visible", "StyleId", "CaptionStyle",
				"ConsolidateAttribs", "SubtotalAttribs", "Permissions", "CaptionFormula", "CrossAttributes"
			}, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PushRow), PushRow.Parser, new string[10] { "Id", "Action", "Mask", "Height", "Index", "Visible", "Locker", "Role", "Permissions", "Creator" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PushCell), PushCell.Parser, new string[10] { "Id", "Action", "Mask", "CId", "RId", "Value", "Formula", "StyleId", "CollectSource", "HeaderFormula" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PushCellStyle), PushCellStyle.Parser, new string[16]
			{
				"Id", "Mask", "FontFamily", "FontSize", "ForeColor", "BackColor", "Align", "Margin", "Bold", "Italic",
				"Underline", "DataType", "Format", "Locker", "DefaultValue", "Comment"
			}, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PushMerge), PushMerge.Parser, new string[4] { "Id", "Action", "TopLeft", "BottomRight" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PushCellAttachment), PushCellAttachment.Parser, new string[5] { "TableId", "CellId", "Action", "Mask", "Attachments" }, null, null, null, null)
		}));
	}
}
