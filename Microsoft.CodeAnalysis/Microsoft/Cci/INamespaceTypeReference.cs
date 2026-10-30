using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.Cci;

internal interface INamespaceTypeReference : INamedTypeReference, ITypeReference, IReference, INamedEntity
{
	string NamespaceName { get; }

	IUnitReference GetUnit(EmitContext context);
}
