﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Text;

namespace Leqisoft.Model;

public class LeqiBookmark
{
	public string ParaIdBase64 { get; set; }

	public LeqiBookmarkStatus Status { get; set; }

	public string TableId { get; set; }

	public string VariableId { get; set; }

	public int? TableStyle { get; set; }

	/// <summary>自定义表格边框样式的 JSON（仅当 TableStyle==6 时使用）</summary>
	public string CustomBorderStyleJson { get; set; }

	/// <summary>
	/// 将书签序列化为字符串，使用 _ 分隔符（OOXML 安全，不会被截断）。
	/// 格式：lsbm_0{ParaId}_1{Status}_2{TableId}_3{VariableId}_4{TableStyle}
	/// 注意：key 后面直接跟 value，不加额外的 _，与 TryParse 的解析逻辑保持一致。
	/// </summary>
	public string GetString()
	{
		var sb = new StringBuilder("lsbm");
		if (VariableId == null)
		{
			if (!string.IsNullOrEmpty(ParaIdBase64))
			{
				sb.Append("_0");
				sb.Append(ParaIdBase64.TrimEnd('='));
			}
			sb.Append("_1");
			sb.Append((int)Status);
		}
		if (TableId != null)
		{
			sb.Append("_2");
			sb.Append(TableId.TrimEnd('='));
		}
		if (VariableId != null)
		{
			sb.Append("_3");
			sb.Append(VariableId.TrimEnd('='));
		}
		if (TableStyle.HasValue)
		{
			sb.Append("_4");
			sb.Append(TableStyle.Value);
		}
		if (TableStyle.HasValue && TableStyle.Value == 6 && !string.IsNullOrEmpty(CustomBorderStyleJson))
		{
			sb.Append("_5");
			sb.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(CustomBorderStyleJson)).TrimEnd('='));
		}
		return sb.ToString();
	}

	/// <summary>
	/// 解析书签字符串，兼容 lsbm_（新格式）和 lsbm@（旧格式）。
	/// Base64 的 = 填充被去掉的情况也会正确处理。
	/// </summary>
	public static bool TryParse(string s, out LeqiBookmark lsbm)
	{
		lsbm = null;
		if (string.IsNullOrEmpty(s) || !s.StartsWith("lsbm"))
			return false;

		lsbm = new LeqiBookmark();
		int pos = 4; // 跳过 "lsbm"

		while (pos < s.Length)
		{
			char sep = s[pos];
			if (sep != '_' && sep != '@')
				return false;
			pos++;

			if (pos >= s.Length)
				return false;
			char key = s[pos];
			pos++;

			switch (key)
			{
				case '0':
					lsbm.ParaIdBase64 = ReadUntilSeparator(s, ref pos);
					break;
				case '1':
				{
					string statusStr = ReadUntilSeparator(s, ref pos);
					if (int.TryParse(statusStr, out var st))
						lsbm.Status = (LeqiBookmarkStatus)st;
					break;
				}
				case '2':
					lsbm.TableId = ReadUntilSeparator(s, ref pos);
					break;
				case '3':
					lsbm.VariableId = ReadUntilSeparator(s, ref pos);
					break;
				case '4':
			{
				string styleStr = ReadUntilSeparator(s, ref pos);
				if (int.TryParse(styleStr, out var ts))
					lsbm.TableStyle = ts;
				break;
			}
			case '5':
			{
				string raw = ReadUntilSeparator(s, ref pos);
				// Base64 解码（补回 = 填充）
				string padded = raw;
				int pad = padded.Length % 4;
				if (pad > 0) padded += new string('=', 4 - pad);
				try
				{
					lsbm.CustomBorderStyleJson = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
				}
				catch
				{
					lsbm.CustomBorderStyleJson = null;
				}
				break;
			}
			default:
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// 从当前位置读取到下一个分隔符（_ 或 @）或字符串结尾。
	/// </summary>
	private static string ReadUntilSeparator(string s, ref int pos)
	{
		int start = pos;
		while (pos < s.Length && s[pos] != '_' && s[pos] != '@')
			pos++;
		return s.Substring(start, pos - start);
	}

	public void TagModifiedIfNotNew()
	{
		if (Status != 0)
		{
			Status = LeqiBookmarkStatus.Modified;
		}
	}
}