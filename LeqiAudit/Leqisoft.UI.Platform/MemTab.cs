using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Leqisoft.UI.Platform;

public class MemTab
{
	public string Id { get; set; }

	public string Name { get; set; }

	public Image Image { get; protected set; }

	public Image GrayImage { get; protected set; }

	public DateTime TempMostEarly { get; set; } = DateTime.Now;


	public Queue<IActionMessage> UnhandleActionMessage { get; private set; }

	public Queue<TempRecord> UnhandleNotifyMessage { get; private set; }

	public MemTab()
	{
		UnhandleActionMessage = new Queue<IActionMessage>();
		UnhandleNotifyMessage = new Queue<TempRecord>();
	}

	public virtual bool SetPicture(byte[] bytes)
	{
		if (bytes == null)
		{
			return false;
		}
		try
		{
			using MemoryStream stream = new MemoryStream(bytes);
			Image image = Image.FromStream(stream);
			Image = image.ToSize(32, 32);
			image.Dispose();
			GrayImage = ((Bitmap)Image).ToGray();
		}
		catch
		{
			return false;
		}
		return true;
	}

	public virtual bool SetPicture(Bitmap src)
	{
		if (src == null)
		{
			return false;
		}
		try
		{
			Image = src.ToSize(32, 32);
			GrayImage = ((Bitmap)Image).ToGray();
		}
		catch
		{
			return false;
		}
		return true;
	}
}
