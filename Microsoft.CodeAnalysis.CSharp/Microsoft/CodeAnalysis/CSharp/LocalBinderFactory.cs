using System;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LocalBinderFactory : CSharpSyntaxWalker
{
	private readonly SmallDictionary<SyntaxNode, Binder> _map;

	private bool _sawYield;

	private readonly ArrayBuilder<SyntaxNode> _methodsWithYields;

	private Symbol _containingMemberOrLambda;

	private Binder _enclosing;

	private readonly SyntaxNode _root;

	private void Visit(CSharpSyntaxNode syntax, Binder enclosing)
	{
		if (_enclosing == enclosing)
		{
			Visit(syntax);
			return;
		}
		Binder enclosing2 = _enclosing;
		_enclosing = enclosing;
		Visit(syntax);
		_enclosing = enclosing2;
	}

	public static SmallDictionary<SyntaxNode, Binder> BuildMap(Symbol containingMemberOrLambda, SyntaxNode syntax, Binder enclosing, ArrayBuilder<SyntaxNode> methodsWithYields, Action<Binder, SyntaxNode> binderUpdatedHandler = null)
	{
		LocalBinderFactory localBinderFactory = new LocalBinderFactory(containingMemberOrLambda, syntax, enclosing, methodsWithYields);
		if (syntax is ExpressionSyntax syntax2)
		{
			enclosing = new ExpressionVariableBinder(syntax, enclosing);
			binderUpdatedHandler?.Invoke(enclosing, syntax);
			localBinderFactory.AddToMap(syntax, enclosing);
			localBinderFactory.Visit(syntax2, enclosing);
		}
		else if (syntax.Kind() != SyntaxKind.Block && syntax is StatementSyntax statementSyntax)
		{
			enclosing = localBinderFactory.GetBinderForPossibleEmbeddedStatement(statementSyntax, enclosing, out var embeddedScopeDesignator);
			binderUpdatedHandler?.Invoke(enclosing, embeddedScopeDesignator);
			if (embeddedScopeDesignator != null)
			{
				localBinderFactory.AddToMap(embeddedScopeDesignator, enclosing);
			}
			localBinderFactory.Visit(statementSyntax, enclosing);
		}
		else
		{
			binderUpdatedHandler?.Invoke(enclosing, null);
			localBinderFactory.Visit((CSharpSyntaxNode)syntax, enclosing);
		}
		if (localBinderFactory._sawYield)
		{
			methodsWithYields.Add(syntax);
		}
		return localBinderFactory._map;
	}

	public override void VisitCompilationUnit(CompilationUnitSyntax node)
	{
		foreach (MemberDeclarationSyntax member in node.Members)
		{
			if (member.Kind() == SyntaxKind.GlobalStatement)
			{
				Visit(member);
			}
		}
	}

	private LocalBinderFactory(Symbol containingMemberOrLambda, SyntaxNode root, Binder enclosing, ArrayBuilder<SyntaxNode> methodsWithYields)
	{
		_map = new SmallDictionary<SyntaxNode, Binder>(ReferenceEqualityComparer.Instance);
		_containingMemberOrLambda = containingMemberOrLambda;
		_enclosing = enclosing;
		_methodsWithYields = methodsWithYields;
		_root = root;
	}

	public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
	{
		Visit(node.Body);
		Visit(node.ExpressionBody);
	}

	public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
	{
		Binder binder = new ExpressionVariableBinder(node, _enclosing);
		AddToMap(node, binder);
		Visit(node.Initializer, binder);
		Visit(node.Body, binder);
		Visit(node.ExpressionBody, binder);
	}

	public override void VisitDestructorDeclaration(DestructorDeclarationSyntax node)
	{
		Visit(node.Body);
		Visit(node.ExpressionBody);
	}

	public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
	{
		Visit(node.Body);
		Visit(node.ExpressionBody);
	}

	public override void VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
	{
		Visit(node.Body);
		Visit(node.ExpressionBody);
	}

	public override void VisitOperatorDeclaration(OperatorDeclarationSyntax node)
	{
		Visit(node.Body);
		Visit(node.ExpressionBody);
	}

	public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
	{
		VisitLambdaExpression(node);
	}

	private void VisitLambdaExpression(LambdaExpressionSyntax node)
	{
		if (_root == node)
		{
			CSharpSyntaxNode body = node.Body;
			if (body.Kind() == SyntaxKind.Block)
			{
				VisitBlock((BlockSyntax)body);
				return;
			}
			ExpressionVariableBinder expressionVariableBinder = new ExpressionVariableBinder(body, _enclosing);
			AddToMap(body, expressionVariableBinder);
			Visit(body, expressionVariableBinder);
		}
	}

	public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
	{
		VisitLambdaExpression(node);
	}

	public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
	{
		bool sawYield = _sawYield;
		Symbol containingMemberOrLambda = _containingMemberOrLambda;
		Binder enclosing = _enclosing;
		LocalFunctionSymbol localFunctionSymbol = FindLocalFunction(node, _enclosing);
		if ((object)localFunctionSymbol != null)
		{
			_containingMemberOrLambda = localFunctionSymbol;
			enclosing = (localFunctionSymbol.IsGenericMethod ? new WithMethodTypeParametersBinder(localFunctionSymbol, _enclosing) : _enclosing);
			enclosing = enclosing.WithUnsafeRegionIfNecessary(node.Modifiers);
			enclosing = new InMethodBinder(localFunctionSymbol, enclosing);
		}
		BlockSyntax body = node.Body;
		if (body != null)
		{
			_sawYield = false;
			Visit(body, enclosing);
			if (_sawYield)
			{
				_methodsWithYields.Add(body);
			}
		}
		ArrowExpressionClauseSyntax expressionBody = node.ExpressionBody;
		if (expressionBody != null)
		{
			_sawYield = false;
			Visit(expressionBody, enclosing);
		}
		_containingMemberOrLambda = containingMemberOrLambda;
		_sawYield = sawYield;
	}

	private static LocalFunctionSymbol FindLocalFunction(LocalFunctionStatementSyntax node, Binder enclosing)
	{
		LocalFunctionSymbol result = null;
		Binder binder = enclosing;
		while (binder != null && !binder.IsLocalFunctionsScopeBinder)
		{
			binder = binder.Next;
		}
		if (binder != null)
		{
			foreach (LocalFunctionSymbol localFunction in binder.LocalFunctions)
			{
				if (localFunction.Locations[0] == node.Identifier.GetLocation())
				{
					result = localFunction;
				}
			}
		}
		return result;
	}

	public override void VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
	{
		ExpressionVariableBinder expressionVariableBinder = new ExpressionVariableBinder(node, _enclosing);
		AddToMap(node, expressionVariableBinder);
		Visit(node.Expression, expressionVariableBinder);
	}

	public override void VisitEqualsValueClause(EqualsValueClauseSyntax node)
	{
		ExpressionVariableBinder expressionVariableBinder = new ExpressionVariableBinder(node, _enclosing);
		AddToMap(node, expressionVariableBinder);
		Visit(node.Value, expressionVariableBinder);
	}

	public override void VisitAttribute(AttributeSyntax node)
	{
		ExpressionVariableBinder expressionVariableBinder = new ExpressionVariableBinder(node, _enclosing);
		AddToMap(node, expressionVariableBinder);
		AttributeArgumentListSyntax argumentList = node.ArgumentList;
		if (argumentList != null && argumentList.Arguments.Count > 0)
		{
			foreach (AttributeArgumentSyntax argument in node.ArgumentList.Arguments)
			{
				Visit(argument.Expression, expressionVariableBinder);
			}
		}
	}

	public override void VisitConstructorInitializer(ConstructorInitializerSyntax node)
	{
		Binder binder = _enclosing.WithAdditionalFlags(BinderFlags.ConstructorInitializer);
		AddToMap(node, binder);
		if (node.ArgumentList != null)
		{
			if (_root == node)
			{
				binder = new ExpressionVariableBinder(node.ArgumentList, binder);
				AddToMap(node.ArgumentList, binder);
			}
			Visit(node.ArgumentList, binder);
		}
	}

	public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
	{
		if (_root == node)
		{
			VisitBlock(node.Block);
		}
	}

	public override void VisitGlobalStatement(GlobalStatementSyntax node)
	{
		Visit(node.Statement);
	}

	public override void VisitBlock(BlockSyntax node)
	{
		BlockBinder blockBinder = new BlockBinder(_enclosing, node);
		AddToMap(node, blockBinder);
		foreach (StatementSyntax statement in node.Statements)
		{
			Visit(statement, blockBinder);
		}
	}

	public override void VisitUsingStatement(UsingStatementSyntax node)
	{
		UsingStatementBinder usingStatementBinder = new UsingStatementBinder(_enclosing, node);
		AddToMap(node, usingStatementBinder);
		ExpressionSyntax expression = node.Expression;
		VariableDeclarationSyntax declaration = node.Declaration;
		if (expression != null)
		{
			Visit(expression, usingStatementBinder);
		}
		else
		{
			foreach (VariableDeclaratorSyntax variable in declaration.Variables)
			{
				Visit(variable, usingStatementBinder);
			}
		}
		VisitPossibleEmbeddedStatement(node.Statement, usingStatementBinder);
	}

	public override void VisitWhileStatement(WhileStatementSyntax node)
	{
		WhileBinder whileBinder = new WhileBinder(_enclosing, node);
		AddToMap(node, whileBinder);
		Visit(node.Condition, whileBinder);
		VisitPossibleEmbeddedStatement(node.Statement, whileBinder);
	}

	public override void VisitDoStatement(DoStatementSyntax node)
	{
		WhileBinder whileBinder = new WhileBinder(_enclosing, node);
		AddToMap(node, whileBinder);
		Visit(node.Condition, whileBinder);
		VisitPossibleEmbeddedStatement(node.Statement, whileBinder);
	}

	public override void VisitForStatement(ForStatementSyntax node)
	{
		Binder binder = new ForLoopBinder(_enclosing, node);
		AddToMap(node, binder);
		VariableDeclarationSyntax declaration = node.Declaration;
		if (declaration != null)
		{
			foreach (VariableDeclaratorSyntax variable in declaration.Variables)
			{
				Visit(variable, binder);
			}
		}
		else
		{
			foreach (ExpressionSyntax initializer in node.Initializers)
			{
				Visit(initializer, binder);
			}
		}
		ExpressionSyntax condition = node.Condition;
		if (condition != null)
		{
			binder = new ExpressionVariableBinder(condition, binder);
			AddToMap(condition, binder);
			Visit(condition, binder);
		}
		SeparatedSyntaxList<ExpressionSyntax> incrementors = node.Incrementors;
		if (incrementors.Count > 0)
		{
			ExpressionListVariableBinder expressionListVariableBinder = new ExpressionListVariableBinder(incrementors, binder);
			AddToMap(incrementors.First(), expressionListVariableBinder);
			foreach (ExpressionSyntax item in incrementors)
			{
				Visit(item, expressionListVariableBinder);
			}
		}
		VisitPossibleEmbeddedStatement(node.Statement, binder);
	}

	private void VisitCommonForEachStatement(CommonForEachStatementSyntax node)
	{
		ExpressionVariableBinder expressionVariableBinder = new ExpressionVariableBinder(node.Expression, _enclosing);
		AddToMap(node.Expression, expressionVariableBinder);
		Visit(node.Expression, expressionVariableBinder);
		ForEachLoopBinder forEachLoopBinder = new ForEachLoopBinder(expressionVariableBinder, node);
		AddToMap(node, forEachLoopBinder);
		VisitPossibleEmbeddedStatement(node.Statement, forEachLoopBinder);
	}

	public override void VisitForEachStatement(ForEachStatementSyntax node)
	{
		VisitCommonForEachStatement(node);
	}

	public override void VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
	{
		VisitCommonForEachStatement(node);
	}

	public override void VisitCheckedStatement(CheckedStatementSyntax node)
	{
		Binder binder = _enclosing.WithCheckedOrUncheckedRegion(node.Kind() == SyntaxKind.CheckedStatement);
		AddToMap(node, binder);
		Visit(node.Block, binder);
	}

	public override void VisitUnsafeStatement(UnsafeStatementSyntax node)
	{
		Binder binder = _enclosing.WithAdditionalFlags(BinderFlags.UnsafeRegion);
		AddToMap(node, binder);
		Visit(node.Block, binder);
	}

	public override void VisitFixedStatement(FixedStatementSyntax node)
	{
		FixedStatementBinder fixedStatementBinder = new FixedStatementBinder(_enclosing, node);
		AddToMap(node, fixedStatementBinder);
		if (node.Declaration != null)
		{
			foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
			{
				Visit(variable, fixedStatementBinder);
			}
		}
		VisitPossibleEmbeddedStatement(node.Statement, fixedStatementBinder);
	}

	public override void VisitLockStatement(LockStatementSyntax node)
	{
		LockBinder lockBinder = new LockBinder(_enclosing, node);
		AddToMap(node, lockBinder);
		Visit(node.Expression, lockBinder);
		StatementSyntax statement = node.Statement;
		Binder binder = lockBinder.WithAdditionalFlags(BinderFlags.InLockBody);
		if (binder != lockBinder)
		{
			AddToMap(statement, binder);
		}
		VisitPossibleEmbeddedStatement(statement, binder);
	}

	public override void VisitSwitchStatement(SwitchStatementSyntax node)
	{
		AddToMap(node.Expression, _enclosing);
		Visit(node.Expression, _enclosing);
		SwitchBinder switchBinder = SwitchBinder.Create(_enclosing, node);
		AddToMap(node, switchBinder);
		foreach (SwitchSectionSyntax section in node.Sections)
		{
			Visit(section, switchBinder);
		}
	}

	public override void VisitSwitchSection(SwitchSectionSyntax node)
	{
		ExpressionVariableBinder expressionVariableBinder = new ExpressionVariableBinder(node, _enclosing);
		AddToMap(node, expressionVariableBinder);
		foreach (SwitchLabelSyntax label in node.Labels)
		{
			switch (label.Kind())
			{
			case SyntaxKind.CasePatternSwitchLabel:
			{
				CasePatternSwitchLabelSyntax casePatternSwitchLabelSyntax = (CasePatternSwitchLabelSyntax)label;
				Visit(casePatternSwitchLabelSyntax.Pattern, expressionVariableBinder);
				if (casePatternSwitchLabelSyntax.WhenClause != null)
				{
					Visit(casePatternSwitchLabelSyntax.WhenClause.Condition, expressionVariableBinder);
				}
				break;
			}
			case SyntaxKind.CaseSwitchLabel:
			{
				CaseSwitchLabelSyntax caseSwitchLabelSyntax = (CaseSwitchLabelSyntax)label;
				Visit(caseSwitchLabelSyntax.Value, expressionVariableBinder);
				break;
			}
			}
		}
		foreach (StatementSyntax statement in node.Statements)
		{
			Visit(statement, expressionVariableBinder);
		}
	}

	public override void VisitIfStatement(IfStatementSyntax node)
	{
		Visit(node.Condition, _enclosing);
		VisitPossibleEmbeddedStatement(node.Statement, _enclosing);
		Visit(node.Else, _enclosing);
	}

	public override void VisitElseClause(ElseClauseSyntax node)
	{
		VisitPossibleEmbeddedStatement(node.Statement, _enclosing);
	}

	public override void VisitLabeledStatement(LabeledStatementSyntax node)
	{
		Visit(node.Statement, _enclosing);
	}

	public override void VisitTryStatement(TryStatementSyntax node)
	{
		if (node.Catches.Any())
		{
			Visit(node.Block, _enclosing.WithAdditionalFlags(BinderFlags.InTryBlockOfTryCatch));
		}
		else
		{
			Visit(node.Block, _enclosing);
		}
		foreach (CatchClauseSyntax @catch in node.Catches)
		{
			Visit(@catch, _enclosing);
		}
		if (node.Finally != null)
		{
			Visit(node.Finally, _enclosing);
		}
	}

	public override void VisitCatchClause(CatchClauseSyntax node)
	{
		CatchClauseBinder catchClauseBinder = new CatchClauseBinder(_enclosing, node);
		AddToMap(node, catchClauseBinder);
		if (node.Filter != null)
		{
			Binder binder = catchClauseBinder.WithAdditionalFlags(BinderFlags.InCatchFilter);
			AddToMap(node.Filter, binder);
			Visit(node.Filter, binder);
		}
		Visit(node.Block, catchClauseBinder);
	}

	public override void VisitCatchFilterClause(CatchFilterClauseSyntax node)
	{
		Visit(node.FilterExpression);
	}

	public override void VisitFinallyClause(FinallyClauseSyntax node)
	{
		BinderFlags binderFlags = BinderFlags.InFinallyBlock;
		if (_enclosing.Flags.Includes(BinderFlags.InCatchBlock))
		{
			binderFlags |= BinderFlags.InNestedFinallyBlock;
		}
		Visit(node.Block, _enclosing.WithAdditionalFlags(binderFlags));
	}

	public override void VisitYieldStatement(YieldStatementSyntax node)
	{
		if (node.Expression != null)
		{
			Visit(node.Expression, _enclosing);
		}
		_sawYield = true;
	}

	public override void VisitExpressionStatement(ExpressionStatementSyntax node)
	{
		Visit(node.Expression, _enclosing);
	}

	public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
	{
		foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
		{
			Visit(variable);
		}
	}

	public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
	{
		Visit(node.ArgumentList);
		Visit(node.Initializer?.Value);
	}

	public override void VisitReturnStatement(ReturnStatementSyntax node)
	{
		if (node.Expression != null)
		{
			Visit(node.Expression, _enclosing);
		}
	}

	public override void VisitThrowStatement(ThrowStatementSyntax node)
	{
		if (node.Expression != null)
		{
			Visit(node.Expression, _enclosing);
		}
	}

	public override void VisitBinaryExpression(BinaryExpressionSyntax node)
	{
		while (true)
		{
			Visit(node.Right);
			if (!(node.Left is BinaryExpressionSyntax binaryExpressionSyntax))
			{
				break;
			}
			node = binaryExpressionSyntax;
		}
		Visit(node.Left);
	}

	public override void DefaultVisit(SyntaxNode node)
	{
		base.DefaultVisit(node);
	}

	private void AddToMap(SyntaxNode node, Binder binder)
	{
		_map[node] = binder;
	}

	private Binder GetBinderForPossibleEmbeddedStatement(StatementSyntax statement, Binder enclosing, out CSharpSyntaxNode embeddedScopeDesignator)
	{
		switch (statement.Kind())
		{
		case SyntaxKind.LocalDeclarationStatement:
		case SyntaxKind.ExpressionStatement:
		case SyntaxKind.LabeledStatement:
		case SyntaxKind.ReturnStatement:
		case SyntaxKind.YieldReturnStatement:
		case SyntaxKind.ThrowStatement:
		case SyntaxKind.LockStatement:
		case SyntaxKind.IfStatement:
		case SyntaxKind.LocalFunctionStatement:
			embeddedScopeDesignator = statement;
			return new EmbeddedStatementBinder(enclosing, statement);
		case SyntaxKind.SwitchStatement:
		{
			SwitchStatementSyntax switchStatementSyntax = (SwitchStatementSyntax)statement;
			embeddedScopeDesignator = switchStatementSyntax.Expression;
			return new ExpressionVariableBinder(switchStatementSyntax.Expression, enclosing);
		}
		default:
			embeddedScopeDesignator = null;
			return enclosing;
		}
	}

	private void VisitPossibleEmbeddedStatement(StatementSyntax statement, Binder enclosing)
	{
		if (statement != null)
		{
			enclosing = GetBinderForPossibleEmbeddedStatement(statement, enclosing, out var embeddedScopeDesignator);
			if (embeddedScopeDesignator != null)
			{
				AddToMap(embeddedScopeDesignator, enclosing);
			}
			Visit(statement, enclosing);
		}
	}

	public override void VisitQueryExpression(QueryExpressionSyntax node)
	{
		Visit(node.FromClause.Expression);
		Visit(node.Body);
	}

	public override void VisitQueryBody(QueryBodySyntax node)
	{
		foreach (QueryClauseSyntax clause in node.Clauses)
		{
			if (clause.Kind() == SyntaxKind.JoinClause)
			{
				Visit(((JoinClauseSyntax)clause).InExpression);
			}
		}
		Visit(node.Continuation);
	}
}
