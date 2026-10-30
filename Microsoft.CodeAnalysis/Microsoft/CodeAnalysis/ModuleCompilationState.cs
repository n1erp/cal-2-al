using System.Collections.Generic;
using System.Threading;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis;

internal class ModuleCompilationState<TNamedTypeSymbol, TMethodSymbol> : CommonModuleCompilationState where TNamedTypeSymbol : class, INamespaceTypeDefinition where TMethodSymbol : class, IMethodDefinition
{
	private Dictionary<TMethodSymbol, TNamedTypeSymbol> _lazyStateMachineTypes;

	internal void SetStateMachineType(TMethodSymbol method, TNamedTypeSymbol stateMachineClass)
	{
		if (_lazyStateMachineTypes == null)
		{
			Interlocked.CompareExchange(ref _lazyStateMachineTypes, new Dictionary<TMethodSymbol, TNamedTypeSymbol>(), null);
		}
		lock (_lazyStateMachineTypes)
		{
			_lazyStateMachineTypes.Add(method, stateMachineClass);
		}
	}

	internal bool TryGetStateMachineType(TMethodSymbol method, out TNamedTypeSymbol stateMachineType)
	{
		stateMachineType = null;
		if (_lazyStateMachineTypes != null)
		{
			return _lazyStateMachineTypes.TryGetValue(method, out stateMachineType);
		}
		return false;
	}
}
