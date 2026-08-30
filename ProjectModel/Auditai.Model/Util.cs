using System;
using System.Collections.Generic;
using Auditai.DTO;

namespace Auditai.Model;

public static class Util
{
	private const int TYPECODE_TIMESPAN = 19;

	private const int TYPECODE_DATEYEARMONTH = 20;

	private static readonly Dictionary<TypeCode, Type> TypeCodeToType = new Dictionary<TypeCode, Type>
	{
		{
			TypeCode.Double,
			typeof(double)
		},
		{
			TypeCode.String,
			typeof(string)
		},
		{
			TypeCode.Boolean,
			typeof(bool)
		},
		{
			TypeCode.DateTime,
			typeof(DateTime)
		},
		{
			(TypeCode)19,
			typeof(TimeSpan)
		},
		{
			(TypeCode)20,
			typeof(DateYearMonth)
		}
	};

	public static int? DataTypeToNullableInt(Type type)
	{
		if (type == typeof(TimeSpan))
		{
			return 19;
		}
		if (type == typeof(DateYearMonth))
		{
			return 20;
		}
		if (!(type == null))
		{
			return (int)Type.GetTypeCode(type);
		}
		return null;
	}

	public static Type NullableIntToDataType(int? code)
	{
		if (!code.HasValue)
		{
			return null;
		}
		// 修复：字典仅包含 6 个受支持的类型码，而 DataTypeToNullableInt 对任意
		// 非 null 类型都会返回 GetTypeCode 值（如 Int32=9）。坏数据/旧库读到这类
		// 码会抛 KeyNotFoundException，导致整表 Load 被标记损坏或同步中断。
		// 未知类型码回退为 string（无损兜底），避免单点脏数据拖垮整个加载。
		if (TypeCodeToType.TryGetValue((TypeCode)code.Value, out var t))
		{
			return t;
		}
		return typeof(string);
	}

	public static string GetReadableFileSize(int byteSize)
	{
		if (byteSize < 1024)
		{
			return $"{byteSize} B";
		}
		int num = byteSize / 1024;
		if (num < 1024)
		{
			return $"{num} KB";
		}
		num /= 1024;
		if (num < 1024)
		{
			return $"{num} MB";
		}
		num /= 1024;
		return $"{num} GB";
	}
}
