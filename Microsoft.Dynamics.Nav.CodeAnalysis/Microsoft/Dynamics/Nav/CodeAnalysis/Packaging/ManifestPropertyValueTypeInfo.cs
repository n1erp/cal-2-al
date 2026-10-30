using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class ManifestPropertyValueTypeInfo
{
	private class EqualityComparerImpl : IEqualityComparer<ManifestPropertyValueTypeInfo>
	{
		public bool Equals(ManifestPropertyValueTypeInfo x, ManifestPropertyValueTypeInfo y)
		{
			if (string.Equals(x.Name, y.Name, StringComparison.Ordinal))
			{
				return string.Equals(x.Property, y.Property, StringComparison.Ordinal);
			}
			return false;
		}

		public int GetHashCode(ManifestPropertyValueTypeInfo obj)
		{
			return Hash.Combine(obj.Name.GetHashCode(), obj.Property.GetHashCode());
		}
	}

	internal static IEqualityComparer<ManifestPropertyValueTypeInfo> EqualityComparer = new EqualityComparerImpl();

	internal string Name { get; }

	internal string Property { get; }

	internal VersionCompatibility Compatibility { get; }

	internal ManifestPropertyValueTypeInfo(string name, string property, VersionCompatibility compatibility = null)
	{
		Name = name;
		Property = property;
		Compatibility = compatibility;
	}
}
