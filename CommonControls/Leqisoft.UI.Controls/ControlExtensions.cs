using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using C1.Win.C1Sizer;
using Leqisoft.Model;

namespace Leqisoft.UI.Controls;

public static class ControlExtensions
{
	private static readonly Dictionary<Control, int> _dicSuspendDrawing = new Dictionary<Control, int>();

	private const int WM_SETREDRAW = 11;

	[DllImport("user32")]
	private static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

	[DllImport("gdi32.dll")]
	private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);

	[DllImport("gdi32.dll")]
	private static extern IntPtr DeleteDC(IntPtr hDc);

	[DllImport("gdi32.dll")]
	private static extern IntPtr DeleteObject(IntPtr hDc);

	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

	[DllImport("gdi32.dll")]
	private static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);

	[DllImport("user32.dll")]
	private static extern IntPtr GetWindowDC(IntPtr ptr);

	[DllImport("user32.dll")]
	private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

	public static void SuspendDrawing(this Control parent)
	{
		if (!_dicSuspendDrawing.TryGetValue(parent, out var value))
		{
			SendMessage(parent.Handle, 11, 0, 0);
		}
		value++;
		_dicSuspendDrawing[parent] = value;
	}

	public static void ResumeDrawing(this Control parent)
	{
		if (_dicSuspendDrawing.TryGetValue(parent, out var value))
		{
			value--;
			if (value == 0)
			{
				_dicSuspendDrawing.Remove(parent);
				SendMessage(parent.Handle, 11, 1, 0);
				parent.Refresh();
			}
			else
			{
				_dicSuspendDrawing[parent] = value;
			}
		}
	}

	public static Bitmap GetScreenshot(this Control control)
	{
		IntPtr windowDC = GetWindowDC(control.Handle);
		IntPtr intPtr = CreateCompatibleDC(windowDC);
		IntPtr intPtr2 = CreateCompatibleBitmap(windowDC, control.Width, control.Height);
		IntPtr bmp = SelectObject(intPtr, intPtr2);
		if (BitBlt(intPtr, 0, 0, control.Width, control.Height, windowDC, 0, 0, (CopyPixelOperation)1087111200))
		{
			Bitmap result = System.Drawing.Image.FromHbitmap(intPtr2);
			SelectObject(intPtr, bmp);
			DeleteObject(intPtr2);
			DeleteDC(intPtr);
			ReleaseDC(control.Handle, windowDC);
			return result;
		}
		return null;
	}

	public static void SetFixed(this C1.Win.C1Sizer.RowCollection rc, params int[] idxs)
	{
		Array.ForEach(idxs, delegate(int _)
		{
			rc[_].IsFixedSize = true;
		});
	}

	public static void SetFixed(this C1.Win.C1Sizer.ColumnCollection cc, params int[] idxs)
	{
		Array.ForEach(idxs, delegate(int _)
		{
			cc[_].IsFixedSize = true;
		});
	}

	public static HorizontalAlignment ToHorizontalAlignment(CellTextAlign a)
	{
		switch (a)
		{
		case CellTextAlign.TopCenter:
		case CellTextAlign.MiddleCenter:
		case CellTextAlign.BottomCenter:
			return HorizontalAlignment.Center;
		case CellTextAlign.TopLeft:
		case CellTextAlign.MiddleLeft:
		case CellTextAlign.BottomLeft:
			return HorizontalAlignment.Left;
		case CellTextAlign.TopRight:
		case CellTextAlign.MiddleRight:
		case CellTextAlign.BottomRight:
			return HorizontalAlignment.Right;
		default:
			return HorizontalAlignment.Left;
		}
	}
}
