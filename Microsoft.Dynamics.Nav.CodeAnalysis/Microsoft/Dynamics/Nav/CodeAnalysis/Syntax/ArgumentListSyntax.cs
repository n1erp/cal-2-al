using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ArgumentListSyntax : SyntaxNode
{
	private SyntaxNode _arguments;

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalArgumentListSyntax)Node)._openParenthesisToken, Position, 0);

	public SeparatedSyntaxList<CodeExpressionSyntax> Arguments
	{
		get
		{
			SyntaxNode red = GetRed(ref _arguments, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<CodeExpressionSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalArgumentListSyntax)Node)._closeParenthesisToken, GetChildPosition(2), GetChildIndex(2));

	internal ArgumentListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _arguments, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _arguments;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitArgumentList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitArgumentList(this);
	}

	public ArgumentListSyntax Update(SyntaxToken openParenthesisToken, SeparatedSyntaxList<CodeExpressionSyntax> arguments, SyntaxToken closeParenthesisToken)
	{
		if (openParenthesisToken != OpenParenthesisToken || arguments != Arguments || closeParenthesisToken != CloseParenthesisToken)
		{
			ArgumentListSyntax argumentListSyntax = SyntaxFactory.ArgumentList(openParenthesisToken, arguments, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return argumentListSyntax.WithAnnotations(annotations);
			}
			return argumentListSyntax;
		}
		return this;
	}

	public ArgumentListSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(openParenthesisToken, Arguments, CloseParenthesisToken);
	}

	public ArgumentListSyntax WithArguments(SeparatedSyntaxList<CodeExpressionSyntax> arguments)
	{
		return Update(OpenParenthesisToken, arguments, CloseParenthesisToken);
	}

	public ArgumentListSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(OpenParenthesisToken, Arguments, closeParenthesisToken);
	}

	public ArgumentListSyntax AddArguments(params CodeExpressionSyntax[] items)
	{
		return WithArguments(Arguments.AddRange(items));
	}
}
