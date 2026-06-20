using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Leqisoft.Model;
using Leqisoft.DTO;
using TXTextControl;
using Leqisoft.UI.Controls;


namespace Leqisoft.UI.Platform;

/// <summary>
/// 文档校验域管理对话框。
/// 列出当前文档中所有 DocValidation 域和带有稽核规则的 Formula 域，
/// 支持删除校验点和导航到指定域。
/// </summary>
public class frmDocValidationMgmt : Form
{
	private readonly DocumentEditor _editor;
	private DataGridView _grid;
	private System.Windows.Forms.Button _btnNavigate;
	private System.Windows.Forms.Button _btnDelete;
	private System.Windows.Forms.Button _btnClose;

	private sealed class ValidationItem
	{
		public int Index { get; set; }
		public string Type { get; set; }      // "校验点" / "稽核规则"
		public string FieldId { get; set; }
		public string Content { get; set; }    // 域文字
		public string Rule { get; set; }       // 规则摘要
		public int TextPosition { get; set; }  // 域起始位置
	}

	private List<ValidationItem> _items = new List<ValidationItem>();

	public frmDocValidationMgmt(DocumentEditor editor)
	{
		_editor = editor;
		InitializeComponents();
		LoadValidationItems();
	}

	private void InitializeComponents()
	{
		this.Text = "文档校验域管理";
		this.Size = new Size(700, 450);
		this.StartPosition = FormStartPosition.CenterParent;
		this.MinimizeBox = false;
		this.MaximizeBox = false;
		this.ShowInTaskbar = false;
		this.FormBorderStyle = FormBorderStyle.FixedDialog;

		_grid = new DataGridView
		{
			Location = new Point(12, 12),
			Size = new Size(660, 340),
			ReadOnly = true,
			AllowUserToAddRows = false,
			AllowUserToDeleteRows = false,
			AllowUserToResizeRows = false,
			MultiSelect = false,
			SelectionMode = DataGridViewSelectionMode.FullRowSelect,
			RowHeadersVisible = false,
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
			BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D,
			BackgroundColor = SystemColors.Window,
		};

		_grid.Columns.Add("Type", "类型");
		_grid.Columns.Add("Content", "内容");
		_grid.Columns.Add("Rule", "规则");
		_grid.Columns["Type"].Width = 80;
		_grid.Columns["Content"].Width = 200;
		_grid.Columns["Rule"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

		_grid.CellDoubleClick += (s, e) => NavigateToSelected();
		this.Controls.Add(_grid);

		int btnY = 365;
		_btnNavigate = new System.Windows.Forms.Button
		{
			Text = "导航到",
			Location = new Point(12, btnY),
			Size = new Size(90, 28),
		};
		_btnNavigate.Click += (s, e) => NavigateToSelected();
		this.Controls.Add(_btnNavigate);

		_btnDelete = new System.Windows.Forms.Button
		{
			Text = "删除校验点",
			Location = new Point(110, btnY),
			Size = new Size(100, 28),
		};
		_btnDelete.Click += (s, e) => DeleteSelected();
		this.Controls.Add(_btnDelete);

		_btnClose = new System.Windows.Forms.Button
		{
			Text = "关闭",
			DialogResult = DialogResult.Cancel,
			Location = new Point(600, btnY),
			Size = new Size(70, 28),
		};
		this.Controls.Add(_btnClose);

		this.CancelButton = _btnClose;
	}

	private void LoadValidationItems()
	{
		_items.Clear();
		int index = 0;

		if (_editor.Tx == null) return;

		foreach (TXTextControl.ApplicationField f in _editor.Tx.ApplicationFields)
		{
			if (f == null || f.TypeName != "MERGEFIELD" || f.Parameters == null || f.Parameters.Length < 1)
				continue;

			if (f.Parameters[0] == "DocValidation" && f.Parameters.Length >= 2
				&& !string.IsNullOrWhiteSpace(f.Parameters[1]))
			{
				// 解析规则摘要
				string ruleSummary = TruncateText(f.Parameters[1], 50);
				_items.Add(new ValidationItem
				{
					Index = index++,
					Type = "校验点",
					FieldId = f.Parameters.Length >= 3 ? f.Parameters[2] : "",
					Content = TruncateText(f.Text, 30),
					Rule = ruleSummary,
					TextPosition = f.Start,
				});
			}
			else if (f.Parameters[0] == "Formula" && f.Parameters.Length >= 4
				&& !string.IsNullOrWhiteSpace(f.Parameters[3]))
			{
				// 带稽核规则的 Formula 域
				string ruleSummary = TruncateText(f.Parameters[3], 50);
				_items.Add(new ValidationItem
				{
					Index = index++,
					Type = "稽核规则",
					FieldId = f.Parameters.Length >= 3 ? f.Parameters[2] : "",
					Content = TruncateText(f.Text, 30),
					Rule = ruleSummary,
					TextPosition = f.Start,
				});
			}
		}

		_grid.Rows.Clear();
		foreach (var item in _items)
		{
			_grid.Rows.Add(item.Type, item.Content, item.Rule);
		}

		if (_items.Count == 0)
		{
			_grid.Rows.Add("(无)", "(文档中没有校验点或稽核规则)", "");
		}
	}

	private void NavigateToSelected()
	{
		if (_grid.SelectedRows.Count == 0) return;
		int rowIdx = _grid.SelectedRows[0].Index;
		if (rowIdx < 0 || rowIdx >= _items.Count) return;

		var item = _items[rowIdx];
		try
		{
			_editor.Tx.Select(item.TextPosition, 0);
			_editor.Tx.Focus();
		}
		catch { }
	}

	private void DeleteSelected()
	{
		if (_grid.SelectedRows.Count == 0) return;
		int rowIdx = _grid.SelectedRows[0].Index;
		if (rowIdx < 0 || rowIdx >= _items.Count) return;

		var item = _items[rowIdx];

		if (Leqisoft.UI.Controls.MessageBox.Show(MessageBoxIcon.Question,
				"确定要删除此" + item.Type + "吗？\n\n内容：" + item.Content,
				MessageBoxButtons.YesNo,
				"确认删除") != DialogResult.Yes)
			return;

		try
		{
			// 导航到域位置
			_editor.Tx.Select(item.TextPosition, 0);
			_editor.Tx.Focus();

			if (item.Type == "校验点")
			{
				_editor.RemoveValidationPoint();
			}
			else if (item.Type == "稽核规则")
			{
				var field = _editor.GetCurrentApplicationField();
				if (field != null && field.TypeName == "MERGEFIELD"
					&& field.Parameters != null && field.Parameters.Length >= 4)
				{
					// 清空稽核规则
					var newParams = new List<string>(field.Parameters);
					newParams[3] = string.Empty;
					field.Parameters = newParams.ToArray();
				}
			}

			// 刷新列表
			LoadValidationItems();
		}
		catch (Exception ex)
		{
			ex.Log("frmDocValidationMgmt.DeleteSelected");
		}
	}

	private static string TruncateText(string text, int maxLen)
	{
		if (string.IsNullOrEmpty(text)) return string.Empty;
		return text.Length <= maxLen ? text : text.Substring(0, maxLen) + "...";
	}
}