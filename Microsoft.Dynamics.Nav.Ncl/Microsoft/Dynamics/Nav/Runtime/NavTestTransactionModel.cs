using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTestTransactionModel : NavEnumBase<TestTransactionModel>
{
	public static NavTestTransactionModel Default { get; } = new NavTestTransactionModel(TestTransactionModel.AutoCommit);

	public override NavNclType NclType { get; } = NavNclType.NavTestTransactionModel;

	public NavTestTransactionModel(TestTransactionModel initValue)
		: base(initValue)
	{
	}

	public NavTestTransactionModel(int initValue)
		: base((TestTransactionModel)initValue)
	{
	}

	public override int CompareTo(NavValue other)
	{
		if (other != null)
		{
			NavType navType = other.NavType;
			if (navType == NavType.TestTransactionModel)
			{
				return Value.CompareTo(((NavTestTransactionModel)other).Value);
			}
		}
		ThrowUnableToCompareException(other);
		return 0;
	}
}
