﻿using System;
using System.Drawing;
using System.Windows.Forms;
using C1.Win.C1Ribbon;
using CefSharp;
using CefSharp.Web;
using CefSharp.WinForms;
using Leqisoft.UI.Controls.Properties;

namespace Leqisoft.UI.Controls;

public class FormHelpCenter
{
	private readonly C1RibbonForm _form;

	private ChromiumWebBrowser _browser;

	public static bool IsOpen { get; private set; }

	public string Text
	{
		get
		{
			return _form.Text;
		}
		set
		{
			_form.Text = value;
		}
	}

	public string Url { get; set; }

	public string RootPage { get; set; }

	public FormHelpCenter()
	{
		_form = new C1RibbonForm
		{
			Size = new Size(1024, 768),
			StartPosition = FormStartPosition.CenterScreen
		};
		_form.Load += _form_Load;
		_form.FormClosed += _form_Closed;
		IsOpen = true;
	}

	private void _form_Closed(object sender, FormClosedEventArgs e)
	{
		IsOpen = false;
	}

	private async void _browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
	{
		if (!e.IsLoading)
		{
			SetTheme();
			if (RootPage != null && RootPage.Equals(e.Browser.MainFrame.Url))
			{
				await _browser.EvaluateScriptAsync("Load", Url);
			}
		}
	}

	private void _form_Load(object sender, EventArgs e)
	{
		_form.Icon = Theme.SelectedLeqiTheme.GetThemedIcon(Resources.HelpCenter16, Resources.HelpCenter24);
		Theme.SetCurrentTree(_form);
	}

	public void Show()
	{
		try
		{
			_form.Show();
			// Cef.Initialize 未执行，不创建 ChromiumWebBrowser 实例
			// 避免析构时 Cef.RemoveDisposable() 因 Cef 运行时未初始化而崩溃
			_browser = null;
			_form.Update();
		}
		catch (Exception)
		{
		}
	}

	private void _browser_IsBrowserInitializedChanged(object sender, EventArgs e)
	{
		_browser.Load(RootPage);
	}

	public void SetTheme()
	{
		LeqiTheme selectedLeqiTheme = Theme.SelectedLeqiTheme;
		SetVariable("chapter-header-background", selectedLeqiTheme.GetCssBackground("C1Command\\C1OutBar\\Page\\Title\\Default\\Background"));
		SetVariable("chapter-header-background-hover", selectedLeqiTheme.GetCssBackground("C1Command\\C1OutBar\\Page\\Title\\Hot\\Background"));
		SetVariable("chapter-header-background-active", selectedLeqiTheme.GetCssBackground("C1Command\\C1OutBar\\Page\\Title\\Pressed\\Background"));
		SetVariable("chapter-header-color", selectedLeqiTheme.GetCssColor("C1Command\\C1OutBar\\Page\\Title\\Default\\ForeColor"));
		SetVariable("page-button-background", selectedLeqiTheme.GetCssBackground("C1FlexGrid\\Styles\\Normal\\Background"));
		SetVariable("page-button-background-hover", selectedLeqiTheme.GetCssBackground("C1FlexGrid\\Styles\\Highlight\\Background", 100));
		SetVariable("page-button-background-active", selectedLeqiTheme.GetCssBackground("C1FlexGrid\\Styles\\Highlight\\Background"));
		SetVariable("page-button-color", selectedLeqiTheme.GetCssColor("C1FlexGrid\\Styles\\Normal\\ForeColor"));
		SetVariable("page-button-color-active", selectedLeqiTheme.GetCssColor("C1FlexGrid\\Styles\\Highlight\\ForeColor"));
		SetVariable("search-item-background", selectedLeqiTheme.GetCssColor("C1TileControl\\Tiles\\TileBackColor"));
	}

	public void SetVariable(string name, string value)
	{
		if (_browser != null && _browser.CanExecuteJavascriptInMainFrame)
		{
			_browser.EvaluateScriptAsync("document.querySelector(':root').style.setProperty('--" + name + "','" + value + "');");
		}
	}
}
