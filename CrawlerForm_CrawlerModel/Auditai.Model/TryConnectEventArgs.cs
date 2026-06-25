using System;
using System.Data.Common;

namespace Auditai.Model;

public class TryConnectEventArgs : EventArgs
{
	public bool IsSuccess { get; set; }

	public DbException Exception { get; set; }

	public TryConnectEventArgs(bool isSuccess, DbException exception)
	{
		IsSuccess = isSuccess;
		Exception = exception;
	}
}
