using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[XmlInclude(typeof(InfopartChartDefinition))]
[XmlInclude(typeof(InfopartSystemDefinition))]
[XmlInclude(typeof(InfopartPageDefinition))]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[KnownType(typeof(InfopartChartDefinition))]
[KnownType(typeof(InfopartSystemDefinition))]
[KnownType(typeof(InfopartPageDefinition))]
[DataContract(Name = "IBD", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public abstract class InfopartBaseDefinition : ControlBaseDefinition
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
		return (InfopartBaseDefinition)base.Clone();
	}

	public override void SolveProperties(MetaField field, MetaTable metaTable, MasterPage masterPage)
	{
		SolveName(field);
	}
}
