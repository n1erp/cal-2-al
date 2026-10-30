using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTestTransactionModelEvaluator : NavEnumTypeEvaluator<TestTransactionModel, NavTestTransactionModel>
{
	internal static NavTestTransactionModelEvaluator Instance { get; } = new NavTestTransactionModelEvaluator();

	private NavTestTransactionModelEvaluator()
		: base((Func<TestTransactionModel, NavTestTransactionModel>)((TestTransactionModel testTransactionModel) => new NavTestTransactionModel(testTransactionModel)))
	{
	}

	protected override NavTestTransactionModel GetDefaultValue(INavValueMetadata metadata)
	{
		return NavTestTransactionModel.Default;
	}
}
