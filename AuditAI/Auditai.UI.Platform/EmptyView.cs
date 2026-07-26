using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1SplitContainer;
using C1.Win.C1SuperTooltip;
using Auditai.PlatformResource;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public static class EmptyView
{
	private static C1SuperLabel _sl;

	public static C1SplitContainer View { get; }

	static EmptyView()
	{
		View = new C1SplitContainer();
		View.Size = new Size(500, 400);
		View.Dock = DockStyle.Fill;
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel
		{
			SizeRatio = 100.0,
			BackgroundImageLayout = ImageLayout.Stretch,
			BackgroundImage = Resources.contactback,
			DoubleBuffered = true
		};
		_sl = new C1SuperLabel
		{
			Dock = DockStyle.Top,
			Height = 210,
			BackColor = Color.Transparent
		};
		c1SplitterPanel.Controls.Add(_sl);
		View.Panels.Add(c1SplitterPanel);
	}

	public static void SetQQ()
	{
		AppEditionBase currentEdition = Program.MainForm.CurrentEdition;
		string html;
		if (Program.ClientPlatformType == PlatformType.AuditPlatform)
		{
			html = GetHtml("852569234");
		}
		else if (Program.ClientPlatformType == PlatformType.EnterpriseReportPlatform)
		{
			html = GetHtml("1030358605");
		}
		else if (Program.ClientPlatformType == PlatformType.TableDevelopPlatform)
		{
			html = GetHtml("858176000");
		}
		else if (Program.ClientPlatformType == PlatformType.Custom)
		{
			string optionValueInSettingIniFile_String = ClientCustomizeData.Current.GetOptionValueInSettingIniFile_String("qq_number", "858176000");
			html = GetHtml(optionValueInSettingIniFile_String);
		}
		else
		{
			html = GetHtml("858176000");
		}
		_sl.Text = html;
	}

	private static string GetHtml(string qq)
	{
		return "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\r\n<html>\r\n<head><tidtle></title></head>\r\n<body>\r\n<span style=\"height:80px;\"></span>\r\n<p align = 'center' style = \"color:#484848;font: bold 18px Noto Sans SC\" > 保持沟通，享受更好服务 </ p >\r\n<p align = 'center' style = \"color:#909090;font: bold 15px Noto Sans SC\" > AuditAI 提供全程性服务，为您在使用上保驾护航 </ p >\r\n<p align = 'center' style = \"color:#9c9c9c;font: bold 12px Noto Sans SC\" > 官方qq群：" + qq + " </ p >\r\n</body>\r\n</html>";
	}
}
