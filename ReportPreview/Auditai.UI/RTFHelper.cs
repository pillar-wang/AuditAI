using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Drawing;
using Auditai.Model;
using TXTextControl;

namespace Auditai.UI;

public static class RTFHelper
{
	private static readonly ServerTextControl _tx;

	static RTFHelper()
	{
		try
		{
			EnsureTxLicense();
			_tx = new ServerTextControl();
			_tx.Create();
		}
		catch
		{
			_tx = null;
		}
	}

	/// <summary>
	/// 从入口程序集的嵌入资源中读取 .licenses 文件，
	/// 绕过 cryptoKey 校验直接注入到 RuntimeLicenseContext.savedLicenseKeys。
	/// </summary>
	private static void EnsureTxLicense()
	{
		try
		{
			var entryAsm = Assembly.GetEntryAssembly();
			if (entryAsm == null) return;

			var context = LicenseManager.CurrentContext;
			if (context == null) return;

			var contextType = context.GetType();
			if (contextType.Name != "RuntimeLicenseContext") return;

			var keysField = contextType.GetField("savedLicenseKeys",
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if (keysField == null) return;

			string resourceName = entryAsm.GetManifestResourceNames()
				.FirstOrDefault(n => n.Equals(entryAsm.GetName().Name + ".exe.licenses", StringComparison.OrdinalIgnoreCase) ||
				                     n.Equals(entryAsm.GetName().Name + ".dll.licenses", StringComparison.OrdinalIgnoreCase) ||
				                     n.EndsWith(".licenses", StringComparison.OrdinalIgnoreCase));
			if (resourceName == null) return;

			using (var stream = entryAsm.GetManifestResourceStream(resourceName))
			{
				if (stream == null) return;

				var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				object obj = formatter.Deserialize(stream);

				if (obj is object[] arr && arr.Length >= 2 && arr[1] is Hashtable licenseKeys)
				{
					keysField.SetValue(context, licenseKeys);
				}
			}
		}
		catch { }
	}

	public static string ParsePlainText(string Rtf)
	{
		if (string.IsNullOrWhiteSpace(Rtf))
		{
			return Rtf;
		}
		_tx.Load(Rtf, StringStreamType.RichTextFormat);
		return _tx.Text;
	}

	public static string MakeBlackWhite(string rtf)
	{
		_tx.Load(rtf, StringStreamType.RichTextFormat);
		_tx.SelectAll();
		_tx.Selection.TextBackColor = Color.Transparent;
		_tx.Selection.ForeColor = Color.Black;
		_tx.Save(out var stringData, StringStreamType.RichTextFormat);
		return stringData;
	}

	public static string SystemVariable(DataReferenceManager drm, string rtf, DataReferenceEvaluationContext context)
	{
		_tx.Load(rtf, StringStreamType.RichTextFormat);
		Tuple<int, int, string> tuple = null;
		while ((tuple = drm.FindIn(_tx.Text, context)) != null)
		{
			_tx.Select(tuple.Item1, tuple.Item2);
			_tx.Selection.Text = tuple.Item3 ?? string.Empty;
		}
		_tx.Save(out var stringData, StringStreamType.RichTextFormat);
		return stringData;
	}
}
