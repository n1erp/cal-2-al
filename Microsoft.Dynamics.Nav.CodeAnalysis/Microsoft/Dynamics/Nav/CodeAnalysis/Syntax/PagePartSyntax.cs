using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PagePartSyntax : ControlBaseSyntax
{
	private IdentifierNameSyntax _name;

	private ObjectNameOrIdSyntax _partName;

	private PropertyListSyntax _propertyList;

	public override SyntaxToken ControlKeyword => new SyntaxToken(this, ((InternalPagePartSyntax)Node)._controlKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalPagePartSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalPagePartSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public ObjectNameOrIdSyntax PartName => GetRed(ref _partName, 4);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalPagePartSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPagePartSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPagePartSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal PagePartSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref _name, 2), 
			4 => GetRed(ref _partName, 4), 
			7 => GetRed(ref _propertyList, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)_name, 
			4 => _partName, 
			7 => _propertyList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPagePart(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPagePart(this);
	}

	public PagePartSyntax Update(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, ObjectNameOrIdSyntax partName, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (controlKeyword != ControlKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || partName != PartName || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			PagePartSyntax pagePartSyntax = SyntaxFactory.PagePart(controlKeyword, openParenthesisToken, name, semicolonToken, partName, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pagePartSyntax.WithAnnotations(annotations);
			}
			return pagePartSyntax;
		}
		return this;
	}

	public PagePartSyntax WithControlKeyword(SyntaxToken controlKeyword)
	{
		return Update(controlKeyword, OpenParenthesisToken, Name, SemicolonToken, PartName, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PagePartSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ControlKeyword, openParenthesisToken, Name, SemicolonToken, PartName, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PagePartSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ControlKeyword, OpenParenthesisToken, name, SemicolonToken, PartName, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PagePartSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, semicolonToken, PartName, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PagePartSyntax WithPartName(ObjectNameOrIdSyntax partName)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, partName, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PagePartSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, PartName, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PagePartSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, PartName, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public PagePartSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, PartName, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public PagePartSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, PartName, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public PagePartSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}
}
