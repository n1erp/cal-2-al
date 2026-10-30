using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
public enum TypeCategoryKind
{
	Unknown = 0,
	Simple = 1,
	Complex = 2,
	ApplicationObject = 3,
	Array = 4,
	Error = 6,
	Module = 8,
	Option = 9,
	Field = Option,
	Control = 10,
	Action = 11,
	QueryElement = 12,
	ReportDataElement = 13,
	RequestPage = 14
}
