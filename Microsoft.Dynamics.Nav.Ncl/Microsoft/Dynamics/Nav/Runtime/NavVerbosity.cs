using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavVerbosity : NavEnumBase<ALVerbosity>
{
	public static NavVerbosity Default { get; } = new NavVerbosity(ALVerbosity.Normal);

	public override NavNclType NclType { get; } = NavNclType.NavVerbosity;

	public NavVerbosity(ALVerbosity initValue)
		: base(initValue)
	{
	}

	public NavVerbosity(int initValue)
		: base((ALVerbosity)initValue)
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.Verbosity)
			{
				return Value.CompareTo(((NavVerbosity)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
