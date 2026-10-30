using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SubstitutedEventSymbol : WrappedEventSymbol
{
	private readonly SubstitutedNamedTypeSymbol _containingType;

	private TypeSymbol _lazyType;

	private ImmutableArray<EventSymbol> _lazyExplicitInterfaceImplementations;

	private OverriddenOrHiddenMembersResult _lazyOverriddenOrHiddenMembers;

	public override TypeSymbol Type
	{
		get
		{
			if ((object)_lazyType == null)
			{
				Interlocked.CompareExchange(ref _lazyType, _containingType.TypeSubstitution.SubstituteTypeWithTupleUnification(OriginalDefinition.Type).Type, null);
			}
			return _lazyType;
		}
	}

	public override Symbol ContainingSymbol => _containingType;

	public override EventSymbol OriginalDefinition => _underlyingEvent;

	public override MethodSymbol AddMethod => OriginalDefinition.AddMethod?.AsMember(_containingType);

	public override MethodSymbol RemoveMethod => OriginalDefinition.RemoveMethod?.AsMember(_containingType);

	internal override FieldSymbol AssociatedField => OriginalDefinition.AssociatedField?.AsMember(_containingType);

	internal override bool IsExplicitInterfaceImplementation => OriginalDefinition.IsExplicitInterfaceImplementation;

	public override ImmutableArray<EventSymbol> ExplicitInterfaceImplementations
	{
		get
		{
			if (_lazyExplicitInterfaceImplementations.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _lazyExplicitInterfaceImplementations, ExplicitInterfaceHelpers.SubstituteExplicitInterfaceImplementations(OriginalDefinition.ExplicitInterfaceImplementations, _containingType.TypeSubstitution), default);
			}
			return _lazyExplicitInterfaceImplementations;
		}
	}

	internal override bool MustCallMethodsDirectly => OriginalDefinition.MustCallMethodsDirectly;

	internal override OverriddenOrHiddenMembersResult OverriddenOrHiddenMembers
	{
		get
		{
			if (_lazyOverriddenOrHiddenMembers == null)
			{
				Interlocked.CompareExchange(ref _lazyOverriddenOrHiddenMembers, this.MakeOverriddenOrHiddenMembers(), null);
			}
			return _lazyOverriddenOrHiddenMembers;
		}
	}

	public override bool IsWindowsRuntimeEvent => OriginalDefinition.IsWindowsRuntimeEvent;

	internal SubstitutedEventSymbol(SubstitutedNamedTypeSymbol containingType, EventSymbol originalDefinition)
		: base(originalDefinition)
	{
		_containingType = containingType;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return OriginalDefinition.GetAttributes();
	}
}
