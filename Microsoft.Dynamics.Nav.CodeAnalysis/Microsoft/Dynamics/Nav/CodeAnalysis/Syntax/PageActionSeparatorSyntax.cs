using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageActionSeparatorSyntax : ActionBaseSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	public override SyntaxToken ActionKeyword => new SyntaxToken(this, ((InternalPageActionSeparatorSyntax)Node)._actionKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalPageActionSeparatorSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalPageActionSeparatorSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageActionSeparatorSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageActionSeparatorSyntax)Node)._closeBraceToken, GetChildPosition(6), GetChildIndex(6));

	internal PageActionSeparatorSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref _name, 2), 
			5 => GetRed(ref _propertyList, 5), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)_name, 
			5 => _propertyList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageActionSeparator(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageActionSeparator(this);
	}

	public PageActionSeparatorSyntax Update(SyntaxToken actionKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (actionKeyword != ActionKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			PageActionSeparatorSyntax pageActionSeparatorSyntax = SyntaxFactory.PageActionSeparator(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageActionSeparatorSyntax.WithAnnotations(annotations);
			}
			return pageActionSeparatorSyntax;
		}
		return this;
	}

	public PageActionSeparatorSyntax WithActionKeyword(SyntaxToken actionKeyword)
	{
		return Update(actionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageActionSeparatorSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ActionKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageActionSeparatorSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ActionKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageActionSeparatorSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageActionSeparatorSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public PageActionSeparatorSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public PageActionSeparatorSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public PageActionSeparatorSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}
}
