﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Auditai.UI.Controls;
using Auditai.Util;
using Newtonsoft.Json.Linq;

namespace Auditai.UI.Platform;

public class frmActivateLicense : Form
{
	private Label lblMachineCodeTitle;

	private TextBox txtMachineCode;

	private Label lblLicenseKeyTitle;

	private TextBox txtLicenseKey;

	private Button btnActivate;

	private Button btnCancel;

	private Label lblTip;

	private IContainer components;

	public frmActivateLicense()
	{
		InitializeComponent();
		StartPosition = FormStartPosition.CenterParent;
		FormBorderStyle = FormBorderStyle.FixedSingle;
		MaximizeBox = false;
		MinimizeBox = false;
		txtMachineCode.Text = MachineCode.Code;
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.lblMachineCodeTitle = new System.Windows.Forms.Label();
		this.txtMachineCode = new System.Windows.Forms.TextBox();
		this.lblLicenseKeyTitle = new System.Windows.Forms.Label();
		this.txtLicenseKey = new System.Windows.Forms.TextBox();
		this.btnActivate = new System.Windows.Forms.Button();
		this.btnCancel = new System.Windows.Forms.Button();
		this.lblTip = new System.Windows.Forms.Label();
		base.SuspendLayout();
		// lblMachineCodeTitle
		this.lblMachineCodeTitle.AutoSize = true;
		this.lblMachineCodeTitle.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblMachineCodeTitle.Location = new System.Drawing.Point(20, 23);
		this.lblMachineCodeTitle.Name = "lblMachineCodeTitle";
		this.lblMachineCodeTitle.Size = new System.Drawing.Size(73, 22);
		this.lblMachineCodeTitle.TabIndex = 0;
		this.lblMachineCodeTitle.Text = "机器码：";
		// txtMachineCode
		this.txtMachineCode.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
		this.txtMachineCode.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtMachineCode.Location = new System.Drawing.Point(104, 20);
		this.txtMachineCode.Name = "txtMachineCode";
		this.txtMachineCode.ReadOnly = true;
		this.txtMachineCode.Size = new System.Drawing.Size(254, 30);
		this.txtMachineCode.TabIndex = 1;
		// lblLicenseKeyTitle
		this.lblLicenseKeyTitle.AutoSize = true;
		this.lblLicenseKeyTitle.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblLicenseKeyTitle.Location = new System.Drawing.Point(20, 72);
		this.lblLicenseKeyTitle.Name = "lblLicenseKeyTitle";
		this.lblLicenseKeyTitle.Size = new System.Drawing.Size(73, 22);
		this.lblLicenseKeyTitle.TabIndex = 2;
		this.lblLicenseKeyTitle.Text = "激活码：";
		// txtLicenseKey
		this.txtLicenseKey.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.txtLicenseKey.Location = new System.Drawing.Point(104, 68);
		this.txtLicenseKey.Name = "txtLicenseKey";
		this.txtLicenseKey.Size = new System.Drawing.Size(254, 30);
		this.txtLicenseKey.TabIndex = 3;
		// lblTip
		this.lblTip.AutoSize = false;
		this.lblTip.Font = new System.Drawing.Font("Noto Sans SC", 8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblTip.ForeColor = System.Drawing.Color.Gray;
		this.lblTip.Location = new System.Drawing.Point(20, 111);
		this.lblTip.Name = "lblTip";
		this.lblTip.Size = new System.Drawing.Size(338, 42);
		this.lblTip.TabIndex = 4;
		this.lblTip.Text = "请联系管理员获取激活码，激活后需重新登录";
		this.lblTip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		// btnActivate
		this.btnActivate.BackColor = System.Drawing.Color.FromArgb(0, 195, 245);
		this.btnActivate.FlatAppearance.BorderSize = 0;
		this.btnActivate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnActivate.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnActivate.ForeColor = System.Drawing.Color.White;
		this.btnActivate.Location = new System.Drawing.Point(104, 163);
		this.btnActivate.Name = "btnActivate";
		this.btnActivate.Size = new System.Drawing.Size(117, 39);
		this.btnActivate.TabIndex = 5;
		this.btnActivate.Text = "激活";
		this.btnActivate.UseVisualStyleBackColor = false;
		this.btnActivate.Click += new System.EventHandler(btnActivate_Click);
		// btnCancel
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.FlatAppearance.BorderSize = 0;
		this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnCancel.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCancel.Location = new System.Drawing.Point(241, 163);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(117, 39);
		this.btnCancel.TabIndex = 6;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		// frmActivateLicense
		base.AcceptButton = this.btnActivate;
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(390, 228);
		base.Controls.Add(this.lblTip);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnActivate);
		base.Controls.Add(this.txtLicenseKey);
		base.Controls.Add(this.lblLicenseKeyTitle);
		base.Controls.Add(this.txtMachineCode);
		base.Controls.Add(this.lblMachineCodeTitle);
		this.Font = new System.Drawing.Font("Noto Sans SC", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmActivateLicense";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "License 激活";
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	private async void btnActivate_Click(object sender, EventArgs e)
	{
		string licenseKey = txtLicenseKey.Text.Trim();
		if (string.IsNullOrEmpty(licenseKey))
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "请输入激活码");
			return;
		}
		string machineCode = MachineCode.Code;
		try
		{
			btnActivate.Enabled = false;
			// 调用 WebApiClient 激活 License
			var result = await WebApiClient.ActivateLicense(licenseKey, machineCode);
			if (result != null && result["activated"]?.Value<bool>() == true)
			{
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "激活成功，请重新登录");
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			else
			{
				string errorMsg = result?["message"]?.Value<string>() ?? "激活失败，请检查 LicenseKey";
				Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, errorMsg);
			}
			return;
		}
		catch (Exception ex)
		{
			ex.Log("激活 License 失败");
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "激活失败：" + ex.Message);
			return;
		}
		finally
		{
			btnActivate.Enabled = true;
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
}
