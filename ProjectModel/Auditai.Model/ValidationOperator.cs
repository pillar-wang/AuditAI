using Newtonsoft.Json;

namespace Auditai.Model;

[JsonConverter(typeof(ValidationOperatorJsonConverter))]
public class ValidationOperator
{
	public int Code { get; }

	public string Display { get; }

	public static ValidationOperator[] Operators { get; } = new ValidationOperator[6]
	{
		new ValidationOperator(0, "="),
		new ValidationOperator(1, ">"),
		new ValidationOperator(2, ">="),
		new ValidationOperator(3, "<"),
		new ValidationOperator(4, "<="),
		new ValidationOperator(5, "<>")
	};


	public static ValidationOperator FromCode(int code)
	{
		return Operators[code];
	}

	private ValidationOperator(int code, string display)
	{
		Code = code;
		Display = display;
	}
}
