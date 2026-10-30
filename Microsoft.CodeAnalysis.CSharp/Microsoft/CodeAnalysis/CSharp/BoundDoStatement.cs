using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundDoStatement : BoundLoopStatement
{
	public ImmutableArray<LocalSymbol> Locals { get; }

	public BoundExpression Condition { get; }

	public BoundStatement Body { get; }

	public BoundDoStatement(SyntaxNode syntax, ImmutableArray<LocalSymbol> locals, BoundExpression condition, BoundStatement body, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel, bool hasErrors = false)
		: base(BoundKind.DoStatement, syntax, breakLabel, continueLabel, hasErrors || condition.HasErrors() || body.HasErrors())
	{
		Locals = locals;
		Condition = condition;
		Body = body;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitDoStatement(this);
	}

	public BoundDoStatement Update(ImmutableArray<LocalSymbol> locals, BoundExpression condition, BoundStatement body, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel)
	{
		if (locals != Locals || condition != Condition || body != Body || breakLabel != BreakLabel || continueLabel != ContinueLabel)
		{
			return new BoundDoStatement(Syntax, locals, condition, body, breakLabel, continueLabel, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
