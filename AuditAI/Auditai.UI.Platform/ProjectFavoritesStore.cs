using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Auditai.UI.Platform;

/// <summary>
/// 项目/模板收藏存储（GUID 字符串集合，JSON 数组文件持久化）。
/// 首次访问惰性加载；文件不存在或损坏时按空集处理；Toggle 后立即落盘。
/// </summary>
public static class ProjectFavoritesStore
{
	private static HashSet<string> _ids;

	public static bool IsFavorite(Guid id)
	{
		LoadIfNotLoaded();
		return _ids.Contains(id.ToString());
	}

	public static void Toggle(Guid id)
	{
		LoadIfNotLoaded();
		if (!_ids.Add(id.ToString()))
		{
			_ids.Remove(id.ToString());
		}
		Save();
	}

	public static IReadOnlyCollection<string> GetAll()
	{
		LoadIfNotLoaded();
		return _ids;
	}

	private static void LoadIfNotLoaded()
	{
		if (_ids != null)
		{
			return;
		}
		HashSet<string> loaded = new HashSet<string>();
		try
		{
			string path = ConfigManager.PROJECTMANAGEMENT_FAVORITES;
			if (File.Exists(path))
			{
				List<string> list = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(path));
				if (list != null)
				{
					foreach (string id in list)
					{
						if (!string.IsNullOrWhiteSpace(id))
						{
							loaded.Add(id.Trim());
						}
					}
				}
			}
		}
		catch (Exception)
		{
			loaded = new HashSet<string>();
		}
		_ids = loaded;
	}

	public static void Save()
	{
		try
		{
			string path = ConfigManager.PROJECTMANAGEMENT_FAVORITES;
			string directoryName = Path.GetDirectoryName(path);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			File.WriteAllText(path, JsonConvert.SerializeObject(_ids ?? new HashSet<string>()));
		}
		catch (Exception)
		{
		}
	}
}
