using System;
using System.IO;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class User
{
	public static User Current { get; set; }

	public long Id { get; set; }

	public string UserName { get; set; }

	public string Name { get; set; }

	public Guid TeamId { get; set; }

	public bool IsLicenseOutOfDate { get; set; }

	public bool IsTeamAdmin { get; set; }

	public string TelPhone { get; set; }

	public DateTime LicenseDate { get; set; }

	public bool IsSystemAdmin { get; set; }

	public bool IsSystemSupporter { get; set; }

	public void CreateProfileFolderIfNotExist()
	{
		Directory.CreateDirectory("data\\" + Id);
	}

	public Leqisoft.DTO.Project GetNewProjectCandidate()
	{
		Leqisoft.DTO.Project project = new Leqisoft.DTO.Project();
		project.Id = Guid.NewGuid();
		project.ParentId = null;
		project.Number = string.Empty;
		project.Name = string.Empty;
		project.Category = string.Empty;
		project.Note = string.Empty;
		project.Auditee = string.Empty;
		project.Type = ProjectType.Project;
		project.ChargeType = ChargeType.None;
		project.Creator = new Leqisoft.DTO.User
		{
			Id = Id,
			Name = Name,
			UserName = UserName
		};
		project.Users = new Leqisoft.DTO.User[1]
		{
			new Leqisoft.DTO.User
			{
				Id = Id,
				Name = Name,
				UserName = UserName,
				Role = UserRole.Manager
			}
		};
		project.CreateTime = DateTime.Now;
		return project;
	}

	public Leqisoft.DTO.Project GetNewTemplateCandidate()
	{
		Leqisoft.DTO.Project project = new Leqisoft.DTO.Project();
		project.Id = Guid.NewGuid();
		project.ParentId = null;
		project.Number = string.Empty;
		project.Name = string.Empty;
		project.Category = string.Empty;
		project.Note = string.Empty;
		project.Type = ProjectType.Template;
		project.ChargeType = ChargeType.None;
		project.TeamVisible = true;
		project.Creator = new Leqisoft.DTO.User
		{
			Id = Id,
			Name = Name,
			UserName = UserName
		};
		project.Users = new Leqisoft.DTO.User[1]
		{
			new Leqisoft.DTO.User
			{
				Id = Id,
				Name = Name,
				UserName = UserName,
				Role = UserRole.Editor
			}
		};
		project.CreateTime = DateTime.Now;
		return project;
	}
}
