using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class EnumConversions
{
	internal static SymbolKind ToSymbolKind(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.CodeunitKeyword:
		case SyntaxKind.CodeunitObject:
			return SymbolKind.Codeunit;
		case SyntaxKind.TableKeyword:
		case SyntaxKind.TableObject:
			return SymbolKind.Table;
		case SyntaxKind.TableExtensionKeyword:
		case SyntaxKind.TableExtensionObject:
			return SymbolKind.TableExtension;
		case SyntaxKind.PageKeyword:
		case SyntaxKind.PageObject:
			return SymbolKind.Page;
		case SyntaxKind.PageExtensionKeyword:
		case SyntaxKind.PageExtensionObject:
			return SymbolKind.PageExtension;
		case SyntaxKind.PageCustomizationKeyword:
		case SyntaxKind.PageCustomizationObject:
			return SymbolKind.PageCustomization;
		case SyntaxKind.ReportKeyword:
		case SyntaxKind.ReportObject:
			return SymbolKind.Report;
		case SyntaxKind.XmlPortKeyword:
		case SyntaxKind.XmlPortObject:
			return SymbolKind.XmlPort;
		case SyntaxKind.QueryKeyword:
		case SyntaxKind.QueryObject:
			return SymbolKind.Query;
		case SyntaxKind.ProfileKeyword:
		case SyntaxKind.ProfileObject:
			return SymbolKind.Profile;
		case SyntaxKind.DotNetKeyword:
		case SyntaxKind.DotNetPackage:
			return SymbolKind.DotNetPackage;
		case SyntaxKind.ControlAddInKeyword:
			return SymbolKind.ControlAddIn;
		case SyntaxKind.EnumKeyword:
		case SyntaxKind.EnumType:
			return SymbolKind.Enum;
		case SyntaxKind.EnumExtensionKeyword:
		case SyntaxKind.EnumExtensionType:
			return SymbolKind.EnumExtension;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind.ToString());
		}
	}
}
