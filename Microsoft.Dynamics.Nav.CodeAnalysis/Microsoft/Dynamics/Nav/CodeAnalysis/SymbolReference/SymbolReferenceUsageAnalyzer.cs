using System.Collections.Immutable;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public static class SymbolReferenceUsageAnalyzer
{
	public static ImmutableArray<IModuleSymbol> GetReferenceModuleSymbolsUsed(Compilation compilation, CancellationToken cancellationToken = default(CancellationToken))
	{
		PooledHashSet<IModuleSymbol> builder = PooledHashSet<IModuleSymbol>.GetInstance();
		try
		{
			foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
			{
				SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
				syntaxTree.GetRoot().WalkDescendantsAndPerformAction((SyntaxNode x) =>
				{
					AddReferenceModuleSymbolsFromSyntaxNode(compilation, semanticModel, x, builder, cancellationToken);
				});
			}
			return builder.ToImmutableArrayOrEmpty();
		}
		finally
		{
			builder.Free();
		}
	}

	internal static ImmutableArray<ISymbol> GetReferenceSymbolsUsed(Compilation compilation, CancellationToken cancellationToken = default(CancellationToken))
	{
		PooledHashSet<ISymbol> builder = PooledHashSet<ISymbol>.GetInstance();
		try
		{
			foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
			{
				SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
				syntaxTree.GetRoot().WalkDescendantsAndPerformAction((SyntaxNode x) =>
				{
					AddSymbolsFromSyntaxNode(compilation, semanticModel, x, builder, cancellationToken);
				});
			}
			return builder.ToImmutableArrayOrEmpty();
		}
		finally
		{
			builder.Free();
		}
	}

	private static void AddSymbolsFromSyntaxNode(Compilation compilation, SemanticModel semanticModel, SyntaxNode syntaxNode, PooledHashSet<ISymbol> builder, CancellationToken cancellationToken)
	{
		if (syntaxNode.Kind == SyntaxKind.IdentifierName || syntaxNode.Kind == SyntaxKind.ObjectId)
		{
			SymbolInfo symbolInfoWorker = semanticModel.GetSymbolInfoWorker(syntaxNode, cancellationToken);
			if (symbolInfoWorker.Symbol != null && ((Symbol)symbolInfoWorker.Symbol).ContainingModule != compilation.CompiledModule)
			{
				builder.Add(symbolInfoWorker.Symbol);
			}
		}
	}

	private static void AddReferenceModuleSymbolsFromSyntaxNode(Compilation compilation, SemanticModel semanticModel, SyntaxNode syntaxNode, PooledHashSet<IModuleSymbol> builder, CancellationToken cancellationToken)
	{
		if (syntaxNode.Kind != SyntaxKind.IdentifierName && syntaxNode.Kind != SyntaxKind.ObjectId)
		{
			return;
		}
		SymbolInfo symbolInfoWorker = semanticModel.GetSymbolInfoWorker(syntaxNode, cancellationToken);
		if (symbolInfoWorker.Symbol != null)
		{
			ModuleSymbol containingModule = ((Symbol)symbolInfoWorker.Symbol).ContainingModule;
			if (containingModule != null && containingModule != compilation.CompiledModule)
			{
				builder.Add(containingModule);
			}
		}
	}
}
