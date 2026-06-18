using System;
using System.Collections.Generic;
using System.Drawing;
using Leqisoft.DTO;
using Newtonsoft.Json;

namespace Leqisoft.Model;

public class TableTitleCell
{
	private static HashSet<TableTitleCell> _circularRefDetector = new HashSet<TableTitleCell>();

	private float _fontSize;

	private int _margin;

	[JsonIgnore]
	internal Table _table { get; set; }

	[JsonIgnore]
	public object Value { get; set; }

	public BinaryValue BinaryValue { get; set; }

	[JsonIgnore]
	private DataReferenceManager _drm => _table.Project.DataReferenceManager;

	public string Formula { get; set; }

	public Color ForeColor { get; set; }

	public Color BackColor { get; set; }

	public string FontFamily { get; set; }

	public float FontSize
	{
		get
		{
			if (_fontSize <= 5f)
			{
				_fontSize = 6f;
			}
			return _fontSize;
		}
		set
		{
			_fontSize = value;
		}
	}

	public CellTextAlign Align { get; set; }

	public bool Bold { get; set; }

	public bool Italic { get; set; }

	public bool Underline { get; set; }

	public bool Strikeout { get; set; }

	public string DefaultValue { get; set; }

	public string Comment { get; set; }

	public string ComboList { get; set; }

	public bool MultiComboList { get; set; }

	public bool IgnoreComboList { get; set; }

	public string CellId { get; set; }

	public int Margin
	{
		get
		{
			return _margin;
		}
		set
		{
			_margin = Math.Max(0, value);
		}
	}

	public DataFormat DataFormat { get; set; }

	public string GetDisplayValue()
	{
		return Cell.GetDisplayValueImpl(Value, DataFormat);
	}

	public string ConvertToDisplayValue(object value)
	{
		return Cell.GetDisplayValueImpl(value, DataFormat);
	}

	public void EvaluateFormula()
	{
		if (string.IsNullOrWhiteSpace(Formula))
		{
			return;
		}
		try
		{
			if (_circularRefDetector.Contains(this))
			{
				Value = "[公式存在循环引用，请修改]";
			}
			_circularRefDetector.Add(this);
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(Formula)
			{
				Env = new FormulaEvaluationEnvironment
				{
					Resolver = new FormulaReferenceModelResolver(_table.Project),
					RefManager = _drm,
					RefEvalContext = new DataReferenceEvaluationContext
					{
						Project = _table.Project,
						CurrentTreeNode = _table.TreeNode
					}
				}
			};
			Value = formulaEvaluator.Evaluate();
		}
		finally
		{
			_circularRefDetector.Remove(this);
		}
	}

	public TableTitleCell Clone()
	{
		return (TableTitleCell)MemberwiseClone();
	}

	internal void InitTitleCell()
	{
		FontFamily = UserSet.Config.TableStyle.TitleStyle.FontFamily;
		FontSize = UserSet.Config.TableStyle.TitleStyle.FontSize;
		ForeColor = UserSet.Config.TableStyle.TitleStyle.FontColor;
		Bold = UserSet.Config.TableStyle.TitleStyle.Bold;
	}

	internal void InitSubtitleCell()
	{
		FontFamily = UserSet.Config.TableStyle.SubTitleStyle.FontFamily;
		FontSize = UserSet.Config.TableStyle.SubTitleStyle.FontSize;
		ForeColor = UserSet.Config.TableStyle.SubTitleStyle.FontColor;
		Bold = UserSet.Config.TableStyle.SubTitleStyle.Bold;
	}
}
