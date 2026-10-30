using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class InstanceExpressionSyntax : ExpressionSyntax
{
	internal InstanceExpressionSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal InstanceExpressionSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected InstanceExpressionSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
