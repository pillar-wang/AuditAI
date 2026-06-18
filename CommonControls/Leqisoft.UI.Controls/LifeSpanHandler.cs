using System.Diagnostics;
using CefSharp;

namespace Leqisoft.UI.Controls;

public class LifeSpanHandler : ILifeSpanHandler
{
	public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
	{
		return false;
	}

	public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
	{
	}

	public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
	{
	}

	public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
	{
		newBrowser = null;
		if (targetUrl.StartsWith("http"))
		{
			Process.Start(targetUrl);
			return true;
		}
		return false;
	}
}
