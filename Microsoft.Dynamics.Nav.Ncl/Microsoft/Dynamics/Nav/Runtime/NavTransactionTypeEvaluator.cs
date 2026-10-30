using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTransactionTypeEvaluator : NavEnumTypeEvaluator<TransactionType, NavTransactionType>
{
	private static readonly NavTransactionTypeEvaluator instance = new NavTransactionTypeEvaluator();

	internal static NavTransactionTypeEvaluator Instance => instance;

	private NavTransactionTypeEvaluator()
		: base((Func<TransactionType, NavTransactionType>)((TransactionType transactionType) => new NavTransactionType(transactionType)))
	{
	}

	protected override NavTransactionType GetDefaultValue(INavValueMetadata metadata)
	{
		return NavTransactionType.Default;
	}
}
