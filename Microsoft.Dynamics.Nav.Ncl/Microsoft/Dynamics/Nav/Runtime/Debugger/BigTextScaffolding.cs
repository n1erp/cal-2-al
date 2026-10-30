using System;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class BigTextScaffolding : NavValueScaffolding
{
	protected override bool ProbablyHasChildren => false;

	public BigTextScaffolding(string name, Type type, ValueBinder valueBinder)
		: base(name, type, NavNclType.NavBigText, valueBinder)
	{
	}

	protected override string GetStringValue(NavValue instance)
	{
		NavBigText navBigText = (NavBigText)instance;
		NavBigText navBigText2 = navBigText;
		string substring = ((navBigText2 != null) ? ((string)navBigText2) : string.Empty);
		substring = Scaffolding.TruncateIfNeeded(substring);
		return "'" + substring + "'";
	}
}
