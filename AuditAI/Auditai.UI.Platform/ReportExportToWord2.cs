using System.IO;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class ReportExportToWord2
{
	public Document Document { get; set; }

	public void Save(string path)
	{
		// 修复 BUG: 此前 File.Exists → File.Delete → File.Move 三步操作非原子（TOCTOU 竞态），
		// 且 MakePackage 返回的临时文件在 File.Move 失败时不会被清理，导致磁盘泄漏。
		// 改用 File.Copy(overwrite:true) 原子覆盖，并用 try/finally 确保临时文件总是被清理。
		string item = Document.MakePackage(isExport: true).Item1;
		try
		{
			File.Copy(item, path, overwrite: true);
		}
		finally
		{
			try { File.Delete(item); } catch { /* 忽略临时文件清理失败 */ }
		}
	}
}
