using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class SymbolExtensions
{
	internal static Glyph GetGlyph(this ISymbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.ArrayType:
			return ((ArrayTypeSymbol)symbol).ElementType.GetGlyph();
		case SymbolKind.Field:
		{
			ITypeSymbol containingType = symbol.ContainingType;
			if (containingType != null && containingType.Kind == SymbolKind.Option)
			{
				return Glyph.Option;
			}
			return Glyph.Field;
		}
		case SymbolKind.Option:
		case SymbolKind.OptionType:
			return Glyph.Option;
		case SymbolKind.GlobalVariable:
			return Glyph.GlobalVariable;
		case SymbolKind.LocalVariable:
			return Glyph.LocalVariable;
		case SymbolKind.Class:
			return Glyph.Builtin;
		case SymbolKind.Table:
			return Glyph.Table;
		case SymbolKind.Codeunit:
			return Glyph.Codeunit;
		case SymbolKind.Page:
			return Glyph.Page;
		case SymbolKind.Control:
			return Glyph.Control;
		case SymbolKind.Action:
			return Glyph.Action;
		case SymbolKind.Method:
			switch (((IMethodSymbol)symbol).MethodKind)
			{
			case MethodKind.Trigger:
				return Glyph.Trigger;
			case MethodKind.BuiltInMethod:
			case MethodKind.BuiltInOperator:
				return Glyph.Builtin;
			default:
				return Glyph.Method;
			}
		case SymbolKind.Parameter:
			return Glyph.Parameter;
		case SymbolKind.Property:
			return Glyph.Property;
		case SymbolKind.Key:
			return Glyph.Key;
		default:
			return Glyph.None;
		}
	}
}
