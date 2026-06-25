using System;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public static class PullDocumentReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PullDocumentReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChJwdWxsRG9jdW1lbnQucHJvdG8SDExlcWlzb2Z0LkRUTxoPcHVsbHRhYmxl" + "LnByb3RvIt0CCgxQdWxsRG9jdW1lbnQSMgoNbmV3UGFyYWdyYXBocxgBIAMo" + "CzIbLkxlcWlzb2Z0LkRUTy5QdWxsUGFyYWdyYXBoEjIKDWRlbFBhcmFncmFw" + "aHMYAiADKAsyGy5MZXFpc29mdC5EVE8uUHVsbFBhcmFncmFwaBIyCg1tb2RQ" + "YXJhZ3JhcGhzGAMgAygLMhsuTGVxaXNvZnQuRFRPLlB1bGxQYXJhZ3JhcGgS" + "KwoGbG9ja2VyGBAgASgLMhsuTGVxaXNvZnQuRFRPLk9wdGlvbmFsSW50NjQS" + "LAoGc2VjdFByGBEgASgLMhwuTGVxaXNvZnQuRFRPLk9wdGlvbmFsU3RyaW5n" + "Ei8KCm1lcmdlVGFibGUYEiABKAsyGy5MZXFpc29mdC5EVE8uT3B0aW9uYWxJ" + "bnQ2NBIOCgZyZXN1bHQYEyABKAkSDwoHdmVyc2lvbhgUIAEoBUoECAQQECLR" + "AQoNUHVsbFBhcmFncmFwaBIKCgJpZBgBIAEoAxIqCgVpbmRleBgCIAEoCzIb" + "LkxlcWlzb2Z0LkRUTy5PcHRpb25hbEludDMyEisKBnN0cmVhbRgDIAEoCzIb" + "LkxlcWlzb2Z0LkRUTy5PcHRpb25hbEJ5dGVzEiwKB3NlY3Rpb24YBCABKAsy" + "Gy5MZXFpc29mdC5EVE8uTnVsbGFibGVCeXRlcxItCgdjb21tZW50GAUgASgL" + "MhwuTGVxaXNvZnQuRFRPLk9wdGlvbmFsU3RyaW5nYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { PulltableReflection.Descriptor }, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(PullDocument), PullDocument.Parser, new string[8] { "NewParagraphs", "DelParagraphs", "ModParagraphs", "Locker", "SectPr", "MergeTable", "Result", "Version" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PullParagraph), PullParagraph.Parser, new string[5] { "Id", "Index", "Stream", "Section", "Comment" }, null, null, null, null)
		}));
	}
}
