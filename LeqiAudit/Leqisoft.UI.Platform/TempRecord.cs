using System;

namespace Leqisoft.UI.Platform;

public class TempRecord : ChatRecord
{
	public bool Bullet { get; set; }

	public object Value { get; set; }

	public TempRecord()
	{
		base.CreateTime = DateTime.Now;
	}
}
