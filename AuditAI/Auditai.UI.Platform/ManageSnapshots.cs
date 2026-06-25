using System.Windows.Forms;
using Auditai.DTO;
using Auditai.UI.Controls;
using Auditai.UI.Platform.Properties;

namespace Auditai.UI.Platform;

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
		_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.Snapshots);
		return _form.ShowDialog();
	}

	public DialogResult ShowRecycle()
	{
		_form.View = frmManageSnapshots.ViewKind.Recycle;
		_form.ShowIcon = true;
		_form.Icon = Theme.SelectedAuditaiTheme.GetThemedIcon(Resources.RecycleNode);
		return _form.ShowDialog();
	}
}
