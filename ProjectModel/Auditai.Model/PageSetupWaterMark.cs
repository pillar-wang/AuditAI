namespace Auditai.Model;

public class PageSetupWaterMark
{
	public class WaterMarkSetting
	{
		public string LeftText;

		public string RightText;

		public string CenterText;

		public string FontName;

		public double Height;
	}

	public WaterMarkSetting Header;

	public WaterMarkSetting Footer;
}
