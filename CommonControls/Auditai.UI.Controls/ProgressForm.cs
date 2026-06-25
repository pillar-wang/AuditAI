using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Auditai.DTO;

namespace Auditai.UI.Controls;

public class ProgressForm<T>
{
	private delegate void InvokeDelegate();

	internal CancellationTokenSource _cts = new CancellationTokenSource();

	private GaugeForm<T> _progressForm;

	private bool _closed;

	private Func<IProgress<ProgressInfo>, Task<T>> _taskFactory;

	private Func<Task<T>> _taskFactory1;

	private bool _closingByCode;

	private TimeSpan _delay;

	private System.Windows.Forms.Timer _timer;

	private int _progressBarValue;

	private object _progressBarValueLocker = new object();

	private bool _isForceToProgressEnd;

	private Func<ProgressInfo> _reportProgress;

	public Color ThemeColor { get; set; }

	public Progress<ProgressInfo> Progress { get; } = new Progress<ProgressInfo>();


	public Task<T> Task { get; private set; }

	protected int LastDisplayProgressBarValue
	{
		get
		{
			lock (_progressBarValueLocker)
			{
				return _progressBarValue;
			}
		}
		set
		{
			lock (_progressBarValueLocker)
			{
				_progressBarValue = value;
			}
		}
	}

	protected bool IsForceToProgressProgressEnd
	{
		get
		{
			lock (_progressBarValueLocker)
			{
				return _isForceToProgressEnd;
			}
		}
		set
		{
			lock (_progressBarValueLocker)
			{
				_isForceToProgressEnd = value;
			}
		}
	}

	public ProgressForm(Func<IProgress<ProgressInfo>, Task<T>> taskFactory, bool showCancel = false)
	{
		_taskFactory = taskFactory;
		Progress.ProgressChanged += Progress_ProgressChanged;
		_progressForm = new GaugeForm<T>(this);
		_progressForm.StartPosition = FormStartPosition.CenterScreen;
		_progressForm.btnCancel.Visible = showCancel;
		_progressForm.Shown += ProgressForm_Shown;
		_progressForm.FormClosing += _progressForm_FormClosing;
	}

	public ProgressForm(Func<ProgressInfo> reportProgress, Func<Task<T>> taskFactory, TimeSpan delay)
	{
		_reportProgress = reportProgress;
		_taskFactory1 = taskFactory;
		_delay = delay;
		_progressForm = new GaugeForm<T>(this);
		_progressForm.StartPosition = FormStartPosition.CenterScreen;
		_progressForm.btnCancel.Visible = false;
		_progressForm.Shown += _progressForm_Shown1;
		_progressForm.Load += _progressForm_Load;
		_progressForm.FormClosing += _progressForm_FormClosing;
	}

	public ProgressForm(Func<IProgress<ProgressInfo>, Task<T>> taskFactory, Func<ProgressInfo> reportProgress, int delayToShow = 0, bool showCancel = false)
	{
		_reportProgress = reportProgress;
		_taskFactory = taskFactory;
		_delay = TimeSpan.FromMilliseconds(delayToShow);
		Progress.ProgressChanged += Progress_ProgressChanged;
		_progressForm = new GaugeForm<T>(this);
		_progressForm.StartPosition = FormStartPosition.CenterScreen;
		_progressForm.btnCancel.Visible = showCancel;
		_progressForm.Shown += _progressForm_Shown1;
		_progressForm.Load += _progressForm_Load2;
		_progressForm.FormClosing += _progressForm_FormClosing;
	}

	public ProgressForm(Func<ProgressInfo> reportProgress, Func<Task<T>> taskFactory, int delayToShow)
	{
		_reportProgress = reportProgress;
		_taskFactory1 = taskFactory;
		_delay = TimeSpan.FromMilliseconds((delayToShow == 0) ? 10 : delayToShow);
		Progress.ProgressChanged += Progress_ProgressChanged;
		_progressForm = new GaugeForm<T>(this);
		_progressForm.StartPosition = FormStartPosition.CenterScreen;
		_progressForm.btnCancel.Visible = false;
		_progressForm.Shown += _progressForm_Shown1;
		_progressForm.Load += _progressForm_Load3;
		_progressForm.FormClosing += _progressForm_FormClosing;
	}

	private async void _progressForm_Load(object sender, EventArgs e)
	{
		_progressForm.Opacity = 0.0;
		try
		{
			Task = _taskFactory1();
			_ = System.Threading.Tasks.Task.Delay(_delay).ContinueWith(delegate
			{
				if (!Task.IsCompleted)
				{
					_progressForm.Invoke((InvokeDelegate)delegate
					{
						_progressForm.Opacity = 100.0;
					});
				}
			});
			await Task;
			await ForceProgressValueToEnd();
		}
		catch
		{
		}
		finally
		{
			Close();
		}
	}

	private async Task ForceProgressValueToEnd()
	{
		if (_progressForm.Opacity == 0.0 || LastDisplayProgressBarValue == _progressForm._progBar.Maximum)
		{
			return;
		}
		IsForceToProgressProgressEnd = true;
		for (int i = 0; i < 10; i++)
		{
			Application.DoEvents();
			if (LastDisplayProgressBarValue == _progressForm._progBar.Maximum)
			{
				await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(200.0)).ConfigureAwait(continueOnCapturedContext: false);
				break;
			}
			await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(100.0)).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private void _progressForm_Shown1(object sender, EventArgs e)
	{
		_timer = new System.Windows.Forms.Timer();
		_timer.Tick += Timer_Tick;
		_timer.Interval = 100;
		_timer.Start();
	}

	private void Timer_Tick(object sender, EventArgs e)
	{
		if (IsForceToProgressProgressEnd)
		{
			_progressForm._progBar.Value = _progressForm._progBar.Maximum;
			_progressForm.Invalidate();
			LastDisplayProgressBarValue = _progressForm._progBar.Value;
			return;
		}
		ProgressInfo progressInfo = _reportProgress();
		try
		{
			_progressForm._progBar.Value = Math.Min(_progressForm._progBar.Maximum, progressInfo.MainProgress);
			_progressForm.lblMain.Value = progressInfo.MainCaption;
			_progressForm.Invalidate();
			LastDisplayProgressBarValue = _progressForm._progBar.Value;
		}
		catch
		{
		}
	}

	private async void _progressForm_Load2(object sender, EventArgs e)
	{
		_ = 1;
		try
		{
			if (_delay.TotalMilliseconds == 0.0)
			{
				Task = _taskFactory(Progress);
			}
			else
			{
				_progressForm.Opacity = 0.0;
				Task = _taskFactory(Progress);
				_ = System.Threading.Tasks.Task.Delay(_delay).ContinueWith(delegate
				{
					if (!Task.IsCompleted)
					{
						_progressForm.Invoke((InvokeDelegate)delegate
						{
							_progressForm.Opacity = 100.0;
						});
					}
				});
			}
			Timer_Tick(_timer, EventArgs.Empty);
			Application.DoEvents();
			await Task;
			await ForceProgressValueToEnd();
		}
		catch
		{
		}
		finally
		{
			Close();
		}
	}

	private async void _progressForm_Load3(object sender, EventArgs e)
	{
		_ = 1;
		try
		{
			if (_delay.TotalMilliseconds == 0.0)
			{
				Task = _taskFactory1();
			}
			else
			{
				_progressForm.Opacity = 0.0;
				Task = _taskFactory1();
				_ = System.Threading.Tasks.Task.Delay(_delay).ContinueWith(delegate
				{
					if (!Task.IsCompleted)
					{
						_progressForm.Invoke((InvokeDelegate)delegate
						{
							_progressForm.Opacity = 100.0;
						});
					}
				});
			}
			Timer_Tick(_timer, EventArgs.Empty);
			Application.DoEvents();
			await Task;
			await ForceProgressValueToEnd();
		}
		catch
		{
		}
		finally
		{
			Close();
		}
	}

	private void _progressForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		if (_timer != null)
		{
			_timer.Stop();
			_timer.Tick -= Timer_Tick;
		}
		if (e.CloseReason != CloseReason.ApplicationExitCall)
		{
			if (e.CloseReason == CloseReason.UserClosing && !_closingByCode)
			{
				e.Cancel = true;
			}
			_closingByCode = false;
		}
	}

	private async void ProgressForm_Shown(object sender, EventArgs e)
	{
		Application.DoEvents();
		try
		{
			Task = _taskFactory(Progress);
			Application.DoEvents();
			await Task.ConfigureAwait(continueOnCapturedContext: false);
			await ForceProgressValueToEnd();
		}
		catch
		{
		}
		finally
		{
			Application.DoEvents();
			Close();
		}
	}

	public void Close()
	{
		if (_closed)
		{
			return;
		}
		try
		{
			_progressForm.Invoke((Action)delegate
			{
				Progress.ProgressChanged -= Progress_ProgressChanged;
				_closingByCode = true;
				_progressForm.Close();
				_closed = true;
			});
		}
		catch (InvalidOperationException)
		{
			Thread thread = new Thread((ThreadStart)delegate
			{
				for (int i = 0; i < 5; i++)
				{
					try
					{
						_progressForm.Invoke((Action)delegate
						{
							Progress.ProgressChanged -= Progress_ProgressChanged;
							_closingByCode = true;
							_progressForm.Close();
							_closed = true;
						});
						break;
					}
					catch (InvalidOperationException)
					{
						Thread.Sleep(200);
					}
				}
			});
			thread.IsBackground = true;
			thread.Start();
		}
	}

	public void ShowDialog()
	{
		try
		{
			Color lineColor = Theme.SelectedAuditaiTheme.ThemeContext.LineColor;
			_progressForm.ProgressColor = lineColor;
		}
		catch
		{
		}
		_progressForm.ShowDialog();
	}

	private void Progress_ProgressChanged(object sender, ProgressInfo e)
	{
		if (_progressForm.IsDisposed)
		{
			return;
		}
		try
		{
			_progressForm.Invoke((Action)delegate
			{
				_progressForm._progBar.Value = Math.Min(_progressForm._progBar.Maximum, e.MainProgress);
				_progressForm.lblMain.Value = e.MainCaption;
				_progressForm.Invalidate();
			});
			_progressForm.UpdatePaint();
		}
		catch (InvalidAsynchronousStateException)
		{
		}
	}
}
