using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class SingleObjectCollectionDeclaration : SingleDeclaration
{
	private readonly ImmutableArray<SingleObjectDeclaration> children;

	public override DeclarationKind Kind => DeclarationKind.ObjectCollection;

	internal SingleObjectCollectionDeclaration(SyntaxReference syntaxReference, SourceLocation nameLocation, ImmutableArray<SingleObjectDeclaration> children)
		: base(string.Empty, syntaxReference, nameLocation)
	{
		this.children = children;
	}

	protected override ImmutableArray<SingleDeclaration> GetSingleDeclarationChildren()
	{
		return StaticCast<SingleDeclaration>.From(children);
	}
}
