using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Shared.Extensions;

internal struct TokenSemanticInfo(ISymbol declaredSymbol, ImmutableArray<ISymbol> referencedSymbols, ITypeSymbol type)
{
	public static readonly TokenSemanticInfo Empty = new TokenSemanticInfo(null, ImmutableArray<ISymbol>.Empty, null);

	public readonly ISymbol DeclaredSymbol = declaredSymbol;

	public readonly ImmutableArray<ISymbol> ReferencedSymbols = referencedSymbols;

	public readonly ITypeSymbol Type = type;

	public ImmutableArray<ISymbol> GetSymbols(bool includeType)
	{
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		instance.AddIfNotNull(DeclaredSymbol);
		instance.AddRange(ReferencedSymbols);
		if (includeType)
		{
			instance.AddIfNotNull(Type);
		}
		return instance.ToImmutableAndFree();
	}

	public ISymbol GetAnySymbol(bool includeType)
	{
		return GetSymbols(includeType).FirstOrDefault();
	}
}
