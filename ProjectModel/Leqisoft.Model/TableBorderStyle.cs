﻿namespace Leqisoft.Model;

public class TableBorderStyle
{
	// ===== 原有属性（向后兼容，预设样式使用） =====
	public LineStyle UpDownLine { get; set; }
	public LineStyle LeftRightLine { get; set; }
	public LineStyle BodyLine { get; set; }
	public LineStyle SecondLine { get; set; }
	public int InternalNumber { get; set; }

	// ===== 新增：细粒度边框配置（自定义样式使用） =====

	/// <summary>表头边框上部</summary>
	public BorderEdgeStyle HeaderTop { get; set; }

	/// <summary>表头边框下部（标题与正文分隔线）</summary>
	public BorderEdgeStyle HeaderBottom { get; set; }

	/// <summary>表头边框左侧</summary>
	public BorderEdgeStyle HeaderLeft { get; set; }

	/// <summary>表头边框右侧</summary>
	public BorderEdgeStyle HeaderRight { get; set; }

	/// <summary>表格顶部边框（无表头时使用）</summary>
	public BorderEdgeStyle TableTop { get; set; }

	/// <summary>表格底部边框</summary>
	public BorderEdgeStyle TableBottom { get; set; }

	/// <summary>表格左侧边框</summary>
	public BorderEdgeStyle TableLeft { get; set; }

	/// <summary>表格右侧边框</summary>
	public BorderEdgeStyle TableRight { get; set; }

	/// <summary>内部横线</summary>
	public BorderEdgeStyle InnerHorizontal { get; set; }

	/// <summary>内部竖线</summary>
	public BorderEdgeStyle InnerVertical { get; set; }

	/// <summary>关键词行是否加粗+下划线</summary>
	public bool KeywordRowBoldUnderline { get; set; }

	/// <summary>
	/// 自定义关键词列表（用逗号分隔），用于识别关键词行。
	/// 为空时使用默认关键词：合计,小计,总计,关键词
	/// </summary>
	public string KeywordList { get; set; } = "合计,小计,总计,关键词";

	/// <summary>是否为自定义样式（true 时使用细粒度配置，false 时使用原有预设属性）</summary>
	public bool IsCustomStyle { get; set; }

	public TableBorderStyle()
	{
		// 初始化所有 BorderEdgeStyle 为默认值
		HeaderTop = new BorderEdgeStyle(LineStyle.Thin, 0.5f);
		HeaderBottom = new BorderEdgeStyle(LineStyle.Thin, 0.5f);
		HeaderLeft = new BorderEdgeStyle(LineStyle.None, 0.5f);
		HeaderRight = new BorderEdgeStyle(LineStyle.None, 0.5f);
		TableTop = new BorderEdgeStyle(LineStyle.Thin, 0.5f);
		TableBottom = new BorderEdgeStyle(LineStyle.Thin, 0.5f);
		TableLeft = new BorderEdgeStyle(LineStyle.None, 0.5f);
		TableRight = new BorderEdgeStyle(LineStyle.None, 0.5f);
		InnerHorizontal = new BorderEdgeStyle(LineStyle.Thin, 0.5f);
		InnerVertical = new BorderEdgeStyle(LineStyle.None, 0.5f);
	}

	/// <summary>
	/// 将自定义样式序列化为 JSON（用于持久化）
	/// </summary>
	public string ToJson()
	{
		var sb = new System.Text.StringBuilder();
		sb.Append("{");
		sb.Append("\"IsCustomStyle\":true");
		sb.Append(",\"KeywordRowBoldUnderline\":").Append(KeywordRowBoldUnderline ? "true" : "false");
		sb.Append(",\"KeywordList\":").Append(KeywordListToJson(KeywordList));
		sb.Append(",\"HeaderTop\":").Append(EdgeToJson(HeaderTop));
		sb.Append(",\"HeaderBottom\":").Append(EdgeToJson(HeaderBottom));
		sb.Append(",\"HeaderLeft\":").Append(EdgeToJson(HeaderLeft));
		sb.Append(",\"HeaderRight\":").Append(EdgeToJson(HeaderRight));
		sb.Append(",\"TableTop\":").Append(EdgeToJson(TableTop));
		sb.Append(",\"TableBottom\":").Append(EdgeToJson(TableBottom));
		sb.Append(",\"TableLeft\":").Append(EdgeToJson(TableLeft));
		sb.Append(",\"TableRight\":").Append(EdgeToJson(TableRight));
		sb.Append(",\"InnerHorizontal\":").Append(EdgeToJson(InnerHorizontal));
		sb.Append(",\"InnerVertical\":").Append(EdgeToJson(InnerVertical));
		sb.Append("}");
		return sb.ToString();
	}

	private static string EdgeToJson(BorderEdgeStyle edge)
	{
		if (edge == null) return "null";
		return "{\"LineType\":" + (int)edge.LineType + ",\"Weight\":" + edge.Weight.ToString(System.Globalization.CultureInfo.InvariantCulture) + "}";
	}

	/// <summary>
	/// 将关键词列表字符串序列化为 JSON 字符串值
	/// </summary>
	private static string KeywordListToJson(string keywordList)
	{
		if (string.IsNullOrEmpty(keywordList))
			return "\"\"";
		// 转义 JSON 字符串中的特殊字符
		var escaped = keywordList.Replace("\\", "\\\\").Replace("\"", "\\\"");
		return "\"" + escaped + "\"";
	}

	/// <summary>
	/// 从 JSON 反序列化自定义样式
	/// </summary>
	public static TableBorderStyle FromJson(string json)
	{
		if (string.IsNullOrEmpty(json))
			return null;

		var style = new TableBorderStyle { IsCustomStyle = true };
		style.HeaderTop = ParseEdge(json, "HeaderTop") ?? style.HeaderTop;
		style.HeaderBottom = ParseEdge(json, "HeaderBottom") ?? style.HeaderBottom;
		style.HeaderLeft = ParseEdge(json, "HeaderLeft") ?? style.HeaderLeft;
		style.HeaderRight = ParseEdge(json, "HeaderRight") ?? style.HeaderRight;
		style.TableTop = ParseEdge(json, "TableTop") ?? style.TableTop;
		style.TableBottom = ParseEdge(json, "TableBottom") ?? style.TableBottom;
		style.TableLeft = ParseEdge(json, "TableLeft") ?? style.TableLeft;
		style.TableRight = ParseEdge(json, "TableRight") ?? style.TableRight;
		style.InnerHorizontal = ParseEdge(json, "InnerHorizontal") ?? style.InnerHorizontal;
		style.InnerVertical = ParseEdge(json, "InnerVertical") ?? style.InnerVertical;

		// 解析 KeywordRowBoldUnderline
		int kuIdx = json.IndexOf("\"KeywordRowBoldUnderline\":");
		if (kuIdx >= 0)
		{
			int valStart = kuIdx + "\"KeywordRowBoldUnderline\":".Length;
			style.KeywordRowBoldUnderline = json.Substring(valStart).StartsWith("true");
		}

		// 解析 KeywordList
		style.KeywordList = ParseKeywordList(json);

		return style;
	}

	/// <summary>
	/// 从 JSON 中解析 KeywordList 字符串值
	/// </summary>
	private static string ParseKeywordList(string json)
	{
		string pattern = "\"KeywordList\":";
		int idx = json.IndexOf(pattern);
		if (idx < 0) return "合计,小计,总计,关键词";

		int valStart = idx + pattern.Length;
		if (valStart >= json.Length) return "合计,小计,总计,关键词";

		// 跳过空白
		while (valStart < json.Length && char.IsWhiteSpace(json[valStart]))
			valStart++;

		if (valStart >= json.Length || json[valStart] != '"')
			return "合计,小计,总计,关键词";

		valStart++; // 跳过开头的 "
		var sb = new System.Text.StringBuilder();
		int i = valStart;
		while (i < json.Length)
		{
			char c = json[i];
			if (c == '\\' && i + 1 < json.Length)
			{
				// 转义字符
				char next = json[i + 1];
				if (next == '"') { sb.Append('"'); i += 2; continue; }
				if (next == '\\') { sb.Append('\\'); i += 2; continue; }
				sb.Append(c); i++; continue;
			}
			if (c == '"')
				break; // 字符串结束
			sb.Append(c);
			i++;
		}
		return sb.ToString();
	}

	private static BorderEdgeStyle ParseEdge(string json, string propName)
	{
		string pattern = "\"" + propName + "\":";
		int idx = json.IndexOf(pattern);
		if (idx < 0) return null;
		int objStart = json.IndexOf("{", idx);
		if (objStart < 0) return null;
		int objEnd = json.IndexOf("}", objStart);
		if (objEnd < 0) return null;

		string objStr = json.Substring(objStart, objEnd - objStart + 1);
		// 解析 LineType
		int ltIdx = objStr.IndexOf("\"LineType\":");
		int wIdx = objStr.IndexOf("\"Weight\":");
		if (ltIdx < 0 || wIdx < 0) return null;

		int ltValStart = ltIdx + "\"LineType\":".Length;
		int ltValEnd = objStr.IndexOf(",", ltValStart);
		if (ltValEnd < 0) ltValEnd = objStr.IndexOf("}", ltValStart);
		string ltStr = objStr.Substring(ltValStart, ltValEnd - ltValStart).Trim();

		int wValStart = wIdx + "\"Weight\":".Length;
		int wValEnd = objStr.IndexOf("}", wValStart);
		string wStr = objStr.Substring(wValStart, wValEnd - wValStart).Trim();

		if (int.TryParse(ltStr, out int lt) && float.TryParse(wStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float w))
		{
			return new BorderEdgeStyle((LineStyle)lt, w);
		}
		return null;
	}

	public TableBorderStyle Clone()
	{
		var clone = new TableBorderStyle
		{
			UpDownLine = UpDownLine,
			LeftRightLine = LeftRightLine,
			BodyLine = BodyLine,
			SecondLine = SecondLine,
			InternalNumber = InternalNumber,
			IsCustomStyle = IsCustomStyle,
		KeywordRowBoldUnderline = KeywordRowBoldUnderline,
		KeywordList = KeywordList,
			HeaderTop = HeaderTop?.Clone(),
			HeaderBottom = HeaderBottom?.Clone(),
			HeaderLeft = HeaderLeft?.Clone(),
			HeaderRight = HeaderRight?.Clone(),
			TableTop = TableTop?.Clone(),
			TableBottom = TableBottom?.Clone(),
			TableLeft = TableLeft?.Clone(),
			TableRight = TableRight?.Clone(),
			InnerHorizontal = InnerHorizontal?.Clone(),
			InnerVertical = InnerVertical?.Clone()
		};
		return clone;
	}
}
