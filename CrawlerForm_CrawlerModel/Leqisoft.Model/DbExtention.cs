using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;

namespace Leqisoft.Model;

public static class DbExtention
{
	private class TimeTask<T>
	{
		private Func<T> _action;

		private int timeout = 3;

		private T _result;

		public TimeTask(Func<T> action)
		{
			_action = action;
		}

		public TimeTask(Func<T> action, int timeout)
		{
			_action = action;
			this.timeout = timeout;
		}

		public T Excute()
		{
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				_result = _action();
			}
			catch (SqlException)
			{
			}
			catch (InvalidOperationException)
			{
			}
		});
			thread.IsBackground = true;
			thread.Start();
			thread.Join(timeout * 1000);
			return _result;
		}
	}

	public static bool TableExists(this LSDb db, string tableName, int timeout)
	{
		return new TimeTask<bool>(() => db.TableExists(tableName), timeout).Excute();
	}

	public static IEnumerable<string> GetDatabaseNames(this LSDb db, int timeout)
	{
		return new TimeTask<IEnumerable<string>>(() => db.GetDatabaseNames(), timeout).Excute();
	}
}
