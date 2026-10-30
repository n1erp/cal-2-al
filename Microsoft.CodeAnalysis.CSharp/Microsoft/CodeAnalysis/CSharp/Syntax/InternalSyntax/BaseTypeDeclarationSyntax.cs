using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseTypeDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists { get; }

	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers { get; }

	public abstract SyntaxToken Identifier { get; }

	public abstract BaseListSyntax BaseList { get; }

	public abstract SyntaxToken OpenBraceToken { get; }

	public abstract SyntaxToken CloseBraceToken { get; }

	public abstract SyntaxToken SemicolonToken { get; }

	internal BaseTypeDeclarationSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseTypeDeclarationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected BaseTypeDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
