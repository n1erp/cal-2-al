using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal abstract class SingleDeclaration : Declaration
{
	public SourceLocation Location { get; }

	public SyntaxReference SyntaxReference { get; }

	public SourceLocation NameLocation { get; }

	public new ImmutableArray<SingleDeclaration> Children => GetSingleDeclarationChildren();

	protected SingleDeclaration(string name, SyntaxReference syntaxReference, SourceLocation nameLocation)
		: base(name)
	{
		SyntaxReference = syntaxReference;
		NameLocation = nameLocation;
		Location = new SourceLocation(SyntaxReference);
	}

	protected override ImmutableArray<Declaration> GetDeclarationChildren()
	{
		return StaticCast<Declaration>.From(GetSingleDeclarationChildren());
	}

	protected abstract ImmutableArray<SingleDeclaration> GetSingleDeclarationChildren();
}
