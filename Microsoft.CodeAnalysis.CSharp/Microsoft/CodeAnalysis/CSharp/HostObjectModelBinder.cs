using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class HostObjectModelBinder : Binder
{
	public HostObjectModelBinder(Binder next)
		: base(next)
	{
	}

	private TypeSymbol GetHostObjectType()
	{
		return Compilation.GetHostObjectTypeSymbol();
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol hostObjectType = GetHostObjectType();
		if (hostObjectType.Kind == SymbolKind.ErrorType)
		{
			result.SetFrom(new CSDiagnosticInfo(ErrorCode.ERR_NameNotInContextPossibleMissingReference, new object[2]
			{
				name,
				((MissingMetadataTypeSymbol)hostObjectType).ContainingAssembly.Identity
			}, ImmutableArray<Symbol>.Empty, ImmutableArray<Location>.Empty));
		}
		else
		{
			LookupMembersInternal(result, hostObjectType, name, arity, basesBeingResolved, options, originalBinder, diagnose, ref useSiteDiagnostics);
		}
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		TypeSymbol hostObjectType = GetHostObjectType();
		if (hostObjectType.Kind != SymbolKind.ErrorType)
		{
			AddMemberLookupSymbolsInfo(result, hostObjectType, options, originalBinder);
		}
	}
}
