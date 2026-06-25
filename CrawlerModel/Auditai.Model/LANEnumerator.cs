using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Auditai.Model;

public sealed class LANEnumerator
{
	[StructLayout(LayoutKind.Sequential)]
	private class SERVER_INFO_101
	{
		public int platform_id;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string name;

		public int version_major;

		public int version_minor;

		public int type;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string comment;

		public override string ToString()
		{
			return $"{platform_id}\t{name}\t{version_major}\t{version_minor}\t{type:X}\t{comment}";
		}
	}

	private SynchronizationContext syncContext;

	public event EventHandler<LANEnumerateEventArgs> EnumerateCallback;

	public LANEnumerator()
	{
		syncContext = SynchronizationContext.Current;
	}

	public void BeginEnumerate()
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			IntPtr bufptr = IntPtr.Zero;
			int entriesread = 0;
			int totalentries = 0;
			SERVER_INFO_101 sERVER_INFO_ = new SERVER_INFO_101();
			NetServerEnum(string.Empty, 101, ref bufptr, -1, ref entriesread, ref totalentries, -1, null, IntPtr.Zero);
			IntPtr intPtr = bufptr;
			for (int i = 0; i < entriesread; i++)
			{
				Marshal.PtrToStructure(intPtr, sERVER_INFO_);
				Dns.BeginGetHostAddresses(sERVER_INFO_.name, delegate(IAsyncResult ar)
				{
					string hostName = ar.AsyncState as string;
					try
					{
						OnEnumerateCallback(hostName, Dns.EndGetHostAddresses(ar).FirstOrDefault((IPAddress ip) => ip.AddressFamily == AddressFamily.InterNetwork));
					}
					catch (SocketException)
					{
					}
				}, sERVER_INFO_.name);
				intPtr = (IntPtr)((int)intPtr + Marshal.SizeOf(sERVER_INFO_));
			}
			NetApiBufferFree(bufptr);
		}, null);
	}

	private void OnEnumerateCallback(string hostName, IPAddress ip)
	{
		syncContext.Send(delegate
		{
			this.EnumerateCallback?.Invoke(this, new LANEnumerateEventArgs
			{
				HostName = hostName,
				IP = ip
			});
		}, null);
	}

	[DllImport("netapi32", CharSet = CharSet.Unicode)]
	private static extern int NetServerEnum(string servername, int level, ref IntPtr bufptr, int prefmaxlen, ref int entriesread, ref int totalentries, int servertype, string domain, IntPtr resume_handle);

	[DllImport("netapi32", CharSet = CharSet.Unicode)]
	private static extern int NetApiBufferFree(IntPtr buffer);
}
