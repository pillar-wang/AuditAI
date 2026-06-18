using System;
using System.Collections;

namespace ParadoxReader;

public abstract class ParadoxCondition
{
	public class Compare : ParadoxCondition
	{
		public ParadoxCompareOperator Operator { get; private set; }

		public object Value { get; private set; }

		public int DataFieldIndex { get; private set; }

		public int IndexFieldIndex { get; private set; }

		public override bool IsDataOk(ParadoxRecord dataRec)
		{
			object a = dataRec.DataValues[DataFieldIndex];
			int num = Comparer.Default.Compare(a, Value);
			return Operator switch
			{
				ParadoxCompareOperator.Equal => num == 0, 
				ParadoxCompareOperator.NotEqual => num != 0, 
				ParadoxCompareOperator.Greater => num > 0, 
				ParadoxCompareOperator.GreaterOrEqual => num >= 0, 
				ParadoxCompareOperator.Less => num < 0, 
				ParadoxCompareOperator.LessOrEqual => num <= 0, 
				_ => throw new NotSupportedException(), 
			};
		}

		public override bool IsIndexPossible(ParadoxRecord indexRec, ParadoxRecord nextRec)
		{
			object a = indexRec.DataValues[DataFieldIndex];
			int num = Comparer.Default.Compare(a, Value);
			int num2;
			if (nextRec != null)
			{
				object a2 = nextRec.DataValues[DataFieldIndex];
				num2 = Comparer.Default.Compare(a2, Value);
			}
			else
			{
				num2 = 1;
			}
			return Operator switch
			{
				ParadoxCompareOperator.Equal => num <= 0 && num2 >= 0, 
				ParadoxCompareOperator.NotEqual => num > 0 || num2 < 0, 
				ParadoxCompareOperator.Greater => num2 > 0, 
				ParadoxCompareOperator.GreaterOrEqual => num2 >= 0, 
				ParadoxCompareOperator.Less => num < 0, 
				ParadoxCompareOperator.LessOrEqual => num <= 0, 
				_ => throw new NotSupportedException(), 
			};
		}

		public Compare(ParadoxCompareOperator op, object value, int dataFieldIndex, int indexFieldIndex)
		{
			Operator = op;
			Value = value;
			DataFieldIndex = dataFieldIndex;
			IndexFieldIndex = indexFieldIndex;
		}
	}

	public abstract class Multiple : ParadoxCondition
	{
		protected ParadoxCondition[] SubConditions { get; private set; }

		protected Multiple(ParadoxCondition[] subConditions)
		{
			SubConditions = subConditions;
		}

		public override bool IsDataOk(ParadoxRecord dataRec)
		{
			return Test((ParadoxCondition c) => c.IsDataOk(dataRec));
		}

		public override bool IsIndexPossible(ParadoxRecord indexRec, ParadoxRecord nextRec)
		{
			return Test((ParadoxCondition c) => c.IsIndexPossible(indexRec, nextRec));
		}

		protected abstract bool Test(Predicate<ParadoxCondition> test);
	}

	public class LogicalAnd : Multiple
	{
		public LogicalAnd(params ParadoxCondition[] subConditions)
			: base(subConditions)
		{
		}

		protected override bool Test(Predicate<ParadoxCondition> test)
		{
			ParadoxCondition[] subConditions = base.SubConditions;
			foreach (ParadoxCondition obj in subConditions)
			{
				if (!test(obj))
				{
					return false;
				}
			}
			return true;
		}
	}

	public class LogicalOr : Multiple
	{
		public LogicalOr(params ParadoxCondition[] subConditions)
			: base(subConditions)
		{
		}

		protected override bool Test(Predicate<ParadoxCondition> test)
		{
			ParadoxCondition[] subConditions = base.SubConditions;
			foreach (ParadoxCondition obj in subConditions)
			{
				if (test(obj))
				{
					return true;
				}
			}
			return false;
		}
	}

	public abstract bool IsDataOk(ParadoxRecord dataRec);

	public abstract bool IsIndexPossible(ParadoxRecord indexRec, ParadoxRecord nextRec);
}
