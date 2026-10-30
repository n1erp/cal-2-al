using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BranchingDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	public abstract bool BranchTaken { get; }

	internal BranchingDirectiveTriviaSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BranchingDirectiveTriviaSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected BranchingDirectiveTriviaSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
