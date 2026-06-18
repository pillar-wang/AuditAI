﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Ribbon;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class frmDownloadInstallPackage : C1RibbonForm
{
	private bool _isExistApplication;

	private IContainer components;

	private Label label1;

	private Label label2;

	private LinkLabel linkLabel1;

	public frmDownloadInstallPackage(bool isExitApplication)
	{
		InitializeComponent();
		_isExistApplication = isExitApplication;
		base.Icon = Resources.warningIcon16;
		base.FormClosed += FrmDownloadInstallPackage_FormClosed;
	}

	private void FrmDownloadInstallPackage_FormClosed(object sender, FormClosedEventArgs e)
	{
		try
		{
			if (_isExistApplication)
			{
				Application.Exit();
			}
		}
		catch
		{
		}
	}

	private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo("about:blank")
			{
				UseShellExecute = true
			});
		}
		catch (Exception)
		{
		}
		finally
		{
			Close();
		}
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
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.linkLabel1 = new System.Windows.Forms.LinkLabel();
		base.SuspendLayout();
		this.label1.BackColor = System.Drawing.Color.Transparent;
		this.label1.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.label1.Location = new System.Drawing.Point(12, 13);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(541, 23);
		this.label1.TabIndex = 0;
		this.label1.Text = "找不到更新程序LeqiUpdater.exe，请登录官方网站下载安装包重新进行安装！";
		this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.label2.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.label2.Location = new System.Drawing.Point(159, 56);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(70, 23);
		this.label2.TabIndex = 1;
		this.label2.Text = "下载地址:";
		this.linkLabel1.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.linkLabel1.LinkColor = System.Drawing.Color.FromArgb(0, 102, 204);
		this.linkLabel1.Location = new System.Drawing.Point(228, 55);
		this.linkLabel1.Name = "linkLabel1";
		this.linkLabel1.Size = new System.Drawing.Size(222, 23);
		this.linkLabel1.TabIndex = 2;
		this.linkLabel1.TabStop = true;
		this.linkLabel1.Text = "about:blank";
		this.linkLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(linkLabel1_LinkClicked);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.BackgroundColor = System.Drawing.SystemColors.Control;
		base.ClientSize = new System.Drawing.Size(565, 108);
		base.Controls.Add(this.linkLabel1);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmDownloadInstallPackage";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "错误！";
		base.TopMost = true;
		base.ResumeLayout(false);
	}
}
