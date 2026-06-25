using System;
using Google.Protobuf.Reflection;

namespace Auditai.DTO;

public static class CellattachmentsReflection
{
	private static FileDescriptor descriptor;

	public static FileDescriptor Descriptor => descriptor;

	static CellattachmentsReflection()
	{
		byte[] descriptorData = Convert.FromBase64String("ChVjZWxsYXR0YWNobWVudHMucHJvdG8SDExlcWlzb2Z0LkRUTyJFCg9DZWxs" + "QXR0YWNobWVudHMSMgoHRW50cmllcxgBIAMoCzIhLkxlcWlzb2Z0LkRUTy5D" + "ZWxsQXR0YWNobWVudEVudHJ5Ii8KE0NlbGxBdHRhY2htZW50RW50cnkSCgoC" + "aWQYASABKAwSDAoEbmFtZRgCIAEoCWIGcHJvdG8z");
		descriptor = FileDescriptor.FromGeneratedCode(descriptorData, new FileDescriptor[0], new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[2]
		{
			new GeneratedClrTypeInfo(typeof(CellAttachments), CellAttachments.Parser, new string[1] { "Entries" }, null, null, null, null),
			new GeneratedClrTypeInfo(typeof(CellAttachmentEntry), CellAttachmentEntry.Parser, new string[2] { "Id", "Name" }, null, null, null, null)
		}));
	}
}
