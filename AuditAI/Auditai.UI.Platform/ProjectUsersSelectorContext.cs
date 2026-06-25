using System;
using System.Collections.Generic;
using Auditai.DTO;

namespace Auditai.UI.Platform;

public class ProjectUsersSelectorContext
{
	public Project Project { get; set; }

	public List<User> RootUsers { get; set; }

	public List<UserGroup> UserGroups { get; set; }

	public List<Tuple<User, bool>> UserViewStates { get; set; }

	public long ManagerId { get; set; }
}
