using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalSkippedTokensTriviaSyntax : InternalStructuredTriviaSyntax
{
	internal readonly InternalSyntaxNode _tokens;

	internal override SyntaxNodeDefinition Definition => SkippedTokensTriviaSyntaxDefinition.Instance.Value;

	public InternalSyntaxNode Tokens => _tokens;

	internal InternalSkippedTokensTriviaSyntax(SyntaxKind kind, InternalSyntaxNode tokens, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(tokens);
		_tokens = tokens;
	}

	internal InternalSkippedTokensTriviaSyntax(SyntaxKind kind, InternalSyntaxNode tokens, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(tokens);
		_tokens = tokens;
	}

	internal InternalSkippedTokensTriviaSyntax(SyntaxKind kind, InternalSyntaxNode tokens)
		: base(kind)
	{
		SlotCount = 1;
		AdjustFlagsAndWidth(tokens);
		_tokens = tokens;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _tokens;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new SkippedTokensTriviaSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSkippedTokensTrivia(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitSkippedTokensTrivia(this);
	}

	public InternalSkippedTokensTriviaSyntax Update(InternalSyntaxNode tokens)
	{
		if (tokens != Tokens)
		{
			return InternalSyntaxFactory.SkippedTokensTrivia(tokens).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalSkippedTokensTriviaSyntax(Kind, _tokens, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalSkippedTokensTriviaSyntax(Kind, _tokens, GetDiagnostics(), annotations);
	}
}
