using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalActionMoveChangeSyntax : InternalActionChangeBaseSyntax
{
	internal readonly InternalSyntaxToken _changeKeyword;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _anchor;

	internal readonly InternalSyntaxToken _semicolonToken;

	internal readonly InternalSyntaxNode _actions;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => ActionMoveChangeSyntaxDefinition.Instance.Value;

	public override InternalSyntaxToken ChangeKeyword => _changeKeyword;

	public override InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalIdentifierNameSyntax Anchor => _anchor;

	public InternalSyntaxToken SemicolonToken => _semicolonToken;

	public InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> Actions => new InternalSeparatedSyntaxList<InternalIdentifierNameSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_actions));

	public override InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalActionMoveChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSyntaxNode actions, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(anchor);
		_anchor = anchor;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		if (actions != null)
		{
			AdjustFlagsAndWidth(actions);
			_actions = actions;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalActionMoveChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSyntaxNode actions, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(anchor);
		_anchor = anchor;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		if (actions != null)
		{
			AdjustFlagsAndWidth(actions);
			_actions = actions;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalActionMoveChangeSyntax(SyntaxKind kind, InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSyntaxNode actions, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 6;
		AdjustFlagsAndWidth(changeKeyword);
		_changeKeyword = changeKeyword;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(anchor);
		_anchor = anchor;
		AdjustFlagsAndWidth(semicolonToken);
		_semicolonToken = semicolonToken;
		if (actions != null)
		{
			AdjustFlagsAndWidth(actions);
			_actions = actions;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _changeKeyword, 
			1 => _openParenthesisToken, 
			2 => _anchor, 
			3 => _semicolonToken, 
			4 => _actions, 
			5 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ActionMoveChangeSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitActionMoveChange(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitActionMoveChange(this);
	}

	public InternalActionMoveChangeSyntax Update(InternalSyntaxToken changeKeyword, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax anchor, InternalSyntaxToken semicolonToken, InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> actions, InternalSyntaxToken closeParenthesisToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || anchor != Anchor || semicolonToken != SemicolonToken || actions != Actions || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.ActionMoveChange(changeKeyword, openParenthesisToken, anchor, semicolonToken, actions, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalActionMoveChangeSyntax(Kind, _changeKeyword, _openParenthesisToken, _anchor, _semicolonToken, _actions, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalActionMoveChangeSyntax(Kind, _changeKeyword, _openParenthesisToken, _anchor, _semicolonToken, _actions, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
