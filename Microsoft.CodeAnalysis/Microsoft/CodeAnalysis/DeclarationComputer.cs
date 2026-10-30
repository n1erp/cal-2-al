using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal class DeclarationComputer
{
	internal static DeclarationInfo GetDeclarationInfo(SemanticModel model, SyntaxNode node, bool getSymbol, IEnumerable<SyntaxNode> executableCodeBlocks, CancellationToken cancellationToken)
	{
		ISymbol declaredSymbol = GetDeclaredSymbol(model, node, getSymbol, cancellationToken);
		ImmutableArray<SyntaxNode> executableCodeBlocks2 = executableCodeBlocks?.Where((SyntaxNode c) => c != null).AsImmutableOrEmpty() ?? ImmutableArray<SyntaxNode>.Empty;
		return new DeclarationInfo(node, executableCodeBlocks2, declaredSymbol);
	}

	internal static DeclarationInfo GetDeclarationInfo(SemanticModel model, SyntaxNode node, bool getSymbol, CancellationToken cancellationToken)
	{
		return GetDeclarationInfo(model, node, getSymbol, (IEnumerable<SyntaxNode>)null, cancellationToken);
	}

	internal static DeclarationInfo GetDeclarationInfo(SemanticModel model, SyntaxNode node, bool getSymbol, SyntaxNode executableCodeBlock, CancellationToken cancellationToken)
	{
		return GetDeclarationInfo(model, node, getSymbol, SpecializedCollections.SingletonEnumerable(executableCodeBlock), cancellationToken);
	}

	internal static DeclarationInfo GetDeclarationInfo(SemanticModel model, SyntaxNode node, bool getSymbol, CancellationToken cancellationToken, params SyntaxNode[] executableCodeBlocks)
	{
		return GetDeclarationInfo(model, node, getSymbol, executableCodeBlocks.AsEnumerable(), cancellationToken);
	}

	private static ISymbol GetDeclaredSymbol(SemanticModel model, SyntaxNode node, bool getSymbol, CancellationToken cancellationToken)
	{
		if (!getSymbol)
		{
			return null;
		}
		ISymbol symbol = model.GetDeclaredSymbol(node, cancellationToken);
		if (symbol is INamespaceSymbol { ConstituentNamespaces: { Length: >1 } } namespaceSymbol)
		{
			IAssemblySymbol assemblyToScope = model.Compilation.Assembly;
			INamespaceSymbol namespaceSymbol2 = namespaceSymbol.ConstituentNamespaces.FirstOrDefault((INamespaceSymbol ns) => ns.ContainingAssembly == assemblyToScope);
			if (namespaceSymbol2 != null)
			{
				symbol = namespaceSymbol2;
			}
		}
		return symbol;
	}
}
