using System;
using System.Reflection;
using System.Threading.Tasks;
using Auditai.PlatformResource;

namespace Auditai.UI.Platform;

public static class UpdateChecker
{
	private static Task<bool> IsClientCustomizePackageUpdate()
	{
		return Task.FromResult(false);
	}

	public static Task<Tuple<CheckUpdateResult, Version>> CheckUpdate()
	{
		Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
		return Task.FromResult(Tuple.Create(CheckUpdateResult.UpToDate, currentVersion));
	}

	public static void SetBaseAddress(string address)
	{
		// 本地模式下不设置远程更新地址
	}
}
