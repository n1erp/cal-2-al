using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class ConditionalDirectiveTriviaSyntax : BranchingDirectiveTriviaSyntax
{
	public abstract ExpressionSyntax Condition { get; }

	public abstract bool ConditionValue { get; }

	internal ConditionalDirectiveTriviaSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal ConditionalDirectiveTriviaSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected ConditionalDirectiveTriviaSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
