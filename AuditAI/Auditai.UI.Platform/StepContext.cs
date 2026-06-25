using System;

namespace Auditai.UI.Platform;

public class StepContext<Key>
{
	public Action<Key> Restore;
}
