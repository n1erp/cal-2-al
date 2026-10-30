using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal static class NavCorLibExtensions
{
	internal static bool IsEnumerableType_Internal(this ITypeSymbol type)
	{
		switch (type.NavTypeKind)
		{
		case NavTypeKind.List:
		case NavTypeKind.JsonArray:
		case NavTypeKind.XmlAttributeCollection:
		case NavTypeKind.XmlNodeList:
			return true;
		default:
			return false;
		}
	}

	internal static TypeSymbol GetEnumeratorElementType_Internal(this ITypeSymbol type)
	{
		return type.NavTypeKind switch
		{
			NavTypeKind.JsonArray => NavCorLib.JsonTokenType, 
			NavTypeKind.List => ((GenericNavTypeSymbol)type).TypeArguments[0], 
			NavTypeKind.XmlAttributeCollection => NavCorLib.XmlAttributeType, 
			NavTypeKind.XmlNodeList => NavCorLib.XmlNodeType, 
			_ => throw ExceptionUtilities.UnexpectedValue(type.NavTypeKind), 
		};
	}

	internal static bool IsSystemOption(this ITypeSymbol type)
	{
		if (type.Kind != SymbolKind.OptionType)
		{
			return false;
		}
		switch (type.NavTypeKind)
		{
		case NavTypeKind.WebServiceActionResultCode:
		case NavTypeKind.NotificationScope:
		case NavTypeKind.DataScope:
		case NavTypeKind.Action:
		case NavTypeKind.TableConnectionType:
		case NavTypeKind.SecurityFilter:
		case NavTypeKind.TransactionType:
		case NavTypeKind.DefaultLayout:
		case NavTypeKind.TextEncoding:
		case NavTypeKind.ExecutionMode:
		case NavTypeKind.ObjectType:
		case NavTypeKind.ClientType:
		case NavTypeKind.ReportFormat:
		case NavTypeKind.FieldType:
		case NavTypeKind.FieldClass:
		case NavTypeKind.Verbosity:
		case NavTypeKind.DataClassification:
		case NavTypeKind.TestPermissions:
		case NavTypeKind.TransactionModel:
		case NavTypeKind.ExecutionContext:
		case NavTypeKind.ErrorType:
			return true;
		default:
			return false;
		}
	}
}
