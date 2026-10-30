using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "PVD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[XmlRoot(ElementName = "Views", Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects", IsNullable = false)]
public class PageViewDefinition : ElementDefinition
{
	[DataMember(Name = "PVD_1", EmitDefaultValue = false, IsRequired = false)]
	private string viewLayoutDeltaField;

	[DataMember(Name = "PVD_2", EmitDefaultValue = false, IsRequired = false)]
	private string nameField;

	public string ViewLayoutDelta
	{
		get
		{
			return viewLayoutDeltaField;
		}
		set
		{
			viewLayoutDeltaField = value;
		}
	}

	[XmlAttribute]
	public string Name
	{
		get
		{
			return nameField;
		}
		set
		{
			nameField = value;
		}
	}

	public override ElementDefinition Find(Predicate<ElementDefinition> match)
	{
		if (match(this))
		{
			return this;
		}
		return base.Find(match);
	}

	public override object Clone()
	{
		return (PageViewDefinition)base.Clone();
	}
}
