using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ErrorMethodSymbol : MethodSymbol
{
	public static readonly ErrorMethodSymbol UnknownMethod = new ErrorMethodSymbol(ErrorTypeSymbol.UnknownResultType, ErrorTypeSymbol.UnknownResultType, string.Empty);

	private readonly TypeSymbol _containingType;

	private readonly TypeSymbol _returnType;

	private readonly string _name;

	public override string Name => _name;

	internal sealed override bool HasSpecialName => false;

	internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	public override bool IsExtern => false;

	public override bool IsSealed => false;

	public override bool IsAbstract => false;

	public override bool IsOverride => false;

	public override bool IsVirtual => false;

	public override bool IsStatic => false;

	public override bool IsAsync => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Symbol ContainingSymbol => _containingType;

	internal override CallingConvention CallingConvention => CallingConvention.Default;

	public override Symbol AssociatedSymbol => null;

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	internal override bool IsExplicitInterfaceImplementation => false;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	internal override int ParameterCount => 0;

	public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override ImmutableArray<TypeSymbol> TypeArguments => ImmutableArray<TypeSymbol>.Empty;

	public override RefKind RefKind => RefKind.None;

	public override TypeSymbol ReturnType => _returnType;

	public override bool ReturnsVoid => ReturnType.SpecialType == SpecialType.System_Void;

	public override bool IsVararg => false;

	public override bool HidesBaseMethodsByName => false;

	public override bool IsExtensionMethod => false;

	public override int Arity => 0;

	public override MethodKind MethodKind
	{
		get
		{
			string name = _name;
			if (name == ".ctor")
			{
				return MethodKind.Constructor;
			}
			return MethodKind.Ordinary;
		}
	}

	internal override bool IsMetadataFinal => false;

	internal sealed override bool RequiresSecurityObject => false;

	internal sealed override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

	internal sealed override bool HasDeclarativeSecurity => false;

	internal override bool GenerateDebugInfo => false;

	public ErrorMethodSymbol(TypeSymbol containingType, TypeSymbol returnType, string name)
	{
		_containingType = containingType;
		_returnType = returnType;
		_name = name;
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal sealed override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	public sealed override DllImportData GetDllImportData()
	{
		return null;
	}

	internal sealed override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal sealed override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
