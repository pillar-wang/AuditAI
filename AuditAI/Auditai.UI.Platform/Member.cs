using System;
using System.Drawing;
using Auditai.DTO;
using Auditai.SignalR;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class Member : MemTab
{
	private System.Drawing.Image _image16;

	private Guid? _teamId;

	public bool Sex { get; set; } = true;


	public Guid? TeamId
	{
		get
		{
			return _teamId;
		}
		set
		{
			_teamId = value;
			if (UserState != null)
			{
				UserState.TeamId = _teamId?.ToString();
			}
		}
	}

	public UserRole? Role { get; set; } = UserRole.User;


	public UserState UserState { get; set; }

	public bool IsOnline => UserState != null;

	public override bool SetPicture(byte[] bytes)
	{
		if (!base.SetPicture(bytes) && base.Image == null)
		{
			Bitmap bitmap = (Sex ? Resources.Boy : Resources.Girl);
			base.Image = bitmap.ToSize(32, 32);
			bitmap.Dispose();
			base.GrayImage = ((Bitmap)base.Image).ToGray();
			ClearImage16InMainThread();
		}
		return true;
	}

	public override bool SetPicture(Bitmap bitmap)
	{
		if (!base.SetPicture(bitmap) && base.Image == null)
		{
			Bitmap bitmap2 = (Sex ? Resources.Boy : Resources.Girl);
			base.Image = bitmap2.ToSize(32, 32);
			bitmap2.Dispose();
			base.GrayImage = ((Bitmap)base.Image).ToGray();
			ClearImage16InMainThread();
		}
		return true;
	}

	public User ToDto()
	{
		return new User
		{
			Id = long.Parse(base.Id),
			Sex = (Sex ? "m" : "f"),
			Name = base.Name,
			TeamId = (TeamId.HasValue ? TeamId.Value : default(Guid)),
			Role = (Role.HasValue ? Role.Value : UserRole.User)
		};
	}

	public System.Drawing.Image GetOrGenerateImage16()
	{
		if (_image16 != null)
		{
			return _image16;
		}
		_image16 = Auditai.UI.Controls.Util.StandardImage(base.Image, 16);
		return _image16;
	}

	private void ClearImage16InMainThread()
	{
		if (Program.MainForm == null || Program.MainForm.View == null || !Program.MainForm.View.Created)
		{
			return;
		}
		Program.MainForm.View.Invoke((Action)delegate
		{
			if (_image16 != null)
			{
				_image16.Dispose();
				_image16 = null;
			}
		});
	}
}
