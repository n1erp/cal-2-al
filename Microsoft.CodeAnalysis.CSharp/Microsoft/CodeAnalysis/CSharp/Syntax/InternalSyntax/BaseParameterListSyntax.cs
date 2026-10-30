using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BaseParameterListSyntax : CSharpSyntaxNode
{
	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SeparatedSyntaxList<ParameterSyntax> Parameters { get; }

	internal BaseParameterListSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BaseParameterListSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected BaseParameterListSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
