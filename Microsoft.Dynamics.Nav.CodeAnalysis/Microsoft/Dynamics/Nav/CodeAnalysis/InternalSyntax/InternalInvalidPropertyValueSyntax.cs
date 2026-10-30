using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalInvalidPropertyValueSyntax : InternalPropertyValueSyntax
{
	internal readonly InternalSyntaxToken _invalidToken;

	internal override SyntaxNodeDefinition Definition => InvalidPropertyValueSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken InvalidToken => _invalidToken;

	internal InternalInvalidPropertyValueSyntax(SyntaxKind kind, InternalSyntaxToken invalidToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 1;
		if (invalidToken != null)
		{
			AdjustFlagsAndWidth(invalidToken);
			_invalidToken = invalidToken;
		}
	}

	internal InternalInvalidPropertyValueSyntax(SyntaxKind kind, InternalSyntaxToken invalidToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 1;
		if (invalidToken != null)
		{
			AdjustFlagsAndWidth(invalidToken);
			_invalidToken = invalidToken;
		}
	}

	internal InternalInvalidPropertyValueSyntax(SyntaxKind kind, InternalSyntaxToken invalidToken)
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
		return new InvalidPropertyValueSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInvalidPropertyValue(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitInvalidPropertyValue(this);
	}

	public InternalInvalidPropertyValueSyntax Update(InternalSyntaxToken invalidToken)
	{
		if (invalidToken != InvalidToken)
		{
			return InternalSyntaxFactory.InvalidPropertyValue(invalidToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalInvalidPropertyValueSyntax(Kind, _invalidToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalInvalidPropertyValueSyntax(Kind, _invalidToken, GetDiagnostics(), annotations);
	}
}
