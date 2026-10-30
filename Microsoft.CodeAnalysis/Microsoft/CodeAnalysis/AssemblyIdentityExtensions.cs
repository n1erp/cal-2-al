using System;
using System.Reflection;

namespace Microsoft.CodeAnalysis;

internal static class AssemblyIdentityExtensions
{
	internal static bool IsWindowsComponent(this AssemblyIdentity identity)
	{
		if (identity.ContentType == AssemblyContentType.WindowsRuntime)
		{
			return identity.Name.StartsWith("windows.", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	internal static bool IsWindowsRuntime(this AssemblyIdentity identity)
	{
		if (identity.ContentType == AssemblyContentType.WindowsRuntime)
		{
			return string.Equals(identity.Name, "windows", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}
}
