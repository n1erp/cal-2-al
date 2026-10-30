using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ControlModifyChangeSyntax : ControlChangeBaseSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _triggers;

	public override SyntaxToken ChangeKeyword => new SyntaxToken(this, ((InternalControlModifyChangeSyntax)Node)._changeKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalControlModifyChangeSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalControlModifyChangeSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalControlModifyChangeSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public SyntaxList<TriggerDeclarationSyntax> Triggers => new SyntaxList<TriggerDeclarationSyntax>(GetRed(ref _triggers, 6));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalControlModifyChangeSyntax)Node)._closeBraceToken, GetChildPosition(7), GetChildIndex(7));

	internal ControlModifyChangeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitControlModifyChange(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitControlModifyChange(this);
	}

	public ControlModifyChangeSyntax Update(SyntaxToken changeKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		if (changeKeyword != ChangeKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			ControlModifyChangeSyntax controlModifyChangeSyntax = SyntaxFactory.ControlModifyChange(changeKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return controlModifyChangeSyntax.WithAnnotations(annotations);
			}
			return controlModifyChangeSyntax;
		}
		return this;
	}

	public ControlModifyChangeSyntax WithChangeKeyword(SyntaxToken changeKeyword)
	{
		return Update(changeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public ControlModifyChangeSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ChangeKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public ControlModifyChangeSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public ControlModifyChangeSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public ControlModifyChangeSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public ControlModifyChangeSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, Triggers, CloseBraceToken);
	}

	public ControlModifyChangeSyntax WithTriggers(SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, triggers, CloseBraceToken);
	}

	public ControlModifyChangeSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ChangeKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, closeBraceToken);
	}

	public ControlModifyChangeSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public ControlModifyChangeSyntax AddTriggers(params TriggerDeclarationSyntax[] items)
	{
		return WithTriggers(Triggers.AddRange(items));
	}
}
