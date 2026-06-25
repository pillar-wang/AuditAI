using System;
using System.Drawing;
using System.Windows.Forms;

namespace Auditai.UI.CommonControls;

public abstract class AbstractFlickerProxy
{
	protected Image orignImage;

	protected Image emptyImage;

	protected Image twinkleImage;

	protected string orignContent;

	protected string twinkleContent;

	protected Timer _trigger;

	protected bool starting;

	private int times = -10;

	public object UserData { get; set; }

	public AbstractFlickerProxy()
	{
		_trigger = new Timer
		{
			Interval = 500
		};
		_trigger.Tick += Timer_Elapsed;
	}

	public abstract bool IsDisposed();

	public virtual void Start()
	{
		if (emptyImage == null && twinkleImage != null)
		{
			int width = twinkleImage.Width;
			int height = twinkleImage.Height;
			emptyImage = new Bitmap(width, height);
		}
		starting = true;
	}

	public virtual void Stop()
	{
		starting = false;
		if (!IsDisposed())
		{
			SetView(orignImage, orignContent);
		}
	}

	public bool Status()
	{
		return starting;
	}

	public void SetTimer(Timer timer)
	{
		_trigger = timer;
		_trigger.Tick += Timer_Elapsed;
	}

	public void UpdateTwinkleImage(Image image)
	{
		twinkleImage = image;
	}

	public void UpdateOrignImage(Image image)
	{
		orignImage = image;
	}

	public void UpdateEmptyImage(Image image)
	{
		emptyImage = image;
	}

	public void UpdateTwinkleContent(string text)
	{
		twinkleContent = text;
	}

	public void SetFlickTime(int times)
	{
		this.times = times;
	}

	public void Dispose()
	{
		_trigger.Tick -= Timer_Elapsed;
	}

	private void Timer_Elapsed(object sender, EventArgs e)
	{
		if (starting)
		{
			if (IsDisposed())
			{
				starting = false;
			}
			else if (times != -10 && times-- < 0)
			{
				Stop();
			}
			else if (SecondTrigger.Display)
			{
				SetView(twinkleImage, twinkleContent);
			}
			else
			{
				SetView(emptyImage, GetEmptyContent());
			}
		}
	}

	protected abstract void SetView(Image image, string content);

	protected abstract string GetContent();

	protected abstract Image GetImage();

	protected virtual string GetEmptyContent()
	{
		return string.Empty;
	}
}
