using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageExtensionLayoutSyntax : SyntaxNode
{
	private SyntaxNode _changes;

	public SyntaxToken LayoutKeyword => new SyntaxToken(this, ((InternalPageExtensionLayoutSyntax)Node)._layoutKeyword, Position, 0);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageExtensionLayoutSyntax)Node)._openBraceToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxList<ControlChangeBaseSyntax> Changes => new SyntaxList<ControlChangeBaseSyntax>(GetRed(ref _changes, 2));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageExtensionLayoutSyntax)Node)._closeBraceToken, GetChildPosition(3), GetChildIndex(3));

	internal PageExtensionLayoutSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref _changes, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return _changes;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageExtensionLayout(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageExtensionLayout(this);
	}

	public PageExtensionLayoutSyntax Update(SyntaxToken layoutKeyword, SyntaxToken openBraceToken, SyntaxList<ControlChangeBaseSyntax> changes, SyntaxToken closeBraceToken)
	{
		if (layoutKeyword != LayoutKeyword || openBraceToken != OpenBraceToken || changes != Changes || closeBraceToken != CloseBraceToken)
		{
			PageExtensionLayoutSyntax pageExtensionLayoutSyntax = SyntaxFactory.PageExtensionLayout(layoutKeyword, openBraceToken, changes, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageExtensionLayoutSyntax.WithAnnotations(annotations);
			}
			return pageExtensionLayoutSyntax;
		}
		return this;
	}

	public PageExtensionLayoutSyntax WithLayoutKeyword(SyntaxToken layoutKeyword)
	{
		return Update(layoutKeyword, OpenBraceToken, Changes, CloseBraceToken);
	}

	public PageExtensionLayoutSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(LayoutKeyword, openBraceToken, Changes, CloseBraceToken);
	}

	public PageExtensionLayoutSyntax WithChanges(SyntaxList<ControlChangeBaseSyntax> changes)
	{
		return Update(LayoutKeyword, OpenBraceToken, changes, CloseBraceToken);
	}

	public PageExtensionLayoutSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(LayoutKeyword, OpenBraceToken, Changes, closeBraceToken);
	}

	public PageExtensionLayoutSyntax AddChanges(params ControlChangeBaseSyntax[] items)
	{
		return WithChanges(Changes.AddRange(items));
	}
}
