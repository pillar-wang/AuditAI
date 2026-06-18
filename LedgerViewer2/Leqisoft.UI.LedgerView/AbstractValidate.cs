using System.Collections.Generic;

namespace Leqisoft.UI.LedgerView;

public abstract class AbstractValidate<T>
{
	protected IEnumerable<T> _validateItems;

	protected abstract bool TryValidate(T t, out ValidateResult result);

	public IEnumerable<ValidateResult> Validate()
	{
		List<ValidateResult> list = new List<ValidateResult>();
		foreach (T validateItem in _validateItems)
		{
			if (TryValidate(validateItem, out var result))
			{
				list.Add(result);
			}
		}
		return list;
	}
}
