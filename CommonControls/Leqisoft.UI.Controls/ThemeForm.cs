﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Ribbon;
using Leqisoft.DTO;
using Leqisoft.UI.Controls.Properties;

namespace Leqisoft.UI.Controls;

public class ThemeForm : C1RibbonForm
{
	private ThemeTile themeTile;

	public EventHandler<LeqiTheme> SelectedThemeChanged;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	public ThemeForm()
	{
		InitializeComponent();
		base.Shown += ThemeForm_Shown;
		base.StartPosition = FormStartPosition.CenterScreen;
		themeTile = new ThemeTile();
		base.Controls.Add(themeTile.View);
		themeTile.AfterSelectedThemeChanged += Tile_AfterSelectedTileChanged;
	}

	private void ThemeForm_Shown(object sender, EventArgs e)
	{
		base.Icon = Icon.FromHandle(Resources.Theme.GetHicon());
	}

	public void SelectTheme(string themeId)
	{
		themeTile.CheckTile(themeId);
	}

	public void ShowForm()
	{
		Show();
	}

	private void ThemeForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (e.CloseReason != CloseReason.ApplicationExitCall)
		{
			e.Cancel = true;
			Hide();
		}
	}

	private void Tile_AfterSelectedTileChanged(object sender, string themeId)
	{
		Theme.SelectedThemeById(themeId);
		UserSet.Config.CurrentTheme = themeId;
		SelectedThemeChanged?.Invoke(this, Theme.SelectedLeqiTheme);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 17f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(454, 324);
		this.Font = new System.Drawing.Font("Microsoft YaHei", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ThemeForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "设置皮肤";
		base.TopMost = true;
		base.VisualStyleHolder = C1.Win.C1Ribbon.VisualStyle.Office2010Blue;
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(ThemeForm_FormClosing);
		base.ResumeLayout(false);
	}
}
