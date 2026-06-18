﻿using Leqisoft.Model;

namespace Leqisoft.UI.Platform;

public class TicketInputColumnVM
{
	public Column TableColumn { get; set; }
	public TicketColumn TicketColumn { get; set; }
	public bool IsHiddenColumn { get; set; }
	public string Formula { get; set; }
}