using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leqisoft.DTO;
using Leqisoft.UI.Controls.Properties;

namespace Leqisoft.UI.Controls;

public static class Util
{
	private delegate System.Drawing.Image InvokeHandle(System.Drawing.Image image, int desLength);

	private static System.Drawing.Image markImage = Resources.managerMark;

	public static void ShellExecuteUrl(string url)
	{
		try
		{
			Process.Start("explorer", "\"" + url + "\"");
		}
		catch (Win32Exception)
		{
			MessageBox.Show(MessageBoxIcon.None, "无法打开网页。");
		}
	}

	public static Bitmap StandardImage(System.Drawing.Image image, int desLength)
	{
		try
		{
			return standardImpl(image, desLength);
		}
		catch (InvalidOperationException)
		{
			try
			{
				return standardImpl(image, desLength);
			}
			catch (Exception exception)
			{
				exception.Log();
				return null;
			}
		}
		catch (Exception exception2)
		{
			exception2.Log();
			return null;
		}
	}

	public static Bitmap MarkImage2(System.Drawing.Image image, Bitmap mark)
	{
		Bitmap bitmap = new Bitmap(image.Width + mark.Width * 2, (image.Height > mark.Height) ? image.Height : mark.Height);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.DrawImage(mark, new RectangleF(0f, 0f, mark.Width, mark.Height), new RectangleF(0f, 0f, mark.Width, mark.Height), GraphicsUnit.Pixel);
		graphics.DrawImage(image, new RectangleF(mark.Width, 0f, image.Width, image.Height), new RectangleF(0f, 0f, image.Width, image.Height), GraphicsUnit.Pixel);
		return bitmap;
	}

	public static System.Drawing.Image MarkImage2(System.Drawing.Image image, int width, int height)
	{
		Bitmap bitmap = new Bitmap(image.Width + width * 2, (image.Height > height) ? image.Height : height);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.DrawImage(image, new RectangleF(width, 0f, image.Width, image.Height), new RectangleF(0f, 0f, image.Width, image.Height), GraphicsUnit.Pixel);
		return bitmap;
	}

	public static System.Drawing.Image GetHeadPic(User user, int size, bool withManagerMark)
	{
		System.Drawing.Image image;
		if (user.Picture != null)
		{
			try
			{
				using MemoryStream stream = new MemoryStream(user.Picture);
				image = System.Drawing.Image.FromStream(stream);
			}
			catch
			{
				image = ((user.Sex == "f") ? Resources.Girl : Resources.Boy);
			}
		}
		else
		{
			image = ((user.Sex == "f") ? Resources.Girl : Resources.Boy);
		}
		if (!withManagerMark)
		{
			return StandardImage(image, size);
		}
		if (user.IsTeamAdmin)
		{
			return MarkImage2(StandardImage(image, size), Resources.managerMark);
		}
		return MarkImage2(StandardImage(image, size), markImage.Width, markImage.Height);
	}

	private static Bitmap standardImpl(System.Drawing.Image image, int desLength)
	{
		Bitmap bitmap = new Bitmap(desLength, desLength);
		int num = ((image.Width < image.Height) ? image.Width : image.Height);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.DrawImage(image, new RectangleF(0f, 0f, bitmap.Width, bitmap.Height), new RectangleF(0f, 0f, num, num), GraphicsUnit.Pixel);
		return bitmap;
	}

	public static Color DarkenColor(Color inColor, double lightenAmount)
	{
		return Color.FromArgb(inColor.A, (int)Math.Max(0.0, (double)(int)inColor.R - 255.0 * lightenAmount), (int)Math.Max(0.0, (double)(int)inColor.G - 255.0 * lightenAmount), (int)Math.Max(0.0, (double)(int)inColor.B - 255.0 * lightenAmount));
	}

	public static Color LightColor(Color inColor, double lightenAmount)
	{
		return Color.FromArgb(inColor.A, (int)Math.Min(255.0, (double)(int)inColor.R + 255.0 * lightenAmount), (int)Math.Min(255.0, (double)(int)inColor.G + 255.0 * lightenAmount), (int)Math.Min(255.0, (double)(int)inColor.B + 255.0 * lightenAmount));
	}

	public static Color AlphaColor(Color inColor, double alphaAmount)
	{
		double num = (double)(int)inColor.A * alphaAmount;
		num = ((num > 255.0) ? 255.0 : ((num < 0.0) ? 0.0 : num));
		return Color.FromArgb((int)num, inColor.R, inColor.G, inColor.B);
	}

	public static bool RgbEquals(Color lhs, Color rhs)
	{
		if (lhs.R == rhs.R && lhs.G == rhs.G)
		{
			return lhs.B == rhs.B;
		}
		return false;
	}

	/// <summary>
	/// 使用进度对话框，逐项处理 UI 元素集合。每项通过 control.Invoke 回到 UI 线程执行。
	/// 适用于所有需要在 WinForms UI 线程上循环处理对象并展示进度的场景。
	/// </summary>
	/// <typeparam name="T">集合元素类型</typeparam>
	/// <param name="control">用于封送到 UI 线程的 Control 实例（如 TextControl / UserControl）</param>
	/// <param name="items">要处理的元素列表</param>
	/// <param name="taskName">任务描述名称，如"正在刷新表格"</param>
	/// <param name="processItem">处理单个元素的回调，参数为 (元素, 索引, 总数)</param>
	public static void ProcessItemsWithProgress<T>(
		Control control,
		IList<T> items,
		string taskName,
		Action<T, int, int> processItem)
	{
		int total = items.Count;
		if (total == 0) return;

		new ProgressForm<object>(async delegate(IProgress<ProgressInfo> iProg)
		{
			for (int i = 0; i < total; i++)
			{
				int idx = i;
				iProg.Report(new ProgressInfo
				{
					MainCaption = $"{taskName} ({i + 1}/{total})...",
					MainProgress = (int)((double)i / total * 100.0)
				});

				control.Invoke(new Action(() => processItem(items[idx], idx, total)));
				await Task.Delay(1);
			}

			iProg.Report(new ProgressInfo
			{
				MainCaption = $"{taskName}完成",
				MainProgress = 100
			});
			return (object)null;
		}).ShowDialog();
	}
}
