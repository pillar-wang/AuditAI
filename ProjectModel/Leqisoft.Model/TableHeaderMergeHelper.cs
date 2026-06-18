using System.Collections.Generic;
using System.Linq;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public static class TableHeaderMergeHelper
{
	public static List<CellRange> GetHeaderMergeInfoVisibleOnly(Table table)
	{
		if (table.Columns.VisibleCount == 0)
		{
			return new List<CellRange>();
		}
		int numVisibleCaptionRows = table.GetNumVisibleCaptionRows();
		return removeDup(GetHeaderMergeInfoImpl(numVisibleCaptionRows, table.Columns.WhereVisible.ToList()));
	}

	public static List<CellRange> GetHeaderMergeInfo(Table table)
	{
		if (table.Columns.Count == 0)
		{
			return new List<CellRange>();
		}
		int numCaptionRows = table.GetNumCaptionRows();
		return removeDup(GetHeaderMergeInfoImpl(numCaptionRows, table.Columns.ToList()));
	}

	private static List<CellRange> GetHeaderMergeInfoImpl(int numCaptionRows, List<Column> columns)
	{
		List<CellRange> list = new List<CellRange>();
		string text = null;
		int c2 = -1;
		int num = -1;
		for (int i = 0; i < columns.Count; i++)
		{
			string caption = columns[i].Caption;
			if (!caption.Contains("_") && caption == text)
			{
				num = i;
				continue;
			}
			if (num > -1)
			{
				list.Add(new CellRange(0, c2, numCaptionRows - 1, num));
			}
			text = caption;
			c2 = i;
			num = -1;
		}
		List<string[]> list2 = columns.Select(delegate(Column c)
		{
			string text3 = c.CaptionDisplay;
			if (text3.StartsWith("_"))
			{
				text3 = " " + text3;
			}
			if (text3 == string.Empty)
			{
				text3 = " ";
			}
			int num3 = text3.Count((char c1) => c1 == '_') + 1;
			text3 += new string('_', numCaptionRows - num3);
			return text3.Split('_');
		}).ToList();
		bool[,] array = new bool[numCaptionRows, columns.Count];
		for (int j = 0; j < columns.Count; j++)
		{
			string[] array2 = list2[j];
			for (int k = 0; k < array2.Length; k++)
			{
				if (array2[k] == "")
				{
					int r = k - 1;
					array[k - 1, j] = true;
					for (; k < array2.Length && array2[k] == ""; k++)
					{
						array[k, j] = true;
					}
					int r2 = k - 1;
					list.Add(new CellRange(r, j, r2, j));
				}
			}
		}
		List<bool> list3 = columns.Select((Column _) => true).ToList();
		for (int l = 0; l < numCaptionRows; l++)
		{
			string text2 = null;
			int num2 = -1;
			for (int m = 0; m < columns.Count; m++)
			{
				if (list2[m][l] == text2 && !array[l, m - 1] && !array[l, m] && list3[m])
				{
					if (num2 == -1)
					{
						num2 = m - 1;
					}
					continue;
				}
				list3[m] = false;
				if (num2 != -1)
				{
					list.Add(new CellRange(l, num2, l, m - 1));
					num2 = -1;
				}
				text2 = list2[m][l];
			}
			if (num2 != -1)
			{
				list.Add(new CellRange(l, num2, l, columns.Count - 1));
			}
		}
		return list;
	}

	private static List<CellRange> removeDup(List<CellRange> merges)
	{
		if (merges == null)
		{
			return null;
		}
		List<CellRange> list = new List<CellRange>();
		foreach (CellRange merge in merges)
		{
			foreach (CellRange merge2 in merges)
			{
				if (!merge.Equals(merge2) && merge.r1 >= merge2.r1 && merge.r2 <= merge2.r2 && merge.c1 >= merge2.c1 && merge.c2 <= merge2.c2)
				{
					list.Add(merge);
					break;
				}
			}
		}
		merges = merges.Except(list).ToList();
		return merges;
	}
}
