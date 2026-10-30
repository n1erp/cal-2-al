using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSessionSettingsEvaluator : NavValueEvaluator<NavSessionSettings>
{
	private static readonly NavSessionSettingsEvaluator instance = new NavSessionSettingsEvaluator();

	internal static NavSessionSettingsEvaluator Instance => instance;

	private NavSessionSettingsEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavSessionSettings value, INavValueMetadata metadata, string source, int number)
	{
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		value = new NavSessionSettings(source);
		return true;
	}

	protected override NavSessionSettings GetDefaultValue(INavValueMetadata metadata)
	{
		return NavSessionSettings.Default;
	}
}
