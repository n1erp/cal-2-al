using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[XmlInclude(typeof(PageDefinition))]
[XmlInclude(typeof(NavigationDefinition))]
[XmlInclude(typeof(MasterPage))]
[XmlInclude(typeof(MasterNavigation))]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[KnownType(typeof(PageDefinition))]
[KnownType(typeof(NavigationDefinition))]
[KnownType(typeof(MasterPage))]
[KnownType(typeof(MasterNavigation))]
[DataContract(Name = "CD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class ConceptDefinition : NamedElementDefinition
{
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
		return (ConceptDefinition)base.Clone();
	}
}
