using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class SingleObjectDeclaration : SingleDeclaration
{
	private readonly DeclarationKind kind;

	private readonly int objectId;

	public override DeclarationKind Kind => kind;

	internal int ObjectId => objectId;

	internal SingleObjectDeclaration(DeclarationKind kind, int objectId, string name, SyntaxReference syntaxReference, SourceLocation nameLocation)
		: base(name, syntaxReference, nameLocation)
	{
		this.kind = kind;
		this.objectId = objectId;
	}

	protected override ImmutableArray<SingleDeclaration> GetSingleDeclarationChildren()
	{
		return ImmutableArray<SingleDeclaration>.Empty;
	}
}
