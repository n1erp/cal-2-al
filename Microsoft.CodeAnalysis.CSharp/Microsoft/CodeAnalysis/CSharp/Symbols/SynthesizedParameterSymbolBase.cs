using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedParameterSymbolBase : ParameterSymbol
{
	private readonly MethodSymbol _container;

	private readonly TypeSymbol _type;

	private readonly int _ordinal;

	private readonly string _name;

	private readonly RefKind _refKind;

	public override TypeSymbol Type => _type;

	public override RefKind RefKind => _refKind;

	internal override bool IsMetadataIn => RefKind == RefKind.In;

	internal override bool IsMetadataOut => RefKind == RefKind.Out;

	internal override MarshalPseudoCustomAttributeData MarshallingInformation => null;

	public override string Name => _name;

	public abstract override ImmutableArray<CustomModifier> CustomModifiers { get; }

	public abstract override ImmutableArray<CustomModifier> RefCustomModifiers { get; }

	public override int Ordinal => _ordinal;

	public override bool IsParams => false;

	internal override bool IsMetadataOptional => false;

	public override bool IsImplicitlyDeclared => true;

	internal override ConstantValue ExplicitDefaultConstantValue => null;

	internal override bool IsIDispatchConstant => false;

	internal override bool IsIUnknownConstant => false;

	internal override bool IsCallerLineNumber => false;

	internal override bool IsCallerFilePath => false;

	internal override bool IsCallerMemberName => false;

	public override Symbol ContainingSymbol => _container;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public SynthesizedParameterSymbolBase(MethodSymbol container, TypeSymbol type, int ordinal, RefKind refKind, string name = "")
	{
		_container = container;
		_type = type;
		_ordinal = ordinal;
		_refKind = refKind;
		_name = name;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (Type.ContainsDynamic() && declaringCompilation.HasDynamicEmitAttributes() && declaringCompilation.CanEmitBoolean())
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDynamicAttribute(Type, CustomModifiers.Length + RefCustomModifiers.Length, RefKind));
		}
		if (Type.ContainsTupleNames() && declaringCompilation.HasTupleNamesAttributes && declaringCompilation.CanEmitSpecialType(SpecialType.System_String))
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeTupleNamesAttribute(Type));
		}
		if (RefKind == RefKind.In)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeIsReadOnlyAttribute(this));
		}
	}
}
