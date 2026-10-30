using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "ABOT", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum AccessByObjectType
{
	[EnumMember]
	None,
	[EnumMember]
	Page,
	[EnumMember]
	Report,
	[EnumMember]
	Codeunit,
	[EnumMember]
	XMLport,
	[EnumMember]
	TableData,
	[EnumMember]
	Table,
	[EnumMember]
	Query,
	[EnumMember]
	System,
	[EnumMember]
	PageExtension,
	[EnumMember]
	TableExtension
}
