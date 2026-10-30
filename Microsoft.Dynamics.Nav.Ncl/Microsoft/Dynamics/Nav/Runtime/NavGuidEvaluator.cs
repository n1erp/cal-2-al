using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavGuidEvaluator : NavValueEvaluator<NavGuid>
{
	private static readonly NavGuidEvaluator instance = new NavGuidEvaluator();

	internal static NavGuidEvaluator Instance => instance;

	private NavGuidEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavGuid value, INavValueMetadata metadata, string source, int number)
	{
		value = new NavGuid(source);
		return true;
	}

	internal bool Evaluate(DataError dataError, out Guid value, string source, int number)
	{
		bool result = Evaluate(dataError, out var value2, null, source, number);
		value = value2.Value;
		return result;
	}

	protected override NavGuid GetDefaultValue(INavValueMetadata metadata)
	{
		return NavGuid.Default;
	}
}
