using System;
using System.Globalization;
using System.IO;

namespace Microsoft.Dynamics.Nav.Types;

public static class ProductApplicationData
{
	private static readonly string RelativePath = string.Format(CultureInfo.InvariantCulture, "Microsoft{0}{1}{0}", Path.DirectorySeparatorChar, "Microsoft Dynamics NAV");

	private const string ServerFolder = "Server";

	private const string KeysFolder = "Server\\Keys";

	private const string WebClientFolder = "Web Client";

	public static string ServerPath => Path.Combine(FullPathWithVersion, "Server");

	public static string KeysPath => Path.Combine(FullPathWithVersion, "Server\\Keys");

	public static string WebClientPath => Path.Combine(FullPathWithVersion, "Web Client");

	private static string FullPathWithVersion
	{
		get
		{
			string fullPath = GetFullPath(Environment.SpecialFolder.CommonApplicationData);
			string path = "14.0".Replace(".", string.Empty);
			return Path.Combine(fullPath, path);
		}
	}

	public static string GetFullPath(Environment.SpecialFolder applicationDataFolder)
	{
		if (!IsValidApplicationFolder(applicationDataFolder))
		{
			throw new ArgumentException("The special folder is not a valid application data folder.");
		}
		return Path.Combine(Environment.GetFolderPath(applicationDataFolder), RelativePath);
	}

	private static bool IsValidApplicationFolder(Environment.SpecialFolder applicationDataFolder)
	{
		if (applicationDataFolder != Environment.SpecialFolder.CommonApplicationData && applicationDataFolder != Environment.SpecialFolder.LocalApplicationData)
		{
			return applicationDataFolder == Environment.SpecialFolder.ApplicationData;
		}
		return true;
	}
}
