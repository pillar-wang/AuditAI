using System.Windows.Forms;

namespace Auditai.UI.Controls;

public class BulletLauncher
{
	private BulletForm _form;

	public BulletLauncher(Form owner)
	{
		_form = new BulletForm(owner);
	}

	public void Launch(Bullet bullet)
	{
		_form.Launch(bullet);
	}
}
