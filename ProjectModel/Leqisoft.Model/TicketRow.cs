using System;
using Newtonsoft.Json;

namespace Leqisoft.Model;

[JsonObject]
public class TicketRow
{
	private int _height;

	public bool IsMixRangeFixedDataRow { get; set; }

	public bool IsMixRangeDynamicDataRow { get; set; }

	public bool IsMixRangeTemplateRow { get; set; }

	public int MixRangeDynamicDataRowTemplateId { get; set; } = -1;


	public int MixRangeDataKeyId { get; set; } = -1;


	public int Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = Math.Max(1, value);
		}
	}
}
