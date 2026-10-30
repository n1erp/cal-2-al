using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedAccessorValueParameterSymbol : SourceComplexParameterSymbol
{
	private readonly ImmutableArray<CustomModifier> _customModifiers;

	public override ImmutableArray<CustomModifier> CustomModifiers => _customModifiers;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override bool IsImplicitlyDeclared => true;

	protected override IAttributeTargetSymbol AttributeOwner => (SourceMemberMethodSymbol)ContainingSymbol;

	public SynthesizedAccessorValueParameterSymbol(SourceMemberMethodSymbol accessor, TypeSymbol paramType, int ordinal, ImmutableArray<CustomModifier> customModifiers)
		: base(accessor, ordinal, paramType, RefKind.None, "value", accessor.Locations, null, ConstantValue.Unset, isParams: false, isExtensionMethodThis: false)
	{
		_customModifiers = customModifiers;
	}

	internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return ((SourceMemberMethodSymbol)ContainingSymbol).GetAttributeDeclarations();
	}
}
