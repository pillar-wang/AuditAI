using System;
using System.Collections.Generic;

namespace Auditai.UI.Controls.CollectTable;

internal class CaptionTypeDic : Dictionary<string, Type>
{
	public IEnumerable<string> Captions => base.Keys;
}
