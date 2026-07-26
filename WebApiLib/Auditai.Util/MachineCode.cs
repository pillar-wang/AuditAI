﻿using System;
using System.Diagnostics;
using System.Management;

namespace Auditai.Util;

public class MachineCode
{
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
			// MAC 读取失败时回退到稳定标识（CPU ID → 磁盘型号 → 机器名哈希），
			// 避免 Guid.NewGuid() 导致每次启动机器码不同、Cookie 解密失败被迫重登。
			return GetStableFallbackCode();
		}
	}

	private static string GetStableFallbackCode()
	{
		// 1. 尝试 CPU ID
		try
		{
			var cpu = GetCpuInfo();
			if (!string.IsNullOrEmpty(cpu)) return cpu.GetHashCode().ToString();
		}
		catch { }
		// 2. 尝试磁盘型号
		try
		{
			var hd = GetHDid();
			if (!string.IsNullOrEmpty(hd)) return hd.GetHashCode().ToString();
		}
		catch { }
		// 3. 最后兜底：机器名 + 当前用户名哈希（始终可获取，保证稳定性）
		try
		{
			var fallback = Environment.MachineName + "|" + Environment.UserName;
			return fallback.GetHashCode().ToString();
		}
		catch
		{
			// 理论不会走到这里；若走到则用固定常量避免每次不同
			return "AuditAI_Fixed_Machine_001".GetHashCode().ToString();
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
				if (item["IPEnabled"] is bool enabled && enabled)
			{
				var mac = item["MacAddress"];
				if (mac != null)
				{
					text = mac.ToString();
				}
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
