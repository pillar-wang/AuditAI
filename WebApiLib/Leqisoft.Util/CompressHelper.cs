using System.IO;
using System.IO.Compression;

namespace Leqisoft.Util;

public static class CompressHelper
{
	public static byte[] Compress(byte[] inputBytes)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true);
		gZipStream.Write(inputBytes, 0, inputBytes.Length);
		gZipStream.Close();
		return memoryStream.ToArray();
	}

	public static byte[] Decompress(byte[] inputBytes)
	{
		using MemoryStream stream = new MemoryStream(inputBytes);
		using MemoryStream memoryStream = new MemoryStream();
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		gZipStream.CopyTo(memoryStream);
		gZipStream.Close();
		return memoryStream.ToArray();
	}
}
