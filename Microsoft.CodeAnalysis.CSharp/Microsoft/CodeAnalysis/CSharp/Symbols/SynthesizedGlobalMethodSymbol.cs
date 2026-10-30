using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedGlobalMethodSymbol : MethodSymbol
{
	private readonly ModuleSymbol _containingModule;

	private readonly PrivateImplementationDetails _privateImplType;

	private readonly TypeSymbol _returnType;

	private ImmutableArray<ParameterSymbol> _parameters;

	private readonly string _name;

	public sealed override bool IsImplicitlyDeclared => true;

	internal sealed override bool GenerateDebugInfo => false;

	internal sealed override ModuleSymbol ContainingModule => _containingModule;

	public sealed override AssemblySymbol ContainingAssembly => _containingModule.ContainingAssembly;

	public sealed override Symbol ContainingSymbol => null;

	public sealed override NamedTypeSymbol ContainingType => null;

	internal PrivateImplementationDetails ContainingPrivateImplementationDetailsType => _privateImplType;

	public override string Name => _name;

	internal override bool HasSpecialName => false;

	internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	internal override bool RequiresSecurityObject => false;

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

	internal override bool HasDeclarativeSecurity => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	public override bool IsVararg => false;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			if (_parameters.IsEmpty)
			{
				return ImmutableArray<ParameterSymbol>.Empty;
			}
			return _parameters;
		}
	}

	public override Accessibility DeclaredAccessibility => Accessibility.Internal;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override RefKind RefKind => RefKind.None;

	public override TypeSymbol ReturnType => _returnType;

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<TypeSymbol> TypeArguments => ImmutableArray<TypeSymbol>.Empty;

	public override Symbol AssociatedSymbol => null;

	public override int Arity => 0;

	public override bool ReturnsVoid => ReturnType.SpecialType == SpecialType.System_Void;

	public override MethodKind MethodKind => MethodKind.Ordinary;

	public override bool IsExtern => false;

	public override bool IsSealed => false;

	public override bool IsAbstract => false;

	public override bool IsOverride => false;

	public override bool IsVirtual => false;

	public override bool IsStatic => true;

	public override bool IsAsync => false;

	public override bool HidesBaseMethodsByName => false;

	internal override bool IsMetadataFinal => false;

	public override bool IsExtensionMethod => false;

	internal override CallingConvention CallingConvention => CallingConvention.Default;

	internal override bool IsExplicitInterfaceImplementation => false;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	internal override bool SynthesizesLoweredBoundBody => true;

	internal SynthesizedGlobalMethodSymbol(ModuleSymbol containingModule, PrivateImplementationDetails privateImplType, TypeSymbol returnType, string name)
	{
		_containingModule = containingModule;
		_privateImplType = privateImplType;
		_returnType = returnType;
		_name = name;
	}

	protected void SetParameters(ImmutableArray<ParameterSymbol> parameters)
	{
		ImmutableInterlocked.InterlockedExchange(ref _parameters, parameters);
	}

	public override DllImportData GetDllImportData()
	{
		return null;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal sealed override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal sealed override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal abstract override void GenerateMethodBody(TypeCompilationState compilationState, DiagnosticBag diagnostics);

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
