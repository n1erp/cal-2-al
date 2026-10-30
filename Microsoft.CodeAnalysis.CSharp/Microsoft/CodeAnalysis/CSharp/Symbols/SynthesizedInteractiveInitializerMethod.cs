using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedInteractiveInitializerMethod : SynthesizedInstanceMethodSymbol
{
	internal const string InitializerName = "<Initialize>";

	private readonly SourceMemberContainerTypeSymbol _containingType;

	private readonly TypeSymbol _resultType;

	private readonly TypeSymbol _returnType;

	public override string Name => "<Initialize>";

	internal override bool IsScriptInitializer => true;

	public override int Arity => TypeParameters.Length;

	public override Symbol AssociatedSymbol => null;

	public override Symbol ContainingSymbol => _containingType;

	public override Accessibility DeclaredAccessibility => Accessibility.Internal;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public override bool HidesBaseMethodsByName => false;

	public override bool IsAbstract => false;

	public override bool IsAsync => true;

	public override bool IsExtensionMethod => false;

	public override bool IsExtern => false;

	public override bool IsOverride => false;

	public override bool IsSealed => false;

	public override bool IsStatic => false;

	public override bool IsVararg => false;

	public override RefKind RefKind => RefKind.None;

	public override bool IsVirtual => false;

	public override ImmutableArray<Location> Locations => _containingType.Locations;

	public override MethodKind MethodKind => MethodKind.Ordinary;

	public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

	public override bool ReturnsVoid => _returnType.SpecialType == SpecialType.System_Void;

	public override TypeSymbol ReturnType => _returnType;

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<TypeSymbol> TypeArguments => ImmutableArray<TypeSymbol>.Empty;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	internal override CallingConvention CallingConvention => CallingConvention.HasThis;

	internal override bool GenerateDebugInfo => true;

	internal override bool HasDeclarativeSecurity => false;

	internal override bool HasSpecialName => true;

	internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	internal override bool RequiresSecurityObject => false;

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

	internal TypeSymbol ResultType => _resultType;

	internal SynthesizedInteractiveInitializerMethod(SourceMemberContainerTypeSymbol containingType, DiagnosticBag diagnostics)
	{
		_containingType = containingType;
		CalculateReturnType(containingType.DeclaringCompilation, diagnostics, out _resultType, out _returnType);
	}

	public override DllImportData GetDllImportData()
	{
		return null;
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		return _containingType.CalculateSyntaxOffsetInSynthesizedConstructor(localPosition, localTree, isStatic: false);
	}

	private static void CalculateReturnType(CSharpCompilation compilation, DiagnosticBag diagnostics, out TypeSymbol resultType, out TypeSymbol returnType)
	{
		Type type = compilation.ScriptCompilationInfo?.ReturnTypeOpt;
		NamedTypeSymbol wellKnownType = compilation.GetWellKnownType(WellKnownType.System_Threading_Tasks_Task_T);
		DiagnosticInfo useSiteDiagnostic = wellKnownType.GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			diagnostics.Add(useSiteDiagnostic, NoLocation.Singleton);
		}
		resultType = (((object)type == null) ? compilation.GetSpecialType(SpecialType.System_Object) : compilation.GetTypeByReflectionType(type, diagnostics));
		returnType = wellKnownType.Construct(resultType);
	}
}
