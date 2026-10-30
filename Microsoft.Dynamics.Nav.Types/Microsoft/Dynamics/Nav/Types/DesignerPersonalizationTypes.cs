using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[Flags]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "DPT", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum DesignerPersonalizationTypes
{
	[EnumMember]
	None = 0,
	[EnumMember]
	Controls = 1,
	[EnumMember]
	Actions = 2,
	[EnumMember]
	Views = 4,
	[EnumMember]
	ViewControls = 8
}
