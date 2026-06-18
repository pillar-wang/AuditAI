﻿﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using C1.Framework;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using Leqisoft.UI.Controls.Properties;

namespace Leqisoft.UI.Controls;

public class ProgressForm2 : C1RibbonForm
{
	protected class EnumeratorRunner
	{
		protected Form _mainForm;

		protected ProgressForm2 _parent;

		protected IEnumerator _callback;

		public EnumeratorRunner(Form mainForm, ProgressForm2 parent, IEnumerator it)
		{
			_parent = parent;
			_callback = it;
			_mainForm = mainForm;
		}

		public void StartRun()
		{
			_mainForm.BeginInvoke(new Action(OnRunOnceUpdate));
		}

		private void OnRunOnceUpdate()
		{
			try
			{
				if (_callback.MoveNext())
				{
					_mainForm.BeginInvoke(new Action(OnRunOnceUpdate));
				}
				else
				{
					_parent.CloseOnTaskEnd();
				}
			}
			catch (Exception ex)
			{
				ex.Log();
				_parent.CloseWithException(ex);
			}
		}
	}

	protected class CallbackRunner
	{
		protected ProgressForm2 _parent;

		protected Action<ProgressForm2> _callback;

		public CallbackRunner(ProgressForm2 parent, Action<ProgressForm2> callback)
		{
			_parent = parent;
			_callback = callback;
		}

		public void StartRunAsync()
		{
			ThreadPool.QueueUserWorkItem(RunCallback, null);
		}

		protected void RunCallback(object state)
		{
			try
			{
				_callback(_parent);
			}
			catch (Exception ex)
			{
				ex.Log();
				_parent.CloseWithException(ex);
			}
			finally
			{
				_parent.IsTaskThreadEnd = true;
			}
		}
	}

	protected class TaskRunner
	{
		protected ProgressForm2 _parent;

		protected Task _task;

		public TaskRunner(ProgressForm2 parent, Task task)
		{
			_parent = parent;
			_task = task;
		}

		public void StartRunAsync()
		{
			Task.Run(async delegate
			{
				_ = 1;
				try
				{
					await Task.Delay(1).ConfigureAwait(continueOnCapturedContext: false);
					if (_task != null)
					{
						await _task;
					}
				}
				catch (Exception ex)
				{
					ex.Log();
					_parent.CloseWithException(ex);
				}
				finally
				{
					_parent.IsTaskThreadEnd = true;
				}
			});
		}
	}

	private Color _progressColor;

	private readonly System.Windows.Forms.Timer _progressValueUpdateTimer;

	private readonly ProgressBarEx _progBar;

	private ExceptionDispatchInfo _causeClosedException;

	private ProgressRuntimeData _progressRuntimeData;

	private ProgressSnapshotData _lastSnapshotData;

	private bool _isDelayToShow;

	private int _delayToShowMilliseconds;

	private DateTime _dialogBecomeVisibleTime = DateTime.Now;

	private string _progressPercentDisplayFormat = "{0}%";

	private bool _isTaskThreadEnd;

	private object _lockObject = new object();

	private C1LabelEx _progressValueLabel;

	private ProgressDisplayValueConverter _progressDisplayValueConverter;

	private ProgressDisplayValueConverter _currentInUseConverter;

	private bool _isWindowCreated;

	public object UserData;

	private int _taskEndRemainShowTimes = -1;

	private Point mouseOffset;

	private bool isMouseDown;

	#pragma warning disable CS0649
	private IContainer components;
#pragma warning restore CS0649

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

	protected bool IsTaskThreadEnd
	{
		get
		{
			lock (_lockObject)
			{
				return _isTaskThreadEnd;
			}
		}
		set
		{
			lock (_lockObject)
			{
				_isTaskThreadEnd = value;
			}
		}
	}

	public string ProgressPercentDisplayFormat
	{
		get
		{
			lock (_lockObject)
			{
				return _progressPercentDisplayFormat;
			}
		}
		set
		{
			lock (_lockObject)
			{
				_progressPercentDisplayFormat = value;
			}
		}
	}

	protected ProgressDisplayValueConverter ProgressDisplayValueConverter
	{
		get
		{
			lock (_lockObject)
			{
				return _progressDisplayValueConverter;
			}
		}
		set
		{
			lock (_lockObject)
			{
				_progressDisplayValueConverter = value;
			}
		}
	}

	protected bool IsWindowCreated
	{
		get
		{
			lock (_lockObject)
			{
				return _isWindowCreated;
			}
		}
		set
		{
			lock (_lockObject)
			{
				_isWindowCreated = value;
			}
		}
	}

	protected ExceptionDispatchInfo CaseCloseException
	{
		get
		{
			lock (_lockObject)
			{
				return _causeClosedException;
			}
		}
		set
		{
			lock (_lockObject)
			{
				_causeClosedException = value;
			}
		}
	}

	public ProgressForm2(ProgressDisplayValueConverter progressValueConverter)
		: this()
	{
		_progressDisplayValueConverter = progressValueConverter;
	}

	public ProgressForm2()
	{
		base.FormClosing += GaugeForm_FormClosing;
		_progressValueUpdateTimer = new System.Windows.Forms.Timer
		{
			Interval = 100
		};
		InitializeComponent();
		c1SplitContainer1.FixedLineWidth = 0;
		c1SplitContainer1.FixedLineColor = Color.Transparent;
		c1PictureBox1.Image = Resource1.imgsub;
		c1PictureBox1.Width = c1PictureBox1.Image.Width + 2;
		c1PictureBox1.Height = c1PictureBox1.Image.Height + 2;
		c1PictureBox1.Top = lblMain.Top + lblMain.Height / 2 - c1PictureBox1.Size.Height / 2;
		_progBar = new ProgressBarEx
		{
			Minimum = 0,
			Maximum = 100,
			Dock = DockStyle.Bottom,
			ForeColor = Theme.SelectedLeqiTheme.ThemeContext.ProgressBarColor
		};
		pnlGauge.Controls.Add(_progBar);
		_progressValueLabel = new C1LabelEx
		{
			TextDetached = true
		};
		_progressValueLabel.Text = "";
		_progressValueLabel.Dock = DockStyle.None;
		_progressValueLabel.TextAlign = ContentAlignment.BottomCenter;
		_progressValueLabel.Width = 150;
		_progressValueLabel.Height = 40;
		_progressValueLabel.Font = new Font(lblMain.Font.FontFamily, 9f);
		_progressValueLabel.TextColor = Color.Black;
		pnlGauge.Controls.Add(_progressValueLabel);
		_progressValueLabel.Location = new Point((pnlGauge.Width - _progressValueLabel.Width) / 2, pnlGauge.Height - _progBar.Height);
		pnlHeader.MouseDown += Form1_MouseDown;
		pnlHeader.MouseMove += Form1_MouseMove;
		pnlHeader.MouseUp += Form1_MouseUp;
		_progressValueUpdateTimer.Tick += ProgressValueUpdateTimer_Tick;
		pnlHeader.Paint += delegate(object s, PaintEventArgs e)
		{
			e.Graphics.DrawRectangle(new Pen(Color.DarkGray, 1f), new Rectangle(0, 0, base.Width - 1, base.Height - 1));
		};
		pnlGauge.Paint += delegate(object s, PaintEventArgs e)
		{
			e.Graphics.DrawRectangle(new Pen(Color.DarkGray, 1f), new Rectangle(0, 0, base.Width - 1, base.Height - 1));
		};
		base.Shown += ProgressForm2_Shown;
	}

	private void ProgressForm2_Shown(object sender, EventArgs e)
	{
		IsWindowCreated = true;
		try
		{
			if (IsTaskThreadEnd)
			{
				Close();
			}
		}
		catch (Exception)
		{
		}
	}

	public void SetProgressDisplayValueConverter(ProgressDisplayValueConverter converter)
	{
		ProgressDisplayValueConverter = converter;
	}

	private void GaugeForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		_progressValueUpdateTimer.Stop();
		_progressValueUpdateTimer.Tick -= ProgressValueUpdateTimer_Tick;
		_progressValueUpdateTimer.Dispose();
		c1PictureBox1.StopRotate();
		c1PictureBox1.Dispose();
	}

	private void StartRotateImage()
	{
		c1PictureBox1.StartRotate(40, 360f);
	}

	private void InitLableDisplayValue(ProgressRuntimeData progressRuntimeData)
	{
		ProgressSnapshotData progressData = progressRuntimeData.CreateSnapshot();
		_progBar.Value = _progBar.Minimum;
		_progressValueLabel.Text = GetProgressPercentDisplayString(_progBar.Value);
		lblMain.Value = GetProgressDisplayMessage(progressData, _progBar.Value);
	}

	public DialogResult ShowDialog(ProgressRuntimeData progressRuntimeData, Action<ProgressForm2> callback)
	{
		try
		{
			Color backColor = Theme.SelectedLeqiTheme.ThemeContext.BackColor;
			c1SplitContainer1.BackColor = backColor;
			pnlHeader.BackColor = backColor;
			pnlGauge.BackColor = backColor;
			_progressValueLabel.TextColor = Theme.SelectedLeqiTheme.ThemeContext.ProgressBarColor;
		}
		catch
		{
		}
		_progressRuntimeData = progressRuntimeData;
		_isDelayToShow = false;
		StartRotateImage();
		InitLableDisplayValue(progressRuntimeData);
		IsTaskThreadEnd = false;
		CallbackRunner callbackRunner = new CallbackRunner(this, callback);
		callbackRunner.StartRunAsync();
		_progressValueUpdateTimer.Start();
		DialogResult result = ShowDialog();
		CaseCloseException?.Throw();
		return result;
	}

	public DialogResult ShowDialog(ProgressRuntimeData progressRuntimeData, Func<ProgressForm2, Task> task)
	{
		try
		{
			Color backColor = Theme.SelectedLeqiTheme.ThemeContext.BackColor;
			c1SplitContainer1.BackColor = backColor;
			pnlHeader.BackColor = backColor;
			pnlGauge.BackColor = backColor;
			_progressValueLabel.TextColor = Theme.SelectedLeqiTheme.ThemeContext.ProgressBarColor;
		}
		catch
		{
		}
		_progressRuntimeData = progressRuntimeData;
		_isDelayToShow = false;
		StartRotateImage();
		InitLableDisplayValue(progressRuntimeData);
		IsTaskThreadEnd = false;
		TaskRunner taskRunner = new TaskRunner(this, task(this) ?? Task.CompletedTask);
		taskRunner.StartRunAsync();
		_progressValueUpdateTimer.Start();
		DialogResult result = ShowDialog();
		CaseCloseException?.Throw();
		return result;
	}

	public async Task ShowDialog(ProgressRuntimeData progressRuntimeData, Func<Task> task, int delayMilliseconds)
	{
		_progressRuntimeData = progressRuntimeData;
		IsTaskThreadEnd = false;
		TaskRunner taskRunner = new TaskRunner(this, task() ?? Task.CompletedTask);
		taskRunner.StartRunAsync();
		_delayToShowMilliseconds = delayMilliseconds;
		_dialogBecomeVisibleTime = DateTime.Now.AddMilliseconds(delayMilliseconds);
		while (DateTime.Now < _dialogBecomeVisibleTime)
		{
			await Task.Delay(10);
			if (IsTaskThreadEnd)
			{
				CaseCloseException?.Throw();
				return;
			}
		}
		if (IsTaskThreadEnd)
		{
			CaseCloseException?.Throw();
			return;
		}
		try
		{
			Color backColor = Theme.SelectedLeqiTheme.ThemeContext.BackColor;
			c1SplitContainer1.BackColor = backColor;
			pnlHeader.BackColor = backColor;
			pnlGauge.BackColor = backColor;
			_progressValueLabel.TextColor = Theme.SelectedLeqiTheme.ThemeContext.ProgressBarColor;
		}
		catch
		{
		}
		StartRotateImage();
		InitLableDisplayValue(_progressRuntimeData);
		_isDelayToShow = false;
		_progressValueUpdateTimer.Start();
		ShowDialog();
		CaseCloseException?.Throw();
	}

	public DialogResult ShowDialog_RunCallbackInMainThread(Form mainForm, ProgressRuntimeData progressRuntimeData, Func<IEnumerator> callback)
	{
		try
		{
			Color backColor = Theme.SelectedLeqiTheme.ThemeContext.BackColor;
			c1SplitContainer1.BackColor = backColor;
			pnlHeader.BackColor = backColor;
			pnlGauge.BackColor = backColor;
			_progressValueLabel.TextColor = Theme.SelectedLeqiTheme.ThemeContext.ProgressBarColor;
		}
		catch
		{
		}
		_progressRuntimeData = progressRuntimeData;
		_isDelayToShow = false;
		StartRotateImage();
		InitLableDisplayValue(progressRuntimeData);
		IsTaskThreadEnd = false;
		EnumeratorRunner enumeratorRunner = new EnumeratorRunner(mainForm, this, callback());
		enumeratorRunner.StartRun();
		_progressValueUpdateTimer.Start();
		DialogResult result = ShowDialog();
		CaseCloseException?.Throw();
		return result;
	}

	protected void CloseWithException(Exception e)
	{
		try
		{
			CaseCloseException = ExceptionDispatchInfo.Capture(e);
			try
			{
				Invoke(new Action(base.Close));
			}
			catch
			{
			}
		}
		catch (Exception exception)
		{
			exception.Log();
		}
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

	private void CloseOnTaskEnd()
	{
		_taskEndRemainShowTimes = 2;
		_progBar.Value = _progBar.Maximum;
		_progressValueLabel.Text = GetProgressPercentDisplayString(_progBar.Value);
	}

	private string GetProgressDisplayMessage(ProgressSnapshotData progressData, int progressValue)
	{
		return progressData.Message + progressData.PostfixString;
	}

	private string GetProgressPercentDisplayString(int progressValue)
	{
		if (ProgressPercentDisplayFormat != null)
		{
			return string.Format(ProgressPercentDisplayFormat, progressValue);
		}
		return string.Empty;
	}

	private float GetProgressDisplayValue(ProgressSnapshotData snapshotData)
	{
		return _currentInUseConverter?.GetProgressDislayValue(snapshotData) ?? snapshotData.ProgressValue;
	}

	private void ProgressValueUpdateTimer_Tick(object sender, EventArgs e)
	{
		try
		{
			if (_taskEndRemainShowTimes >= 0)
			{
				if (_taskEndRemainShowTimes == 0)
				{
					Close();
				}
				else
				{
					_taskEndRemainShowTimes--;
				}
				return;
			}
			if (_isDelayToShow)
			{
				if (IsTaskThreadEnd)
				{
					Close();
					return;
				}
				if (!(DateTime.Now >= _dialogBecomeVisibleTime))
				{
					return;
				}
				base.Opacity = 100.0;
				_isDelayToShow = false;
				StartRotateImage();
			}
			ProgressDisplayValueConverter progressDisplayValueConverter = ProgressDisplayValueConverter;
			if (progressDisplayValueConverter != _currentInUseConverter)
			{
				progressDisplayValueConverter?.StartTimer();
				_currentInUseConverter = progressDisplayValueConverter;
			}
			if (_lastSnapshotData == null)
			{
				_lastSnapshotData = _progressRuntimeData.CreateSnapshot();
				if (_lastSnapshotData.IsTaskEnd)
				{
					CloseOnTaskEnd();
					lblMain.Value = GetProgressDisplayMessage(_lastSnapshotData, _progBar.Value);
					return;
				}
				if (IsTaskThreadEnd)
				{
					CloseOnTaskEnd();
					lblMain.Value = GetProgressDisplayMessage(_lastSnapshotData, _progBar.Value);
					return;
				}
				int num = (int)(GetProgressDisplayValue(_lastSnapshotData) * (float)_progBar.Maximum);
				if (num > _progBar.Value)
				{
					_progBar.Value = Math.Min(_progBar.Maximum, num);
					_progressValueLabel.Text = GetProgressPercentDisplayString(_progBar.Value);
				}
				lblMain.Value = GetProgressDisplayMessage(_lastSnapshotData, _progBar.Value);
				return;
			}
			ProgressSnapshotData progressSnapshotData = _progressRuntimeData.CreateSnapshot();
			if (progressSnapshotData.IsTaskEnd)
			{
				CloseOnTaskEnd();
				lblMain.Value = GetProgressDisplayMessage(_lastSnapshotData, _progBar.Value);
			}
			else if (IsTaskThreadEnd)
			{
				CloseOnTaskEnd();
				lblMain.Value = GetProgressDisplayMessage(_lastSnapshotData, _progBar.Value);
			}
			else if (progressSnapshotData.Step != _lastSnapshotData.Step && _progBar.Value != 0)
			{
				if (_progBar.Value != _progBar.Maximum)
				{
					_progBar.Value = _progBar.Maximum;
					lblMain.Value = GetProgressDisplayMessage(_lastSnapshotData, _progBar.Value);
					_progressValueLabel.Text = GetProgressPercentDisplayString(_progBar.Value);
				}
				else
				{
					_lastSnapshotData = progressSnapshotData;
					_progBar.Value = 0;
					lblMain.Value = GetProgressDisplayMessage(progressSnapshotData, _progBar.Value);
					_progressValueLabel.Text = GetProgressPercentDisplayString(_progBar.Value);
				}
			}
			else
			{
				int num2 = (int)(GetProgressDisplayValue(_lastSnapshotData) * (float)_progBar.Maximum);
				if (num2 > _progBar.Value)
				{
					_progBar.Value = Math.Min(_progBar.Maximum, num2);
					_progressValueLabel.Text = GetProgressPercentDisplayString(_progBar.Value);
				}
				lblMain.Value = GetProgressDisplayMessage(progressSnapshotData, _progBar.Value);
				_lastSnapshotData = progressSnapshotData;
			}
		}
		catch
		{
		}
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
		this.lblMain = new C1.Win.C1Input.C1Label();
		this.c1SplitContainer1 = new C1.Win.C1SplitContainer.C1SplitContainer();
		this.pnlHeader = new C1.Win.C1SplitContainer.C1SplitterPanel();
		this.c1PictureBox1 = new Leqisoft.UI.Controls.RotatingImage();
		this.pnlGauge = new C1.Win.C1SplitContainer.C1SplitterPanel();
		((System.ComponentModel.ISupportInitialize)this.lblMain).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).BeginInit();
		this.c1SplitContainer1.SuspendLayout();
		this.pnlHeader.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.c1PictureBox1).BeginInit();
		base.SuspendLayout();
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
		base.Name = "ProgressForm2";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Form1";
		((System.ComponentModel.ISupportInitialize)this.lblMain).EndInit();
		((System.ComponentModel.ISupportInitialize)this.c1SplitContainer1).EndInit();
		this.c1SplitContainer1.ResumeLayout(false);
		this.pnlHeader.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.c1PictureBox1).EndInit();
		base.ResumeLayout(false);
	}
}
