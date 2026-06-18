using Leqisoft.DTO;

namespace Leqisoft.Model;

public class FormulaDependency
{
	public Id64 HostTable { get; set; }

	public Id64 HostObject { get; set; }

	public FormulaDependencyObjectKind HostKind { get; set; }

	public Id64 ReferredTable { get; set; }

	public Id64 ReferredObject { get; set; }

	public FormulaDependencyObjectKind ReferredKind { get; set; }
}
