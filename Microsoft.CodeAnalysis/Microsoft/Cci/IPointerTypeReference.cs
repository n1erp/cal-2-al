using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IPointerTypeReference : ITypeReference, IReference
{
	ITypeReference GetTargetType(EmitContext context);
}
