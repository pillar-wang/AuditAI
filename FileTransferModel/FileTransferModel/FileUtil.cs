using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileTransferModel;

public static class FileUtil
{
	public const int defaultSectionLength = 30000;

	public static FileInfo GetFileInfo(string file, int sectionLength = 0)
	{
		if (sectionLength <= 0)
		{
			sectionLength = 30000;
		}
		using FileStream fileStream = new FileStream(file, FileMode.Open);
		long num = fileStream.Length / sectionLength;
		long num2 = fileStream.Length % sectionLength;
		return new FileInfo
		{
			Id = Guid.NewGuid().ToString("N"),
			Name = Path.GetFileName(file),
			ByteLength = fileStream.Length,
			SectionCount = (int)(num + ((num2 > 0) ? 1 : 0))
		};
	}

	public static FileSection[] GetFileSections(string file, string fileId, int sectionLength = 0)
	{
		if (sectionLength <= 0)
		{
			sectionLength = 30000;
		}
		using FileStream fileStream = new FileStream(file, FileMode.Open);
		long num = fileStream.Length / sectionLength;
		long num2 = fileStream.Length % sectionLength;
		FileSection[] array = new FileSection[num + ((num2 > 0) ? 1 : 0)];
		using (BufferedStream bufferedStream = new BufferedStream(fileStream))
		{
			for (int i = 0; i < num; i++)
			{
				FileSection fileSection = new FileSection
				{
					Id = fileId,
					Index = i,
					Value = new byte[sectionLength]
				};
				bufferedStream.Read(fileSection.Value, 0, sectionLength);
				array[i] = fileSection;
			}
			if (num2 > 0)
			{
				FileSection fileSection2 = new FileSection();
				fileSection2.Id = fileId;
				fileSection2.Index = (int)num;
				fileSection2.Value = new byte[num2];
				bufferedStream.Read(fileSection2.Value, 0, (int)num2);
				array[num] = fileSection2;
			}
		}
		return array;
	}

	public static byte[] GetFileFromSections(IEnumerable<FileSection> sections)
	{
		byte[] array = new byte[sections.Sum((FileSection s) => s.Value.Length)];
		sections = sections.OrderBy((FileSection s) => s.Index);
		int num = 0;
		foreach (FileSection section in sections)
		{
			byte[] value = section.Value;
			foreach (byte b in value)
			{
				array[num++] = b;
			}
		}
		return array;
	}
}
