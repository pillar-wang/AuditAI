using System;
using System.Diagnostics;
using System.Management;

namespace Leqisoft.Util;

public class MachineCode
{
	private static string _guid;

	private static Lazy<string> _code = new Lazy<string>(GetMachineCodeString, isThreadSafe: true);

	private static Lazy<string> _processId = new Lazy<string>(GetProcessId, isThreadSafe: true);

	public static string Code => _code.Value;

	public static string ProcessId => _processId.Value;

	private static string GetProcessId()
	{
		return Process.GetCurrentProcess().Id.ToString();
	}

	public static string GetMachineCodeString()
	{
		try
		{
			string moAddress = GetMoAddress();
			return moAddress.GetHashCode().ToString();
		}
		catch (Exception)
		{
			if (_guid == null)
			{
				_guid = Guid.NewGuid().ToString("N");
			}
			return _guid;
		}
	}

	private static string GetMoAddress()
	{
		string text = string.Empty;
		using (ManagementClass managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration"))
		{
			ManagementObjectCollection instances = managementClass.GetInstances();
			foreach (ManagementObject item in instances)
			{
				if ((bool)item["IPEnabled"])
				{
					text = item["MacAddress"].ToString();
				}
				item.Dispose();
			}
		}
		return text.ToString();
	}

	private static string GetCpuInfo()
	{
		string text = string.Empty;
		using (ManagementClass managementClass = new ManagementClass("Win32_Processor"))
		{
			ManagementObjectCollection instances = managementClass.GetInstances();
			foreach (ManagementObject item in instances)
			{
				text = item.Properties["ProcessorId"].Value.ToString();
				item.Dispose();
			}
		}
		return text.ToString();
	}

	private static string GetHDid()
	{
		string text = string.Empty;
		using (ManagementClass managementClass = new ManagementClass("Win32_DiskDrive"))
		{
			ManagementObjectCollection instances = managementClass.GetInstances();
			foreach (ManagementBaseObject item in instances)
			{
				text = (string)item.Properties["Model"].Value;
				item.Dispose();
			}
		}
		return text.ToString();
	}
}
