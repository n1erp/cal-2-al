using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Emit.NoPia;

namespace Microsoft.CodeAnalysis.CSharp.Emit.NoPia;

internal sealed class EmbeddedField : EmbeddedTypesManager<PEModuleBuilder, ModuleCompilationState, EmbeddedTypesManager, SyntaxNode, CSharpAttributeData, Symbol, AssemblySymbol, NamedTypeSymbol, FieldSymbol, MethodSymbol, EventSymbol, PropertySymbol, ParameterSymbol, TypeParameterSymbol, EmbeddedType, EmbeddedField, EmbeddedMethod, EmbeddedEvent, EmbeddedProperty, EmbeddedParameter, EmbeddedTypeParameter>.CommonEmbeddedField
{
	internal override EmbeddedTypesManager TypeManager => ContainingType.TypeManager;

	protected override bool IsCompileTimeConstant => UnderlyingField.IsMetadataConstant;

	protected override bool IsNotSerialized => UnderlyingField.IsNotSerialized;

	protected override bool IsReadOnly => UnderlyingField.IsReadOnly;

	protected override bool IsRuntimeSpecial => UnderlyingField.HasRuntimeSpecialName;

	protected override bool IsSpecialName => UnderlyingField.HasSpecialName;

	protected override bool IsStatic => UnderlyingField.IsStatic;

	protected override bool IsMarshalledExplicitly => UnderlyingField.IsMarshalledExplicitly;

	protected override IMarshallingInformation MarshallingInformation => UnderlyingField.MarshallingInformation;

	protected override ImmutableArray<byte> MarshallingDescriptor => UnderlyingField.MarshallingDescriptor;

	protected override int? TypeLayoutOffset => UnderlyingField.TypeLayoutOffset;

	protected override TypeMemberVisibility Visibility => PEModuleBuilder.MemberVisibility(UnderlyingField);

	protected override string Name => UnderlyingField.MetadataName;

	public EmbeddedField(EmbeddedType containingType, FieldSymbol underlyingField)
		: base(containingType, underlyingField)
	{
	}

	protected override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return UnderlyingField.GetCustomAttributesToEmit(moduleBuilder);
	}

	protected override MetadataConstant GetCompileTimeValue(EmitContext context)
	{
		return UnderlyingField.GetMetadataConstantValue(context);
	}
}
