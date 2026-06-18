﻿﻿﻿﻿﻿﻿﻿﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Leqisoft.DTO;

namespace Leqisoft.Model;

public class FormatComplianceChecker
{
	private Project _project;

	internal HashSet<Id64> _removed = new HashSet<Id64>();

	internal HashSet<Id64> _toDelete = new HashSet<Id64>();

	public List<FormatComplianceRule> Rules { get; } = new List<FormatComplianceRule>();


	public FormatComplianceChecker(Project project)
	{
		_project = project;
	}

	public void RemoveOne(FormatComplianceRule rule)
	{
		Rules.Remove(rule);
		_removed.Add(rule.Id);
	}

	internal void Reset()
	{
		Rules.Clear();
		_removed.Clear();
		_toDelete.Clear();
	}

	/// <summary>
	/// 从 Document 的 Paragraphs 中提取每段的纯文本（按段落顺序）。
	/// Paragraph.Stream 是 OOXML 片段，文本节点为 w:t。
	/// </summary>
	public static List<string> ExtractParagraphTexts(Document doc)
	{
		List<string> list = new List<string>();
		if (doc == null)
		{
			return list;
		}
		foreach (Paragraph paragraph in doc.Paragraphs)
		{
			list.Add(ExtractParagraphText(paragraph));
		}
		return list;
	}

	/// <summary>
	/// 从单个 Paragraph 的 Stream（OOXML 片段）中提取纯文本。
	/// </summary>
	public static string ExtractParagraphText(Paragraph paragraph)
	{
		if (paragraph == null || string.IsNullOrEmpty(paragraph.Stream))
		{
			return string.Empty;
		}
		try
		{
			XElement root = XElement.Parse(paragraph.Stream);
			XNamespace w = Document.xmlns_w;
			var texts = root.Descendants(w + "t").Select(t => t.Value);
			return string.Concat(texts);
		}
		catch
		{
			return string.Empty;
		}
	}

	/// <summary>
	/// 必备段落校验：扫描段落文本，按 Pattern 关键字匹配标题，缺失则返回错误。
	/// 支持传入 Document（自动抽取段落文本）或直接传入段落文本列表。
	/// </summary>
	public FormatComplianceResult CheckRequiredParagraph(FormatComplianceRule rule, Document doc)
	{
		return CheckRequiredParagraph(rule, ExtractParagraphTexts(doc));
	}

	public FormatComplianceResult CheckRequiredParagraph(FormatComplianceRule rule, IList<string> paragraphTexts)
	{
		FormatComplianceResult result = new FormatComplianceResult
		{
			RuleId = rule.Id,
			Passed = false,
			Position = -1
		};
		if (string.IsNullOrEmpty(rule.Pattern))
		{
			result.Passed = true;
			result.Message = "规则未指定关键字，跳过校验";
			return result;
		}
		if (paragraphTexts == null || paragraphTexts.Count == 0)
		{
			result.Message = $"缺失包含「{rule.Pattern}」的段落";
			return result;
		}
		for (int i = 0; i < paragraphTexts.Count; i++)
		{
			string text = paragraphTexts[i] ?? string.Empty;
			if (text.Contains(rule.Pattern))
			{
				result.Passed = true;
				result.Position = i;
				result.Message = $"第 {i + 1} 段命中关键字「{rule.Pattern}」";
				return result;
			}
		}
		result.Message = $"缺失包含「{rule.Pattern}」的段落";
		return result;
	}

	/// <summary>
	/// 编号连续性校验：解析段落起始编号格式，检测是否存在断裂。
	/// Pattern 用于指定需要校验的编号系列（如 "1."、"（一）"、"第一条" 等），
	/// 留空时自动按段落数量最多的编号系列进行校验。
	/// </summary>
	public FormatComplianceResult CheckNumberingContinuity(FormatComplianceRule rule, Document doc)
	{
		return CheckNumberingContinuity(rule, ExtractParagraphTexts(doc));
	}

	public FormatComplianceResult CheckNumberingContinuity(FormatComplianceRule rule, IList<string> paragraphTexts)
	{
		FormatComplianceResult result = new FormatComplianceResult
		{
			RuleId = rule.Id,
			Passed = true,
			Position = -1
		};
		if (paragraphTexts == null || paragraphTexts.Count == 0)
		{
			result.Passed = true;
			result.Message = "无段落，跳过编号连续性校验";
			return result;
		}
		// 解析每段的编号
		List<NumberingHelper.Numbering> numberings = new List<NumberingHelper.Numbering>();
		List<int> numberingIndexes = new List<int>();
		for (int i = 0; i < paragraphTexts.Count; i++)
		{
			NumberingHelper.Numbering n = NumberingHelper.Matches(paragraphTexts[i] ?? string.Empty);
			numberings.Add(n);
			numberingIndexes.Add(i);
		}
		// 若指定了 Pattern，则只校验起始编号匹配该 Pattern 的系列
		// Pattern 形如 "Series=3" 表示只校验 Series=3（阿拉伯数字加点）的系列
		// 或留空：自动选取出现次数最多的非空编号系列
		int? targetSeries = ParseTargetSeries(rule.Pattern);
		var grouped = numberings
			.Select((n, idx) => new { n, idx })
			.Where(x => x.n != null)
			.GroupBy(x => x.n.Series)
			.Select(g => new { Series = g.Key, Items = g.ToList() })
			.ToList();
		if (grouped.Count == 0)
		{
			result.Passed = true;
			result.Message = "未检测到任何编号段落，跳过校验";
			return result;
		}
		var target = targetSeries.HasValue
			? grouped.FirstOrDefault(g => g.Series == targetSeries.Value)
			: grouped.OrderByDescending(g => g.Items.Count).First();
		if (target == null)
		{
			result.Passed = true;
			result.Message = $"未检测到 Series={targetSeries} 的编号段落，跳过校验";
			return result;
		}
		// 检测连续性：相邻编号应满足 next == prev.GetNext()
		int prevNumber = -1;
		foreach (var item in target.Items)
		{
			if (prevNumber < 0)
			{
				prevNumber = item.n.Number;
				continue;
			}
			if (item.n.Number != prevNumber + 1)
			{
				result.Passed = false;
				result.Position = numberingIndexes[item.idx];
				result.Message = $"编号断裂：期望 {prevNumber + 1}，实际 {item.n.Number}（位于第 {numberingIndexes[item.idx] + 1} 段）";
				return result;
			}
			prevNumber = item.n.Number;
		}
		result.Passed = true;
		result.Message = $"编号连续（Series={target.Series}，共 {target.Items.Count} 段）";
		return result;
	}

	private static int? ParseTargetSeries(string pattern)
	{
		if (string.IsNullOrEmpty(pattern))
		{
			return null;
		}
		string p = pattern.Trim();
		if (p.StartsWith("Series=") && int.TryParse(p.Substring("Series=".Length), out int s))
		{
			return s;
		}
		return null;
	}

	/// <summary>
	/// 执行所有规则，返回结果列表。
	/// </summary>
	public List<FormatComplianceResult> ValidateAll(Document doc)
	{
		return ValidateAll(ExtractParagraphTexts(doc));
	}

	public List<FormatComplianceResult> ValidateAll(IList<string> paragraphTexts)
	{
		List<FormatComplianceResult> results = new List<FormatComplianceResult>();
		foreach (FormatComplianceRule rule in Rules)
		{
			FormatComplianceResult r;
			switch (rule.RuleType)
			{
				case FormatComplianceRuleType.RequiredParagraph:
					r = CheckRequiredParagraph(rule, paragraphTexts);
					break;
				case FormatComplianceRuleType.NumberingContinuity:
					r = CheckNumberingContinuity(rule, paragraphTexts);
					break;
				default:
					r = new FormatComplianceResult
					{
						RuleId = rule.Id,
						Passed = true,
						Message = $"未知规则类型 {rule.RuleType}，跳过",
						Position = -1
					};
					break;
			}
			results.Add(r);
		}
		return results;
	}
}
