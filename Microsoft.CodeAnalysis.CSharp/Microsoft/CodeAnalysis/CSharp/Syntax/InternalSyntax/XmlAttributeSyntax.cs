using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class XmlAttributeSyntax : CSharpSyntaxNode
{
	public abstract XmlNameSyntax Name { get; }

	public abstract SyntaxToken EqualsToken { get; }

	public abstract SyntaxToken StartQuoteToken { get; }

	public abstract SyntaxToken EndQuoteToken { get; }

	internal XmlAttributeSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal XmlAttributeSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected XmlAttributeSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
