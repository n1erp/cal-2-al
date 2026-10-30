using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavVerbosityEvaluator : NavEnumTypeEvaluator<ALVerbosity, NavVerbosity>
{
	internal static NavVerbosityEvaluator Instance { get; } = new NavVerbosityEvaluator();

	private NavVerbosityEvaluator()
		: base((Func<ALVerbosity, NavVerbosity>)((ALVerbosity Verbosity) => new NavVerbosity(Verbosity)))
	{
	}

	protected override NavVerbosity GetDefaultValue(INavValueMetadata metadata)
	{
		return NavVerbosity.Default;
	}
}
