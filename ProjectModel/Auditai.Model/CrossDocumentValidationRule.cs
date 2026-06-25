﻿﻿﻿﻿﻿using Auditai.DTO;

namespace Auditai.Model;

public class CrossDocumentValidationRule
{
    public Id64 Id { get; set; }
    public Id64 SourceDocumentId { get; set; }   // 源文档（文档A）
    public Id64 SourceFieldId { get; set; }       // 源域 Id
    public Id64 TargetDocumentId { get; set; }    // 目标文档（文档B）
    public Id64 TargetFieldId { get; set; }       // 目标域 Id
    public int Operator { get; set; }             // 运算符编码（同 ValidationOperator.Code）
    public string Note { get; set; }              // 规则说明
    public int Dirty { get; set; }
    public SyncStatus Status { get; set; }

    public bool IsDirty
    {
        get { return Dirty == 1; }
        set { Dirty = value ? 1 : 0; }
    }

    public void SetSynced()
    {
        IsDirty = false;
        Status = SyncStatus.Synced;
    }

    public CrossDocumentValidationRule Duplicate()
    {
        return (CrossDocumentValidationRule)MemberwiseClone();
    }

    internal Auditai.DTO.CrossDocumentValidationRule ToDto()
    {
        return new Auditai.DTO.CrossDocumentValidationRule
        {
            Id = Id,
            SourceDocumentId = SourceDocumentId,
            SourceFieldId = SourceFieldId,
            TargetDocumentId = TargetDocumentId,
            TargetFieldId = TargetFieldId,
            Operator = Operator,
            Note = Note,
            Dirty = Dirty,
            Status = (int)Status
        };
    }

    internal static CrossDocumentValidationRule FromDto(Auditai.DTO.CrossDocumentValidationRule dto)
    {
        return new CrossDocumentValidationRule
        {
            Id = dto.Id,
            SourceDocumentId = dto.SourceDocumentId,
            SourceFieldId = dto.SourceFieldId,
            TargetDocumentId = dto.TargetDocumentId,
            TargetFieldId = dto.TargetFieldId,
            Operator = dto.Operator,
            Note = dto.Note,
            Dirty = dto.Dirty,
            Status = (SyncStatus)dto.Status
        };
    }
}