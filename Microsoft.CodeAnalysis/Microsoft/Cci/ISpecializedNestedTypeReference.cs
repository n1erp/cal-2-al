using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface ISpecializedNestedTypeReference : INestedTypeReference, INamedTypeReference, ITypeReference, IReference, INamedEntity, ITypeMemberReference
{
	INestedTypeReference GetUnspecializedVersion(EmitContext context);
}
