using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Leqisoft.UI.Platform;

internal class Program
{
	private static void Main(string[] args)
	{
		FixCurrentDirectory();
		Process[] processesByName = Process.GetProcessesByName("leqiupdater");
		Process[] array = processesByName;
		foreach (Process process in array)
		{
			process.WaitForExit();
		}
		UpdaterRenamedFilesRemoveExtension();
		if (args.Length != 0 && args[0].Equals("--ReplaceUpdater", StringComparison.InvariantCultureIgnoreCase))
		{
			Process.Start("leqiupdater");
		}
		else
		{
			Process.Start("leqiaudit");
		}
	}

	private static void UpdaterRenamedFilesRemoveExtension()
	{
		string location = Assembly.GetExecutingAssembly().Location;
		location = Path.GetDirectoryName(location);
		string[] files = Directory.GetFiles(location, "*.rename", SearchOption.AllDirectories);
		foreach (string text in files)
		{
			string text2 = text.Substring(0, text.Length - ".rename".Length);
			File.Delete(text2);
			File.Move(text, text2);
		}
	}

	private static void FixCurrentDirectory()
	{
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		Environment.CurrentDirectory = Path.GetDirectoryName(executingAssembly.Location);
	}
}
