using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Auditai.UI.Platform;

public class ProjectInfoManager
{
	private static ProjectInfoManager _projectInfoManager;

	private bool loaded;

	private Dictionary<string, ProjectInfo> _infos = new Dictionary<string, ProjectInfo>();

	public static ProjectInfoManager GetInstance()
	{
		if (_projectInfoManager == null)
		{
			_projectInfoManager = new ProjectInfoManager();
		}
		return _projectInfoManager;
	}

	private ProjectInfoManager()
	{
	}

	public void UpdateOpenTime(string projectId, DateTime time)
	{
		LoadIfNotLoaded();
		if (!_infos.ContainsKey(projectId))
		{
			_infos.Add(projectId, new ProjectInfo());
		}
		_infos[projectId].OpenTime = time;
	}

	public ProjectInfo GetProject(string projectId)
	{
		LoadIfNotLoaded();
		if (_infos.ContainsKey(projectId))
		{
			return _infos[projectId];
		}
		return null;
	}

	public IEnumerable<string> GetRecent()
	{
		LoadIfNotLoaded();
		IEnumerable<string> enumerable = from i in _infos
			orderby i.Value.OpenTime descending
			select i.Key;
		if (enumerable.Count() <= 5)
		{
			return enumerable;
		}
		return enumerable.Take(5);
	}

	private void LoadIfNotLoaded()
	{
		if (loaded)
		{
			return;
		}
		if (!File.Exists(ConfigManager.PROJECT_OPERATEINFO_RECORD))
		{
			_infos = new Dictionary<string, ProjectInfo>();
		}
		else
		{
			try
			{
				string value = File.ReadAllText(ConfigManager.PROJECT_OPERATEINFO_RECORD);
				_infos = JsonConvert.DeserializeObject<Dictionary<string, ProjectInfo>>(value);
				_infos = _infos ?? new Dictionary<string, ProjectInfo>();
			}
			catch (Exception)
			{
				_infos = new Dictionary<string, ProjectInfo>();
			}
		}
		loaded = true;
	}

	public void Save()
	{
		try
		{
			string directoryName = Path.GetDirectoryName(ConfigManager.PROJECT_OPERATEINFO_RECORD);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			string contents = JsonConvert.SerializeObject(_infos);
			SetFileAttributeToNormal(ConfigManager.PROJECT_OPERATEINFO_RECORD);
			File.WriteAllText(ConfigManager.PROJECT_OPERATEINFO_RECORD, contents);
		}
		catch (Exception)
		{
		}
	}

	private static void SetFileAttributeToNormal(string filePath)
	{
		try
		{
			if (File.Exists(filePath))
			{
				File.SetAttributes(filePath, FileAttributes.Normal);
			}
		}
		catch (Exception)
		{
		}
	}
}
