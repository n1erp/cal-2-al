using System.Reflection.Metadata;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ObsoleteAttributeHelpers
{
	internal static void InitializeObsoleteDataFromMetadata(ref ObsoleteAttributeData data, EntityHandle token, PEModuleSymbol containingModule, bool ignoreByRefLikeMarker)
	{
		if (data == ObsoleteAttributeData.Uninitialized)
		{
			ObsoleteAttributeData obsoleteDataFromMetadata = GetObsoleteDataFromMetadata(token, containingModule, ignoreByRefLikeMarker);
			Interlocked.CompareExchange(ref data, obsoleteDataFromMetadata, ObsoleteAttributeData.Uninitialized);
		}
	}

	internal static ObsoleteAttributeData GetObsoleteDataFromMetadata(EntityHandle token, PEModuleSymbol containingModule, bool ignoreByRefLikeMarker)
	{
		return containingModule.Module.TryGetDeprecatedOrExperimentalOrObsoleteAttribute(token, ignoreByRefLikeMarker);
	}

	private static ThreeState GetObsoleteContextState(Symbol symbol, bool forceComplete)
	{
		while ((object)symbol != null)
		{
			if (symbol.IsAccessor())
			{
				symbol = ((MethodSymbol)symbol).AssociatedSymbol;
			}
			else if (symbol.Kind == SymbolKind.Field)
			{
				Symbol associatedSymbol = ((FieldSymbol)symbol).AssociatedSymbol;
				if ((object)associatedSymbol != null)
				{
					symbol = associatedSymbol;
				}
			}
			if (forceComplete)
			{
				symbol.ForceCompleteObsoleteAttribute();
			}
			ThreeState obsoleteState = symbol.ObsoleteState;
			if (obsoleteState != ThreeState.False)
			{
				return obsoleteState;
			}
			symbol = symbol.ContainingSymbol;
		}
		return ThreeState.False;
	}

	internal static ObsoleteDiagnosticKind GetObsoleteDiagnosticKind(Symbol symbol, Symbol containingMember, bool forceComplete = false)
	{
		return symbol.ObsoleteKind switch
		{
			ObsoleteAttributeKind.None => ObsoleteDiagnosticKind.NotObsolete, 
			ObsoleteAttributeKind.Experimental => ObsoleteDiagnosticKind.Diagnostic, 
			ObsoleteAttributeKind.Uninitialized => ObsoleteDiagnosticKind.Lazy, 
			_ => GetObsoleteContextState(containingMember, forceComplete) switch
			{
				ThreeState.False => ObsoleteDiagnosticKind.Diagnostic, 
				ThreeState.True => ObsoleteDiagnosticKind.Suppressed, 
				_ => ObsoleteDiagnosticKind.LazyPotentiallySuppressed, 
			}, 
		};
	}

	internal static DiagnosticInfo CreateObsoleteDiagnostic(Symbol symbol, BinderFlags location)
	{
		ObsoleteAttributeData obsoleteAttributeData = symbol.ObsoleteAttributeData;
		if (obsoleteAttributeData == null)
		{
			return null;
		}
		if (location.Includes(BinderFlags.SuppressObsoleteChecks))
		{
			return null;
		}
		if (obsoleteAttributeData.Kind == ObsoleteAttributeKind.Experimental)
		{
			return new CSDiagnosticInfo(ErrorCode.WRN_Experimental, new FormattedSymbol(symbol, SymbolDisplayFormat.CSharpErrorMessageFormat));
		}
		bool flag = location.Includes(BinderFlags.CollectionInitializerAddMethod);
		if (obsoleteAttributeData.Message == null)
		{
			return new CSDiagnosticInfo(flag ? ErrorCode.WRN_DeprecatedCollectionInitAdd : ErrorCode.WRN_DeprecatedSymbol, symbol);
		}
		ErrorCode code;
		if (obsoleteAttributeData.IsError)
		{
			code = (flag ? ErrorCode.ERR_DeprecatedCollectionInitAddStr : ErrorCode.ERR_DeprecatedSymbolStr);
		}
		else
		{
			code = (flag ? ErrorCode.WRN_DeprecatedCollectionInitAddStr : ErrorCode.WRN_DeprecatedSymbolStr);
		}
		return new CSDiagnosticInfo(code, symbol, obsoleteAttributeData.Message);
	}
}
