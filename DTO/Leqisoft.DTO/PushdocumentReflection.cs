using System;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Leqisoft.DTO;

public static class PushdocumentReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static PushdocumentReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChJQdXNoZG9jdW1lbnQucHJvdG8SDExlcWlzb2Z0LkRUTxoOd3JhcHBlcnMu" + "cHJvdG8itwEKDFB1c2hEb2N1bWVudBIKCgJpZBgBIAEoAxIRCglwcm9qZWN0" + "SWQYAiABKAwSDwoHdmVyc2lvbhgDIAEoBRIMCgRtYXNrGAQgASgFEi8KCnBh" + "cmFncmFwaHMYBSADKAsyGy5MZXFpc29mdC5EVE8uUHVzaFBhcmFncmFwaBIO" + "CgZsb2NrZXIYECABKAMSDgoGc2VjdFByGBEgASgJEhIKCm1lcmdlVGFibGUY" + "EiABKANKBAgGEBAilwEKDVB1c2hQYXJhZ3JhcGgSCgoCaWQYASABKAMSDgoG" + "YWN0aW9uGAIgASgFEgwKBG1hc2sYAyABKAUSDQoFaW5kZXgYBCABKAUSDgoG" + "c3RyZWFtGAUgASgMEiwKB3NlY3Rpb24YBiABKAsyGy5nb29nbGUucHJvdG9i" + "dWYuQnl0ZXNWYWx1ZRIPCgdjb21tZW50GAcgASgJYgZwcm90bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[1] { WrappersReflection.Descriptor }, new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(PushDocument), PushDocument.Parser, new string[8] { "Id", "ProjectId", "Version", "Mask", "Paragraphs", "Locker", "SectPr", "MergeTable" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(PushParagraph), PushParagraph.Parser, new string[7] { "Id", "Action", "Mask", "Index", "Stream", "Section", "Comment" }, null, null, null, null)
		}));
	}
}
