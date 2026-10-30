using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

[Flags]
public enum UnsupportedAppFieldChangeTypes
{
	None = 0,
	Name = 1,
	AutoIncrement = 2,
	DataType = 4,
	LengthReduced = 8,
	SqlDataType = 0x10,
	Compression = 0x20,
	LengthChanged = 0x40
}
