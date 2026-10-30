using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageSystemPartSyntax : ControlBaseSyntax
{
	private IdentifierNameSyntax _name;

	private IdentifierNameSyntax _systemPartType;

	private PropertyListSyntax _propertyList;

	public override SyntaxToken ControlKeyword => new SyntaxToken(this, ((InternalPageSystemPartSyntax)Node)._controlKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalPageSystemPartSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalPageSystemPartSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public IdentifierNameSyntax SystemPartType => GetRed(ref _systemPartType, 4);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalPageSystemPartSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageSystemPartSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageSystemPartSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal PageSystemPartSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref _name, 2), 
			4 => GetRed(ref _systemPartType, 4), 
			7 => GetRed(ref _propertyList, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)_name, 
			4 => _systemPartType, 
			7 => _propertyList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageSystemPart(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageSystemPart(this);
	}

	public PageSystemPartSyntax Update(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, IdentifierNameSyntax systemPartType, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (controlKeyword != ControlKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || systemPartType != SystemPartType || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			PageSystemPartSyntax pageSystemPartSyntax = SyntaxFactory.PageSystemPart(controlKeyword, openParenthesisToken, name, semicolonToken, systemPartType, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageSystemPartSyntax.WithAnnotations(annotations);
			}
			return pageSystemPartSyntax;
		}
		return this;
	}

	public PageSystemPartSyntax WithControlKeyword(SyntaxToken controlKeyword)
	{
		return Update(controlKeyword, OpenParenthesisToken, Name, SemicolonToken, SystemPartType, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageSystemPartSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ControlKeyword, openParenthesisToken, Name, SemicolonToken, SystemPartType, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageSystemPartSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ControlKeyword, OpenParenthesisToken, name, SemicolonToken, SystemPartType, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageSystemPartSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, semicolonToken, SystemPartType, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageSystemPartSyntax WithSystemPartType(IdentifierNameSyntax systemPartType)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, systemPartType, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageSystemPartSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, SystemPartType, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageSystemPartSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, SystemPartType, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public PageSystemPartSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, SystemPartType, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public PageSystemPartSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, SystemPartType, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public PageSystemPartSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}
}
