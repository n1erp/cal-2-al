using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavEnumTextEncoding : NavEnumBase<TextEncoding>
{
	private static readonly NavEnumTextEncoding DefaultValue = new NavEnumTextEncoding(TextEncoding.MSDos);

	public override NavNclType NclType => NavNclType.NavTextEncoding;

	public static NavEnumTextEncoding Default => DefaultValue;

	public NavEnumTextEncoding(TextEncoding initValue)
		: base(initValue)
	{
	}

	public NavEnumTextEncoding(int initValue)
		: base((TextEncoding)initValue)
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.TextEncoding)
			{
				return Value.CompareTo(((NavEnumTextEncoding)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
