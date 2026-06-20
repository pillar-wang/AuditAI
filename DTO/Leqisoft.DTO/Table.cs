﻿﻿﻿namespace Leqisoft.DTO;

public class Table
{
	public Id64 Id { get; set; }

	public string Title { get; set; }

	public string PageSetup { get; set; }

	public int Dirty { get; set; }

	public string HeaderHeights { get; set; }

	public Id64 DefaultStyleId { get; set; }

	public string ConsolidateSettings { get; set; }

	public int Version { get; set; }

	public int BorderStyle { get; set; }

	public string CustomBorderStyle { get; set; }

	public int FrozenCols { get; set; }

	public int HeaderMode { get; set; }

	public string CollectSource { get; set; }

	public long Locker { get; set; }

	public string FilterInfo { get; set; }

	public string Foot { get; set; }

	public bool RowOwnerExclusive { get; set; }

	public bool RowOwnerLoad { get; set; }

	public byte[] RowOwnerLoadShare { get; set; }

	public string Ticket { get; set; }

	public string ControlFormula { get; set; }
}
