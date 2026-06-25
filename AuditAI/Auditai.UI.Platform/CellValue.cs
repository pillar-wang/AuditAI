using C1.Win.C1FlexGrid;
using Auditai.Model;

namespace Auditai.UI.Platform;

public class CellValue
{
	public Node Node { get; set; }

	public string Col { get; set; }

	public Permission Permission { get; set; }

	public CellValue(Node node, string col, Permission permission)
	{
		Node = node;
		Col = col;
		Permission = permission;
	}
}
