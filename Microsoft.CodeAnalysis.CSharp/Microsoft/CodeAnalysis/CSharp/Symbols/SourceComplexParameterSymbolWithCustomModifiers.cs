using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceComplexParameterSymbolWithCustomModifiers : SourceComplexParameterSymbol
{
	private readonly ImmutableArray<CustomModifier> _customModifiers;

	private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

	public override ImmutableArray<CustomModifier> CustomModifiers => _customModifiers;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

	internal SourceComplexParameterSymbolWithCustomModifiers(Symbol owner, int ordinal, TypeSymbol parameterType, RefKind refKind, ImmutableArray<CustomModifier> customModifiers, ImmutableArray<CustomModifier> refCustomModifiers, string name, ImmutableArray<Location> locations, SyntaxReference syntaxRef, ConstantValue defaultSyntaxValue, bool isParams, bool isExtensionMethodThis)
		: base(owner, ordinal, parameterType, refKind, name, locations, syntaxRef, defaultSyntaxValue, isParams, isExtensionMethodThis)
	{
		_customModifiers = customModifiers;
		_refCustomModifiers = refCustomModifiers;
	}
}
