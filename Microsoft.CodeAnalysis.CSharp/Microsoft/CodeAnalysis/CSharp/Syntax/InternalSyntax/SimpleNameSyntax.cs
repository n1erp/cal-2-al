using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class SimpleNameSyntax : NameSyntax
{
	public abstract SyntaxToken Identifier { get; }

	internal SimpleNameSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal SimpleNameSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected SimpleNameSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
