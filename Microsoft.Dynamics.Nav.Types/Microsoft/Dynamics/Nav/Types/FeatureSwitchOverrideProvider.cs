using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Types;

internal class FeatureSwitchOverrideProvider
{
	public static IEnumerable<KeyValuePair<string, bool>> OverrideValues
	{
		get
		{
			if (!string.IsNullOrEmpty(ServerUserSettings.Instance?.FeatureSwitchOverrides?.Value))
			{
				return from fs in ServerUserSettings.Instance.FeatureSwitchOverrides.Value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					select new KeyValuePair<string, bool>(fs, value: true);
			}
			return null;
		}
	}
}
