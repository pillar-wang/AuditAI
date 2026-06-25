using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1FlexGrid;

namespace Auditai.UI.CommonControls;

public class RowFlickerProxy : AbstractFlickerProxy
{
	private Row _row;

	public readonly bool contentFlick;

	public RowFlickerProxy(Row row, bool contentFlick)
	{
		_row = row;
		orignImage = GetImage();
		twinkleImage = GetImage();
		orignContent = GetContent();
		twinkleContent = GetContent();
		this.contentFlick = contentFlick;
	}

	public RowFlickerProxy(Row row, Timer timer, Image trans, bool contentFlick)
		: this(row, contentFlick)
	{
		SetTimer(timer);
		UpdateEmptyImage(trans);
	}

	protected override void SetView(Image image, string content)
	{
		try
		{
			_row.Grid.SetCellImage(_row.Index, 0, image);
			if (contentFlick)
			{
				_row[0] = content;
			}
		}
		catch (NullReferenceException)
		{
			starting = false;
		}
	}

	protected override string GetContent()
	{
		return _row[0]?.ToString() ?? string.Empty;
	}

	protected override Image GetImage()
	{
		try
		{
			return _row.Grid.GetCellImage(_row.Index, 0);
		}
		catch (NullReferenceException)
		{
			starting = false;
			return null;
		}
	}

	public override void Start()
	{
		orignContent = GetContent();
		twinkleContent = GetContent();
		base.Start();
	}

	public override void Stop()
	{
		if (starting)
		{
			base.Stop();
		}
	}

	public override bool IsDisposed()
	{
		try
		{
			return _row == null || _row.Grid == null || _row.Grid.IsDisposed;
		}
		catch
		{
			return true;
		}
	}
}
