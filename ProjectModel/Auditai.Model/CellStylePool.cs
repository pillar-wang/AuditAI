using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Auditai.DTO;

namespace Auditai.Model;

public class CellStylePool : IEnumerable<CellStyle>, IEnumerable
{
	internal Table _table;

	private List<CellStyle> _cellStyles = new List<CellStyle>();

	internal CellStylePool(Table table)
	{
		_table = table;
	}

	public IEnumerator<CellStyle> GetEnumerator()
	{
		return _cellStyles.GetEnumerator();
	}

	public void Add(CellStyle cellStyle)
	{
		cellStyle._pool = this;
		_cellStyles.Add(cellStyle);
	}

	public CellStyle Get(CellStyle style, Action<CellStyle> how)
	{
		if (style == null)
		{
			return null;
		}
		CellStyle clone = style.Clone();
		how(clone);
		return _cellStyles.FirstOrDefault((CellStyle c) => c.Equals(clone));
	}

	public CellStyle MutateAndGet(CellStyle style, Action<CellStyle> how)
	{
		CellStyle clone = ((style == null) ? new CellStyle() : style.Clone());
		how(clone);
		if (object.Equals(style, clone))
		{
			return style;
		}
		CellStyle cellStyle = _cellStyles.FirstOrDefault((CellStyle c) => c.Equals(clone));
		if (cellStyle == null)
		{
			clone.Id = Project.Current.GetNextId();
			clone.Status = SyncStatus.New;
			Add(clone);
			return clone;
		}
		return cellStyle;
	}

	internal CellStyle GetDefault()
	{
		CellStyle cellStyle = new CellStyle();
		cellStyle.FontFamily = UserSet.Config.TableStyle.FontStyle.FontFamily;
		cellStyle.BackColor = Color.Transparent;
		cellStyle.FontSize = UserSet.Config.TableStyle.FontStyle.FontSize;
		cellStyle.ForeColor = UserSet.Config.TableStyle.FontStyle.FontColor;
		cellStyle.Margin = 0;
		cellStyle.Align = CellTextAlign.MiddleLeft;
		cellStyle.Bold = false;
		cellStyle.Italic = false;
		cellStyle.Underline = false;
		cellStyle.DataType = typeof(string);
		cellStyle.Format = new DataFormat(DataFormatType.General);
		cellStyle.Locker = 0L;
		cellStyle.Id = Project.Current.GetNextId();
		cellStyle.DefaultValue = string.Empty;
		cellStyle.Comment = string.Empty;
		CellStyle cellStyle2 = cellStyle;
		Add(cellStyle2);
		return cellStyle2;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _cellStyles.GetEnumerator();
	}

	internal void Clear()
	{
		_cellStyles.Clear();
	}
}
