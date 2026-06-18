using System.Collections.Generic;

namespace Leqisoft.UI.Platform;

public class TableValidationInfo
{
	public IDictionary<object, object> DicField { get; } = new Dictionary<object, object>();
	public IList<object> Cells { get; } = new List<object>();
	public IList<object> Ranges { get; } = new List<object>();
	public IList<object> Columns { get; } = new List<object>();
	public IList<object> HeaderCells { get; } = new List<object>();
	public IList<object> ErrorRefs { get; } = new List<object>();
	public IList<object> Titles { get; } = new List<object>();
	public IList<object> Foots { get; } = new List<object>();
}