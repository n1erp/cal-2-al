using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseFieldDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists { get; }

	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers { get; }

	public abstract VariableDeclarationSyntax Declaration { get; }

	public abstract SyntaxToken SemicolonToken { get; }

	internal BaseFieldDeclarationSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseFieldDeclarationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected BaseFieldDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
