using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class GenericNestedTypeInstanceReference : GenericTypeInstanceReference, INestedTypeReference, INamedTypeReference, ITypeReference, IReference, INamedEntity, ITypeMemberReference
{
	public override IGenericTypeInstanceReference AsGenericTypeInstanceReference => this;

	public override INamespaceTypeReference AsNamespaceTypeReference => null;

	public override INestedTypeReference AsNestedTypeReference => this;

	public override ISpecializedNestedTypeReference AsSpecializedNestedTypeReference => null;

	public GenericNestedTypeInstanceReference(NamedTypeSymbol underlyingNamedType)
		: base(underlyingNamedType)
	{
	}

	ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(UnderlyingNamedType.ContainingType, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
	}
}
