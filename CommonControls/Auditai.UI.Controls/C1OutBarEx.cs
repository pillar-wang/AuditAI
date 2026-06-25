using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Command;

namespace Auditai.UI.Controls;

public class C1OutBarEx : C1OutBar
{
	public class PageSelectEvent
	{
		public MouseEventArgs MouseEvent;

		public C1OutPage Page;

		public bool Cancel;
	}

	public Action<C1OutPage, PaintEventArgs, Rectangle> PageTitlePostPaintHandle;

	public Action<PageSelectEvent> PageBeforeSelectEventHandle;

	private C1OutPage _cancelSelectPage;

	public C1OutPage HotPage { get; private set; }

	public event PageClickEventHandler PageDoubleClicked;

	protected override void OnMouseClick(MouseEventArgs e)
	{
		HotPage = null;
		foreach (C1OutPage page in base.Pages)
		{
			if (page.IsHot)
			{
				HotPage = page;
			}
		}
		base.OnMouseClick(e);
	}

	public bool IsFirstPage(C1OutPage page)
	{
		return base.Pages.IndexOf(page) == 0;
	}

	protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
		foreach (C1OutPage page in base.Pages)
		{
			if (new Rectangle(page.Left, page.Top - base.PageTitleHeight, page.Width, base.PageTitleHeight).Contains(e.Location))
			{
				this.PageDoubleClicked?.Invoke(this, new PageClickEventArgs(page));
			}
		}
		base.OnMouseDoubleClick(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (PageTitlePostPaintHandle == null)
		{
			return;
		}
		int num = 0;
		foreach (C1OutPage page in base.Pages)
		{
			if (page != null && page.PageVisible && num <= page.CaptionBounds.Top)
			{
				if (e.ClipRectangle.IntersectsWith(page.CaptionBounds))
				{
					PageTitlePostPaintHandle(page, e, page.CaptionBounds);
				}
				num = page.CaptionBounds.Bottom;
			}
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		_cancelSelectPage = null;
		if (PageBeforeSelectEventHandle != null && e.Button == MouseButtons.Left)
		{
			C1OutPage hitPage = GetHitPage(e.X, e.Y);
			if (hitPage != null)
			{
				PageSelectEvent pageSelectEvent = new PageSelectEvent
				{
					Page = hitPage,
					MouseEvent = e,
					Cancel = false
				};
				C1OutPage hotPage = HotPage;
				HotPage = hitPage;
				PageBeforeSelectEventHandle(pageSelectEvent);
				if (pageSelectEvent.Cancel)
				{
					_cancelSelectPage = hitPage;
					return;
				}
				HotPage = hotPage;
			}
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (_cancelSelectPage != null)
		{
			_cancelSelectPage = null;
		}
		else
		{
			base.OnMouseUp(e);
		}
	}

	public C1OutPage GetHitPage(int mouseX, int mouseY)
	{
		foreach (C1OutPage page in base.Pages)
		{
			Rectangle captionBounds = page.CaptionBounds;
			if (page.PageVisible && captionBounds.Contains(mouseX, mouseY))
			{
				return page;
			}
		}
		return null;
	}
}
