using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class RootObjectCollectionDeclaration : SingleObjectCollectionDeclaration
{
	internal RootObjectCollectionDeclaration(SyntaxReference syntaxReference, SourceLocation nameLocation, ImmutableArray<SingleObjectDeclaration> children)
		: base(syntaxReference, nameLocation, children)
	{
	}
}
