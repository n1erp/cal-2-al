namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class WebServiceTenantTable : WebServiceTable
{
	private WebServiceTenantTable(NavSession session)
		: base(session, 2000000168, 3, 6, 9, 12)
	{
	}

	public static WebServiceTenantTable Create(NavSession session)
	{
		return new WebServiceTenantTable(session);
	}
}
