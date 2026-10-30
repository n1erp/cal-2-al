using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavWebServiceActionResultCode : NavEnumBase<WebServiceActionResultCode>
{
	private static NavWebServiceActionResultCode defaultValue = new NavWebServiceActionResultCode(WebServiceActionResultCode.None);

	public override NavNclType NclType => NavNclType.NavWebServiceActionResultCode;

	public static NavWebServiceActionResultCode Default => defaultValue;

	public NavWebServiceActionResultCode(WebServiceActionResultCode initValue)
		: base(initValue)
	{
	}

	public NavWebServiceActionResultCode(byte[] value)
		: base((WebServiceActionResultCode)BitConverter.ToInt32(value, 0))
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.WebServiceActionResultCode)
			{
				return Value.CompareTo(((NavWebServiceActionResultCode)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
