using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ControlMoveChangeSyntax : ControlChangeBaseSyntax
{
	private IdentifierNameSyntax _anchor;

	private SyntaxNode _controls;

	public override SyntaxToken ChangeKeyword => new SyntaxToken(this, ((InternalControlMoveChangeSyntax)Node)._changeKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalControlMoveChangeSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Anchor => GetRed(ref _anchor, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalControlMoveChangeSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public SeparatedSyntaxList<IdentifierNameSyntax> Controls
	{
		get
		{
			SyntaxNode red = GetRed(ref _controls, 4);
			if (red != null)
			{
				return new SeparatedSyntaxList<IdentifierNameSyntax>(red, GetChildIndex(4));
			}
			return default;
		}
	}

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalControlMoveChangeSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	internal ControlMoveChangeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _anchor, 2), 
			4 => GetRed(ref _controls, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _anchor, 
			4 => _controls, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitControlMoveChange(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitControlMoveChange(this);
	}

	public ControlMoveChangeSyntax Update(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax anchor, SyntaxToken semicolonToken, SeparatedSyntaxList<IdentifierNameSyntax> controls, SyntaxToken closeParenthesisToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || anchor != Anchor || semicolonToken != SemicolonToken || controls != Controls || closeParenthesisToken != CloseParenthesisToken)
		{
			ControlMoveChangeSyntax controlMoveChangeSyntax = SyntaxFactory.ControlMoveChange(changeKeyword, openParenthesisToken, anchor, semicolonToken, controls, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return controlMoveChangeSyntax.WithAnnotations(annotations);
			}
			return controlMoveChangeSyntax;
		}
		return this;
	}

	public ControlMoveChangeSyntax WithChangeKeyword(SyntaxToken changeKeyword)
	{
		return Update(changeKeyword, OpenParenthesisToken, Anchor, SemicolonToken, Controls, CloseParenthesisToken);
	}

	public ControlMoveChangeSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ChangeKeyword, openParenthesisToken, Anchor, SemicolonToken, Controls, CloseParenthesisToken);
	}

	public ControlMoveChangeSyntax WithAnchor(IdentifierNameSyntax anchor)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, anchor, SemicolonToken, Controls, CloseParenthesisToken);
	}

	public ControlMoveChangeSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, semicolonToken, Controls, CloseParenthesisToken);
	}

	public ControlMoveChangeSyntax WithControls(SeparatedSyntaxList<IdentifierNameSyntax> controls)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, SemicolonToken, controls, CloseParenthesisToken);
	}

	public ControlMoveChangeSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Anchor, SemicolonToken, Controls, closeParenthesisToken);
	}

	public ControlMoveChangeSyntax AddControls(params IdentifierNameSyntax[] items)
	{
		return WithControls(Controls.AddRange(items));
	}
}
