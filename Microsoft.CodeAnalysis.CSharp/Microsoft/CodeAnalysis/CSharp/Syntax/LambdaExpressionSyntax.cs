using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public abstract class LambdaExpressionSyntax : AnonymousFunctionExpressionSyntax
{
	public abstract SyntaxToken ArrowToken { get; }

	internal LambdaExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	public LambdaExpressionSyntax WithArrowToken(SyntaxToken arrowToken)
	{
		return WithArrowTokenCore(arrowToken);
	}

	internal abstract LambdaExpressionSyntax WithArrowTokenCore(SyntaxToken arrowToken);

	public new LambdaExpressionSyntax WithAsyncKeyword(SyntaxToken asyncKeyword)
	{
		return (LambdaExpressionSyntax)WithAsyncKeywordCore(asyncKeyword);
	}

	public new LambdaExpressionSyntax WithBody(CSharpSyntaxNode body)
	{
		return (LambdaExpressionSyntax)WithBodyCore(body);
	}
}
