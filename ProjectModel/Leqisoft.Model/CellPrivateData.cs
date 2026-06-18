using System.Text;
using Newtonsoft.Json;

namespace Leqisoft.Model;

public class CellPrivateData
{
	[JsonProperty(PropertyName = "M")]
	public bool IsExistManualInputValue { get; set; }

	public byte[] GetBytes()
	{
		string s = JsonConvert.SerializeObject(this);
		return Encoding.UTF8.GetBytes(s);
	}

	protected bool IsEmpty(Cell cellTarget)
	{
		if (IsExistManualInputValue && cellTarget.IsAllowManualInputOnFormula)
		{
			return false;
		}
		return true;
	}

	public static byte[] GetBytes(CellPrivateData data, Cell cellTarget)
	{
		if (data == null)
		{
			return null;
		}
		if (data.IsEmpty(cellTarget))
		{
			return null;
		}
		return data.GetBytes();
	}

	public static CellPrivateData FromBytes(byte[] data)
	{
		if (data == null)
		{
			return null;
		}
		string @string = Encoding.UTF8.GetString(data);
		return JsonConvert.DeserializeObject<CellPrivateData>(@string);
	}
}
