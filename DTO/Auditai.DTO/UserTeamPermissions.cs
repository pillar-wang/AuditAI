﻿﻿﻿﻿﻿﻿﻿using Auditai.Model;
using Newtonsoft.Json;

namespace Auditai.DTO;

[JsonObject]
public class UserTeamPermissions
{
	[JsonProperty]
	public bool MustInclude { get; set; }

	public UserTeamPermissions Clone()
	{
		return (UserTeamPermissions)MemberwiseClone();
	}

	public string Serialize()
	{
		return JsonConvert.SerializeObject(this);
	}

	public static UserTeamPermissions Deserialize(string s)
	{
		// 防御性处理：服务端未返回 Permissions 字段或数据库为 NULL 时，
		// JsonConvert.DeserializeObject<T>(null) 会抛 ArgumentNullException: value。
		// 返回默认实例以保持原有“无权限”语义。
		if (string.IsNullOrWhiteSpace(s)) return new UserTeamPermissions();
		return JsonConvert.DeserializeObject<UserTeamPermissions>(s);
	}

	public string GetDisplay()
	{
		if (MustInclude)
		{
			return GetValues()[0];
		}
		return "";
	}

	public static UserTeamPermissions Parse(string s)
	{
		// 修复：s 为空时原实现 s.Split 抛 NRE；GetValues 返回空数组时 values[0] 越界。
		UserTeamPermissions userTeamPermissions = new UserTeamPermissions();
		if (string.IsNullOrEmpty(s)) return userTeamPermissions;
		string[] values = GetValues();
		if (values == null || values.Length == 0) return userTeamPermissions;
		string[] array = s.Split('|');
		foreach (string text in array)
		{
			if (values[0] == text)
			{
				userTeamPermissions.MustInclude = true;
			}
		}
		return userTeamPermissions;
	}

	public static string[] GetValues()
	{
		return new string[1] { "新建" + StringConstBase.Current.Project + "时该人员必选" };
	}
}
