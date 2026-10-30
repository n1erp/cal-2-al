using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Translation;

internal static class LanguageFileUtilities
{
	internal static string GetTranslationFileName(string appName, string extension)
	{
		if (string.IsNullOrEmpty(appName))
		{
			appName = "default";
		}
		else
		{
			char[] invalidChars = Path.GetInvalidFileNameChars();
			appName = string.Join(string.Empty, appName.Where((char c) => !invalidChars.Contains(c)));
		}
		return appName + ".g" + extension;
	}

	internal static string GetLanguageSymbolId(Symbol symbol, bool shouldHashName, bool isMissingCaption = false)
	{
		PooledList<string> instance = PooledList<string>.GetInstance();
		try
		{
			Symbol containingSymbol = symbol.ContainingSymbol;
			IRootTypeSymbol containingSymbolOfType = symbol.GetContainingSymbolOfType<IRootTypeSymbol>();
			ISymbol symbol2 = containingSymbolOfType;
			instance.Add(GetSingleSymbolId(containingSymbolOfType, shouldHashName));
			if (containingSymbolOfType != containingSymbol.ContainingSymbol && containingSymbol.Kind == SymbolKind.Method)
			{
				symbol2 = containingSymbol.ContainingSymbol;
				instance.Add(GetSingleSymbolId(containingSymbol.ContainingSymbol, shouldHashName));
			}
			if (!isMissingCaption && symbol2 != containingSymbol)
			{
				instance.Add(GetSingleSymbolId(containingSymbol, shouldHashName));
			}
			instance.Add(GetSingleSymbolId(symbol, shouldHashName));
			if (isMissingCaption)
			{
				instance.Add(GetMissingPropertyCaptionId(shouldHashName));
			}
			return MakeSymbolIdString(instance);
		}
		finally
		{
			instance.Free();
		}
	}

	private static string GetMissingPropertyCaptionId(bool shouldHashName)
	{
		string text = PropertyKind.Caption.ToString();
		if (shouldHashName)
		{
			text = GetNameHash(text);
		}
		return string.Concat(SymbolKind.Property, " ", text);
	}

	internal static string MakeSymbolIdString(IList<string> ids)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		for (int i = 0; i < ids.Count; i++)
		{
			if (instance.Length > 0)
			{
				instance.Builder.Append(" - ");
			}
			instance.Builder.Append(ids[i]);
		}
		return instance.ToStringAndFree();
	}

	internal static IEnumerable<string> Distinct(IEnumerable<string> ids)
	{
		List<string> list = new List<string>();
		foreach (string id in ids)
		{
			if (id != null && !list.Contains(id))
			{
				list.Add(id);
			}
		}
		return list;
	}

	private static string GetSingleSymbolId(ISymbol symbol, bool shouldHashName)
	{
		if (symbol.Kind == SymbolKind.Module)
		{
			return null;
		}
		string text = symbol.Kind.ToString();
		string text2 = ((!shouldHashName) ? symbol.Name : GetNameHash(symbol.Name));
		return text + " " + text2;
	}

	internal static string GetItemTypeFromSymbol(Symbol symbol)
	{
		return string.Concat(symbol.GetContainingSymbolOfType<IRootTypeSymbol>().Kind, " - ", symbol.Kind);
	}

	internal static string GetNameHash(string name)
	{
		return ((long)Hash.GetFNVHashCode(name) + 2147483647L).ToString(CultureInfo.InvariantCulture);
	}

	internal static string EscapeCdata(string text)
	{
		return text?.Replace("]", "]5D;");
	}
}
