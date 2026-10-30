using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SumIndexField : ElementDefinition
{
	private int id;

	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	private string name;

	[DefaultValue(0)]
	[XmlAttribute]
	[DataMember(IsRequired = false, EmitDefaultValue = false)]
	public int Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	[DefaultValue("")]
	[XmlAttribute]
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}
}
