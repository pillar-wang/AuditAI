﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Auditai.DTO;

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

	public IEnumerable<User> Users { get; set; } = Enumerable.Empty<User>();

	public bool TeamVisible { get; set; }

	public Guid? TemplateId { get; set; }

	public int DemoId { get; set; }

	public bool SystemBuild { get; set; }

	public DateTime CreateTime { get; set; }

	public ChargeType ProjectChargeType { get; set; }

	public DateTime ProjectLicenseDate { get; set; } = DateTime.Now;


	/// <summary>自定义填充规则配置（JSON 格式）</summary>
	public string CustomFillConfig { get; set; }

	/// <summary>审核状态：0=未上报 1=审批中 2=已通过 3=已退回</summary>
	public int ReviewStatus { get; set; }

	/// <summary>是否已归档（归档后项目锁定，禁止数据推送）</summary>
	public bool IsArchived { get; set; }


	public Project Clone()
	{
		Project project = (Project)MemberwiseClone();
		project.Creator = Creator?.Clone();
		project.Users = Users?.Select((User u) => u.Clone()).ToList();
		return project;
	}
}
