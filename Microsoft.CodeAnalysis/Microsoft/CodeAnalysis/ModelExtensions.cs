using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.CodeAnalysis;

public static class ModelExtensions
{
	public static SymbolInfo GetSymbolInfo(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return semanticModel.GetSymbolInfo(node, cancellationToken);
	}

	public static SymbolInfo GetSpeculativeSymbolInfo(this SemanticModel semanticModel, int position, SyntaxNode expression, SpeculativeBindingOption bindingOption)
	{
		return semanticModel.GetSpeculativeSymbolInfo(position, expression, bindingOption);
	}

	public static TypeInfo GetTypeInfo(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return semanticModel.GetTypeInfo(node, cancellationToken);
	}

	public static IAliasSymbol GetAliasInfo(this SemanticModel semanticModel, SyntaxNode nameSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return semanticModel.GetAliasInfo(nameSyntax, cancellationToken);
	}

	public static IAliasSymbol GetSpeculativeAliasInfo(this SemanticModel semanticModel, int position, SyntaxNode nameSyntax, SpeculativeBindingOption bindingOption)
	{
		return semanticModel.GetSpeculativeAliasInfo(position, nameSyntax, bindingOption);
	}

	public static TypeInfo GetSpeculativeTypeInfo(this SemanticModel semanticModel, int position, SyntaxNode expression, SpeculativeBindingOption bindingOption)
	{
		return semanticModel.GetSpeculativeTypeInfo(position, expression, bindingOption);
	}

	public static ISymbol GetDeclaredSymbol(this SemanticModel semanticModel, SyntaxNode declaration, CancellationToken cancellationToken = default(CancellationToken))
	{
		return semanticModel.GetDeclaredSymbolForNode(declaration, cancellationToken);
	}

	public static ImmutableArray<ISymbol> GetMemberGroup(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return semanticModel.GetMemberGroup(node, cancellationToken);
	}

	public static ControlFlowAnalysis AnalyzeControlFlow(this SemanticModel semanticModel, SyntaxNode firstStatement, SyntaxNode lastStatement)
	{
		return semanticModel.AnalyzeControlFlow(firstStatement, lastStatement);
	}

	public static ControlFlowAnalysis AnalyzeControlFlow(this SemanticModel semanticModel, SyntaxNode statement)
	{
		return semanticModel.AnalyzeControlFlow(statement);
	}

	public static DataFlowAnalysis AnalyzeDataFlow(this SemanticModel semanticModel, SyntaxNode firstStatement, SyntaxNode lastStatement)
	{
		return semanticModel.AnalyzeDataFlow(firstStatement, lastStatement);
	}

	public static DataFlowAnalysis AnalyzeDataFlow(this SemanticModel semanticModel, SyntaxNode statementOrExpression)
	{
		return semanticModel.AnalyzeDataFlow(statementOrExpression);
	}
}
