using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("xsd", "2.0.50727.42")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
public enum ChartType
{
	Column,
	Point,
	Bubble,
	Line,
	ColumnStacked,
	ColumnStacked100,
	Area,
	AreaStacked,
	AreaStacked100,
	StepLine,
	Pie,
	Doughnut,
	Range,
	Radar,
	Funnel
}
