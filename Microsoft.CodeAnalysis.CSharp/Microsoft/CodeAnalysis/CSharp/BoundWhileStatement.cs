using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundWhileStatement : BoundLoopStatement
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public BoundExpression Condition { get; }

	public BoundStatement Body { get; }

	public BoundWhileStatement(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, BoundExpression condition, BoundStatement body, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel, bool hasErrors = false)
		: base(BoundKind.WhileStatement, syntax, breakLabel, continueLabel, hasErrors || condition.HasErrors() || body.HasErrors())
	{
		Locals = locals;
		Condition = condition;
		Body = body;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitWhileStatement(this);
	}

	public BoundWhileStatement Update(ImmutableArray<LocalSymbol> locals, BoundExpression condition, BoundStatement body, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel)
	{
		if (locals != Locals || condition != Condition || body != Body || breakLabel != BreakLabel || continueLabel != ContinueLabel)
		{
			return new BoundWhileStatement(Syntax, locals, condition, body, breakLabel, continueLabel, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
