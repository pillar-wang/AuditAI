namespace Leqisoft.Model;

public class InputListOperand : ValueSetOperand
{
	public string KeyName { get; set; }

	public override OperandType OperandType => OperandType.InputListOperand;

	public InputListOperand(ValueSetOperand vso)
		: base(vso.Set)
	{
	}
}
