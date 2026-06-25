using System;
using Auditai.DTO;

namespace Auditai.UI.Platform;

public class WechatLogin
{
	private wechatLoginForm _form;

	public Tuple<UserToken, User> ShowDialog()
	{
		if (_form == null)
		{
			_form = new wechatLoginForm();
		}
		_form.ShowDialog();
		return _form.resultMsg;
	}
}
