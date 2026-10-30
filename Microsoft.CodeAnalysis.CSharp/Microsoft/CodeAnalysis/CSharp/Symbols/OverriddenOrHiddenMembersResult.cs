using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class OverriddenOrHiddenMembersResult
{
	public static readonly OverriddenOrHiddenMembersResult Empty = new OverriddenOrHiddenMembersResult(ImmutableArray<Symbol>.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray<Symbol>.Empty);

	private readonly ImmutableArray<Symbol> _overriddenMembers;

	private readonly ImmutableArray<Symbol> _hiddenMembers;

	private readonly ImmutableArray<Symbol> _runtimeOverriddenMembers;

	public ImmutableArray<Symbol> OverriddenMembers => _overriddenMembers;

	public ImmutableArray<Symbol> HiddenMembers => _hiddenMembers;

	public ImmutableArray<Symbol> RuntimeOverriddenMembers => _runtimeOverriddenMembers;

	private OverriddenOrHiddenMembersResult(ImmutableArray<Symbol> overriddenMembers, ImmutableArray<Symbol> hiddenMembers, ImmutableArray<Symbol> runtimeOverriddenMembers)
	{
		_overriddenMembers = overriddenMembers;
		_hiddenMembers = hiddenMembers;
		_runtimeOverriddenMembers = runtimeOverriddenMembers;
	}

	public static OverriddenOrHiddenMembersResult Create(ImmutableArray<Symbol> overriddenMembers, ImmutableArray<Symbol> hiddenMembers, ImmutableArray<Symbol> runtimeOverriddenMembers)
	{
		if (overriddenMembers.IsEmpty && hiddenMembers.IsEmpty && runtimeOverriddenMembers.IsEmpty)
		{
			return Empty;
		}
		return new OverriddenOrHiddenMembersResult(overriddenMembers, hiddenMembers, runtimeOverriddenMembers);
	}

	internal static Symbol GetOverriddenMember(Symbol substitutedOverridingMember, Symbol overriddenByDefinitionMember)
	{
		if ((object)overriddenByDefinitionMember != null)
		{
			NamedTypeSymbol containingType = overriddenByDefinitionMember.ContainingType;
			NamedTypeSymbol originalDefinition = containingType.OriginalDefinition;
			NamedTypeSymbol baseTypeNoUseSiteDiagnostics = substitutedOverridingMember.ContainingType.BaseTypeNoUseSiteDiagnostics;
			while ((object)baseTypeNoUseSiteDiagnostics != null)
			{
				if (baseTypeNoUseSiteDiagnostics.OriginalDefinition == originalDefinition)
				{
					if (baseTypeNoUseSiteDiagnostics == containingType)
					{
						return overriddenByDefinitionMember;
					}
					return overriddenByDefinitionMember.OriginalDefinition.SymbolAsMember(baseTypeNoUseSiteDiagnostics);
				}
				baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
			}
			throw ExceptionUtilities.Unreachable;
		}
		return null;
	}

	internal Symbol GetOverriddenMember()
	{
		foreach (Symbol overriddenMember in _overriddenMembers)
		{
			if (overriddenMember.IsAbstract || overriddenMember.IsVirtual || overriddenMember.IsOverride)
			{
				return overriddenMember;
			}
		}
		return null;
	}
}
