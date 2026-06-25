using System.Collections.Generic;
using System.Linq;

namespace Auditai.DTO;

public class UserGroup
{
	public long Id { get; set; }

	public string Name { get; set; }

	public long? ParentId { get; set; }

	public HashSet<User> Users { get; set; }

	public UserGroup ParentGroup { get; set; }

	public HashSet<UserGroup> Children { get; set; }

	public UserGroup()
	{
		Users = new HashSet<User>();
		Children = new HashSet<UserGroup>();
	}

	public List<User> DescendantsUsers()
	{
		return Users.Concat(Children.SelectMany((UserGroup c) => c.DescendantsUsers())).ToList();
	}

	public List<UserGroup> DescendantsAndSelfGroup()
	{
		return new UserGroup[1] { this }.Concat(Children).Concat(Children.SelectMany((UserGroup c) => c.DescendantsAndSelfGroup())).ToList();
	}
}
