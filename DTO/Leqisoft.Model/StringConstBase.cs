using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Leqisoft.Model;

public abstract class StringConstBase
{
	public abstract string AppName { get; }

	public abstract string Auditee { get; }

	public abstract string TableNote { get; }

	public abstract string Project { get; }

	public abstract string Manager { get; }

	public abstract string Assistant { get; }

	public abstract string Template { get; }

	public abstract string SelectTemplate { get; }

	public abstract string NotUseTemplate { get; }

	public static StringConstBase Current { get; set; }

	public static string EnPlaceHolder(string var)
	{
		return "[:" + var + "]";
	}

	public static string DePlaceHolder(string message)
	{
		if (message == null)
		{
			return null;
		}
		if (Current == null)
		{
			return message;
		}
		Type typeFromHandle = typeof(StringConstBase);
		Dictionary<string, string> kvDic = new Dictionary<string, string>();
		PropertyInfo[] properties = typeFromHandle.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			object value = propertyInfo.GetValue(Current, null);
			kvDic.Add(propertyInfo.Name, value?.ToString());
		}
		return Regex.Replace(message, "\\[:(.+?)\\]", delegate(Match match)
		{
			string value2 = match.Groups[1].Value;
			return (!kvDic.ContainsKey(value2)) ? match.Value : kvDic[value2];
		});
	}
}
