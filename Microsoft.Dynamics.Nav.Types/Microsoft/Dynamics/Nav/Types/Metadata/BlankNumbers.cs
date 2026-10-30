using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "BN", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum BlankNumbers
{
	[EnumMember]
	DontBlank,
	[EnumMember]
	BlankNeg,
	[EnumMember]
	BlankNegAndZero,
	[EnumMember]
	BlankZero,
	[EnumMember]
	BlankZeroAndPos,
	[EnumMember]
	BlankPos
}
