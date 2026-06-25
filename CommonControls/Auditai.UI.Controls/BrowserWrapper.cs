﻿using System;
using CefSharp;
using CefSharp.WinForms;

namespace Auditai.UI.Controls;

public class BrowserWrapper
{
	private ChromiumWebBrowser _browser;

	public ChromiumWebBrowser Control => _browser;

	public event EventHandler<OpenPageEventArgs> OpenPage;

	public event EventHandler Close;

	public BrowserWrapper()
	{
		// Cef.Initialize 未执行，不创建 ChromiumWebBrowser 实例
		// 避免析构时 Cef.RemoveDisposable() 因 Cef 运行时未初始化而崩溃
		_browser = null;
	}

	private void _browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
	{
		if (!e.IsLoading)
		{
			SetTheme();
		}
	}

	private void _browser_JavascriptMessageReceived(object sender, JavascriptMessageReceivedEventArgs e)
	{
		dynamic msg = e.Message;
		string evt = (string)msg.@event;
		_browser.Invoke((Action)delegate
		{
			string text = evt;
			if (!(text == "OpenPage"))
			{
				if (text == "Close")
				{
					OnClose();
				}
			}
			else
			{
				OnOpenPage(new OpenPageEventArgs
				{
					url = msg.args.url,
					title = msg.args.title
				});
			}
		});
	}

	public void SetVariable(string name, string value)
	{
		if (_browser != null && _browser.CanExecuteJavascriptInMainFrame)
		{
			Control.EvaluateScriptAsync("document.querySelector(':root').style.setProperty('--" + name + "','" + value + "');");
		}
	}

	public void Load(string url)
	{
		if (_browser != null)
			_browser.Load(url);
	}

	public void SetTheme()
	{
		AuditaiTheme selectedAuditaiTheme = Theme.SelectedAuditaiTheme;
		SetVariable("chapter-header-background", selectedAuditaiTheme.GetCssBackground("C1Command\\C1OutBar\\Page\\Title\\Default\\Background"));
		SetVariable("chapter-header-background-hover", selectedAuditaiTheme.GetCssBackground("C1Command\\C1OutBar\\Page\\Title\\Hot\\Background"));
		SetVariable("chapter-header-background-active", selectedAuditaiTheme.GetCssBackground("C1Command\\C1OutBar\\Page\\Title\\Pressed\\Background"));
		SetVariable("chapter-header-color", selectedAuditaiTheme.GetCssColor("C1Command\\C1OutBar\\Page\\Title\\Default\\ForeColor"));
		SetVariable("page-button-background", selectedAuditaiTheme.GetCssBackground("C1FlexGrid\\Styles\\Normal\\Background"));
		SetVariable("page-button-background-hover", selectedAuditaiTheme.GetCssBackground("C1FlexGrid\\Styles\\Highlight\\Background", 100));
		SetVariable("page-button-background-active", selectedAuditaiTheme.GetCssBackground("C1FlexGrid\\Styles\\Highlight\\Background"));
		SetVariable("page-button-color", selectedAuditaiTheme.GetCssColor("C1FlexGrid\\Styles\\Normal\\ForeColor"));
		SetVariable("page-button-color-active", selectedAuditaiTheme.GetCssColor("C1FlexGrid\\Styles\\Highlight\\ForeColor"));
		SetVariable("search-item-background", selectedAuditaiTheme.GetCssColor("C1TileControl\\Tiles\\TileBackColor"));
	}

	protected void OnOpenPage(OpenPageEventArgs e)
	{
		this.OpenPage?.Invoke(this, e);
	}

	protected void OnClose()
	{
		this.Close?.Invoke(this, EventArgs.Empty);
	}
}
