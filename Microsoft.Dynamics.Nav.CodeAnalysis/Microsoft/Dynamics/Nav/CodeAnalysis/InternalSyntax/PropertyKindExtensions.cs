namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

public static class PropertyKindExtensions
{
	public static SymbolKind GetSymbolKind(this PropertyKind kind)
	{
		switch (kind)
		{
		case PropertyKind.SumIndexFields:
			return SymbolKind.Field;
		case PropertyKind.TableNo:
		case PropertyKind.SourceTable:
		case PropertyKind.LinkTable:
			return SymbolKind.Table;
		case PropertyKind.LookupPageId:
		case PropertyKind.DrillDownPageId:
		case PropertyKind.CardPageId:
		case PropertyKind.RoleCenter:
			return SymbolKind.Page;
		case PropertyKind.Customizations:
			return SymbolKind.PageCustomization;
		default:
			return SymbolKind.Undefined;
		}
	}

	public static bool IsSourceObjectAttribute(this PropertyKind kind)
	{
		switch (kind)
		{
		case PropertyKind.Permissions:
		case PropertyKind.DataCaptionFields:
		case PropertyKind.SourceTable:
		case PropertyKind.SourceTableTemporary:
		case PropertyKind.SourceTableView:
		case PropertyKind.InsertAllowed:
		case PropertyKind.ModifyAllowed:
		case PropertyKind.DeleteAllowed:
		case PropertyKind.AutoSplitKey:
		case PropertyKind.SaveValues:
		case PropertyKind.LinksAllowed:
		case PropertyKind.MultipleNewLines:
		case PropertyKind.PopulateAllFields:
		case PropertyKind.ShowFilter:
		case PropertyKind.DelayedInsert:
		case PropertyKind.ODataKeyFields:
			return true;
		default:
			return false;
		}
	}

	public static ControlKind? GetRequiredReferenceControlKind(this PropertyKind kind)
	{
		switch (kind)
		{
		case PropertyKind.Provider:
			return ControlKind.Part;
		case PropertyKind.IndentationColumn:
		case PropertyKind.IndentationControls:
		case PropertyKind.FreezeColumn:
			return ControlKind.Field;
		default:
			return null;
		}
	}

	public static ControlKind? GetRequiredParentControlKind(this PropertyKind kind)
	{
		if ((uint)(kind - 126) <= 2u)
		{
			return ControlKind.Repeater;
		}
		return null;
	}

	public static bool IsTranslatableProperty(this PropertyKind kind)
	{
		switch (kind)
		{
		case PropertyKind.Caption:
		case PropertyKind.OptionCaption:
		case PropertyKind.InstructionalText:
		case PropertyKind.PromotedActionCategories:
		case PropertyKind.AdditionalSearchTerms:
		case PropertyKind.ToolTip:
		case PropertyKind.RequestFilterHeading:
			return true;
		default:
			return false;
		}
	}

	public static bool IsEitherCaptionProperty(this PropertyKind kind)
	{
		if (kind != PropertyKind.Caption)
		{
			return kind == PropertyKind.CaptionML;
		}
		return true;
	}
}
