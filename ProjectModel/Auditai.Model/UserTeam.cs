using System;
using System.Collections.Generic;
using Auditai.DTO;

namespace Auditai.Model;

public class UserTeam
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public long MaxUsers { get; set; }

	public long ManagerId { get; set; }

	public DateTime LicenseDate { get; set; }

	public long MaxProjects { get; set; }

	public long MaxTemplates { get; set; }

	public PayStatus PayStatus { get; set; }

	public int Type { get; set; }

	public TeamLevel Level { get; set; }

	public static UserTeam Current { get; set; }

	public static List<UserTeam> Teams { get; set; }

	public static bool CurrentTeamIsPayByProject { get; set; }
}
