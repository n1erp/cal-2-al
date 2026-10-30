using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class ExpressionSyntax : CSharpSyntaxNode
{
	internal ExpressionSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal ExpressionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected ExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
