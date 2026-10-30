using System;
using System.Collections.Generic;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Emit;

internal sealed class SymbolChanges
{
	private readonly DefinitionMap _definitionMap;

	private readonly IReadOnlyDictionary<ISymbol, SymbolChange> _changes;

	private readonly Func<ISymbol, bool> _isAddedSymbol;

	public SymbolChanges(DefinitionMap definitionMap, IEnumerable<SemanticEdit> edits, Func<ISymbol, bool> isAddedSymbol)
	{
		_definitionMap = definitionMap;
		_isAddedSymbol = isAddedSymbol;
		_changes = CalculateChanges(edits);
	}

	public bool IsAdded(ISymbol symbol)
	{
		return _isAddedSymbol(symbol);
	}

	public bool RequiresCompilation(ISymbol symbol)
	{
		return GetChange(symbol) != SymbolChange.None;
	}

	public SymbolChange GetChange(IDefinition def)
	{
		if (def is ISynthesizedMethodBodyImplementationSymbol { Method: var method } synthesizedMethodBodyImplementationSymbol)
		{
			ISymbol symbol = (ISymbol)synthesizedMethodBodyImplementationSymbol;
			SymbolChange change = GetChange((IDefinition)method);
			switch (change)
			{
			case SymbolChange.Updated:
				if (!_definitionMap.DefinitionExists((IDefinition)symbol.ContainingType))
				{
					return SymbolChange.Added;
				}
				if (!_definitionMap.DefinitionExists(def))
				{
					return SymbolChange.Added;
				}
				if (!synthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency)
				{
					return SymbolChange.None;
				}
				if (symbol.Kind == SymbolKind.NamedType)
				{
					return SymbolChange.ContainsChanges;
				}
				if (symbol.Kind == SymbolKind.Method)
				{
					return SymbolChange.Updated;
				}
				return SymbolChange.None;
			case SymbolChange.Added:
				if (!_definitionMap.DefinitionExists(def))
				{
					return SymbolChange.Added;
				}
				if (symbol.Kind == SymbolKind.NamedType)
				{
					return SymbolChange.ContainsChanges;
				}
				if (symbol.Kind == SymbolKind.Method)
				{
					return SymbolChange.Updated;
				}
				return SymbolChange.None;
			default:
				throw ExceptionUtilities.UnexpectedValue(change);
			}
		}
		if (def is ISymbol symbol2)
		{
			return GetChange(symbol2);
		}
		if (_definitionMap.DefinitionExists(def))
		{
			if (!(def is ITypeDefinition))
			{
				return SymbolChange.None;
			}
			return SymbolChange.ContainsChanges;
		}
		return SymbolChange.Added;
	}

	private SymbolChange GetChange(ISymbol symbol)
	{
		if (_changes.TryGetValue(symbol, out var value))
		{
			return value;
		}
		ISymbol containingSymbol = GetContainingSymbol(symbol);
		if (containingSymbol == null)
		{
			return SymbolChange.None;
		}
		value = GetChange(containingSymbol);
		switch (value)
		{
		case SymbolChange.Added:
			return SymbolChange.Added;
		case SymbolChange.None:
			return SymbolChange.None;
		case SymbolChange.ContainsChanges:
		case SymbolChange.Updated:
			if (symbol is IDefinition definition && !_definitionMap.DefinitionExists(definition))
			{
				return SymbolChange.Added;
			}
			return SymbolChange.None;
		default:
			throw ExceptionUtilities.UnexpectedValue(value);
		}
	}

	public IEnumerable<INamespaceTypeDefinition> GetTopLevelTypes(EmitContext context)
	{
		CommonPEModuleBuilder module = context.Module;
		foreach (INamespaceTypeDefinition anonymousType in module.GetAnonymousTypes(context))
		{
			yield return anonymousType;
		}
		foreach (ISymbol key in _changes.Keys)
		{
			INamespaceTypeDefinition namespaceTypeDefinition = (key as ITypeDefinition)?.AsNamespaceTypeDefinition(context);
			if (namespaceTypeDefinition != null)
			{
				yield return namespaceTypeDefinition;
			}
		}
	}

	private static IReadOnlyDictionary<ISymbol, SymbolChange> CalculateChanges(IEnumerable<SemanticEdit> edits)
	{
		Dictionary<ISymbol, SymbolChange> dictionary = new Dictionary<ISymbol, SymbolChange>();
		foreach (SemanticEdit edit in edits)
		{
			SymbolChange value;
			switch (edit.Kind)
			{
			case SemanticEditKind.Update:
				value = SymbolChange.Updated;
				break;
			case SemanticEditKind.Insert:
				value = SymbolChange.Added;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(edit.Kind);
			case SemanticEditKind.Delete:
				continue;
			}
			ISymbol symbol = edit.NewSymbol;
			if (symbol.Kind == SymbolKind.Method)
			{
				IMethodSymbol partialDefinitionPart = ((IMethodSymbol)symbol).PartialDefinitionPart;
				if (partialDefinitionPart != null)
				{
					symbol = partialDefinitionPart;
				}
			}
			AddContainingTypes(dictionary, symbol);
			dictionary.Add(symbol, value);
		}
		return dictionary;
	}

	private static void AddContainingTypes(Dictionary<ISymbol, SymbolChange> changes, ISymbol symbol)
	{
		while (true)
		{
			symbol = GetContainingSymbol(symbol);
			if (symbol == null || changes.ContainsKey(symbol))
			{
				break;
			}
			SymbolKind kind = symbol.Kind;
			if (kind == SymbolKind.Property || kind == SymbolKind.Event)
			{
				changes.Add(symbol, SymbolChange.Updated);
			}
			else
			{
				changes.Add(symbol, SymbolChange.ContainsChanges);
			}
		}
	}

	private static ISymbol GetContainingSymbol(ISymbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Field:
		{
			ISymbol associatedSymbol2 = ((IFieldSymbol)symbol).AssociatedSymbol;
			if (associatedSymbol2 != null)
			{
				return associatedSymbol2;
			}
			break;
		}
		case SymbolKind.Method:
		{
			ISymbol associatedSymbol = ((IMethodSymbol)symbol).AssociatedSymbol;
			if (associatedSymbol != null)
			{
				return associatedSymbol;
			}
			break;
		}
		}
		symbol = symbol.ContainingSymbol;
		if (symbol != null)
		{
			SymbolKind kind = symbol.Kind;
			if (kind == SymbolKind.Assembly || kind == SymbolKind.NetModule)
			{
				return null;
			}
		}
		return symbol;
	}
}
