using System;

namespace Microsoft.CodeAnalysis;

public class AssemblyIdentityComparer
{
	public enum ComparisonResult
	{
		NotEquivalent,
		Equivalent,
		EquivalentIgnoringVersion
	}

	public static AssemblyIdentityComparer Default { get; } = new AssemblyIdentityComparer();

	public static StringComparer SimpleNameComparer => StringComparer.OrdinalIgnoreCase;

	public static StringComparer CultureComparer => StringComparer.OrdinalIgnoreCase;

	internal AssemblyIdentityComparer()
	{
	}

	public bool ReferenceMatchesDefinition(string referenceDisplayName, AssemblyIdentity definition)
	{
		bool unificationApplied;
		return Compare(null, referenceDisplayName, definition, out unificationApplied, ignoreVersion: false) != ComparisonResult.NotEquivalent;
	}

	public bool ReferenceMatchesDefinition(AssemblyIdentity reference, AssemblyIdentity definition)
	{
		bool unificationApplied;
		return Compare(reference, null, definition, out unificationApplied, ignoreVersion: false) != ComparisonResult.NotEquivalent;
	}

	public ComparisonResult Compare(AssemblyIdentity reference, AssemblyIdentity definition)
	{
		bool unificationApplied;
		return Compare(reference, null, definition, out unificationApplied, ignoreVersion: true);
	}

	internal ComparisonResult Compare(AssemblyIdentity reference, string referenceDisplayName, AssemblyIdentity definition, out bool unificationApplied, bool ignoreVersion)
	{
		unificationApplied = false;
		AssemblyIdentityParts parts;
		if (reference != null)
		{
			bool? flag = TriviallyEquivalent(reference, definition);
			if (flag.HasValue)
			{
				if (!flag.Value)
				{
					return ComparisonResult.NotEquivalent;
				}
				return ComparisonResult.Equivalent;
			}
			parts = AssemblyIdentityParts.Version | AssemblyIdentityParts.Name | AssemblyIdentityParts.Culture | AssemblyIdentityParts.PublicKeyToken;
		}
		else if (!AssemblyIdentity.TryParseDisplayName(referenceDisplayName, out reference, out parts) || reference.ContentType != definition.ContentType)
		{
			return ComparisonResult.NotEquivalent;
		}
		if (!ApplyUnificationPolicies(ref reference, ref definition, parts, out var isDefinitionFxAssembly))
		{
			return ComparisonResult.NotEquivalent;
		}
		if ((object)reference == definition)
		{
			return ComparisonResult.Equivalent;
		}
		bool flag2 = (parts & AssemblyIdentityParts.Culture) != 0;
		bool flag3 = (parts & AssemblyIdentityParts.PublicKeyOrToken) != 0;
		if (!definition.IsStrongName)
		{
			if (reference.IsStrongName)
			{
				return ComparisonResult.NotEquivalent;
			}
			if (!AssemblyIdentity.IsFullName(parts))
			{
				if (!SimpleNameComparer.Equals(reference.Name, definition.Name))
				{
					return ComparisonResult.NotEquivalent;
				}
				if (flag2 && !CultureComparer.Equals(reference.CultureName, definition.CultureName))
				{
					return ComparisonResult.NotEquivalent;
				}
				return ComparisonResult.Equivalent;
			}
			isDefinitionFxAssembly = false;
		}
		if (!SimpleNameComparer.Equals(reference.Name, definition.Name))
		{
			return ComparisonResult.NotEquivalent;
		}
		if (flag2 && !CultureComparer.Equals(reference.CultureName, definition.CultureName))
		{
			return ComparisonResult.NotEquivalent;
		}
		if (flag3 && !AssemblyIdentity.KeysEqual(reference, definition))
		{
			return ComparisonResult.NotEquivalent;
		}
		bool flag4 = (parts & AssemblyIdentityParts.Version) != 0;
		bool flag5 = (parts & AssemblyIdentityParts.Version) != AssemblyIdentityParts.Version;
		if ((definition.IsStrongName & flag4) && (flag5 || reference.Version != definition.Version))
		{
			if (isDefinitionFxAssembly)
			{
				unificationApplied = true;
				return ComparisonResult.Equivalent;
			}
			if (ignoreVersion)
			{
				return ComparisonResult.EquivalentIgnoringVersion;
			}
			return ComparisonResult.NotEquivalent;
		}
		return ComparisonResult.Equivalent;
	}

	private static bool? TriviallyEquivalent(AssemblyIdentity x, AssemblyIdentity y)
	{
		if (x.ContentType != y.ContentType)
		{
			return false;
		}
		if (x.IsRetargetable || y.IsRetargetable)
		{
			return null;
		}
		return AssemblyIdentity.MemberwiseEqual(x, y);
	}

	internal virtual bool ApplyUnificationPolicies(ref AssemblyIdentity reference, ref AssemblyIdentity definition, AssemblyIdentityParts referenceParts, out bool isDefinitionFxAssembly)
	{
		isDefinitionFxAssembly = false;
		return true;
	}
}
