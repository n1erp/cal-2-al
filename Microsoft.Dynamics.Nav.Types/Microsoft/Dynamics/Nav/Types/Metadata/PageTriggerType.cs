using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

[Serializable]
[GeneratedCode("Microsoft.Dynamics.Nav.Tools.MetaTypesGenerator", "10.0.0.0")]
[XmlType(Namespace = "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects")]
[DataContract(Name = "PTT", Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public enum PageTriggerType
{
	[EnumMember]
	OnInit,
	[EnumMember]
	OnOpenPage,
	[EnumMember]
	OnClosePage,
	[EnumMember]
	OnFindRecord,
	[EnumMember]
	OnNextRecord,
	[EnumMember]
	OnAfterGetRecord,
	[EnumMember]
	OnNewRecord,
	[EnumMember]
	OnInsertRecord,
	[EnumMember]
	OnModifyRecord,
	[EnumMember]
	OnDeleteRecord,
	[EnumMember]
	OnQueryClosePage,
	[EnumMember]
	OnAfterGetCurrRecord,
	[EnumMember]
	OnPageBackgroundTaskCompleted,
	[EnumMember]
	OnPageBackgroundTaskError
}
