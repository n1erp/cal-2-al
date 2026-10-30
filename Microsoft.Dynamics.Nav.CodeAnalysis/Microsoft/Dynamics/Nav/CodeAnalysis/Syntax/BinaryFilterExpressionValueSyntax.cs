using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class BinaryFilterExpressionValueSyntax : FilterExpressionValueSyntax
{
	private SyntaxNode _left;

	private SyntaxNode _right;

	public SyntaxNode Left => GetRedAtZero(ref _left);

	public SyntaxToken OperatorToken => new SyntaxToken(this, ((InternalBinaryFilterExpressionValueSyntax)Node)._operatorToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxNode Right => GetRed(ref _right, 2);

	internal BinaryFilterExpressionValueSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref _left), 
			2 => GetRed(ref _right, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => _left, 
			2 => _right, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBinaryFilterExpressionValue(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBinaryFilterExpressionValue(this);
	}

	public BinaryFilterExpressionValueSyntax Update(SyntaxNode left, SyntaxToken operatorToken, SyntaxNode right)
	{
		if (left != Left || operatorToken != OperatorToken || right != Right)
		{
			BinaryFilterExpressionValueSyntax binaryFilterExpressionValueSyntax = SyntaxFactory.BinaryFilterExpressionValue(Kind, left, operatorToken, right);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return binaryFilterExpressionValueSyntax.WithAnnotations(annotations);
			}
			return binaryFilterExpressionValueSyntax;
		}
		return this;
	}

	public BinaryFilterExpressionValueSyntax WithLeft(SyntaxNode left)
	{
		return Update(left, OperatorToken, Right);
	}

	public BinaryFilterExpressionValueSyntax WithOperatorToken(SyntaxToken operatorToken)
	{
		return Update(Left, operatorToken, Right);
	}

	public BinaryFilterExpressionValueSyntax WithRight(SyntaxNode right)
	{
		return Update(Left, OperatorToken, right);
	}
}
