using System.Collections.Generic;

namespace Auditai.Model;

public static class ListExtensions
{
	public static void SafeInsert<T>(this List<T> list, int index, T item)
	{
		if (index > list.Count)
		{
			list.Add(item);
		}
		else
		{
			list.Insert(index, item);
		}
	}

	public static void SafeMove<T>(this List<T> list, int index, int newIndex)
	{
		T item = list[index];
		list.RemoveAt(index);
		if (newIndex > index)
		{
			list.SafeInsert(newIndex - 1, item);
		}
		else
		{
			list.SafeInsert(newIndex, item);
		}
	}
}
