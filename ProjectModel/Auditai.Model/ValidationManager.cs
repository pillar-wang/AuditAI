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
			return CreateEmptyValidationResult(vf);
		}

		var environment = CreateValidationEnvironment();

		try
		{
			return ExecuteValidation(vf, environment);
		}
		catch (FormulaException)
		{
			if (rethrow)
			{
				throw;
			}
			return CreateEmptyValidationResult(vf);
		}
	}

	private List<ValidationResult> ExecuteValidation(ValidationFormula vf, FormulaEvaluationEnvironment environment)
	{
		var leftEvaluator = new FormulaEvaluator(vf.LeftExpr) { Env = environment };
		var rightEvaluator = new FormulaEvaluator(vf.RightExpr) { Env = environment };
		var table = _project.GetTableById(vf.TableId)?.LoadAndReturn();
		var references = GetMergedReferences(leftEvaluator, rightEvaluator, environment);

		if (HasColumnWildcardWithLookup(leftEvaluator, rightEvaluator, references))
		{
			return HandleColumnWildcardValidation(vf, leftEvaluator, rightEvaluator, environment, references);
		}

		return HandleRegularValidation(vf, leftEvaluator, rightEvaluator, environment, references, table);
	}

	private FormulaEvaluationEnvironment CreateValidationEnvironment()
	{
		var resolver = new FormulaReferenceModelResolver(_project);
		return new FormulaEvaluationEnvironment
		{
			Resolver = resolver,
			RefManager = _project.DataReferenceManager,
			RefEvalContext = new DataReferenceEvaluationContext
			{
				Project = _project
			}
		};
	}

	private static List<ValidationResult> CreateEmptyValidationResult(ValidationFormula vf)
	{
		return new List<ValidationResult>
		{
			new ValidationResult
			{
				Source = vf
			}
		};
	}

	private static bool HasColumnWildcardWithLookup(FormulaEvaluator leftEval, FormulaEvaluator rightEval, FormulaReferences references)
	{
		return (leftEval.HasLqSumIfVLookUp() || rightEval.HasLqSumIfVLookUp())
			   && references.ColumnWildcardReferences.Count > 0;
	}

	private static FormulaReferences GetMergedReferences(FormulaEvaluator leftEvaluator, FormulaEvaluator rightEvaluator, FormulaEvaluationEnvironment environment)
	{
		var leftRefs = leftEvaluator.ValidationGetReferences(environment);
		var rightRefs = rightEvaluator.ValidationGetReferences(environment);
		leftRefs.UnionWith(rightRefs);
		return leftRefs;
	}

	private List<ValidationResult> HandleColumnWildcardValidation(
		ValidationFormula vf,
		FormulaEvaluator leftEvaluator,
		FormulaEvaluator rightEvaluator,
		FormulaEvaluationEnvironment environment,
		FormulaReferences references)
	{
		var results = new List<ValidationResult>();
		var column = references.ColumnWildcardReferences.First();
		var distinctCells = column.GetCells().Distinct(CellValueEqualsComparer.Instance);

		foreach (var cell in distinctCells)
		{
			environment.RowIndex = cell.Row.Index;
			var cellRefs = GetMergedReferences(leftEvaluator, rightEvaluator, environment);
			cellRefs.ColumnWildcardReferences.Clear();

			var result = CreateValidationResult(
				vf, leftEvaluator, rightEvaluator, cellRefs, cell.Row.Index, hasWildcard: false);
			results.Add(result);
		}

		return results;
	}

	private List<ValidationResult> HandleRegularValidation(
		ValidationFormula vf,
		FormulaEvaluator leftEvaluator,
		FormulaEvaluator rightEvaluator,
		FormulaEvaluationEnvironment environment,
		FormulaReferences references,
		Table table)
	{
		var (startIndex, endIndex, hasWildcard) = DetermineValidationRange(references, table);
		var results = new List<ValidationResult>();

		for (int i = startIndex; i <= endIndex; i++)
		{
			if (ShouldSkipRowForColumnWildcard(references, i))
			{
				continue;
			}

			try
			{
				environment.RowIndex = i;
				environment.HostTable = table;

				var result = CreateValidationResult(
					vf, leftEvaluator, rightEvaluator, references, i, hasWildcard);
				results.Add(result);
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new FormulaBadReferenceException();
			}
		}

		return results;
	}

	private static (int startIndex, int endIndex, bool hasWildcard) DetermineValidationRange(
		FormulaReferences references, Table table)
	{
		if (references.ColumnWildcardReferences.Count > 0)
		{
			return (0, table.Rows.Count - 1, true);
		}

		if (references.HeaderCellWildcardReferences.Count > 0)
		{
			var cell = references.HeaderCellWildcardReferences.First();
			return (cell.Row.Index + 1, cell.GetHeaderLastRow(), true);
		}

		return (0, 0, false);
	}

	private static bool ShouldSkipRowForColumnWildcard(FormulaReferences references, int rowIndex)
	{
		if (references.ColumnWildcardReferences.Count <= 0)
		{
			return false;
		}

		var column = references.ColumnWildcardReferences.First();
		return !column.Table[rowIndex, column.Index].ShouldApplyColumnFormula();
	}

	private static ValidationResult CreateValidationResult(
		ValidationFormula vf,
		FormulaEvaluator leftEvaluator,
		FormulaEvaluator rightEvaluator,
		FormulaReferences references,
		int rowIndex,
		bool hasWildcard)
	{
		var leftValue = RoundDoubleValue(leftEvaluator.EvaluateToOperand());
		var rightValue = RoundDoubleValue(rightEvaluator.EvaluateToOperand());

		return new ValidationResult
		{
			Source = vf,
			IsValid = true,
			LeftValue = leftValue.Evaluate(),
			RightValue = rightValue.Evaluate(),
			Refs = references,
			Passed = GetPassed(leftValue, rightValue, vf.Operator),
			RowIndex = rowIndex,
			HasWildcard = hasWildcard
		};
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