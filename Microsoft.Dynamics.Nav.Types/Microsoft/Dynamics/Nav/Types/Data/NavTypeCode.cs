using System;

namespace Microsoft.Dynamics.Nav.Types.Data;

[Serializable]
public enum NavTypeCode : byte
{
	Object,
	DBNull,
	Boolean,
	Char,
	SByte,
	Byte,
	Int16,
	UInt16,
	Int32,
	UInt32,
	Int64,
	UInt64,
	Single,
	Double,
	Decimal,
	DateTime,
	TimeSpan,
	String,
	Guid,
	ByteArray,
	Media,
	MediaSet
}
