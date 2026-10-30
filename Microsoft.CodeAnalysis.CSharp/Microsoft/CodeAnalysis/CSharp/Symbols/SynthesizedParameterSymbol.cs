using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedParameterSymbol : SynthesizedParameterSymbolBase
{
	private sealed class SynthesizedParameterSymbolWithCustomModifiers : SynthesizedParameterSymbolBase
	{
		private readonly ImmutableArray<CustomModifier> _customModifiers;

		private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

		public override ImmutableArray<CustomModifier> CustomModifiers => _customModifiers;

		public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

		public SynthesizedParameterSymbolWithCustomModifiers(MethodSymbol container, TypeSymbol type, int ordinal, RefKind refKind, string name, ImmutableArray<CustomModifier> customModifiers, ImmutableArray<CustomModifier> refCustomModifiers)
			: base(container, type, ordinal, refKind, name)
		{
			_customModifiers = customModifiers.NullToEmpty();
			_refCustomModifiers = refCustomModifiers.NullToEmpty();
		}
	}

	public override ImmutableArray<CustomModifier> CustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	private SynthesizedParameterSymbol(MethodSymbol container, TypeSymbol type, int ordinal, RefKind refKind, string name)
		: base(container, type, ordinal, refKind, name)
	{
	}

	public static ParameterSymbol Create(MethodSymbol container, TypeSymbol type, int ordinal, RefKind refKind, string name = "", ImmutableArray<CustomModifier> customModifiers = default(ImmutableArray<CustomModifier>), ImmutableArray<CustomModifier> refCustomModifiers = default(ImmutableArray<CustomModifier>))
	{
		if (customModifiers.IsDefaultOrEmpty && refCustomModifiers.IsDefaultOrEmpty)
		{
			return new SynthesizedParameterSymbol(container, type, ordinal, refKind, name);
		}
		return new SynthesizedParameterSymbolWithCustomModifiers(container, type, ordinal, refKind, name, customModifiers, refCustomModifiers);
	}

	internal static ImmutableArray<ParameterSymbol> DeriveParameters(MethodSymbol sourceMethod, MethodSymbol destinationMethod)
	{
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance();
		foreach (ParameterSymbol parameter in sourceMethod.Parameters)
		{
			instance.Add(Create(destinationMethod, parameter.Type, parameter.Ordinal, parameter.RefKind, parameter.Name, parameter.CustomModifiers, parameter.RefCustomModifiers));
		}
		return instance.ToImmutableAndFree();
	}
}
