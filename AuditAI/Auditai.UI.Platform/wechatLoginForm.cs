﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using C1.Win.C1Input;
using C1.Win.C1Ribbon;
using CefSharp;
using CefSharp.WinForms;
using Auditai.DTO;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;
using Auditai.Util;

namespace Auditai.UI.Platform;

public class wechatLoginForm : C1RibbonForm
{
	public class RenderProcessMessageHandler : IRenderProcessMessageHandler
	{
		public void OnContextReleased(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
		{
		}

		public void OnFocusedNodeChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IDomNode node)
		{
		}

		public void OnUncaughtException(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, JavascriptException exception)
		{
		}

		void IRenderProcessMessageHandler.OnContextCreated(IWebBrowser browserControl, IBrowser browser, IFrame frame)
		{
			frame.ExecuteJavaScriptAsync("document.addEventListener('DOMContentLoaded', function(){document.body.style.overflow='hidden';});");
		}
	}

	private const string CODE = "code";

	private const string STATE = "state";

	private const string wechat_AppId = "wx35b8673de0264d25";

	private const string callback = "about:blank";

	private ChromiumWebBrowser _browser;

	private IContainer components;

	private C1Button btnClose;

	public Tuple<UserToken, User> resultMsg { get; set; }

	public wechatLoginForm()
	{
		InitializeComponent();
		base.Shown += WechatLoginForm_Shown;
		base.StartPosition = FormStartPosition.CenterScreen;
		// Cef.Initialize 未执行，不创建 ChromiumWebBrowser 实例
		// 避免析构时 Cef.RemoveDisposable() 因 Cef 运行时未初始化而崩溃
		_browser = null;
		Theme.SetCurrentTree(this);
	}

	private void _browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
	{
		_ = e.IsLoading;
	}

	private void _browser_IsBrowserInitializedChanged(object sender, EventArgs e)
	{
		_ = _browser.IsBrowserInitialized;
	}

	private void _browser_AddressChanged(object sender, AddressChangedEventArgs e)
	{
		Invoke((Action)async delegate
		{
			if (e.Address.StartsWith("http://www.Auditai.com/logintemp.html"))
			{
				try
				{
					Uri url = new Uri(e.Address);
					if (url.TryGetValue("code", out var value) && url.TryGetValue("state", out var value2))
					{
						Tuple<UserToken, User> tuple = await WebApiClient.WechatLogin(value, value2);
						resultMsg = tuple;
						await Task.Delay(1000);
					}
				}
				catch (HttpRequestException ex)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex.InnerException.Message);
				}
				catch (TimeoutException ex2)
				{
					Auditai.UI.Controls.MessageBox.Show(MessageBoxIcon.None, ex2.Message);
				}
				finally
				{
					Close();
				}
			}
		});
	}

	private void WechatLoginForm_Shown(object sender, EventArgs e)
	{
		base.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.wechat_img);
	}

	private void weChatLogin_Load(object sender, EventArgs e)
	{
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		Close();
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Auditai.UI.Platform.wechatLoginForm));
		this.btnClose = new C1.Win.C1Input.C1Button();
		((System.ComponentModel.ISupportInitialize)this.btnClose).BeginInit();
		base.SuspendLayout();
		this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnClose.BackgroundImage = Auditai.UI.Platform.Properties.Resources.close2;
		this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.btnClose.FlatAppearance.BorderSize = 0;
		this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Red;
		this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
		this.btnClose.Location = new System.Drawing.Point(424, 0);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(25, 25);
		this.btnClose.TabIndex = 2;
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Visible = false;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(449, 627);
		base.Controls.Add(this.btnClose);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "wechatLoginForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "微信登录";
		base.Load += new System.EventHandler(weChatLogin_Load);
		((System.ComponentModel.ISupportInitialize)this.btnClose).EndInit();
		base.ResumeLayout(false);
	}
}
