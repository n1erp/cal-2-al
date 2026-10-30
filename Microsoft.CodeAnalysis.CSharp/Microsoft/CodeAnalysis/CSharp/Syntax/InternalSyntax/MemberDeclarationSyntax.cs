using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class MemberDeclarationSyntax : CSharpSyntaxNode
{
	internal MemberDeclarationSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal MemberDeclarationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected MemberDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
