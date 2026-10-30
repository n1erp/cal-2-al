using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

internal struct AnonymousTypeKeyField(string name, bool isKey, bool ignoreCase) : IEquatable<AnonymousTypeKeyField>
{
	internal readonly string Name = name;

	internal readonly bool IsKey = isKey;

	internal readonly bool IgnoreCase = ignoreCase;

	public bool Equals(AnonymousTypeKeyField other)
	{
		if (IsKey == other.IsKey && IgnoreCase == other.IgnoreCase)
		{
			return (IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal).Equals(Name, other.Name);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals((AnonymousTypeKeyField)obj);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(IsKey, Hash.Combine(IgnoreCase, (IgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal).GetHashCode(Name)));
	}
}
