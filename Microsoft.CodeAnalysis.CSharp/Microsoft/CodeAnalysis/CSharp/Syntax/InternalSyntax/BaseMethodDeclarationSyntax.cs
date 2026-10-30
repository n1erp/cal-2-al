using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseMethodDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists { get; }

	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers { get; }

	public abstract ParameterListSyntax ParameterList { get; }

	public abstract BlockSyntax Body { get; }

	public abstract ArrowExpressionClauseSyntax ExpressionBody { get; }

	public abstract SyntaxToken SemicolonToken { get; }

	internal BaseMethodDeclarationSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseMethodDeclarationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected BaseMethodDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
