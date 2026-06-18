using System.Windows.Forms;
using Leqisoft.DTO;
using Leqisoft.UI.Controls;
using Leqisoft.UI.Platform.Properties;

namespace Leqisoft.UI.Platform;

public class ManageSnapshots
{
	private frmManageSnapshots _form;

	public SnapshotInfo SelectedSnapshot => _form.SelectedSnapshot;

	public ManageSnapshots()
	{
		_form = new frmManageSnapshots(this);
	}

	public DialogResult ShowSnapshots()
	{
		_form.View = frmManageSnapshots.ViewKind.Snapshot;
		_form.ShowIcon = true;
		_form.Icon = Theme.SelectedLeqiTheme.GetThemedIcon(Resources.Snapshots);
		return _form.ShowDialog();
	}

	public DialogResult ShowRecycle()
	{
		_form.View = frmManageSnapshots.ViewKind.Recycle;
		_form.ShowIcon = true;
		_form.Icon = Theme.SelectedLeqiTheme.GetThemedIcon(Resources.RecycleNode);
		return _form.ShowDialog();
	}
}
