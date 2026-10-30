using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class NavTypeExtensions
{
	private static readonly string OptionTypeName = NavTypeKind.Option.ToString().ToUpperInvariant();

	private static readonly string TextConstTypeName = NavTypeKind.TextConst.ToString().ToUpperInvariant();

	private static readonly Lazy<ImmutableDictionary<string, NavTypeKind>> TextToNavTypeKindMap = new Lazy<ImmutableDictionary<string, NavTypeKind>>(() => Enum.GetValues(typeof(NavTypeKind)).Cast<object>().ToImmutableDictionary((object value) => value.ToString().ToUpperInvariant(), (object value) => (NavTypeKind)value));

	internal static NavTypeKind FromLiteralSyntaxKind(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.Int32LiteralToken:
		case SyntaxKind.Int32SignedLiteralValue:
		case SyntaxKind.Int32PropertyValue:
			return NavTypeKind.Integer;
		case SyntaxKind.Int64LiteralToken:
		case SyntaxKind.Int64SignedLiteralValue:
		case SyntaxKind.Int64PropertyValue:
			return NavTypeKind.BigInteger;
		case SyntaxKind.DecimalLiteralToken:
		case SyntaxKind.DecimalSignedLiteralValue:
		case SyntaxKind.DecimalPropertyValue:
			return NavTypeKind.Decimal;
		case SyntaxKind.StringLiteralToken:
		case SyntaxKind.StringLiteralValue:
		case SyntaxKind.StringPropertyValue:
			return NavTypeKind.String;
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.BooleanLiteralValue:
		case SyntaxKind.BooleanPropertyValue:
			return NavTypeKind.Boolean;
		case SyntaxKind.DateLiteralToken:
		case SyntaxKind.DateLiteralValue:
		case SyntaxKind.DatePropertyValue:
			return NavTypeKind.Date;
		case SyntaxKind.DateTimeLiteralToken:
		case SyntaxKind.DateTimeLiteralValue:
		case SyntaxKind.DateTimePropertyValue:
			return NavTypeKind.DateTime;
		case SyntaxKind.TimeLiteralToken:
		case SyntaxKind.TimeLiteralValue:
		case SyntaxKind.TimePropertyValue:
			return NavTypeKind.Time;
		default:
			return NavTypeKind.None;
		}
	}

	internal static SymbolKind ExpectedFromApplicationObjectSyntaxKind(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.TableKeyword:
		case SyntaxKind.TableDataKeyword:
			return SymbolKind.Table;
		case SyntaxKind.PageKeyword:
			return SymbolKind.Page;
		case SyntaxKind.XmlPortKeyword:
			return SymbolKind.XmlPort;
		case SyntaxKind.ReportKeyword:
			return SymbolKind.Report;
		case SyntaxKind.QueryKeyword:
			return SymbolKind.Query;
		case SyntaxKind.CodeunitKeyword:
			return SymbolKind.Codeunit;
		case SyntaxKind.ControlAddInKeyword:
			return SymbolKind.ControlAddIn;
		default:
			return SymbolKind.Undefined;
		}
	}

	internal static NavTypeKind GetNavTypeKind(string value, bool throwIfUnknown = true)
	{
		if (!TextToNavTypeKindMap.Value.TryGetValue(value.ToUpperInvariant(), out var value2) & throwIfUnknown)
		{
			throw ExceptionUtilities.UnexpectedValue(value);
		}
		return value2;
	}

	internal static bool IsOptionType(string value)
	{
		return SemanticFacts.NameEqualityComparer.Equals(OptionTypeName, value);
	}

	internal static bool IsTextConstType(string value)
	{
		return SemanticFacts.NameEqualityComparer.Equals(TextConstTypeName, value);
	}

	internal static bool IsValidSumIndexFieldValue(this NavTypeKind kind)
	{
		if ((uint)(kind - 12451845) <= 2u || kind == NavTypeKind.Duration)
		{
			return true;
		}
		return false;
	}

	internal static bool IsComplexValue(this NavTypeKind kind)
	{
		switch (kind)
		{
		case NavTypeKind.List:
		case NavTypeKind.Dictionary:
		case NavTypeKind.WebServiceActionContext:
		case NavTypeKind.WebServiceActionResultCode:
		case NavTypeKind.NotificationScope:
		case NavTypeKind.Notification:
		case NavTypeKind.JsonToken:
		case NavTypeKind.JsonObject:
		case NavTypeKind.JsonArray:
		case NavTypeKind.JsonValue:
		case NavTypeKind.HttpHeaders:
		case NavTypeKind.XmlAttribute:
		case NavTypeKind.XmlAttributeCollection:
		case NavTypeKind.XmlComment:
		case NavTypeKind.XmlCData:
		case NavTypeKind.XmlDeclaration:
		case NavTypeKind.XmlDocument:
		case NavTypeKind.XmlDocumentType:
		case NavTypeKind.XmlElement:
		case NavTypeKind.XmlNamespaceManager:
		case NavTypeKind.XmlNameTable:
		case NavTypeKind.XmlNode:
		case NavTypeKind.XmlNodeList:
		case NavTypeKind.XmlProcessingInstruction:
		case NavTypeKind.XmlText:
		case NavTypeKind.XmlReadOptions:
		case NavTypeKind.XmlWriteOptions:
		case NavTypeKind.Version:
		case NavTypeKind.ModuleInfo:
		case NavTypeKind.ModuleDependencyInfo:
		case NavTypeKind.ErrorInfo:
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
		case NavTypeKind.Verbosity:
		case NavTypeKind.DataClassification:
		case NavTypeKind.TestPermissions:
		case NavTypeKind.TransactionModel:
		case NavTypeKind.ExecutionContext:
		case NavTypeKind.ErrorType:
		case NavTypeKind.TextConst:
		case NavTypeKind.Label:
		case NavTypeKind.BigText:
		case NavTypeKind.Byte:
		case NavTypeKind.Char:
		case NavTypeKind.Media:
		case NavTypeKind.MediaSet:
		case NavTypeKind.DateFormula:
		case NavTypeKind.RecordId:
		case NavTypeKind.Boolean:
		case NavTypeKind.Integer:
		case NavTypeKind.BigInteger:
		case NavTypeKind.Decimal:
		case NavTypeKind.Option:
		case NavTypeKind.DateTime:
		case NavTypeKind.Time:
		case NavTypeKind.Date:
		case NavTypeKind.Duration:
		case NavTypeKind.Guid:
		case NavTypeKind.Enum:
		case NavTypeKind.Text:
		case NavTypeKind.Code:
			return false;
		case NavTypeKind.None:
		case NavTypeKind.Array:
		case NavTypeKind.Joker:
		case NavTypeKind.Table:
		case NavTypeKind.AnyCodeunit:
		case NavTypeKind.TestAction:
		case NavTypeKind.TestField:
		case NavTypeKind.TestFilterField:
		case NavTypeKind.TestFilter:
		case NavTypeKind.PageExtension:
		case NavTypeKind.TableExtension:
		case NavTypeKind.Profile:
		case NavTypeKind.QueryFilter:
		case NavTypeKind.MethodReference:
		case NavTypeKind.RequestPage:
		case NavTypeKind.CurrPage:
		case NavTypeKind.CurrReport:
		case NavTypeKind.CurrXmlPort:
		case NavTypeKind.CurrQuery:
		case NavTypeKind._ClrOption:
		case NavTypeKind._Variable:
		case NavTypeKind.Dialog:
		case NavTypeKind._Parameter:
		case NavTypeKind.TestRequestPage:
		case NavTypeKind.RecordRef:
		case NavTypeKind.FieldRef:
		case NavTypeKind.KeyRef:
		case NavTypeKind.InStream:
		case NavTypeKind.OutStream:
		case NavTypeKind.Variant:
		case NavTypeKind.DotNet:
		case NavTypeKind.TestPage:
		case NavTypeKind.File:
		case NavTypeKind.FilterPageBuilder:
		case NavTypeKind.HttpClient:
		case NavTypeKind.HttpRequestMessage:
		case NavTypeKind.HttpResponseMessage:
		case NavTypeKind.HttpContent:
		case NavTypeKind.Record:
		case NavTypeKind.Codeunit:
		case NavTypeKind.Page:
		case NavTypeKind.Report:
		case NavTypeKind.XmlPort:
		case NavTypeKind.Query:
		case NavTypeKind._ReturnValue:
		case NavTypeKind.FieldType:
		case NavTypeKind.FieldClass:
		case NavTypeKind._Field:
		case NavTypeKind._Control:
		case NavTypeKind.String:
		case NavTypeKind.Blob:
		case NavTypeKind._Text:
		case NavTypeKind._Key:
		case NavTypeKind.TableFilter:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	internal static bool IsNonComplexValue(this NavTypeKind kind)
	{
		return !kind.IsComplexValue();
	}

	internal static bool IsValidStyleExpressionValue(this NavTypeKind kind)
	{
		if (kind == NavTypeKind.String || kind == NavTypeKind.Boolean || (uint)(kind - 16646153) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool HasLength(this NavTypeKind type)
	{
		if ((uint)(type - 16646153) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool LengthRequired(this NavTypeKind type)
	{
		if (type == NavTypeKind.Code)
		{
			return true;
		}
		return false;
	}

	internal static bool HasSubType(this NavTypeKind type)
	{
		switch (type)
		{
		case NavTypeKind.TestRequestPage:
		case NavTypeKind.DotNet:
		case NavTypeKind.TestPage:
		case NavTypeKind.Record:
		case NavTypeKind.Codeunit:
		case NavTypeKind.Page:
		case NavTypeKind.Report:
		case NavTypeKind.XmlPort:
		case NavTypeKind.Query:
		case NavTypeKind.ControlAddIn:
			return true;
		default:
			return false;
		}
	}

	internal static SymbolKind ToSymbolKind(this NavTypeKind kind)
	{
		switch (kind)
		{
		case NavTypeKind.Codeunit:
			return SymbolKind.Codeunit;
		case NavTypeKind.Table:
		case NavTypeKind.Record:
			return SymbolKind.Table;
		case NavTypeKind.Page:
			return SymbolKind.Page;
		case NavTypeKind.TestPage:
			return SymbolKind.TestPage;
		case NavTypeKind.TestRequestPage:
			return SymbolKind.TestRequestPage;
		case NavTypeKind.Report:
			return SymbolKind.Report;
		case NavTypeKind.Query:
			return SymbolKind.Query;
		case NavTypeKind.XmlPort:
			return SymbolKind.XmlPort;
		case NavTypeKind.ControlAddIn:
			return SymbolKind.ControlAddIn;
		case NavTypeKind.DotNet:
			return SymbolKind.DotNet;
		case NavTypeKind.Enum:
			return SymbolKind.Enum;
		default:
			return SymbolKind.Undefined;
		}
	}

	internal static SymbolKind ToSubTypeSymbolKind(this NavTypeKind kind)
	{
		SymbolKind symbolKind = kind.ToSymbolKind();
		return symbolKind switch
		{
			SymbolKind.TestPage => SymbolKind.Page, 
			SymbolKind.TestRequestPage => SymbolKind.Report, 
			SymbolKind.DotNet => SymbolKind.DotNetTypeDeclaration, 
			_ => symbolKind, 
		};
	}

	internal static bool IsOptionOrEnum(this NavTypeKind kind)
	{
		if (kind != NavTypeKind.Option && kind != NavTypeKind.Enum)
		{
			return kind == NavTypeKind.EnumExtension;
		}
		return true;
	}

	internal static bool IsClrOption(this NavTypeKind kind)
	{
		return (kind & NavTypeKind._ClrOption) != 0;
	}

	internal static bool IsClrType(this NavTypeKind kind)
	{
		switch (kind)
		{
		case NavTypeKind.String:
		case NavTypeKind.Byte:
		case NavTypeKind.Char:
		case NavTypeKind.Boolean:
		case NavTypeKind.Integer:
		case NavTypeKind.BigInteger:
		case NavTypeKind.Decimal:
		case NavTypeKind.Guid:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsGenericType(this NavTypeKind kind)
	{
		if ((uint)(kind - 393422) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsValidControlField(this NavTypeKind kind)
	{
		return (kind & NavTypeKind._Control) != 0;
	}

	internal static bool IsValidColumnKind(this NavTypeKind kind)
	{
		switch (kind)
		{
		case NavTypeKind.Variant:
		case NavTypeKind.String:
		case NavTypeKind.TextConst:
		case NavTypeKind.Label:
		case NavTypeKind.Char:
		case NavTypeKind.Blob:
		case NavTypeKind.TableFilter:
		case NavTypeKind.Media:
		case NavTypeKind.MediaSet:
		case NavTypeKind.DateFormula:
		case NavTypeKind.RecordId:
		case NavTypeKind.Boolean:
		case NavTypeKind.Integer:
		case NavTypeKind.BigInteger:
		case NavTypeKind.Decimal:
		case NavTypeKind.Option:
		case NavTypeKind.DateTime:
		case NavTypeKind.Time:
		case NavTypeKind.Date:
		case NavTypeKind.Duration:
		case NavTypeKind.Guid:
		case NavTypeKind.Enum:
		case NavTypeKind.Text:
		case NavTypeKind.Code:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsValidColumnTypeForAggregationMethod(this NavTypeKind kind)
	{
		if ((uint)(kind - 12451845) <= 2u || kind == NavTypeKind.Duration)
		{
			return true;
		}
		return false;
	}

	internal static bool IsValidColumnTypeForDateMethod(this NavTypeKind kind)
	{
		if (kind == NavTypeKind.DateTime || kind == NavTypeKind.Date)
		{
			return true;
		}
		return false;
	}

	internal static ImmutableArray<NavTypeKind> GetTypesOfKind(NavTypeKind kind)
	{
		return (from NavTypeKind e in typeof(NavTypeKind).GetTypeInfo().GetEnumValues()
			where (e & kind) != 0 && e != kind
			select e).ToImmutableArray();
	}
}
