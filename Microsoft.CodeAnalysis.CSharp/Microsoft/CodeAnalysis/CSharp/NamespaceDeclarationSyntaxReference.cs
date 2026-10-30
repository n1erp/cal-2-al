using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class NamespaceDeclarationSyntaxReference : TranslationSyntaxReference
{
	public NamespaceDeclarationSyntaxReference(SyntaxReference reference)
		: base(reference)
	{
	}

	protected override SyntaxNode Translate(SyntaxReference reference, CancellationToken cancellationToken)
	{
		return GetSyntax(reference, cancellationToken);
	}

	internal static SyntaxNode GetSyntax(SyntaxReference reference, CancellationToken cancellationToken)
	{
		CSharpSyntaxNode cSharpSyntaxNode = (CSharpSyntaxNode)reference.GetSyntax(cancellationToken);
		while (cSharpSyntaxNode is NameSyntax)
		{
			cSharpSyntaxNode = cSharpSyntaxNode.Parent;
		}
		return cSharpSyntaxNode;
	}
}
