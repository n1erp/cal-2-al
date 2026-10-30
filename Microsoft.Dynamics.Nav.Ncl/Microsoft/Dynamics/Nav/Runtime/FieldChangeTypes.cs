using System;

namespace Microsoft.Dynamics.Nav.Runtime;

[Flags]
public enum FieldChangeTypes
{
	None = 0,
	Missing = 1,
	Excessive = 2,
	Name = 4,
	AutoIncrement = 8,
	DataType = 0x10,
	Length = 0x20,
	Sorting = 0x40,
	SqlDataType = 0x80,
	Compression = 0x100
}
