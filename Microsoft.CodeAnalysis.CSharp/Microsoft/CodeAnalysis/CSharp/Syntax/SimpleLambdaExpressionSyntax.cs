using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SimpleLambdaExpressionSyntax : LambdaExpressionSyntax
{
	private ParameterSyntax parameter;

	private CSharpSyntaxNode body;

	public override SyntaxToken AsyncKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken asyncKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SimpleLambdaExpressionSyntax)Green).asyncKeyword;
			if (asyncKeyword != null)
			{
				return new SyntaxToken(this, asyncKeyword, Position, 0);
			}
			return default;
		}
	}

	public ParameterSyntax Parameter => GetRed(ref parameter, 1);

	public override SyntaxToken ArrowToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SimpleLambdaExpressionSyntax)Green).arrowToken, GetChildPosition(2), GetChildIndex(2));

	public override CSharpSyntaxNode Body => GetRed(ref body, 3);

	internal SimpleLambdaExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref parameter, 1), 
			3 => GetRed(ref body, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)parameter, 
			3 => body, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSimpleLambdaExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSimpleLambdaExpression(this);
	}

	public SimpleLambdaExpressionSyntax Update(SyntaxToken asyncKeyword, ParameterSyntax parameter, SyntaxToken arrowToken, CSharpSyntaxNode body)
	{
		if (asyncKeyword != AsyncKeyword || parameter != Parameter || arrowToken != ArrowToken || body != Body)
		{
			SimpleLambdaExpressionSyntax simpleLambdaExpressionSyntax = SyntaxFactory.SimpleLambdaExpression(asyncKeyword, parameter, arrowToken, body);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return simpleLambdaExpressionSyntax.WithAnnotations(annotations);
			}
			return simpleLambdaExpressionSyntax;
		}
		return this;
	}

	internal override AnonymousFunctionExpressionSyntax WithAsyncKeywordCore(SyntaxToken asyncKeyword)
	{
		return WithAsyncKeyword(asyncKeyword);
	}

	public new SimpleLambdaExpressionSyntax WithAsyncKeyword(SyntaxToken asyncKeyword)
	{
		return Update(asyncKeyword, Parameter, ArrowToken, Body);
	}

	public SimpleLambdaExpressionSyntax WithParameter(ParameterSyntax parameter)
	{
		return Update(AsyncKeyword, parameter, ArrowToken, Body);
	}

	internal override LambdaExpressionSyntax WithArrowTokenCore(SyntaxToken arrowToken)
	{
		return WithArrowToken(arrowToken);
	}

	public new SimpleLambdaExpressionSyntax WithArrowToken(SyntaxToken arrowToken)
	{
		return Update(AsyncKeyword, Parameter, arrowToken, Body);
	}

	internal override AnonymousFunctionExpressionSyntax WithBodyCore(CSharpSyntaxNode body)
	{
		return WithBody(body);
	}

	public new SimpleLambdaExpressionSyntax WithBody(CSharpSyntaxNode body)
	{
		return Update(AsyncKeyword, Parameter, ArrowToken, body);
	}

	public SimpleLambdaExpressionSyntax AddParameterAttributeLists(params AttributeListSyntax[] items)
	{
		return WithParameter(Parameter.WithAttributeLists(Parameter.AttributeLists.AddRange(items)));
	}

	public SimpleLambdaExpressionSyntax AddParameterModifiers(params SyntaxToken[] items)
	{
		return WithParameter(Parameter.WithModifiers(Parameter.Modifiers.AddRange(items)));
	}
}
