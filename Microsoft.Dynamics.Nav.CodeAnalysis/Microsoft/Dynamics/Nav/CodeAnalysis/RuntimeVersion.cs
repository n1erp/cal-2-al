using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public static class RuntimeVersion
{
	public static readonly Version Spring2018 = new Version(1, 0);

	public static readonly Version Fall2018 = new Version(2, 0);

	public static readonly Version Fall2018CU1 = new Version(2, 1);

	public static readonly Version Fall2018CU2 = new Version(2, 2);

	public static readonly Version Fall2018CU3 = new Version(2, 3);

	public static readonly Version Fall2018CU4 = new Version(2, 4);

	public static readonly Version Spring2019 = new Version(3, 0);

	public static readonly Version Spring2019CU1 = new Version(3, 1);

	public static readonly Version Spring2019CU2 = new Version(3, 2);

	public static readonly Version Latest = Spring2019CU2;

	public static readonly Version CurrentRelease = Spring2019CU2;

	public static readonly HashSet<Version> SupportedVersions = new HashSet<Version> { Spring2018, Fall2018, Fall2018CU1, Fall2018CU2, Fall2018CU3, Fall2018CU4, Spring2019, Spring2019CU1, Spring2019CU2 };

	public static Version Parse(string displayVersion)
	{
		return Version.Parse(displayVersion);
	}

	public static bool TryParse(string displayVersion, out Version result)
	{
		if (Version.TryParse(displayVersion, out result) && result.Build == -1 && result.Revision == -1)
		{
			return true;
		}
		result = null;
		return false;
	}
}
