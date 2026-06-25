﻿﻿﻿﻿﻿﻿﻿using Auditai.DTO;

namespace Auditai.Model;

public class ValidationFormula
{
	private const int DIRTY = 1;

	public string LeftExpr { get; set; }

	public ValidationOperator Operator { get; set; }

	public string RightExpr { get; set; }

	public string Note { get; set; }

	public Id64 Id { get; set; }

	public int Dirty { get; set; }

	public bool IsDirty
	{
		get
		{
			return Dirty == 1;
		}
		set
		{
			Dirty = (value ? 1 : 0);
		}
	}

	public SyncStatus Status { get; set; }

	public Id64 TableId { get; set; }

	/// <summary>
	/// 标识关联的文档域 Id。为 0/null 表示该公式绑定到表格（TableId）而非文档域。
	/// </summary>
	public Id64 DocumentFieldId { get; set; }

	public void SetSynced()
	{
		IsDirty = false;
		Status = SyncStatus.Synced;
	}

	public ValidationFormula Duplicate()
	{
		ValidationFormula validationFormula = (ValidationFormula)MemberwiseClone();
		validationFormula.Id = Project.Current.GetNextId();
		validationFormula.DocumentFieldId = DocumentFieldId;
		return validationFormula;
	}

	internal Auditai.DTO.ValidationFormula ToDto()
	{
		return new Auditai.DTO.ValidationFormula
		{
			Dirty = Dirty,
			Id = Id,
			LeftExpr = LeftExpr,
			Note = Note,
			Operator = Operator.Code,
			RightExpr = RightExpr,
			Status = (int)Status,
			TableId = TableId,
			DocumentFieldId = DocumentFieldId
		};
	}

	internal static ValidationFormula FromDto(Auditai.DTO.ValidationFormula dto)
	{
		return new ValidationFormula
		{
			Dirty = dto.Dirty,
			Id = dto.Id,
			LeftExpr = dto.LeftExpr,
			Note = dto.Note,
			Operator = ValidationOperator.FromCode(dto.Operator),
			RightExpr = dto.RightExpr,
			Status = (SyncStatus)dto.Status,
			TableId = dto.TableId,
			DocumentFieldId = dto.DocumentFieldId
		};
	}
}
