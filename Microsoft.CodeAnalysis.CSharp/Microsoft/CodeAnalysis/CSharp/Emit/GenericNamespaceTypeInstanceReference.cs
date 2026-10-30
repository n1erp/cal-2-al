using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class GenericNamespaceTypeInstanceReference : GenericTypeInstanceReference
{
	public override IGenericTypeInstanceReference AsGenericTypeInstanceReference => this;

	public override INamespaceTypeReference AsNamespaceTypeReference => null;

	public override INestedTypeReference AsNestedTypeReference => null;

	public override ISpecializedNestedTypeReference AsSpecializedNestedTypeReference => null;

	public GenericNamespaceTypeInstanceReference(NamedTypeSymbol underlyingNamedType)
		: base(underlyingNamedType)
	{
	}
}
