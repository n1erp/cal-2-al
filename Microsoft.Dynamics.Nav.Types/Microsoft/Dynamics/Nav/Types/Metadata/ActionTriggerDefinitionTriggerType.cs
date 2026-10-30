using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[XmlType(AnonymousType = true, Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "ATDTT", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum ActionTriggerDefinitionTriggerType
{
	[EnumMember]
	OnAction
}
