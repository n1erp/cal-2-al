using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class SymbolKindExtensions
{
	public static List<SymbolKind> AllSymbolKinds = Enum.GetValues(typeof(SymbolKind)).Cast<SymbolKind>().ToList();

	public static List<SymbolKind> AllRootObjectSymbolKinds = AllSymbolKinds.Where(IsRootObjectTypeSymbol).ToList();

	public static List<SymbolKind> AllApplicationObjectSymbolKinds = AllSymbolKinds.Where(IsApplicationObject).ToList();

	public static int ToSortOrder(this SymbolKind kind)
	{
		return kind switch
		{
			SymbolKind.GlobalVariable => 0, 
			SymbolKind.Method => 1, 
			SymbolKind.RequestPage => 2, 
			_ => (int)(kind + 100), 
		};
	}

	public static bool IsExtensionObject(this SymbolKind kind)
	{
		if ((uint)(kind - 16) <= 1u || kind == SymbolKind.PageCustomization || kind == SymbolKind.EnumExtension)
		{
			return true;
		}
		return false;
	}

	public static bool IsApplicationObject(this SymbolKind kind)
	{
		if ((uint)(kind - 10) <= 7u || (uint)(kind - 22) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsObjectWithSubtype(this SymbolKind kind)
	{
		if (!kind.IsApplicationObject())
		{
			return kind == SymbolKind.ControlAddIn;
		}
		return true;
	}

	public static bool IsObjectTypeSymbol(this SymbolKind kind)
	{
		if (!kind.IsApplicationObject() && kind != SymbolKind.PageCustomization && kind != SymbolKind.Profile && kind != SymbolKind.ControlAddIn && kind != SymbolKind.DotNet && kind != SymbolKind.DotNetPackage && kind != SymbolKind.DotNetTypeDeclaration)
		{
			return kind == SymbolKind.RequestPage;
		}
		return true;
	}

	public static bool IsRadObjectTypeSymbol(this SymbolKind kind)
	{
		if (!kind.IsApplicationObject() && kind != SymbolKind.PageCustomization)
		{
			return kind == SymbolKind.Profile;
		}
		return true;
	}

	public static bool IsRootObjectTypeSymbol(this SymbolKind kind)
	{
		if (kind.IsObjectTypeSymbol())
		{
			return !kind.IsNestedObject();
		}
		return false;
	}

	public static bool IsNestedObject(this SymbolKind kind)
	{
		return kind == SymbolKind.RequestPage;
	}

	public static bool IsQueryElement(this SymbolKind kind)
	{
		if (kind != SymbolKind.QueryColumn)
		{
			return kind == SymbolKind.QueryFilter;
		}
		return true;
	}

	public static bool IsQueryMember(this SymbolKind kind)
	{
		if (kind != SymbolKind.QueryColumn && kind != SymbolKind.QueryFilter)
		{
			return kind == SymbolKind.QueryDataItem;
		}
		return true;
	}

	public static bool IsTableMember(this SymbolKind kind)
	{
		if (kind != SymbolKind.Field && kind != SymbolKind.FieldGroup)
		{
			return kind == SymbolKind.Key;
		}
		return true;
	}

	public static bool IsTableExtensionMember(this SymbolKind kind)
	{
		if (kind != SymbolKind.Field && kind != SymbolKind.FieldGroup && kind != SymbolKind.Key)
		{
			return kind == SymbolKind.Change;
		}
		return true;
	}

	public static bool IsPageMember(this SymbolKind kind)
	{
		if (kind != SymbolKind.Control && kind != SymbolKind.Action)
		{
			return kind == SymbolKind.View;
		}
		return true;
	}

	public static bool IsPageExtensionMember(this SymbolKind kind)
	{
		if (kind != SymbolKind.Control && kind != SymbolKind.Action && kind != SymbolKind.View)
		{
			return kind == SymbolKind.Change;
		}
		return true;
	}

	public static bool IsReportMember(this SymbolKind kind)
	{
		if ((uint)(kind - 103) <= 3u)
		{
			return true;
		}
		return false;
	}

	public static bool IsKind(this SymbolKind kind, SymbolKind otherKind)
	{
		return kind == otherKind;
	}

	public static bool IsKind(this SymbolKind kind, SymbolKind otherKind1, SymbolKind otherKind2)
	{
		if (kind != otherKind1)
		{
			return kind == otherKind2;
		}
		return true;
	}

	public static bool IsKind(this SymbolKind kind, SymbolKind otherKind1, SymbolKind otherKind2, SymbolKind otherKind3)
	{
		if (kind != otherKind1 && kind != otherKind2)
		{
			return kind == otherKind3;
		}
		return true;
	}

	public static bool IsKind(this SymbolKind kind, SymbolKind otherKind1, SymbolKind otherKind2, SymbolKind otherKind3, SymbolKind otherKind4)
	{
		if (kind != otherKind1 && kind != otherKind2 && kind != otherKind3)
		{
			return kind == otherKind4;
		}
		return true;
	}
}
