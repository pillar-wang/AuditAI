using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using C1.Win.C1Command;
using C1.Win.C1SplitContainer;
using Auditai.Model;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

public class ImageEditor
{
	private Form _owner;

	private C1ContextMenu _ctx = new C1ContextMenu();

	private C1Command cmdRotate270 = new C1Command();

	private C1CommandLink lnkRotate270 = new C1CommandLink();

	private C1Command cmdRotate90 = new C1Command();

	private C1CommandLink lnkRotate90 = new C1CommandLink();

	private C1Command cmdFlipHori = new C1Command();

	private C1CommandLink lnkFlipHori = new C1CommandLink();

	private C1Command cmdFlipVert = new C1Command();

	private C1CommandLink lnkFlipVert = new C1CommandLink();

	private C1Command cmdRotate90T = new C1Command
	{
		Text = "顺转90度",
		Image = Resources.Rotate90
	};

	private C1CommandLink lnkRotate90T = new C1CommandLink();

	private C1Command cmdRotate270T = new C1Command
	{
		Text = "逆转90度",
		Image = Resources.Rotate270
	};

	private C1CommandLink lnkRotate270T = new C1CommandLink();

	private C1Command cmdFlipHoriT = new C1Command
	{
		Text = "水平翻转",
		Image = Resources.FlipHori32
	};

	private C1CommandLink lnkFlipHoriT = new C1CommandLink();

	private C1Command cmdFlipVertT = new C1Command
	{
		Text = "垂直翻转",
		Image = Resources.FlipVert32
	};

	private C1CommandLink lnkFlipVertT = new C1CommandLink();

	private readonly C1Command cmdBack = new C1Command
	{
		Image = Resources.back32,
		Text = "后退"
	};

	private readonly C1CommandLink lnkBack = new C1CommandLink();

	private readonly C1Command cmdForward = new C1Command
	{
		Image = Resources.forward32,
		Text = "前进"
	};

	private readonly C1CommandLink lnkForward = new C1CommandLink();

	private readonly C1Command cmdHideToolbar = new C1Command
	{
		Text = "隐藏侧边栏",
		Image = Resources.HideSideToolbar
	};

	private readonly C1CommandLink lnkZoomIn = new C1CommandLink();

	private readonly C1Command cmdZoomIn = new C1Command
	{
		Text = "放大显示",
		Image = Resources.ZoomIn
	};

	private readonly C1CommandLink lnkZoomOut = new C1CommandLink();

	private readonly C1Command cmdZoomOut = new C1Command
	{
		Text = "缩小显示",
		Image = Resources.ZoomOut
	};

	private readonly C1CommandLink lnkExportImage = new C1CommandLink();

	private readonly C1Command cmdExportImage = new C1Command
	{
		Text = "导出图片",
		Image = Resources.ExportImage
	};

	private readonly C1CommandLink lnkHideToolbar = new C1CommandLink();

	private readonly C1Command _cmdHelpCenter;

	private readonly C1CommandLink _lnkHelpCenter;

	private Point _lastMousePos;

	private Cursor _handHover = new Cursor(new MemoryStream(Resources.HandHover));

	private Cursor _handGrab = new Cursor(new MemoryStream(Resources.HandGrab));

	private PictureBoxEx _pb = new PictureBoxEx
	{
		Dock = DockStyle.Fill,
		Cursor = Cursors.Hand
	};

	private C1SplitterPanel _pnlToolbar = new C1SplitterPanel
	{
		Collapsible = false,
		Dock = PanelDockStyle.Right,
		Width = 80,
		KeepRelativeSize = false,
		Resizable = false
	};

	private C1ToolBar _toolBar = new C1ToolBar
	{
		Horizontal = false,
		Dock = DockStyle.Fill,
		ButtonLookVert = ButtonLookFlags.TextAndImage,
		MinButtonSize = 40,
		ShowToolTips = false
	};

	private bool _isNeedInitImageZoomOnSizeChanged;

	public C1SplitContainer View { get; } = new C1SplitContainer
	{
		Dock = DockStyle.Fill
	};


	public Auditai.Model.Image Image { get; set; }

	public ImageEditor(Form owner)
	{
		_owner = owner;
		cmdRotate90.CommandStateQuery += CmdRotate90_CommandStateQuery;
		cmdRotate90.Click += CmdRotate90_Click;
		lnkRotate90.Command = cmdRotate90;
		_ctx.CommandLinks.Add(lnkRotate90);
		cmdRotate270.CommandStateQuery += CmdRotate270_CommandStateQuery;
		cmdRotate270.Click += CmdRotate270_Click;
		lnkRotate270.Command = cmdRotate270;
		_ctx.CommandLinks.Add(lnkRotate270);
		cmdFlipHori.CommandStateQuery += CmdFlipHori_CommandStateQuery;
		cmdFlipHori.Click += CmdFlipHori_Click;
		lnkFlipHori.Command = cmdFlipHori;
		_ctx.CommandLinks.Add(lnkFlipHori);
		cmdFlipVert.CommandStateQuery += CmdFlipVert_CommandStateQuery;
		cmdFlipVert.Click += CmdFlipVert_Click;
		lnkFlipVert.Command = cmdFlipVert;
		_ctx.CommandLinks.Add(lnkFlipVert);
		_pb.MouseClick += View_MouseClick;
		_pb.MouseWheel += View_MouseWheel;
		_pb.MouseDown += View_MouseDown;
		_pb.MouseUp += View_MouseUp;
		View.Panels.Add(_pnlToolbar);
		_pnlToolbar.Controls.Add(_toolBar);
		View.Panels.Add(new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left
		});
		View.Panels[1].Controls.Add(_pb);
		cmdRotate90T.Click += CmdRotate90T_Click;
		lnkRotate90T.Command = cmdRotate90T;
		cmdRotate270T.Click += CmdRotate270T_Click;
		lnkRotate270T.Command = cmdRotate270T;
		cmdFlipHoriT.Click += CmdFlipHoriT_Click;
		lnkFlipHoriT.Command = cmdFlipHoriT;
		cmdFlipVertT.Click += CmdFlipVertT_Click;
		lnkFlipVertT.Command = cmdFlipVertT;
		cmdBack.Click += CmdBack_Click;
		lnkBack.Command = cmdBack;
		lnkBack.Delimiter = true;
		cmdForward.Click += CmdForward_Click;
		lnkForward.Command = cmdForward;
		lnkHideToolbar.Delimiter = true;
		cmdHideToolbar.Click += CmdHideToolbar_Click;
		lnkHideToolbar.Command = cmdHideToolbar;
		cmdZoomOut.Click += CmdZoomOut_Click;
		lnkZoomOut.Command = cmdZoomOut;
		cmdZoomIn.Click += CmdZoomIn_Click;
		lnkZoomIn.Command = cmdZoomIn;
		cmdExportImage.Click += CmdExportImage_Click;
		lnkExportImage.Command = cmdExportImage;
		_cmdHelpCenter = new C1Command
		{
			Text = "帮助中心",
			Image = Resources.HelpCenter,
			Visible = SoftwareLicenseManager.IsShowHelpDocumentButton()
		};
		_cmdHelpCenter.Click += _cmdHelpCenter_Click;
		_lnkHelpCenter = new C1CommandLink(_cmdHelpCenter)
		{
			Delimiter = true
		};
		_toolBar.CommandLinks.AddRange(new C1CommandLink[8] { lnkRotate90T, lnkRotate270T, lnkFlipHoriT, lnkFlipVertT, lnkZoomIn, lnkZoomOut, lnkExportImage, _lnkHelpCenter });
		RibbonImageProcess imageProcess = MainForm.ImageProcess;
		foreach (C1CommandLink commandLink in _toolBar.CommandLinks)
		{
			imageProcess.Register(new C1CommandAdapter(commandLink.Command));
		}
		_pb.ClientSizeChanged += _pb_ClientSizeChanged;
	}

	private void InitImageZoomFactor(System.Drawing.Image image)
	{
		if (image == null)
		{
			return;
		}
		Image._isFirstOpened = false;
		Image.Center = new PointF(0.5f, 0.5f);
		Image.RotateFlip = RotateFlipType.RotateNoneFlipNone;
		float num = 1f;
		float num2 = 100f;
		if ((float)image.Width + num2 <= (float)_pb.Width && (float)image.Height + num2 <= (float)_pb.Height)
		{
			num = 1f;
		}
		else
		{
			float val = (((float)_pb.Width > num2) ? ((float)_pb.Width - num2) : ((float)_pb.Width)) / (float)image.Width;
			float val2 = (((float)_pb.Height > num2) ? ((float)_pb.Height - num2) : ((float)_pb.Height)) / (float)image.Height;
			for (num = Math.Min(val, val2); num < 1f && ((float)image.Width * num < 1f || (float)image.Height * num < 1f); num += 0.01f)
			{
			}
		}
		Image.ZoomFactor = num;
	}

	private void _pb_ClientSizeChanged(object sender, EventArgs e)
	{
		if (_pb.Width != 0 && _pb.Height != 0)
		{
			if (_isNeedInitImageZoomOnSizeChanged)
			{
				InitImageZoomFactor(_pb.Image);
				_pb.Center = Image.Center;
				_pb.ZoomFactor = Image.ZoomFactor;
				_isNeedInitImageZoomOnSizeChanged = false;
			}
			RefreshImageCenter();
		}
	}

	public void Populate()
	{
		if (_pb.Image != null)
		{
			_pb.Image.Dispose();
			_pb.Image = null;
		}
		try
		{
			System.Drawing.Image graphicsImage = Image.GetGraphicsImage();
			if (Image._isFirstOpened)
			{
				Image._isFirstOpened = false;
				_isNeedInitImageZoomOnSizeChanged = true;
				InitImageZoomFactor(graphicsImage);
			}
			else
			{
				_isNeedInitImageZoomOnSizeChanged = false;
			}
			graphicsImage.RotateFlip(Image.RotateFlip);
			_pb.Center = Image.Center;
			_pb.ZoomFactor = Image.ZoomFactor;
			_pb.Cursor = _handHover;
			_pb.Image = graphicsImage;
		}
		catch (ExternalException ex)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "显示图片过程发生异常，请稍候重试。异常信息：\n" + ex.Message);
		}
		catch (OutOfMemoryException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "图片文件过大，暂时无法显示，请稍候重试。");
		}
		catch (FileNotFoundException)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "要打开的图片文件不存在。");
		}
		catch (UnauthorizedAccessException ex4)
		{
			Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, "没有权限打开文件:" + ex4.Message);
			ex4.Log();
		}
	}

	public void ReloadFromDb()
	{
		Image.ReloadAndReturn();
		Populate();
	}

	public void SetZoomFactor(int percent)
	{
		float num = (float)percent / 100f;
		Image.UpdateZoomFactor(num);
		_pb.ZoomFactor = num;
	}

	public void HideToolbar()
	{
		_pnlToolbar.Hide();
	}

	public void ShowToolbar()
	{
		_pnlToolbar.Show();
	}

	private void View_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right && ShouldShowContextMenu())
		{
			_ctx.ShowContextMenu(_pb, e.Location);
		}
	}

	private void View_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			_pb.Cursor = _handHover;
			_pb.MouseMove -= View_MouseMove;
			_owner.Deactivate -= _owner_Deactivate;
		}
	}

	private void View_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			_pb.Cursor = _handGrab;
			_pb.MouseMove += View_MouseMove;
			_owner.Deactivate += _owner_Deactivate;
			_lastMousePos = e.Location;
		}
	}

	private void _owner_Deactivate(object sender, EventArgs e)
	{
		_owner.Deactivate -= _owner_Deactivate;
		_pb.MouseMove -= View_MouseMove;
	}

	private PointF ClampPictureBoxCenter(PointF ps)
	{
		if (_pb.Image == null)
		{
			return ps;
		}
		PointF result = new PointF(ps.X, ps.Y);
		float num = 50f;
		float num2 = (float)_pb.Image.Width * _pb.ZoomFactor;
		float num3 = (float)_pb.Width * ps.X - num2 / 2f;
		if (num3 + num2 < num)
		{
			result.X = (0f - num2 + num + num2 / 2f) / (float)_pb.Width;
		}
		else if (num3 > (float)_pb.Width - num)
		{
			result.X = ((float)_pb.Width - num + num2 / 2f) / (float)_pb.Width;
		}
		float num4 = 50f;
		float num5 = (float)_pb.Image.Height * _pb.ZoomFactor;
		float num6 = (float)_pb.Height * ps.Y - num5 / 2f;
		if (num6 + num5 < num4)
		{
			result.Y = (0f - num5 + num4 + num5 / 2f) / (float)_pb.Height;
		}
		else if (num6 > (float)_pb.Height - num4)
		{
			result.Y = ((float)_pb.Height - num4 + num5 / 2f) / (float)_pb.Height;
		}
		return result;
	}

	private void VScrollPictureBoxByScrollDistance(float distance)
	{
		if (_pb.Image != null)
		{
			float num = (float)_pb.Image.Height * _pb.ZoomFactor;
			float num2 = (float)_pb.Height * _pb.Center.Y - num / 2f;
			float num3 = num2 + distance;
			float y = (num3 + num / 2f) / (float)_pb.Height;
			PointF pointF = ClampPictureBoxCenter(new PointF(_pb.Center.X, y));
			if (!(pointF == _pb.Center))
			{
				Image.UpdateCenter(pointF);
				_pb.Center = pointF;
			}
		}
	}

	private void RefreshImageCenter()
	{
		if (_pb.Image != null)
		{
			PointF pointF = ClampPictureBoxCenter(_pb.Center);
			if (!(pointF == _pb.Center))
			{
				Image.UpdateCenter(pointF);
				_pb.Center = pointF;
			}
		}
	}

	private void View_MouseMove(object sender, MouseEventArgs e)
	{
		if (_pb.Image != null)
		{
			float num = (float)(e.Location.X - _lastMousePos.X) / (float)_pb.Width;
			float num2 = (float)(e.Location.Y - _lastMousePos.Y) / (float)_pb.Height;
			PointF ps = new PointF(_pb.Center.X + num, _pb.Center.Y + num2);
			ps = ClampPictureBoxCenter(ps);
			if (_pb.Center == ps)
			{
				_lastMousePos = e.Location;
				return;
			}
			Image.UpdateCenter(ps);
			_pb.Center = ps;
			_lastMousePos = e.Location;
		}
	}

	private void View_MouseWheel(object sender, MouseEventArgs e)
	{
		if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
		{
			if (e.Delta > 0)
			{
				_pb.ZoomFactor += 0.05f;
				Image.UpdateZoomFactor(_pb.ZoomFactor);
			}
			else
			{
				_pb.ZoomFactor -= 0.05f;
				Image.UpdateZoomFactor(_pb.ZoomFactor);
			}
			RefreshImageCenter();
		}
		else
		{
			VScrollPictureBoxByScrollDistance(0.5f * (float)e.Delta);
		}
	}

	private void CmdFlipVert_Click(object sender, ClickEventArgs e)
	{
		FlipVert();
	}

	private void CmdFlipVert_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdFlipVert.Text = "垂直翻转";
		cmdFlipVert.Image = ContextResources.FlipVert;
	}

	private void CmdFlipHori_Click(object sender, ClickEventArgs e)
	{
		FlipHori();
	}

	private void CmdFlipHori_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdFlipHori.Text = "水平翻转";
		cmdFlipHori.Image = ContextResources.FlipHori;
	}

	private void CmdRotate90_Click(object sender, ClickEventArgs e)
	{
		Rotate90();
	}

	private void CmdRotate90_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdRotate90.Text = "顺转90度";
		cmdRotate90.Image = ContextResources.ctxReloadFile;
	}

	private void CmdRotate270_Click(object sender, ClickEventArgs e)
	{
		Rotate270();
	}

	private void CmdRotate270_CommandStateQuery(object sender, CommandStateQueryEventArgs e)
	{
		cmdRotate270.Text = "逆转90度";
		cmdRotate270.Image = ContextResources.ctxRotate270;
	}

	private void CmdFlipVertT_Click(object sender, ClickEventArgs e)
	{
		FlipVert();
	}

	private void CmdFlipHoriT_Click(object sender, ClickEventArgs e)
	{
		FlipHori();
	}

	private void CmdRotate270T_Click(object sender, ClickEventArgs e)
	{
		Rotate270();
	}

	private void CmdRotate90T_Click(object sender, ClickEventArgs e)
	{
		Rotate90();
	}

	private void CmdHideToolbar_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.ToggleSideToolbar();
	}

	private void CmdForward_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.Forward();
	}

	private void CmdBack_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.Back();
	}

	private void _cmdHelpCenter_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.ShowHelpCenter();
	}

	private void CmdZoomOut_Click(object sender, ClickEventArgs e)
	{
		_pb.ZoomFactor -= 0.05f;
		Image.UpdateZoomFactor(_pb.ZoomFactor);
		RefreshImageCenter();
	}

	private void CmdZoomIn_Click(object sender, ClickEventArgs e)
	{
		_pb.ZoomFactor += 0.05f;
		Image.UpdateZoomFactor(_pb.ZoomFactor);
	}

	private void CmdExportImage_Click(object sender, ClickEventArgs e)
	{
		Program.MainForm.ExportImageDialog();
	}

	private bool ShouldShowContextMenu()
	{
		return !ImageAnimator.CanAnimate(_pb.Image);
	}

	private void Rotate270()
	{
		if (_pb.Image != null)
		{
			_pb.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
			Image.DoRotateFlip(RotateFlipType.Rotate270FlipNone);
			_pb.Refresh();
		}
	}

	private void Rotate90()
	{
		if (_pb.Image != null)
		{
			_pb.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
			Image.DoRotateFlip(RotateFlipType.Rotate90FlipNone);
			_pb.Refresh();
		}
	}

	private void FlipHori()
	{
		if (_pb.Image != null)
		{
			_pb.Image.RotateFlip(RotateFlipType.RotateNoneFlipX);
			Image.DoRotateFlip(RotateFlipType.RotateNoneFlipX);
			_pb.Refresh();
		}
	}

	private void FlipVert()
	{
		if (_pb.Image != null)
		{
			_pb.Image.RotateFlip(RotateFlipType.Rotate180FlipX);
			Image.DoRotateFlip(RotateFlipType.Rotate180FlipX);
			_pb.Refresh();
		}
	}
}
