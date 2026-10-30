using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavVersionEvaluator : NavValueEvaluator<NavVersion>
{
	internal static NavVersionEvaluator Instance { get; } = new NavVersionEvaluator();

	protected override bool InternalEvaluate(DataError dataError, out NavVersion value, INavValueMetadata metadata, string source, int number)
	{
		value = GetDefaultValue(metadata);
		if (Version.TryParse(source, out var result))
		{
			value = new NavVersion(result);
			return true;
		}
		if (dataError == DataError.TrapError)
		{
			return false;
		}
		throw NavNCLEvaluateException.Create(CultureInfo.CurrentCulture, NavType.Version, source);
	}
}
