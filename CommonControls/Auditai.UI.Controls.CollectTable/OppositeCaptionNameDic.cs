using System.Collections.Generic;

namespace Auditai.UI.Controls.CollectTable;

internal class OppositeCaptionNameDic : Dictionary<string, string>
{
	public IEnumerable<string> Captions => base.Keys;
}
