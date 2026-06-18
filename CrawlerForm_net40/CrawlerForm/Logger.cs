using System;
using System.IO;

namespace CrawlerForm;

public class Logger
{
	private string _file;

	public Logger(string file)
	{
		string directoryName = Path.GetDirectoryName(file);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		if (!File.Exists(file))
		{
			File.Create(file).Close();
		}
		_file = file;
	}

	public void Writer(string message)
	{
		try
		{
			using FileStream stream = new FileStream(_file, FileMode.Append);
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.WriteLine($"{DateTime.Now}");
			streamWriter.WriteLine(message);
			streamWriter.WriteLine();
		}
		catch (Exception)
		{
		}
	}
}
