﻿﻿﻿﻿﻿﻿﻿﻿namespace Auditai.DTO;

public class CrossDocumentValidationRule
{
    public Id64 Id { get; set; }
    public Id64 SourceDocumentId { get; set; }
    public Id64 SourceFieldId { get; set; }
    public Id64 TargetDocumentId { get; set; }
    public Id64 TargetFieldId { get; set; }
    public int Operator { get; set; }
    public string Note { get; set; }
    public int Dirty { get; set; }
    public int Status { get; set; }
}
