using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.International.Converters.PinYinConverter;

namespace Auditai.UI.Controls;

public static class FuzzySearch
{
	private static HashSet<char> unChineseCharCache = new HashSet<char>();

	private static Dictionary<char, ChineseChar> chineseCharCache = new Dictionary<char, ChineseChar>();

	private static Dictionary<string, Tuple<string, List<int>>> inputAllCharCache = new Dictionary<string, Tuple<string, List<int>>>();

	private static Dictionary<string, Tuple<string, List<int>>> inputSimpleCharCache = new Dictionary<string, Tuple<string, List<int>>>();

	public static int Filter(string target, string keyword)
	{
		if (string.IsNullOrWhiteSpace(target))
		{
			return 0;
		}
		if (string.IsNullOrWhiteSpace(keyword))
		{
			return 1;
		}
		try
		{
			keyword = keyword.Trim();
			target = target.ToLower();
			keyword = keyword.ToLower();
			if (Match0(target, keyword))
			{
				return 1;
			}
			if (Match1(target, keyword))
			{
				return 2;
			}
			target = target.Replace("(", "").Replace(")", "").Replace("（", "")
				.Replace("）", "")
				.Replace("-", "")
				.Replace("[", "")
				.Replace("]", "")
				.Replace("*", "")
				.Replace("\\", "")
				.Replace("|", "");
			if (Match2(target, keyword))
			{
				return 3;
			}
			if (Match5(target, keyword))
			{
				return 6;
			}
			if (Match6(target, keyword))
			{
				return 7;
			}
			return 0;
		}
		catch (Exception)
		{
			return 0;
		}
	}

	private static bool Match0(string target, string keyword)
	{
		return target.Equals(keyword);
	}

	private static bool Match1(string target, string keyword)
	{
		return target.Contains(keyword);
	}

	private static bool Match2(string target, string keyword)
	{
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			char[] array = keyword.ToCharArray();
			char[] array2 = array;
			foreach (char c in array2)
			{
				switch (c)
				{
				case '#':
				case '$':
				case '(':
				case ')':
				case '*':
				case '+':
				case '.':
				case '[':
				case '\\':
				case ']':
				case '^':
				case '{':
				case '|':
				case '}':
					stringBuilder.Append($"\\{c}.*");
					break;
				default:
					stringBuilder.Append($"{c}.*");
					break;
				}
			}
			string pattern = stringBuilder.ToString();
			return Regex.IsMatch(target, pattern);
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static bool Match3(string target, string keyword)
	{
		target = SimpleChar(target);
		if (keyword.Length < 2)
		{
			return target.StartsWith(keyword);
		}
		return target.Contains(keyword);
	}

	private static bool Match4(string target, string keyword)
	{
		target = AllChar(target);
		if (keyword.Length < 3)
		{
			return target.StartsWith(keyword);
		}
		return target.Contains(keyword);
	}

	private static bool Match5(string target, string keyword)
	{
		target = SimpleCharPattern(target, out var splits);
		if (keyword.Length > splits.Count)
		{
			return false;
		}
		List<string> list = SubPatterns(target, splits, keyword.Length);
		if (keyword.Length < 2)
		{
			return Regex.IsMatch(keyword, "^" + list[0]);
		}
		foreach (string item in list)
		{
			if (Regex.IsMatch(keyword, item))
			{
				return true;
			}
		}
		return false;
	}

	private static bool Match6(string target, string keyword)
	{
		target = AllCharPattern(target, out var splits);
		if (keyword.Length > splits.Count)
		{
			return false;
		}
		List<string> list = SubPatterns(target, splits, keyword.Length);
		if (keyword.Length < 3)
		{
			return Regex.IsMatch(keyword, "^" + list[0]);
		}
		foreach (string item in list)
		{
			if (Regex.IsMatch(keyword, item))
			{
				return true;
			}
		}
		return false;
	}

	public static string AllChar(string input)
	{
		string text = string.Empty;
		string text2 = input.Trim();
		for (int i = 0; i < text2.Length; i++)
		{
			char ch = text2[i];
			if (ChineseChar.IsValidChar(ch))
			{
				ChineseChar chineseChar = new ChineseChar(ch);
				string text3 = chineseChar.Pinyins[0];
				text += text3.Substring(0, text3.Length - 1).ToLower();
			}
			else
			{
				text += ch;
			}
		}
		return text;
	}

	public static string SimpleChar(string input)
	{
		string text = string.Empty;
		string text2 = input.Trim();
		for (int i = 0; i < text2.Length; i++)
		{
			char ch = text2[i];
			if (ChineseChar.IsValidChar(ch))
			{
				ChineseChar chineseChar = new ChineseChar(ch);
				string text3 = chineseChar.Pinyins[0];
				text += text3.Substring(0, 1).ToLower();
			}
			else
			{
				text += ch;
			}
		}
		return text;
	}

	public static string AllCharPattern(string input, out List<int> splits)
	{
		if (inputAllCharCache.ContainsKey(input))
		{
			Tuple<string, List<int>> tuple = inputAllCharCache[input];
			splits = tuple.Item2;
			return tuple.Item1;
		}
		splits = new List<int>();
		string text = string.Empty;
		string text2 = input.Trim();
		for (int i = 0; i < text2.Length; i++)
		{
			char c = text2[i];
			if (chineseCharCache.ContainsKey(c))
			{
				ChineseChar chineseChar = chineseCharCache[c];
				if (chineseChar.Pinyins.Count == 1)
				{
					string text3 = chineseChar.Pinyins[0];
					text += text3.Substring(0, text3.Length - 1).ToLower();
					splits.Add(text3.Length - 1);
					continue;
				}
				List<string> list = (from p in chineseChar.Pinyins
					where p != null
					select p.Substring(0, p.Length - 1).ToLower()).Distinct().ToList();
				if (list.Count == 1)
				{
					text += list[0];
					splits.Add(list[0].Length);
				}
				else
				{
					string text4 = "(" + string.Join("|", list) + ")";
					text += text4;
					splits.Add(text4.Length);
				}
			}
			else if (unChineseCharCache.Contains(c))
			{
				text += c;
				splits.Add(1);
			}
			else if (ChineseChar.IsValidChar(c))
			{
				ChineseChar chineseChar2 = new ChineseChar(c);
				if (chineseChar2.Pinyins.Count == 1)
				{
					string text5 = chineseChar2.Pinyins[0];
					text += text5.Substring(0, text5.Length - 1).ToLower();
					splits.Add(text5.Length - 1);
				}
				else
				{
					List<string> list2 = (from p in chineseChar2.Pinyins
						where p != null
						select p.Substring(0, p.Length - 1).ToLower()).Distinct().ToList();
					if (list2.Count == 1)
					{
						text += list2[0];
						splits.Add(list2[0].Length);
					}
					else
					{
						string text6 = "(" + string.Join("|", list2) + ")";
						text += text6;
						splits.Add(text6.Length);
					}
				}
				if (!chineseCharCache.ContainsKey(c))
				{
					chineseCharCache.Add(c, chineseChar2);
				}
			}
			else
			{
				text += c;
				splits.Add(1);
				unChineseCharCache.Add(c);
			}
		}
		if (!inputAllCharCache.ContainsKey(input))
		{
			inputAllCharCache.Add(input, Tuple.Create(text, splits));
		}
		return text;
	}

	private static string SimpleCharPattern(string input, out List<int> splits)
	{
		if (inputSimpleCharCache.ContainsKey(input))
		{
			Tuple<string, List<int>> tuple = inputSimpleCharCache[input];
			splits = tuple.Item2;
			return tuple.Item1;
		}
		splits = new List<int>();
		string text = string.Empty;
		string text2 = input.Trim();
		for (int i = 0; i < text2.Length; i++)
		{
			char c = text2[i];
			if (chineseCharCache.ContainsKey(c))
			{
				ChineseChar chineseChar = chineseCharCache[c];
				if (chineseChar.Pinyins.Count == 1)
				{
					string text3 = chineseChar.Pinyins[0];
					text += text3.Substring(0, 1).ToLower();
					splits.Add(1);
					continue;
				}
				List<string> list = (from p in chineseChar.Pinyins
					where p != null
					select p.Substring(0, 1).ToLower()).Distinct().ToList();
				if (list.Count == 1)
				{
					text += list[0];
					splits.Add(list[0].Length);
				}
				else
				{
					string text4 = "(" + string.Join("|", list) + ")";
					text += text4;
					splits.Add(text4.Length);
				}
			}
			else if (unChineseCharCache.Contains(c))
			{
				text += c;
				splits.Add(1);
			}
			else if (ChineseChar.IsValidChar(c))
			{
				ChineseChar chineseChar2 = new ChineseChar(c);
				if (chineseChar2.Pinyins.Count == 1)
				{
					string text5 = chineseChar2.Pinyins[0];
					text += text5.Substring(0, 1).ToLower();
					splits.Add(1);
				}
				else
				{
					List<string> list2 = (from p in chineseChar2.Pinyins
						where p != null
						select p.Substring(0, 1).ToLower()).Distinct().ToList();
					if (list2.Count == 1)
					{
						text += list2[0];
						splits.Add(list2[0].Length);
					}
					else
					{
						string text6 = "(" + string.Join("|", list2) + ")";
						text += text6;
						splits.Add(text6.Length);
					}
				}
				if (!chineseCharCache.ContainsKey(c))
				{
					chineseCharCache.Add(c, chineseChar2);
				}
			}
			else
			{
				text += c;
				splits.Add(1);
				unChineseCharCache.Add(c);
			}
		}
		if (!inputSimpleCharCache.ContainsKey(input))
		{
			inputSimpleCharCache.Add(input, Tuple.Create(text, splits));
		}
		return text;
	}

	private static List<string> SubPatterns(string pattern, List<int> splits, int keyLength)
	{
		List<string> list = new List<string>();
		int num = 0;
		for (int i = 0; i <= splits.Count - keyLength; i++)
		{
			int num2 = 0;
			for (int j = i; j < i + keyLength; j++)
			{
				num2 += splits[j];
			}
			list.Add(pattern.Substring(num, num2));
			num += splits[i];
		}
		return list;
	}
}
