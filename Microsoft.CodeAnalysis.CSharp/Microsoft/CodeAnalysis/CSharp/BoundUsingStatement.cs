using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundUsingStatement : BoundStatement
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public BoundMultipleLocalDeclarations DeclarationsOpt { get; }

	public BoundExpression ExpressionOpt { get; }

	public Conversion IDisposableConversion { get; }

	public BoundStatement Body { get; }

	public BoundUsingStatement(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, BoundMultipleLocalDeclarations declarationsOpt, BoundExpression expressionOpt, Conversion iDisposableConversion, BoundStatement body, bool hasErrors = false)
		: base(BoundKind.UsingStatement, syntax, hasErrors || declarationsOpt.HasErrors() || expressionOpt.HasErrors() || body.HasErrors())
	{
		Locals = locals;
		DeclarationsOpt = declarationsOpt;
		ExpressionOpt = expressionOpt;
		IDisposableConversion = iDisposableConversion;
		Body = body;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitUsingStatement(this);
	}

	public BoundUsingStatement Update(ImmutableArray<LocalSymbol> locals, BoundMultipleLocalDeclarations declarationsOpt, BoundExpression expressionOpt, Conversion iDisposableConversion, BoundStatement body)
	{
		if (locals != Locals || declarationsOpt != DeclarationsOpt || expressionOpt != ExpressionOpt || iDisposableConversion != IDisposableConversion || body != Body)
		{
			return new BoundUsingStatement(Syntax, locals, declarationsOpt, expressionOpt, iDisposableConversion, body, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
