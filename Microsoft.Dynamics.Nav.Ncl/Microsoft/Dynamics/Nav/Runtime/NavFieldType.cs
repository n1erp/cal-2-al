using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavFieldType : NavEnumBase<NavType>
{
	private static NavFieldType defaultValue = new NavFieldType(NavType.None);

	public override NavNclType NclType => NavNclType.NavFieldType;

	public static NavFieldType Default => defaultValue;

	public NavFieldType(NavType initValue)
		: base(initValue)
	{
	}

	public NavFieldType(byte[] bytes)
		: base((NavType)BitConverter.ToInt32(bytes, 0))
	{
	}
}
