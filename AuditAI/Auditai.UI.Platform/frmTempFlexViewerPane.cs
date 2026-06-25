using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.FlexViewer;

namespace Auditai.UI.Platform;

public class frmTempFlexViewerPane : Form
{
	internal C1FlexViewerPane _fvp = new C1FlexViewerPane
	{
		Dock = DockStyle.Fill
	};

	private IContainer components;

	public frmTempFlexViewerPane()
	{
		InitializeComponent();
		base.Load += FrmTempFlexViewerPane_Load;
	}

	private void FrmTempFlexViewerPane_Load(object sender, EventArgs e)
	{
		base.Controls.Add(_fvp);
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
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1, 1);
		base.ControlBox = false;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "frmTempFlexViewerPane";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "frmTempFlexViewerPane";
		base.ResumeLayout(false);
	}
}
