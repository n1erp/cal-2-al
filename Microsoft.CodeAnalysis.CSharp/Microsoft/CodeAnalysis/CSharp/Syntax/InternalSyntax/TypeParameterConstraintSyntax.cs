using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class TypeParameterConstraintSyntax : CSharpSyntaxNode
{
	internal TypeParameterConstraintSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal TypeParameterConstraintSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected TypeParameterConstraintSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
