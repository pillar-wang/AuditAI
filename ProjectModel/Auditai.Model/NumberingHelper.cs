using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Auditai.Model;

public static class NumberingHelper
{
	public class Numbering
	{
		private static int[] PriorityArray = new int[12]
		{
			6, 7, 8, 8, 9, 7, 9, 3, 4, 5,
			1, 2
		};

		public int Series { get; set; }

		public int Number { get; set; }

		public int Priority => PriorityArray[Series];

		public override string ToString()
		{
			return Series switch
			{
				0 => NumToChinese(Number) + "、", 
				1 => "（" + NumToChinese(Number) + "）", 
				2 => $"{Number}、", 
				3 => $"{Number}.", 
				4 => $"（{Number}）", 
				5 => "(" + NumToChinese(Number) + ")", 
				6 => $"({Number})", 
				7 => "第" + NumToChinese(Number) + "章", 
				8 => "第" + NumToChinese(Number) + "节", 
				9 => "第" + NumToChinese(Number) + "条", 
				10 => "第" + NumToChinese(Number) + "部分", 
				11 => "第" + NumToChinese(Number) + "篇", 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		public Numbering GetNext()
		{
			return new Numbering
			{
				Series = Series,
				Number = Number + 1
			};
		}
	}

	private static readonly string[] ChineseChars;

	private static readonly string[] P_array_num;

	private static readonly string[] P_array_digit;

	private static readonly string[] P_array_units;

	private static readonly string[] AllChineseCache;

	private static readonly Regex ChineseSlash;

	private static readonly Regex ChineseFullParen;

	private static readonly Regex ArabicSlash;

	private static readonly Regex ArabicDot;

	private static readonly Regex ArabicFullParen;

	private static readonly Regex ChineseHalfParen;

	private static readonly Regex ArabicHalfParen;

	private static readonly Regex ChineseChapter;

	private static readonly Regex ChineseSection;

	private static readonly Regex ChineseArticle;

	private static readonly Regex ChinesePart;

	private static readonly Regex ChineseBigChapter;

	public static string[] BuiltInBullets;

	private static string[] roman1;

	private static string[] roman2;

	private static string[] roman3;

	private static string[] roman4;

	private static string[] _wingdingsMap;

	public static int NumberOfSeries => 10;

	static NumberingHelper()
	{
		ChineseChars = new string[11]
		{
			"零", "一", "二", "三", "四", "五", "六", "七", "八", "九",
			"十"
		};
		P_array_num = new string[10] { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
		P_array_digit = new string[4] { "", "拾", "佰", "仟" };
		P_array_units = new string[5] { "", "万", "亿", "万亿", "亿亿" };
		ChineseSlash = new Regex("^\\s*([零一二三四五六七八九十百]+)、");
		ChineseFullParen = new Regex("^\\s*（([零一二三四五六七八九十百]+)）");
		ArabicSlash = new Regex("^\\s*(\\d+)、", RegexOptions.ECMAScript);
		ArabicDot = new Regex("^\\s*(\\d+)\\.", RegexOptions.ECMAScript);
		ArabicFullParen = new Regex("\\s*^（(\\d+)）", RegexOptions.ECMAScript);
		ChineseHalfParen = new Regex("^\\s*\\(([零一二三四五六七八九十百]+)\\)");
		ArabicHalfParen = new Regex("^\\s*\\((\\d+)\\)", RegexOptions.ECMAScript);
		ChineseChapter = new Regex("^\\s*第([零一二三四五六七八九十百]+)章");
		ChineseSection = new Regex("^\\s*第([零一二三四五六七八九十百]+)节");
		ChineseArticle = new Regex("^\\s*第([零一二三四五六七八九十百]+)条");
		ChinesePart = new Regex("^\\s*第([零一二三四五六七八九十百]+)部分");
		ChineseBigChapter = new Regex("^\\s*第([零一二三四五六七八九十百]+)篇");
		BuiltInBullets = new string[6] { "•", "*", "-", "￭", "◆", "※" };
		roman1 = new string[3] { "MMM", "MM", "M" };
		roman2 = new string[9] { "CM", "DCCC", "DCC", "DC", "D", "CD", "CCC", "CC", "C" };
		roman3 = new string[9] { "XC", "LXXX", "LXX", "LX", "L", "XL", "XXX", "XX", "X" };
		roman4 = new string[9] { "IX", "VIII", "VII", "VI", "V", "IV", "III", "II", "I" };
		_wingdingsMap = new string[224]
		{
			"\u3000", "\ud83d\udd89", "✂", "✁", "\ud83d\udc53", "\ud83d\udd6d", "\ud83d\udd6e", "\ud83d\udd6f", "\ud83d\udd7f", "✆",
			"\ud83d\udd82", "\ud83d\udd83", "\ud83d\udcea", "\ud83d\udceb", "\ud83d\udcec", "\ud83d\udced", "\ud83d\udcc1", "\ud83d\udcc2", "\ud83d\udcc4", "\ud83d\uddcf",
			"\ud83d\uddd0", "\ud83d\uddc4", "⌛", "\ud83d\uddae", "\ud83d\uddb0", "\ud83d\uddb2", "\ud83d\uddb3", "\ud83d\uddb4", "\ud83d\uddab", "\ud83d\uddac",
			"✇", "✍", "\ud83d\udd8e", "✌", "\ud83d\udc4c", "\ud83d\udc4d", "\ud83d\udc4e", "☜", "☞", "☝",
			"☟", "\ud83d\udd90", "☺", "\ud83d\ude10", "☹", "\ud83d\udca3", "☠", "\ud83c\udff3", "\ud83c\udff1", "✈",
			"☼", "\ud83d\udca7", "❄", "\ud83d\udd46", "✞", "\ud83d\udd48", "✠", "✡", "☪", "☯",
			"ॐ", "☸", "♈", "♉", "♊", "♋", "♌", "♍", "♎", "♏",
			"♐", "♑", "♒", "♓", "\ud83d\ude70", "\ud83d\ude75", "●", "\ud83d\udd3e", "■", "□",
			"\ud83d\udf90", "❑", "❒", "⬧", "⧫", "◆", "❖", "⬥", "⌧", "⮹",
			"⌘", "\ud83c\udff5", "\ud83c\udff6", "\ud83d\ude76", "\ud83d\ude77", "●", "⓪", "①", "②", "③",
			"④", "⑤", "⑥", "⑦", "⑧", "⑨", "⑩", "⓿", "❶", "❷",
			"❸", "❹", "❺", "❻", "❼", "❽", "❾", "❿", "\ud83d\ude62", "\ud83d\ude60",
			"\ud83d\ude61", "\ud83d\ude63", "\ud83d\ude5e", "\ud83d\ude5c", "\ud83d\ude5d", "\ud83d\ude5f", "·", "•", "▪", "⚪",
			"\ud83d\udf86", "\ud83d\udf88", "◉", "◎", "\ud83d\udd3f", "▪", "◻", "\ud83d\udfc2", "✦", "★",
			"✶", "✴", "✹", "✵", "⯐", "⌖", "⟡", "⌑", "⯑", "✪",
			"✰", "\ud83d\udd50", "\ud83d\udd51", "\ud83d\udd52", "\ud83d\udd53", "\ud83d\udd54", "\ud83d\udd55", "\ud83d\udd56", "\ud83d\udd57", "\ud83d\udd58",
			"\ud83d\udd59", "\ud83d\udd5a", "\ud83d\udd5b", "⮰", "⮱", "⮲", "⮳", "⮴", "⮵", "⮶",
			"⮷", "\ud83d\ude6a", "\ud83d\ude6b", "\ud83d\ude55", "\ud83d\ude54", "\ud83d\ude57", "\ud83d\ude56", "\ud83d\ude50", "\ud83d\ude51", "\ud83d\ude52",
			"\ud83d\ude53", "⌫", "⌦", "⮘", "⮚", "⮙", "⮛", "⮈", "⮊", "⮉",
			"⮋", "\ud83e\udc68", "\ud83e\udc6a", "\ud83e\udc69", "\ud83e\udc6b", "\ud83e\udc6c", "\ud83e\udc6d", "\ud83e\udc6f", "\ud83e\udc6e", "\ud83e\udc78",
			"\ud83e\udc7a", "\ud83e\udc79", "\ud83e\udc7b", "\ud83e\udc7c", "\ud83e\udc7d", "\ud83e\udc7f", "\ud83e\udc7e", "⇦", "⇨", "⇧",
			"⇩", "⬄", "⇳", "⬁", "⬀", "⬃", "⬂", "\ud83e\udcac", "\ud83e\udcad", "\ud83d\uddf6",
			"✔", "\ud83d\uddf7", "\ud83d\uddf9", "●"
		};
		AllChineseCache = Enumerable.Range(0, 109).Select(NumToChinese).ToArray();
	}

	public static Numbering Matches(string s)
	{
		if (string.IsNullOrWhiteSpace(s))
		{
			return null;
		}
		Match match = ChineseSlash.Match(s);
		if (match.Success)
		{
			int num = Array.IndexOf(AllChineseCache, match.Groups[1].Value);
			if (num > 0)
			{
				return new Numbering
				{
					Series = 0,
					Number = num
				};
			}
		}
		match = ChineseFullParen.Match(s);
		if (match.Success)
		{
			int num2 = Array.IndexOf(AllChineseCache, match.Groups[1].Value);
			if (num2 > 0)
			{
				return new Numbering
				{
					Series = 1,
					Number = num2
				};
			}
		}
		match = ArabicSlash.Match(s);
		if (match.Success && int.TryParse(match.Groups[1].Value, out var result))
		{
			return new Numbering
			{
				Series = 2,
				Number = result
			};
		}
		match = ArabicDot.Match(s);
		if (match.Success && int.TryParse(match.Groups[1].Value, out var result2))
		{
			return new Numbering
			{
				Series = 3,
				Number = result2
			};
		}
		match = ArabicFullParen.Match(s);
		if (match.Success && int.TryParse(match.Groups[1].Value, out var result3))
		{
			return new Numbering
			{
				Series = 4,
				Number = result3
			};
		}
		match = ChineseHalfParen.Match(s);
		if (match.Success)
		{
			int num3 = Array.IndexOf(AllChineseCache, match.Groups[1].Value);
			if (num3 > 0)
			{
				return new Numbering
				{
					Series = 5,
					Number = num3
				};
			}
		}
		match = ArabicHalfParen.Match(s);
		if (match.Success)
		{
			return new Numbering
			{
				Series = 6,
				Number = int.Parse(match.Groups[1].Value)
			};
		}
		match = ChineseChapter.Match(s);
		if (match.Success)
		{
			int num4 = Array.IndexOf(AllChineseCache, match.Groups[1].Value);
			if (num4 > 0)
			{
				return new Numbering
				{
					Series = 7,
					Number = num4
				};
			}
		}
		match = ChineseSection.Match(s);
		if (match.Success)
		{
			int num5 = Array.IndexOf(AllChineseCache, match.Groups[1].Value);
			if (num5 > 0)
			{
				return new Numbering
				{
					Series = 8,
					Number = num5
				};
			}
		}
		match = ChineseArticle.Match(s);
		if (match.Success)
		{
			int num6 = Array.IndexOf(AllChineseCache, match.Groups[1].Value);
			if (num6 > 0)
			{
				return new Numbering
				{
					Series = 9,
					Number = num6
				};
			}
		}
		match = ChinesePart.Match(s);
		if (match.Success)
		{
			int num7 = Array.IndexOf(AllChineseCache, match.Groups[1].Value);
			if (num7 > 0)
			{
				return new Numbering
				{
					Series = 10,
					Number = num7
				};
			}
		}
		match = ChineseBigChapter.Match(s);
		if (match.Success)
		{
			int num8 = Array.IndexOf(AllChineseCache, match.Groups[1].Value);
			if (num8 > 0)
			{
				return new Numbering
				{
					Series = 11,
					Number = num8
				};
			}
		}
		return null;
	}

	public static string NumToChinese(int x)
	{
		if (x < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (x <= 10)
		{
			return ChineseChars[x];
		}
		if (x < 20)
		{
			return ChineseChars[10] + NumToChinese(x % 10);
		}
		if (x < 100)
		{
			int num = x % 10;
			int num2 = x - num;
			if (num == 0)
			{
				return NumToChinese(num2 / 10) + ChineseChars[10];
			}
			return NumToChinese(num2) + NumToChinese(num);
		}
		if (x < 1000)
		{
			int num3 = x % 10;
			int num4 = x % 100 - num3;
			int num5 = x - num3 - num4;
			if (num4 == 0 && num3 == 0)
			{
				return NumToChinese(num5 / 100) + "百";
			}
			if (num4 == 0 && num3 != 0)
			{
				return NumToChinese(num5) + "零" + NumToChinese(num3);
			}
			if (num4 == 10)
			{
				return NumToChinese(num5) + "一" + NumToChinese(num4 + num3);
			}
			return NumToChinese(num5) + NumToChinese(num4 + num3);
		}
		return "中文数字过大";
	}

	public static string NumToRmbBig(double num)
	{
		bool flag = false;
		if (num < 0.0)
		{
			flag = true;
			num = 0.0 - num;
		}
		double d = Math.Max(0.0, num);
		d = Math.Truncate(d);
		int num2 = (int)Math.Round((num - d) * 100.0, 0);
		StringBuilder stringBuilder = new StringBuilder();
		if (flag)
		{
			stringBuilder.Append("负");
		}
		stringBuilder.Append(NumToRmbBigIntegral(d.ToString()));
		if (num2 == 0)
		{
			stringBuilder.Append("元整");
		}
		else if (num2 < 10)
		{
			stringBuilder.Append("元零");
			stringBuilder.Append(P_array_num[num2]);
			stringBuilder.Append("分");
		}
		else if (num2 % 10 == 0)
		{
			stringBuilder.Append("元");
			stringBuilder.Append(P_array_num[num2 / 10]);
			stringBuilder.Append("角");
		}
		else
		{
			stringBuilder.Append("元");
			stringBuilder.Append(P_array_num[num2 / 10]);
			stringBuilder.Append("角");
			stringBuilder.Append(P_array_num[num2 % 10]);
			stringBuilder.Append("分");
		}
		return stringBuilder.ToString();
	}

	public static string NumToRmbBigIntegral(string x)
	{
		if (x == "0")
		{
			return P_array_num[0];
		}
		string text = "";
		int num = 0;
		int num2 = x.Length % 4;
		int num3 = 0;
		num3 = ((num2 <= 0) ? (x.Length / 4) : (x.Length / 4 + 1));
		for (int num4 = num3; num4 > 0; num4--)
		{
			int num5 = 4;
			if (num4 == num3 && num2 != 0)
			{
				num5 = num2;
			}
			string text2 = x.Substring(num, num5);
			int length = text2.Length;
			for (int i = 0; i < length; i++)
			{
				int num6 = Convert.ToInt32(text2.Substring(i, 1));
				if (num6 == 0)
				{
					if (i < length - 1 && Convert.ToInt32(text2.Substring(i + 1, 1)) > 0 && !text.EndsWith(P_array_num[num6]))
					{
						text += P_array_num[num6];
					}
				}
				else
				{
					text += P_array_num[num6];
					text += P_array_digit[length - i - 1];
				}
			}
			num += num5;
			if (num4 < num3)
			{
				if (Convert.ToInt32(text2) != 0)
				{
					text += P_array_units[num4 - 1];
				}
			}
			else
			{
				text += P_array_units[num4 - 1];
			}
		}
		return text;
	}

	public static string NumToUpperRoman(int num)
	{
		int num2 = num / 1000;
		num %= 1000;
		int num3 = num / 100;
		num %= 100;
		int num4 = num / 10;
		int num5 = num % 10;
		StringBuilder stringBuilder = new StringBuilder();
		if (num2 > 0)
		{
			stringBuilder.Append(roman1[3 - num2]);
		}
		if (num3 > 0)
		{
			stringBuilder.Append(roman2[9 - num3]);
		}
		if (num4 > 0)
		{
			stringBuilder.Append(roman3[9 - num4]);
		}
		if (num5 > 0)
		{
			stringBuilder.Append(roman4[9 - num5]);
		}
		return stringBuilder.ToString();
	}

	public static string NumEncircled(int num)
	{
		if (num <= 20)
		{
			return char.ConvertFromUtf32(9311 + num);
		}
		if (num <= 35)
		{
			return char.ConvertFromUtf32(12860 + num);
		}
		if (num <= 50)
		{
			return char.ConvertFromUtf32(12941 + num);
		}
		return num.ToString();
	}

	public static string NumFullstop(int num)
	{
		if (num <= 20)
		{
			return char.ConvertFromUtf32(9351 + num);
		}
		return $"{num}.";
	}

	public static string NumEnclosedParen(int num)
	{
		if (num <= 20)
		{
			return char.ConvertFromUtf32(9331 + num);
		}
		return $"({num})";
	}

	public static string NumEncircledChinese(int num)
	{
		if (num <= 10)
		{
			return char.ConvertFromUtf32(12927 + num);
		}
		return NumEncircled(num);
	}

	public static string NumFullwidth(int num)
	{
		return string.Concat(from c in num.ToString()
			select char.ConvertFromUtf32(c + 65248));
	}

	public static string NumIdeographTraditional(int num)
	{
		return "癸甲乙丙丁戊己庚辛壬".Substring(num % 10, 1);
	}

	public static string NumIdeographZodiac(int num)
	{
		return "亥子丑寅卯辰巳午未申酉戌".Substring(num % 12, 1);
	}

	public static string NumIdeographZodiacTraditional(int num)
	{
		return NumIdeographTraditional(num) + NumIdeographZodiac(num);
	}

	public static string WingdingsToUnicode(string wingdings)
	{
		int num = char.ConvertToUtf32(wingdings, 0) & 0xFF;
		return _wingdingsMap[num - 32];
	}
}
