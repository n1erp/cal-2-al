using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedStaticConstructor : MethodSymbol
{
	private readonly NamedTypeSymbol _containingType;

	public override Symbol ContainingSymbol => _containingType;

	public override NamedTypeSymbol ContainingType => _containingType;

	public override string Name => ".cctor";

	internal override bool HasSpecialName => true;

	internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	public override bool IsVararg => false;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	internal override int ParameterCount => 0;

	public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override ImmutableArray<Location> Locations => ContainingType.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override RefKind RefKind => RefKind.None;

	public override TypeSymbol ReturnType => ContainingAssembly.GetSpecialType(SpecialType.System_Void);

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<TypeSymbol> TypeArguments => ImmutableArray<TypeSymbol>.Empty;

	public override Symbol AssociatedSymbol => null;

	public override int Arity => 0;

	public override bool ReturnsVoid => true;

	public override MethodKind MethodKind => MethodKind.StaticConstructor;

	public override bool IsExtern => false;

	public override bool IsSealed => false;

	public override bool IsAbstract => false;

	public override bool IsOverride => false;

	public override bool IsVirtual => false;

	public override bool IsStatic => true;

	public override bool IsAsync => false;

	public override bool HidesBaseMethodsByName => false;

	public override bool IsExtensionMethod => false;

	internal override CallingConvention CallingConvention => CallingConvention.Default;

	internal override bool IsExplicitInterfaceImplementation => false;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public sealed override bool IsImplicitlyDeclared => true;

	internal sealed override bool GenerateDebugInfo => true;

	internal override bool IsMetadataFinal => false;

	internal override bool RequiresSecurityObject => false;

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

	internal override bool HasDeclarativeSecurity => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal SynthesizedStaticConstructor(NamedTypeSymbol containingType)
	{
		_containingType = containingType;
	}

	internal override bool TryGetThisParameter(out ParameterSymbol thisParameter)
	{
		thisParameter = null;
		return true;
	}

	internal override LexicalSortKey GetLexicalSortKey()
	{
		return LexicalSortKey.SynthesizedCCtor;
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal sealed override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	public override DllImportData GetDllImportData()
	{
		return null;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		return ((SourceMemberContainerTypeSymbol)ContainingType).CalculateSyntaxOffsetInSynthesizedConstructor(localPosition, localTree, isStatic: true);
	}
}
