using System;
using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class ImportNodeArgs : EventArgs
{
	public ImportTypeEnum Type { get; set; }

	public object ParentNode { get; set; }

	public int Index { get; set; }

	public TreeNodeBase AppendNode { get; set; }

	public string Message { get; set; } = string.Empty;


	public bool OnProgress { get; set; }
}
