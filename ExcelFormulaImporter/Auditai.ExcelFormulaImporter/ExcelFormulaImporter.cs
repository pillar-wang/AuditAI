using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Irony.Parsing;
using Auditai.DTO;
using XLParser;

namespace Auditai.ExcelFormulaImporter;

public class ExcelFormulaImporter
{
	private static readonly Regex _reCell = new Regex("([A-Z]+)(\\d+)", RegexOptions.Compiled);

	private ExcelFormulaImportContext _context;

	public ExcelFormulaImporter(ExcelFormulaImportContext context)
	{
		_context = context;
	}

	public string Convert(string excelFormula)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		excelFormula = excelFormula.TrimStart('=');
		ParseTreeNode root = ExcelFormulaParser.Parse(excelFormula);
		FormulaAnalyzer formulaAnalyzer = new FormulaAnalyzer(root);
		List<Tuple<SourceSpan, string>> list = new List<Tuple<SourceSpan, string>>();
		StringBuilder stringBuilder = new StringBuilder(excelFormula);
		foreach (ParseTreeNode item in formulaAnalyzer.References())
		{
			ParseTreeNode parseTreeNode = item.ChildNodes[0];
			if (parseTreeNode.Is("Cell"))
			{
				string text = parseTreeNode.ChildNodes[0].Token.Text;
				Tuple<int, int> rowColumn = GetRowColumn(text);
				Id64 val = _context.CellMapper(null, rowColumn.Item1, rowColumn.Item2);
				list.Add(Tuple.Create(item.Span, $"[1:{_context.CurrentTableId}:{val}]"));
			}
			else if (parseTreeNode.Is("Prefix"))
			{
				string text2 = parseTreeNode.ChildNodes[0].Token.Text.TrimEnd('!');
				string text3 = item.ChildNodes[1].ChildNodes[0].Token.Text;
				Tuple<int, int> rowColumn2 = GetRowColumn(text3);
				Id64 val2 = _context.SheetNameMapper(text2);
				Id64 val3 = _context.CellMapper(text2, rowColumn2.Item1, rowColumn2.Item2);
				list.Add(Tuple.Create(item.Span, $"[1:{val2}:{val3}]"));
			}
			else if (parseTreeNode.Is("ReferenceFunctionCall"))
			{
				ParseTreeNode parseTreeNode2 = parseTreeNode.ChildNodes[0].ChildNodes[0];
				if (parseTreeNode2.Is("Cell"))
				{
					string text4 = parseTreeNode2.ChildNodes[0].Token.Text;
					string text5 = parseTreeNode.ChildNodes[2].ChildNodes[0].ChildNodes[0].Token.Text;
					Tuple<int, int> rowColumn3 = GetRowColumn(text4);
					Tuple<int, int> rowColumn4 = GetRowColumn(text5);
					Id64 val4 = _context.CellMapper(null, rowColumn3.Item1, rowColumn3.Item2);
					Id64 val5 = _context.CellMapper(null, rowColumn4.Item1, rowColumn4.Item2);
					list.Add(Tuple.Create(item.Span, $"[3:{_context.CurrentTableId}:{val4}:{val5}]"));
				}
				else if (parseTreeNode2.Is("Prefix"))
				{
					string text6 = parseTreeNode2.ChildNodes[0].Token.Text.TrimEnd('!');
					string text7 = parseTreeNode.ChildNodes[0].ChildNodes[1].ChildNodes[0].Token.Text;
					string text8 = parseTreeNode.ChildNodes[2].ChildNodes[0].ChildNodes[0].Token.Text;
					Tuple<int, int> rowColumn5 = GetRowColumn(text7);
					Tuple<int, int> rowColumn6 = GetRowColumn(text8);
					Id64 val6 = _context.SheetNameMapper(text6);
					Id64 val7 = _context.CellMapper(text6, rowColumn5.Item1, rowColumn5.Item2);
					Id64 val8 = _context.CellMapper(text6, rowColumn6.Item1, rowColumn6.Item2);
					list.Add(Tuple.Create(item.Span, $"[3:{_context.CurrentTableId}:{val7}:{val8}]"));
				}
			}
		}
		list.Reverse();
		foreach (Tuple<SourceSpan, string> item2 in list)
		{
			stringBuilder.Remove(item2.Item1.Location.Position, item2.Item1.Length);
			stringBuilder.Insert(item2.Item1.Location.Position, item2.Item2);
		}
		return stringBuilder.ToString();
	}

	private static Tuple<int, int> GetRowColumn(string cell)
	{
		Match match = _reCell.Match(cell);
		int item = ColumnLetterToInt(match.Groups[1].Value);
		int item2 = int.Parse(match.Groups[2].Value);
		return Tuple.Create(item2, item);
	}

	private static int ColumnLetterToInt(string columnLetter)
	{
		return columnLetter.Select((char c, int i) => (c - 65 + 1) * (int)Math.Pow(26.0, columnLetter.Length - i - 1)).Sum();
	}
}
