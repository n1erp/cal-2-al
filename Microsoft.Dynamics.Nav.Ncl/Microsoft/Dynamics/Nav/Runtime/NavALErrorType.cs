using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavALErrorType : NavEnumBase<ALErrorType>
{
	private static NavALErrorType defaultValue = new NavALErrorType(ALErrorType.Client);

	public override NavNclType NclType => NavNclType.NavALErrorType;

	public static NavALErrorType Default => defaultValue;

	public NavALErrorType(ALErrorType initValue)
		: base(initValue)
	{
	}

	public NavALErrorType(int initValue)
		: base((ALErrorType)initValue)
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.ALErrorType)
			{
				return Value.CompareTo(((NavALErrorType)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
