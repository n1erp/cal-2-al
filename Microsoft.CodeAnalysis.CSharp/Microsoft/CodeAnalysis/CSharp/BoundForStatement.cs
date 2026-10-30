using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundForStatement : BoundLoopStatement
{
	public ImmutableArray<LocalSymbol> OuterLocals { get; }

	public BoundStatement Initializer { get; }

	public ImmutableArray<LocalSymbol> InnerLocals { get; }

	public BoundExpression Condition { get; }

	public BoundStatement Increment { get; }

	public BoundStatement Body { get; }

	public BoundForStatement(SyntaxNode syntax, ImmutableArray<LocalSymbol> outerLocals, BoundStatement initializer, ImmutableArray<LocalSymbol> innerLocals, BoundExpression condition, BoundStatement increment, BoundStatement body, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel, bool hasErrors = false)
		: base(BoundKind.ForStatement, syntax, breakLabel, continueLabel, hasErrors || initializer.HasErrors() || condition.HasErrors() || increment.HasErrors() || body.HasErrors())
	{
		OuterLocals = outerLocals;
		Initializer = initializer;
		InnerLocals = innerLocals;
		Condition = condition;
		Increment = increment;
		Body = body;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitForStatement(this);
	}

	public BoundForStatement Update(ImmutableArray<LocalSymbol> outerLocals, BoundStatement initializer, ImmutableArray<LocalSymbol> innerLocals, BoundExpression condition, BoundStatement increment, BoundStatement body, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel)
	{
		if (outerLocals != OuterLocals || initializer != Initializer || innerLocals != InnerLocals || condition != Condition || increment != Increment || body != Body || breakLabel != BreakLabel || continueLabel != ContinueLabel)
		{
			return new BoundForStatement(Syntax, outerLocals, initializer, innerLocals, condition, increment, body, breakLabel, continueLabel, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
