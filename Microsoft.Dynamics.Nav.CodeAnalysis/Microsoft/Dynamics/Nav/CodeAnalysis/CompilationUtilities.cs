using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.PreviewReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public static class CompilationUtilities
{
	public static void WriteSymbolReference(Stream outputStream, IModuleSymbol moduleSymbol)
	{
		ModuleDefinition module = SerializableSymbolModelConverter.ConvertModuleToSerializableSymbolModel(moduleSymbol);
		SymbolReferenceJsonWriter.WriteModule(outputStream, module);
	}

	public static string ResolvePageControlName(this Compilation compilation, string pageName, int controlId)
	{
		IPageTypeSymbol pageTypeSymbol = (IPageTypeSymbol)compilation.GetApplicationObjectTypeSymbolsByNameAcrossModules(SymbolKind.Page, pageName).FirstOrDefault();
		string text = pageTypeSymbol?.FlattenedControls.FirstOrDefault((IControlSymbol c) => c.Id == controlId)?.Name;
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		foreach (IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in compilation.GetApplicationObjectExtensionTypeSymbolsAcrossModules(pageTypeSymbol))
		{
			if (applicationObjectExtensionTypeSymbolsAcrossModule is IPageExtensionTypeSymbol { AddedControlsFlattened: var addedControlsFlattened })
			{
				string text2 = addedControlsFlattened.FirstOrDefault((IControlSymbol c) => c.Id == controlId)?.Name;
				if (!string.IsNullOrEmpty(text2))
				{
					return text2;
				}
			}
		}
		return string.Empty;
	}

	public static string ResolvePageActionName(this Compilation compilation, string pageName, int actionId)
	{
		IPageTypeSymbol pageTypeSymbol = (IPageTypeSymbol)compilation.GetApplicationObjectTypeSymbolsByNameAcrossModules(SymbolKind.Page, pageName).FirstOrDefault();
		string text = pageTypeSymbol?.FlattenedActions.FirstOrDefault((IActionSymbol c) => c.Id == actionId)?.Name;
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		foreach (IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in compilation.GetApplicationObjectExtensionTypeSymbolsAcrossModules(pageTypeSymbol))
		{
			if (applicationObjectExtensionTypeSymbolsAcrossModule is IPageExtensionTypeSymbol { AddedActionsFlattened: var addedActionsFlattened })
			{
				string text2 = addedActionsFlattened.FirstOrDefault((IActionSymbol c) => c.Id == actionId)?.Name;
				if (!string.IsNullOrEmpty(text2))
				{
					return text2;
				}
			}
		}
		return string.Empty;
	}

	public static PageViewSyntax GetPageViewSyntaxFromCompilation(this Compilation compilation, int pageId, string viewName)
	{
		ViewSymbol viewSymbolFromCompilation = compilation.GetViewSymbolFromCompilation(pageId, viewName);
		if (viewSymbolFromCompilation == null)
		{
			return null;
		}
		if (viewSymbolFromCompilation is SourceViewSymbol sourceViewSymbol)
		{
			return (PageViewSyntax)sourceViewSymbol.GetFirstSyntax();
		}
		return ReferenceSymbolToSyntax.ConvertPageView(viewSymbolFromCompilation);
	}

	private static ViewSymbol GetViewSymbolFromCompilation(this Compilation compilation, int pageId, string viewName)
	{
		foreach (PageTypeSymbol item in compilation.GetObjectSymbolsByIdAcrossModules(SymbolKind.Page, pageId).Cast<PageTypeSymbol>())
		{
			if (!item.PageType.IsViewSupported())
			{
				continue;
			}
			foreach (ViewSymbol flattenedView in item.FlattenedViews)
			{
				if (SemanticFacts.IsSameName(flattenedView.Name, viewName))
				{
					return flattenedView;
				}
			}
			foreach (PageExtensionBaseTypeSymbol item2 in compilation.GetApplicationObjectExtensionTypeSymbolsAcrossModules(item).Cast<PageExtensionBaseTypeSymbol>())
			{
				foreach (ViewSymbol flattenedView2 in item2.FlattenedViews)
				{
					if (SemanticFacts.IsSameName(flattenedView2.Name, viewName))
					{
						return flattenedView2;
					}
				}
			}
		}
		return null;
	}

	public static ImmutableArray<string> UnescapeViewName(string namespacedViewName)
	{
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < namespacedViewName.Length; i++)
			{
				char c = namespacedViewName[i];
				char? c2 = ((i + 1 == namespacedViewName.Length) ? ((char?)null) : new char?(namespacedViewName[i + 1]));
				if (c != '|' && c != '_')
				{
					stringBuilder.Append(c);
				}
				else if (c == '|' && c2.HasValue && c2 == '_')
				{
					stringBuilder.Append('_');
					i++;
				}
				else if (c == '|' && c2.HasValue && c2 == '|')
				{
					stringBuilder.Append('|');
					i++;
				}
				else if (c == '_')
				{
					instance.Add(stringBuilder.ToString());
					stringBuilder = stringBuilder.Clear();
				}
			}
			instance.Add(stringBuilder.ToString());
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	public static string CreateEscapedViewName(Guid appId, string viewName)
	{
		return EscapeName(appId.ToString()) + "_" + EscapeName(viewName);
	}

	private static string EscapeName(string name)
	{
		return name.Replace("|", "||").Replace("_", "|_");
	}

	internal static ControlAddInTypeSymbol GetControlAddInTypeSymbol(this Compilation compilation, ObjectNameReferenceSyntax identifier, DiagnosticBag diagnostics)
	{
		return compilation.LookupControlAddInTypeSymbol(identifier, diagnostics);
	}

	private static ControlAddInTypeSymbol LookupControlAddInTypeSymbol(this Compilation compilation, ObjectNameReferenceSyntax identifier, DiagnosticBag diagnostics)
	{
		string identifierOrLiteralValue = identifier.Identifier.GetIdentifierOrLiteralValue();
		ControlAddInTypeSymbol controlAddInTypeSymbol = null;
		if (compilation.TryLookupDotNetControlAddInTypeSymbol(identifier, identifierOrLiteralValue, ref controlAddInTypeSymbol, diagnostics))
		{
			return controlAddInTypeSymbol;
		}
		return compilation.LookupNativeControlAddInTypeSymbol(identifier, identifierOrLiteralValue, diagnostics);
	}

	private static bool TryLookupDotNetControlAddInTypeSymbol(this Compilation compilation, ObjectNameReferenceSyntax identifier, string controlAddInName, ref ControlAddInTypeSymbol controlAddInTypeSymbol, DiagnosticBag diagnostics)
	{
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.DotNet, controlAddInName);
		if (objectSymbolsByNameAcrossModules.IsDefaultOrEmpty)
		{
			return false;
		}
		controlAddInTypeSymbol = compilation.LookupDotNetControlAddInTypeSymbol(identifier, controlAddInName, objectSymbolsByNameAcrossModules, diagnostics);
		return true;
	}

	private static ControlAddInTypeSymbol LookupDotNetControlAddInTypeSymbol(this Compilation compilation, ObjectNameReferenceSyntax identifier, string controlAddInName, ImmutableArray<ISymbol> dotNetTypeSymbols, DiagnosticBag diagnostics)
	{
		if (dotNetTypeSymbols.Length > 1)
		{
			diagnostics.Add(ErrorCode.ERR_AmbigReference, identifier.Location, controlAddInName);
			return null;
		}
		DotNetTypeSymbol dotNetTypeSymbol = (DotNetTypeSymbol)dotNetTypeSymbols[0];
		if (!dotNetTypeSymbol.IsControlAddInType)
		{
			diagnostics.Add(ErrorCode.ERR_DotNetTypeDeclarationDoesNotContainIsControlAddInProperty, identifier.Location, dotNetTypeSymbol.Name);
			return null;
		}
		return dotNetTypeSymbol.ControlAddIn;
	}

	private static ControlAddInTypeSymbol LookupNativeControlAddInTypeSymbol(this Compilation compilation, ObjectNameReferenceSyntax identifier, string controlAddInName, DiagnosticBag diagnostics)
	{
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.ControlAddIn, controlAddInName);
		switch (objectSymbolsByNameAcrossModules.Length)
		{
		case 0:
			return null;
		case 1:
			return (ControlAddInTypeSymbol)objectSymbolsByNameAcrossModules[0];
		default:
			diagnostics.Add(ErrorCode.ERR_AmbigReference, identifier.Location, controlAddInName);
			return null;
		}
	}
}
