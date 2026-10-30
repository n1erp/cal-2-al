using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDataClassificationEvaluator : NavEnumTypeEvaluator<DataClassification, NavDataClassification>
{
	private static readonly NavDataClassificationEvaluator instance = new NavDataClassificationEvaluator();

	internal static NavDataClassificationEvaluator Instance => instance;

	private NavDataClassificationEvaluator()
		: base((Func<DataClassification, NavDataClassification>)((DataClassification dataClassification) => new NavDataClassification(dataClassification)))
	{
	}

	protected override NavDataClassification GetDefaultValue(INavValueMetadata metadata)
	{
		return NavDataClassification.Default;
	}
}
