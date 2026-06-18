﻿using System;
using System.Windows.Forms;

namespace Leqisoft.UI.Platform;

public class LedgerCollectFormulaEditor
{
	public bool IsEditing { get; set; }
	public dynamic View { get; set; }
	public event EventHandler Closed;

	public void InsertRefTextAndFocus(params object[] args)
	{
		try
		{
			if (args.Length > 0 && args[0] is string refText)
			{
				if (View != null)
				{
					View.Value = (View.Value ?? "") + refText;
					View.Focus();
				}
			}
		}
		catch
		{
			// 插入引用文本失败时静默处理
		}
	}

	public void RemoveRefAtPos()
	{
		try
		{
			if (View != null)
			{
				// 移除当前位置的引用：将 View.Value 置空
				View.Value = "";
			}
		}
		catch
		{
			// 移除引用失败时静默处理
		}
	}

	public bool UseWildcard() { return false; }
	
	public void New()
	{
		IsEditing = true;
		View = null;
	}
	
	public static void ShowEditor(params object[] args)
	{
		try
		{
			// 显示公式编辑器窗口
			// args[0]: 父控件, args[1]: Table, args[2]: Column
			if (args.Length > 0 && args[0] is Control parent)
			{
				Form editorForm = new Form
				{
					Text = "账套采集公式编辑器",
					Size = new System.Drawing.Size(600, 400),
					StartPosition = FormStartPosition.CenterParent,
					ShowInTaskbar = false
				};
				var editor = new LedgerCollectFormulaEditor();
				editor.View = new TextBox { Dock = DockStyle.Fill, Multiline = true };
				editor.View.Dock = DockStyle.Fill;
				editorForm.Controls.Add((Control)editor.View);
				editorForm.ShowDialog(parent);
			}
		}
		catch
		{
			// 显示编辑器失败时静默处理
		}
	}
}