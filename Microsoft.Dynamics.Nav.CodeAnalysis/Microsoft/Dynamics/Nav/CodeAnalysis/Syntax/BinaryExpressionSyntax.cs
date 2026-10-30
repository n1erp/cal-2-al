using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class BinaryExpressionSyntax : CodeExpressionSyntax
{
	private CodeExpressionSyntax _left;

	private CodeExpressionSyntax _right;

	public CodeExpressionSyntax Left => GetRedAtZero(ref _left);

	public SyntaxToken OperatorToken => new SyntaxToken(this, ((InternalBinaryExpressionSyntax)Node)._operatorToken, GetChildPosition(1), GetChildIndex(1));

	public CodeExpressionSyntax Right => GetRed(ref _right, 2);

	internal BinaryExpressionSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _left), 
			2 => GetRed(ref _right, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_left, 
			2 => _right, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBinaryExpression(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBinaryExpression(this);
	}

	public BinaryExpressionSyntax Update(CodeExpressionSyntax left, SyntaxToken operatorToken, CodeExpressionSyntax right)
	{
		if (left != Left || operatorToken != OperatorToken || right != Right)
		{
			BinaryExpressionSyntax binaryExpressionSyntax = SyntaxFactory.BinaryExpression(Kind, left, operatorToken, right);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return binaryExpressionSyntax.WithAnnotations(annotations);
			}
			return binaryExpressionSyntax;
		}
		return this;
	}

	public BinaryExpressionSyntax WithLeft(CodeExpressionSyntax left)
	{
		return Update(left, OperatorToken, Right);
	}

	public BinaryExpressionSyntax WithOperatorToken(SyntaxToken operatorToken)
	{
		return Update(Left, operatorToken, Right);
	}

	public BinaryExpressionSyntax WithRight(CodeExpressionSyntax right)
	{
		return Update(Left, OperatorToken, right);
	}
}
