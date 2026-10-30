using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "DT", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum DataType
{
	[EnumMember]
	Integer,
	[EnumMember]
	OemText,
	[EnumMember]
	TextConst,
	[EnumMember]
	Label,
	[EnumMember]
	OemCode,
	[EnumMember]
	Decimal,
	[EnumMember]
	Option,
	[EnumMember]
	Boolean,
	[EnumMember]
	Byte,
	[EnumMember]
	Date,
	[EnumMember]
	Time,
	[EnumMember]
	Binary,
	[EnumMember]
	BLOB,
	[EnumMember]
	DateFormula,
	[EnumMember]
	TableFilter,
	[EnumMember]
	BigInteger,
	[EnumMember]
	Duration,
	[EnumMember]
	GUID,
	[EnumMember]
	RecordID,
	[EnumMember]
	DateTime,
	[EnumMember]
	BigText,
	[EnumMember]
	Text,
	[EnumMember]
	String,
	[EnumMember]
	Code,
	[EnumMember]
	Char,
	[EnumMember]
	Media,
	[EnumMember]
	MediaSet
}
