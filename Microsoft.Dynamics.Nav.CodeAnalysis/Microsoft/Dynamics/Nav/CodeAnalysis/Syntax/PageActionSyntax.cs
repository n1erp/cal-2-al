using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageActionSyntax : ActionBaseSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _triggers;

	public override SyntaxToken ActionKeyword => new SyntaxToken(this, ((InternalPageActionSyntax)Node)._actionKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalPageActionSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalPageActionSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageActionSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public SyntaxList<TriggerDeclarationSyntax> Triggers => new SyntaxList<TriggerDeclarationSyntax>(GetRed(ref _triggers, 6));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageActionSyntax)Node)._closeBraceToken, GetChildPosition(7), GetChildIndex(7));

	internal PageActionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _name, 2), 
			5 => GetRed(ref _propertyList, 5), 
			6 => GetRed(ref _triggers, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _name, 
			5 => _propertyList, 
			6 => _triggers, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageAction(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageAction(this);
	}

	public PageActionSyntax Update(SyntaxToken actionKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		if (actionKeyword != ActionKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			PageActionSyntax pageActionSyntax = SyntaxFactory.PageAction(actionKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageActionSyntax.WithAnnotations(annotations);
			}
			return pageActionSyntax;
		}
		return this;
	}

	public PageActionSyntax WithActionKeyword(SyntaxToken actionKeyword)
	{
		return Update(actionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public PageActionSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ActionKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public PageActionSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ActionKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public PageActionSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public PageActionSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public PageActionSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, Triggers, CloseBraceToken);
	}

	public PageActionSyntax WithTriggers(SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, triggers, CloseBraceToken);
	}

	public PageActionSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ActionKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, closeBraceToken);
	}

	public PageActionSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public PageActionSyntax AddTriggers(params TriggerDeclarationSyntax[] items)
	{
		return WithTriggers(Triggers.AddRange(items));
	}
}
