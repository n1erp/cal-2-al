using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class VersionUtilities
{
	public static string ToMajorMinorString(this Version version)
	{
		return version.ToString(2);
	}
}
