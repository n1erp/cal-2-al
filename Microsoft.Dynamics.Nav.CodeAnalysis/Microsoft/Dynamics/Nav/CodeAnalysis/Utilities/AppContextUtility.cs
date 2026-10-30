using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal class AppContextUtility
{
	public static string GetBaseDirectory()
	{
		return AppDomain.CurrentDomain.BaseDirectory;
	}
}
