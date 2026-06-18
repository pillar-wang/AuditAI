using System;
using System.IO;
using System.Text;

namespace ParadoxReader;

public class ParadoxRecord
{
	internal readonly ParadoxFile.DataBlock block;

	private readonly int recIndex;

	private object[] data;

	public object[] DataValues
	{
		get
		{
			if (data == null)
			{
				MemoryStream memoryStream = new MemoryStream(block.data);
				memoryStream.Position = block.file.RecordSize * recIndex;
				using BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.GetEncoding("gb2312"));
				data = new object[block.file.FieldCount];
				for (int i = 0; i < data.Length; i++)
				{
					ParadoxFile.FieldInfo fieldInfo = block.file.FieldTypes[i];
					int num = ((fieldInfo.fType == ParadoxFieldTypes.BCD) ? 17 : fieldInfo.fSize);
					bool flag = true;
					for (int j = 0; j < num; j++)
					{
						if (block.data[memoryStream.Position + j] != 0)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						data[i] = DBNull.Value;
						memoryStream.Position += num;
						continue;
					}
					object obj;
					switch (fieldInfo.fType)
					{
					case ParadoxFieldTypes.Alpha:
						obj = block.file.GetString(block.data, (int)memoryStream.Position, num);
						memoryStream.Position += num;
						break;
					case ParadoxFieldTypes.MemoBLOb:
						obj = block.file.GetStringFromMemo(block.data, (int)memoryStream.Position, num);
						memoryStream.Position += num;
						break;
					case ParadoxFieldTypes.Short:
						ConvertBytes((int)memoryStream.Position, num);
						obj = binaryReader.ReadInt16();
						break;
					case ParadoxFieldTypes.Long:
					case ParadoxFieldTypes.AutoInc:
						ConvertBytes((int)memoryStream.Position, num);
						obj = binaryReader.ReadInt32();
						break;
					case ParadoxFieldTypes.Currency:
						ConvertBytes((int)memoryStream.Position, num);
						obj = binaryReader.ReadDouble();
						break;
					case ParadoxFieldTypes.Number:
					{
						ConvertBytesNum((int)memoryStream.Position, num);
						double num3 = binaryReader.ReadDouble();
						obj = (double.IsNaN(num3) ? DBNull.Value : ((object)num3));
						break;
					}
					case ParadoxFieldTypes.Date:
					{
						ConvertBytes((int)memoryStream.Position, num);
						int num2 = binaryReader.ReadInt32();
						obj = new DateTime(1, 1, 1).AddDays(num2 - 1);
						break;
					}
					case ParadoxFieldTypes.Timestamp:
					{
						ConvertBytes((int)memoryStream.Position, num);
						double value = binaryReader.ReadDouble();
						obj = new DateTime(1, 1, 1).AddMilliseconds(value).AddDays(-1.0);
						break;
					}
					case ParadoxFieldTypes.Time:
						ConvertBytes((int)memoryStream.Position, num);
						obj = TimeSpan.FromMilliseconds(binaryReader.ReadInt32());
						break;
					case ParadoxFieldTypes.Logical:
						obj = block.data[(int)memoryStream.Position] - 128 > 0;
						memoryStream.Position += num;
						break;
					case ParadoxFieldTypes.BLOb:
					{
						byte[] array3 = new byte[num];
						binaryReader.Read(array3, 0, num);
						obj = block.file.ReadBlob(array3);
						break;
					}
					case ParadoxFieldTypes.BCD:
					{
						byte[] array = binaryReader.ReadBytes(num);
						switch (array[0])
						{
						case 194:
							array[0] = 0;
							obj = double.Parse(BitConverter.ToString(array).Replace("-", "")) / 100.0;
							break;
						case 66:
						{
							byte[] array2 = new byte[array.Length];
							for (int k = 0; k < array.Length; k++)
							{
								array2[k] = (byte)(~array[k]);
							}
							array2[0] = 0;
							obj = double.Parse(BitConverter.ToString(array2).Replace("-", "").TrimStart('0')) / -100.0;
							break;
						}
						default:
							throw new ArgumentOutOfRangeException("异常数据");
						}
						break;
					}
					default:
						obj = null;
						memoryStream.Position += num;
						break;
					}
					data[i] = obj;
				}
			}
			return data;
		}
	}

	internal ParadoxRecord(ParadoxFile.DataBlock block, int recIndex)
	{
		this.block = block;
		this.recIndex = recIndex;
	}

	public object GetValue(string field)
	{
		return DataValues[block.file.FieldNameMap[field]];
	}

	public bool TryGetValue(string field, out object value)
	{
		if (block.file.FieldNameMap.TryGetValue(field, out var value2))
		{
			value = DataValues[value2];
			return true;
		}
		value = null;
		return false;
	}

	private void ConvertBytes(int start, int length)
	{
		block.data[start] = (byte)(block.data[start] ^ 0x80u);
		Array.Reverse(block.data, start, length);
	}

	private void ConvertBytesNum(int start, int length)
	{
		if ((block.data[start] & 0x80u) != 0)
		{
			block.data[start] = (byte)(block.data[start] & 0x7Fu);
		}
		else if (block.data[start] != 0 || block.data[start + 1] != 0 || block.data[start + 2] != 0 || block.data[start + 3] != 0 || block.data[start + 4] != 0 || block.data[start + 5] != 0 || block.data[start + 6] != 0 || block.data[start + 7] != 0)
		{
			for (int i = 0; i < 8; i++)
			{
				block.data[start + i] = (byte)(~block.data[start + i]);
			}
		}
		Array.Reverse(block.data, start, length);
	}
}
