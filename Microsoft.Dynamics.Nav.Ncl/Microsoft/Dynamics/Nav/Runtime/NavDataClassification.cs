using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDataClassification : NavEnumBase<DataClassification>
{
	private static NavDataClassification defaultValue = new NavDataClassification(DataClassification.ToBeClassified);

	public override NavNclType NclType => NavNclType.NavDataClassification;

	public static NavDataClassification Default => defaultValue;

	public NavDataClassification(DataClassification initValue)
		: base(initValue)
	{
	}

	public NavDataClassification(int initValue)
		: base((DataClassification)initValue)
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.DataClassification)
			{
				return Value.CompareTo(((NavDataClassification)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
