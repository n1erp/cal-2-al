using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Emit.NoPia;

namespace Microsoft.CodeAnalysis.CSharp.Emit.NoPia;

internal sealed class EmbeddedParameter : EmbeddedTypesManager<PEModuleBuilder, ModuleCompilationState, EmbeddedTypesManager, SyntaxNode, CSharpAttributeData, Symbol, AssemblySymbol, NamedTypeSymbol, FieldSymbol, MethodSymbol, EventSymbol, PropertySymbol, ParameterSymbol, TypeParameterSymbol, EmbeddedType, EmbeddedField, EmbeddedMethod, EmbeddedEvent, EmbeddedProperty, EmbeddedParameter, EmbeddedTypeParameter>.CommonEmbeddedParameter
{
	protected override bool HasDefaultValue => UnderlyingParameter.HasMetadataConstantValue;

	protected override bool IsIn => UnderlyingParameter.IsMetadataIn;

	protected override bool IsOut => UnderlyingParameter.IsMetadataOut;

	protected override bool IsOptional => UnderlyingParameter.IsMetadataOptional;

	protected override bool IsMarshalledExplicitly => UnderlyingParameter.IsMarshalledExplicitly;

	protected override IMarshallingInformation MarshallingInformation => UnderlyingParameter.MarshallingInformation;

	protected override ImmutableArray<byte> MarshallingDescriptor => UnderlyingParameter.MarshallingDescriptor;

	protected override string Name => UnderlyingParameter.MetadataName;

	protected override IParameterTypeInformation UnderlyingParameterTypeInformation => UnderlyingParameter;

	protected override ushort Index => (ushort)UnderlyingParameter.Ordinal;

	public EmbeddedParameter(EmbeddedTypesManager<PEModuleBuilder, ModuleCompilationState, EmbeddedTypesManager, SyntaxNode, CSharpAttributeData, Symbol, AssemblySymbol, NamedTypeSymbol, FieldSymbol, MethodSymbol, EventSymbol, PropertySymbol, ParameterSymbol, TypeParameterSymbol, EmbeddedType, EmbeddedField, EmbeddedMethod, EmbeddedEvent, EmbeddedProperty, EmbeddedParameter, EmbeddedTypeParameter>.CommonEmbeddedMember containingPropertyOrMethod, ParameterSymbol underlyingParameter)
		: base(containingPropertyOrMethod, underlyingParameter)
	{
	}

	protected override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return UnderlyingParameter.GetCustomAttributesToEmit(moduleBuilder);
	}

	protected override MetadataConstant GetDefaultValue(EmitContext context)
	{
		return UnderlyingParameter.GetMetadataConstantValue(context);
	}
}
