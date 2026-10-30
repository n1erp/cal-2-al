using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class DirectiveTriviaSyntax : StructuredTriviaSyntax
{
	public abstract SyntaxToken HashToken { get; }

	public abstract SyntaxToken EndOfDirectiveToken { get; }

	public abstract bool IsActive { get; }

	internal DirectiveTriviaSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		flags |= NodeFlags.ContainsDirectives;
	}

	internal DirectiveTriviaSyntax(SyntaxKind kind)
		: base(kind)
	{
		flags |= NodeFlags.ContainsDirectives;
	}

	protected DirectiveTriviaSyntax(ObjectReader reader)
		: base(reader)
	{
		flags |= NodeFlags.ContainsDirectives;
	}

	internal override DirectiveStack ApplyDirectives(DirectiveStack stack)
	{
		return stack.Add(new Directive(this));
	}
}
