using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IFieldReference : ITypeMemberReference, IReference, INamedEntity
{
	ISpecializedFieldReference AsSpecializedFieldReference { get; }

	bool IsContextualNamedEntity { get; }

	ITypeReference GetType(EmitContext context);

	IFieldDefinition GetResolvedField(EmitContext context);
}
