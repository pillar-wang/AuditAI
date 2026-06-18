using System;
using Newtonsoft.Json;

namespace Leqisoft.Model;

[Serializable]
public class Permissions
{
	private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
	{
		ObjectCreationHandling = ObjectCreationHandling.Replace
	};

	public Permission Read { get; set; } = new Permission();


	public Permission Write { get; set; } = new Permission();


	public Permission Schema { get; set; } = new Permission();


	public bool CanRead()
	{
		if (Read == null)
		{
			return true;
		}
		if (Read.GrantAll)
		{
			return true;
		}
		return Read.Users.Contains(User.Current.Id);
	}

	public bool CanWrite()
	{
		if (Write == null)
		{
			return true;
		}
		if (Write.GrantAll)
		{
			return true;
		}
		return Write.Users.Contains(User.Current.Id);
	}

	public bool CanDelete()
	{
		if (CanRead() && CanWrite())
		{
			return CanEditSchema();
		}
		return false;
	}

	public PermissionKind GetPermission(long userId)
	{
		PermissionKind permissionKind = PermissionKind.None;
		if (CanRead())
		{
			permissionKind |= PermissionKind.Read;
		}
		if (CanWrite())
		{
			permissionKind |= PermissionKind.Write;
		}
		if (CanEditSchema())
		{
			permissionKind |= PermissionKind.Schema;
		}
		return permissionKind;
	}

	public bool CanEditSchema()
	{
		if (Schema == null)
		{
			return true;
		}
		if (Schema.GrantAll)
		{
			return true;
		}
		return Schema.Users.Contains(User.Current.Id);
	}

	public string Serialize()
	{
		return JsonConvert.SerializeObject(this);
	}

	public void Deserialize(string s)
	{
		JsonConvert.PopulateObject(s, this, jsonSerializerSettings);
	}
}
