using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface IGenericTypeInstanceReference : ITypeReference, IReference
{
	ImmutableArray<ITypeReference> GetGenericArguments(EmitContext context);

	INamedTypeReference GetGenericType(EmitContext context);
}
