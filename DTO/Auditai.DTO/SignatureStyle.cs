using Newtonsoft.Json;

namespace Auditai.DTO;

public class SignatureStyle
{
	private string _signatureRow;

	private string _reviewSignRow;

	[JsonProperty(PropertyName = "SignatureFormat")]
	public string SignatureFormat { get; set; }

	[JsonProperty(PropertyName = "SignatureRow")]
	public string SignatureRow
	{
		get
		{
			return _signatureRow;
		}
		set
		{
			_signatureRow = value;
		}
	}

	[JsonProperty(PropertyName = "SignatureAlign")]
	public SignAlign SignatureAlign { get; set; }

	[JsonProperty(PropertyName = "ReviewSignFormat")]
	public string ReviewSignFormat { get; set; }

	[JsonProperty(PropertyName = "ReviewSignRow")]
	public string ReviewSignRow
	{
		get
		{
			return _reviewSignRow;
		}
		set
		{
			_reviewSignRow = value;
		}
	}

	[JsonProperty(PropertyName = "ReviewSignAlign")]
	public SignAlign ReviewSignAlign { get; set; }

	public SignatureStyle()
	{
		SignatureRow = "1";
		SignatureAlign = SignAlign.Right;
		SignatureFormat = "编制人：[当前用户姓名]  编制日期：[当前日期]";
		ReviewSignRow = "2";
		ReviewSignAlign = SignAlign.Right;
		ReviewSignFormat = "复核人：[当前用户姓名]  复核日期：[当前日期]";
	}
}
