using Auditai.Model;
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
		string[] values = GetValues();
		UserTeamPermissions userTeamPermissions = new UserTeamPermissions();
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
