using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class ExpressionFieldFinder : ExpressionVariableFinder<Symbol>
{
	private SourceMemberContainerTypeSymbol _containingType;

	private DeclarationModifiers _modifiers;

	private FieldSymbol _containingFieldOpt;

	private static readonly ObjectPool<ExpressionFieldFinder> s_poolInstance = CreatePool();

	internal static void FindExpressionVariables(ArrayBuilder<Symbol> builder, CSharpSyntaxNode node, SourceMemberContainerTypeSymbol containingType, DeclarationModifiers modifiers, FieldSymbol containingFieldOpt)
	{
		if (node != null)
		{
			ExpressionFieldFinder expressionFieldFinder = s_poolInstance.Allocate();
			expressionFieldFinder._containingType = containingType;
			expressionFieldFinder._modifiers = modifiers;
			expressionFieldFinder._containingFieldOpt = containingFieldOpt;
			expressionFieldFinder.FindExpressionVariables(builder, node);
			expressionFieldFinder._containingType = null;
			expressionFieldFinder._modifiers = DeclarationModifiers.None;
			expressionFieldFinder._containingFieldOpt = null;
			s_poolInstance.Free(expressionFieldFinder);
		}
	}

	protected override Symbol MakePatternVariable(DeclarationPatternSyntax node, SyntaxNode nodeToBind)
	{
		if (!(node.Designation is SingleVariableDesignationSyntax singleVariableDesignationSyntax))
		{
			return null;
		}
		return GlobalExpressionVariable.Create(_containingType, _modifiers, node.Type, singleVariableDesignationSyntax.Identifier.ValueText, singleVariableDesignationSyntax, singleVariableDesignationSyntax.GetLocation(), _containingFieldOpt, nodeToBind);
	}

	protected override Symbol MakeDeclarationExpressionVariable(DeclarationExpressionSyntax node, SingleVariableDesignationSyntax designation, BaseArgumentListSyntax argumentListSyntaxOpt, SyntaxNode nodeToBind)
	{
		return GlobalExpressionVariable.Create(_containingType, _modifiers, node.Type, designation.Identifier.ValueText, designation, designation.Identifier.GetLocation(), _containingFieldOpt, nodeToBind);
	}

	protected override Symbol MakeDeconstructionVariable(TypeSyntax closestTypeSyntax, SingleVariableDesignationSyntax designation, AssignmentExpressionSyntax deconstruction)
	{
		return GlobalExpressionVariable.Create(_containingType, DeclarationModifiers.Private, closestTypeSyntax, designation.Identifier.ValueText, designation, designation.Location, null, deconstruction);
	}

	public static ObjectPool<ExpressionFieldFinder> CreatePool()
	{
		return new ObjectPool<ExpressionFieldFinder>(() => new ExpressionFieldFinder(), 10);
	}
}
