using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class PatternSyntax : CSharpSyntaxNode
{
	internal PatternSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal PatternSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected PatternSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
