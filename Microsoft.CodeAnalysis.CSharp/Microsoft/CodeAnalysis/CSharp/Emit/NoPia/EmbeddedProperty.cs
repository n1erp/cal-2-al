using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit.NoPia;

namespace Microsoft.CodeAnalysis.CSharp.Emit.NoPia;

internal sealed class EmbeddedProperty : EmbeddedTypesManager<PEModuleBuilder, ModuleCompilationState, EmbeddedTypesManager, SyntaxNode, CSharpAttributeData, Symbol, AssemblySymbol, NamedTypeSymbol, FieldSymbol, MethodSymbol, EventSymbol, PropertySymbol, ParameterSymbol, TypeParameterSymbol, EmbeddedType, EmbeddedField, EmbeddedMethod, EmbeddedEvent, EmbeddedProperty, EmbeddedParameter, EmbeddedTypeParameter>.CommonEmbeddedProperty
{
	protected override bool IsRuntimeSpecial => UnderlyingProperty.HasRuntimeSpecialName;

	protected override bool IsSpecialName => UnderlyingProperty.HasSpecialName;

	protected override ISignature UnderlyingPropertySignature => UnderlyingProperty;

	protected override EmbeddedType ContainingType => AnAccessor.ContainingType;

	protected override TypeMemberVisibility Visibility => PEModuleBuilder.MemberVisibility(UnderlyingProperty);

	protected override string Name => UnderlyingProperty.MetadataName;

	public EmbeddedProperty(PropertySymbol underlyingProperty, EmbeddedMethod getter, EmbeddedMethod setter)
		: base(underlyingProperty, getter, setter)
	{
	}

	protected override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return UnderlyingProperty.GetCustomAttributesToEmit(moduleBuilder);
	}

	protected override ImmutableArray<EmbeddedParameter> GetParameters()
	{
		return EmbeddedTypesManager.EmbedParameters(this, UnderlyingProperty.Parameters);
	}
}
