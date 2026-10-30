using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal static class ModuleSymbolExtensions
{
	internal static DotNetTypeSymbol GetDeclaredDotNetTypeSymbol(this ModuleSymbol module, DotNetTypeInfo typeInfo)
	{
		DebugAssertHelper.Assert(typeInfo != null);
		ImmutableArray<ISymbol> objectSymbols = module.GetObjectSymbols(SymbolKind.DotNetTypeDeclaration);
		for (int i = 0; i < objectSymbols.Length; i++)
		{
			DotNetTypeDeclarationSymbol dotNetTypeDeclarationSymbol = (DotNetTypeDeclarationSymbol)objectSymbols[i];
			if (typeInfo.Equals(dotNetTypeDeclarationSymbol.DotNetType?.DotNetTypeInfo))
			{
				return dotNetTypeDeclarationSymbol.DotNetType;
			}
		}
		return null;
	}

	internal static Symbol GetSymbol(this ModuleSymbol module, ObjectChangeElement element)
	{
		int? id = element.Id;
		SymbolKind kind = element.Kind;
		ISymbol symbol;
		if (!id.HasValue)
		{
			symbol = module.GetObjectSymbolByName(kind, element.Name);
		}
		else
		{
			ISymbol objectSymbolById = module.GetObjectSymbolById(kind, id.Value);
			symbol = objectSymbolById;
		}
		return symbol as Symbol;
	}
}
