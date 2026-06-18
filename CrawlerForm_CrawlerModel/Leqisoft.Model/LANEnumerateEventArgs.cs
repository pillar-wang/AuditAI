using System;
using System.Net;

namespace Leqisoft.Model;

public class LANEnumerateEventArgs : EventArgs
{
	public string HostName { get; set; }

	public IPAddress IP { get; set; }

	public override string ToString()
	{
		return $"{IP} ({HostName})";
	}
}
