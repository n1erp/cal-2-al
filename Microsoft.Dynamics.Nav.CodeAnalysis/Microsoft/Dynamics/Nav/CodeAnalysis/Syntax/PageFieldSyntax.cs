using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageFieldSyntax : ControlBaseSyntax
{
	private IdentifierNameSyntax _name;

	private CodeExpressionSyntax _expression;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _triggers;

	public override SyntaxToken ControlKeyword => new SyntaxToken(this, ((InternalPageFieldSyntax)Node)._controlKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalPageFieldSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalPageFieldSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public CodeExpressionSyntax Expression => GetRed(ref _expression, 4);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalPageFieldSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageFieldSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public SyntaxList<TriggerDeclarationSyntax> Triggers => new SyntaxList<TriggerDeclarationSyntax>(GetRed(ref _triggers, 8));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageFieldSyntax)Node)._closeBraceToken, GetChildPosition(9), GetChildIndex(9));

	internal PageFieldSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _name, 2), 
			4 => GetRed(ref _expression, 4), 
			7 => GetRed(ref _propertyList, 7), 
			8 => GetRed(ref _triggers, 8), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _name, 
			4 => _expression, 
			7 => _propertyList, 
			8 => _triggers, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageField(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageField(this);
	}

	public PageFieldSyntax Update(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, CodeExpressionSyntax expression, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		if (controlKeyword != ControlKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || expression != Expression || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			PageFieldSyntax pageFieldSyntax = SyntaxFactory.PageField(controlKeyword, openParenthesisToken, name, semicolonToken, expression, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageFieldSyntax.WithAnnotations(annotations);
			}
			return pageFieldSyntax;
		}
		return this;
	}

	public PageFieldSyntax WithControlKeyword(SyntaxToken controlKeyword)
	{
		return Update(controlKeyword, OpenParenthesisToken, Name, SemicolonToken, Expression, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public PageFieldSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ControlKeyword, openParenthesisToken, Name, SemicolonToken, Expression, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public PageFieldSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ControlKeyword, OpenParenthesisToken, name, SemicolonToken, Expression, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public PageFieldSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, semicolonToken, Expression, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public PageFieldSyntax WithExpression(CodeExpressionSyntax expression)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, expression, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public PageFieldSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, Expression, closeParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public PageFieldSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, Expression, CloseParenthesisToken, openBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public PageFieldSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, Expression, CloseParenthesisToken, OpenBraceToken, propertyList, Triggers, CloseBraceToken);
	}

	public PageFieldSyntax WithTriggers(SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, Expression, CloseParenthesisToken, OpenBraceToken, PropertyList, triggers, CloseBraceToken);
	}

	public PageFieldSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, Expression, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, closeBraceToken);
	}

	public PageFieldSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public PageFieldSyntax AddTriggers(params TriggerDeclarationSyntax[] items)
	{
		return WithTriggers(Triggers.AddRange(items));
	}
}
