using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using Auditai.PlatformResource.Properties;

namespace Auditai.PlatformResource;

public class ApplicationIconManger
{
	private static Dictionary<PlatformType, string> ApplicationIconFilePathDic;

	private static bool _isIconVersionChanged;

	public static bool IsIconVersionChanged => _isIconVersionChanged;

	public static string GetApplicationIconFilePath(PlatformType platformType)
	{
		if (ApplicationIconFilePathDic == null)
		{
			GenerateApplicationIcon(platformType);
		}
		if (ApplicationIconFilePathDic.TryGetValue(platformType, out var value))
		{
			return value;
		}
		return null;
	}

	private static void GenerateApplicationIcon(PlatformType platformType)
	{
		ApplicationIconFilePathDic = new Dictionary<PlatformType, string>();
		string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		directoryName = Path.Combine(directoryName, "icon");
		string text = Path.Combine(directoryName, "icon.dat");
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		string empty = string.Empty;
		empty = ((platformType != PlatformType.Custom || ClientCustomizeData.Current == null) ? Assembly.GetExecutingAssembly().GetName().Version.ToString() : ClientCustomizeData.Current.Version);
		bool flag = true;
		SetFileAttributeToNormal(text);
		if (File.Exists(text))
		{
			string text2 = File.ReadAllText(text);
			flag = empty != text2;
		}
		if (platformType == PlatformType.Custom && ClientCustomizeData.Current != null)
		{
			string text3 = "Application.ico";
			string text4 = Path.Combine(directoryName, text3);
			ApplicationIconFilePathDic.Add(platformType, text4);
			if (flag && text4 != null)
			{
				_isIconVersionChanged = true;
				SetFileAttributeToNormal(text4);
				if (File.Exists(text4))
				{
					File.Delete(text4);
				}
				byte[] fileData = ClientCustomizeData.Current.GetFileData(text3);
				if (fileData != null)
				{
					using FileStream fileStream = File.Create(text4);
					fileStream.Write(fileData, 0, fileData.Length);
				}
			}
			List<string> list = new List<string> { "Uninstall.ico" };
			foreach (string item in list)
			{
				string text5 = Path.Combine(directoryName, item);
				if (!flag || text5 == null)
				{
					continue;
				}
				_isIconVersionChanged = true;
				SetFileAttributeToNormal(text5);
				if (File.Exists(text5))
				{
					File.Delete(text5);
				}
				byte[] fileData2 = ClientCustomizeData.Current.GetFileData(item);
				if (fileData2 != null)
				{
					using FileStream fileStream2 = File.Create(text5);
					fileStream2.Write(fileData2, 0, fileData2.Length);
				}
			}
		}
		else
		{
			Resource obj = new Resource();
			PropertyInfo[] properties = typeof(Resource).GetProperties(BindingFlags.Static | BindingFlags.NonPublic);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (!(propertyInfo.GetValue(obj, null) is Icon icon))
				{
					continue;
				}
				string name = propertyInfo.Name;
				string text6 = null;
				switch (name)
				{
				case "审计协作平台":
					text6 = Path.Combine(directoryName, "AuditPlatform.ico");
					ApplicationIconFilePathDic.Add(PlatformType.AuditPlatform, text6);
					break;
				case "集团报表平台":
					text6 = Path.Combine(directoryName, "ReportPlatform.ico");
					ApplicationIconFilePathDic.Add(PlatformType.EnterpriseReportPlatform, text6);
					break;
				case "业务管控平台":
					text6 = Path.Combine(directoryName, "ManagerPlatform.ico");
					ApplicationIconFilePathDic.Add(PlatformType.EnterpriseManagerPlatform, text6);
					break;
				case "报表开发平台":
					if (platformType == PlatformType.TableDevelopPlatform)
					{
						text6 = Path.Combine(directoryName, "TablePlatform.ico");
						ApplicationIconFilePathDic.Add(PlatformType.TableDevelopPlatform, text6);
					}
					else if ((uint)(platformType - 5) <= 5u)
					{
						text6 = Path.Combine(directoryName, "Application.ico");
						ApplicationIconFilePathDic.Add(platformType, text6);
					}
					break;
				}
				if (flag && text6 != null)
				{
					_isIconVersionChanged = true;
					SetFileAttributeToNormal(text6);
					if (File.Exists(text6))
					{
						File.Delete(text6);
					}
					using FileStream outputStream = File.Create(text6);
					icon.Save(outputStream);
				}
			}
		}
		if (flag)
		{
			File.WriteAllText(text, empty);
		}
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
