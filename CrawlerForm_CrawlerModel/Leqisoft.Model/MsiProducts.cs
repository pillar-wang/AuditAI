﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Leqisoft.Model;

public static class MsiProducts
{
	private static List<Guid> products;

	[DllImport("msi", CharSet = CharSet.Unicode)]
	private static extern uint MsiEnumProducts(int iProductIndex, string lpProductBuf);

	[DllImport("msi", CharSet = CharSet.Unicode)]
	private static extern uint MsiGetProductInfo(string szProduct, string szProperty, string lpValueBuf, ref uint pcchValueBuf);

	public static IEnumerable<Guid> GetProducts()
	{
		if (products == null)
		{
			products = new List<Guid>();
			int num = 0;
			string text = new string(' ', 39);
			uint num2 = 0u;
			while ((num2 = MsiEnumProducts(num++, text)) != 259)
			{
				products.Add(new Guid(text.TrimEnd(default(char))));
			}
		}
		return products;
	}

	public static bool IsProductInstalled(Guid product)
	{
		return GetProducts().Contains(product);
	}

	public static string GetInstallLocation(Guid product)
	{
		string text = new string(' ', 256);
		uint pcchValueBuf = (uint)text.Length;
		return text;
	}
}
