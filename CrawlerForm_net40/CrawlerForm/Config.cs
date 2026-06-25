using System;
using System.IO;
using Auditai.Model;
using Newtonsoft.Json;

namespace CrawlerForm;

internal class Config
{
	public string Brand { get; set; }

	public string FriendlyName { get; set; }

	public DatabaseInfo DatabaseInfo { get; set; }

	public void Save(string path)
	{
		try
		{
			string contents = JsonConvert.SerializeObject(this);
			string directoryName = Path.GetDirectoryName(path);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			if (!File.Exists(path))
			{
				File.Create(path).Close();
			}
			File.WriteAllText(path, contents);
		}
		catch (Exception)
		{
		}
	}

	public void Load(string path)
	{
		try
		{
			string value = File.ReadAllText(path);
			JsonConvert.PopulateObject(value, this);
		}
		catch (Exception)
		{
		}
	}
}
