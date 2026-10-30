using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavALErrorTypeEvaluator : NavEnumTypeEvaluator<ALErrorType, NavALErrorType>
{
	internal static readonly NavALErrorTypeEvaluator Instance = new NavALErrorTypeEvaluator();

	private NavALErrorTypeEvaluator()
		: base((Func<ALErrorType, NavALErrorType>)((ALErrorType errorType) => new NavALErrorType(errorType)))
	{
	}

	protected override NavALErrorType GetDefaultValue(INavValueMetadata metadata)
	{
		return NavALErrorType.Default;
	}
}
