using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class DebugInfoInjector : CompoundInstrumenter
{
	public static readonly DebugInfoInjector Singleton = new DebugInfoInjector(Instrumenter.NoOp);

	public DebugInfoInjector(Instrumenter previous)
		: base(previous)
	{
	}

	public override BoundStatement InstrumentNoOpStatement(BoundNoOpStatement original, BoundStatement rewritten)
	{
		return AddSequencePoint(base.InstrumentNoOpStatement(original, rewritten));
	}

	public override BoundStatement InstrumentBreakStatement(BoundBreakStatement original, BoundStatement rewritten)
	{
		return AddSequencePoint(base.InstrumentBreakStatement(original, rewritten));
	}

	public override BoundStatement InstrumentContinueStatement(BoundContinueStatement original, BoundStatement rewritten)
	{
		return AddSequencePoint(base.InstrumentContinueStatement(original, rewritten));
	}

	public override BoundStatement InstrumentExpressionStatement(BoundExpressionStatement original, BoundStatement rewritten)
	{
		rewritten = base.InstrumentExpressionStatement(original, rewritten);
		if (original.IsConstructorInitializer())
		{
			switch (original.Syntax.Kind())
			{
			case SyntaxKind.ConstructorDeclaration:
			{
				ConstructorDeclarationSyntax constructorDeclarationSyntax = (ConstructorDeclarationSyntax)original.Syntax;
				return new BoundSequencePointWithSpan(constructorDeclarationSyntax, rewritten, CreateSpanForConstructorInitializer(constructorDeclarationSyntax));
			}
			case SyntaxKind.BaseConstructorInitializer:
			case SyntaxKind.ThisConstructorInitializer:
			{
				ConstructorInitializerSyntax constructorInitializerSyntax = (ConstructorInitializerSyntax)original.Syntax;
				return new BoundSequencePointWithSpan(constructorInitializerSyntax, rewritten, CreateSpanForConstructorInitializer((ConstructorDeclarationSyntax)constructorInitializerSyntax.Parent));
			}
			}
		}
		return AddSequencePoint(rewritten);
	}

	public override BoundStatement InstrumentFieldOrPropertyInitializer(BoundStatement original, BoundStatement rewritten)
	{
		rewritten = base.InstrumentFieldOrPropertyInitializer(original, rewritten);
		SyntaxNode syntax = original.Syntax;
		if (rewritten.Kind == BoundKind.Block)
		{
			BoundBlock boundBlock = (BoundBlock)rewritten;
			return boundBlock.Update(boundBlock.Locals, boundBlock.LocalFunctions, ImmutableArray.Create(InstrumentFieldOrPropertyInitializer(boundBlock.Statements.Single(), syntax)));
		}
		return InstrumentFieldOrPropertyInitializer(rewritten, syntax);
	}

	private static BoundStatement InstrumentFieldOrPropertyInitializer(BoundStatement rewritten, SyntaxNode syntax)
	{
		return syntax.Parent.Parent.Kind() switch
		{
			SyntaxKind.VariableDeclarator => AddSequencePoint((VariableDeclaratorSyntax)syntax.Parent.Parent, rewritten), 
			SyntaxKind.PropertyDeclaration => AddSequencePoint((PropertyDeclarationSyntax)syntax.Parent.Parent, rewritten), 
			_ => throw ExceptionUtilities.UnexpectedValue(syntax.Parent.Parent.Kind()), 
		};
	}

	public override BoundStatement InstrumentGotoStatement(BoundGotoStatement original, BoundStatement rewritten)
	{
		return AddSequencePoint(base.InstrumentGotoStatement(original, rewritten));
	}

	public override BoundStatement InstrumentThrowStatement(BoundThrowStatement original, BoundStatement rewritten)
	{
		return AddSequencePoint(base.InstrumentThrowStatement(original, rewritten));
	}

	public override BoundStatement InstrumentYieldBreakStatement(BoundYieldBreakStatement original, BoundStatement rewritten)
	{
		rewritten = base.InstrumentYieldBreakStatement(original, rewritten);
		if (original.WasCompilerGenerated && original.Syntax.Kind() == SyntaxKind.Block)
		{
			return new BoundSequencePointWithSpan(original.Syntax, rewritten, ((BlockSyntax)original.Syntax).CloseBraceToken.Span);
		}
		return AddSequencePoint(rewritten);
	}

	public override BoundStatement InstrumentYieldReturnStatement(BoundYieldReturnStatement original, BoundStatement rewritten)
	{
		return AddSequencePoint(base.InstrumentYieldReturnStatement(original, rewritten));
	}

	public override BoundStatement CreateBlockPrologue(BoundBlock original, out LocalSymbol synthesizedLocal)
	{
		BoundStatement boundStatement = base.CreateBlockPrologue(original, out synthesizedLocal);
		if (original.Syntax.Kind() == SyntaxKind.Block && !original.WasCompilerGenerated)
		{
			TextSpan span = ((BlockSyntax)original.Syntax).OpenBraceToken.Span;
			return new BoundSequencePointWithSpan(original.Syntax, boundStatement, span);
		}
		if (boundStatement != null)
		{
			return new BoundSequencePoint(original.Syntax, boundStatement);
		}
		return null;
	}

	public override BoundStatement CreateBlockEpilogue(BoundBlock original)
	{
		BoundStatement boundStatement = base.CreateBlockEpilogue(original);
		if (original.Syntax.Kind() == SyntaxKind.Block && !original.WasCompilerGenerated)
		{
			SyntaxNode parent = original.Syntax.Parent;
			if (parent == null || (!parent.IsAnonymousFunction() && !(parent is BaseMethodDeclarationSyntax)))
			{
				TextSpan span = ((BlockSyntax)original.Syntax).CloseBraceToken.Span;
				return new BoundSequencePointWithSpan(original.Syntax, boundStatement, span);
			}
		}
		return boundStatement;
	}

	public override BoundExpression InstrumentDoStatementCondition(BoundDoStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return AddConditionSequencePoint(base.InstrumentDoStatementCondition(original, rewrittenCondition, factory), original.Syntax, factory);
	}

	public override BoundExpression InstrumentWhileStatementCondition(BoundWhileStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return AddConditionSequencePoint(base.InstrumentWhileStatementCondition(original, rewrittenCondition, factory), original.Syntax, factory);
	}

	public override BoundStatement InstrumentDoStatementConditionalGotoStart(BoundDoStatement original, BoundStatement ifConditionGotoStart)
	{
		DoStatementSyntax doStatementSyntax = (DoStatementSyntax)original.Syntax;
		TextSpan span = TextSpan.FromBounds(doStatementSyntax.WhileKeyword.SpanStart, doStatementSyntax.SemicolonToken.Span.End);
		return new BoundSequencePointWithSpan(doStatementSyntax, base.InstrumentDoStatementConditionalGotoStart(original, ifConditionGotoStart), span);
	}

	public override BoundStatement InstrumentWhileStatementConditionalGotoStartOrBreak(BoundWhileStatement original, BoundStatement ifConditionGotoStart)
	{
		WhileStatementSyntax whileStatementSyntax = (WhileStatementSyntax)original.Syntax;
		TextSpan span = TextSpan.FromBounds(whileStatementSyntax.WhileKeyword.SpanStart, whileStatementSyntax.CloseParenToken.Span.End);
		return new BoundSequencePointWithSpan(whileStatementSyntax, base.InstrumentWhileStatementConditionalGotoStartOrBreak(original, ifConditionGotoStart), span);
	}

	private static BoundExpression AddConditionSequencePoint(BoundExpression condition, BoundStatement containingStatement, SyntheticBoundNodeFactory factory)
	{
		return AddConditionSequencePoint(condition, containingStatement.Syntax, factory);
	}

	public override BoundStatement InstrumentForEachStatementCollectionVarDeclaration(BoundForEachStatement original, BoundStatement collectionVarDecl)
	{
		return new BoundSequencePoint(((CommonForEachStatementSyntax)original.Syntax).Expression, base.InstrumentForEachStatementCollectionVarDeclaration(original, collectionVarDecl));
	}

	public override BoundStatement InstrumentForEachStatementDeconstructionVariablesDeclaration(BoundForEachStatement original, BoundStatement iterationVarDecl)
	{
		ForEachVariableStatementSyntax forEachVariableStatementSyntax = (ForEachVariableStatementSyntax)original.Syntax;
		return new BoundSequencePointWithSpan(forEachVariableStatementSyntax, base.InstrumentForEachStatementDeconstructionVariablesDeclaration(original, iterationVarDecl), forEachVariableStatementSyntax.Variable.Span);
	}

	public override BoundStatement InstrumentForEachStatement(BoundForEachStatement original, BoundStatement rewritten)
	{
		CommonForEachStatementSyntax commonForEachStatementSyntax = (CommonForEachStatementSyntax)original.Syntax;
		BoundSequencePointWithSpan item = new BoundSequencePointWithSpan(commonForEachStatementSyntax, null, commonForEachStatementSyntax.ForEachKeyword.Span);
		return new BoundStatementList(commonForEachStatementSyntax, ImmutableArray.Create(item, base.InstrumentForEachStatement(original, rewritten)));
	}

	public override BoundStatement InstrumentForEachStatementIterationVarDeclaration(BoundForEachStatement original, BoundStatement iterationVarDecl)
	{
		TextSpan span;
		switch (original.Syntax.Kind())
		{
		case SyntaxKind.ForEachStatement:
		{
			ForEachStatementSyntax forEachStatementSyntax = (ForEachStatementSyntax)original.Syntax;
			span = TextSpan.FromBounds(forEachStatementSyntax.Type.SpanStart, forEachStatementSyntax.Identifier.Span.End);
			break;
		}
		case SyntaxKind.ForEachVariableStatement:
			span = ((ForEachVariableStatementSyntax)original.Syntax).Variable.Span;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(original.Syntax.Kind());
		}
		return new BoundSequencePointWithSpan(original.Syntax, base.InstrumentForEachStatementIterationVarDeclaration(original, iterationVarDecl), span);
	}

	public override BoundStatement InstrumentForStatementConditionalGotoStartOrBreak(BoundForStatement original, BoundStatement branchBack)
	{
		return new BoundSequencePoint(original.Condition?.Syntax, base.InstrumentForStatementConditionalGotoStartOrBreak(original, branchBack));
	}

	public override BoundStatement InstrumentForEachStatementConditionalGotoStart(BoundForEachStatement original, BoundStatement branchBack)
	{
		CommonForEachStatementSyntax commonForEachStatementSyntax = (CommonForEachStatementSyntax)original.Syntax;
		return new BoundSequencePointWithSpan(commonForEachStatementSyntax, base.InstrumentForEachStatementConditionalGotoStart(original, branchBack), commonForEachStatementSyntax.InKeyword.Span);
	}

	public override BoundExpression InstrumentForStatementCondition(BoundForStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return AddConditionSequencePoint(base.InstrumentForStatementCondition(original, rewrittenCondition, factory), original.Syntax, factory);
	}

	public override BoundStatement InstrumentIfStatement(BoundIfStatement original, BoundStatement rewritten)
	{
		IfStatementSyntax ifStatementSyntax = (IfStatementSyntax)original.Syntax;
		return new BoundSequencePointWithSpan(ifStatementSyntax, base.InstrumentIfStatement(original, rewritten), TextSpan.FromBounds(ifStatementSyntax.IfKeyword.SpanStart, ifStatementSyntax.CloseParenToken.Span.End), original.HasErrors);
	}

	public override BoundExpression InstrumentIfStatementCondition(BoundIfStatement original, BoundExpression rewrittenCondition, SyntheticBoundNodeFactory factory)
	{
		return AddConditionSequencePoint(base.InstrumentIfStatementCondition(original, rewrittenCondition, factory), original.Syntax, factory);
	}

	public override BoundStatement InstrumentLabelStatement(BoundLabeledStatement original, BoundStatement rewritten)
	{
		LabeledStatementSyntax labeledStatementSyntax = (LabeledStatementSyntax)original.Syntax;
		TextSpan span = TextSpan.FromBounds(labeledStatementSyntax.Identifier.SpanStart, labeledStatementSyntax.ColonToken.Span.End);
		return new BoundSequencePointWithSpan(labeledStatementSyntax, base.InstrumentLabelStatement(original, rewritten), span);
	}

	public override BoundStatement InstrumentLocalInitialization(BoundLocalDeclaration original, BoundStatement rewritten)
	{
		return AddSequencePoint((original.Syntax.Kind() == SyntaxKind.VariableDeclarator) ? ((VariableDeclaratorSyntax)original.Syntax) : ((LocalDeclarationStatementSyntax)original.Syntax).Declaration.Variables.First(), base.InstrumentLocalInitialization(original, rewritten));
	}

	public override BoundStatement InstrumentLockTargetCapture(BoundLockStatement original, BoundStatement lockTargetCapture)
	{
		LockStatementSyntax lockStatementSyntax = (LockStatementSyntax)original.Syntax;
		return new BoundSequencePointWithSpan(lockStatementSyntax, base.InstrumentLockTargetCapture(original, lockTargetCapture), TextSpan.FromBounds(lockStatementSyntax.LockKeyword.SpanStart, lockStatementSyntax.CloseParenToken.Span.End));
	}

	public override BoundStatement InstrumentReturnStatement(BoundReturnStatement original, BoundStatement rewritten)
	{
		rewritten = base.InstrumentReturnStatement(original, rewritten);
		if (original.WasCompilerGenerated && original.ExpressionOpt == null && original.Syntax.Kind() == SyntaxKind.Block)
		{
			return new BoundSequencePointWithSpan(original.Syntax, rewritten, ((BlockSyntax)original.Syntax).CloseBraceToken.Span);
		}
		return new BoundSequencePoint(original.Syntax, rewritten);
	}

	public override BoundStatement InstrumentSwitchStatement(BoundSwitchStatement original, BoundStatement rewritten)
	{
		SwitchStatementSyntax switchStatementSyntax = (SwitchStatementSyntax)original.Syntax;
		TextSpan span = TextSpan.FromBounds(switchStatementSyntax.SwitchKeyword.SpanStart, switchStatementSyntax.CloseParenToken.Span.End);
		return new BoundSequencePointWithSpan(switchStatementSyntax, base.InstrumentSwitchStatement(original, rewritten), span);
	}

	public override BoundStatement InstrumentPatternSwitchStatement(BoundPatternSwitchStatement original, BoundStatement rewritten)
	{
		SwitchStatementSyntax switchStatementSyntax = (SwitchStatementSyntax)original.Syntax;
		TextSpan span = TextSpan.FromBounds(switchStatementSyntax.SwitchKeyword.SpanStart, switchStatementSyntax.CloseParenToken.Span.End);
		return new BoundSequencePointWithSpan(switchStatementSyntax, base.InstrumentPatternSwitchStatement(original, rewritten), span);
	}

	public override BoundStatement InstrumentPatternSwitchWhenClauseConditionalGotoBody(BoundExpression original, BoundStatement ifConditionGotoBody)
	{
		WhenClauseSyntax whenClauseSyntax = original.Syntax.FirstAncestorOrSelf<WhenClauseSyntax>();
		return new BoundSequencePointWithSpan(whenClauseSyntax, base.InstrumentPatternSwitchWhenClauseConditionalGotoBody(original, ifConditionGotoBody), whenClauseSyntax.Span);
	}

	public override BoundStatement InstrumentUsingTargetCapture(BoundUsingStatement original, BoundStatement usingTargetCapture)
	{
		return AddSequencePoint((UsingStatementSyntax)original.Syntax, base.InstrumentUsingTargetCapture(original, usingTargetCapture));
	}

	public override BoundExpression InstrumentCatchClauseFilter(BoundCatchBlock original, BoundExpression rewrittenFilter, SyntheticBoundNodeFactory factory)
	{
		rewrittenFilter = base.InstrumentCatchClauseFilter(original, rewrittenFilter, factory);
		CatchFilterClauseSyntax filter = ((CatchClauseSyntax)original.Syntax).Filter;
		return AddConditionSequencePoint(new BoundSequencePointExpression(filter, rewrittenFilter, rewrittenFilter.Type), filter, factory);
	}

	public override BoundExpression InstrumentSwitchStatementExpression(BoundStatement original, BoundExpression rewrittenExpression, SyntheticBoundNodeFactory factory)
	{
		return AddConditionSequencePoint(base.InstrumentSwitchStatementExpression(original, rewrittenExpression, factory), original.Syntax, factory);
	}

	public override BoundStatement InstrumentPatternSwitchBindCasePatternVariables(BoundStatement bindings)
	{
		return new BoundSequencePoint(null, base.InstrumentPatternSwitchBindCasePatternVariables(bindings));
	}

	private BoundStatement AddSequencePoint(BoundStatement node)
	{
		return new BoundSequencePoint(node.Syntax, node);
	}

	internal static BoundStatement AddSequencePoint(VariableDeclaratorSyntax declaratorSyntax, BoundStatement rewrittenStatement)
	{
		GetBreakpointSpan(declaratorSyntax, out var _, out var part);
		BoundStatement boundStatement = BoundSequencePoint.Create(declaratorSyntax, part, rewrittenStatement);
		boundStatement.WasCompilerGenerated = rewrittenStatement.WasCompilerGenerated;
		return boundStatement;
	}

	internal static BoundStatement AddSequencePoint(PropertyDeclarationSyntax declarationSyntax, BoundStatement rewrittenStatement)
	{
		int spanStart = declarationSyntax.Initializer.Value.SpanStart;
		int end = declarationSyntax.Initializer.Span.End;
		TextSpan value = TextSpan.FromBounds(spanStart, end);
		BoundStatement boundStatement = BoundSequencePoint.Create(declarationSyntax, value, rewrittenStatement);
		boundStatement.WasCompilerGenerated = rewrittenStatement.WasCompilerGenerated;
		return boundStatement;
	}

	internal static BoundStatement AddSequencePoint(UsingStatementSyntax usingSyntax, BoundStatement rewrittenStatement)
	{
		int start = usingSyntax.Span.Start;
		int end = usingSyntax.CloseParenToken.Span.End;
		TextSpan span = TextSpan.FromBounds(start, end);
		return new BoundSequencePointWithSpan(usingSyntax, rewrittenStatement, span);
	}

	private static TextSpan CreateSpanForConstructorInitializer(ConstructorDeclarationSyntax constructorSyntax)
	{
		if (constructorSyntax.Initializer != null)
		{
			int spanStart = constructorSyntax.Initializer.ThisOrBaseKeyword.SpanStart;
			int end = constructorSyntax.Initializer.ArgumentList.CloseParenToken.Span.End;
			return TextSpan.FromBounds(spanStart, end);
		}
		if (constructorSyntax.Modifiers.Any(SyntaxKind.StaticKeyword))
		{
			int spanStart2 = constructorSyntax.Body.OpenBraceToken.SpanStart;
			int end2 = constructorSyntax.Body.OpenBraceToken.Span.End;
			return TextSpan.FromBounds(spanStart2, end2);
		}
		return CreateSpan(constructorSyntax.Modifiers, constructorSyntax.Identifier, constructorSyntax.ParameterList.CloseParenToken);
	}

	private static TextSpan CreateSpan(SyntaxTokenList startOpt, SyntaxNodeOrToken startFallbackOpt, SyntaxNodeOrToken endOpt)
	{
		int start;
		if (startOpt.Count > 0)
		{
			start = startOpt.First().SpanStart;
		}
		else
		{
			start = ((!(startFallbackOpt != default(SyntaxNodeOrToken))) ? endOpt.SpanStart : startFallbackOpt.SpanStart);
		}
		int end = ((!(endOpt != default(SyntaxNodeOrToken))) ? GetEndPosition(startFallbackOpt) : GetEndPosition(endOpt));
		return TextSpan.FromBounds(start, end);
	}

	private static int GetEndPosition(SyntaxNodeOrToken nodeOrToken)
	{
		if (nodeOrToken.IsToken)
		{
			return nodeOrToken.Span.End;
		}
		return nodeOrToken.AsNode().GetLastToken().Span.End;
	}

	internal static void GetBreakpointSpan(VariableDeclaratorSyntax declaratorSyntax, out SyntaxNode node, out TextSpan? part)
	{
		VariableDeclarationSyntax variableDeclarationSyntax = (VariableDeclarationSyntax)declaratorSyntax.Parent;
		if (variableDeclarationSyntax.Variables.First() == declaratorSyntax)
		{
			switch (variableDeclarationSyntax.Parent.Kind())
			{
			case SyntaxKind.FieldDeclaration:
			case SyntaxKind.EventFieldDeclaration:
				GetFirstLocalOrFieldBreakpointSpan(((BaseFieldDeclarationSyntax)variableDeclarationSyntax.Parent).Modifiers, declaratorSyntax, out node, out part);
				break;
			case SyntaxKind.LocalDeclarationStatement:
				GetFirstLocalOrFieldBreakpointSpan(default, declaratorSyntax, out node, out part);
				break;
			case SyntaxKind.ForStatement:
			case SyntaxKind.UsingStatement:
			case SyntaxKind.FixedStatement:
				node = variableDeclarationSyntax;
				part = TextSpan.FromBounds(variableDeclarationSyntax.SpanStart, declaratorSyntax.Span.End);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(variableDeclarationSyntax.Parent.Kind());
			}
		}
		else
		{
			node = declaratorSyntax;
			part = null;
		}
	}

	internal static void GetFirstLocalOrFieldBreakpointSpan(SyntaxTokenList modifiers, VariableDeclaratorSyntax declaratorSyntax, out SyntaxNode node, out TextSpan? part)
	{
		VariableDeclarationSyntax variableDeclarationSyntax = (VariableDeclarationSyntax)declaratorSyntax.Parent;
		int start = (modifiers.Any() ? modifiers[0].SpanStart : variableDeclarationSyntax.SpanStart);
		int end = ((variableDeclarationSyntax.Variables.Count != 1) ? declaratorSyntax.Span.End : variableDeclarationSyntax.Parent.Span.End);
		part = TextSpan.FromBounds(start, end);
		node = variableDeclarationSyntax.Parent;
	}

	private static BoundExpression AddConditionSequencePoint(BoundExpression condition, SyntaxNode synthesizedVariableSyntax, SyntheticBoundNodeFactory factory)
	{
		if (!factory.Compilation.Options.EnableEditAndContinue)
		{
			return condition;
		}
		LocalSymbol localSymbol = factory.SynthesizedLocal(condition.Type, synthesizedVariableSyntax, isPinned: false, RefKind.None, SynthesizedLocalKind.ConditionalBranchDiscriminator);
		BoundExpression value = ((condition.ConstantValue == null) ? new BoundSequencePointExpression(null, factory.Local(localSymbol), condition.Type) : condition);
		return new BoundSequence(condition.Syntax, ImmutableArray.Create(localSymbol), ImmutableArray.Create((BoundExpression)factory.AssignmentExpression(factory.Local(localSymbol), condition)), value, condition.Type);
	}
}
