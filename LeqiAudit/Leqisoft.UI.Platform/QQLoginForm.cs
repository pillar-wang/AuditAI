﻿using System;
using Leqisoft.DTO;

namespace Leqisoft.UI.Platform;

public class QQLoginForm
{
	private frmLoginQQ _form;

	public QQLoginForm()
	{
	}

	public Tuple<UserToken, User> ShowDialog()
	{
		if (_form == null)
		{
			_form = new frmLoginQQ();
		}
		_form.ShowDialog();
		return _form.resultMsg;
	}
}
