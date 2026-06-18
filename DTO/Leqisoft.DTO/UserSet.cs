using System;
using System.IO;
using System.Text;

namespace Leqisoft.DTO;

public static class UserSet
{
	private const string FILENAME = "./config/config.json";

	private static bool _createdNew = true;

	public static string LoginPassword;

	public static string LoginPhone;

	public static UserConfig Config { get; set; } = new UserConfig();


	public static void Load()
	{
		try
		{
			Config.LoadConfig(File.ReadAllText("./config/config.json", Encoding.UTF8));
			_createdNew = false;
		}
		catch (FileNotFoundException)
		{
			Config = new UserConfig();
		}
		catch (Exception)
		{
			Config = new UserConfig();
		}
		if (Config.TableStyle.SubTitleContent.Count < Config.TableStyle.SubTitleRows)
		{
			Config = new UserConfig();
		}
		Config.Tooltip = false;
	}

	public static void Save()
	{
		string directoryName = Path.GetDirectoryName("./config/config.json");
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		if (!File.Exists("./config/config.json"))
		{
			File.Create("./config/config.json").Close();
		}
		SetFileAttributeToNormal("./config/config.json");
		try
		{
			File.WriteAllText("./config/config.json", Config.SaveConfig(), Encoding.UTF8);
		}
		catch (Exception)
		{
		}
	}

	public static void InitializeForEdition(int defaultSubTitleRows, bool enableLedger)
	{
		if (_createdNew)
		{
			Config.TableStyle.SubTitleRows = defaultSubTitleRows;
			Config.BooksStyle.EnableLedger = enableLedger;
		}
	}

	public static void InitializeDefaultTheme(string themeId)
	{
		if (_createdNew)
		{
			Config.CurrentTheme = themeId;
		}
	}

	public static bool IsCreateNew()
	{
		return _createdNew;
	}

	private static void SetFileAttributeToNormal(string filePath)
	{
		try
		{
			if (File.Exists(filePath))
			{
				File.SetAttributes(filePath, FileAttributes.Normal);
			}
		}
		catch (Exception)
		{
		}
	}
}
