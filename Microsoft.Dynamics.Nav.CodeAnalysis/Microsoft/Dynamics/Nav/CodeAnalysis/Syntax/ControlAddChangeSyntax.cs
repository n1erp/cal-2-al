using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ControlAddChangeSyntax : ControlChangeBaseSyntax
{
	private IdentifierNameSyntax _anchor;

	private SyntaxNode _controls;

	public override SyntaxToken ChangeKeyword => new SyntaxToken(this, ((InternalControlAddChangeSyntax)Node)._changeKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalControlAddChangeSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Anchor => GetRed(ref _anchor, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalControlAddChangeSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalControlAddChangeSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public SyntaxList<ControlBaseSyntax> Controls => new SyntaxList<ControlBaseSyntax>(GetRed(ref _controls, 5));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalControlAddChangeSyntax)Node)._closeBraceToken, GetChildPosition(6), GetChildIndex(6));

	internal ControlAddChangeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _anchor, 2), 
			5 => GetRed(ref _controls, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _anchor, 
			5 => _controls, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitControlAddChange(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitControlAddChange(this);
	}

	public ControlAddChangeSyntax Update(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, SyntaxList<ControlBaseSyntax> controls, SyntaxToken closeBraceToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || anchor != Anchor || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || controls != Controls || closeBraceToken != CloseBraceToken)
		{
			ControlAddChangeSyntax controlAddChangeSyntax = SyntaxFactory.ControlAddChange(changeKeyword, openParenthesisToken, anchor, closeParenthesisToken, openBraceToken, controls, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return controlAddChangeSyntax.WithAnnotations(annotations);
			}
			return controlAddChangeSyntax;
		}
		return this;
	}

	public ControlAddChangeSyntax WithChangeKeyword(SyntaxToken changeKeyword)
	{
		return Update(changeKeyword, OpenParenthesisToken, Anchor, CloseParenthesisToken, OpenBraceToken, Controls, CloseBraceToken);
	}

	public ControlAddChangeSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ChangeKeyword, openParenthesisToken, Anchor, CloseParenthesisToken, OpenBraceToken, Controls, CloseBraceToken);
	}

	public ControlAddChangeSyntax WithAnchor(IdentifierNameSyntax anchor)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, anchor, CloseParenthesisToken, OpenBraceToken, Controls, CloseBraceToken);
	}

	public ControlAddChangeSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, closeParenthesisToken, OpenBraceToken, Controls, CloseBraceToken);
	}

	public ControlAddChangeSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, CloseParenthesisToken, openBraceToken, Controls, CloseBraceToken);
	}

	public ControlAddChangeSyntax WithControls(SyntaxList<ControlBaseSyntax> controls)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, CloseParenthesisToken, OpenBraceToken, controls, CloseBraceToken);
	}

	public ControlAddChangeSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, CloseParenthesisToken, OpenBraceToken, Controls, closeBraceToken);
	}

	public ControlAddChangeSyntax AddControls(params ControlBaseSyntax[] items)
	{
		return WithControls(Controls.AddRange(items));
	}
}
