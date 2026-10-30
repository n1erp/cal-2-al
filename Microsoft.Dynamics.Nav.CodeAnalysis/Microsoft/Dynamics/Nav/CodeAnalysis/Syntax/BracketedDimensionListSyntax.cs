using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class BracketedDimensionListSyntax : SyntaxNode
{
	private SyntaxNode _dimensions;

	public SyntaxToken OpenBracketToken => new SyntaxToken(this, ((InternalBracketedDimensionListSyntax)Node)._openBracketToken, Position, 0);

	public SeparatedSyntaxList<DimensionSyntax> Dimensions
	{
		get
		{
			SyntaxNode red = GetRed(ref _dimensions, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<DimensionSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken CloseBracketToken => new SyntaxToken(this, ((InternalBracketedDimensionListSyntax)Node)._closeBracketToken, GetChildPosition(2), GetChildIndex(2));

	internal BracketedDimensionListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _dimensions, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _dimensions;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBracketedDimensionList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBracketedDimensionList(this);
	}

	public BracketedDimensionListSyntax Update(SyntaxToken openBracketToken, SeparatedSyntaxList<DimensionSyntax> dimensions, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || dimensions != Dimensions || closeBracketToken != CloseBracketToken)
		{
			BracketedDimensionListSyntax bracketedDimensionListSyntax = SyntaxFactory.BracketedDimensionList(openBracketToken, dimensions, closeBracketToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return bracketedDimensionListSyntax.WithAnnotations(annotations);
			}
			return bracketedDimensionListSyntax;
		}
		return this;
	}

	public BracketedDimensionListSyntax WithOpenBracketToken(SyntaxToken openBracketToken)
	{
		return Update(openBracketToken, Dimensions, CloseBracketToken);
	}

	public BracketedDimensionListSyntax WithDimensions(SeparatedSyntaxList<DimensionSyntax> dimensions)
	{
		return Update(OpenBracketToken, dimensions, CloseBracketToken);
	}

	public BracketedDimensionListSyntax WithCloseBracketToken(SyntaxToken closeBracketToken)
	{
		return Update(OpenBracketToken, Dimensions, closeBracketToken);
	}

	public BracketedDimensionListSyntax AddDimensions(params DimensionSyntax[] items)
	{
		return WithDimensions(Dimensions.AddRange(items));
	}
}
