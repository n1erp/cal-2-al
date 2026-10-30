using System;
using System.Reflection;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

public static class VersionCompatibility
{
	private static Lazy<string> assemblyFileVersionName = new Lazy<string>(() => AssemblyFileVersion.ToString());

	public const int CompatibilityVersionNumber = 14002;

	public static Version AssemblyFileVersion
	{
		get
		{
			Assembly assembly = typeof(ElementDefinition).Assembly;
			object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				return new Version(((AssemblyFileVersionAttribute)customAttributes[0]).Version);
			}
			return assembly.GetName().Version;
		}
	}

	public static string AssemblyFileVersionName => assemblyFileVersionName.Value;

	public static bool IsVersionCompatible(int versionNumber)
	{
		return versionNumber.Equals(14002);
	}

	public static bool IsAssemblyCompatible(Version assemblyVersion)
	{
		if (!(assemblyVersion != null))
		{
			return false;
		}
		return assemblyVersion.CompareTo(AssemblyFileVersion) == 0;
	}
}
