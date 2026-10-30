namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public static class NavAppXmlSchemas
{
	public static EmbeddedXmlResource SupportedPermissionSets => new EmbeddedXmlResource("XML.PermissionSetsV2.xsd");

	public static EmbeddedXmlResource SupportedTenantWebServices => new EmbeddedXmlResource("XML.TenantWebServicesV1.xsd");

	public static EmbeddedXmlResource SupportedCustomReportLayouts => new EmbeddedXmlResource("XML.CustomReportLayoutsV2.xsd");
}
