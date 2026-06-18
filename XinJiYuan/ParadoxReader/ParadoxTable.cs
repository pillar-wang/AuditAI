using System;
using System.IO;

namespace ParadoxReader;

public class ParadoxTable : ParadoxFile
{
	public readonly ParadoxPrimaryKey PrimaryKeyIndex;

	private readonly ParadoxBlobFile BlobFile;

	public ParadoxTable(string dbPath, string tableName)
		: base(Path.Combine(dbPath, tableName + ".db"))
	{
		string[] files = Directory.GetFiles(dbPath, tableName + "*.*");
		string[] array = files;
		foreach (string text in array)
		{
			if (!(Path.GetFileName(text) == tableName + ".db"))
			{
				if (Path.GetFileNameWithoutExtension(text).EndsWith(".PX", StringComparison.InvariantCultureIgnoreCase) || Path.GetExtension(text).Equals(".PX", StringComparison.InvariantCultureIgnoreCase))
				{
					PrimaryKeyIndex = new ParadoxPrimaryKey(this, text);
					break;
				}
				if (Path.GetFileNameWithoutExtension(text).EndsWith(".MB", StringComparison.InvariantCultureIgnoreCase) || Path.GetExtension(text).Equals(".MB", StringComparison.InvariantCultureIgnoreCase))
				{
					BlobFile = new ParadoxBlobFile(text);
				}
			}
		}
	}

	internal override byte[] ReadBlob(byte[] blobInfo)
	{
		if (BlobFile == null)
		{
			return base.ReadBlob(blobInfo);
		}
		return BlobFile.ReadBlob(blobInfo);
	}

	public override void Dispose()
	{
		base.Dispose();
		if (PrimaryKeyIndex != null)
		{
			PrimaryKeyIndex.Dispose();
		}
		if (BlobFile != null)
		{
			BlobFile.Dispose();
		}
	}
}
