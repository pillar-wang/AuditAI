using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Auditai.UI.Platform;

public class ProjectUsersListSelector : UserControl
{
	private readonly ListBox _listBox = new ListBox();
	private List<object> _allUsers = new List<object>();

	public ProjectUsersListSelector()
	{
		_listBox.Dock = DockStyle.Fill;
		Controls.Add(_listBox);
	}

	public void PopulateUsers(params object[] args)
	{
		try
		{
			_allUsers.Clear();
			_listBox.Items.Clear();

			if (args.Length > 0 && args[0] is IEnumerable<object> users)
			{
				_allUsers.AddRange(users);
				foreach (var user in _allUsers)
				{
					_listBox.Items.Add(user?.ToString() ?? "");
				}
			}
		}
		catch
		{
			// 加载用户列表失败时静默处理
		}
	}

	public object Context { get; set; }

	public Control GetControl(params object[] args)
	{
		return _listBox;
	}

	public void Search(params object[] args)
	{
		try
		{
			_listBox.Items.Clear();
			string searchText = args.Length > 0 ? args[0]?.ToString()?.ToLower() ?? "" : "";

			foreach (var user in _allUsers)
			{
				var display = user?.ToString() ?? "";
				if (string.IsNullOrEmpty(searchText) || display.ToLower().Contains(searchText))
				{
					_listBox.Items.Add(display);
				}
			}
		}
		catch
		{
			// 搜索失败时静默处理
		}
	}

	public void SetTheme(params object[] args)
	{
		try
		{
			// 应用主题：暂为基础实现
			BackColor = System.Drawing.Color.White;
			ForeColor = System.Drawing.Color.Black;
		}
		catch
		{
			// 设置主题失败时静默处理
		}
	}

	public object ValidateAndGetUsers(params object[] args) { return null; }
}