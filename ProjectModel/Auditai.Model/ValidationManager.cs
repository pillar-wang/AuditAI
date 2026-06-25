using System;
using System.Collections.Generic;
using System.Linq;
using Auditai.DTO;

namespace Auditai.Model;

public class ValidationManager
{
	private Project _project;

	internal HashSet<Id64> _removed = new HashSet<Id64>();

	internal HashSet<Id64> _toDelete = new HashSet<Id64>();

	private static FunctionEvaluator _roundFunctionEvaluator = new FunctionEvaluator(null, null);

	public List<ValidationFormula> Formulas { get; } = new List<ValidationFormula>();


	public ValidationManager(Project project)
	{
		_project = project;
	}

	public void RemoveOne(ValidationFormula vf)
	{
		Formulas.Remove(vf);
		_removed.Add(vf.Id);
		_project.FormulaManager.RemoveHostObject(vf.TableId, vf.Id);
	}

	public List<ValidationResult> Validate(ValidationFormula vf, bool rethrow = false)
	{
		if (string.IsNullOrEmpty(vf.LeftExpr) || string.IsNullOrEmpty(vf.RightExpr))
		{
			return new List<ValidationResult>
			{
				new ValidationResult
				{
					Source = vf
				}
			};
		}
		FormulaReferenceModelResolver resolver = new FormulaReferenceModelResolver(_project);
		FormulaEvaluationEnvironment formulaEvaluationEnvironment = new FormulaEvaluationEnvironment
		{
			Resolver = resolver,
			RefManager = _project.DataReferenceManager,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = _project
			}
		};
		try
		{
			List<ValidationResult> list = new List<ValidationResult>();
			FormulaEvaluator formulaEvaluator = new FormulaEvaluator(vf.LeftExpr)
			{
				Env = formulaEvaluationEnvironment
			};
			FormulaEvaluator formulaEvaluator2 = new FormulaEvaluator(vf.RightExpr)
			{
				Env = formulaEvaluationEnvironment
			};
			Table table = _project.GetTableById(vf.TableId)?.LoadAndReturn();
			FormulaReferences formulaReferences = formulaEvaluator.ValidationGetReferences(formulaEvaluationEnvironment);
			FormulaReferences other = formulaEvaluator2.ValidationGetReferences(formulaEvaluationEnvironment);
			formulaReferences.UnionWith(other);
			if ((formulaEvaluator.HasLqSumIfVLookUp() || formulaEvaluator2.HasLqSumIfVLookUp()) && formulaReferences.ColumnWildcardReferences.Count > 0)
			{
				Column column = formulaReferences.ColumnWildcardReferences.First();
				IEnumerable<Cell> enumerable = column.GetCells().Distinct(CellValueEqualsComparer.Instance);
				foreach (Cell item3 in enumerable)
				{
					formulaEvaluationEnvironment.RowIndex = item3.Row.Index;
					formulaReferences = formulaEvaluator.ValidationGetReferences(formulaEvaluationEnvironment);
					other = formulaEvaluator2.ValidationGetReferences(formulaEvaluationEnvironment);
					formulaReferences.UnionWith(other);
					formulaReferences.ColumnWildcardReferences.Clear();
					Operand operand = RoundDoubleValue(formulaEvaluator.EvaluateToOperand());
					Operand operand2 = RoundDoubleValue(formulaEvaluator2.EvaluateToOperand());
					ValidationResult item = new ValidationResult
					{
						Source = vf,
						IsValid = true,
						LeftValue = operand.Evaluate(),
						RightValue = operand2.Evaluate(),
						Refs = formulaReferences,
						Passed = GetPassed(operand, operand2, vf.Operator),
						RowIndex = item3.Row.Index,
						HasWildcard = false
					};
					list.Add(item);
				}
			}
			else
			{
				int num;
				int num2;
				bool hasWildcard;
				if (formulaReferences.ColumnWildcardReferences.Count > 0)
				{
					num = 0;
					num2 = table.Rows.Count - 1;
					hasWildcard = true;
				}
				else if (formulaReferences.HeaderCellWildcardReferences.Count > 0)
				{
					Cell cell = formulaReferences.HeaderCellWildcardReferences.First();
					num = cell.Row.Index + 1;
					num2 = cell.GetHeaderLastRow();
					hasWildcard = true;
				}
				else
				{
					num = (num2 = 0);
					hasWildcard = false;
				}
				for (int i = num; i <= num2; i++)
				{
					Operand operand3;
					Operand operand4;
					try
					{
						if (formulaReferences.ColumnWildcardReferences.Count <= 0)
						{
							goto IL_02ce;
						}
						Column column2 = formulaReferences.ColumnWildcardReferences.First();
						if (column2.Table[i, column2.Index].ShouldApplyColumnFormula())
						{
							goto IL_02ce;
						}
						goto end_IL_0290;
						IL_02ce:
						formulaEvaluationEnvironment.RowIndex = i;
						formulaEvaluationEnvironment.HostTable = table;
						operand3 = RoundDoubleValue(formulaEvaluator.EvaluateToOperand());
						operand4 = RoundDoubleValue(formulaEvaluator2.EvaluateToOperand());
						goto IL_0303;
						end_IL_0290:;
					}
					catch (ArgumentOutOfRangeException)
					{
						throw new FormulaBadReferenceException();
					}
					continue;
					IL_0303:
					ValidationResult item2 = new ValidationResult
					{
						Source = vf,
						IsValid = true,
						LeftValue = operand3.Evaluate(),
						RightValue = operand4.Evaluate(),
						Refs = formulaReferences,
						Passed = GetPassed(operand3, operand4, vf.Operator),
						RowIndex = i,
						HasWildcard = hasWildcard
					};
					list.Add(item2);
				}
			}
			return list;
		}
		catch (FormulaException)
		{
			if (rethrow)
			{
				throw;
			}
			return new List<ValidationResult>
			{
				new ValidationResult
				{
					Source = vf
				}
			};
		}
	}

	public HashSet<Id64> GetReferredTables(ValidationFormula vf)
	{
		try
		{
			HashSet<Id64> referredTableIds = new FormulaEvaluator(vf.LeftExpr).GetReferredTableIds();
			HashSet<Id64> referredTableIds2 = new FormulaEvaluator(vf.RightExpr).GetReferredTableIds();
			referredTableIds.UnionWith(referredTableIds2);
			return referredTableIds;
		}
		catch (FormulaException)
		{
			return new HashSet<Id64>();
		}
	}

	internal void Reset()
	{
		Formulas.Clear();
		_removed.Clear();
		_toDelete.Clear();
	}

	private static bool GetPassed(Operand left, Operand right, ValidationOperator op)
	{
		if (left is NumberOperand numberOperand)
		{
			NumberOperand numberOperand2 = right.ToNumber();
			double num = Math.Round(Math.Abs(numberOperand.Value - numberOperand2.Value), 4, MidpointRounding.AwayFromZero);
			bool flag = num < 0.0001;
			switch (op.Code)
			{
			case 0:
				return flag;
			case 1:
				if (!flag)
				{
					return numberOperand.Value > numberOperand2.Value;
				}
				return false;
			case 2:
				if (!flag)
				{
					return numberOperand.Value > numberOperand2.Value;
				}
				return true;
			case 3:
				if (!flag)
				{
					return numberOperand.Value < numberOperand2.Value;
				}
				return false;
			case 4:
				if (!flag)
				{
					return numberOperand.Value < numberOperand2.Value;
				}
				return true;
			case 5:
				return !flag;
			}
		}
		return op.Code switch
		{
			0 => (bool)left.Equal(right).ToBool(), 
			1 => (bool)left.GreaterThan(right).ToBool(), 
			2 => (bool)left.GreaterThanOrEqual(right).ToBool(), 
			3 => (bool)left.LessThan(right).ToBool(), 
			4 => (bool)left.LessThanOrEqual(right).ToBool(), 
			5 => (bool)left.NotEqual(right).ToBool(), 
			_ => throw new ArgumentOutOfRangeException("op", op.Code, ""), 
		};
	}

	private static Operand RoundDoubleValue(Operand value)
	{
		if (value is NumberOperand number)
		{
			return _roundFunctionEvaluator.Round(number, 4);
		}
		if (value is CellOperand { Value: NumberOperand value2 })
		{
			return _roundFunctionEvaluator.Round(value2, 4);
		}
		if (value is ValueOperand { Object: var @object } && @object is double num)
		{
			return _roundFunctionEvaluator.Round(num, 4);
		}
		return value;
	}
}
