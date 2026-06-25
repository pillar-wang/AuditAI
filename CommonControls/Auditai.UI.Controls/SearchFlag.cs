using System;

namespace Auditai.UI.Controls;

[Flags]
public enum SearchFlag
{
	None = 0,
	Case = 1,
	WholeWord = 2,
	Search = 4,
	ScopeCurrent = 8,
	ScopeGlobal = 0x10
}
