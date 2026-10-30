using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Mono.Cecil;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class DotNetUtilities
{
	private const string NullPublicKeyTokenLiteral = "null";

	internal static string GetAssemblyName(string name, string version, string culture, string publicKeyToken)
	{
		DebugAssertHelper.Assert(name != null);
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		instance.Builder.Append(name);
		if (!string.IsNullOrEmpty(version))
		{
			instance.Builder.Append(", Version=");
			instance.Builder.Append(version);
		}
		if (culture != null)
		{
			instance.Builder.Append(", Culture=");
			if (string.IsNullOrWhiteSpace(culture))
			{
				instance.Builder.Append("neutral");
			}
			else
			{
				instance.Builder.Append(culture);
			}
		}
		instance.Builder.Append(", PublicKeyToken=");
		if (!string.IsNullOrWhiteSpace(publicKeyToken))
		{
			instance.Builder.Append(publicKeyToken);
		}
		else
		{
			instance.Builder.Append("null");
		}
		return instance.ToStringAndFree();
	}

	internal static string GetNormalizedCultureName(string cultureName)
	{
		if (string.IsNullOrWhiteSpace(cultureName))
		{
			return "neutral";
		}
		return cultureName;
	}

	[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
	internal static string GetPublicKeyToken(byte[] publicKeyToken)
	{
		if (publicKeyToken == null || publicKeyToken.Length == 0)
		{
			return "null";
		}
		return BitConverter.ToString(publicKeyToken).Replace("-", string.Empty).ToLowerInvariant();
	}

	internal static string GetAssemblyQualifiedTypeName(DotNetAssemblyInfo assembly, string fullTypeName)
	{
		return fullTypeName + ", " + assembly.FullName;
	}

	internal static string GetAssemblyQualifiedTypeName(AssemblyDefinition assembly, string fullTypeName)
	{
		return fullTypeName + ", " + assembly.FullName;
	}

	internal static string GetAssemblyQualifiedTypeName(Assembly assembly, string fullTypeName)
	{
		return fullTypeName + ", " + assembly.FullName;
	}

	internal static string GetConstructorNameFromTypeName(string typeName)
	{
		string text = typeName;
		int num = text.IndexOf('`');
		if (num != -1)
		{
			text = text.Substring(0, num);
		}
		int num2 = text.LastIndexOf('.');
		if (num2 != -1)
		{
			text = text.Substring(num2 + 1);
		}
		return text;
	}

	internal static Dictionary<string, string> GetLocationOfLoadedAssemblies(AppDomain domain, bool cachePathsToAssembliesInAppDomain)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (!cachePathsToAssembliesInAppDomain)
		{
			Assembly assembly = typeof(object).Assembly;
			dictionary.Add(assembly.GetName().Name, assembly.Location);
			return dictionary;
		}
		foreach (Assembly item in from p in domain.GetAssemblies()
			where !p.IsDynamic
			select p)
		{
			string name = item.GetName().Name;
			if (!dictionary.ContainsKey(name))
			{
				dictionary.Add(name, item.Location);
			}
		}
		return dictionary;
	}
}
