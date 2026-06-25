﻿﻿﻿﻿﻿﻿﻿﻿using System;
using Newtonsoft.Json;

namespace Auditai.DTO;

[JsonConverter(typeof(Id64JsonConverter))]
public struct Id64
{
	private readonly long _value;

	public static Id64 Zero { get; } = new Id64(0L);


	public long Value => _value;

	public Id64(long value)
	{
		_value = value;
	}

	public Id64(int upper, int lower)
	{
		_value = ((long)upper << 32) + lower;
	}

	public bool IsZero()
	{
		return this == Zero;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Id64 id))
		{
			return false;
		}
		return _value == id._value;
	}

	public static bool operator ==(Id64 lhs, Id64 rhs)
	{
		return lhs._value == rhs._value;
	}

	public static bool operator !=(Id64 lhs, Id64 rhs)
	{
		return lhs._value != rhs._value;
	}

	public string ToBase64()
	{
		return Convert.ToBase64String(BitConverter.GetBytes(Value));
	}

	public static Id64 ParseBase64(string base64)
	{
		// 补齐 Base64 填充（书签格式会去掉 = 以缩短长度）
		int missing = (4 - base64.Length % 4) % 4;
		if (missing > 0)
			base64 += new string('=', missing);
		return new Id64(BitConverter.ToInt64(Convert.FromBase64String(base64), 0));
	}

	public static Id64 Parse(string s)
	{
		return new Id64(long.Parse(s));
	}

	public static Id64? FromNullableLong(long? l)
	{
		if (!l.HasValue)
		{
			return null;
		}
		return new Id64(l.Value);
	}

	public override string ToString()
	{
		return Value.ToString();
	}
}
