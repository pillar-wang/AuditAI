using System.Reflection;
using Newtonsoft.Json;

namespace Leqisoft.DTO;

[Obfuscation(Exclude = true)]
public class NotifyMessage : IMessage
{
	public const string NORMAL_MESSAGE = "normalmessage";

	public const string SEND_FILE_REQUEST = "sendfilerequest";

	public const string SEND_FILE_RESPONSE = "sendfileresponse";

	public const string SEND_FILE_CANCELSEND = "cancelsend";

	public const string SEND_FILE_CANCELRECIEVE = "cancelrecieve";

	public const string SEND_MERGE_REQUEST = "mergemessage";

	[JsonProperty(PropertyName = "k")]
	public string Kind { get; set; } = "normalmessage";


	[JsonProperty(PropertyName = "t")]
	public string Text { get; set; }

	[JsonProperty(PropertyName = "b")]
	public bool Bullet { get; set; }

	[JsonProperty(PropertyName = "v")]
	public object Value { get; set; }

	public NotifyMessage()
	{
		Text = string.Empty;
		Bullet = false;
		Value = string.Empty;
	}

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}

	public static bool TryParse(string json, out NotifyMessage mpg)
	{
		try
		{
			mpg = JsonConvert.DeserializeObject<NotifyMessage>(json);
			if (mpg == null)
			{
				return false;
			}
			return true;
		}
		catch
		{
			mpg = null;
			return false;
		}
	}
}
