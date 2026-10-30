using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageAreaSyntax : ControlGroupBaseSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _controls;

	public override SyntaxToken ControlKeyword => new SyntaxToken(this, ((InternalPageAreaSyntax)Node)._controlKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalPageAreaSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalPageAreaSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageAreaSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public override SyntaxList<ControlBaseSyntax> Controls => new SyntaxList<ControlBaseSyntax>(GetRed(ref _controls, 6));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageAreaSyntax)Node)._closeBraceToken, GetChildPosition(7), GetChildIndex(7));

	internal PageAreaSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _name, 2), 
			5 => GetRed(ref _propertyList, 5), 
			6 => GetRed(ref _controls, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _name, 
			5 => _propertyList, 
			6 => _controls, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageArea(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageArea(this);
	}

	public PageAreaSyntax Update(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<ControlBaseSyntax> controls, SyntaxToken closeBraceToken)
	{
		if (controlKeyword != ControlKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || controls != Controls || closeBraceToken != CloseBraceToken)
		{
			PageAreaSyntax pageAreaSyntax = SyntaxFactory.PageArea(controlKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, controls, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageAreaSyntax.WithAnnotations(annotations);
			}
			return pageAreaSyntax;
		}
		return this;
	}

	public PageAreaSyntax WithControlKeyword(SyntaxToken controlKeyword)
	{
		return Update(controlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Controls, CloseBraceToken);
	}

	public PageAreaSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ControlKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Controls, CloseBraceToken);
	}

	public PageAreaSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ControlKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, Controls, CloseBraceToken);
	}

	public PageAreaSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, Controls, CloseBraceToken);
	}

	public PageAreaSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, Controls, CloseBraceToken);
	}

	public PageAreaSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, Controls, CloseBraceToken);
	}

	public PageAreaSyntax WithControls(SyntaxList<ControlBaseSyntax> controls)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, controls, CloseBraceToken);
	}

	public PageAreaSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Controls, closeBraceToken);
	}

	public PageAreaSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public PageAreaSyntax AddControls(params ControlBaseSyntax[] items)
	{
		return WithControls(Controls.AddRange(items));
	}
}
