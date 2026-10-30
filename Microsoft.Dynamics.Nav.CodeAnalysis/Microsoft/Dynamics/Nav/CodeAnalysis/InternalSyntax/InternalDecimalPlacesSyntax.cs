using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalDecimalPlacesSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _minimumToken;

	internal readonly InternalSyntaxToken _colonToken;

	internal readonly InternalSyntaxToken _maximumToken;

	internal override SyntaxNodeDefinition Definition => DecimalPlacesSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken MinimumToken => _minimumToken;

	public InternalSyntaxToken ColonToken => _colonToken;

	public InternalSyntaxToken MaximumToken => _maximumToken;

	internal InternalDecimalPlacesSyntax(SyntaxKind kind, InternalSyntaxToken minimumToken, InternalSyntaxToken colonToken, InternalSyntaxToken maximumToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(minimumToken);
		_minimumToken = minimumToken;
		if (colonToken != null)
		{
			AdjustFlagsAndWidth(colonToken);
			_colonToken = colonToken;
		}
		if (maximumToken != null)
		{
			AdjustFlagsAndWidth(maximumToken);
			_maximumToken = maximumToken;
		}
	}

	internal InternalDecimalPlacesSyntax(SyntaxKind kind, InternalSyntaxToken minimumToken, InternalSyntaxToken colonToken, InternalSyntaxToken maximumToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(minimumToken);
		_minimumToken = minimumToken;
		if (colonToken != null)
		{
			AdjustFlagsAndWidth(colonToken);
			_colonToken = colonToken;
		}
		if (maximumToken != null)
		{
			AdjustFlagsAndWidth(maximumToken);
			_maximumToken = maximumToken;
		}
	}

	internal InternalDecimalPlacesSyntax(SyntaxKind kind, InternalSyntaxToken minimumToken, InternalSyntaxToken colonToken, InternalSyntaxToken maximumToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(minimumToken);
		_minimumToken = minimumToken;
		if (colonToken != null)
		{
			AdjustFlagsAndWidth(colonToken);
			_colonToken = colonToken;
		}
		if (maximumToken != null)
		{
			AdjustFlagsAndWidth(maximumToken);
			_maximumToken = maximumToken;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_minimumToken, 
			1 => _colonToken, 
			2 => _maximumToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new DecimalPlacesSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitDecimalPlaces(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitDecimalPlaces(this);
	}

	public InternalDecimalPlacesSyntax Update(InternalSyntaxToken minimumToken, InternalSyntaxToken colonToken, InternalSyntaxToken maximumToken)
	{
		if (minimumToken != MinimumToken || colonToken != ColonToken || maximumToken != MaximumToken)
		{
			return InternalSyntaxFactory.DecimalPlaces(minimumToken, colonToken, maximumToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalDecimalPlacesSyntax(Kind, _minimumToken, _colonToken, _maximumToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalDecimalPlacesSyntax(Kind, _minimumToken, _colonToken, _maximumToken, GetDiagnostics(), annotations);
	}
}
