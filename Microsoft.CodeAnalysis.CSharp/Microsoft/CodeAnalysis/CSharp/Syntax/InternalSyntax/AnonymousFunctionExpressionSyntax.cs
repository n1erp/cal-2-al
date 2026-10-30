using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class AnonymousFunctionExpressionSyntax : ExpressionSyntax
{
	public abstract SyntaxToken AsyncKeyword { get; }

	public abstract CSharpSyntaxNode Body { get; }

	internal AnonymousFunctionExpressionSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal AnonymousFunctionExpressionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected AnonymousFunctionExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
