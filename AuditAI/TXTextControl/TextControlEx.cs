﻿﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TXTextControl;

public class TextControlEx : TextControl
{
	private const int GCS_RESULTSTR = 2048;

	private const int WM_IME_COMPOSITION = 271;

	private const int WM_IME_STARTCOMPOSITION = 269;

	private const int WM_IME_ENDCOMPOSITION = 270;

	private const int WM_CHAR = 258;

	private const int WM_CREATE = 1;

	private const int WM_GETOBJECT = 0x003D;


	public new event EventHandler<TextContextMenuEventArgs> TextContextMenuOpening;

	public new void FontDialog()
	{
		try { base.FontDialog(); }
		catch { }
	}

	public new void ParagraphFormatDialog()
	{
		try { base.ParagraphFormatDialog(); }
		catch { }
	}

	public override Cursor Cursor
	{
		get
		{
			try
			{
				return base.Cursor;
			}
			catch
			{
				return Cursors.Default;
			}
		}
		set
		{
			base.Cursor = value;
		}
	}

	[DllImport("imm32")]
	private static extern IntPtr ImmGetContext(IntPtr hwnd);

	[DllImport("imm32")]
	private static extern bool ImmReleaseContext(IntPtr hwnd, IntPtr himc);

	[DllImport("imm32", CharSet = CharSet.Unicode)]
	public static extern int ImmGetCompositionString(IntPtr himc, int dword, byte[] buf, int bufLen);

	public void FixTableTopLeftMergedRenderBug()
	{
		Table item = base.Tables.GetItem();
		if (item != null)
		{
			TableCell item2 = item.Cells.GetItem();
			if (item2 != null)
			{
				BeginUndoAction("FixTableTopLeftMergedRenderBug");
				TableRow item3 = item.Rows.GetItem(item2.Row);
				item3.IsHeader = item3.IsHeader;
				EndUndoAction();
			}
		}
	}

	public void CancelAllTrackChanges()
	{
		RemoveAllTrackChanges(isAccept: false);
	}

	public void AcceptAllTrackChanges()
	{
		RemoveAllTrackChanges(isAccept: true);
	}

	private void RemoveAllTrackChanges(bool isAccept)
	{
		TrackedChangeCollection trackedChanges = base.TrackedChanges;
		if (trackedChanges != null)
		{
			for (int num = trackedChanges.Count; num > 0; num--)
			{
				trackedChanges.Remove(trackedChanges[num], isAccept);
			}
		}
	}

	public void SetSelectionText(string text)
	{
		base.ImeMode = ImeMode.Disable;
		if (text == "\r")
		{
			text = "\n";
		}
		if (base.Selection.Length > 0)
		{
			Table item = base.Tables.GetItem();
			if (item != null)
			{
				int start = base.Selection.Start;
				int num = start + base.Selection.Length - 1;
				Select(start, 0);
				TableCell item2 = item.Cells.GetItem();
				Select(num, 0);
				TableCell item3 = item.Cells.GetItem();
				if (item2 != null && item3 != null)
				{
					if (item2.Row == item3.Row && item2.Column == item3.Column)
					{
						Select(start, num - start + 1);
						base.Selection.Text = text;
					}
					else if (text == "")
					{
						for (int i = item2.Row; i <= item3.Row; i++)
						{
							for (int j = item2.Column; j <= item3.Column; j++)
							{
								TableCell item4 = item.Cells.GetItem(i, j);
								if (item4.Length > 0)
								{
									item4.Text = "";
								}
							}
						}
					}
				}
			}
			else
			{
				base.Selection.Text = "";
				if (text != "")
				{
					base.Selection.Text = text;
				}
			}
		}
		else
		{
			base.Selection.Text = text;
		}
		base.ImeMode = ImeMode.NoControl;
	}

	protected override void WndProc(ref Message m)
	{
		// 拦截 WM_GETOBJECT，避免触发 TXTextControl.RawFragmentRootProvider 的
		// NonComVisibleBaseClass MDA 警告（COM QueryInterface 失败）
		// 返回 0 让系统使用默认辅助功能行为
		if (m.Msg == WM_GETOBJECT && m.LParam != IntPtr.Zero)
		{
			m.Result = IntPtr.Zero;
			return;
		}

		switch (m.Msg)
		{
		case 269:
		case 271:
			return;
		case 270:
			if (base.EditMode == EditMode.Edit)
			{
				IntPtr himc = ImmGetContext(base.Handle);
				byte[] buf = null;
				int num = ImmGetCompositionString(himc, 2048, buf, 0);
				buf = new byte[num];
				ImmGetCompositionString(himc, 2048, buf, num);
				string @string = Encoding.Unicode.GetString(buf);
				ImmReleaseContext(base.Handle, himc);
				SetSelectionText(@string);
				FixTableTopLeftMergedRenderBug();
			}
			return;
		case 258:
		{
			if (Control.ModifierKeys.HasFlag(Keys.Control) || Control.ModifierKeys.HasFlag(Keys.Alt))
			{
				base.WndProc(ref m);
				return;
			}
			if (base.EditMode != EditMode.Edit)
			{
				return;
			}
			string text = char.ConvertFromUtf32(m.WParam.ToInt32());
			if (text == "\b")
			{
				if (base.Selection.Length > 0)
				{
					SetSelectionText("");
				}
				else
				{
					if (base.Selection.Start <= 0)
					{
						return;
					}
					Table item = base.Tables.GetItem();
					if (item == null)
					{
						int start = base.Selection.Start;
						Select(start - 1, 1);
						if (base.Tables.GetItem() == null)
						{
							SetSelectionText("");
						}
						else
						{
							Select(start, 0);
						}
					}
					else
					{
						TableCell item2 = item.Cells.GetItem();
						if (item2.Start - 1 != base.Selection.Start)
						{
							Select(base.Selection.Start - 1, 1);
							SetSelectionText("");
						}
					}
				}
				return;
			}
			if (!(text == "\u001b"))
			{
				SetSelectionText(text);
				FixTableTopLeftMergedRenderBug();
				return;
			}
			break;
		}
		case 1:
			try
			{
				base.WndProc(ref m);
				return;
			}
			catch (TextEditorException)
			{
				return;
			}
		}
		base.WndProc(ref m);
	}
}
