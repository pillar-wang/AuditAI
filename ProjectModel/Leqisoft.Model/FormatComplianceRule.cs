﻿﻿﻿﻿﻿﻿﻿﻿using Leqisoft.DTO;

namespace Leqisoft.Model;

public enum FormatComplianceRuleType
{
	RequiredParagraph = 0,
	NumberingContinuity = 1
}

public class FormatComplianceRule
{
	private const int DIRTY = 1;

	public Id64 Id { get; set; }

	public FormatComplianceRuleType RuleType { get; set; }

	public string Pattern { get; set; }

	public string Note { get; set; }

	public int Dirty { get; set; }

	public bool IsDirty
	{
		get
		{
			return Dirty == DIRTY;
		}
		set
		{
			Dirty = (value ? 1 : 0);
		}
	}

	public SyncStatus Status { get; set; }

	public void SetSynced()
	{
		IsDirty = false;
		Status = SyncStatus.Synced;
	}

	public FormatComplianceRule Duplicate()
	{
		FormatComplianceRule rule = (FormatComplianceRule)MemberwiseClone();
		rule.Id = Project.Current.GetNextId();
		return rule;
	}

	internal Leqisoft.DTO.FormatComplianceRule ToDto()
	{
		return new Leqisoft.DTO.FormatComplianceRule
		{
			Dirty = Dirty,
			Id = Id,
			RuleType = (int)RuleType,
			Pattern = Pattern,
			Note = Note,
			Status = (int)Status
		};
	}

	internal static FormatComplianceRule FromDto(Leqisoft.DTO.FormatComplianceRule dto)
	{
		return new FormatComplianceRule
		{
			Dirty = dto.Dirty,
			Id = dto.Id,
			RuleType = (FormatComplianceRuleType)dto.RuleType,
			Pattern = dto.Pattern,
			Note = dto.Note,
			Status = (SyncStatus)dto.Status
		};
	}
}
