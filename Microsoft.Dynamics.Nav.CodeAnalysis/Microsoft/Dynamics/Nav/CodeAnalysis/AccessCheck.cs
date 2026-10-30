using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class AccessCheck
{
	public static bool IsSymbolAccessible(Symbol symbol, ModuleSymbol within, LookupOptions options, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		bool failedThroughTypeCheck;
		return IsSymbolAccessibleCore(symbol, within, null, options, out failedThroughTypeCheck, within.DeclaringCompilation, ref useSiteDiagnostics);
	}

	public static bool IsSymbolAccessible(Symbol symbol, Symbol within, LookupOptions options, TypeSymbol throughTypeOpt, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return IsSymbolAccessibleCore(symbol, within, throughTypeOpt, options, out failedThroughTypeCheck, within.DeclaringCompilation, ref useSiteDiagnostics);
	}

	private static bool IsSymbolAccessibleCore(Symbol symbol, Symbol within, TypeSymbol throughTypeOpt, LookupOptions options, out bool failedThroughTypeCheck, Compilation compilation, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert((object)symbol != null);
		DebugAssertHelper.Assert((object)within != null);
		DebugAssertHelper.Assert(within.IsDefinition);
		DebugAssertHelper.Assert(within is TypeSymbol || within is ModuleSymbol);
		failedThroughTypeCheck = false;
		switch (symbol.Kind)
		{
		case SymbolKind.ArrayType:
			return IsSymbolAccessibleCore(((ArrayTypeSymbol)symbol).ElementType, within, null, options, out failedThroughTypeCheck, compilation, ref useSiteDiagnostics);
		case SymbolKind.ErrorType:
			return true;
		case SymbolKind.Enum:
		case SymbolKind.LocalVariable:
		case SymbolKind.Parameter:
		case SymbolKind.ReturnValue:
		case SymbolKind.Field:
		case SymbolKind.Option:
		case SymbolKind.OptionType:
		case SymbolKind.Action:
		case SymbolKind.EnumValue:
		case SymbolKind.View:
		case SymbolKind.QueryFilter:
		case SymbolKind.QueryColumn:
			return true;
		case SymbolKind.Class:
			return true;
		case SymbolKind.GlobalVariable:
		case SymbolKind.Method:
			return IsMemberAccessible(symbol, symbol.DeclaredAccessibility, within, throughTypeOpt, options, out failedThroughTypeCheck);
		case SymbolKind.Control:
			return IsControlAccessible((ControlSymbol)symbol, within, throughTypeOpt, options);
		case SymbolKind.FieldGroup:
			return options.HasFlag(LookupOptions.MustBeFieldGroup);
		case SymbolKind.Event:
		case SymbolKind.Property:
		case SymbolKind.Key:
		case SymbolKind.Change:
		case SymbolKind.ReportColumn:
		case SymbolKind.ReportLabel:
			return false;
		case SymbolKind.TestField:
		case SymbolKind.TestAction:
		case SymbolKind.TestPart:
		case SymbolKind.TestFilter:
		case SymbolKind.TestFilterField:
			return true;
		case SymbolKind.QueryDataItem:
		case SymbolKind.ReportDataItem:
		case SymbolKind.RequestPage:
		case SymbolKind.XmlPortNode:
			return IsNestedMemberAccessible(symbol, symbol.DeclaredAccessibility, within, throughTypeOpt, options, out failedThroughTypeCheck);
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	private static bool IsNestedMemberAccessible(Symbol member, Accessibility declaredAccessibility, Symbol within, TypeSymbol throughTypeOpt, LookupOptions options, out bool failedThroughTypeCheck)
	{
		if (IsMemberAccessible(member, declaredAccessibility, within, throughTypeOpt, options, out failedThroughTypeCheck))
		{
			return true;
		}
		ApplicationObjectTypeSymbol containingSymbolOfType = member.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
		if (!throughTypeOpt.IsCurrObjectType() && (object)containingSymbolOfType == within && !options.IsMemberAccess())
		{
			return true;
		}
		RequestPageTypeSymbol requestPageTypeSymbol = within as RequestPageTypeSymbol;
		if (requestPageTypeSymbol != null && (object)containingSymbolOfType == requestPageTypeSymbol.ContainingSymbol)
		{
			return true;
		}
		return false;
	}

	private static bool IsControlAccessible(ControlSymbol control, Symbol within, TypeSymbol throughTypeOpt, LookupOptions options)
	{
		if (options.HasFlag(LookupOptions.MustBePageElement))
		{
			return true;
		}
		if (IsControlAccessibleFromCode(control, throughTypeOpt))
		{
			ObjectTypeSymbol containingSymbolOfType = control.GetContainingSymbolOfType<ObjectTypeSymbol>();
			if (within.Kind == SymbolKind.PageExtension && containingSymbolOfType == ((PageExtensionTypeSymbol)within).Target)
			{
				return true;
			}
			return containingSymbolOfType == within;
		}
		return false;
	}

	private static bool IsControlAccessibleFromCode(ControlSymbol control, TypeSymbol throughTypeOpt)
	{
		if (!control.IsAccessibleFromCode())
		{
			return false;
		}
		return throughTypeOpt.Kind switch
		{
			SymbolKind.PageReference => ((PageReferenceTypeSymbol)throughTypeOpt).IsCurrPage, 
			SymbolKind.RequestPage => true, 
			_ => false, 
		};
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "throughTypeOpt")]
	private static bool IsMemberAccessible(Symbol member, Accessibility declaredAccessibility, Symbol within, TypeSymbol throughTypeOpt, LookupOptions options, out bool failedThroughTypeCheck)
	{
		TypeSymbol containingType = member.ContainingType;
		DebugAssertHelper.Assert((object)containingType != null);
		DebugAssertHelper.Assert(within is TypeSymbol || within is ModuleSymbol);
		bool flag = (options & LookupOptions.MemberAccess) == 0;
		failedThroughTypeCheck = false;
		if (throughTypeOpt != null && throughTypeOpt.IsCurrObjectType() && member.Kind == SymbolKind.Method)
		{
			return ((MethodSymbol)member).MethodKind == MethodKind.BuiltInMethod;
		}
		if (flag && (object)containingType == within)
		{
			return true;
		}
		if (declaredAccessibility == Accessibility.Public)
		{
			return true;
		}
		return IsNonPublicMemberAccessible(containingType, declaredAccessibility, within, flag, out failedThroughTypeCheck);
	}

	private static bool IsNonPublicMemberAccessible(TypeSymbol containingType, Accessibility declaredAccessibility, Symbol within, bool accessedDirectlyOnSelf, out bool failedThroughTypeCheck)
	{
		failedThroughTypeCheck = false;
		TypeSymbol originalDefinition = containingType.OriginalDefinition;
		TypeSymbol typeSymbol = within as TypeSymbol;
		switch (declaredAccessibility)
		{
		case Accessibility.NotApplicable:
			return true;
		case Accessibility.Private:
			return ((object)typeSymbol != null && IsPrivateSymbolAccessible(typeSymbol, originalDefinition)) & accessedDirectlyOnSelf;
		case Accessibility.Protected:
			if ((object)typeSymbol != null)
			{
				return IsPrivateSymbolAccessible(typeSymbol, originalDefinition);
			}
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(declaredAccessibility);
		}
	}

	private static bool IsPrivateSymbolAccessible(Symbol within, TypeSymbol originalContainingType)
	{
		TypeSymbol typeSymbol = within as TypeSymbol;
		DebugAssertHelper.Assert(typeSymbol != null || within is ModuleSymbol);
		if (typeSymbol == null)
		{
			return false;
		}
		return IsNestedWithinOriginalContainingType(typeSymbol, originalContainingType);
	}

	private static bool IsNestedWithinOriginalContainingType(TypeSymbol withinType, TypeSymbol originalContainingType)
	{
		DebugAssertHelper.Assert((object)withinType != null);
		DebugAssertHelper.Assert((object)originalContainingType != null);
		TypeSymbol typeSymbol = withinType.OriginalDefinition;
		while ((object)typeSymbol != null)
		{
			DebugAssertHelper.Assert(typeSymbol.IsDefinition);
			if (typeSymbol.Equals(originalContainingType))
			{
				return true;
			}
			typeSymbol = typeSymbol.ContainingType;
		}
		return false;
	}
}
