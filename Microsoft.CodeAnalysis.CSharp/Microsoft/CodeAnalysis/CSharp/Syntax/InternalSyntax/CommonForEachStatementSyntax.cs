using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class CommonForEachStatementSyntax : StatementSyntax
{
	public abstract SyntaxToken ForEachKeyword { get; }

	public abstract SyntaxToken OpenParenToken { get; }

	public abstract SyntaxToken InKeyword { get; }

	public abstract ExpressionSyntax Expression { get; }

	public abstract SyntaxToken CloseParenToken { get; }

	public abstract StatementSyntax Statement { get; }

	internal CommonForEachStatementSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal CommonForEachStatementSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected CommonForEachStatementSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
