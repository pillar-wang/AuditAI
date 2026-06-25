namespace Auditai.UI.Controls.CollectCell;

public class Operand
{
	private static Operand Default = new Operand(OperateEnum.Add, 0m);

	public OperateEnum Operation { get; set; }

	public decimal Value { get; set; }

	public Operand(OperateEnum operate, decimal value)
	{
		Operation = operate;
		Value = value;
	}

	public Operand First()
	{
		return new Operand(OperateEnum.Add, 0m).With(this);
	}

	public Operand With(Operand item)
	{
		switch (item.Operation)
		{
		case OperateEnum.Add:
			Value = decimal.Add(Value, item.Value);
			return this;
		case OperateEnum.Subtract:
			Value = decimal.Subtract(Value, item.Value);
			return this;
		default:
			return null;
		}
	}
}
