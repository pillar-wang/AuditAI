using System;
using System.Drawing;

namespace Auditai.UI.LedgerView;

public class Program
{
	public static void Main(string[] args)
	{
		// 高 DPI 下按当前屏幕 DPI 扩大图标位图生成分辨率，避免固定逻辑像素位图被拉伸而模糊
		try
		{
			using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
			{
				Auditai.UI.Controls.IconLibrary.DpiScale = Math.Max(1f, g.DpiX / 96f);
			}
		}
		catch { /* 读取 DPI 失败时保持默认 1f */ }
		frmImport frmImport2 = new frmImport();
		frmImport2.ShowDialog();
	}
}
