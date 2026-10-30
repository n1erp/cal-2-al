using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface ITypeMemberReference : IReference, INamedEntity
{
	ITypeReference GetContainingType(EmitContext context);
}
