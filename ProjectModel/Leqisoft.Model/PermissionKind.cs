using System;

namespace Leqisoft.Model;

[Flags]
public enum PermissionKind
{
	None = 0,
	Read = 1,
	Write = 2,
	Schema = 4,
	All = 7
}
