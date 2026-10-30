using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncValidatorFactory
{
	private static readonly List<ISyncValidator> allValidators = new List<ISyncValidator>
	{
		new SyncTableValidator(),
		new SyncFieldValidator(),
		new SyncKeyValidator()
	};

	public static IEnumerable<ISyncValidator> AllValidators => allValidators;
}
