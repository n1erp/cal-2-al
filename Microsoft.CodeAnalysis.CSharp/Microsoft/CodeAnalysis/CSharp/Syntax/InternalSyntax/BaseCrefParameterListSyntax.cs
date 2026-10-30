using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseCrefParameterListSyntax : CSharpSyntaxNode
{
	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<CrefParameterSyntax> Parameters { get; }

	internal BaseCrefParameterListSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseCrefParameterListSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected BaseCrefParameterListSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
