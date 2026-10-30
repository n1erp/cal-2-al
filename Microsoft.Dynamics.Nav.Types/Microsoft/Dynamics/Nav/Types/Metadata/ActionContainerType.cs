using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "ACT", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum ActionContainerType
{
	[EnumMember]
	NewDocumentItems,
	[EnumMember]
	ActionItems,
	[EnumMember]
	RelatedInformation,
	[EnumMember]
	Reports,
	[EnumMember]
	HomeItems,
	[EnumMember]
	ActivityButtons,
	[EnumMember]
	Departments,
	[EnumMember]
	AutoQueryActions,
	[EnumMember]
	ViewActions
}
