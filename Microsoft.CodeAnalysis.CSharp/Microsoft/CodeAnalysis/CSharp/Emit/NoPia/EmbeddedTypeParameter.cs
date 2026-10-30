using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Emit.NoPia;

namespace Microsoft.CodeAnalysis.CSharp.Emit.NoPia;

internal sealed class EmbeddedTypeParameter : EmbeddedTypesManager<PEModuleBuilder, ModuleCompilationState, EmbeddedTypesManager, SyntaxNode, CSharpAttributeData, Symbol, AssemblySymbol, NamedTypeSymbol, FieldSymbol, MethodSymbol, EventSymbol, PropertySymbol, ParameterSymbol, TypeParameterSymbol, EmbeddedType, EmbeddedField, EmbeddedMethod, EmbeddedEvent, EmbeddedProperty, EmbeddedParameter, EmbeddedTypeParameter>.CommonEmbeddedTypeParameter
{
	protected override bool MustBeReferenceType => UnderlyingTypeParameter.HasReferenceTypeConstraint;

	protected override bool MustBeValueType => UnderlyingTypeParameter.HasValueTypeConstraint;

	protected override bool MustHaveDefaultConstructor => UnderlyingTypeParameter.HasConstructorConstraint;

	protected override string Name => UnderlyingTypeParameter.MetadataName;

	protected override ushort Index => (ushort)UnderlyingTypeParameter.Ordinal;

	public EmbeddedTypeParameter(EmbeddedMethod containingMethod, TypeParameterSymbol underlyingTypeParameter)
		: base(containingMethod, underlyingTypeParameter)
	{
	}

	protected override IEnumerable<TypeReferenceWithAttributes> GetConstraints(EmitContext context)
	{
		return ((IGenericParameter)UnderlyingTypeParameter).GetConstraints(context);
	}
}
