using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedSealedPropertyAccessor : SynthesizedInstanceMethodSymbol
{
	private readonly PropertySymbol _property;

	private readonly MethodSymbol _overriddenAccessor;

	private readonly ImmutableArray<ParameterSymbol> _parameters;

	internal override bool SynthesizesLoweredBoundBody => true;

	internal override bool GenerateDebugInfo => false;

	internal MethodSymbol OverriddenAccessor => _overriddenAccessor;

	public override Symbol ContainingSymbol => _property.ContainingType;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			Accessibility declaredAccessibility = _overriddenAccessor.DeclaredAccessibility;
			switch (declaredAccessibility)
			{
			case Accessibility.ProtectedOrInternal:
				if (!ContainingAssembly.HasInternalAccessTo(_overriddenAccessor.ContainingAssembly))
				{
					return Accessibility.Protected;
				}
				break;
			case Accessibility.ProtectedAndInternal:
				if (!ContainingAssembly.HasInternalAccessTo(_overriddenAccessor.ContainingAssembly))
				{
					return Accessibility.Private;
				}
				break;
			}
			return declaredAccessibility;
		}
	}

	public override bool IsStatic => false;

	public override bool IsAsync => false;

	public override bool IsVirtual => false;

	internal override CallingConvention CallingConvention => _overriddenAccessor.CallingConvention;

	public override MethodKind MethodKind => _overriddenAccessor.MethodKind;

	public override int Arity => 0;

	public override bool IsExtensionMethod => false;

	public override bool HidesBaseMethodsByName => false;

	public override bool IsVararg => _overriddenAccessor.IsVararg;

	public override bool ReturnsVoid => _overriddenAccessor.ReturnsVoid;

	public override RefKind RefKind => _overriddenAccessor.RefKind;

	public override TypeSymbol ReturnType => _overriddenAccessor.ReturnType;

	public override ImmutableArray<TypeSymbol> TypeArguments => ImmutableArray<TypeSymbol>.Empty;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	internal override bool IsExplicitInterfaceImplementation => false;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => _overriddenAccessor.ReturnTypeCustomModifiers;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _overriddenAccessor.RefCustomModifiers;

	public override Symbol AssociatedSymbol => _property;

	public override bool IsOverride => true;

	public override bool IsAbstract => false;

	public override bool IsSealed => true;

	public override bool IsExtern => false;

	public override string Name => _overriddenAccessor.Name;

	internal override bool HasSpecialName => true;

	internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	internal override bool IsMetadataFinal => true;

	internal override bool RequiresSecurityObject => false;

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

	internal override bool HasDeclarativeSecurity => false;

	internal override void GenerateMethodBody(TypeCompilationState compilationState, DiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
		syntheticBoundNodeFactory.CurrentFunction = OriginalDefinition;
		try
		{
			syntheticBoundNodeFactory.CloseMethod(MethodBodySynthesizer.ConstructSingleInvocationMethodBody(syntheticBoundNodeFactory, OverriddenAccessor, useBaseReference: true));
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}

	public SynthesizedSealedPropertyAccessor(PropertySymbol property, MethodSymbol overriddenAccessor)
	{
		_property = property;
		_overriddenAccessor = overriddenAccessor;
		_parameters = SynthesizedParameterSymbol.DeriveParameters(overriddenAccessor, this);
	}

	internal sealed override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
	{
		return true;
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
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
}
