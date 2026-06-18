using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms;

public class RichTextBoxEx : RichTextBox
{
	private enum ScrollBarType
	{
		SbHorz,
		SbVert,
		SbCtl,
		SbBoth
	}

	private enum ScrollBarCommands
	{
		ThumbPosition = 4
	}

	private enum WindowMessage
	{
		VScroll = 277
	}

	private const int WM_PAINT = 15;

	public int VScrollPos
	{
		get
		{
			return GetScrollPos(base.Handle, 1);
		}
		set
		{
			SendMessage(base.Handle, 277u, new IntPtr((value << 16) | 4), IntPtr.Zero);
		}
	}

	public event PaintEventHandler Paint1;

	[DllImport("user32")]
	private static extern int GetScrollPos(IntPtr hWnd, int nBar);

	[DllImport("user32")]
	private static extern int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	public void SetHeight(int height)
	{
		base.Height = height;
		Refresh();
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		if (!base.AutoWordSelection)
		{
			base.AutoWordSelection = true;
			base.AutoWordSelection = false;
		}
	}

	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);
		if (m.Msg == 15)
		{
			using (Graphics graphics = CreateGraphics())
			{
				this.Paint1?.Invoke(this, new PaintEventArgs(graphics, base.ClientRectangle));
			}
		}
	}
}
