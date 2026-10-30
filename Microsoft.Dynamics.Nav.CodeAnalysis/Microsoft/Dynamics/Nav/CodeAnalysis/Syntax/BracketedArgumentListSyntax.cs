using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class BracketedArgumentListSyntax : SyntaxNode
{
	private SyntaxNode _arguments;

	public SyntaxToken OpenBracketToken => new SyntaxToken(this, ((InternalBracketedArgumentListSyntax)Node)._openBracketToken, Position, 0);

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

	public SyntaxToken CloseBracketToken => new SyntaxToken(this, ((InternalBracketedArgumentListSyntax)Node)._closeBracketToken, GetChildPosition(2), GetChildIndex(2));

	internal BracketedArgumentListSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitBracketedArgumentList(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBracketedArgumentList(this);
	}

	public BracketedArgumentListSyntax Update(SyntaxToken openBracketToken, SeparatedSyntaxList<CodeExpressionSyntax> arguments, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || arguments != Arguments || closeBracketToken != CloseBracketToken)
		{
			BracketedArgumentListSyntax bracketedArgumentListSyntax = SyntaxFactory.BracketedArgumentList(openBracketToken, arguments, closeBracketToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return bracketedArgumentListSyntax.WithAnnotations(annotations);
			}
			return bracketedArgumentListSyntax;
		}
		return this;
	}

	public BracketedArgumentListSyntax WithOpenBracketToken(SyntaxToken openBracketToken)
	{
		return Update(openBracketToken, Arguments, CloseBracketToken);
	}

	public BracketedArgumentListSyntax WithArguments(SeparatedSyntaxList<CodeExpressionSyntax> arguments)
	{
		return Update(OpenBracketToken, arguments, CloseBracketToken);
	}

	public BracketedArgumentListSyntax WithCloseBracketToken(SyntaxToken closeBracketToken)
	{
		return Update(OpenBracketToken, Arguments, closeBracketToken);
	}

	public BracketedArgumentListSyntax AddArguments(params CodeExpressionSyntax[] items)
	{
		return WithArguments(Arguments.AddRange(items));
	}
}
