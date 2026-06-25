using System;
using C1.Win.FlexViewer;

namespace Auditai.UI.Platform;

public class TempFlexViewerPane : IDisposable
{
	private frmTempFlexViewerPane _temp = new frmTempFlexViewerPane();

	public C1FlexViewerPane FlexViewerPane => _temp._fvp;

	public TempFlexViewerPane()
	{
		_temp.Show();
	}

	public void Dispose()
	{
		_temp.Close();
	}
}
