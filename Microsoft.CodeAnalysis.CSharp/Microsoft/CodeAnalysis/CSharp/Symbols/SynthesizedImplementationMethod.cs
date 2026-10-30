using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedImplementationMethod : SynthesizedInstanceMethodSymbol
{
	private readonly MethodSymbol _interfaceMethod;

	private readonly NamedTypeSymbol _implementingType;

	private readonly bool _generateDebugInfo;

	private readonly PropertySymbol _associatedProperty;

	private readonly ImmutableArray<MethodSymbol> _explicitInterfaceImplementations;

	private readonly ImmutableArray<TypeParameterSymbol> _typeParameters;

	private readonly ImmutableArray<ParameterSymbol> _parameters;

	private readonly string _name;

	public sealed override bool IsVararg => _interfaceMethod.IsVararg;

	public sealed override int Arity => _interfaceMethod.Arity;

	public sealed override bool ReturnsVoid => _interfaceMethod.ReturnsVoid;

	internal sealed override CallingConvention CallingConvention => _interfaceMethod.CallingConvention;

	public sealed override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => _interfaceMethod.ReturnTypeCustomModifiers;

	public sealed override ImmutableArray<CustomModifier> RefCustomModifiers => _interfaceMethod.RefCustomModifiers;

	internal sealed override bool GenerateDebugInfo => _generateDebugInfo;

	public sealed override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	public sealed override ImmutableArray<TypeSymbol> TypeArguments => _typeParameters.Cast<TypeParameterSymbol, TypeSymbol>();

	public override RefKind RefKind => _interfaceMethod.RefKind;

	public sealed override TypeSymbol ReturnType => _interfaceMethod.ReturnType;

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public override Symbol ContainingSymbol => _implementingType;

	public override NamedTypeSymbol ContainingType => _implementingType;

	internal override bool IsExplicitInterfaceImplementation => true;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => _explicitInterfaceImplementations;

	public override MethodKind MethodKind => MethodKind.ExplicitInterfaceImplementation;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override Symbol AssociatedSymbol => _associatedProperty;

	public override bool HidesBaseMethodsByName => false;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override bool IsStatic => false;

	public override bool IsAsync => false;

	public override bool IsVirtual => false;

	public override bool IsOverride => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => false;

	public override bool IsExtern => false;

	public override bool IsExtensionMethod => false;

	public override string Name => _name;

	internal sealed override bool HasSpecialName => _interfaceMethod.HasSpecialName;

	internal sealed override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	internal sealed override bool RequiresSecurityObject => _interfaceMethod.RequiresSecurityObject;

	internal override bool IsMetadataFinal => true;

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

	internal override bool HasDeclarativeSecurity => false;

	public SynthesizedImplementationMethod(MethodSymbol interfaceMethod, NamedTypeSymbol implementingType, string name = null, bool generateDebugInfo = true, PropertySymbol associatedProperty = null)
	{
		_name = name ?? ExplicitInterfaceHelpers.GetMemberName(interfaceMethod.Name, interfaceMethod.ContainingType, null);
		_implementingType = implementingType;
		_generateDebugInfo = generateDebugInfo;
		_associatedProperty = associatedProperty;
		_explicitInterfaceImplementations = ImmutableArray.Create(interfaceMethod);
		(interfaceMethod.ContainingType.TypeSubstitution ?? TypeMap.Empty).WithAlphaRename(interfaceMethod, this, out _typeParameters);
		_interfaceMethod = interfaceMethod.ConstructIfGeneric(_typeParameters.Cast<TypeParameterSymbol, TypeSymbol>());
		_parameters = SynthesizedParameterSymbol.DeriveParameters(_interfaceMethod, this);
	}

	internal sealed override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
	{
		return true;
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return true;
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
}
