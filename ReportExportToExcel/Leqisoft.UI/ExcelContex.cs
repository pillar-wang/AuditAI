using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using C1.C1Excel;
using Leqisoft.Model;

namespace Leqisoft.UI;

public class ExcelContex : Dictionary<XLSheet, BookInfo>
{
	private Dictionary<Table, BookInfo> _tableDic = new Dictionary<Table, BookInfo>();

	public int dataRowStartIndex;

	public int dataRowsCount;

	public BookInfo this[Table tb] => _tableDic[tb];

	public event EventHandler<string> BeforeTableSave;

	public async Task ExportFormula()
	{
		await Task.Factory.StartNew(delegate
		{
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<XLSheet, BookInfo> current = enumerator.Current;
				this.BeforeTableSave?.Invoke(this, Path.GetFileName(current.Value.SavePath));
				try
				{
					current.Value.Exporter.Save(this);
				}
				catch (Exception)
				{
				}
			}
		});
	}

	public new void Add(XLSheet xlSheet, BookInfo bookInfo)
	{
		base.Add(xlSheet, bookInfo);
		_tableDic.Add(bookInfo.Table, bookInfo);
	}
}
