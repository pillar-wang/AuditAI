using System;
using System.Collections.Generic;
using Leqisoft.DTO;

namespace Leqisoft.Model;

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
		return TypeCodeToType[(TypeCode)code.Value];
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
