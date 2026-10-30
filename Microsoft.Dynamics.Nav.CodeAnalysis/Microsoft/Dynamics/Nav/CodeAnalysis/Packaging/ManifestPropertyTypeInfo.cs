using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class ManifestPropertyTypeInfo : IEquatable<ManifestPropertyTypeInfo>
{
	internal string Name { get; }

	internal string ParentPropertyName { get; }

	internal VersionCompatibility Compatibility { get; }

	internal bool IsRequired { get; }

	internal bool IsArray { get; }

	internal ManifestPropertyTypeInfo(string name, string parentPropertyName = null, VersionCompatibility compatibility = null, bool isRequired = false, bool isArray = false)
	{
		Name = name;
		ParentPropertyName = parentPropertyName;
		Compatibility = compatibility;
		IsRequired = isRequired;
		IsArray = isArray;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ManifestPropertyTypeInfo other))
		{
			return false;
		}
		return Equals(other);
	}

	public bool Equals(ManifestPropertyTypeInfo other)
	{
		return string.Equals(Name, other?.Name, StringComparison.Ordinal);
	}

	public override int GetHashCode()
	{
		return Name.GetHashCode();
	}
}
