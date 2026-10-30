using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSwitchStatement : BoundStatement
{
	public BoundStatement LoweredPreambleOpt { get; }

	public BoundExpression Expression { get; }

	public LabelSymbol ConstantTargetOpt { get; }

	public ImmutableArray<LocalSymbol> InnerLocals { get; }

	public ImmutableArray<LocalFunctionSymbol> InnerLocalFunctions { get; }

	public ImmutableArray<BoundSwitchSection> SwitchSections { get; }

	public GeneratedLabelSymbol BreakLabel { get; }

	public MethodSymbol StringEquality { get; }

	public BoundSwitchStatement(SyntaxNode syntax, BoundStatement loweredPreambleOpt, BoundExpression expression, LabelSymbol constantTargetOpt, ImmutableArray<LocalSymbol> innerLocals, ImmutableArray<LocalFunctionSymbol> innerLocalFunctions, ImmutableArray<BoundSwitchSection> switchSections, GeneratedLabelSymbol breakLabel, MethodSymbol stringEquality, bool hasErrors = false)
		: base(BoundKind.SwitchStatement, syntax, hasErrors || loweredPreambleOpt.HasErrors() || expression.HasErrors() || switchSections.HasErrors())
	{
		LoweredPreambleOpt = loweredPreambleOpt;
		Expression = expression;
		ConstantTargetOpt = constantTargetOpt;
		InnerLocals = innerLocals;
		InnerLocalFunctions = innerLocalFunctions;
		SwitchSections = switchSections;
		BreakLabel = breakLabel;
		StringEquality = stringEquality;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSwitchStatement(this);
	}

	public BoundSwitchStatement Update(BoundStatement loweredPreambleOpt, BoundExpression expression, LabelSymbol constantTargetOpt, ImmutableArray<LocalSymbol> innerLocals, ImmutableArray<LocalFunctionSymbol> innerLocalFunctions, ImmutableArray<BoundSwitchSection> switchSections, GeneratedLabelSymbol breakLabel, MethodSymbol stringEquality)
	{
		if (loweredPreambleOpt != LoweredPreambleOpt || expression != Expression || constantTargetOpt != ConstantTargetOpt || innerLocals != InnerLocals || innerLocalFunctions != InnerLocalFunctions || switchSections != SwitchSections || breakLabel != BreakLabel || stringEquality != StringEquality)
		{
			return new BoundSwitchStatement(Syntax, loweredPreambleOpt, expression, constantTargetOpt, innerLocals, innerLocalFunctions, switchSections, breakLabel, stringEquality, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
