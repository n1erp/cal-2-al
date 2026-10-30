using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "S", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum Style
{
	[EnumMember]
	None,
	[EnumMember]
	Standard,
	[EnumMember]
	StandardAccent,
	[EnumMember]
	Strong,
	[EnumMember]
	StrongAccent,
	[EnumMember]
	Attention,
	[EnumMember]
	AttentionAccent,
	[EnumMember]
	Favorable,
	[EnumMember]
	Unfavorable,
	[EnumMember]
	Ambiguous,
	[EnumMember]
	Subordinate
}
