using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundForEachStatement : BoundLoopStatement
{
	public ForEachEnumeratorInfo EnumeratorInfoOpt { get; }

	public Conversion ElementConversion { get; }

	public BoundTypeExpression IterationVariableType { get; }

	public ImmutableArray<LocalSymbol> IterationVariables { get; }

	public BoundExpression IterationErrorExpressionOpt { get; }

	public BoundExpression Expression { get; }

	public BoundForEachDeconstructStep DeconstructionOpt { get; }

	public BoundStatement Body { get; }

	public bool Checked { get; }

	public BoundForEachStatement(SyntaxNode syntax, ForEachEnumeratorInfo enumeratorInfoOpt, Conversion elementConversion, BoundTypeExpression iterationVariableType, ImmutableArray<LocalSymbol> iterationVariables, BoundExpression expression, BoundForEachDeconstructStep deconstructionOpt, BoundStatement body, bool @checked, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel, bool hasErrors = false)
		: this(syntax, enumeratorInfoOpt, elementConversion, iterationVariableType, iterationVariables, null, expression, deconstructionOpt, body, @checked, breakLabel, continueLabel, hasErrors)
	{
	}

	public BoundForEachStatement(SyntaxNode syntax, ForEachEnumeratorInfo enumeratorInfoOpt, Conversion elementConversion, BoundTypeExpression iterationVariableType, ImmutableArray<LocalSymbol> iterationVariables, BoundExpression iterationErrorExpressionOpt, BoundExpression expression, BoundForEachDeconstructStep deconstructionOpt, BoundStatement body, bool @checked, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel, bool hasErrors = false)
		: base(BoundKind.ForEachStatement, syntax, breakLabel, continueLabel, hasErrors || iterationVariableType.HasErrors() || iterationErrorExpressionOpt.HasErrors() || expression.HasErrors() || deconstructionOpt.HasErrors() || body.HasErrors())
	{
		EnumeratorInfoOpt = enumeratorInfoOpt;
		ElementConversion = elementConversion;
		IterationVariableType = iterationVariableType;
		IterationVariables = iterationVariables;
		IterationErrorExpressionOpt = iterationErrorExpressionOpt;
		Expression = expression;
		DeconstructionOpt = deconstructionOpt;
		Body = body;
		Checked = @checked;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitForEachStatement(this);
	}

	public BoundForEachStatement Update(ForEachEnumeratorInfo enumeratorInfoOpt, Conversion elementConversion, BoundTypeExpression iterationVariableType, ImmutableArray<LocalSymbol> iterationVariables, BoundExpression iterationErrorExpressionOpt, BoundExpression expression, BoundForEachDeconstructStep deconstructionOpt, BoundStatement body, bool @checked, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel)
	{
		if (enumeratorInfoOpt != EnumeratorInfoOpt || elementConversion != ElementConversion || iterationVariableType != IterationVariableType || iterationVariables != IterationVariables || iterationErrorExpressionOpt != IterationErrorExpressionOpt || expression != Expression || deconstructionOpt != DeconstructionOpt || body != Body || @checked != Checked || breakLabel != BreakLabel || continueLabel != ContinueLabel)
		{
			return new BoundForEachStatement(Syntax, enumeratorInfoOpt, elementConversion, iterationVariableType, iterationVariables, iterationErrorExpressionOpt, expression, deconstructionOpt, body, @checked, breakLabel, continueLabel, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
