using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundLocalFunctionStatement : BoundStatement, IBoundLambdaOrFunction
{
	public BoundBlock Body => BlockBody ?? ExpressionBody;

	MethodSymbol IBoundLambdaOrFunction.Symbol => Symbol;

	SyntaxNode IBoundLambdaOrFunction.Syntax => Syntax;

	BoundBlock IBoundLambdaOrFunction.Body => Body;

	public LocalFunctionSymbol Symbol { get; }

	public BoundBlock BlockBody { get; }

	public BoundBlock ExpressionBody { get; }

	public BoundLocalFunctionStatement(SyntaxNode syntax, LocalFunctionSymbol symbol, BoundBlock blockBody, BoundBlock expressionBody, bool hasErrors = false)
		: base(BoundKind.LocalFunctionStatement, syntax, hasErrors || blockBody.HasErrors() || expressionBody.HasErrors())
	{
		Symbol = symbol;
		BlockBody = blockBody;
		ExpressionBody = expressionBody;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitLocalFunctionStatement(this);
	}

	public BoundLocalFunctionStatement Update(LocalFunctionSymbol symbol, BoundBlock blockBody, BoundBlock expressionBody)
	{
		if (symbol != Symbol || blockBody != BlockBody || expressionBody != ExpressionBody)
		{
			return new BoundLocalFunctionStatement(Syntax, symbol, blockBody, expressionBody, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
