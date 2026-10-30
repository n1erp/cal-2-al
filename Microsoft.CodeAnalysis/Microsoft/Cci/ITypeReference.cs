using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface ITypeReference : IReference
{
	bool IsEnum { get; }

	bool IsValueType { get; }

	PrimitiveTypeCode TypeCode { get; }

	TypeDefinitionHandle TypeDef { get; }

	IGenericMethodParameterReference AsGenericMethodParameterReference { get; }

	IGenericTypeInstanceReference AsGenericTypeInstanceReference { get; }

	IGenericTypeParameterReference AsGenericTypeParameterReference { get; }

	INamespaceTypeReference AsNamespaceTypeReference { get; }

	INestedTypeReference AsNestedTypeReference { get; }

	ISpecializedNestedTypeReference AsSpecializedNestedTypeReference { get; }

	ITypeDefinition GetResolvedType(EmitContext context);

	INamespaceTypeDefinition AsNamespaceTypeDefinition(EmitContext context);

	INestedTypeDefinition AsNestedTypeDefinition(EmitContext context);

	ITypeDefinition AsTypeDefinition(EmitContext context);
}
