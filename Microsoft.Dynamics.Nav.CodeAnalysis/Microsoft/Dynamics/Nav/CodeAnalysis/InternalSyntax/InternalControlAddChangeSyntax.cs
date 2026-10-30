using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalControlAddChangeSyntax : InternalControlChangeBaseSyntax
{
	internal readonly InternalSyntaxToken _changeKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _anchor;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalSyntaxNode _controls;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => ControlAddChangeSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken ChangeKeyword => _changeKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalIdentifierNameSyntax Anchor => _anchor;

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public InternalSyntaxList<InternalControlBaseSyntax> Controls => new InternalSyntaxList<InternalControlBaseSyntax>(_controls);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalControlAddChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxNode controls, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(anchor);
		_anchor = anchor;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (controls != null)
		{
			AdjustFlagsAndWidth(controls);
			_controls = controls;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalControlAddChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxNode controls, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(anchor);
		_anchor = anchor;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (controls != null)
		{
			AdjustFlagsAndWidth(controls);
			_controls = controls;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalControlAddChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxNode controls, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 7;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(anchor);
		_anchor = anchor;
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		if (controls != null)
		{
			AdjustFlagsAndWidth(controls);
			_controls = controls;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _changeKeyword, 
			1 => _openParenthesisToken, 
			2 => _anchor, 
			3 => _closeParenthesisToken, 
			4 => _openBraceToken, 
			5 => _controls, 
			6 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ControlAddChangeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitControlAddChange(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitControlAddChange(this);
	}

	public InternalControlAddChangeSyntax Update(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken closeParenthesisToken, InternalSyntaxToken openBraceToken, InternalSyntaxList<InternalControlBaseSyntax> controls, InternalSyntaxToken closeBraceToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || anchor != Anchor || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || controls != Controls || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.ControlAddChange(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, controls, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalControlAddChangeSyntax(Kind, _changeKeyword, _openParenthesisToken, _anchor, _closeParenthesisToken, _openBraceToken, _controls, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalControlAddChangeSyntax(Kind, _changeKeyword, _openParenthesisToken, _anchor, _closeParenthesisToken, _openBraceToken, _controls, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
