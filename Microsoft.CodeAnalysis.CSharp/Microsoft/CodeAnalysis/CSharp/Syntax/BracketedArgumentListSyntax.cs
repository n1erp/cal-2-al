using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class BracketedArgumentListSyntax : BaseArgumentListSyntax
{
	private SyntaxNode arguments;

	public SyntaxToken OpenBracketToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BracketedArgumentListSyntax)Green).openBracketToken, Position, 0);

	public override SeparatedSyntaxList<ArgumentSyntax> Arguments
	{
		get
		{
			SyntaxNode red = GetRed(ref arguments, 1);
			if (red != null)
			{
				return new SeparatedSyntaxList<ArgumentSyntax>(red, GetChildIndex(1));
			}
			return default;
		}
	}

	public SyntaxToken CloseBracketToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BracketedArgumentListSyntax)Green).closeBracketToken, GetChildPosition(2), GetChildIndex(2));

	internal BracketedArgumentListSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref arguments, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return arguments;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBracketedArgumentList(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBracketedArgumentList(this);
	}

	public BracketedArgumentListSyntax Update(SyntaxToken openBracketToken, SeparatedSyntaxList<ArgumentSyntax> arguments, SyntaxToken closeBracketToken)
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

	internal override BaseArgumentListSyntax WithArgumentsCore(SeparatedSyntaxList<ArgumentSyntax> arguments)
	{
		return WithArguments(arguments);
	}

	public new BracketedArgumentListSyntax WithArguments(SeparatedSyntaxList<ArgumentSyntax> arguments)
	{
		return Update(OpenBracketToken, arguments, CloseBracketToken);
	}

	public BracketedArgumentListSyntax WithCloseBracketToken(SyntaxToken closeBracketToken)
	{
		return Update(OpenBracketToken, Arguments, closeBracketToken);
	}

	internal override BaseArgumentListSyntax AddArgumentsCore(params ArgumentSyntax[] items)
	{
		return AddArguments(items);
	}

	public new BracketedArgumentListSyntax AddArguments(params ArgumentSyntax[] items)
	{
		return WithArguments(Arguments.AddRange(items));
	}
}
