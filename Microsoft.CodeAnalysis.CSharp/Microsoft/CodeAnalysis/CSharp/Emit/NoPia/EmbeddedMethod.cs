using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Emit.NoPia;

namespace Microsoft.CodeAnalysis.CSharp.Emit.NoPia;

internal sealed class EmbeddedMethod : EmbeddedTypesManager<PEModuleBuilder, ModuleCompilationState, EmbeddedTypesManager, SyntaxNode, CSharpAttributeData, Symbol, AssemblySymbol, NamedTypeSymbol, FieldSymbol, MethodSymbol, EventSymbol, PropertySymbol, ParameterSymbol, TypeParameterSymbol, EmbeddedType, EmbeddedField, EmbeddedMethod, EmbeddedEvent, EmbeddedProperty, EmbeddedParameter, EmbeddedTypeParameter>.CommonEmbeddedMethod
{
	internal override EmbeddedTypesManager TypeManager => ContainingType.TypeManager;

	protected override bool IsAbstract => UnderlyingMethod.IsAbstract;

	protected override bool IsAccessCheckedOnOverride => UnderlyingMethod.IsAccessCheckedOnOverride;

	protected override bool IsConstructor => UnderlyingMethod.MethodKind == MethodKind.Constructor;

	protected override bool IsExternal => UnderlyingMethod.IsExternal;

	protected override bool IsHiddenBySignature => !UnderlyingMethod.HidesBaseMethodsByName;

	protected override bool IsNewSlot => UnderlyingMethod.IsMetadataNewSlot();

	protected override IPlatformInvokeInformation PlatformInvokeData => UnderlyingMethod.GetDllImportData();

	protected override bool IsRuntimeSpecial => UnderlyingMethod.HasRuntimeSpecialName;

	protected override bool IsSpecialName => UnderlyingMethod.HasSpecialName;

	protected override bool IsSealed => UnderlyingMethod.IsMetadataFinal;

	protected override bool IsStatic => UnderlyingMethod.IsStatic;

	protected override bool IsVirtual => UnderlyingMethod.IsMetadataVirtual();

	protected override bool ReturnValueIsMarshalledExplicitly => UnderlyingMethod.ReturnValueIsMarshalledExplicitly;

	protected override IMarshallingInformation ReturnValueMarshallingInformation => UnderlyingMethod.ReturnValueMarshallingInformation;

	protected override ImmutableArray<byte> ReturnValueMarshallingDescriptor => UnderlyingMethod.ReturnValueMarshallingDescriptor;

	protected override TypeMemberVisibility Visibility => PEModuleBuilder.MemberVisibility(UnderlyingMethod);

	protected override string Name => UnderlyingMethod.MetadataName;

	protected override bool AcceptsExtraArguments => UnderlyingMethod.IsVararg;

	protected override ISignature UnderlyingMethodSignature => UnderlyingMethod;

	protected override INamespace ContainingNamespace => UnderlyingMethod.ContainingNamespace;

	public EmbeddedMethod(EmbeddedType containingType, MethodSymbol underlyingMethod)
		: base(containingType, underlyingMethod)
	{
	}

	protected override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return UnderlyingMethod.GetCustomAttributesToEmit(moduleBuilder);
	}

	protected override ImmutableArray<EmbeddedParameter> GetParameters()
	{
		return EmbeddedTypesManager.EmbedParameters(this, UnderlyingMethod.Parameters);
	}

	protected override ImmutableArray<EmbeddedTypeParameter> GetTypeParameters()
	{
		return UnderlyingMethod.TypeParameters.SelectAsArray((TypeParameterSymbol t, EmbeddedMethod m) => new EmbeddedTypeParameter(m, t), this);
	}

	protected override MethodImplAttributes GetImplementationAttributes(EmitContext context)
	{
		return UnderlyingMethod.ImplementationAttributes;
	}
}
