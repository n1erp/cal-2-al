using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class AnonymousMethodExpressionSyntax : AnonymousFunctionExpressionSyntax
{
	private ParameterListSyntax parameterList;

	private CSharpSyntaxNode body;

	public override SyntaxToken AsyncKeyword
	{
		get
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken asyncKeyword = ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AnonymousMethodExpressionSyntax)Green).asyncKeyword;
			if (asyncKeyword != null)
			{
				return new SyntaxToken(this, asyncKeyword, Position, 0);
			}
			return default;
		}
	}

	public SyntaxToken DelegateKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.AnonymousMethodExpressionSyntax)Green).delegateKeyword, GetChildPosition(1), GetChildIndex(1));

	public ParameterListSyntax ParameterList => GetRed(ref parameterList, 2);

	public override CSharpSyntaxNode Body => GetRed(ref body, 3);

	public BlockSyntax Block => (BlockSyntax)Body;

	internal AnonymousMethodExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)GetRed(ref parameterList, 2), 
			3 => GetRed(ref body, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => (SyntaxNode)parameterList, 
			3 => body, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitAnonymousMethodExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitAnonymousMethodExpression(this);
	}

	public AnonymousMethodExpressionSyntax Update(SyntaxToken asyncKeyword, SyntaxToken delegateKeyword, ParameterListSyntax parameterList, CSharpSyntaxNode body)
	{
		if (asyncKeyword != AsyncKeyword || delegateKeyword != DelegateKeyword || parameterList != ParameterList || body != Body)
		{
			AnonymousMethodExpressionSyntax anonymousMethodExpressionSyntax = SyntaxFactory.AnonymousMethodExpression(asyncKeyword, delegateKeyword, parameterList, body);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return anonymousMethodExpressionSyntax.WithAnnotations(annotations);
			}
			return anonymousMethodExpressionSyntax;
		}
		return this;
	}

	internal override AnonymousFunctionExpressionSyntax WithAsyncKeywordCore(SyntaxToken asyncKeyword)
	{
		return WithAsyncKeyword(asyncKeyword);
	}

	public new AnonymousMethodExpressionSyntax WithAsyncKeyword(SyntaxToken asyncKeyword)
	{
		return Update(asyncKeyword, DelegateKeyword, ParameterList, Body);
	}

	public AnonymousMethodExpressionSyntax WithDelegateKeyword(SyntaxToken delegateKeyword)
	{
		return Update(AsyncKeyword, delegateKeyword, ParameterList, Body);
	}

	public AnonymousMethodExpressionSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return Update(AsyncKeyword, DelegateKeyword, parameterList, Body);
	}

	internal override AnonymousFunctionExpressionSyntax WithBodyCore(CSharpSyntaxNode body)
	{
		return WithBody(body);
	}

	public new AnonymousMethodExpressionSyntax WithBody(CSharpSyntaxNode body)
	{
		return Update(AsyncKeyword, DelegateKeyword, ParameterList, body);
	}

	public AnonymousMethodExpressionSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		ParameterListSyntax parameterListSyntax = ParameterList ?? SyntaxFactory.ParameterList();
		return WithParameterList(parameterListSyntax.WithParameters(parameterListSyntax.Parameters.AddRange(items)));
	}

	public AnonymousMethodExpressionSyntax WithBlock(BlockSyntax block)
	{
		return Update(AsyncKeyword, DelegateKeyword, ParameterList, block);
	}

	public AnonymousMethodExpressionSyntax AddBlockStatements(params StatementSyntax[] items)
	{
		return WithBlock(Block.WithStatements(Block.Statements.AddRange(items)));
	}
}
