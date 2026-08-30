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
					// 修复：原实现对共享 block.data 原地 XOR+反转，同一 DataBlock 被重复读取或
					// 并发读取时得到已二次反转的错误数据。改为复制到局部缓冲后再转换。
					int available = Math.Min(num, block.data.Length - (int)memoryStream.Position);
					byte[] fieldBuf = new byte[num];
					Array.Copy(block.data, (int)memoryStream.Position, fieldBuf, 0, Math.Max(0, available));
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
						ConvertBytes(fieldBuf, 0, num);
						obj = BitConverter.ToInt16(fieldBuf, 0);
						memoryStream.Position += num;
						break;
					case ParadoxFieldTypes.Long:
					case ParadoxFieldTypes.AutoInc:
						ConvertBytes(fieldBuf, 0, num);
						obj = BitConverter.ToInt32(fieldBuf, 0);
						memoryStream.Position += num;
						break;
					case ParadoxFieldTypes.Currency:
						ConvertBytes(fieldBuf, 0, num);
						obj = BitConverter.ToDouble(fieldBuf, 0);
						memoryStream.Position += num;
						break;
					case ParadoxFieldTypes.Number:
					{
						ConvertBytesNum(fieldBuf, 0, num);
						double num3 = BitConverter.ToDouble(fieldBuf, 0);
						obj = (double.IsNaN(num3) ? DBNull.Value : ((object)num3));
						memoryStream.Position += num;
						break;
					}
					case ParadoxFieldTypes.Date:
					{
						ConvertBytes(fieldBuf, 0, num);
						int num2 = BitConverter.ToInt32(fieldBuf, 0);
						// 修复：Date 值 0（Paradox 空日期）时 new DateTime(1,1,1).AddDays(-1) 抛 ArgumentOutOfRangeException。
						obj = ((num2 <= 1) ? ((object)DBNull.Value) : ((object)new DateTime(1, 1, 1).AddDays(num2 - 1)));
						memoryStream.Position += num;
						break;
					}
					case ParadoxFieldTypes.Timestamp:
					{
						ConvertBytes(fieldBuf, 0, num);
						double value = BitConverter.ToDouble(fieldBuf, 0);
						// 修复：Timestamp 值 0（空时间戳）同样 AddMilliseconds(0).AddDays(-1) 越界。
						obj = ((value <= 0.0) ? ((object)DBNull.Value) : ((object)new DateTime(1, 1, 1).AddMilliseconds(value).AddDays(-1.0)));
						memoryStream.Position += num;
						break;
					}
					case ParadoxFieldTypes.Time:
						ConvertBytes(fieldBuf, 0, num);
						obj = TimeSpan.FromMilliseconds(BitConverter.ToInt32(fieldBuf, 0));
						memoryStream.Position += num;
						break;
					case ParadoxFieldTypes.Logical:
						obj = block.data[(int)memoryStream.Position] - 128 > 0;
						memoryStream.Position += num;
						break;
					case ParadoxFieldTypes.BLOb:
					{
						byte[] array3 = new byte[num];
						Array.Copy(block.data, (int)memoryStream.Position, array3, 0, Math.Min(num, block.data.Length - (int)memoryStream.Position));
						obj = block.file.ReadBlob(array3);
						memoryStream.Position += num;
						break;
					}
					case ParadoxFieldTypes.BCD:
					{
						byte[] array = fieldBuf;
						switch (array[0])
						{
						case 194:
							array[0] = 0;
							obj = double.Parse(BitConverter.ToString(array).Replace("-", ""), System.Globalization.CultureInfo.InvariantCulture) / 100.0;
							break;
						case 66:
						{
							byte[] array2 = new byte[array.Length];
							for (int k = 0; k < array.Length; k++)
							{
								array2[k] = (byte)(~array[k]);
							}
							array2[0] = 0;
							obj = double.Parse(BitConverter.ToString(array2).Replace("-", "").TrimStart('0'), System.Globalization.CultureInfo.InvariantCulture) / -100.0;
							break;
						}
						default:
							throw new ArgumentOutOfRangeException("异常数据");
						}
						memoryStream.Position += num;
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

	private void ConvertBytes(byte[] buf, int start, int length)
	{
		buf[start] = (byte)(buf[start] ^ 0x80u);
		Array.Reverse(buf, start, length);
	}

	private void ConvertBytesNum(byte[] buf, int start, int length)
	{
		if ((buf[start] & 0x80u) != 0)
		{
			buf[start] = (byte)(buf[start] & 0x7Fu);
		}
		else if (buf[start] != 0 || buf[start + 1] != 0 || buf[start + 2] != 0 || buf[start + 3] != 0 || buf[start + 4] != 0 || buf[start + 5] != 0 || buf[start + 6] != 0 || buf[start + 7] != 0)
		{
			for (int i = 0; i < 8; i++)
			{
				buf[start + i] = (byte)(~buf[start + i]);
			}
		}
		Array.Reverse(buf, start, length);
	}
}
