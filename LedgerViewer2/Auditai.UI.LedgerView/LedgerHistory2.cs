using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Auditai.UI.LedgerView;

public class LedgerHistory2
{
	[JsonProperty("dic")]
	private Dictionary<string, Dictionary<string, DateTime>> _dic;

	[JsonIgnore]
	public static LedgerHistory2 OpenHistory { get; } = new LedgerHistory2();


	public LedgerHistory2()
	{
		_dic = new Dictionary<string, Dictionary<string, DateTime>>();
		Load(ConfigManager.RECENTLEDGERPATH);
	}

	public void Add(string projectId, string ledgerFile)
	{
		if (projectId == null || ledgerFile == null)
		{
			return;
		}
		if (_dic.ContainsKey(projectId))
		{
			Dictionary<string, DateTime> dictionary = _dic[projectId];
			if (dictionary.ContainsKey(ledgerFile))
			{
				dictionary[ledgerFile] = DateTime.Now;
			}
			else
			{
				dictionary.Add(ledgerFile, DateTime.Now);
			}
		}
		else
		{
			_dic.Add(projectId, new Dictionary<string, DateTime> { [ledgerFile] = DateTime.Now });
		}
	}

	public Dictionary<string, DateTime> GetProject(string projectId)
	{
		if (projectId == null)
		{
			return null;
		}
		if (!_dic.ContainsKey(projectId))
		{
			return null;
		}
		return _dic[projectId];
	}

	public void Load(string file)
	{
		if (!File.Exists(file))
		{
			return;
		}
		try
		{
			string value = File.ReadAllText(file);
			JsonConvert.PopulateObject(value, this);
		}
		catch (Exception)
		{
		}
	}

	public void Save(string file)
	{
		try
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
			string contents = JsonConvert.SerializeObject(this);
			File.WriteAllText(file, contents);
		}
		catch (Exception)
		{
		}
	}
}
