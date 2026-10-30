using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class LambdaExpressionSyntax : AnonymousFunctionExpressionSyntax
{
	public abstract SyntaxToken ArrowToken { get; }

	internal LambdaExpressionSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal LambdaExpressionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected LambdaExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
