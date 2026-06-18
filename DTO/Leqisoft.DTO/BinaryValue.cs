using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Leqisoft.DTO;

[JsonConverter(typeof(BinaryValueJsonConverter))]
public struct BinaryValue
{
	private enum ValueType : byte
	{
		String,
		Number,
		Boolean,
		Date,
		Time,
		DateYearMonth
	}

	private readonly ValueType _type;

	private readonly byte[] _value;

	private byte[] _additionalData;

	public byte[] AdditionalData => _additionalData;

	public object Value => _type switch
	{
		ValueType.String => Encoding.UTF8.GetString(_value), 
		ValueType.Number => BitConverter.ToDouble(_value, 0), 
		ValueType.Boolean => BitConverter.ToBoolean(_value, 0), 
		ValueType.Date => DateTime.FromBinary(BitConverter.ToInt64(_value, 0)), 
		ValueType.Time => new TimeSpan(BitConverter.ToInt64(_value, 0)), 
		ValueType.DateYearMonth => new DateYearMonth(DateTime.FromBinary(BitConverter.ToInt64(_value, 0))), 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	public BinaryValue(string s)
	{
		_additionalData = null;
		_type = ValueType.String;
		_value = Encoding.UTF8.GetBytes(s);
	}

	public BinaryValue(double d)
	{
		_additionalData = null;
		_type = ValueType.Number;
		_value = BitConverter.GetBytes(d);
	}

	public BinaryValue(bool b)
	{
		_additionalData = null;
		_type = ValueType.Boolean;
		_value = BitConverter.GetBytes(b);
	}

	public BinaryValue(DateTime dt)
	{
		_additionalData = null;
		_type = ValueType.Date;
		_value = BitConverter.GetBytes(dt.ToBinary());
	}

	public BinaryValue(TimeSpan time)
	{
		_additionalData = null;
		_type = ValueType.Time;
		_value = BitConverter.GetBytes(time.Ticks);
	}

	public BinaryValue(DateYearMonth ym)
	{
		_additionalData = null;
		_type = ValueType.DateYearMonth;
		_value = BitConverter.GetBytes(ym.Date.Ticks);
	}

	public BinaryValue(byte[] blob)
	{
		_additionalData = null;
		byte type = blob[0];
		if (GetAdditionalDataExistFlag(type) == 0)
		{
			_type = (ValueType)type;
			_value = blob.Skip(1).ToArray();
			return;
		}
		_type = (ValueType)SetAdditionalDataExistFlag(type, isHasAdditionalData: false);
		byte[] array = blob.ToArray();
		int num = BitConverter.ToInt32(blob, 1);
		int num2 = array.Length - 5 - num;
		_value = new byte[num];
		_additionalData = new byte[num2];
		Array.Copy(array, 5, _value, 0, num);
		Array.Copy(array, 5 + num, _additionalData, 0, num2);
	}

	public static byte SetAdditionalDataExistFlag(byte type, bool isHasAdditionalData)
	{
		byte b = type;
		if (isHasAdditionalData)
		{
			return (byte)(b | 0x80u);
		}
		return (byte)(b & 0xFFFFFF7Fu);
	}

	public static byte GetAdditionalDataExistFlag(byte type)
	{
		if (((type >> 7) & 1) == 0)
		{
			return 0;
		}
		return 1;
	}

	public void SetAdditionalData(byte[] data)
	{
		_additionalData = data;
	}

	public static BinaryValue FromObject(object o)
	{
		if (!(o is string s))
		{
			if (!(o is int num))
			{
				if (!(o is double d))
				{
					if (!(o is bool b))
					{
						if (!(o is DateTime dt))
						{
							if (!(o is byte[] blob))
							{
								if (!(o is TimeSpan time))
								{
									if (o is DateYearMonth ym)
									{
										return new BinaryValue(ym);
									}
									throw new ArgumentOutOfRangeException();
								}
								return new BinaryValue(time);
							}
							return new BinaryValue(blob);
						}
						return new BinaryValue(dt);
					}
					return new BinaryValue(b);
				}
				return new BinaryValue(d);
			}
			return new BinaryValue(num);
		}
		return new BinaryValue(s);
	}

	public byte[] GetBytes()
	{
		if (_additionalData == null)
		{
			return new byte[1] { (byte)_type }.Concat(_value).ToArray();
		}
		int value = ((_value != null) ? _value.Length : 0);
		byte b = SetAdditionalDataExistFlag((byte)_type, isHasAdditionalData: true);
		return new byte[1] { b }.Concat(BitConverter.GetBytes(value)).Concat(_value).Concat(_additionalData)
			.ToArray();
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
