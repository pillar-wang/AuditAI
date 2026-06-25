using System;

namespace Auditai.UI.Controls;

public class FindReplaceEventArgs : EventArgs
{
	public string SearchText { get; set; }

	public string ReplaceText { get; set; }

	public SearchFlag SearchFlag { get; set; }

	public FindReplaceEventArgs()
	{
	}

	public FindReplaceEventArgs(string searchText, string replaceText, SearchFlag searchFlag)
	{
		SearchText = searchText;
		ReplaceText = replaceText;
		SearchFlag = searchFlag;
	}
}
