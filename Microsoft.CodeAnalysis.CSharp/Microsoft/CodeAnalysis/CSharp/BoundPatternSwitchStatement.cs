using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPatternSwitchStatement : BoundStatement
{
	public BoundExpression Expression { get; }

	public bool SomeLabelAlwaysMatches { get; }

	public ImmutableArray<LocalSymbol> InnerLocals { get; }

	public ImmutableArray<LocalFunctionSymbol> InnerLocalFunctions { get; }

	public ImmutableArray<BoundPatternSwitchSection> SwitchSections { get; }

	public BoundPatternSwitchLabel DefaultLabel { get; }

	public GeneratedLabelSymbol BreakLabel { get; }

	public PatternSwitchBinder Binder { get; }

	public bool IsComplete { get; }

	public BoundPatternSwitchStatement(SyntaxNode syntax, BoundExpression expression, bool someLabelAlwaysMatches, ImmutableArray<LocalSymbol> innerLocals, ImmutableArray<LocalFunctionSymbol> innerLocalFunctions, ImmutableArray<BoundPatternSwitchSection> switchSections, BoundPatternSwitchLabel defaultLabel, GeneratedLabelSymbol breakLabel, PatternSwitchBinder binder, bool isComplete, bool hasErrors = false)
		: base(BoundKind.PatternSwitchStatement, syntax, hasErrors || expression.HasErrors() || switchSections.HasErrors() || defaultLabel.HasErrors())
	{
		Expression = expression;
		SomeLabelAlwaysMatches = someLabelAlwaysMatches;
		InnerLocals = innerLocals;
		InnerLocalFunctions = innerLocalFunctions;
		SwitchSections = switchSections;
		DefaultLabel = defaultLabel;
		BreakLabel = breakLabel;
		Binder = binder;
		IsComplete = isComplete;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPatternSwitchStatement(this);
	}

	public BoundPatternSwitchStatement Update(BoundExpression expression, bool someLabelAlwaysMatches, ImmutableArray<LocalSymbol> innerLocals, ImmutableArray<LocalFunctionSymbol> innerLocalFunctions, ImmutableArray<BoundPatternSwitchSection> switchSections, BoundPatternSwitchLabel defaultLabel, GeneratedLabelSymbol breakLabel, PatternSwitchBinder binder, bool isComplete)
	{
		if (expression != Expression || someLabelAlwaysMatches != SomeLabelAlwaysMatches || innerLocals != InnerLocals || innerLocalFunctions != InnerLocalFunctions || switchSections != SwitchSections || defaultLabel != DefaultLabel || breakLabel != BreakLabel || binder != Binder || isComplete != IsComplete)
		{
			return new BoundPatternSwitchStatement(Syntax, expression, someLabelAlwaysMatches, innerLocals, innerLocalFunctions, switchSections, defaultLabel, breakLabel, binder, isComplete, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
