using System.Collections.Generic;

namespace ParadoxReader;

public class ParadoxPrimaryKey : ParadoxFile
{
	private readonly ParadoxTable table;

	public ParadoxPrimaryKey(ParadoxTable table, string filePath)
		: base(filePath)
	{
		this.table = table;
	}

	public IEnumerable<ParadoxRecord> Enumerate(ParadoxCondition condition)
	{
		return Enumerate(condition, (ushort)(pxRootBlockId - 1), pxLevelCount);
	}

	private IEnumerable<ParadoxRecord> Enumerate(ParadoxCondition condition, ushort blockId, int indexLevel)
	{
		if (indexLevel == 0)
		{
			DataBlock block = table.GetBlock(blockId);
			for (int i = 0; i < block.RecordCount; i++)
			{
				ParadoxRecord rec = block[i];
				if (condition.IsDataOk(rec))
				{
					yield return rec;
				}
			}
			yield break;
		}
		DataBlock block2 = GetBlock(blockId);
		int blockIdFldIndex = base.FieldCount - 3;
		for (int j = 0; j < block2.RecordCount; j++)
		{
			ParadoxRecord rec2 = block2[j];
			if (!condition.IsIndexPossible(rec2, (j < block2.RecordCount - 1) ? block2[j + 1] : null))
			{
				continue;
			}
			IEnumerable<ParadoxRecord> qry = Enumerate(condition, (ushort)((short)rec2.DataValues[blockIdFldIndex] - 1), indexLevel - 1);
			foreach (ParadoxRecord item in qry)
			{
				yield return item;
			}
		}
	}
}
