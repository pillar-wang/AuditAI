using System.IO;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class ReportExportToWord2
{
	public Document Document { get; set; }

	public void Save(string path)
	{
		string item = Document.MakePackage(isExport: true).Item1;
		if (File.Exists(path))
		{
			File.Delete(path);
		}
		File.Move(item, path);
	}
}
