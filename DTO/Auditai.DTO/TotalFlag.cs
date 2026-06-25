using System;

namespace Auditai.DTO;

[Flags]
public enum TotalFlag
{
	None = 1,
	MonthSum = 2,
	YearSum = 4,
	Data = 8
}
