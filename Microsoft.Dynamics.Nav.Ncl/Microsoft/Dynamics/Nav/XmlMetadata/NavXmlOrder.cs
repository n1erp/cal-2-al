using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public class NavXmlOrder : NavXmlBase
{
	private string sequence;

	[XmlAttribute("sequence")]
	public string Sequence
	{
		get
		{
			return sequence;
		}
		set
		{
			sequence = value;
		}
	}
}
