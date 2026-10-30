using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class SwitchLabelSyntax : CSharpSyntaxNode
{
	public abstract SyntaxToken Keyword { get; }

	public abstract SyntaxToken ColonToken { get; }

	internal SwitchLabelSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal SwitchLabelSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected SwitchLabelSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
