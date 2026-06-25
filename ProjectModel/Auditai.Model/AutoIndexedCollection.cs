using System.Collections.ObjectModel;

namespace Auditai.Model;

public class AutoIndexedCollection<T> : Collection<T> where T : class, IIndexable
{
	protected override void InsertItem(int index, T item)
	{
		item.Index = index;
		for (int i = index; i < base.Count; i++)
		{
			T val = base.Items[i];
			val.Index++;
		}
		base.InsertItem(index, item);
	}

	protected override void RemoveItem(int index)
	{
		for (int i = index + 1; i < base.Count; i++)
		{
			T val = base.Items[i];
			val.Index--;
		}
		base.RemoveItem(index);
	}
}
