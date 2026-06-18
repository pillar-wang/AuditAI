﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Leqisoft.DTO;

[Obfuscation(ApplyToMembers = true, Exclude = true, StripAfterObfuscation = false)]
public class Project
{
	public Guid Id { get; set; }

	public string Number { get; set; }

	public string Name { get; set; }

	public string Category { get; set; }

	public string Auditee { get; set; }

	public string Note { get; set; }

	public Guid? ParentId { get; set; }

	public User Creator { get; set; }

	public int Version { get; set; }

	public ProjectType Type { get; set; }

	public ChargeType ChargeType { get; set; }

	public IEnumerable<User> Users { get; set; }

	public bool TeamVisible { get; set; }

	public Guid? TemplateId { get; set; }

	public int DemoId { get; set; }

	public bool SystemBuild { get; set; }

	public DateTime CreateTime { get; set; }

	public ChargeType ProjectChargeType { get; set; }

	public DateTime ProjectLicenseDate { get; set; } = DateTime.Now;


	public Project Clone()
	{
		Project project = (Project)MemberwiseClone();
		project.Creator = Creator?.Clone();
		project.Users = Users?.Select((User u) => u.Clone());
		return project;
	}
}
