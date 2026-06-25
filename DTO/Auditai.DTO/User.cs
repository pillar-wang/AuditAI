using System;
using System.Reflection;
using Newtonsoft.Json;

namespace Auditai.DTO;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class User
{
	public long Id { get; set; }

	public string UserName { get; set; }

	public string Password { get; set; }

	public string Email { get; set; }

	public string Sex { get; set; }

	public string Name { get; set; }

	public string Company { get; set; }

	public string Phone { get; set; }

	public string City { get; set; }

	public UserRole Role { get; set; }

	public string QQId { get; set; }

	public string WechatId { get; set; }

	public string Salt { get; set; }

	public Guid TeamId { get; set; }

	public bool IsDataAdmin { get; set; }

	public byte[] Picture { get; set; }

	public DateTime LicenseDate { get; set; }

	public long? GroupId { get; set; }

	public string JobTitle { get; set; }

	[JsonIgnore]
	public UserGroup UserGroup { get; set; }

	public bool IsTeamAdmin { get; set; }

	public bool IsSystemAdmin { get; set; }

	public UserTeamPermissions Permissions { get; set; }

	public User Clone()
	{
		User user = (User)MemberwiseClone();
		if (Permissions != null)
		{
			user.Permissions = Permissions.Clone();
		}
		return user;
	}
}
