using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("xsd", "2.0.50727.42")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
public class MeasureAxisDefinition : AxisDefinition
{
	private MeasureDefinition[] measuresField;

	[XmlArrayItem("Measure", IsNullable = false)]
	public MeasureDefinition[] Measures
	{
		get
		{
			return measuresField;
		}
		set
		{
			measuresField = value;
		}
	}
}
