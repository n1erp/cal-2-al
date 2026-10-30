using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "SET", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum SourceExpressionType
{
	[EnumMember]
	SourceExpression,
	[EnumMember]
	DataItem,
	[EnumMember]
	DynamicCaption,
	[EnumMember]
	Property,
	[EnumMember]
	DataCaption,
	[EnumMember]
	VariableIncludedInDataSet,
	[EnumMember]
	DynamicCueStyle
}
