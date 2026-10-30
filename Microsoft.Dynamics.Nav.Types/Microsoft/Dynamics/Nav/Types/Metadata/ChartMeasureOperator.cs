using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("xsd", "2.0.50727.42")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
public enum ChartMeasureOperator
{
	None,
	Count,
	Sum,
	Min,
	Max,
	Avg,
	Relative,
	RelativePct
}
