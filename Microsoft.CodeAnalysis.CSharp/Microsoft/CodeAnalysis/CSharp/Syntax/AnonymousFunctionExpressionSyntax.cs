using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class AnonymousFunctionExpressionSyntax : ExpressionSyntax
{
	public abstract SyntaxToken AsyncKeyword { get; }

	public abstract CSharpSyntaxNode Body { get; }

	internal AnonymousFunctionExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public AnonymousFunctionExpressionSyntax WithAsyncKeyword(SyntaxToken asyncKeyword)
	{
		return WithAsyncKeywordCore(asyncKeyword);
	}

	internal abstract AnonymousFunctionExpressionSyntax WithAsyncKeywordCore(SyntaxToken asyncKeyword);

	public AnonymousFunctionExpressionSyntax WithBody(CSharpSyntaxNode body)
	{
		return WithBodyCore(body);
	}

	internal abstract AnonymousFunctionExpressionSyntax WithBodyCore(CSharpSyntaxNode body);
}
