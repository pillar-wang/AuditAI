using System.Collections.Generic;
using Leqisoft.DTO;
using Newtonsoft.Json;

namespace Leqisoft.Model;

[JsonObject]
public class TicketNav
{
	public List<Id64> Columns { get; set; }
}
