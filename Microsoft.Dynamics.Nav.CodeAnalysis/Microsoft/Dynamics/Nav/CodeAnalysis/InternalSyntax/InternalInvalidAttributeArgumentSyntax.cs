using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalInvalidAttributeArgumentSyntax : InternalAttributeArgumentSyntax
{
	internal readonly InternalSyntaxNode _invalidToken;

	internal override SyntaxNodeDefinition Definition => InvalidAttributeArgumentSyntaxDefinition.Instance.Value;

	public InternalSyntaxNode InvalidToken => _invalidToken;

	internal InternalInvalidAttributeArgumentSyntax(SyntaxKind kind, InternalSyntaxNode invalidToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		if (invalidToken != null)
		{
			AdjustFlagsAndWidth(invalidToken);
			_invalidToken = invalidToken;
		}
	}

	internal InternalInvalidAttributeArgumentSyntax(SyntaxKind kind, InternalSyntaxNode invalidToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		if (invalidToken != null)
		{
			AdjustFlagsAndWidth(invalidToken);
			_invalidToken = invalidToken;
		}
	}

	internal InternalInvalidAttributeArgumentSyntax(SyntaxKind kind, InternalSyntaxNode invalidToken)
		: base(kind)
	{
		SlotCount = 1;
		if (invalidToken != null)
		{
			AdjustFlagsAndWidth(invalidToken);
			_invalidToken = invalidToken;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		if (index == 0)
		{
			return _invalidToken;
		}
		return null;
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new InvalidAttributeArgumentSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInvalidAttributeArgument(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitInvalidAttributeArgument(this);
	}

	public InternalInvalidAttributeArgumentSyntax Update(InternalSyntaxNode invalidToken)
	{
		if (invalidToken != InvalidToken)
		{
			return InternalSyntaxFactory.InvalidAttributeArgument(invalidToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalInvalidAttributeArgumentSyntax(Kind, _invalidToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalInvalidAttributeArgumentSyntax(Kind, _invalidToken, GetDiagnostics(), annotations);
	}
}
