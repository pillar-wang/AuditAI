using System;
using System.IO;

namespace ParadoxReader;

internal class ParadoxBlobFile : IDisposable
{
	private readonly Stream stream;

	private readonly BinaryReader reader;

	public ParadoxBlobFile(string fileName)
		: this(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
	{
	}

	public ParadoxBlobFile(Stream stream)
	{
		this.stream = stream;
		reader = new BinaryReader(stream);
	}

	public virtual void Dispose()
	{
		stream.Dispose();
	}

	public byte[] ReadBlob(byte[] blobInfo)
	{
		uint num = BitConverter.ToUInt32(blobInfo, 0);
		uint num2 = num & 0xFFu;
		uint num3 = num & 0xFFFFFF00u;
		int num4 = BitConverter.ToInt32(blobInfo, 4);
		int num5 = 9;
		int num6 = BitConverter.ToInt16(blobInfo, 8);
		if (num4 > 0)
		{
			stream.Position = num3;
			byte[] array = new byte[6];
			reader.Read(array, 0, 3);
			reader.Read(array, 0, num5 - 3);
			int num7 = BitConverter.ToInt32(array, 0);
			if (num7 == num4)
			{
				byte[] array2 = new byte[num4];
				reader.Read(array2, 0, num4);
				return array2;
			}
		}
		return null;
	}
}
