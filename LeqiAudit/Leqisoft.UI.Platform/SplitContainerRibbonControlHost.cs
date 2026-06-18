using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using C1.Win.C1SplitContainer;
using C1.Win.C1SuperTooltip;
using Leqisoft.Model;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Controls.Properties;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class SplitContainerRibbonControlHost : RibbonControlHost, ISetTheme
{
	private MainForm _owner;

	private C1SplitContainer _ctn;

	private C1SuperTooltip _ttp = new C1SuperTooltip();

	private TooltipManager tooltipManager = new TooltipManager();

	private TooltipBox _ttpComment = new TooltipBox
	{
		Opacity = 0.8,
		IsBalloon = true
	};

	public C1SuperLabel UserLabel { get; private set; }

	public C1SuperLabel SelectionStatsLabel { get; private set; }

	public C1Button PreviousErrorButton { get; private set; }

	public C1Button NextErrorButton { get; private set; }

	public C1CheckBox TableFoot { get; private set; }

	public C1CheckBox TableNote { get; private set; }

	public C1CheckBox ValidationFormula { get; private set; }

	public override bool Enabled
	{
		get
		{
			return _ctn.Enabled;
		}
		set
		{
			_ctn.Enabled = value;
		}
	}

	public SplitContainerRibbonControlHost(MainForm owner)
		: this(new C1SplitContainer())
	{
		_owner = owner;
		SetTheme();
		ThemeManager.GetInstance().Register(this);
	}

	public void Populate()
	{
		XElement xElement = null;
		xElement = (UserTeam.CurrentTeamIsPayByProject ? new XElement("p", new XText("当前用户账号：" + User.Current.UserName), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XText("当前用户姓名：" + User.Current.Name), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XText("当前" + StringConstBase.Current.Project + "名称：" + Project.Current.Name), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp")) : new XElement("p", new XText("当前用户账号：" + User.Current.UserName), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XText("当前用户姓名：" + User.Current.Name), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp"), new XText("当前组织名称：" + UserTeam.Current.Name), new XEntity("nbsp"), new XEntity("nbsp"), new XEntity("nbsp")));
		if (!Program.IsOnPremise)
		{
			if (SoftwareLicenseManager.IsNeedShowExpiredDate())
			{
				xElement.Add(new XElement("span", new XAttribute("style", "color:red;margin-top:3px"), new XText($"授权使用到期日：{User.Current.LicenseDate:yyyy年MM月dd日}")));
			}
			else
			{
				xElement.Add(new XElement("span", new XAttribute("style", "color:red;margin-top:3px"), new XText("  ")));
			}
		}
		UserLabel.Text = xElement.ToString();
	}

	public void AttachTooltip()
	{
		PreviousErrorButton.MouseEnter += delegate
		{
			show(PreviousErrorButton, TipInfo.Parse(TipResource.主窗体状态栏_上一个错误));
		};
		PreviousErrorButton.MouseLeave += delegate
		{
			hide();
		};
		NextErrorButton.MouseEnter += delegate
		{
			show(NextErrorButton, TipInfo.Parse(TipResource.主窗体状态栏_下一个错误));
		};
		NextErrorButton.MouseLeave += delegate
		{
			hide();
		};
		void hide()
		{
			tooltipManager.Hide();
			_ttp.Active = true;
		}
		void show(C1Button button, TipInfo tip)
		{
			if (!tooltipManager.ShouldDisplay)
			{
				_ttp.Active = true;
			}
			else
			{
				_ttp.Active = false;
				Rectangle bounds = button.Bounds;
				int num = bounds.Left + (bounds.Right - bounds.Left) / 2;
				int top = bounds.Top;
				tooltipManager.Show(tip, button.Parent, num, top);
			}
		}
	}

	public void SetTheme()
	{
		ValidationFormula.FlatAppearance.CheckedBackColor = Theme.SelectedLeqiTheme.ThemeContext.TileColor;
		TableNote.FlatAppearance.CheckedBackColor = Theme.SelectedLeqiTheme.ThemeContext.TileColor;
		TableFoot.FlatAppearance.CheckedBackColor = Theme.SelectedLeqiTheme.ThemeContext.TileColor;
	}

	public void ShowComment()
	{
		if (_owner.CurrentView != MainFormView.TicketInput)
		{
			_ttpComment.Duration = 5000;
			XElement xElement = new XElement("p", new XAttribute("style", "color:red"), "点按此处定位下一处错误。");
			_ttpComment.SetText("消息提示", xElement.ToString());
			_ttpComment.Show(NextErrorButton, new Point(NextErrorButton.Width / 2, 0));
		}
	}

	private void NextErrorButton_Click(object sender, EventArgs e)
	{
		switch (_owner.CurrentView)
		{
		case MainFormView.Table:
			_owner.NextError();
			break;
		case MainFormView.Document:
			_owner.CurrentDocumentEditor.NextError();
			break;
		}
	}

	private void PreviousErrorButton_Click(object sender, EventArgs e)
	{
		switch (_owner.CurrentView)
		{
		case MainFormView.Table:
			_owner.PreviousError();
			break;
		case MainFormView.Document:
			_owner.CurrentDocumentEditor.PreviousError();
			break;
		}
	}

	private void ValidationFormula_Click(object sender, EventArgs e)
	{
		if (ValidationFormula.Checked)
		{
			_owner.TableEditor.HideValidationPane();
		}
		else
		{
			_owner.TableEditor.ShowValidationPane();
		}
	}

	private void TableFoot_Click(object sender, EventArgs e)
	{
		if (TableFoot.Checked)
		{
			_owner.TableEditor.HideFootPane();
			return;
		}
		_owner.TableEditor.ShowFootPane();
		_owner.TableEditor.HideValidationPane();
	}

	private SplitContainerRibbonControlHost(C1SplitContainer ctn)
		: base(ctn)
	{
		_ctn = ctn;
		_ctn.Dock = DockStyle.Fill;
		C1SplitterPanel c1SplitterPanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			Resizable = false,
			SizeRatio = 50.0,
			KeepRelativeSize = true
		};
		_ctn.Panels.Add(c1SplitterPanel);
		UserLabel = new C1SuperLabel
		{
			AutoSize = true
		};
		c1SplitterPanel.Controls.Add(UserLabel);
		c1SplitterPanel = new C1SplitterPanel
		{
			Dock = PanelDockStyle.Left,
			Resizable = false,
			SizeRatio = 50.0,
			KeepRelativeSize = true
		};
		_ctn.Panels.Add(c1SplitterPanel);
		NextErrorButton = new C1Button
		{
			Dock = DockStyle.Left,
			Image = Leqisoft.UI.Platform.Properties.Resources.NextError_S
		};
		_ttp.SetToolTip(NextErrorButton, "下一个错误");
		NextErrorButton.Width = NextErrorButton.Height;
		NextErrorButton.Click += NextErrorButton_Click;
		c1SplitterPanel.Controls.Add(NextErrorButton);
		ValidationFormula = new C1CheckBox
		{
			Appearance = Appearance.Button,
			BackgroundImageLayout = ImageLayout.Center,
			Dock = DockStyle.Left,
			AutoCheck = false,
			BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.ValidationFormula16,
			FlatStyle = FlatStyle.Flat
		};
		ValidationFormula.FlatAppearance.BorderSize = 0;
		ValidationFormula.Width = ValidationFormula.Height;
		ValidationFormula.Click += ValidationFormula_Click;
		_ttp.SetToolTip(ValidationFormula, "显示/隐藏校验公式区");
		c1SplitterPanel.Controls.Add(ValidationFormula);
		TableNote = new C1CheckBox
		{
			Appearance = Appearance.Button,
			BackgroundImageLayout = ImageLayout.Center,
			Dock = DockStyle.Left,
			AutoCheck = false,
			BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.TableNote16,
			FlatStyle = FlatStyle.Flat
		};
		TableNote.FlatAppearance.BorderSize = 0;
		TableNote.Width = TableNote.Height;
		_ttp.SetToolTip(TableNote, "显示/隐藏" + StringConstBase.Current.TableNote + "区");
		TableFoot = new C1CheckBox
		{
			Appearance = Appearance.Button,
			BackgroundImage = Leqisoft.UI.Platform.Properties.Resources.TableFoot16,
			BackgroundImageLayout = ImageLayout.Center,
			Dock = DockStyle.Left,
			FlatStyle = FlatStyle.Flat,
			AutoCheck = false
		};
		TableFoot.FlatAppearance.BorderSize = 0;
		TableFoot.Width = TableFoot.Height;
		TableFoot.Click += TableFoot_Click;
		_ttp.SetToolTip(TableFoot, "显示/隐藏表底签名区");
		PreviousErrorButton = new C1Button
		{
			Dock = DockStyle.Left,
			Image = Leqisoft.UI.Platform.Properties.Resources.PreviousError_S
		};
		_ttp.SetToolTip(PreviousErrorButton, "上一个错误");
		PreviousErrorButton.Width = PreviousErrorButton.Height;
		PreviousErrorButton.Click += PreviousErrorButton_Click;
		c1SplitterPanel.Controls.Add(PreviousErrorButton);
		SelectionStatsLabel = new C1SuperLabel
		{
			Dock = DockStyle.Right,
			AutoSize = true
		};
		c1SplitterPanel.Controls.Add(SelectionStatsLabel);
		AttachTooltip();
	}
}
