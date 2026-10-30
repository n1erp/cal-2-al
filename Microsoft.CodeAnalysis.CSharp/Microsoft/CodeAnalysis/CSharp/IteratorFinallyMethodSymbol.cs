using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class IteratorFinallyMethodSymbol : SynthesizedInstanceMethodSymbol, ISynthesizedMethodBodyImplementationSymbol
{
	private readonly IteratorStateMachine _stateMachineType;

	private readonly string _name;

	public override string Name => _name;

	internal override bool IsMetadataFinal => false;

	public override MethodKind MethodKind => MethodKind.Ordinary;

	public override int Arity => 0;

	public override bool IsExtensionMethod => false;

	internal override bool HasSpecialName => false;

	internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	internal override bool HasDeclarativeSecurity => false;

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

	internal override bool RequiresSecurityObject => false;

	public override bool HidesBaseMethodsByName => false;

	public override bool IsVararg => false;

	public override bool ReturnsVoid => true;

	public override bool IsAsync => false;

	public override RefKind RefKind => RefKind.None;

	public override TypeSymbol ReturnType => ContainingAssembly.GetSpecialType(SpecialType.System_Void);

	public override ImmutableArray<TypeSymbol> TypeArguments => ImmutableArray<TypeSymbol>.Empty;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override Symbol AssociatedSymbol => null;

	internal override CallingConvention CallingConvention => CallingConvention.HasThis;

	internal override bool GenerateDebugInfo => true;

	public override Symbol ContainingSymbol => _stateMachineType;

	public override ImmutableArray<Location> Locations => ContainingType.Locations;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override bool IsStatic => false;

	public override bool IsVirtual => false;

	public override bool IsOverride => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => false;

	public override bool IsExtern => false;

	IMethodSymbol ISynthesizedMethodBodyImplementationSymbol.Method => _stateMachineType.KickoffMethod;

	bool ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency => true;

	public IteratorFinallyMethodSymbol(IteratorStateMachine stateMachineType, string name)
	{
		_stateMachineType = stateMachineType;
		_name = name;
	}

	internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
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
		return _stateMachineType.KickoffMethod.CalculateLocalSyntaxOffset(localPosition, localTree);
	}
}
