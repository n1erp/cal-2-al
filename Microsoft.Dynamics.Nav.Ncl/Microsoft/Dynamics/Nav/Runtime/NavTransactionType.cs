using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTransactionType : NavEnumBase<TransactionType>
{
	private static NavTransactionType defaultValue = new NavTransactionType(TransactionType.UpdateNoLocks);

	public override NavNclType NclType => NavNclType.NavTransactionType;

	public static NavTransactionType Default => defaultValue;

	public NavTransactionType(TransactionType initValue)
		: base(initValue)
	{
	}

	public NavTransactionType(byte[] bytes)
		: base((TransactionType)BitConverter.ToInt32(bytes, 0))
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.TransactionType)
			{
				return Value.CompareTo(((NavTransactionType)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
