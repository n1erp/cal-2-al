using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseTypeSyntax : CSharpSyntaxNode
{
	public abstract TypeSyntax Type { get; }

	internal BaseTypeSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseTypeSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected BaseTypeSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
