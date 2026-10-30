using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSecurityFiltering : NavEnumBase<SecurityFiltering>
{
	private static NavSecurityFiltering defaultValue = new NavSecurityFiltering(SecurityFiltering.Validated);

	public override NavNclType NclType => NavNclType.NavSecurityFiltering;

	public static NavSecurityFiltering Default => defaultValue;

	public NavSecurityFiltering(SecurityFiltering initValue)
		: base(initValue)
	{
	}

	public NavSecurityFiltering(byte[] value)
		: base((SecurityFiltering)BitConverter.ToInt32(value, 0))
	{
	}
}
