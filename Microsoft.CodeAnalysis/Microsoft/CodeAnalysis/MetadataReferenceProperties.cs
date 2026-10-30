using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public struct MetadataReferenceProperties : IEquatable<MetadataReferenceProperties>
{
	private readonly MetadataImageKind _kind;

	private readonly ImmutableArray<string> _aliases;

	private readonly bool _embedInteropTypes;

	public static MetadataReferenceProperties Module => new MetadataReferenceProperties(MetadataImageKind.Module);

	public static MetadataReferenceProperties Assembly => new MetadataReferenceProperties(MetadataImageKind.Assembly, default, false);

	public MetadataImageKind Kind => _kind;

	public static string GlobalAlias => "global";

	public ImmutableArray<string> Aliases => _aliases.NullToEmpty();

	public bool EmbedInteropTypes => _embedInteropTypes;

	internal bool HasRecursiveAliases { get; private set; }

	public MetadataReferenceProperties(MetadataImageKind kind = MetadataImageKind.Assembly, ImmutableArray<string> aliases = default(ImmutableArray<string>), bool embedInteropTypes = false)
	{
		if (!kind.IsValid())
		{
			throw new ArgumentOutOfRangeException("kind");
		}
		if (kind == MetadataImageKind.Module)
		{
			if (embedInteropTypes)
			{
				throw new ArgumentException(CodeAnalysisResources.CannotEmbedInteropTypesFromModule, "embedInteropTypes");
			}
			if (!aliases.IsDefaultOrEmpty)
			{
				throw new ArgumentException(CodeAnalysisResources.CannotAliasModule, "aliases");
			}
		}
		if (!aliases.IsDefaultOrEmpty)
		{
			foreach (string item in aliases)
			{
				if (!item.IsValidClrTypeName())
				{
					throw new ArgumentException(CodeAnalysisResources.InvalidAlias, "aliases");
				}
			}
		}
		_kind = kind;
		_aliases = aliases;
		_embedInteropTypes = embedInteropTypes;
		HasRecursiveAliases = false;
	}

	internal MetadataReferenceProperties(MetadataImageKind kind, ImmutableArray<string> aliases, bool embedInteropTypes, bool hasRecursiveAliases)
		: this(kind, aliases, embedInteropTypes)
	{
		HasRecursiveAliases = hasRecursiveAliases;
	}

	public MetadataReferenceProperties WithAliases(IEnumerable<string> aliases)
	{
		return WithAliases(aliases.AsImmutableOrEmpty());
	}

	public MetadataReferenceProperties WithAliases(ImmutableArray<string> aliases)
	{
		return new MetadataReferenceProperties(_kind, aliases, _embedInteropTypes, HasRecursiveAliases);
	}

	public MetadataReferenceProperties WithEmbedInteropTypes(bool embedInteropTypes)
	{
		return new MetadataReferenceProperties(_kind, _aliases, embedInteropTypes, HasRecursiveAliases);
	}

	internal MetadataReferenceProperties WithRecursiveAliases(bool value)
	{
		return new MetadataReferenceProperties(_kind, _aliases, _embedInteropTypes, value);
	}

	public override bool Equals(object obj)
	{
		if (obj is MetadataReferenceProperties)
		{
			return Equals((MetadataReferenceProperties)obj);
		}
		return false;
	}

	public bool Equals(MetadataReferenceProperties other)
	{
		if (Aliases.SequenceEqual(other.Aliases) && _embedInteropTypes == other._embedInteropTypes && _kind == other._kind)
		{
			return HasRecursiveAliases == other.HasRecursiveAliases;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Hash.CombineValues(Aliases), Hash.Combine(_embedInteropTypes, Hash.Combine(HasRecursiveAliases, _kind.GetHashCode())));
	}

	public static bool operator ==(MetadataReferenceProperties left, MetadataReferenceProperties right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(MetadataReferenceProperties left, MetadataReferenceProperties right)
	{
		return !left.Equals(right);
	}
}
