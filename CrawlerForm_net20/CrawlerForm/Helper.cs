using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace CrawlerForm;

public static class Helper
{
	public static FastZip fz = new FastZip();

	public static void FileToZip(string zipFilePath, string FilePath, string ZipPWD)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(FilePath);
			string name = fileInfo.Name;
			string directoryName = fileInfo.DirectoryName;
			fz.Password = ZipPWD;
			fz.CreateZip(zipFilePath, directoryName, recurse: false, name);
		}
		catch (Exception ex)
		{
			throw new Exception("压缩失败！" + ex.Message, ex);
		}
	}

	public static void Compress(string DirPath, string ZipPath, string ZipPWD)
	{
		fz.Password = ZipPWD;
		fz.ExtractZip(ZipPath, DirPath, null);
	}
}
