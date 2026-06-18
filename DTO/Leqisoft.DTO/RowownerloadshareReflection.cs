using System;
using Google.Protobuf.Reflection;

namespace Leqisoft.DTO;

public static class RowownerloadshareReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static RowownerloadshareReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("Chdyb3dvd25lcmxvYWRzaGFyZS5wcm90bxIMTGVxaXNvZnQuRFRPIkoKEVJv" + "d093bmVyTG9hZFNoYXJlEjUKB0VudHJpZXMYASADKAsyJC5MZXFpc29mdC5E" + "VE8uUm93T3duZXJMb2FkU2hhcmVFbnRyeSI5ChZSb3dPd25lckxvYWRTaGFy" + "ZUVudHJ5Eg8KB2NyZWF0b3IYASABKAMSDgoGc2hhcmVkGAIgAygDYgZwcm90" + "bzM=");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(RowOwnerLoadShare), RowOwnerLoadShare.Parser, new string[1] { "Entries" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(RowOwnerLoadShareEntry), RowOwnerLoadShareEntry.Parser, new string[2] { "Creator", "Shared" }, null, null, null, null)
		}));
	}
}
