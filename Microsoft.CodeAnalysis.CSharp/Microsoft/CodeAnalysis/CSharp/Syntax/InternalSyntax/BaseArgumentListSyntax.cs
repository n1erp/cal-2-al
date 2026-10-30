using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseArgumentListSyntax : CSharpSyntaxNode
{
	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ArgumentSyntax> Arguments { get; }

	internal BaseArgumentListSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseArgumentListSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected BaseArgumentListSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
