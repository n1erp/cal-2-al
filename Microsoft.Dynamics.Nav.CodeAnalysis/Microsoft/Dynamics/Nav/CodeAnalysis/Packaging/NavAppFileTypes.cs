using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

[Flags]
public enum NavAppFileTypes
{
	None = 0,
	NavObjectSourceFile = 1,
	PermissionSet = 2,
	TenantWebService = 4,
	CustomReportLayout = 8,
	TableData = 0x10,
	TranslationFile = 0x20,
	Navigation = 0x40
}
