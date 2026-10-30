using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal static class MemberSymbolExtensions
{
	public static bool IsParams(this MethodSymbol method)
	{
		if (method.ParameterCount != 0)
		{
			return method.Parameters[method.ParameterCount - 1].IsParams;
		}
		return false;
	}

	internal static ImmutableArray<bool> GetParameterIsVars(this Symbol member)
	{
		SymbolKind kind = member.Kind;
		if (kind == SymbolKind.Method)
		{
			return ((MethodSymbol)member).ParameterIsVars;
		}
		throw ExceptionUtilities.UnexpectedValue(member.Kind);
	}

	internal static ImmutableArray<TypeSymbol> GetParameterTypes(this Symbol member)
	{
		SymbolKind kind = member.Kind;
		if (kind == SymbolKind.Method)
		{
			return ((MethodSymbol)member).ParameterTypes;
		}
		throw ExceptionUtilities.UnexpectedValue(member.Kind);
	}

	internal static int GetParameterCount(this Symbol member)
	{
		SymbolKind kind = member.Kind;
		if (kind == SymbolKind.Method)
		{
			return ((MethodSymbol)member).ParameterCount;
		}
		throw ExceptionUtilities.UnexpectedValue(member.Kind);
	}

	internal static ImmutableArray<ParameterSymbol> GetParameters(this Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).Parameters, 
			SymbolKind.Event => ((EventSymbol)member).Parameters, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	internal static bool IsParams(this Symbol member)
	{
		SymbolKind kind = member.Kind;
		if (kind == SymbolKind.Method)
		{
			MethodSymbol methodSymbol = member as MethodSymbol;
			DebugAssertHelper.Assert(methodSymbol != null);
			if (methodSymbol.ParameterCount > 0)
			{
				return methodSymbol.Parameters[methodSymbol.ParameterCount - 1].IsParams;
			}
			return false;
		}
		throw ExceptionUtilities.UnexpectedValue(member.Kind);
	}

	internal static TypeSymbol GetTypeOrReturnType(this Symbol member)
	{
		SymbolKind kind = member.Kind;
		if (kind == SymbolKind.Method)
		{
			return ((MethodSymbol)member).ReturnType;
		}
		throw ExceptionUtilities.UnexpectedValue(member.Kind);
	}

	internal static SourceReportDataItemSymbol GetDataItemLinkReferencedDataItem(this SourceReportDataItemSymbol symbol)
	{
		BoundMemberReferencePropertyValue boundPropertyValue = symbol.GetBoundPropertyValue<BoundMemberReferencePropertyValue>(PropertyKind.DataItemLinkReference);
		if (boundPropertyValue != null && boundPropertyValue.MemberReference?.Kind == SymbolKind.ReportDataItem)
		{
			return (SourceReportDataItemSymbol)boundPropertyValue.MemberReference;
		}
		if (symbol.ContainingSymbol.Kind == SymbolKind.ReportDataItem)
		{
			return (SourceReportDataItemSymbol)symbol.ContainingSymbol;
		}
		return null;
	}
}
