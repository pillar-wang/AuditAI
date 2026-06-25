using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Auditai.Model;

public static class Util
{
	private enum Key_
	{
		Read = 131097,
		Wow6432Key = 512,
		Wow6464Key = 256
	}

	[CompilerGenerated]
	private sealed class _003CGetFiles_003Ed__13 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string root;

		public string _003C_003E3__root;

		private string searchPattern;

		public string _003C_003E3__searchPattern;

		private Stack<string> _003Cpending_003E5__2;

		private string _003Cpath_003E5__3;

		private string[] _003C_003E7__wrap3;

		private int _003C_003E7__wrap4;

		string IEnumerator<string>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetFiles_003Ed__13(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cpending_003E5__2 = null;
			_003Cpath_003E5__3 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				_003C_003E7__wrap4++;
				goto IL_00af;
			}
			_003C_003E1__state = -1;
			_003Cpending_003E5__2 = new Stack<string>();
			_003Cpending_003E5__2.Push(root);
			goto IL_0123;
			IL_0123:
			if (_003Cpending_003E5__2.Count != 0)
			{
				_003Cpath_003E5__3 = _003Cpending_003E5__2.Pop();
				string[] array = null;
				try
				{
					array = Directory.GetFiles(_003Cpath_003E5__3, searchPattern);
				}
				catch
				{
				}
				if (array != null && array.Length != 0)
				{
					_003C_003E7__wrap3 = array;
					_003C_003E7__wrap4 = 0;
					goto IL_00af;
				}
				goto IL_00c7;
			}
			return false;
			IL_00af:
			if (_003C_003E7__wrap4 < _003C_003E7__wrap3.Length)
			{
				string text = _003C_003E7__wrap3[_003C_003E7__wrap4];
				_003C_003E2__current = text;
				_003C_003E1__state = 1;
				return true;
			}
			_003C_003E7__wrap3 = null;
			goto IL_00c7;
			IL_00c7:
			try
			{
				string[] array = Directory.GetDirectories(_003Cpath_003E5__3);
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (Cancel)
					{
						Cancel = false;
						throw new OperationCanceledException();
					}
					CurrentPath = text2;
					_003Cpending_003E5__2.Push(text2);
				}
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception)
			{
			}
			_003Cpath_003E5__3 = null;
			goto IL_0123;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CGetFiles_003Ed__13 _003CGetFiles_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetFiles_003Ed__ = this;
			}
			else
			{
				_003CGetFiles_003Ed__ = new _003CGetFiles_003Ed__13(0);
			}
			_003CGetFiles_003Ed__.root = _003C_003E3__root;
			_003CGetFiles_003Ed__.searchPattern = _003C_003E3__searchPattern;
			return _003CGetFiles_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	private static readonly UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(2147483650u);

	private const int ERROR_NO_MORE_ITEMS = 259;

	public static bool Cancel { get; set; } = false;


	public static string CurrentPath { get; private set; }

	[DllImport("advapi32")]
	private static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, int ulOptions, Key_ samDesired, out UIntPtr hkResult);

	[DllImport("advapi32")]
	private static extern uint RegEnumValue(UIntPtr hKey, uint dwIndex, StringBuilder lpValueName, ref uint lpcValueName, IntPtr lpReserved, IntPtr lpType, IntPtr lpData, IntPtr lpcbData);

	public static IEnumerable<string> GetFiles(string root, string searchPattern)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetFiles_003Ed__13(-2)
		{
			_003C_003E3__root = root,
			_003C_003E3__searchPattern = searchPattern
		};
	}

	public static string[] GetSqlServerInstanceNames()
	{
		List<string> list = new List<string>();
		if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL", 0, (Key_)131609, out var hkResult) == 0)
		{
			uint num = 0u;
			uint lpcValueName = 100u;
			StringBuilder stringBuilder = new StringBuilder((int)lpcValueName);
			for (uint num2 = RegEnumValue(hkResult, num, stringBuilder, ref lpcValueName, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero); num2 == 0; num2 = RegEnumValue(hkResult, num, stringBuilder, ref lpcValueName, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
			{
				list.Add(stringBuilder.ToString());
				num++;
			}
		}
		if (RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL", 0, (Key_)131353, out hkResult) == 0)
		{
			uint num3 = 0u;
			uint lpcValueName2 = 100u;
			StringBuilder stringBuilder2 = new StringBuilder((int)lpcValueName2);
			for (uint num4 = RegEnumValue(hkResult, num3, stringBuilder2, ref lpcValueName2, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero); num4 == 0; num4 = RegEnumValue(hkResult, num3, stringBuilder2, ref lpcValueName2, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
			{
				list.Add(stringBuilder2.ToString());
				num3++;
			}
		}
		return list.ToArray();
	}
}
