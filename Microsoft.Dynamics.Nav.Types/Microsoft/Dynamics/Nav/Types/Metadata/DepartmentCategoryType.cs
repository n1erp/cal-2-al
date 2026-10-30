using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "DCT", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum DepartmentCategoryType
{
	[EnumMember]
	None,
	[EnumMember]
	Lists,
	[EnumMember]
	Tasks,
	[XmlEnum("Reports and Analysis")]
	[EnumMember]
	ReportsandAnalysis,
	[EnumMember]
	Documents,
	[EnumMember]
	History,
	[EnumMember]
	Administration
}
