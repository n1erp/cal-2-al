using System;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavFieldClassEvaluator : NavEnumTypeEvaluator<FieldClass, NavFieldClass>
{
	internal static NavFieldClassEvaluator Instance { get; } = new NavFieldClassEvaluator();

	private NavFieldClassEvaluator()
		: base((Func<FieldClass, NavFieldClass>)((FieldClass FieldClass) => new NavFieldClass(FieldClass)))
	{
	}

	protected override NavFieldClass GetDefaultValue(INavValueMetadata metadata)
	{
		return NavFieldClass.Default;
	}
}
