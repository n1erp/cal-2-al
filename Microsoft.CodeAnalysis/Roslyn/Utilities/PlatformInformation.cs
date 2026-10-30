using System;
using System.IO;

namespace Roslyn.Utilities;

internal static class PlatformInformation
{
	public static bool IsWindows => Path.DirectorySeparatorChar == '\\';

	public static bool IsUnix => Path.DirectorySeparatorChar == '/';

	public static bool IsRunningOnMono
	{
		get
		{
			try
			{
				return (object)Type.GetType("Mono.Runtime") != null;
			}
			catch
			{
				return false;
			}
		}
	}
}
