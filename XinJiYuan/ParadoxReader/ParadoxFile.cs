using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ParadoxReader;

public class ParadoxFile : IDisposable
{
	public class V4Hdr
	{
		private short fileVerID2;

		private short fileVerID3;

		private int encryption2;

		private int fileUpdateTime;

		private ushort hiFieldID;

		private ushort hiFieldIDinfo;

		private short sometimesNumFields;

		private ushort dosCodePage;

		private byte[] unknown6Cx6F;

		private short changeCount4;

		private byte[] unknown72x77;

		public V4Hdr(BinaryReader r)
		{
			fileVerID2 = r.ReadInt16();
			fileVerID3 = r.ReadInt16();
			encryption2 = r.ReadInt32();
			fileUpdateTime = r.ReadInt32();
			hiFieldID = r.ReadUInt16();
			hiFieldIDinfo = r.ReadUInt16();
			sometimesNumFields = r.ReadInt16();
			dosCodePage = r.ReadUInt16();
			unknown6Cx6F = r.ReadBytes(4);
			changeCount4 = r.ReadInt16();
			unknown72x77 = r.ReadBytes(6);
		}
	}

	internal class DataBlock
	{
		public ParadoxFile file;

		private ushort nextBlock;

		private ushort blockNumber;

		private short addDataSize;

		public byte[] data;

		private ParadoxRecord[] recCache;

		public int RecordCount { get; private set; }

		public ParadoxRecord this[int recIndex]
		{
			get
			{
				if (recCache[recIndex] == null)
				{
					recCache[recIndex] = new ParadoxRecord(this, recIndex);
				}
				return recCache[recIndex];
			}
		}

		public DataBlock(ParadoxFile file, BinaryReader r)
		{
			this.file = file;
			nextBlock = r.ReadUInt16();
			blockNumber = r.ReadUInt16();
			addDataSize = r.ReadInt16();
			RecordCount = addDataSize / file.RecordSize + 1;
			data = r.ReadBytes(RecordCount * file.RecordSize);
			recCache = new ParadoxRecord[data.Length];
		}
	}

	internal class FieldInfo
	{
		public ParadoxFieldTypes fType;

		public byte fSize;

		public FieldInfo(ParadoxFieldTypes fType, byte fSize)
		{
			this.fType = fType;
			this.fSize = fSize;
		}

		public FieldInfo(BinaryReader r)
		{
			fType = (ParadoxFieldTypes)r.ReadByte();
			fSize = r.ReadByte();
		}
	}

	public string TableName;

	private ushort headerSize;

	private byte maxTableSize;

	private ushort nextBlock;

	private ushort fileBlocks;

	private ushort firstBlock;

	private ushort lastBlock;

	private ushort unknown12x13;

	private byte modifiedFlags1;

	private byte indexFieldNumber;

	private int primaryIndexWorkspace;

	private int unknownPtr1A;

	protected ushort pxRootBlockId;

	protected byte pxLevelCount;

	private short primaryKeyFields;

	private int encryption1;

	private byte sortOrder;

	private byte modifiedFlags2;

	private byte[] unknown2Bx2C;

	private byte changeCount1;

	private byte changeCount2;

	private byte unknown2F;

	private int tableNamePtrPtr;

	private int fldInfoPtr;

	private byte writeProtected;

	private byte fileVersionID;

	private ushort maxBlocks;

	private byte unknown3C;

	private byte auxPasswords;

	private byte[] unknown3Ex3F;

	private int cryptInfoStartPtr;

	private int cryptInfoEndPtr;

	private byte unknown48;

	private int autoIncVal;

	private byte[] unknown4Dx4E;

	private byte indexUpdateRequired;

	private byte[] unknown50x54;

	private byte refIntegrity;

	private byte[] unknown56x57;

	private V4Hdr V4Header;

	private int tableNamePtr;

	private int[] fieldNamePtrArray;

	private readonly Stream stream;

	private readonly BinaryReader reader;

	public ushort RecordSize { get; private set; }

	public ParadoxFileType FileType { get; private set; }

	public int RecordCount { get; private set; }

	public short FieldCount { get; private set; }

	internal FieldInfo[] FieldTypes { get; set; }

	public Dictionary<string, int> FieldNameMap { get; private set; }

	public ParadoxFile(string fileName)
		: this(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
	{
	}

	public ParadoxFile(Stream stream)
	{
		this.stream = stream;
		reader = new BinaryReader(stream);
		stream.Position = 0L;
		ReadHeader();
	}

	public virtual void Dispose()
	{
		stream.Dispose();
	}

	internal virtual byte[] ReadBlob(byte[] blobInfo)
	{
		return null;
	}

	public IEnumerable<ParadoxRecord> Enumerate(Predicate<ParadoxRecord> where = null)
	{
		for (int blockId = 0; blockId < fileBlocks; blockId++)
		{
			DataBlock block = GetBlock(blockId);
			for (int recId = 0; recId < block.RecordCount; recId++)
			{
				ParadoxRecord rec = block[recId];
				if (where?.Invoke(rec) ?? true)
				{
					yield return rec;
				}
			}
		}
	}

	private void ReadHeader()
	{
		BinaryReader binaryReader = reader;
		RecordSize = binaryReader.ReadUInt16();
		headerSize = binaryReader.ReadUInt16();
		FileType = (ParadoxFileType)binaryReader.ReadByte();
		maxTableSize = binaryReader.ReadByte();
		RecordCount = binaryReader.ReadInt32();
		nextBlock = binaryReader.ReadUInt16();
		fileBlocks = binaryReader.ReadUInt16();
		firstBlock = binaryReader.ReadUInt16();
		lastBlock = binaryReader.ReadUInt16();
		unknown12x13 = binaryReader.ReadUInt16();
		modifiedFlags1 = binaryReader.ReadByte();
		indexFieldNumber = binaryReader.ReadByte();
		primaryIndexWorkspace = binaryReader.ReadInt32();
		unknownPtr1A = binaryReader.ReadInt32();
		pxRootBlockId = binaryReader.ReadUInt16();
		pxLevelCount = binaryReader.ReadByte();
		FieldCount = binaryReader.ReadInt16();
		primaryKeyFields = binaryReader.ReadInt16();
		encryption1 = binaryReader.ReadInt32();
		sortOrder = binaryReader.ReadByte();
		modifiedFlags2 = binaryReader.ReadByte();
		unknown2Bx2C = binaryReader.ReadBytes(2);
		changeCount1 = binaryReader.ReadByte();
		changeCount2 = binaryReader.ReadByte();
		unknown2F = binaryReader.ReadByte();
		tableNamePtrPtr = binaryReader.ReadInt32();
		fldInfoPtr = binaryReader.ReadInt32();
		writeProtected = binaryReader.ReadByte();
		fileVersionID = binaryReader.ReadByte();
		maxBlocks = binaryReader.ReadUInt16();
		unknown3C = binaryReader.ReadByte();
		auxPasswords = binaryReader.ReadByte();
		unknown3Ex3F = binaryReader.ReadBytes(2);
		cryptInfoStartPtr = binaryReader.ReadInt32();
		cryptInfoEndPtr = binaryReader.ReadInt32();
		unknown48 = binaryReader.ReadByte();
		autoIncVal = binaryReader.ReadInt32();
		unknown4Dx4E = binaryReader.ReadBytes(2);
		indexUpdateRequired = binaryReader.ReadByte();
		unknown50x54 = binaryReader.ReadBytes(5);
		refIntegrity = binaryReader.ReadByte();
		unknown56x57 = binaryReader.ReadBytes(2);
		if ((FileType == ParadoxFileType.DbFileIndexed || FileType == ParadoxFileType.DbFileNotIndexed || FileType == ParadoxFileType.XnnFileInc || FileType == ParadoxFileType.XnnFileNonInc) && fileVersionID >= 5)
		{
			V4Header = new V4Hdr(binaryReader);
		}
		List<FieldInfo> list = new List<FieldInfo>();
		for (int i = 0; i < FieldCount; i++)
		{
			list.Add(new FieldInfo(binaryReader));
		}
		if (FileType == ParadoxFileType.PxFile)
		{
			FieldCount += 3;
			list.Add(new FieldInfo(ParadoxFieldTypes.Short, 2));
			list.Add(new FieldInfo(ParadoxFieldTypes.Short, 2));
			list.Add(new FieldInfo(ParadoxFieldTypes.Short, 2));
		}
		FieldTypes = list.ToArray();
		tableNamePtr = binaryReader.ReadInt32();
		if (FileType == ParadoxFileType.DbFileIndexed || FileType == ParadoxFileType.DbFileNotIndexed)
		{
			fieldNamePtrArray = new int[FieldCount];
			for (int j = 0; j < FieldCount; j++)
			{
				fieldNamePtrArray[j] = binaryReader.ReadInt32();
			}
		}
		byte[] array = binaryReader.ReadBytes((fileVersionID >= 12) ? 261 : 79);
		TableName = Encoding.GetEncoding("gb2312").GetString(array, 0, Array.FindIndex(array, (byte b) => b == 0));
		if (FileType != 0 && FileType != ParadoxFileType.DbFileNotIndexed)
		{
			return;
		}
		FieldNameMap = new Dictionary<string, int>(FieldCount);
		for (int k = 0; k < FieldCount; k++)
		{
			StringBuilder stringBuilder = new StringBuilder();
			char value;
			while ((value = binaryReader.ReadChar()) != 0)
			{
				stringBuilder.Append(value);
			}
			FieldNameMap.Add(stringBuilder.ToString(), k);
		}
	}

	internal DataBlock GetBlock(int blockId)
	{
		stream.Position = blockId * maxTableSize * 1024 + headerSize;
		return new DataBlock(this, reader);
	}

	public string GetString(byte[] data, int from, int maxLength)
	{
		int num = Array.FindIndex(data, from, (byte b) => b == 0);
		int num2 = num - from;
		if (num == -1 || num2 > maxLength)
		{
			num2 = maxLength;
		}
		return Encoding.GetEncoding("gb2312").GetString(data, from, num2);
	}

	public string GetStringFromMemo(byte[] data, int from, int size)
	{
		int num = size - 10;
		byte[] array = new byte[num];
		byte[] array2 = new byte[10];
		Array.Copy(data, from, array, 0, num);
		Array.Copy(data, from + num, array2, 0, 10);
		int maxLength = BitConverter.ToInt32(array2, 4);
		return GetString(array, 0, maxLength);
	}
}
