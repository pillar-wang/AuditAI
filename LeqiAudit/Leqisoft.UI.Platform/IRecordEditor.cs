using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Leqisoft.UI.Platform;

public interface IRecordEditor
{
	event EventHandler PreviousRecord;

	void EmptyView();

	void ScrollEnd();

	void Insert(IEnumerable<ChatRecord> records);

	void Append(ChatRecord record);

	void Populate(IEnumerable<ChatRecord> records);

	Control View();

	void SetTheme();
}
