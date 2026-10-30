using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ArraySyntax : SyntaxNode
{
	private BracketedDimensionListSyntax _dimensionList;

	public SyntaxToken ArrayKeyword => new SyntaxToken(this, ((InternalArraySyntax)Node)._arrayKeyword, Position, 0);

	public BracketedDimensionListSyntax DimensionList => GetRed(ref _dimensionList, 1);

	public SyntaxToken OfKeyword => new SyntaxToken(this, ((InternalArraySyntax)Node)._ofKeyword, GetChildPosition(2), GetChildIndex(2));

	internal ArraySyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _dimensionList, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _dimensionList;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitArray(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitArray(this);
	}

	public ArraySyntax Update(SyntaxToken arrayKeyword, BracketedDimensionListSyntax dimensionList, SyntaxToken ofKeyword)
	{
		if (arrayKeyword != ArrayKeyword || dimensionList != DimensionList || ofKeyword != OfKeyword)
		{
			ArraySyntax arraySyntax = SyntaxFactory.Array(arrayKeyword, dimensionList, ofKeyword);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return arraySyntax.WithAnnotations(annotations);
			}
			return arraySyntax;
		}
		return this;
	}

	public ArraySyntax WithArrayKeyword(SyntaxToken arrayKeyword)
	{
		return Update(arrayKeyword, DimensionList, OfKeyword);
	}

	public ArraySyntax WithDimensionList(BracketedDimensionListSyntax dimensionList)
	{
		return Update(ArrayKeyword, dimensionList, OfKeyword);
	}

	public ArraySyntax WithOfKeyword(SyntaxToken ofKeyword)
	{
		return Update(ArrayKeyword, DimensionList, ofKeyword);
	}

	public ArraySyntax AddDimensionListDimensions(params DimensionSyntax[] items)
	{
		return WithDimensionList(DimensionList.WithDimensions(DimensionList.Dimensions.AddRange(items)));
	}
}
