using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavFormResultEvaluator : NavEnumTypeEvaluator<FormResult, NavFormResult>
{
	private static readonly NavFormResultEvaluator instance = new NavFormResultEvaluator();

	internal static NavFormResultEvaluator Instance => instance;

	private NavFormResultEvaluator()
		: base((Func<FormResult, NavFormResult>)((FormResult formResult) => new NavFormResult(formResult)))
	{
	}

	protected override NavFormResult GetDefaultValue(INavValueMetadata metadata)
	{
		return NavFormResult.Default;
	}
}
