using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Gauge;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Auditai.UI.Controls.Properties;

namespace Auditai.UI.Controls;

public class GaugeForm<T> : C1RibbonForm
{
	private Color _progressColor;

	private readonly ProgressForm<T> _owner;

	private readonly Timer timer;

	internal readonly ProgressBarEx _progBar;

	private int num;

	private Point mouseOffset;

	private bool isMouseDown;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

	internal C1Button btnCancel;

	internal C1Label lblMain;

	private C1SplitContainer c1SplitContainer1;

	private C1SplitterPanel pnlHeader;

	private RotatingImage c1PictureBox1;

	private C1SplitterPanel pnlGauge;

	public Color ProgressColor
	{
		get
		{
			_ = _progressColor;
			return _progressColor;
		}
		set
		{
			_progressColor = value;
		}
	}

	public GaugeForm(ProgressForm<T> owner)
	{
		base.FormClosing += GaugeForm_FormClosing;
		timer = new Timer
		{
			Interval = 200
		};
		_owner = owner;
		InitializeComponent();
		c1SplitContainer1.FixedLineWidth = 0;
		c1SplitContainer1.FixedLineColor = Color.Transparent;
		c1PictureBox1.Image = Resource1.imgsub;
		if (c1PictureBox1.Image != null)
		{
			c1PictureBox1.Width = c1PictureBox1.Image.Width + 2;
			c1PictureBox1.Height = c1PictureBox1.Image.Height + 2;
		}
		c1PictureBox1.Top = lblMain.Top + lblMain.Height / 2 - c1PictureBox1.Size.Height / 2;
		_progBar = new ProgressBarEx
		{
			Minimum = 0,
			Maximum = 100,
			Dock = DockStyle.Bottom,
			ForeColor = Theme.SelectedAuditaiTheme.ThemeContext.ProgressBarColor
		};
		pnlGauge.Controls.Add(_progBar);
		pnlHeader.MouseDown += Form1_MouseDown;
		pnlHeader.MouseMove += Form1_MouseMove;
		pnlHeader.MouseUp += Form1_MouseUp;
		pnlHeader.Paint += delegate(object s, PaintEventArgs e)
		{
			e.Graphics.DrawRectangle(new Pen(Color.DarkGray, 1f), new Rectangle(0, 0, base.Width - 1, base.Height - 1));
		};
		pnlGauge.Paint += delegate(object s, PaintEventArgs e)
		{
			e.Graphics.DrawRectangle(new Pen(Color.DarkGray, 1f), new Rectangle(0, 0, base.Width - 1, base.Height - 1));
		};
	}

	public void UpdatePaint()
	{
		c1PictureBox1.Update();
		_progBar.Update();
		lblMain.Update();
	}

	private void StartRotateImage()
	{
		c1PictureBox1.StartRotate(40, 360f);
	}

	private void GaugeForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		c1PictureBox1.StopRotate();
		c1PictureBox1.Dispose();
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		_owner._cts.Cancel();
		Close();
	}

	public new DialogResult ShowDialog()
	{
		try
		{
			Color backColor = Theme.SelectedAuditaiTheme.ThemeContext.BackColor;
			c1SplitContainer1.BackColor = backColor;
			pnlHeader.BackColor = backColor;
			pnlGauge.BackColor = backColor;
			StartRotateImage();
		}
		catch
		{
		}
		return base.ShowDialog();
	}

	private C1LinearGauge CreateLineGauge()
	{
		C1LinearGauge c1LinearGauge = new C1LinearGauge();
		c1LinearGauge.Maximum = 100.0;
		c1LinearGauge.Pointer.Visible = false;
		c1LinearGauge.AxisStart = 0.0;
		c1LinearGauge.AxisLength = 1.0;
		C1GaugeMarks c1GaugeMarks = new C1GaugeMarks();
		c1GaugeMarks.From = 0.0;
		c1GaugeMarks.Interval = 20.0;
		c1GaugeMarks.Length = 20.0;
		c1GaugeMarks.Width = 1.2;
		c1GaugeMarks.Filling = new C1GaugeFilling
		{
			Color = Color.Black
		};
		c1GaugeMarks.Border = new C1GaugeBorder
		{
			LineStyle = C1GaugeBorderStyle.None,
			Thickness = 0.0
		};
		c1GaugeMarks.Alignment = C1GaugeAlignment.Out;
		c1GaugeMarks.AlignmentOffset = 10;
		C1GaugeMarks c1GaugeMarks2 = new C1GaugeMarks();
		c1GaugeMarks2.From = 0.0;
		c1GaugeMarks2.Interval = 5.0;
		c1GaugeMarks2.Length = 10.0;
		c1GaugeMarks2.Width = 0.8;
		c1GaugeMarks2.Filling = new C1GaugeFilling
		{
			Color = Color.Black
		};
		c1GaugeMarks2.Border = new C1GaugeBorder
		{
			LineStyle = C1GaugeBorderStyle.None,
			Thickness = 0.0
		};
		c1GaugeMarks2.Alignment = C1GaugeAlignment.Out;
		c1GaugeMarks2.AlignmentOffset = 25;
		C1GaugeLabels decorator = new C1GaugeLabels
		{
			From = 20.0,
			To = 80.0,
			Interval = 20.0,
			Font = new Font("微软雅黑", 9f),
			Color = Color.Black,
			FontSize = 9.0,
			Alignment = C1GaugeAlignment.Center,
			AlignmentOffset = 30
		};
		C1GaugeLabels decorator2 = new C1GaugeLabels
		{
			From = 0.0,
			To = 20.0,
			Interval = 100.0,
			Font = new Font("微软雅黑", 9f),
			Color = Color.Black,
			FontSize = 9.0,
			Alignment = C1GaugeAlignment.Center,
			AlignmentOffset = 30,
			OrthogonalAlignment = C1GaugeAlignment.Out
		};
		C1GaugeLabels decorator3 = new C1GaugeLabels
		{
			From = 100.0,
			To = 120.0,
			Interval = 100.0,
			Font = new Font("微软雅黑", 9f),
			Color = Color.Black,
			FontSize = 9.0,
			Alignment = C1GaugeAlignment.Center,
			AlignmentOffset = 30,
			OrthogonalAlignment = C1GaugeAlignment.In
		};
		C1GaugeRange c1GaugeRange = new C1GaugeRange();
		c1GaugeRange.Filling = new C1GaugeFilling
		{
			Color = ProgressColor,
			BrushType = C1GaugeBrushType.SolidColor
		};
		c1GaugeRange.AlignmentOffset = 100;
		c1GaugeRange.ToPointerIndex = 100;
		c1LinearGauge.Decorators.Add(c1GaugeRange);
		c1LinearGauge.Decorators.Add(c1GaugeMarks);
		c1LinearGauge.Decorators.Add(c1GaugeMarks2);
		c1LinearGauge.Decorators.Add(decorator);
		c1LinearGauge.Decorators.Add(decorator2);
		c1LinearGauge.Decorators.Add(decorator3);
		return c1LinearGauge;
	}

	private RotateFlipType GetRotate(int num)
	{
		return (num % 4) switch
		{
			0 => RotateFlipType.Rotate90FlipNone, 
			1 => RotateFlipType.Rotate180FlipNone, 
			2 => RotateFlipType.Rotate270FlipNone, 
			_ => RotateFlipType.RotateNoneFlipNone, 
		};
	}

	private void Timer_Tick(object sender, EventArgs e)
	{
		Bitmap imgsub = Resource1.imgsub;
		if (imgsub == null) return;
		RotateFlipType rotate = GetRotate(num++);
		imgsub.RotateFlip(rotate);
		c1PictureBox1.Image = imgsub;
		c1PictureBox1.Update();
	}

	private void Form1_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			int num = -e.X - SystemInformation.FrameBorderSize.Width;
			int num2 = -e.Y - SystemInformation.CaptionHeight - SystemInformation.FrameBorderSize.Height;
			mouseOffset = new Point(num, num2);
			isMouseDown = true;
		}
	}

	private void Form1_MouseMove(object sender, MouseEventArgs e)
	{
		if (isMouseDown)
		{
			Point mousePosition = Control.MousePosition;
			mousePosition.Offset(mouseOffset.X, mouseOffset.Y);
			base.Location = mousePosition;
			Update();
			Application.DoEvents();
		}
	}

	private void Form1_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			isMouseDown = false;
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
		this.btnCancel = new C1.Win.C1Input.C1Button();
		this.lblMain = new C1.Win.C1Input.C1Label();
		this.c1SplitContainer1 = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlHeader = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1PictureBox1 = new Auditai.UI.Controls.RotatingImage();
		this.pnlGauge = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.btnCancel).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.lblMain).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).BeginInit();
		this.c1SplitContainer1.SuspendLayout();
		this.pnlHeader.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1PictureBox1).BeginInit();
		base.SuspendLayout();
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Font = new System.Drawing.Font("微软雅黑", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.btnCancel.Location = new System.Drawing.Point(289, 39);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(70, 26);
		this.btnCancel.TabIndex = 3;
		this.btnCancel.Text = "取消";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.lblMain.BackColor = System.Drawing.Color.Transparent;
		this.lblMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.lblMain.Font = new System.Drawing.Font("微软雅黑", 10.5f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 134);
		this.lblMain.ForeColor = System.Drawing.Color.Black;
		this.lblMain.Location = new System.Drawing.Point(77, 30);
		this.lblMain.MaximumSize = new System.Drawing.Size(375, 48);
		this.lblMain.Name = "lblMain";
		this.lblMain.Size = new System.Drawing.Size(295, 48);
		this.lblMain.TabIndex = 0;
		this.lblMain.Tag = null;
		this.lblMain.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.c1SplitContainer1.AutoSizeElement = C1.Framework.AutoSizeElement.Both;
		this.c1SplitContainer1.BackColor = System.Drawing.Color.FromArgb(164, 195, 235);
		this.c1SplitContainer1.CollapsingAreaColor = System.Drawing.Color.FromArgb(221, 231, 238);
		this.c1SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.c1SplitContainer1.FixedLineColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.c1SplitContainer1.ForeColor = System.Drawing.Color.FromArgb(21, 66, 139);
		this.c1SplitContainer1.Location = new System.Drawing.Point(0, 0);
		this.c1SplitContainer1.Name = "c1SplitContainer1";
		this.c1SplitContainer1.Panels.Add(this.pnlHeader);
		this.c1SplitContainer1.Panels.Add(this.pnlGauge);
		this.c1SplitContainer1.Size = new System.Drawing.Size(400, 150);
		this.c1SplitContainer1.SplitterColor = System.Drawing.Color.FromArgb(119, 147, 185);
		this.c1SplitContainer1.TabIndex = 4;
		this.c1SplitContainer1.ToolTipGradient = C1.Win.C1SplitContainer.ToolTipGradient.Blue;
		this.pnlHeader.Controls.Add(this.c1PictureBox1);
		this.pnlHeader.Controls.Add(this.btnCancel);
		this.pnlHeader.Controls.Add(this.lblMain);
		this.pnlHeader.Height = 90;
		this.pnlHeader.KeepRelativeSize = false;
		this.pnlHeader.Location = new System.Drawing.Point(0, 0);
		this.pnlHeader.Name = "pnlHeader";
		this.pnlHeader.Resizable = false;
		this.pnlHeader.Size = new System.Drawing.Size(400, 90);
		this.pnlHeader.SizeRatio = 52.632;
		this.pnlHeader.TabIndex = 0;
		this.c1PictureBox1.Location = new System.Drawing.Point(25, 30);
		this.c1PictureBox1.Name = "c1PictureBox1";
		this.c1PictureBox1.Size = new System.Drawing.Size(48, 48);
		this.c1PictureBox1.TabIndex = 4;
		this.c1PictureBox1.TabStop = false;
		this.pnlGauge.Height = 59;
		this.pnlGauge.Location = new System.Drawing.Point(0, 91);
		this.pnlGauge.Name = "pnlGauge";
		this.pnlGauge.Size = new System.Drawing.Size(400, 59);
		this.pnlGauge.TabIndex = 1;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(400, 150);
		base.Controls.Add(this.c1SplitContainer1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "GaugeForm";
		base.ShowInTaskbar = false;
		this.Text = "Form1";
		((System.ComponentModel.ISupportInitialize)this.btnCancel).EndInit();
		((System.ComponentModel.ISupportInitialize)this.lblMain).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).EndInit();
		this.c1SplitContainer1.ResumeLayout(false);
		this.pnlHeader.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1PictureBox1).EndInit();
		base.ResumeLayout(false);
	}
}
