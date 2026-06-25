using System;
using System.IO;
using System.Reflection;

namespace Auditai.UI.Controls;

public static class LogWriter
{
	private static string LogPath;

	private static string LogFile;

	static LogWriter()
	{
		try
		{
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			LogPath = Path.Combine(directoryName, "logs");
			if (!Directory.Exists(LogPath))
			{
				Directory.CreateDirectory(LogPath);
			}
			LogFile = Path.Combine(LogPath, DateTime.Now.ToString("yyyyMMdd") + ".txt");
			if (!File.Exists(LogFile))
			{
				File.Create(LogFile).Close();
			}
		}
		catch (Exception)
		{
		}
	}

	public static void Log(this Exception exception, string attachMessage = null)
	{
		try
		{
			using FileStream stream = new FileStream(LogFile, FileMode.Append, FileAccess.Write);
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.WriteLine($"时间：{DateTime.Now} {exception.GetType()}");
			streamWriter.WriteLine("附加信息：【" + (attachMessage ?? string.Empty) + "】");
			streamWriter.WriteLine(exception.ToString());
			streamWriter.WriteLine("##### END");
			streamWriter.WriteLine();
		}
		catch (Exception)
		{
		}
	}
}
