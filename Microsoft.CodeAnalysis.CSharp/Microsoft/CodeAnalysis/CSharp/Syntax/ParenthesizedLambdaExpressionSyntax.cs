using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ParenthesizedLambdaExpressionSyntax : LambdaExpressionSyntax
{
	private ParameterListSyntax parameterList;

	private CSharpSyntaxNode body;

	public override SyntaxToken AsyncKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken asyncKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParenthesizedLambdaExpressionSyntax)Green).asyncKeyword;
			if (asyncKeyword != null)
			{
				return new SyntaxToken(this, asyncKeyword, Position, 0);
			}
			return default;
		}
	}

	public ParameterListSyntax ParameterList => GetRed(ref parameterList, 1);

	public override SyntaxToken ArrowToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.ParenthesizedLambdaExpressionSyntax)Green).arrowToken, GetChildPosition(2), GetChildIndex(2));

	public override CSharpSyntaxNode Body => GetRed(ref body, 3);

	internal ParenthesizedLambdaExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref parameterList, 1), 
			3 => GetRed(ref body, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)parameterList, 
			3 => body, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitParenthesizedLambdaExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitParenthesizedLambdaExpression(this);
	}

	public ParenthesizedLambdaExpressionSyntax Update(SyntaxToken asyncKeyword, ParameterListSyntax parameterList, SyntaxToken arrowToken, CSharpSyntaxNode body)
	{
		if (asyncKeyword != AsyncKeyword || parameterList != ParameterList || arrowToken != ArrowToken || body != Body)
		{
			ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax = SyntaxFactory.ParenthesizedLambdaExpression(asyncKeyword, parameterList, arrowToken, body);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return parenthesizedLambdaExpressionSyntax.WithAnnotations(annotations);
			}
			return parenthesizedLambdaExpressionSyntax;
		}
		return this;
	}

	internal override AnonymousFunctionExpressionSyntax WithAsyncKeywordCore(SyntaxToken asyncKeyword)
	{
		return WithAsyncKeyword(asyncKeyword);
	}

	public new ParenthesizedLambdaExpressionSyntax WithAsyncKeyword(SyntaxToken asyncKeyword)
	{
		return Update(asyncKeyword, ParameterList, ArrowToken, Body);
	}

	public ParenthesizedLambdaExpressionSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return Update(AsyncKeyword, parameterList, ArrowToken, Body);
	}

	internal override LambdaExpressionSyntax WithArrowTokenCore(SyntaxToken arrowToken)
	{
		return WithArrowToken(arrowToken);
	}

	public new ParenthesizedLambdaExpressionSyntax WithArrowToken(SyntaxToken arrowToken)
	{
		return Update(AsyncKeyword, ParameterList, arrowToken, Body);
	}

	internal override AnonymousFunctionExpressionSyntax WithBodyCore(CSharpSyntaxNode body)
	{
		return WithBody(body);
	}

	public new ParenthesizedLambdaExpressionSyntax WithBody(CSharpSyntaxNode body)
	{
		return Update(AsyncKeyword, ParameterList, ArrowToken, body);
	}

	public ParenthesizedLambdaExpressionSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
	}
}
