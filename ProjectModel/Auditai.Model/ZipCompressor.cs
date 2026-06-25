using System.IO;
using System.IO.Compression;

namespace Auditai.Model;

public static class ZipCompressor
{
	public static byte[] Compress(byte[] input)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
		{
			deflateStream.Write(input, 0, input.Length);
		}
		return memoryStream.ToArray();
	}

	public static byte[] Decompress(byte[] input)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (DeflateStream deflateStream = new DeflateStream(new MemoryStream(input), CompressionMode.Decompress))
		{
			deflateStream.CopyTo(memoryStream);
		}
		return memoryStream.ToArray();
	}
}
