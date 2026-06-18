using System.Xml;
using System.Xml.Linq;

namespace Leqisoft.Model;

public class XEntity : XText
{
	public XEntity(string value)
		: base(value)
	{
	}

	public override void WriteTo(XmlWriter writer)
	{
		writer.WriteEntityRef(base.Value);
	}
}
