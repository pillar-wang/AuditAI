using System.Collections.Generic;
using Auditai.DTO;
using Newtonsoft.Json;

namespace Auditai.Model;

[JsonObject]
public class TicketNav
{
	public List<Id64> Columns { get; set; }
}
