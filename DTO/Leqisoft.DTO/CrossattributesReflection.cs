using System;
using Google.Protobuf.Reflection;

namespace Leqisoft.DTO;

public static class CrossattributesReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static CrossattributesReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChVjcm9zc2F0dHJpYnV0ZXMucHJvdG8SDExlcWlzb2Z0LkRUTyJDCg9Dcm9z" + "c0F0dHJpYnV0ZXMSDAoEUm9sZRgBIAEoBRIPCgdDYXB0aW9uGAIgASgJEhEK" + "CVNyY0NvbHVtbhgDIAEoA2IGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[1]
		{
			new GeneratedClrTypeInfo(typeof(CrossAttributes), CrossAttributes.Parser, new string[3] { "Role", "Caption", "SrcColumn" }, null, null, null, null)
		}));
	}
}
