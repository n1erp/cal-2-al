using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class ExpressionVariableFinder<TFieldOrLocalSymbol> : CSharpSyntaxWalker where TFieldOrLocalSymbol : Symbol
{
	private ArrayBuilder<TFieldOrLocalSymbol> _variablesBuilder;

	private SyntaxNode _nodeToBind;

	protected void FindExpressionVariables(ArrayBuilder<TFieldOrLocalSymbol> builder, CSharpSyntaxNode node)
	{
		ArrayBuilder<TFieldOrLocalSymbol> variablesBuilder = _variablesBuilder;
		_variablesBuilder = builder;
		VisitNodeToBind(node);
		_variablesBuilder = variablesBuilder;
	}

	public override void Visit(SyntaxNode node)
	{
		if (node != null)
		{
			((CSharpSyntaxNode)node).Accept(this);
		}
	}

	public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
	{
		if (node.ArgumentList != null)
		{
			foreach (ArgumentSyntax argument in node.ArgumentList.Arguments)
			{
				Visit(argument.Expression);
			}
		}
		VisitNodeToBind(node.Initializer);
	}

	private void VisitNodeToBind(CSharpSyntaxNode node)
	{
		SyntaxNode nodeToBind = _nodeToBind;
		_nodeToBind = node;
		Visit(node);
		_nodeToBind = nodeToBind;
	}

	protected void FindExpressionVariables(ArrayBuilder<TFieldOrLocalSymbol> builder, SeparatedSyntaxList<ExpressionSyntax> nodes)
	{
		ArrayBuilder<TFieldOrLocalSymbol> variablesBuilder = _variablesBuilder;
		_variablesBuilder = builder;
		foreach (ExpressionSyntax item in nodes)
		{
			VisitNodeToBind(item);
		}
		_variablesBuilder = variablesBuilder;
	}

	public override void VisitEqualsValueClause(EqualsValueClauseSyntax node)
	{
		VisitNodeToBind(node.Value);
	}

	public override void VisitArrowExpressionClause(ArrowExpressionClauseSyntax node)
	{
		VisitNodeToBind(node.Expression);
	}

	public override void VisitSwitchSection(SwitchSectionSyntax node)
	{
		foreach (SwitchLabelSyntax label in node.Labels)
		{
			switch (label.Kind())
			{
			case SyntaxKind.CasePatternSwitchLabel:
			{
				CasePatternSwitchLabelSyntax casePatternSwitchLabelSyntax = (CasePatternSwitchLabelSyntax)label;
				SyntaxNode nodeToBind = _nodeToBind;
				_nodeToBind = casePatternSwitchLabelSyntax;
				Visit(casePatternSwitchLabelSyntax.Pattern);
				if (casePatternSwitchLabelSyntax.WhenClause != null)
				{
					VisitNodeToBind(casePatternSwitchLabelSyntax.WhenClause.Condition);
				}
				_nodeToBind = nodeToBind;
				break;
			}
			case SyntaxKind.CaseSwitchLabel:
			{
				CaseSwitchLabelSyntax caseSwitchLabelSyntax = (CaseSwitchLabelSyntax)label;
				VisitNodeToBind(caseSwitchLabelSyntax.Value);
				break;
			}
			}
		}
	}

	public override void VisitAttribute(AttributeSyntax node)
	{
		if (node.ArgumentList != null)
		{
			foreach (AttributeArgumentSyntax argument in node.ArgumentList.Arguments)
			{
				VisitNodeToBind(argument.Expression);
			}
		}
	}

	public override void VisitThrowStatement(ThrowStatementSyntax node)
	{
		VisitNodeToBind(node.Expression);
	}

	public override void VisitReturnStatement(ReturnStatementSyntax node)
	{
		VisitNodeToBind(node.Expression);
	}

	public override void VisitYieldStatement(YieldStatementSyntax node)
	{
		VisitNodeToBind(node.Expression);
	}

	public override void VisitExpressionStatement(ExpressionStatementSyntax node)
	{
		VisitNodeToBind(node.Expression);
	}

	public override void VisitLockStatement(LockStatementSyntax node)
	{
		VisitNodeToBind(node.Expression);
	}

	public override void VisitIfStatement(IfStatementSyntax node)
	{
		VisitNodeToBind(node.Condition);
	}

	public override void VisitSwitchStatement(SwitchStatementSyntax node)
	{
		VisitNodeToBind(node.Expression);
	}

	public override void VisitDeclarationPattern(DeclarationPatternSyntax node)
	{
		TFieldOrLocalSymbol val = MakePatternVariable(node, _nodeToBind);
		if ((object)val != null)
		{
			_variablesBuilder.Add(val);
		}
		base.VisitDeclarationPattern(node);
	}

	protected abstract TFieldOrLocalSymbol MakePatternVariable(DeclarationPatternSyntax node, SyntaxNode nodeToBind);

	public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
	{
	}

	public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
	{
	}

	public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
	{
	}

	public override void VisitQueryExpression(QueryExpressionSyntax node)
	{
		VisitNodeToBind(node.FromClause.Expression);
		Visit(node.Body);
	}

	public override void VisitQueryBody(QueryBodySyntax node)
	{
		foreach (QueryClauseSyntax clause in node.Clauses)
		{
			if (clause.Kind() == SyntaxKind.JoinClause)
			{
				VisitNodeToBind(((JoinClauseSyntax)clause).InExpression);
			}
		}
		Visit(node.Continuation);
	}

	public override void VisitBinaryExpression(BinaryExpressionSyntax node)
	{
		ArrayBuilder<ExpressionSyntax> instance = ArrayBuilder<ExpressionSyntax>.GetInstance();
		ExpressionSyntax expressionSyntax = node;
		do
		{
			BinaryExpressionSyntax binaryExpressionSyntax = (BinaryExpressionSyntax)expressionSyntax;
			instance.Push(binaryExpressionSyntax.Right);
			expressionSyntax = binaryExpressionSyntax.Left;
		}
		while (expressionSyntax is BinaryExpressionSyntax);
		Visit(expressionSyntax);
		while (instance.Count > 0)
		{
			Visit(instance.Pop());
		}
		instance.Free();
	}

	public override void VisitDeclarationExpression(DeclarationExpressionSyntax node)
	{
		BaseArgumentListSyntax argumentListSyntaxOpt = (node.Parent as ArgumentSyntax)?.Parent as BaseArgumentListSyntax;
		VisitDeclarationExpressionDesignation(node, node.Designation, argumentListSyntaxOpt);
	}

	private void VisitDeclarationExpressionDesignation(DeclarationExpressionSyntax node, VariableDesignationSyntax designation, BaseArgumentListSyntax argumentListSyntaxOpt)
	{
		switch (designation.Kind())
		{
		case SyntaxKind.SingleVariableDesignation:
		{
			TFieldOrLocalSymbol val = MakeDeclarationExpressionVariable(node, (SingleVariableDesignationSyntax)designation, argumentListSyntaxOpt, _nodeToBind);
			if ((object)val != null)
			{
				_variablesBuilder.Add(val);
			}
			break;
		}
		case SyntaxKind.ParenthesizedVariableDesignation:
			foreach (VariableDesignationSyntax variable in ((ParenthesizedVariableDesignationSyntax)designation).Variables)
			{
				VisitDeclarationExpressionDesignation(node, variable, argumentListSyntaxOpt);
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(designation.Kind());
		case SyntaxKind.DiscardDesignation:
			break;
		}
	}

	public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
	{
		if (node.IsDeconstruction())
		{
			CollectVariablesFromDeconstruction(node.Left, node);
		}
		else
		{
			Visit(node.Left);
		}
		Visit(node.Right);
	}

	public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
	{
		if (node.Initializer != null)
		{
			VisitNodeToBind(node.Initializer);
		}
	}

	private void CollectVariablesFromDeconstruction(ExpressionSyntax possibleTupleDeclaration, AssignmentExpressionSyntax deconstruction)
	{
		switch (possibleTupleDeclaration.Kind())
		{
		case SyntaxKind.TupleExpression:
			foreach (ArgumentSyntax argument in ((TupleExpressionSyntax)possibleTupleDeclaration).Arguments)
			{
				CollectVariablesFromDeconstruction(argument.Expression, deconstruction);
			}
			break;
		case SyntaxKind.DeclarationExpression:
		{
			DeclarationExpressionSyntax declarationExpressionSyntax = (DeclarationExpressionSyntax)possibleTupleDeclaration;
			CollectVariablesFromDeconstruction(declarationExpressionSyntax.Designation, declarationExpressionSyntax.Type, deconstruction);
			break;
		}
		default:
			Visit(possibleTupleDeclaration);
			break;
		}
	}

	private void CollectVariablesFromDeconstruction(VariableDesignationSyntax designation, TypeSyntax closestTypeSyntax, AssignmentExpressionSyntax deconstruction)
	{
		switch (designation.Kind())
		{
		case SyntaxKind.SingleVariableDesignation:
		{
			SingleVariableDesignationSyntax designation2 = (SingleVariableDesignationSyntax)designation;
			TFieldOrLocalSymbol val = MakeDeconstructionVariable(closestTypeSyntax, designation2, deconstruction);
			if ((object)val != null)
			{
				_variablesBuilder.Add(val);
			}
			break;
		}
		case SyntaxKind.ParenthesizedVariableDesignation:
			foreach (VariableDesignationSyntax variable in ((ParenthesizedVariableDesignationSyntax)designation).Variables)
			{
				CollectVariablesFromDeconstruction(variable, closestTypeSyntax, deconstruction);
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(designation.Kind());
		case SyntaxKind.DiscardDesignation:
			break;
		}
	}

	protected abstract TFieldOrLocalSymbol MakeDeclarationExpressionVariable(DeclarationExpressionSyntax node, SingleVariableDesignationSyntax designation, BaseArgumentListSyntax argumentListSyntax, SyntaxNode nodeToBind);

	protected abstract TFieldOrLocalSymbol MakeDeconstructionVariable(TypeSyntax closestTypeSyntax, SingleVariableDesignationSyntax designation, AssignmentExpressionSyntax deconstruction);
}
internal class ExpressionVariableFinder : ExpressionVariableFinder<LocalSymbol>
{
	private Binder _scopeBinder;

	private Binder _enclosingBinder;

	private static readonly ObjectPool<ExpressionVariableFinder> s_poolInstance = CreatePool();

	internal static void FindExpressionVariables(Binder scopeBinder, ArrayBuilder<LocalSymbol> builder, CSharpSyntaxNode node, Binder enclosingBinderOpt = null)
	{
		if (node != null)
		{
			ExpressionVariableFinder expressionVariableFinder = s_poolInstance.Allocate();
			expressionVariableFinder._scopeBinder = scopeBinder;
			expressionVariableFinder._enclosingBinder = enclosingBinderOpt ?? scopeBinder;
			expressionVariableFinder.FindExpressionVariables(builder, node);
			expressionVariableFinder._scopeBinder = null;
			expressionVariableFinder._enclosingBinder = null;
			s_poolInstance.Free(expressionVariableFinder);
		}
	}

	internal static void FindExpressionVariables(Binder binder, ArrayBuilder<LocalSymbol> builder, SeparatedSyntaxList<ExpressionSyntax> nodes)
	{
		if (nodes.Count != 0)
		{
			ExpressionVariableFinder expressionVariableFinder = s_poolInstance.Allocate();
			expressionVariableFinder._scopeBinder = binder;
			expressionVariableFinder._enclosingBinder = binder;
			expressionVariableFinder.FindExpressionVariables(builder, nodes);
			expressionVariableFinder._scopeBinder = null;
			expressionVariableFinder._enclosingBinder = null;
			s_poolInstance.Free(expressionVariableFinder);
		}
	}

	protected override LocalSymbol MakePatternVariable(DeclarationPatternSyntax node, SyntaxNode nodeToBind)
	{
		if (!(node.Designation is SingleVariableDesignationSyntax singleVariableDesignationSyntax))
		{
			return null;
		}
		NamedTypeSymbol containingType = _scopeBinder.ContainingType;
		if ((object)containingType != null && containingType.IsScriptClass && (object)_scopeBinder.LookupDeclaredField(singleVariableDesignationSyntax) != null)
		{
			return null;
		}
		return SourceLocalSymbol.MakeLocalSymbolWithEnclosingContext(_scopeBinder.ContainingMemberOrLambda, _scopeBinder, _enclosingBinder, node.Type, singleVariableDesignationSyntax.Identifier, LocalDeclarationKind.PatternVariable, nodeToBind, null);
	}

	protected override LocalSymbol MakeDeclarationExpressionVariable(DeclarationExpressionSyntax node, SingleVariableDesignationSyntax designation, BaseArgumentListSyntax argumentListSyntaxOpt, SyntaxNode nodeToBind)
	{
		NamedTypeSymbol containingType = _scopeBinder.ContainingType;
		if ((object)containingType != null && containingType.IsScriptClass && (object)_scopeBinder.LookupDeclaredField(designation) != null)
		{
			return null;
		}
		return SourceLocalSymbol.MakeLocalSymbolWithEnclosingContext(_scopeBinder.ContainingMemberOrLambda, _scopeBinder, _enclosingBinder, node.Type, designation.Identifier, node.IsOutVarDeclaration() ? LocalDeclarationKind.OutVariable : LocalDeclarationKind.DeclarationExpressionVariable, nodeToBind, argumentListSyntaxOpt);
	}

	protected override LocalSymbol MakeDeconstructionVariable(TypeSyntax closestTypeSyntax, SingleVariableDesignationSyntax designation, AssignmentExpressionSyntax deconstruction)
	{
		NamedTypeSymbol containingType = _scopeBinder.ContainingType;
		if ((object)containingType != null && containingType.IsScriptClass && (object)_scopeBinder.LookupDeclaredField(designation) != null)
		{
			return null;
		}
		return SourceLocalSymbol.MakeDeconstructionLocal(_scopeBinder.ContainingMemberOrLambda, _scopeBinder, _enclosingBinder, closestTypeSyntax, designation.Identifier, LocalDeclarationKind.DeconstructionVariable, deconstruction);
	}

	public static ObjectPool<ExpressionVariableFinder> CreatePool()
	{
		return new ObjectPool<ExpressionVariableFinder>(() => new ExpressionVariableFinder(), 10);
	}
}
