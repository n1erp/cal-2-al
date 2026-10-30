using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class PageChartPartSyntax : ControlBaseSyntax
{
	private IdentifierNameSyntax _name;

	private IdentifierNameSyntax _chartPartType;

	private PropertyListSyntax _propertyList;

	public override SyntaxToken ControlKeyword => new SyntaxToken(this, ((InternalPageChartPartSyntax)Node)._controlKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalPageChartPartSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken SemicolonToken => new SyntaxToken(this, ((InternalPageChartPartSyntax)Node)._semicolonToken, GetChildPosition(3), GetChildIndex(3));

	public IdentifierNameSyntax ChartPartType => GetRed(ref _chartPartType, 4);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalPageChartPartSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalPageChartPartSyntax)Node)._openBraceToken, GetChildPosition(6), GetChildIndex(6));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 7);

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalPageChartPartSyntax)Node)._closeBraceToken, GetChildPosition(8), GetChildIndex(8));

	internal PageChartPartSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref _name, 2), 
			4 => GetRed(ref _chartPartType, 4), 
			7 => GetRed(ref _propertyList, 7), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)_name, 
			4 => _chartPartType, 
			7 => _propertyList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPageChartPart(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitPageChartPart(this);
	}

	public PageChartPartSyntax Update(SyntaxToken controlKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken semicolonToken, IdentifierNameSyntax chartPartType, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxToken closeBraceToken)
	{
		if (controlKeyword != ControlKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || semicolonToken != SemicolonToken || chartPartType != ChartPartType || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || closeBraceToken != CloseBraceToken)
		{
			PageChartPartSyntax pageChartPartSyntax = SyntaxFactory.PageChartPart(controlKeyword, openParenthesisToken, name, semicolonToken, chartPartType, closeParenthesisToken, openBraceToken, propertyList, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return pageChartPartSyntax.WithAnnotations(annotations);
			}
			return pageChartPartSyntax;
		}
		return this;
	}

	public PageChartPartSyntax WithControlKeyword(SyntaxToken controlKeyword)
	{
		return Update(controlKeyword, OpenParenthesisToken, Name, SemicolonToken, ChartPartType, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageChartPartSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(ControlKeyword, openParenthesisToken, Name, SemicolonToken, ChartPartType, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageChartPartSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ControlKeyword, OpenParenthesisToken, name, SemicolonToken, ChartPartType, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageChartPartSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, semicolonToken, ChartPartType, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageChartPartSyntax WithChartPartType(IdentifierNameSyntax chartPartType)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, chartPartType, CloseParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageChartPartSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, ChartPartType, closeParenthesisToken, OpenBraceToken, PropertyList, CloseBraceToken);
	}

	public PageChartPartSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, ChartPartType, CloseParenthesisToken, openBraceToken, PropertyList, CloseBraceToken);
	}

	public PageChartPartSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, ChartPartType, CloseParenthesisToken, OpenBraceToken, propertyList, CloseBraceToken);
	}

	public PageChartPartSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ControlKeyword, OpenParenthesisToken, Name, SemicolonToken, ChartPartType, CloseParenthesisToken, OpenBraceToken, PropertyList, closeBraceToken);
	}

	public PageChartPartSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}
}
