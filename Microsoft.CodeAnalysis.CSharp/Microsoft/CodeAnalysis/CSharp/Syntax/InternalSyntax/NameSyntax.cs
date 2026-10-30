using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class NameSyntax : TypeSyntax
{
	internal NameSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal NameSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected NameSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
