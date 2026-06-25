using System.Collections.Generic;

namespace Auditai.Model;

internal class CellListRowsCountDictionary
{
	protected struct CacheData
	{
		public List<Cell> CheckValue;

		public int ValueResult;
	}

	protected const int MaxCacheCount = 10;

	protected CacheData[] _cacheResult = new CacheData[10];

	protected int _cacheCount;

	protected int _firstValueIndex;

	protected int _nextValueWriteStartIndex;

	public bool Get(List<Cell> key, out int result)
	{
		for (int i = 0; i < _cacheCount; i++)
		{
			int num = (_firstValueIndex + i) % 10;
			if (_cacheResult[num].CheckValue == key)
			{
				result = _cacheResult[num].ValueResult;
				return true;
			}
		}
		result = 0;
		return false;
	}

	public void Add(List<Cell> key, int value)
	{
		if (_cacheCount == 10)
		{
			_firstValueIndex = (_firstValueIndex + 1) % 10;
		}
		else
		{
			_cacheCount++;
		}
		_cacheResult[_nextValueWriteStartIndex].CheckValue = key;
		_cacheResult[_nextValueWriteStartIndex].ValueResult = value;
		_nextValueWriteStartIndex = (_nextValueWriteStartIndex + 1) % 10;
	}

	public void Clear()
	{
		_firstValueIndex = 0;
		_nextValueWriteStartIndex = 0;
		_cacheCount = 0;
	}
}
