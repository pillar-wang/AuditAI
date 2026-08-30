using System;
using System.IO;
using System.Text;

namespace Auditai.DTO;

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
			// 修复：原实现直接把整个 Config 重置为默认值，会把用户已配置的主题、
			// 账号等全部个性化设置一并丢失（如用户手动删掉一条副标题内容后重启）。
			// 此处仅补全副标题内容到目标行数，避免破坏其余配置。
			while (Config.TableStyle.SubTitleContent.Count < Config.TableStyle.SubTitleRows)
			{
				Config.TableStyle.SubTitleContent.Add(Tuple.Create(string.Empty, string.Empty, string.Empty));
			}
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
