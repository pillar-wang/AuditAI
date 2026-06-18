using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Leqisoft.Model;

public static class Util
{
	private enum Key_
	{
		Read = 131097,
		Wow6432Key = 512,
		Wow6464Key = 256
	}

	private static readonly UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(2147483650u);

	private const int ERROR_NO_MORE_ITEMS = 259;

	public static bool Cancel { get; set; } = false;


	public static string CurrentPath { get; private set; }

	[DllImport("advapi32")]
	private static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, int ulOptions, Key_ samDesired, out UIntPtr hkResult);

	[DllImport("advapi32")]
	private static extern uint RegEnumValue(UIntPtr hKey, uint dwIndex, StringBuilder lpValueName, ref uint lpcValueName, IntPtr lpReserved, IntPtr lpType, IntPtr lpData, IntPtr lpcbData);

	public static IEnumerable<string> GetFiles(string root, string searchPattern)
	{
		Stack<string> pending = new Stack<string>();
		pending.Push(root);
		while (pending.Count != 0)
		{
			string path = pending.Pop();
			string[] next2 = null;
			try
			{
				next2 = Directory.GetFiles(path, searchPattern);
			}
			catch
			{
			}
			if (next2 != null && next2.Length != 0)
			{
				string[] array = next2;
				for (int i = 0; i < array.Length; i++)
				{
					yield return array[i];
				}
			}
			try
			{
				next2 = Directory.GetDirectories(path);
				string[] array2 = next2;
				foreach (string subdir in array2)
				{
					if (Cancel)
					{
						Cancel = false;
						throw new OperationCanceledException();
					}
					CurrentPath = subdir;
					pending.Push(subdir);
				}
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception)
			{
			}
		}
	}

	public static string[] GetSqlServerInstanceNames()
	{
		List<string> list = new List<string>();
		if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL", 0, (Key_)131609, out var hkResult) == 0)
		{
			uint num = 0u;
			uint lpcValueName = 100u;
			StringBuilder stringBuilder = new StringBuilder((int)lpcValueName);
			for (uint num2 = RegEnumValue(hkResult, num, stringBuilder, ref lpcValueName, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero); num2 == 0; num2 = RegEnumValue(hkResult, num, stringBuilder, ref lpcValueName, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
			{
				list.Add(stringBuilder.ToString());
				num++;
			}
		}
		if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL", 0, (Key_)131353, out hkResult) == 0)
		{
			uint num3 = 0u;
			uint lpcValueName2 = 100u;
			StringBuilder stringBuilder2 = new StringBuilder((int)lpcValueName2);
			for (uint num4 = RegEnumValue(hkResult, num3, stringBuilder2, ref lpcValueName2, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero); num4 == 0; num4 = RegEnumValue(hkResult, num3, stringBuilder2, ref lpcValueName2, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
			{
				list.Add(stringBuilder2.ToString());
				num3++;
			}
		}
		return list.ToArray();
	}
}
