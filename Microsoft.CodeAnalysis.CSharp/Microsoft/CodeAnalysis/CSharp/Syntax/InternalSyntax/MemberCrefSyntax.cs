using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class MemberCrefSyntax : CrefSyntax
{
	internal MemberCrefSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal MemberCrefSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected MemberCrefSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
