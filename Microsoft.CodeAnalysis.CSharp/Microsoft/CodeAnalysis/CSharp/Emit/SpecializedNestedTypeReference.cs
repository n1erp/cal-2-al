using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal class SpecializedNestedTypeReference : NamedTypeReference, ISpecializedNestedTypeReference, INestedTypeReference, INamedTypeReference, ITypeReference, IReference, INamedEntity, ITypeMemberReference
{
	public override IGenericTypeInstanceReference AsGenericTypeInstanceReference => null;

	public override INamespaceTypeReference AsNamespaceTypeReference => null;

	public override INestedTypeReference AsNestedTypeReference => this;

	public override ISpecializedNestedTypeReference AsSpecializedNestedTypeReference => this;

	public SpecializedNestedTypeReference(NamedTypeSymbol underlyingNamedType)
		: base(underlyingNamedType)
	{
	}

	INestedTypeReference ISpecializedNestedTypeReference.GetUnspecializedVersion(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(UnderlyingNamedType.OriginalDefinition, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics, fromImplements: false, needDeclaration: true).AsNestedTypeReference;
	}

	public override void Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	ITypeReference ITypeMemberReference.GetContainingType(EmitContext context)
	{
		return ((PEModuleBuilder)context.Module).Translate(UnderlyingNamedType.ContainingType, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
	}
}
