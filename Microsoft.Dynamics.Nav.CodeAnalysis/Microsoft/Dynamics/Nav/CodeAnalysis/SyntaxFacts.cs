using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[CompilerGenerated]
public static class SyntaxFacts
{
	public const int EventSubscriberParameterAttributesLength = 6;

	public const int EventSubscriberObjectTypePosition = 0;

	public const int EventSubscriberObjectIdPosition = 1;

	public const int EventSubscriberEventNamePosition = 2;

	public const int EventSubscriberElementNamePosition = 3;

	public const int EventSubscriberSkipOnMissingLicense = 4;

	public const int EventSubscriberSkipOnMissingPermission = 5;

	public const int EventPublisherIncludeSenderPosition = 0;

	public const int EventPublisherEventIncludeGlobalsPosition = 1;

	internal const string RecFieldName = "Rec";

	internal const string OldRecFieldName = "xRec";

	internal const string CurrFieldNoFieldName = "CurrFieldNo";

	internal const string CurrPageFieldName = "CurrPage";

	internal const string CurrXmlPortFieldName = "currXMLport";

	internal const string ParentPageFieldName = "ParentPage";

	internal const string SecurityFilterEnumName = "SecurityFilter";

	internal const string CurrReportFieldName = "CurrReport";

	internal const string RequestPageName = "RequestOptionsPage";

	internal const string CurrQueryFieldName = "CurrQuery";

	internal const string ReservedReportColumnNameSuffix = "Format";

	private static Lazy<ImmutableArray<AreaKind>> lazyAreaValues = new Lazy<ImmutableArray<AreaKind>>(() => (from AreaKind v in typeof(AreaKind).GetTypeInfo().GetEnumValues()
		where v != AreaKind.None
		select v).ToImmutableArray());

	public const char OptionMembersSeparatorChar = ',';

	public const int MaximumAlIdentifierLength = 120;

	public const int MSReservedIdRange = 2000000000;

	internal static ImmutableArray<AreaKind> AreaValues = lazyAreaValues.Value;

	internal static ImmutableArray<SyntaxKind> SupportedPermissionKeywordKinds = new SyntaxKind[8]
	{
		SyntaxKind.TableDataKeyword,
		SyntaxKind.TableKeyword,
		SyntaxKind.PageKeyword,
		SyntaxKind.QueryKeyword,
		SyntaxKind.ReportKeyword,
		SyntaxKind.CodeunitKeyword,
		SyntaxKind.XmlPortKeyword,
		SyntaxKind.SystemKeyword
	}.AsImmutable();

	internal static string SupportedPermissionApplicationObjectsString = string.Join(", ", SupportedPermissionKeywordKinds.Select(GetText));

	internal static ImmutableArray<SyntaxKind> SupportedApplicationObjectKeywordKinds = new SyntaxKind[14]
	{
		SyntaxKind.TableKeyword,
		SyntaxKind.TableExtensionKeyword,
		SyntaxKind.PageKeyword,
		SyntaxKind.PageExtensionKeyword,
		SyntaxKind.PageCustomizationKeyword,
		SyntaxKind.ProfileKeyword,
		SyntaxKind.CodeunitKeyword,
		SyntaxKind.ReportKeyword,
		SyntaxKind.XmlPortKeyword,
		SyntaxKind.QueryKeyword,
		SyntaxKind.ControlAddInKeyword,
		SyntaxKind.DotNetKeyword,
		SyntaxKind.EnumKeyword,
		SyntaxKind.EnumExtensionKeyword
	}.AsImmutable();

	internal static string SupportedApplicationObjectsString = string.Join(", ", SupportedApplicationObjectKeywordKinds.Select(GetText));

	internal static ImmutableArray<SyntaxKind> SupportedApplicationObjectReferenceKeywordKinds = new SyntaxKind[6]
	{
		SyntaxKind.CodeunitKeyword,
		SyntaxKind.PageKeyword,
		SyntaxKind.TableKeyword,
		SyntaxKind.ReportKeyword,
		SyntaxKind.XmlPortKeyword,
		SyntaxKind.QueryKeyword
	}.AsImmutable();

	internal static string SupportedApplicationObjectReferencesString = string.Join(", ", SupportedApplicationObjectReferenceKeywordKinds.Select(GetText));

	internal static ImmutableArray<ActionAreaKind> ActionAreaValues = (from ActionAreaKind v in typeof(ActionAreaKind).GetTypeInfo().GetEnumValues()
		where v != ActionAreaKind.None
		select v).ToImmutableArray();

	public static bool IsIdentifierStartCharacter(char identifierStart)
	{
		return UnicodeCharacterUtilities.IsIdentifierStartCharacter(identifierStart);
	}

	public static bool IsIdentifierPartCharacter(char identifierPart)
	{
		return UnicodeCharacterUtilities.IsIdentifierPartCharacter(identifierPart);
	}

	public static bool IsValidIdentifier(string name)
	{
		return UnicodeCharacterUtilities.IsValidIdentifier(name);
	}

	public static bool IsWhitespace(char ch)
	{
		if (ch != ' ' && ch != '\t' && ch != '\v' && ch != '\f' && ch != '\u00a0' && ch != '\ufeff' && ch != '\u001a')
		{
			if (ch > 'ÿ')
			{
				return CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.SpaceSeparator;
			}
			return false;
		}
		return true;
	}

	public static bool IsNewLine(char ch)
	{
		if (ch != '\r' && ch != '\n' && ch != '\u0085' && ch != '\u2028')
		{
			return ch == '\u2029';
		}
		return true;
	}

	public static bool IsCodeunitProperty(PropertyKind property)
	{
		if (property == PropertyKind.Description || (uint)(property - 8) <= 6u)
		{
			return true;
		}
		return false;
	}

	public static bool IsFieldProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Subtype:
		case PropertyKind.DataClassification:
		case PropertyKind.ExternalName:
		case PropertyKind.AutoFormatType:
		case PropertyKind.AutoFormatExpression:
		case PropertyKind.BlankNumbers:
		case PropertyKind.BlankZero:
		case PropertyKind.SignDisplacement:
		case PropertyKind.MinValue:
		case PropertyKind.MaxValue:
		case PropertyKind.NotBlank:
		case PropertyKind.Numeric:
		case PropertyKind.CharAllowed:
		case PropertyKind.DateFormula:
		case PropertyKind.ValuesAllowed:
		case PropertyKind.OptionCaptionML:
		case PropertyKind.OptionCaption:
		case PropertyKind.ClosingDates:
		case PropertyKind.DecimalPlaces:
		case PropertyKind.AccessByPermission:
		case PropertyKind.ExtendedDatatype:
		case PropertyKind.Width:
		case PropertyKind.CaptionClass:
		case PropertyKind.InitValue:
		case PropertyKind.FieldClass:
		case PropertyKind.CalcFormula:
		case PropertyKind.TableRelation:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.SqlDataType:
		case PropertyKind.ValidateTableRelation:
		case PropertyKind.TestTableRelation:
		case PropertyKind.Compressed:
		case PropertyKind.AutoIncrement:
		case PropertyKind.SqlTimestamp:
		case PropertyKind.OptionMembers:
		case PropertyKind.ExternalType:
		case PropertyKind.ExternalAccess:
		case PropertyKind.OptionOrdinalValues:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
			return true;
		default:
			return false;
		}
	}

	public static bool IsKeyProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.Enabled:
		case PropertyKind.SumIndexFields:
		case PropertyKind.MaintainSqlIndex:
		case PropertyKind.MaintainSiftIndex:
		case PropertyKind.Clustered:
		case PropertyKind.SqlIndex:
		case PropertyKind.Unique:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Permissions:
		case PropertyKind.DataCaptionFields:
		case PropertyKind.AccessByPermission:
		case PropertyKind.Editable:
		case PropertyKind.SourceTable:
		case PropertyKind.SourceTableTemporary:
		case PropertyKind.SourceTableView:
		case PropertyKind.CardPageId:
		case PropertyKind.InsertAllowed:
		case PropertyKind.ModifyAllowed:
		case PropertyKind.DeleteAllowed:
		case PropertyKind.AutoSplitKey:
		case PropertyKind.SaveValues:
		case PropertyKind.LinksAllowed:
		case PropertyKind.MultipleNewLines:
		case PropertyKind.PopulateAllFields:
		case PropertyKind.ShowFilter:
		case PropertyKind.InstructionalTextML:
		case PropertyKind.InstructionalText:
		case PropertyKind.DataCaptionExpression:
		case PropertyKind.HelpLink:
		case PropertyKind.ContextSensitiveHelpPage:
		case PropertyKind.PageType:
		case PropertyKind.DelayedInsert:
		case PropertyKind.RefreshOnActivate:
		case PropertyKind.PromotedActionCategoriesML:
		case PropertyKind.PromotedActionCategories:
		case PropertyKind.ODataKeyFields:
		case PropertyKind.APIVersion:
		case PropertyKind.EntityName:
		case PropertyKind.EntitySetName:
		case PropertyKind.APIGroup:
		case PropertyKind.APIPublisher:
		case PropertyKind.ChangeTrackingAllowed:
		case PropertyKind.QueryCategory:
		case PropertyKind.AdditionalSearchTermsML:
		case PropertyKind.AdditionalSearchTerms:
		case PropertyKind.ApplicationArea:
		case PropertyKind.UsageCategory:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageActionProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Scope:
		case PropertyKind.AccessByPermission:
		case PropertyKind.Enabled:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.Image:
		case PropertyKind.RunPageMode:
		case PropertyKind.Promoted:
		case PropertyKind.PromotedIsBig:
		case PropertyKind.PromotedOnly:
		case PropertyKind.PromotedCategory:
		case PropertyKind.Ellipsis:
		case PropertyKind.ShortcutKey:
		case PropertyKind.RunObject:
		case PropertyKind.RunPageView:
		case PropertyKind.RunPageLink:
		case PropertyKind.RunPageOnRec:
		case PropertyKind.InFooterBar:
		case PropertyKind.Gesture:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageActionAreaProperty(PropertyKind property)
	{
		if (property == PropertyKind.Description || (uint)(property - 108) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageActionGroupProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Enabled:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.Image:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageAreaProperty(PropertyKind property)
	{
		if (property == PropertyKind.Description)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageFieldProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.LookupPageId:
		case PropertyKind.DrillDownPageId:
		case PropertyKind.AutoFormatType:
		case PropertyKind.AutoFormatExpression:
		case PropertyKind.BlankNumbers:
		case PropertyKind.BlankZero:
		case PropertyKind.SignDisplacement:
		case PropertyKind.MinValue:
		case PropertyKind.MaxValue:
		case PropertyKind.NotBlank:
		case PropertyKind.Numeric:
		case PropertyKind.CharAllowed:
		case PropertyKind.DateFormula:
		case PropertyKind.ValuesAllowed:
		case PropertyKind.OptionCaptionML:
		case PropertyKind.OptionCaption:
		case PropertyKind.ClosingDates:
		case PropertyKind.DecimalPlaces:
		case PropertyKind.AccessByPermission:
		case PropertyKind.ExtendedDatatype:
		case PropertyKind.Width:
		case PropertyKind.CaptionClass:
		case PropertyKind.TableRelation:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.Style:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.StyleExpr:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.RowSpan:
		case PropertyKind.ColumnSpan:
		case PropertyKind.HideValue:
		case PropertyKind.Importance:
		case PropertyKind.MultiLine:
		case PropertyKind.ShowCaption:
		case PropertyKind.ShowMandatory:
		case PropertyKind.Title:
		case PropertyKind.QuickEntry:
		case PropertyKind.Lookup:
		case PropertyKind.DrillDown:
		case PropertyKind.AssistEdit:
		case PropertyKind.Image:
		case PropertyKind.ODataEDMType:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageGroupProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.InstructionalTextML:
		case PropertyKind.InstructionalText:
		case PropertyKind.Visible:
		case PropertyKind.ShowCaption:
		case PropertyKind.GridLayout:
		case PropertyKind.CuegroupLayout:
		case PropertyKind.IndentationColumn:
		case PropertyKind.IndentationControls:
		case PropertyKind.FreezeColumn:
		case PropertyKind.ShowAsTree:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageLabelProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.CaptionClass:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.Style:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.StyleExpr:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.RowSpan:
		case PropertyKind.ColumnSpan:
		case PropertyKind.HideValue:
		case PropertyKind.Importance:
		case PropertyKind.MultiLine:
		case PropertyKind.ShowCaption:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPagePartProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.AccessByPermission:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.ShowFilter:
		case PropertyKind.EntityName:
		case PropertyKind.EntitySetName:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.SubPageView:
		case PropertyKind.SubPageLink:
		case PropertyKind.UpdatePropagation:
		case PropertyKind.Provider:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageSystemPartProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.AccessByPermission:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.ShowFilter:
		case PropertyKind.EntityName:
		case PropertyKind.EntitySetName:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.SubPageView:
		case PropertyKind.SubPageLink:
		case PropertyKind.UpdatePropagation:
		case PropertyKind.Provider:
			return true;
		default:
			return false;
		}
	}

	public static bool IsPageChartPartProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.AccessByPermission:
		case PropertyKind.Enabled:
		case PropertyKind.Editable:
		case PropertyKind.ShowFilter:
		case PropertyKind.EntityName:
		case PropertyKind.EntitySetName:
		case PropertyKind.ApplicationArea:
		case PropertyKind.Visible:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.SubPageView:
		case PropertyKind.SubPageLink:
		case PropertyKind.UpdatePropagation:
		case PropertyKind.Provider:
			return true;
		default:
			return false;
		}
	}

	public static bool IsQueryProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Permissions:
		case PropertyKind.APIVersion:
		case PropertyKind.EntityName:
		case PropertyKind.EntitySetName:
		case PropertyKind.APIGroup:
		case PropertyKind.APIPublisher:
		case PropertyKind.QueryCategory:
		case PropertyKind.OrderBy:
		case PropertyKind.TopNumberOfRows:
		case PropertyKind.ReadState:
		case PropertyKind.QueryType:
			return true;
		default:
			return false;
		}
	}

	public static bool IsQueryColumnProperty(PropertyKind property)
	{
		if ((uint)(property - 1) <= 2u || (uint)(property - 216) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsQueryDataItemProperty(PropertyKind property)
	{
		if (property == PropertyKind.Description || property == PropertyKind.DataItemLink || (uint)(property - 214) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsQueryFilterProperty(PropertyKind property)
	{
		if ((uint)(property - 1) <= 2u || property == PropertyKind.ColumnFilter)
		{
			return true;
		}
		return false;
	}

	public static bool IsReportProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Permissions:
		case PropertyKind.AccessByPermission:
		case PropertyKind.AdditionalSearchTermsML:
		case PropertyKind.AdditionalSearchTerms:
		case PropertyKind.ApplicationArea:
		case PropertyKind.TransactionType:
		case PropertyKind.UseRequestPage:
		case PropertyKind.UseSystemPrinter:
		case PropertyKind.EnableExternalImages:
		case PropertyKind.EnableHyperlinks:
		case PropertyKind.EnableExternalAssemblies:
		case PropertyKind.ProcessingOnly:
		case PropertyKind.ShowPrintStatus:
		case PropertyKind.PaperSourceFirstPage:
		case PropertyKind.PaperSourceDefaultPage:
		case PropertyKind.PaperSourceLastPage:
		case PropertyKind.DefaultLayout:
		case PropertyKind.WordMergeDataItem:
		case PropertyKind.PdfFontEmbedding:
		case PropertyKind.RDLCLayout:
		case PropertyKind.WordLayout:
		case PropertyKind.PreviewMode:
		case PropertyKind.UsageCategory:
			return true;
		default:
			return false;
		}
	}

	public static bool IsReportDataItemProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.RequestFilterFields:
		case PropertyKind.RequestFilterHeadingML:
		case PropertyKind.RequestFilterHeading:
		case PropertyKind.UseTemporary:
		case PropertyKind.CalcFields:
		case PropertyKind.DataItemTableView:
		case PropertyKind.DataItemLinkReference:
		case PropertyKind.DataItemLink:
		case PropertyKind.MaxIteration:
		case PropertyKind.PrintOnlyIfDetail:
			return true;
		default:
			return false;
		}
	}

	public static bool IsReportColumnProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.AutoFormatType:
		case PropertyKind.AutoFormatExpression:
		case PropertyKind.OptionCaptionML:
		case PropertyKind.OptionCaption:
		case PropertyKind.DecimalPlaces:
		case PropertyKind.OptionMembers:
		case PropertyKind.AutoCalcField:
		case PropertyKind.IncludeCaption:
			return true;
		default:
			return false;
		}
	}

	public static bool IsRequestPageProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Permissions:
		case PropertyKind.DataCaptionFields:
		case PropertyKind.Editable:
		case PropertyKind.SourceTable:
		case PropertyKind.SourceTableTemporary:
		case PropertyKind.SourceTableView:
		case PropertyKind.CardPageId:
		case PropertyKind.InsertAllowed:
		case PropertyKind.ModifyAllowed:
		case PropertyKind.DeleteAllowed:
		case PropertyKind.AutoSplitKey:
		case PropertyKind.SaveValues:
		case PropertyKind.LinksAllowed:
		case PropertyKind.MultipleNewLines:
		case PropertyKind.PopulateAllFields:
		case PropertyKind.ShowFilter:
		case PropertyKind.InstructionalTextML:
		case PropertyKind.InstructionalText:
		case PropertyKind.DataCaptionExpression:
		case PropertyKind.HelpLink:
		case PropertyKind.ContextSensitiveHelpPage:
			return true;
		default:
			return false;
		}
	}

	public static bool IsTableProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.Permissions:
		case PropertyKind.DataClassification:
		case PropertyKind.DataPerCompany:
		case PropertyKind.LookupPageId:
		case PropertyKind.DrillDownPageId:
		case PropertyKind.DataCaptionFields:
		case PropertyKind.PasteIsValid:
		case PropertyKind.LinkedObject:
		case PropertyKind.LinkedInTransaction:
		case PropertyKind.TableType:
		case PropertyKind.CompressionType:
		case PropertyKind.ExternalName:
		case PropertyKind.ExternalSchema:
		case PropertyKind.Scope:
		case PropertyKind.ReplicateData:
		case PropertyKind.ObsoleteState:
		case PropertyKind.ObsoleteReason:
			return true;
		default:
			return false;
		}
	}

	public static bool IsXmlPortProperty(PropertyKind property)
	{
		if ((uint)(property - 1) <= 2u || property == PropertyKind.Permissions || (uint)(property - 150) <= 19u)
		{
			return true;
		}
		return false;
	}

	public static bool IsXmlPortTextElementProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.Width:
		case PropertyKind.TextType:
		case PropertyKind.MaxOccurs:
		case PropertyKind.MinOccurs:
		case PropertyKind.XmlName:
		case PropertyKind.NamespacePrefix:
		case PropertyKind.Unbound:
			return true;
		default:
			return false;
		}
	}

	public static bool IsXmlPortFieldElementProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.Width:
		case PropertyKind.AutoCalcField:
		case PropertyKind.FieldValidate:
		case PropertyKind.MaxOccurs:
		case PropertyKind.MinOccurs:
		case PropertyKind.XmlName:
		case PropertyKind.NamespacePrefix:
		case PropertyKind.Unbound:
			return true;
		default:
			return false;
		}
	}

	public static bool IsXmlPortTableElementProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.Width:
		case PropertyKind.SourceTableView:
		case PropertyKind.LinkTable:
		case PropertyKind.LinkTableForceInsert:
		case PropertyKind.LinkFields:
		case PropertyKind.RequestFilterFields:
		case PropertyKind.RequestFilterHeadingML:
		case PropertyKind.RequestFilterHeading:
		case PropertyKind.UseTemporary:
		case PropertyKind.AutoReplace:
		case PropertyKind.AutoSave:
		case PropertyKind.AutoUpdate:
		case PropertyKind.CalcFields:
		case PropertyKind.MaxOccurs:
		case PropertyKind.MinOccurs:
		case PropertyKind.XmlName:
		case PropertyKind.NamespacePrefix:
		case PropertyKind.Unbound:
			return true;
		default:
			return false;
		}
	}

	public static bool IsXmlPortFieldAttributeProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.Width:
		case PropertyKind.AutoCalcField:
		case PropertyKind.FieldValidate:
		case PropertyKind.Occurrence:
		case PropertyKind.XmlName:
		case PropertyKind.NamespacePrefix:
		case PropertyKind.Unbound:
			return true;
		default:
			return false;
		}
	}

	public static bool IsXmlPortTextAttributeProperty(PropertyKind property)
	{
		switch (property)
		{
		case PropertyKind.Description:
		case PropertyKind.Width:
		case PropertyKind.TextType:
		case PropertyKind.Occurrence:
		case PropertyKind.XmlName:
		case PropertyKind.NamespacePrefix:
		case PropertyKind.Unbound:
			return true;
		default:
			return false;
		}
	}

	public static bool IsFieldGroupProperty(PropertyKind property)
	{
		if ((uint)(property - 2) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageActionSeparatorProperty(PropertyKind property)
	{
		if ((uint)(property - 2) <= 1u || property == PropertyKind.IsHeader)
		{
			return true;
		}
		return false;
	}

	public static bool IsEnumValueProperty(PropertyKind property)
	{
		if ((uint)(property - 2) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsDotNetAssemblyProperty(PropertyKind property)
	{
		if ((uint)(property - 4) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsDotNetTypeDeclarationProperty(PropertyKind property)
	{
		if (property == PropertyKind.IsControlAddIn)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageViewProperty(PropertyKind property)
	{
		if ((uint)(property - 2) <= 1u || property == PropertyKind.Visible || (uint)(property - 148) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsProfileProperty(PropertyKind property)
	{
		if (property == PropertyKind.Description || (uint)(property - 219) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageCustomizationObjectProperty(PropertyKind property)
	{
		return false;
	}

	public static bool IsControlAddInObjectProperty(PropertyKind property)
	{
		if ((uint)(property - 221) <= 15u)
		{
			return true;
		}
		return false;
	}

	public static bool IsPageUserControlProperty(PropertyKind property)
	{
		if ((uint)(property - 105) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsEnumTypeProperty(PropertyKind property)
	{
		if (property == PropertyKind.Extensible)
		{
			return true;
		}
		return false;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetCodeunitPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"TABLENO" => PropertyKind.TableNo, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"SUBTYPE" => PropertyKind.Subtype, 
			"SINGLEINSTANCE" => PropertyKind.SingleInstance, 
			"TESTISOLATION" => PropertyKind.TestIsolation, 
			"EVENTSUBSCRIBERINSTANCE" => PropertyKind.EventSubscriberInstance, 
			"TESTPERMISSIONS" => PropertyKind.TestPermissions, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetFieldPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"AUTOFORMATTYPE" => PropertyKind.AutoFormatType, 
			"AUTOFORMATEXPRESSION" => PropertyKind.AutoFormatExpression, 
			"BLANKNUMBERS" => PropertyKind.BlankNumbers, 
			"BLANKZERO" => PropertyKind.BlankZero, 
			"SIGNDISPLACEMENT" => PropertyKind.SignDisplacement, 
			"MINVALUE" => PropertyKind.MinValue, 
			"MAXVALUE" => PropertyKind.MaxValue, 
			"NOTBLANK" => PropertyKind.NotBlank, 
			"NUMERIC" => PropertyKind.Numeric, 
			"CHARALLOWED" => PropertyKind.CharAllowed, 
			"DATEFORMULA" => PropertyKind.DateFormula, 
			"VALUESALLOWED" => PropertyKind.ValuesAllowed, 
			"OPTIONCAPTIONML" => PropertyKind.OptionCaptionML, 
			"OPTIONCAPTION" => PropertyKind.OptionCaption, 
			"CLOSINGDATES" => PropertyKind.ClosingDates, 
			"DECIMALPLACES" => PropertyKind.DecimalPlaces, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"EXTENDEDDATATYPE" => PropertyKind.ExtendedDatatype, 
			"WIDTH" => PropertyKind.Width, 
			"CAPTIONCLASS" => PropertyKind.CaptionClass, 
			"DATACLASSIFICATION" => PropertyKind.DataClassification, 
			"INITVALUE" => PropertyKind.InitValue, 
			"FIELDCLASS" => PropertyKind.FieldClass, 
			"CALCFORMULA" => PropertyKind.CalcFormula, 
			"TABLERELATION" => PropertyKind.TableRelation, 
			"ENABLED" => PropertyKind.Enabled, 
			"EDITABLE" => PropertyKind.Editable, 
			"SQLDATATYPE" => PropertyKind.SqlDataType, 
			"VALIDATETABLERELATION" => PropertyKind.ValidateTableRelation, 
			"TESTTABLERELATION" => PropertyKind.TestTableRelation, 
			"SUBTYPE" => PropertyKind.Subtype, 
			"COMPRESSED" => PropertyKind.Compressed, 
			"AUTOINCREMENT" => PropertyKind.AutoIncrement, 
			"SQLTIMESTAMP" => PropertyKind.SqlTimestamp, 
			"OPTIONMEMBERS" => PropertyKind.OptionMembers, 
			"EXTERNALNAME" => PropertyKind.ExternalName, 
			"EXTERNALTYPE" => PropertyKind.ExternalType, 
			"EXTERNALACCESS" => PropertyKind.ExternalAccess, 
			"OPTIONORDINALVALUES" => PropertyKind.OptionOrdinalValues, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetKeyPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"ENABLED" => PropertyKind.Enabled, 
			"SUMINDEXFIELDS" => PropertyKind.SumIndexFields, 
			"MAINTAINSQLINDEX" => PropertyKind.MaintainSqlIndex, 
			"MAINTAINSIFTINDEX" => PropertyKind.MaintainSiftIndex, 
			"CLUSTERED" => PropertyKind.Clustered, 
			"SQLINDEX" => PropertyKind.SqlIndex, 
			"UNIQUE" => PropertyKind.Unique, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPagePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"SOURCETABLE" => PropertyKind.SourceTable, 
			"SOURCETABLETEMPORARY" => PropertyKind.SourceTableTemporary, 
			"SOURCETABLEVIEW" => PropertyKind.SourceTableView, 
			"DATACAPTIONFIELDS" => PropertyKind.DataCaptionFields, 
			"CARDPAGEID" => PropertyKind.CardPageId, 
			"EDITABLE" => PropertyKind.Editable, 
			"INSERTALLOWED" => PropertyKind.InsertAllowed, 
			"MODIFYALLOWED" => PropertyKind.ModifyAllowed, 
			"DELETEALLOWED" => PropertyKind.DeleteAllowed, 
			"AUTOSPLITKEY" => PropertyKind.AutoSplitKey, 
			"SAVEVALUES" => PropertyKind.SaveValues, 
			"LINKSALLOWED" => PropertyKind.LinksAllowed, 
			"MULTIPLENEWLINES" => PropertyKind.MultipleNewLines, 
			"POPULATEALLFIELDS" => PropertyKind.PopulateAllFields, 
			"SHOWFILTER" => PropertyKind.ShowFilter, 
			"INSTRUCTIONALTEXTML" => PropertyKind.InstructionalTextML, 
			"INSTRUCTIONALTEXT" => PropertyKind.InstructionalText, 
			"DATACAPTIONEXPRESSION" => PropertyKind.DataCaptionExpression, 
			"HELPLINK" => PropertyKind.HelpLink, 
			"CONTEXTSENSITIVEHELPPAGE" => PropertyKind.ContextSensitiveHelpPage, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"PAGETYPE" => PropertyKind.PageType, 
			"DELAYEDINSERT" => PropertyKind.DelayedInsert, 
			"REFRESHONACTIVATE" => PropertyKind.RefreshOnActivate, 
			"PROMOTEDACTIONCATEGORIESML" => PropertyKind.PromotedActionCategoriesML, 
			"PROMOTEDACTIONCATEGORIES" => PropertyKind.PromotedActionCategories, 
			"ODATAKEYFIELDS" => PropertyKind.ODataKeyFields, 
			"APIVERSION" => PropertyKind.APIVersion, 
			"ENTITYNAME" => PropertyKind.EntityName, 
			"ENTITYSETNAME" => PropertyKind.EntitySetName, 
			"APIGROUP" => PropertyKind.APIGroup, 
			"APIPUBLISHER" => PropertyKind.APIPublisher, 
			"CHANGETRACKINGALLOWED" => PropertyKind.ChangeTrackingAllowed, 
			"QUERYCATEGORY" => PropertyKind.QueryCategory, 
			"ADDITIONALSEARCHTERMSML" => PropertyKind.AdditionalSearchTermsML, 
			"ADDITIONALSEARCHTERMS" => PropertyKind.AdditionalSearchTerms, 
			"USAGECATEGORY" => PropertyKind.UsageCategory, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageActionPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"VISIBLE" => PropertyKind.Visible, 
			"ENABLED" => PropertyKind.Enabled, 
			"IMAGE" => PropertyKind.Image, 
			"RUNPAGEMODE" => PropertyKind.RunPageMode, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"PROMOTED" => PropertyKind.Promoted, 
			"PROMOTEDISBIG" => PropertyKind.PromotedIsBig, 
			"PROMOTEDONLY" => PropertyKind.PromotedOnly, 
			"PROMOTEDCATEGORY" => PropertyKind.PromotedCategory, 
			"SCOPE" => PropertyKind.Scope, 
			"ELLIPSIS" => PropertyKind.Ellipsis, 
			"SHORTCUTKEY" => PropertyKind.ShortcutKey, 
			"RUNOBJECT" => PropertyKind.RunObject, 
			"RUNPAGEVIEW" => PropertyKind.RunPageView, 
			"RUNPAGELINK" => PropertyKind.RunPageLink, 
			"RUNPAGEONREC" => PropertyKind.RunPageOnRec, 
			"INFOOTERBAR" => PropertyKind.InFooterBar, 
			"GESTURE" => PropertyKind.Gesture, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageActionAreaPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageActionGroupPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"VISIBLE" => PropertyKind.Visible, 
			"ENABLED" => PropertyKind.Enabled, 
			"IMAGE" => PropertyKind.Image, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageAreaPropertyKind(string name)
	{
		string text = name.ToUpperInvariant();
		if (text == "DESCRIPTION")
		{
			return PropertyKind.Description;
		}
		return PropertyKind.None;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageFieldPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"AUTOFORMATTYPE" => PropertyKind.AutoFormatType, 
			"AUTOFORMATEXPRESSION" => PropertyKind.AutoFormatExpression, 
			"BLANKNUMBERS" => PropertyKind.BlankNumbers, 
			"BLANKZERO" => PropertyKind.BlankZero, 
			"SIGNDISPLACEMENT" => PropertyKind.SignDisplacement, 
			"MINVALUE" => PropertyKind.MinValue, 
			"MAXVALUE" => PropertyKind.MaxValue, 
			"NOTBLANK" => PropertyKind.NotBlank, 
			"NUMERIC" => PropertyKind.Numeric, 
			"CHARALLOWED" => PropertyKind.CharAllowed, 
			"DATEFORMULA" => PropertyKind.DateFormula, 
			"VALUESALLOWED" => PropertyKind.ValuesAllowed, 
			"OPTIONCAPTIONML" => PropertyKind.OptionCaptionML, 
			"OPTIONCAPTION" => PropertyKind.OptionCaption, 
			"CLOSINGDATES" => PropertyKind.ClosingDates, 
			"DECIMALPLACES" => PropertyKind.DecimalPlaces, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"EXTENDEDDATATYPE" => PropertyKind.ExtendedDatatype, 
			"WIDTH" => PropertyKind.Width, 
			"STYLE" => PropertyKind.Style, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"VISIBLE" => PropertyKind.Visible, 
			"STYLEEXPR" => PropertyKind.StyleExpr, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"ROWSPAN" => PropertyKind.RowSpan, 
			"COLUMNSPAN" => PropertyKind.ColumnSpan, 
			"CAPTIONCLASS" => PropertyKind.CaptionClass, 
			"ENABLED" => PropertyKind.Enabled, 
			"EDITABLE" => PropertyKind.Editable, 
			"HIDEVALUE" => PropertyKind.HideValue, 
			"SHOWMANDATORY" => PropertyKind.ShowMandatory, 
			"MULTILINE" => PropertyKind.MultiLine, 
			"SHOWCAPTION" => PropertyKind.ShowCaption, 
			"LOOKUPPAGEID" => PropertyKind.LookupPageId, 
			"DRILLDOWNPAGEID" => PropertyKind.DrillDownPageId, 
			"IMPORTANCE" => PropertyKind.Importance, 
			"TITLE" => PropertyKind.Title, 
			"QUICKENTRY" => PropertyKind.QuickEntry, 
			"LOOKUP" => PropertyKind.Lookup, 
			"DRILLDOWN" => PropertyKind.DrillDown, 
			"ASSISTEDIT" => PropertyKind.AssistEdit, 
			"IMAGE" => PropertyKind.Image, 
			"ODATAEDMTYPE" => PropertyKind.ODataEDMType, 
			"TABLERELATION" => PropertyKind.TableRelation, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageGroupPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"VISIBLE" => PropertyKind.Visible, 
			"EDITABLE" => PropertyKind.Editable, 
			"ENABLED" => PropertyKind.Enabled, 
			"INSTRUCTIONALTEXTML" => PropertyKind.InstructionalTextML, 
			"INSTRUCTIONALTEXT" => PropertyKind.InstructionalText, 
			"GRIDLAYOUT" => PropertyKind.GridLayout, 
			"CUEGROUPLAYOUT" => PropertyKind.CuegroupLayout, 
			"INDENTATIONCOLUMN" => PropertyKind.IndentationColumn, 
			"INDENTATIONCONTROLS" => PropertyKind.IndentationControls, 
			"FREEZECOLUMN" => PropertyKind.FreezeColumn, 
			"SHOWASTREE" => PropertyKind.ShowAsTree, 
			"SHOWCAPTION" => PropertyKind.ShowCaption, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageLabelPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"STYLE" => PropertyKind.Style, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"VISIBLE" => PropertyKind.Visible, 
			"STYLEEXPR" => PropertyKind.StyleExpr, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"ROWSPAN" => PropertyKind.RowSpan, 
			"COLUMNSPAN" => PropertyKind.ColumnSpan, 
			"CAPTIONCLASS" => PropertyKind.CaptionClass, 
			"EDITABLE" => PropertyKind.Editable, 
			"ENABLED" => PropertyKind.Enabled, 
			"HIDEVALUE" => PropertyKind.HideValue, 
			"IMPORTANCE" => PropertyKind.Importance, 
			"MULTILINE" => PropertyKind.MultiLine, 
			"SHOWCAPTION" => PropertyKind.ShowCaption, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPagePartPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"VISIBLE" => PropertyKind.Visible, 
			"ENABLED" => PropertyKind.Enabled, 
			"EDITABLE" => PropertyKind.Editable, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"SUBPAGEVIEW" => PropertyKind.SubPageView, 
			"SUBPAGELINK" => PropertyKind.SubPageLink, 
			"SHOWFILTER" => PropertyKind.ShowFilter, 
			"UPDATEPROPAGATION" => PropertyKind.UpdatePropagation, 
			"PROVIDER" => PropertyKind.Provider, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"ENTITYNAME" => PropertyKind.EntityName, 
			"ENTITYSETNAME" => PropertyKind.EntitySetName, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageSystemPartPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"VISIBLE" => PropertyKind.Visible, 
			"ENABLED" => PropertyKind.Enabled, 
			"EDITABLE" => PropertyKind.Editable, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"SUBPAGEVIEW" => PropertyKind.SubPageView, 
			"SUBPAGELINK" => PropertyKind.SubPageLink, 
			"SHOWFILTER" => PropertyKind.ShowFilter, 
			"UPDATEPROPAGATION" => PropertyKind.UpdatePropagation, 
			"PROVIDER" => PropertyKind.Provider, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"ENTITYNAME" => PropertyKind.EntityName, 
			"ENTITYSETNAME" => PropertyKind.EntitySetName, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageChartPartPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"VISIBLE" => PropertyKind.Visible, 
			"ENABLED" => PropertyKind.Enabled, 
			"EDITABLE" => PropertyKind.Editable, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"SUBPAGEVIEW" => PropertyKind.SubPageView, 
			"SUBPAGELINK" => PropertyKind.SubPageLink, 
			"SHOWFILTER" => PropertyKind.ShowFilter, 
			"UPDATEPROPAGATION" => PropertyKind.UpdatePropagation, 
			"PROVIDER" => PropertyKind.Provider, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"ENTITYNAME" => PropertyKind.EntityName, 
			"ENTITYSETNAME" => PropertyKind.EntitySetName, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetQueryPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"ORDERBY" => PropertyKind.OrderBy, 
			"TOPNUMBEROFROWS" => PropertyKind.TopNumberOfRows, 
			"READSTATE" => PropertyKind.ReadState, 
			"QUERYTYPE" => PropertyKind.QueryType, 
			"APIVERSION" => PropertyKind.APIVersion, 
			"ENTITYNAME" => PropertyKind.EntityName, 
			"ENTITYSETNAME" => PropertyKind.EntitySetName, 
			"APIGROUP" => PropertyKind.APIGroup, 
			"APIPUBLISHER" => PropertyKind.APIPublisher, 
			"QUERYCATEGORY" => PropertyKind.QueryCategory, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetQueryColumnPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"COLUMNFILTER" => PropertyKind.ColumnFilter, 
			"METHOD" => PropertyKind.Method, 
			"REVERSESIGN" => PropertyKind.ReverseSign, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetQueryDataItemPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"DATAITEMLINK" => PropertyKind.DataItemLink, 
			"SQLJOINTYPE" => PropertyKind.SqlJoinType, 
			"DATAITEMTABLEFILTER" => PropertyKind.DataItemTableFilter, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetQueryFilterPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"COLUMNFILTER" => PropertyKind.ColumnFilter, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetReportPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"TRANSACTIONTYPE" => PropertyKind.TransactionType, 
			"USEREQUESTPAGE" => PropertyKind.UseRequestPage, 
			"USESYSTEMPRINTER" => PropertyKind.UseSystemPrinter, 
			"ENABLEEXTERNALIMAGES" => PropertyKind.EnableExternalImages, 
			"ENABLEHYPERLINKS" => PropertyKind.EnableHyperlinks, 
			"ENABLEEXTERNALASSEMBLIES" => PropertyKind.EnableExternalAssemblies, 
			"PROCESSINGONLY" => PropertyKind.ProcessingOnly, 
			"SHOWPRINTSTATUS" => PropertyKind.ShowPrintStatus, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"PAPERSOURCEFIRSTPAGE" => PropertyKind.PaperSourceFirstPage, 
			"PAPERSOURCEDEFAULTPAGE" => PropertyKind.PaperSourceDefaultPage, 
			"PAPERSOURCELASTPAGE" => PropertyKind.PaperSourceLastPage, 
			"DEFAULTLAYOUT" => PropertyKind.DefaultLayout, 
			"WORDMERGEDATAITEM" => PropertyKind.WordMergeDataItem, 
			"PDFFONTEMBEDDING" => PropertyKind.PdfFontEmbedding, 
			"RDLCLAYOUT" => PropertyKind.RDLCLayout, 
			"WORDLAYOUT" => PropertyKind.WordLayout, 
			"PREVIEWMODE" => PropertyKind.PreviewMode, 
			"ADDITIONALSEARCHTERMSML" => PropertyKind.AdditionalSearchTermsML, 
			"ADDITIONALSEARCHTERMS" => PropertyKind.AdditionalSearchTerms, 
			"USAGECATEGORY" => PropertyKind.UsageCategory, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetReportDataItemPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"DATAITEMTABLEVIEW" => PropertyKind.DataItemTableView, 
			"DATAITEMLINKREFERENCE" => PropertyKind.DataItemLinkReference, 
			"DATAITEMLINK" => PropertyKind.DataItemLink, 
			"REQUESTFILTERHEADINGML" => PropertyKind.RequestFilterHeadingML, 
			"REQUESTFILTERHEADING" => PropertyKind.RequestFilterHeading, 
			"REQUESTFILTERFIELDS" => PropertyKind.RequestFilterFields, 
			"CALCFIELDS" => PropertyKind.CalcFields, 
			"MAXITERATION" => PropertyKind.MaxIteration, 
			"PRINTONLYIFDETAIL" => PropertyKind.PrintOnlyIfDetail, 
			"USETEMPORARY" => PropertyKind.UseTemporary, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetReportColumnPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"OPTIONCAPTIONML" => PropertyKind.OptionCaptionML, 
			"OPTIONCAPTION" => PropertyKind.OptionCaption, 
			"OPTIONMEMBERS" => PropertyKind.OptionMembers, 
			"DECIMALPLACES" => PropertyKind.DecimalPlaces, 
			"AUTOFORMATTYPE" => PropertyKind.AutoFormatType, 
			"AUTOFORMATEXPRESSION" => PropertyKind.AutoFormatExpression, 
			"AUTOCALCFIELD" => PropertyKind.AutoCalcField, 
			"INCLUDECAPTION" => PropertyKind.IncludeCaption, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetRequestPagePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"SOURCETABLE" => PropertyKind.SourceTable, 
			"SOURCETABLETEMPORARY" => PropertyKind.SourceTableTemporary, 
			"SOURCETABLEVIEW" => PropertyKind.SourceTableView, 
			"DATACAPTIONFIELDS" => PropertyKind.DataCaptionFields, 
			"CARDPAGEID" => PropertyKind.CardPageId, 
			"EDITABLE" => PropertyKind.Editable, 
			"INSERTALLOWED" => PropertyKind.InsertAllowed, 
			"MODIFYALLOWED" => PropertyKind.ModifyAllowed, 
			"DELETEALLOWED" => PropertyKind.DeleteAllowed, 
			"AUTOSPLITKEY" => PropertyKind.AutoSplitKey, 
			"SAVEVALUES" => PropertyKind.SaveValues, 
			"LINKSALLOWED" => PropertyKind.LinksAllowed, 
			"MULTIPLENEWLINES" => PropertyKind.MultipleNewLines, 
			"POPULATEALLFIELDS" => PropertyKind.PopulateAllFields, 
			"SHOWFILTER" => PropertyKind.ShowFilter, 
			"INSTRUCTIONALTEXTML" => PropertyKind.InstructionalTextML, 
			"INSTRUCTIONALTEXT" => PropertyKind.InstructionalText, 
			"DATACAPTIONEXPRESSION" => PropertyKind.DataCaptionExpression, 
			"HELPLINK" => PropertyKind.HelpLink, 
			"CONTEXTSENSITIVEHELPPAGE" => PropertyKind.ContextSensitiveHelpPage, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetTablePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"DATACLASSIFICATION" => PropertyKind.DataClassification, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"DATAPERCOMPANY" => PropertyKind.DataPerCompany, 
			"LOOKUPPAGEID" => PropertyKind.LookupPageId, 
			"DRILLDOWNPAGEID" => PropertyKind.DrillDownPageId, 
			"DATACAPTIONFIELDS" => PropertyKind.DataCaptionFields, 
			"PASTEISVALID" => PropertyKind.PasteIsValid, 
			"LINKEDOBJECT" => PropertyKind.LinkedObject, 
			"LINKEDINTRANSACTION" => PropertyKind.LinkedInTransaction, 
			"TABLETYPE" => PropertyKind.TableType, 
			"COMPRESSIONTYPE" => PropertyKind.CompressionType, 
			"EXTERNALNAME" => PropertyKind.ExternalName, 
			"EXTERNALSCHEMA" => PropertyKind.ExternalSchema, 
			"SCOPE" => PropertyKind.Scope, 
			"REPLICATEDATA" => PropertyKind.ReplicateData, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetXmlPortPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"TRANSACTIONTYPE" => PropertyKind.TransactionType, 
			"DEFAULTFIELDSVALIDATION" => PropertyKind.DefaultFieldsValidation, 
			"DEFAULTNAMESPACE" => PropertyKind.DefaultNamespace, 
			"DIRECTION" => PropertyKind.Direction, 
			"ENCODING" => PropertyKind.Encoding, 
			"FIELDDELIMITER" => PropertyKind.FieldDelimiter, 
			"FIELDSEPARATOR" => PropertyKind.FieldSeparator, 
			"FILENAME" => PropertyKind.FileName, 
			"FORMAT" => PropertyKind.Format, 
			"FORMATEVALUATE" => PropertyKind.FormatEvaluate, 
			"INLINESCHEMA" => PropertyKind.InlineSchema, 
			"NAMESPACES" => PropertyKind.Namespaces, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"PRESERVEWHITESPACE" => PropertyKind.PreserveWhiteSpace, 
			"RECORDSEPARATOR" => PropertyKind.RecordSeparator, 
			"TABLESEPARATOR" => PropertyKind.TableSeparator, 
			"TEXTENCODING" => PropertyKind.TextEncoding, 
			"USEDEFAULTNAMESPACE" => PropertyKind.UseDefaultNamespace, 
			"USELAX" => PropertyKind.UseLax, 
			"USEREQUESTPAGE" => PropertyKind.UseRequestPage, 
			"XMLVERSIONNO" => PropertyKind.XmlVersionNo, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetXmlPortTextElementPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"TEXTTYPE" => PropertyKind.TextType, 
			"MAXOCCURS" => PropertyKind.MaxOccurs, 
			"MINOCCURS" => PropertyKind.MinOccurs, 
			"XMLNAME" => PropertyKind.XmlName, 
			"NAMESPACEPREFIX" => PropertyKind.NamespacePrefix, 
			"UNBOUND" => PropertyKind.Unbound, 
			"WIDTH" => PropertyKind.Width, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetXmlPortFieldElementPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"AUTOCALCFIELD" => PropertyKind.AutoCalcField, 
			"FIELDVALIDATE" => PropertyKind.FieldValidate, 
			"MAXOCCURS" => PropertyKind.MaxOccurs, 
			"MINOCCURS" => PropertyKind.MinOccurs, 
			"XMLNAME" => PropertyKind.XmlName, 
			"NAMESPACEPREFIX" => PropertyKind.NamespacePrefix, 
			"UNBOUND" => PropertyKind.Unbound, 
			"WIDTH" => PropertyKind.Width, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetXmlPortTableElementPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"LINKTABLE" => PropertyKind.LinkTable, 
			"LINKTABLEFORCEINSERT" => PropertyKind.LinkTableForceInsert, 
			"LINKFIELDS" => PropertyKind.LinkFields, 
			"REQUESTFILTERFIELDS" => PropertyKind.RequestFilterFields, 
			"REQUESTFILTERHEADINGML" => PropertyKind.RequestFilterHeadingML, 
			"REQUESTFILTERHEADING" => PropertyKind.RequestFilterHeading, 
			"SOURCETABLEVIEW" => PropertyKind.SourceTableView, 
			"USETEMPORARY" => PropertyKind.UseTemporary, 
			"AUTOREPLACE" => PropertyKind.AutoReplace, 
			"AUTOSAVE" => PropertyKind.AutoSave, 
			"AUTOUPDATE" => PropertyKind.AutoUpdate, 
			"CALCFIELDS" => PropertyKind.CalcFields, 
			"MAXOCCURS" => PropertyKind.MaxOccurs, 
			"MINOCCURS" => PropertyKind.MinOccurs, 
			"XMLNAME" => PropertyKind.XmlName, 
			"NAMESPACEPREFIX" => PropertyKind.NamespacePrefix, 
			"UNBOUND" => PropertyKind.Unbound, 
			"WIDTH" => PropertyKind.Width, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetXmlPortFieldAttributePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"AUTOCALCFIELD" => PropertyKind.AutoCalcField, 
			"FIELDVALIDATE" => PropertyKind.FieldValidate, 
			"OCCURRENCE" => PropertyKind.Occurrence, 
			"XMLNAME" => PropertyKind.XmlName, 
			"NAMESPACEPREFIX" => PropertyKind.NamespacePrefix, 
			"UNBOUND" => PropertyKind.Unbound, 
			"WIDTH" => PropertyKind.Width, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetXmlPortTextAttributePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"TEXTTYPE" => PropertyKind.TextType, 
			"OCCURRENCE" => PropertyKind.Occurrence, 
			"XMLNAME" => PropertyKind.XmlName, 
			"NAMESPACEPREFIX" => PropertyKind.NamespacePrefix, 
			"UNBOUND" => PropertyKind.Unbound, 
			"WIDTH" => PropertyKind.Width, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetFieldGroupPropertyKind(string name)
	{
		string text = name.ToUpperInvariant();
		if (!(text == "CAPTIONML"))
		{
			if (text == "CAPTION")
			{
				return PropertyKind.Caption;
			}
			return PropertyKind.None;
		}
		return PropertyKind.CaptionML;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageActionSeparatorPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"ISHEADER" => PropertyKind.IsHeader, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetEnumValuePropertyKind(string name)
	{
		string text = name.ToUpperInvariant();
		if (!(text == "CAPTIONML"))
		{
			if (text == "CAPTION")
			{
				return PropertyKind.Caption;
			}
			return PropertyKind.None;
		}
		return PropertyKind.CaptionML;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetDotNetAssemblyPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"VERSION" => PropertyKind.Version, 
			"CULTURE" => PropertyKind.Culture, 
			"PUBLICKEYTOKEN" => PropertyKind.PublicKeyToken, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetDotNetTypeDeclarationPropertyKind(string name)
	{
		string text = name.ToUpperInvariant();
		if (text == "ISCONTROLADDIN")
		{
			return PropertyKind.IsControlAddIn;
		}
		return PropertyKind.None;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageViewPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"VISIBLE" => PropertyKind.Visible, 
			"FILTERS" => PropertyKind.Filters, 
			"ORDERBY" => PropertyKind.OrderBy, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetProfilePropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"ROLECENTER" => PropertyKind.RoleCenter, 
			"CUSTOMIZATIONS" => PropertyKind.Customizations, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageCustomizationObjectPropertyKind(string name)
	{
		return PropertyKind.None;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetControlAddInObjectPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"SCRIPTS" => PropertyKind.Scripts, 
			"STYLESHEETS" => PropertyKind.StyleSheets, 
			"IMAGES" => PropertyKind.Images, 
			"STARTUPSCRIPT" => PropertyKind.StartupScript, 
			"RECREATESCRIPT" => PropertyKind.RecreateScript, 
			"REFRESHSCRIPT" => PropertyKind.RefreshScript, 
			"REQUESTEDHEIGHT" => PropertyKind.RequestedHeight, 
			"REQUESTEDWIDTH" => PropertyKind.RequestedWidth, 
			"MINIMUMHEIGHT" => PropertyKind.MinimumHeight, 
			"MINIMUMWIDTH" => PropertyKind.MinimumWidth, 
			"MAXIMUMHEIGHT" => PropertyKind.MaximumHeight, 
			"MAXIMUMWIDTH" => PropertyKind.MaximumWidth, 
			"VERTICALSHRINK" => PropertyKind.VerticalShrink, 
			"HORIZONTALSHRINK" => PropertyKind.HorizontalShrink, 
			"VERTICALSTRETCH" => PropertyKind.VerticalStretch, 
			"HORIZONTALSTRETCH" => PropertyKind.HorizontalStretch, 
			_ => PropertyKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetPageUserControlPropertyKind(string name)
	{
		string text = name.ToUpperInvariant();
		if (!(text == "VISIBLE"))
		{
			if (text == "APPLICATIONAREA")
			{
				return PropertyKind.ApplicationArea;
			}
			return PropertyKind.None;
		}
		return PropertyKind.Visible;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static PropertyKind GetEnumTypePropertyKind(string name)
	{
		string text = name.ToUpperInvariant();
		if (text == "EXTENSIBLE")
		{
			return PropertyKind.Extensible;
		}
		return PropertyKind.None;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
	public static PropertyKind GetPropertyKind(string name)
	{
		return name.ToUpperInvariant() switch
		{
			"DESCRIPTION" => PropertyKind.Description, 
			"CAPTIONML" => PropertyKind.CaptionML, 
			"CAPTION" => PropertyKind.Caption, 
			"VERSION" => PropertyKind.Version, 
			"CULTURE" => PropertyKind.Culture, 
			"PUBLICKEYTOKEN" => PropertyKind.PublicKeyToken, 
			"ISCONTROLADDIN" => PropertyKind.IsControlAddIn, 
			"TABLENO" => PropertyKind.TableNo, 
			"PERMISSIONS" => PropertyKind.Permissions, 
			"SUBTYPE" => PropertyKind.Subtype, 
			"SINGLEINSTANCE" => PropertyKind.SingleInstance, 
			"TESTISOLATION" => PropertyKind.TestIsolation, 
			"EVENTSUBSCRIBERINSTANCE" => PropertyKind.EventSubscriberInstance, 
			"TESTPERMISSIONS" => PropertyKind.TestPermissions, 
			"DATACLASSIFICATION" => PropertyKind.DataClassification, 
			"DATAPERCOMPANY" => PropertyKind.DataPerCompany, 
			"LOOKUPPAGEID" => PropertyKind.LookupPageId, 
			"DRILLDOWNPAGEID" => PropertyKind.DrillDownPageId, 
			"DATACAPTIONFIELDS" => PropertyKind.DataCaptionFields, 
			"PASTEISVALID" => PropertyKind.PasteIsValid, 
			"LINKEDOBJECT" => PropertyKind.LinkedObject, 
			"LINKEDINTRANSACTION" => PropertyKind.LinkedInTransaction, 
			"TABLETYPE" => PropertyKind.TableType, 
			"COMPRESSIONTYPE" => PropertyKind.CompressionType, 
			"EXTERNALNAME" => PropertyKind.ExternalName, 
			"EXTERNALSCHEMA" => PropertyKind.ExternalSchema, 
			"SCOPE" => PropertyKind.Scope, 
			"REPLICATEDATA" => PropertyKind.ReplicateData, 
			"AUTOFORMATTYPE" => PropertyKind.AutoFormatType, 
			"AUTOFORMATEXPRESSION" => PropertyKind.AutoFormatExpression, 
			"BLANKNUMBERS" => PropertyKind.BlankNumbers, 
			"BLANKZERO" => PropertyKind.BlankZero, 
			"SIGNDISPLACEMENT" => PropertyKind.SignDisplacement, 
			"MINVALUE" => PropertyKind.MinValue, 
			"MAXVALUE" => PropertyKind.MaxValue, 
			"NOTBLANK" => PropertyKind.NotBlank, 
			"NUMERIC" => PropertyKind.Numeric, 
			"CHARALLOWED" => PropertyKind.CharAllowed, 
			"DATEFORMULA" => PropertyKind.DateFormula, 
			"VALUESALLOWED" => PropertyKind.ValuesAllowed, 
			"OPTIONCAPTIONML" => PropertyKind.OptionCaptionML, 
			"OPTIONCAPTION" => PropertyKind.OptionCaption, 
			"CLOSINGDATES" => PropertyKind.ClosingDates, 
			"DECIMALPLACES" => PropertyKind.DecimalPlaces, 
			"ACCESSBYPERMISSION" => PropertyKind.AccessByPermission, 
			"EXTENDEDDATATYPE" => PropertyKind.ExtendedDatatype, 
			"WIDTH" => PropertyKind.Width, 
			"CAPTIONCLASS" => PropertyKind.CaptionClass, 
			"INITVALUE" => PropertyKind.InitValue, 
			"FIELDCLASS" => PropertyKind.FieldClass, 
			"CALCFORMULA" => PropertyKind.CalcFormula, 
			"TABLERELATION" => PropertyKind.TableRelation, 
			"ENABLED" => PropertyKind.Enabled, 
			"EDITABLE" => PropertyKind.Editable, 
			"SQLDATATYPE" => PropertyKind.SqlDataType, 
			"VALIDATETABLERELATION" => PropertyKind.ValidateTableRelation, 
			"TESTTABLERELATION" => PropertyKind.TestTableRelation, 
			"COMPRESSED" => PropertyKind.Compressed, 
			"AUTOINCREMENT" => PropertyKind.AutoIncrement, 
			"SQLTIMESTAMP" => PropertyKind.SqlTimestamp, 
			"OPTIONMEMBERS" => PropertyKind.OptionMembers, 
			"EXTERNALTYPE" => PropertyKind.ExternalType, 
			"EXTERNALACCESS" => PropertyKind.ExternalAccess, 
			"OPTIONORDINALVALUES" => PropertyKind.OptionOrdinalValues, 
			"SUMINDEXFIELDS" => PropertyKind.SumIndexFields, 
			"MAINTAINSQLINDEX" => PropertyKind.MaintainSqlIndex, 
			"MAINTAINSIFTINDEX" => PropertyKind.MaintainSiftIndex, 
			"CLUSTERED" => PropertyKind.Clustered, 
			"SQLINDEX" => PropertyKind.SqlIndex, 
			"UNIQUE" => PropertyKind.Unique, 
			"SOURCETABLE" => PropertyKind.SourceTable, 
			"SOURCETABLETEMPORARY" => PropertyKind.SourceTableTemporary, 
			"SOURCETABLEVIEW" => PropertyKind.SourceTableView, 
			"CARDPAGEID" => PropertyKind.CardPageId, 
			"INSERTALLOWED" => PropertyKind.InsertAllowed, 
			"MODIFYALLOWED" => PropertyKind.ModifyAllowed, 
			"DELETEALLOWED" => PropertyKind.DeleteAllowed, 
			"AUTOSPLITKEY" => PropertyKind.AutoSplitKey, 
			"SAVEVALUES" => PropertyKind.SaveValues, 
			"LINKSALLOWED" => PropertyKind.LinksAllowed, 
			"MULTIPLENEWLINES" => PropertyKind.MultipleNewLines, 
			"POPULATEALLFIELDS" => PropertyKind.PopulateAllFields, 
			"SHOWFILTER" => PropertyKind.ShowFilter, 
			"INSTRUCTIONALTEXTML" => PropertyKind.InstructionalTextML, 
			"INSTRUCTIONALTEXT" => PropertyKind.InstructionalText, 
			"DATACAPTIONEXPRESSION" => PropertyKind.DataCaptionExpression, 
			"HELPLINK" => PropertyKind.HelpLink, 
			"CONTEXTSENSITIVEHELPPAGE" => PropertyKind.ContextSensitiveHelpPage, 
			"PAGETYPE" => PropertyKind.PageType, 
			"DELAYEDINSERT" => PropertyKind.DelayedInsert, 
			"REFRESHONACTIVATE" => PropertyKind.RefreshOnActivate, 
			"PROMOTEDACTIONCATEGORIESML" => PropertyKind.PromotedActionCategoriesML, 
			"PROMOTEDACTIONCATEGORIES" => PropertyKind.PromotedActionCategories, 
			"ODATAKEYFIELDS" => PropertyKind.ODataKeyFields, 
			"APIVERSION" => PropertyKind.APIVersion, 
			"ENTITYNAME" => PropertyKind.EntityName, 
			"ENTITYSETNAME" => PropertyKind.EntitySetName, 
			"APIGROUP" => PropertyKind.APIGroup, 
			"APIPUBLISHER" => PropertyKind.APIPublisher, 
			"CHANGETRACKINGALLOWED" => PropertyKind.ChangeTrackingAllowed, 
			"QUERYCATEGORY" => PropertyKind.QueryCategory, 
			"ADDITIONALSEARCHTERMSML" => PropertyKind.AdditionalSearchTermsML, 
			"ADDITIONALSEARCHTERMS" => PropertyKind.AdditionalSearchTerms, 
			"STYLE" => PropertyKind.Style, 
			"APPLICATIONAREA" => PropertyKind.ApplicationArea, 
			"VISIBLE" => PropertyKind.Visible, 
			"STYLEEXPR" => PropertyKind.StyleExpr, 
			"TOOLTIPML" => PropertyKind.ToolTipML, 
			"TOOLTIP" => PropertyKind.ToolTip, 
			"ROWSPAN" => PropertyKind.RowSpan, 
			"COLUMNSPAN" => PropertyKind.ColumnSpan, 
			"HIDEVALUE" => PropertyKind.HideValue, 
			"IMPORTANCE" => PropertyKind.Importance, 
			"MULTILINE" => PropertyKind.MultiLine, 
			"SHOWCAPTION" => PropertyKind.ShowCaption, 
			"SHOWMANDATORY" => PropertyKind.ShowMandatory, 
			"TITLE" => PropertyKind.Title, 
			"QUICKENTRY" => PropertyKind.QuickEntry, 
			"LOOKUP" => PropertyKind.Lookup, 
			"DRILLDOWN" => PropertyKind.DrillDown, 
			"ASSISTEDIT" => PropertyKind.AssistEdit, 
			"IMAGE" => PropertyKind.Image, 
			"ODATAEDMTYPE" => PropertyKind.ODataEDMType, 
			"GRIDLAYOUT" => PropertyKind.GridLayout, 
			"CUEGROUPLAYOUT" => PropertyKind.CuegroupLayout, 
			"INDENTATIONCOLUMN" => PropertyKind.IndentationColumn, 
			"INDENTATIONCONTROLS" => PropertyKind.IndentationControls, 
			"FREEZECOLUMN" => PropertyKind.FreezeColumn, 
			"SHOWASTREE" => PropertyKind.ShowAsTree, 
			"SUBPAGEVIEW" => PropertyKind.SubPageView, 
			"SUBPAGELINK" => PropertyKind.SubPageLink, 
			"UPDATEPROPAGATION" => PropertyKind.UpdatePropagation, 
			"PROVIDER" => PropertyKind.Provider, 
			"RUNPAGEMODE" => PropertyKind.RunPageMode, 
			"PROMOTED" => PropertyKind.Promoted, 
			"PROMOTEDISBIG" => PropertyKind.PromotedIsBig, 
			"PROMOTEDONLY" => PropertyKind.PromotedOnly, 
			"PROMOTEDCATEGORY" => PropertyKind.PromotedCategory, 
			"ELLIPSIS" => PropertyKind.Ellipsis, 
			"SHORTCUTKEY" => PropertyKind.ShortcutKey, 
			"RUNOBJECT" => PropertyKind.RunObject, 
			"RUNPAGEVIEW" => PropertyKind.RunPageView, 
			"RUNPAGELINK" => PropertyKind.RunPageLink, 
			"RUNPAGEONREC" => PropertyKind.RunPageOnRec, 
			"INFOOTERBAR" => PropertyKind.InFooterBar, 
			"GESTURE" => PropertyKind.Gesture, 
			"ISHEADER" => PropertyKind.IsHeader, 
			"FILTERS" => PropertyKind.Filters, 
			"ORDERBY" => PropertyKind.OrderBy, 
			"TRANSACTIONTYPE" => PropertyKind.TransactionType, 
			"DEFAULTFIELDSVALIDATION" => PropertyKind.DefaultFieldsValidation, 
			"DEFAULTNAMESPACE" => PropertyKind.DefaultNamespace, 
			"DIRECTION" => PropertyKind.Direction, 
			"ENCODING" => PropertyKind.Encoding, 
			"FIELDDELIMITER" => PropertyKind.FieldDelimiter, 
			"FIELDSEPARATOR" => PropertyKind.FieldSeparator, 
			"FILENAME" => PropertyKind.FileName, 
			"FORMAT" => PropertyKind.Format, 
			"FORMATEVALUATE" => PropertyKind.FormatEvaluate, 
			"INLINESCHEMA" => PropertyKind.InlineSchema, 
			"NAMESPACES" => PropertyKind.Namespaces, 
			"PRESERVEWHITESPACE" => PropertyKind.PreserveWhiteSpace, 
			"RECORDSEPARATOR" => PropertyKind.RecordSeparator, 
			"TABLESEPARATOR" => PropertyKind.TableSeparator, 
			"TEXTENCODING" => PropertyKind.TextEncoding, 
			"USEDEFAULTNAMESPACE" => PropertyKind.UseDefaultNamespace, 
			"USELAX" => PropertyKind.UseLax, 
			"USEREQUESTPAGE" => PropertyKind.UseRequestPage, 
			"XMLVERSIONNO" => PropertyKind.XmlVersionNo, 
			"LINKTABLE" => PropertyKind.LinkTable, 
			"LINKTABLEFORCEINSERT" => PropertyKind.LinkTableForceInsert, 
			"LINKFIELDS" => PropertyKind.LinkFields, 
			"REQUESTFILTERFIELDS" => PropertyKind.RequestFilterFields, 
			"REQUESTFILTERHEADINGML" => PropertyKind.RequestFilterHeadingML, 
			"REQUESTFILTERHEADING" => PropertyKind.RequestFilterHeading, 
			"USETEMPORARY" => PropertyKind.UseTemporary, 
			"AUTOREPLACE" => PropertyKind.AutoReplace, 
			"AUTOSAVE" => PropertyKind.AutoSave, 
			"AUTOUPDATE" => PropertyKind.AutoUpdate, 
			"CALCFIELDS" => PropertyKind.CalcFields, 
			"TEXTTYPE" => PropertyKind.TextType, 
			"AUTOCALCFIELD" => PropertyKind.AutoCalcField, 
			"FIELDVALIDATE" => PropertyKind.FieldValidate, 
			"OCCURRENCE" => PropertyKind.Occurrence, 
			"MAXOCCURS" => PropertyKind.MaxOccurs, 
			"MINOCCURS" => PropertyKind.MinOccurs, 
			"XMLNAME" => PropertyKind.XmlName, 
			"NAMESPACEPREFIX" => PropertyKind.NamespacePrefix, 
			"UNBOUND" => PropertyKind.Unbound, 
			"USESYSTEMPRINTER" => PropertyKind.UseSystemPrinter, 
			"ENABLEEXTERNALIMAGES" => PropertyKind.EnableExternalImages, 
			"ENABLEHYPERLINKS" => PropertyKind.EnableHyperlinks, 
			"ENABLEEXTERNALASSEMBLIES" => PropertyKind.EnableExternalAssemblies, 
			"PROCESSINGONLY" => PropertyKind.ProcessingOnly, 
			"SHOWPRINTSTATUS" => PropertyKind.ShowPrintStatus, 
			"PAPERSOURCEFIRSTPAGE" => PropertyKind.PaperSourceFirstPage, 
			"PAPERSOURCEDEFAULTPAGE" => PropertyKind.PaperSourceDefaultPage, 
			"PAPERSOURCELASTPAGE" => PropertyKind.PaperSourceLastPage, 
			"DEFAULTLAYOUT" => PropertyKind.DefaultLayout, 
			"WORDMERGEDATAITEM" => PropertyKind.WordMergeDataItem, 
			"PDFFONTEMBEDDING" => PropertyKind.PdfFontEmbedding, 
			"RDLCLAYOUT" => PropertyKind.RDLCLayout, 
			"WORDLAYOUT" => PropertyKind.WordLayout, 
			"PREVIEWMODE" => PropertyKind.PreviewMode, 
			"DATAITEMTABLEVIEW" => PropertyKind.DataItemTableView, 
			"DATAITEMLINKREFERENCE" => PropertyKind.DataItemLinkReference, 
			"DATAITEMLINK" => PropertyKind.DataItemLink, 
			"MAXITERATION" => PropertyKind.MaxIteration, 
			"PRINTONLYIFDETAIL" => PropertyKind.PrintOnlyIfDetail, 
			"INCLUDECAPTION" => PropertyKind.IncludeCaption, 
			"TOPNUMBEROFROWS" => PropertyKind.TopNumberOfRows, 
			"READSTATE" => PropertyKind.ReadState, 
			"QUERYTYPE" => PropertyKind.QueryType, 
			"SQLJOINTYPE" => PropertyKind.SqlJoinType, 
			"DATAITEMTABLEFILTER" => PropertyKind.DataItemTableFilter, 
			"COLUMNFILTER" => PropertyKind.ColumnFilter, 
			"METHOD" => PropertyKind.Method, 
			"REVERSESIGN" => PropertyKind.ReverseSign, 
			"ROLECENTER" => PropertyKind.RoleCenter, 
			"CUSTOMIZATIONS" => PropertyKind.Customizations, 
			"SCRIPTS" => PropertyKind.Scripts, 
			"STYLESHEETS" => PropertyKind.StyleSheets, 
			"IMAGES" => PropertyKind.Images, 
			"STARTUPSCRIPT" => PropertyKind.StartupScript, 
			"RECREATESCRIPT" => PropertyKind.RecreateScript, 
			"REFRESHSCRIPT" => PropertyKind.RefreshScript, 
			"REQUESTEDHEIGHT" => PropertyKind.RequestedHeight, 
			"REQUESTEDWIDTH" => PropertyKind.RequestedWidth, 
			"MINIMUMHEIGHT" => PropertyKind.MinimumHeight, 
			"MINIMUMWIDTH" => PropertyKind.MinimumWidth, 
			"MAXIMUMHEIGHT" => PropertyKind.MaximumHeight, 
			"MAXIMUMWIDTH" => PropertyKind.MaximumWidth, 
			"VERTICALSHRINK" => PropertyKind.VerticalShrink, 
			"HORIZONTALSHRINK" => PropertyKind.HorizontalShrink, 
			"VERTICALSTRETCH" => PropertyKind.VerticalStretch, 
			"HORIZONTALSTRETCH" => PropertyKind.HorizontalStretch, 
			"OBSOLETESTATE" => PropertyKind.ObsoleteState, 
			"OBSOLETEREASON" => PropertyKind.ObsoleteReason, 
			"USAGECATEGORY" => PropertyKind.UsageCategory, 
			"EXTENSIBLE" => PropertyKind.Extensible, 
			_ => PropertyKind.None, 
		};
	}

	public static SyntaxKind GetUnaryExpression(SyntaxKind token)
	{
		return token switch
		{
			SyntaxKind.PlusToken => SyntaxKind.UnaryPlusExpression, 
			SyntaxKind.MinusToken => SyntaxKind.UnaryMinusExpression, 
			SyntaxKind.NotKeyword => SyntaxKind.UnaryNotExpression, 
			_ => SyntaxKind.None, 
		};
	}

	public static bool IsNestedObjectSyntaxKind(this SyntaxKind kind)
	{
		return kind == SyntaxKind.RequestPage;
	}

	public static SyntaxKind GetBinaryExpression(SyntaxKind token)
	{
		return token switch
		{
			SyntaxKind.EqualsToken => SyntaxKind.EqualsExpression, 
			SyntaxKind.NotEqualsToken => SyntaxKind.NotEqualsExpression, 
			SyntaxKind.LessThanToken => SyntaxKind.LessThanExpression, 
			SyntaxKind.LessThanEqualsToken => SyntaxKind.LessThanOrEqualExpression, 
			SyntaxKind.GreaterThanToken => SyntaxKind.GreaterThanExpression, 
			SyntaxKind.GreaterThanEqualsToken => SyntaxKind.GreaterThanOrEqualExpression, 
			SyntaxKind.PlusToken => SyntaxKind.AddExpression, 
			SyntaxKind.MinusToken => SyntaxKind.SubtractExpression, 
			SyntaxKind.MultiplyToken => SyntaxKind.MultiplyExpression, 
			SyntaxKind.IDivToken => SyntaxKind.IntegerDivideExpression, 
			SyntaxKind.RDivToken => SyntaxKind.DivideExpression, 
			SyntaxKind.ModuloToken => SyntaxKind.ModuloExpression, 
			SyntaxKind.AndKeyword => SyntaxKind.LogicalAndExpression, 
			SyntaxKind.OrKeyword => SyntaxKind.LogicalOrExpression, 
			SyntaxKind.XorKeyword => SyntaxKind.LogicalXorExpression, 
			SyntaxKind.DotDotToken => SyntaxKind.RangeExpression, 
			_ => SyntaxKind.None, 
		};
	}

	public static bool HasGroupActionList(SyntaxKind kind)
	{
		return kind == SyntaxKind.PageCueGroupKeyword;
	}

	internal static bool NameMustBeClsCompliant(this SyntaxKind kind)
	{
		if ((uint)(kind - 299) <= 2u || (uint)(kind - 310) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal static TypeSymbol GetTypeFromSyntaxKind(SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.BooleanLiteralValue:
			return NavCorLib.BooleanType;
		case SyntaxKind.Int32SignedLiteralValue:
			return NavCorLib.IntegerType;
		case SyntaxKind.Int64SignedLiteralValue:
			return NavCorLib.BigIntegerType;
		case SyntaxKind.StringLiteralValue:
			return NavCorLib.StringType;
		case SyntaxKind.DateLiteralValue:
			return NavCorLib.DateType;
		case SyntaxKind.DateTimeLiteralValue:
			return NavCorLib.DateTimeType;
		case SyntaxKind.TimeLiteralValue:
			return NavCorLib.TimeType;
		default:
			DebugAssertHelper.Assert(condition: false, "Unreachable");
			return NavCorLib.NoneType;
		}
	}

	internal static bool IsSyntaxAllowed(SyntaxNode parent, SyntaxKind childKind)
	{
		SyntaxKind kind = parent.Kind;
		switch (kind)
		{
		case SyntaxKind.ControlAddInObject:
			if (childKind != SyntaxKind.MethodDeclaration && childKind != SyntaxKind.EventDeclaration && childKind != SyntaxKind.EventKeyword)
			{
				return childKind == SyntaxKind.ProcedureKeyword;
			}
			return true;
		case SyntaxKind.PageExtensionActionList:
		case SyntaxKind.PageExtensionLayout:
			if (!childKind.IsPageAddChangeKeyword())
			{
				return true;
			}
			if (parent.GetContainingApplicationObjectSyntax().Kind == SyntaxKind.PageCustomizationObject)
			{
				return false;
			}
			if (parent.GetAncestor<PageViewSyntax>() != null)
			{
				return false;
			}
			return true;
		case SyntaxKind.EnumType:
		case SyntaxKind.EnumExtensionType:
		case SyntaxKind.ProfileObject:
		case SyntaxKind.PageCustomizationObject:
			if (childKind != SyntaxKind.VarKeyword && childKind != SyntaxKind.TriggerKeyword && childKind != SyntaxKind.ProcedureKeyword && childKind != SyntaxKind.LocalKeyword)
			{
				return childKind != SyntaxKind.EventDeclaration;
			}
			return false;
		case SyntaxKind.MethodDeclaration:
		{
			SyntaxNode parent2 = parent.Parent;
			if (parent2 != null && parent2.Kind == SyntaxKind.ControlAddInObject)
			{
				if (childKind != SyntaxKind.VarKeyword)
				{
					return childKind != SyntaxKind.BeginKeyword;
				}
				return false;
			}
			break;
		}
		case SyntaxKind.PageArea:
		case SyntaxKind.PageActionArea:
		case SyntaxKind.ActionAddChange:
		case SyntaxKind.ControlAddChange:
			return childKind != SyntaxKind.PageAreaKeyword;
		case SyntaxKind.PageGroup:
			switch (childKind)
			{
			case SyntaxKind.PageAreaKeyword:
				return false;
			case SyntaxKind.ActionsKeyword:
				return ((PageGroupSyntax)parent).ControlKeyword.Kind == SyntaxKind.PageCueGroupKeyword;
			}
			break;
		case SyntaxKind.IdentifierEqualsLiteral:
			if (parent.GetAncestor<LabelSyntax>() != null)
			{
				string propertyName = (parent as IdentifierEqualsLiteralSyntax)?.Identifier.ValueText;
				if (childKind == SyntaxKind.TrueKeyword || childKind == SyntaxKind.FalseKeyword)
				{
					return LabelPropertyHelper.IsBooleanProperty(propertyName);
				}
			}
			break;
		}
		if (kind.IsObject())
		{
			if (childKind != SyntaxKind.EventDeclaration)
			{
				return childKind != SyntaxKind.EventKeyword;
			}
			return false;
		}
		return true;
	}

	internal static string GetPermissionValueObjectTypeName(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.TableDataKeyword => "TableData", 
			SyntaxKind.SystemKeyword => "System", 
			SyntaxKind.TableKeyword => "Table", 
			SyntaxKind.PageKeyword => "Page", 
			SyntaxKind.QueryKeyword => "Query", 
			SyntaxKind.ReportKeyword => "Report", 
			SyntaxKind.CodeunitKeyword => "Codeunit", 
			SyntaxKind.XmlPortKeyword => "XmlPort", 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	public static bool IsTokenIdentifier(this SyntaxKind kind)
	{
		if (kind != SyntaxKind.IdentifierToken)
		{
			return kind.IsKeywordAllowedIdentifier();
		}
		return true;
	}

	public static bool IsCompoundAssignmentOperatorToken(SyntaxKind token)
	{
		if ((uint)(token - 20) <= 3u)
		{
			return true;
		}
		return false;
	}

	public static bool IsMemberStart(this SyntaxKind kind)
	{
		if (kind.IsPageControlOrGroupKeyword() || kind.IsPageActionOrGroupKeyword() || kind.IsXmlPortNodeKeyword())
		{
			return true;
		}
		switch (kind)
		{
		case SyntaxKind.ProcedureKeyword:
		case SyntaxKind.LocalKeyword:
		case SyntaxKind.EventKeyword:
		case SyntaxKind.TriggerKeyword:
		case SyntaxKind.FieldsKeyword:
		case SyntaxKind.PageLayoutKeyword:
		case SyntaxKind.ActionsKeyword:
		case SyntaxKind.DataItemKeyword:
		case SyntaxKind.ColumnKeyword:
		case SyntaxKind.FilterKeyword:
		case SyntaxKind.ViewsKeyword:
			return true;
		default:
			return false;
		}
	}

	public static bool CanGetNameRecommended(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.PageField:
		case SyntaxKind.ReportDataItem:
		case SyntaxKind.ReportColumn:
		case SyntaxKind.XmlPortTableElement:
		case SyntaxKind.QueryDataItem:
		case SyntaxKind.QueryColumn:
		case SyntaxKind.QueryFilter:
			return true;
		default:
			return false;
		}
	}

	public static bool IsLiteral(this SyntaxKind kind)
	{
		if ((uint)(kind - 2) <= 8u || (uint)(kind - 169) <= 7u)
		{
			return true;
		}
		return false;
	}

	public static bool IsDateTimeLiteral(this SyntaxKind kind)
	{
		if ((uint)(kind - 7) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsNumericLiteral(this SyntaxKind kind)
	{
		if ((uint)(kind - 4) <= 1u || kind == SyntaxKind.Int32PropertyValue)
		{
			return true;
		}
		return false;
	}

	public static bool IsStringLiteral(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.StringLiteralToken || kind == SyntaxKind.StringPropertyValue)
		{
			return true;
		}
		return false;
	}

	public static bool IsFilterTokenUnary(this SyntaxKind kind)
	{
		if ((uint)(kind - 24) <= 5u)
		{
			return true;
		}
		return false;
	}

	public static bool IsAnyToken(this SyntaxKind kind)
	{
		if (kind > SyntaxKind.None)
		{
			return kind < SyntaxKind.List;
		}
		return false;
	}

	public static bool IsBinaryComparisonExpressionOperatorToken(SyntaxKind token)
	{
		if ((uint)(token - 24) <= 5u || token == SyntaxKind.InKeyword)
		{
			return true;
		}
		return false;
	}

	public static bool IsUnaryFilterExpressionValueToken(SyntaxKind token)
	{
		if (token == SyntaxKind.GreaterThanEqualsToken || (uint)(token - 400) <= 5u || token == SyntaxKind.RangeToFilterExpression)
		{
			return true;
		}
		return false;
	}

	public static bool IsBinaryFilterExpressionValueToken(SyntaxKind token)
	{
		if ((uint)(token - 406) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsBinaryFilterExpressionToken(this SyntaxKind token)
	{
		if (token == SyntaxKind.DotDotToken || (uint)(token - 139) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static SyntaxKind ToBinaryFilterExpressionKind(this SyntaxKind token)
	{
		return token switch
		{
			SyntaxKind.AndFilterKeyword => SyntaxKind.AndFilterExpression, 
			SyntaxKind.OrFilterKeyword => SyntaxKind.OrFilterExpression, 
			SyntaxKind.DotDotToken => SyntaxKind.RangeBetweenFilterExpression, 
			_ => SyntaxKind.None, 
		};
	}

	public static SyntaxKind ToUnaryFilterExpressionKind(this SyntaxKind token)
	{
		return token switch
		{
			SyntaxKind.NotEqualsToken => SyntaxKind.UnaryNotEqualsFilterExpression, 
			SyntaxKind.EqualsToken => SyntaxKind.UnaryEqualsFilterExpression, 
			SyntaxKind.LessThanToken => SyntaxKind.UnaryLessThanFilterExpression, 
			SyntaxKind.LessThanEqualsToken => SyntaxKind.UnaryLessThanEqualsFilterExpression, 
			SyntaxKind.GreaterThanToken => SyntaxKind.UnaryGreaterThanFilterExpression, 
			SyntaxKind.GreaterThanEqualsToken => SyntaxKind.UnaryGreaterThanEqualsFilterExpression, 
			SyntaxKind.DotDotToken => SyntaxKind.RangeToFilterExpression, 
			_ => SyntaxKind.None, 
		};
	}

	public static bool IsFilterExpressionValueToken(this SyntaxKind kind)
	{
		if (kind != SyntaxKind.OpenParenToken && kind != SyntaxKind.CloseParenToken && !kind.IsBinaryFilterExpressionToken() && !kind.IsFilterTokenUnary() && !kind.IsTokenIdentifier())
		{
			return kind.IsLiteral();
		}
		return true;
	}

	public static bool IsKeyword(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.IDivToken:
		case SyntaxKind.ModuloToken:
		case SyntaxKind.AndKeyword:
		case SyntaxKind.OrKeyword:
		case SyntaxKind.XorKeyword:
		case SyntaxKind.NotKeyword:
		case SyntaxKind.ExitKeyword:
		case SyntaxKind.BeginKeyword:
		case SyntaxKind.CaseKeyword:
		case SyntaxKind.DoKeyword:
		case SyntaxKind.DownToKeyword:
		case SyntaxKind.ElseKeyword:
		case SyntaxKind.EndKeyword:
		case SyntaxKind.ForKeyword:
		case SyntaxKind.IfKeyword:
		case SyntaxKind.InKeyword:
		case SyntaxKind.OfKeyword:
		case SyntaxKind.RepeatKeyword:
		case SyntaxKind.ThenKeyword:
		case SyntaxKind.ToKeyword:
		case SyntaxKind.UntilKeyword:
		case SyntaxKind.WithKeyword:
		case SyntaxKind.WhileKeyword:
		case SyntaxKind.ProgramKeyword:
		case SyntaxKind.ProcedureKeyword:
		case SyntaxKind.FunctionKeyword:
		case SyntaxKind.VarKeyword:
		case SyntaxKind.ArrayKeyword:
		case SyntaxKind.TemporaryKeyword:
		case SyntaxKind.LocalKeyword:
		case SyntaxKind.EventKeyword:
		case SyntaxKind.AssertErrorKeyword:
		case SyntaxKind.SuppressDisposeKeyword:
		case SyntaxKind.SecurityFilteringKeyword:
		case SyntaxKind.ForEachKeyword:
		case SyntaxKind.TriggerKeyword:
		case SyntaxKind.CodeunitKeyword:
		case SyntaxKind.TableKeyword:
		case SyntaxKind.TableDataKeyword:
		case SyntaxKind.SystemKeyword:
		case SyntaxKind.PageKeyword:
		case SyntaxKind.ReportKeyword:
		case SyntaxKind.QueryKeyword:
		case SyntaxKind.XmlPortKeyword:
		case SyntaxKind.ControlAddInKeyword:
		case SyntaxKind.ProfileKeyword:
		case SyntaxKind.DotNetKeyword:
		case SyntaxKind.PageCustomizationKeyword:
		case SyntaxKind.CustomizesKeyword:
		case SyntaxKind.FieldsKeyword:
		case SyntaxKind.FieldKeyword:
		case SyntaxKind.AssemblyKeyword:
		case SyntaxKind.TypeKeyword:
		case SyntaxKind.BreakKeyword:
		case SyntaxKind.FieldGroupsKeyword:
		case SyntaxKind.FieldGroupKeyword:
		case SyntaxKind.KeysKeyword:
		case SyntaxKind.KeyKeyword:
		case SyntaxKind.PageLayoutKeyword:
		case SyntaxKind.PageAreaKeyword:
		case SyntaxKind.PageGroupKeyword:
		case SyntaxKind.PageRepeaterKeyword:
		case SyntaxKind.PageCueGroupKeyword:
		case SyntaxKind.PageFixedKeyword:
		case SyntaxKind.PageGridKeyword:
		case SyntaxKind.PagePartKeyword:
		case SyntaxKind.PageSystemPartKeyword:
		case SyntaxKind.PageChartPartKeyword:
		case SyntaxKind.PageUserControlKeyword:
		case SyntaxKind.ActionsKeyword:
		case SyntaxKind.ActionKeyword:
		case SyntaxKind.SeparatorKeyword:
		case SyntaxKind.TableExtensionKeyword:
		case SyntaxKind.PageExtensionKeyword:
		case SyntaxKind.ExtendsKeyword:
		case SyntaxKind.AddFirstKeyword:
		case SyntaxKind.AddLastKeyword:
		case SyntaxKind.AddBeforeKeyword:
		case SyntaxKind.AddAfterKeyword:
		case SyntaxKind.MoveFirstKeyword:
		case SyntaxKind.MoveLastKeyword:
		case SyntaxKind.MoveBeforeKeyword:
		case SyntaxKind.MoveAfterKeyword:
		case SyntaxKind.ModifyKeyword:
		case SyntaxKind.DataSetKeyword:
		case SyntaxKind.DataItemKeyword:
		case SyntaxKind.ColumnKeyword:
		case SyntaxKind.LabelsKeyword:
		case SyntaxKind.LabelKeyword:
		case SyntaxKind.RequestPageKeyword:
		case SyntaxKind.XmlPortSchemaKeyword:
		case SyntaxKind.XmlPortTableElementKeyword:
		case SyntaxKind.XmlPortFieldElementKeyword:
		case SyntaxKind.XmlPortTextElementKeyword:
		case SyntaxKind.XmlPortFieldAttributeKeyword:
		case SyntaxKind.XmlPortTextAttributeKeyword:
		case SyntaxKind.FilterKeyword:
		case SyntaxKind.QueryElementsKeyword:
		case SyntaxKind.EnumKeyword:
		case SyntaxKind.EnumExtensionKeyword:
		case SyntaxKind.EnumValueKeyword:
		case SyntaxKind.ViewsKeyword:
		case SyntaxKind.ViewKeyword:
		case SyntaxKind.OrderKeyword:
			return true;
		default:
			return false;
		}
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static SyntaxKind GetALKeywordKind(string text)
	{
		return text switch
		{
			"FALSE" => SyntaxKind.FalseKeyword, 
			"TRUE" => SyntaxKind.TrueKeyword, 
			"DIV" => SyntaxKind.IDivToken, 
			"MOD" => SyntaxKind.ModuloToken, 
			"AND" => SyntaxKind.AndKeyword, 
			"OR" => SyntaxKind.OrKeyword, 
			"XOR" => SyntaxKind.XorKeyword, 
			"NOT" => SyntaxKind.NotKeyword, 
			"EXIT" => SyntaxKind.ExitKeyword, 
			"BEGIN" => SyntaxKind.BeginKeyword, 
			"CASE" => SyntaxKind.CaseKeyword, 
			"DO" => SyntaxKind.DoKeyword, 
			"DOWNTO" => SyntaxKind.DownToKeyword, 
			"ELSE" => SyntaxKind.ElseKeyword, 
			"END" => SyntaxKind.EndKeyword, 
			"FOR" => SyntaxKind.ForKeyword, 
			"FOREACH" => SyntaxKind.ForEachKeyword, 
			"IF" => SyntaxKind.IfKeyword, 
			"IN" => SyntaxKind.InKeyword, 
			"OF" => SyntaxKind.OfKeyword, 
			"REPEAT" => SyntaxKind.RepeatKeyword, 
			"THEN" => SyntaxKind.ThenKeyword, 
			"TO" => SyntaxKind.ToKeyword, 
			"UNTIL" => SyntaxKind.UntilKeyword, 
			"WITH" => SyntaxKind.WithKeyword, 
			"WHILE" => SyntaxKind.WhileKeyword, 
			"ASSERTERROR" => SyntaxKind.AssertErrorKeyword, 
			"VAR" => SyntaxKind.VarKeyword, 
			"TRIGGER" => SyntaxKind.TriggerKeyword, 
			"PROCEDURE" => SyntaxKind.ProcedureKeyword, 
			"LOCAL" => SyntaxKind.LocalKeyword, 
			"BREAK" => SyntaxKind.BreakKeyword, 
			_ => SyntaxKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static SyntaxKind GetPropertyKeywordKind(string text)
	{
		SyntaxKind objectKeyword = GetObjectKeyword(text);
		if (objectKeyword != SyntaxKind.None)
		{
			return objectKeyword;
		}
		return text switch
		{
			"FALSE" => SyntaxKind.FalseKeyword, 
			"TRUE" => SyntaxKind.TrueKeyword, 
			"IF" => SyntaxKind.IfKeyword, 
			"ELSE" => SyntaxKind.ElseKeyword, 
			"WHERE" => SyntaxKind.WhereFormulaKeyword, 
			"FIELD" => SyntaxKind.FieldFormulaKeyword, 
			"CONST" => SyntaxKind.ConstFormulaKeyword, 
			"FILTER" => SyntaxKind.FilterFormulaKeyword, 
			"UPPERLIMIT" => SyntaxKind.UpperLimitFormulaKeyword, 
			"EXIST" => SyntaxKind.ExistCalculationFormulaKeyword, 
			"LOOKUP" => SyntaxKind.LookupCalculationFormulaKeyword, 
			"MIN" => SyntaxKind.MinCalculationFormulaKeyword, 
			"MAX" => SyntaxKind.MaxCalculationFormulaKeyword, 
			"AVERAGE" => SyntaxKind.AverageCalculationFormulaKeyword, 
			"SUM" => SyntaxKind.SumCalculationFormulaKeyword, 
			"COUNT" => SyntaxKind.CountCalculationFormulaKeyword, 
			"TABLEDATA" => SyntaxKind.TableDataKeyword, 
			"SYSTEM" => SyntaxKind.SystemKeyword, 
			"SORTING" => SyntaxKind.SortingKeyword, 
			"ORDER" => SyntaxKind.OrderKeyword, 
			"ASCENDING" => SyntaxKind.AscendingKeyword, 
			"DESCENDING" => SyntaxKind.DescendingKeyword, 
			_ => SyntaxKind.None, 
		};
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static SyntaxKind GetObjectKeywordKind(string text)
	{
		SyntaxKind objectKeyword = GetObjectKeyword(text);
		if (objectKeyword != SyntaxKind.None)
		{
			return objectKeyword;
		}
		return text switch
		{
			"EVENT" => SyntaxKind.EventKeyword, 
			"TEMPORARY" => SyntaxKind.TemporaryKeyword, 
			"TRIGGER" => SyntaxKind.TriggerKeyword, 
			"PROGRAM" => SyntaxKind.ProgramKeyword, 
			"PROCEDURE" => SyntaxKind.ProcedureKeyword, 
			"FUNCTION" => SyntaxKind.FunctionKeyword, 
			"VAR" => SyntaxKind.VarKeyword, 
			"ARRAY" => SyntaxKind.ArrayKeyword, 
			"OF" => SyntaxKind.OfKeyword, 
			"LOCAL" => SyntaxKind.LocalKeyword, 
			"SUPPRESSDISPOSE" => SyntaxKind.SuppressDisposeKeyword, 
			"SECURITYFILTERING" => SyntaxKind.SecurityFilteringKeyword, 
			"FIELDS" => SyntaxKind.FieldsKeyword, 
			"FIELD" => SyntaxKind.FieldKeyword, 
			"KEYS" => SyntaxKind.KeysKeyword, 
			"KEY" => SyntaxKind.KeyKeyword, 
			"FIELDGROUPS" => SyntaxKind.FieldGroupsKeyword, 
			"FIELDGROUP" => SyntaxKind.FieldGroupKeyword, 
			"EXTENDS" => SyntaxKind.ExtendsKeyword, 
			"ADDFIRST" => SyntaxKind.AddFirstKeyword, 
			"ADDLAST" => SyntaxKind.AddLastKeyword, 
			"ADDBEFORE" => SyntaxKind.AddBeforeKeyword, 
			"ADDAFTER" => SyntaxKind.AddAfterKeyword, 
			"MOVEFIRST" => SyntaxKind.MoveFirstKeyword, 
			"MOVELAST" => SyntaxKind.MoveLastKeyword, 
			"MOVEBEFORE" => SyntaxKind.MoveBeforeKeyword, 
			"MOVEAFTER" => SyntaxKind.MoveAfterKeyword, 
			"MODIFY" => SyntaxKind.ModifyKeyword, 
			"LAYOUT" => SyntaxKind.PageLayoutKeyword, 
			"AREA" => SyntaxKind.PageAreaKeyword, 
			"GROUP" => SyntaxKind.PageGroupKeyword, 
			"REPEATER" => SyntaxKind.PageRepeaterKeyword, 
			"CUEGROUP" => SyntaxKind.PageCueGroupKeyword, 
			"FIXED" => SyntaxKind.PageFixedKeyword, 
			"GRID" => SyntaxKind.PageGridKeyword, 
			"PART" => SyntaxKind.PagePartKeyword, 
			"SYSTEMPART" => SyntaxKind.PageSystemPartKeyword, 
			"CHARTPART" => SyntaxKind.PageChartPartKeyword, 
			"USERCONTROL" => SyntaxKind.PageUserControlKeyword, 
			"ACTIONS" => SyntaxKind.ActionsKeyword, 
			"ACTION" => SyntaxKind.ActionKeyword, 
			"SEPARATOR" => SyntaxKind.SeparatorKeyword, 
			"VIEWS" => SyntaxKind.ViewsKeyword, 
			"VIEW" => SyntaxKind.ViewKeyword, 
			"DATASET" => SyntaxKind.DataSetKeyword, 
			"DATAITEM" => SyntaxKind.DataItemKeyword, 
			"COLUMN" => SyntaxKind.ColumnKeyword, 
			"LABELS" => SyntaxKind.LabelsKeyword, 
			"LABEL" => SyntaxKind.LabelKeyword, 
			"REQUESTPAGE" => SyntaxKind.RequestPageKeyword, 
			"ELEMENTS" => SyntaxKind.QueryElementsKeyword, 
			"FILTER" => SyntaxKind.FilterKeyword, 
			"SCHEMA" => SyntaxKind.XmlPortSchemaKeyword, 
			"TABLEELEMENT" => SyntaxKind.XmlPortTableElementKeyword, 
			"FIELDELEMENT" => SyntaxKind.XmlPortFieldElementKeyword, 
			"TEXTELEMENT" => SyntaxKind.XmlPortTextElementKeyword, 
			"FIELDATTRIBUTE" => SyntaxKind.XmlPortFieldAttributeKeyword, 
			"TEXTATTRIBUTE" => SyntaxKind.XmlPortTextAttributeKeyword, 
			"CUSTOMIZES" => SyntaxKind.CustomizesKeyword, 
			"BEGIN" => SyntaxKind.BeginKeyword, 
			"END" => SyntaxKind.EndKeyword, 
			"FALSE" => SyntaxKind.FalseKeyword, 
			"TRUE" => SyntaxKind.TrueKeyword, 
			"ASSEMBLY" => SyntaxKind.AssemblyKeyword, 
			"TYPE" => SyntaxKind.TypeKeyword, 
			"ENUM" => SyntaxKind.EnumKeyword, 
			"VALUE" => SyntaxKind.EnumValueKeyword, 
			"ENUMEXTENSION" => SyntaxKind.EnumExtensionKeyword, 
			_ => SyntaxKind.None, 
		};
	}

	public static bool IsAllowedVariableName(this SyntaxKind kind)
	{
		if (kind != SyntaxKind.IdentifierToken)
		{
			if (kind.IsKeywordAllowedIdentifier())
			{
				return kind != SyntaxKind.LocalKeyword;
			}
			return false;
		}
		return true;
	}

	public static bool IsControlKeyword(this SyntaxKind kind)
	{
		if ((uint)(kind - 47) <= 29u || kind == SyntaxKind.BreakKeyword)
		{
			return true;
		}
		return false;
	}

	public static bool IsXmlPortNodeKeyword(this SyntaxKind kind)
	{
		if ((uint)(kind - 132) <= 4u)
		{
			return true;
		}
		return false;
	}

	public static bool IsXmlPortNode(this SyntaxKind kind)
	{
		if ((uint)(kind - 303) <= 4u)
		{
			return true;
		}
		return false;
	}

	public static bool IsOperatorKeyword(this SyntaxKind kind)
	{
		if ((uint)(kind - 13) <= 16u || (uint)(kind - 43) <= 3u)
		{
			return true;
		}
		return false;
	}

	public static bool IsLogicalOperator(this SyntaxKind kind)
	{
		if ((uint)(kind - 43) <= 3u)
		{
			return true;
		}
		return false;
	}

	public static bool IsMethodOrTriggerDeclaration(this SyntaxKind kind)
	{
		return kind.IsMethodOrTriggerDeclarationSyntax();
	}

	public static bool IsPropertyKeyword(this SyntaxKind kind)
	{
		if ((uint)(kind - 79) <= 1u || (uint)(kind - 146) <= 15u)
		{
			return true;
		}
		return false;
	}

	public static bool IsMetadataDefinitionKeyword(this SyntaxKind kind)
	{
		if ((uint)(kind - 90) <= 3u || (uint)(kind - 95) <= 17u || (uint)(kind - 144) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsBuiltinLiteral(this SyntaxKind kind)
	{
		if ((uint)(kind - 2) <= 1u)
		{
			return true;
		}
		return false;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	public static string GetText(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.RDivToken => "/", 
			SyntaxKind.PlusToken => "+", 
			SyntaxKind.MinusToken => "-", 
			SyntaxKind.MultiplyToken => "*", 
			SyntaxKind.IDivToken => "div", 
			SyntaxKind.ModuloToken => "mod", 
			SyntaxKind.AssignToken => ":=", 
			SyntaxKind.AssignRDivToken => "/=", 
			SyntaxKind.AssignPlusToken => "+=", 
			SyntaxKind.AssignMinusToken => "-=", 
			SyntaxKind.AssignMultiplyToken => "*=", 
			SyntaxKind.LessThanToken => "<", 
			SyntaxKind.LessThanEqualsToken => "<=", 
			SyntaxKind.NotEqualsToken => "<>", 
			SyntaxKind.EqualsToken => "=", 
			SyntaxKind.GreaterThanToken => ">", 
			SyntaxKind.GreaterThanEqualsToken => ">=", 
			SyntaxKind.CommaToken => ",", 
			SyntaxKind.DotToken => ".", 
			SyntaxKind.ColonToken => ":", 
			SyntaxKind.SemicolonToken => ";", 
			SyntaxKind.ColonColonToken => "::", 
			SyntaxKind.DotDotToken => "..", 
			SyntaxKind.AtToken => "@", 
			SyntaxKind.OpenParenToken => "(", 
			SyntaxKind.CloseParenToken => ")", 
			SyntaxKind.OpenBracketToken => "[", 
			SyntaxKind.CloseBracketToken => "]", 
			SyntaxKind.OpenBraceToken => "{", 
			SyntaxKind.CloseBraceToken => "}", 
			SyntaxKind.TrueKeyword => "true", 
			SyntaxKind.FalseKeyword => "false", 
			SyntaxKind.AndKeyword => "and", 
			SyntaxKind.OrKeyword => "or", 
			SyntaxKind.XorKeyword => "xor", 
			SyntaxKind.NotKeyword => "not", 
			SyntaxKind.ExitKeyword => "exit", 
			SyntaxKind.BeginKeyword => "begin", 
			SyntaxKind.CaseKeyword => "case", 
			SyntaxKind.DoKeyword => "do", 
			SyntaxKind.DownToKeyword => "downto", 
			SyntaxKind.ElseKeyword => "else", 
			SyntaxKind.EndKeyword => "end", 
			SyntaxKind.ForKeyword => "for", 
			SyntaxKind.IfKeyword => "if", 
			SyntaxKind.InKeyword => "in", 
			SyntaxKind.OfKeyword => "of", 
			SyntaxKind.RepeatKeyword => "repeat", 
			SyntaxKind.ThenKeyword => "then", 
			SyntaxKind.ToKeyword => "to", 
			SyntaxKind.UntilKeyword => "until", 
			SyntaxKind.WithKeyword => "with", 
			SyntaxKind.WhileKeyword => "while", 
			SyntaxKind.ProgramKeyword => "program", 
			SyntaxKind.ProcedureKeyword => "procedure", 
			SyntaxKind.FunctionKeyword => "function", 
			SyntaxKind.VarKeyword => "var", 
			SyntaxKind.ArrayKeyword => "array", 
			SyntaxKind.TemporaryKeyword => "temporary", 
			SyntaxKind.LocalKeyword => "local", 
			SyntaxKind.BreakKeyword => "break", 
			SyntaxKind.EventKeyword => "event", 
			SyntaxKind.AssertErrorKeyword => "asserterror", 
			SyntaxKind.SuppressDisposeKeyword => "suppressdispose", 
			SyntaxKind.SecurityFilteringKeyword => "securityfiltering", 
			SyntaxKind.ForEachKeyword => "foreach", 
			SyntaxKind.TriggerKeyword => "trigger", 
			SyntaxKind.CodeunitKeyword => "codeunit", 
			SyntaxKind.TableKeyword => "table", 
			SyntaxKind.TableDataKeyword => "tabledata", 
			SyntaxKind.SystemKeyword => "system", 
			SyntaxKind.QueryKeyword => "query", 
			SyntaxKind.PageKeyword => "page", 
			SyntaxKind.PageExtensionKeyword => "pageextension", 
			SyntaxKind.TableExtensionKeyword => "tableextension", 
			SyntaxKind.ReportKeyword => "report", 
			SyntaxKind.XmlPortKeyword => "xmlport", 
			SyntaxKind.ProfileKeyword => "profile", 
			SyntaxKind.ControlAddInKeyword => "controladdin", 
			SyntaxKind.PageCustomizationKeyword => "pagecustomization", 
			SyntaxKind.DotNetKeyword => "dotnet", 
			SyntaxKind.AssemblyKeyword => "assembly", 
			SyntaxKind.TypeKeyword => "type", 
			SyntaxKind.FieldsKeyword => "fields", 
			SyntaxKind.FieldKeyword => "field", 
			SyntaxKind.KeysKeyword => "keys", 
			SyntaxKind.KeyKeyword => "key", 
			SyntaxKind.FieldGroupsKeyword => "fieldgroups", 
			SyntaxKind.FieldGroupKeyword => "fieldgroup", 
			SyntaxKind.PageLayoutKeyword => "layout", 
			SyntaxKind.PageAreaKeyword => "area", 
			SyntaxKind.PageGroupKeyword => "group", 
			SyntaxKind.PageRepeaterKeyword => "repeater", 
			SyntaxKind.PageCueGroupKeyword => "cuegroup", 
			SyntaxKind.PageFixedKeyword => "fixed", 
			SyntaxKind.PageGridKeyword => "grid", 
			SyntaxKind.PagePartKeyword => "part", 
			SyntaxKind.PageSystemPartKeyword => "systempart", 
			SyntaxKind.PageChartPartKeyword => "chartpart", 
			SyntaxKind.PageUserControlKeyword => "usercontrol", 
			SyntaxKind.ActionsKeyword => "actions", 
			SyntaxKind.ActionKeyword => "action", 
			SyntaxKind.SeparatorKeyword => "separator", 
			SyntaxKind.ViewsKeyword => "views", 
			SyntaxKind.ViewKeyword => "view", 
			SyntaxKind.ExtendsKeyword => "extends", 
			SyntaxKind.AddFirstKeyword => "addfirst", 
			SyntaxKind.AddLastKeyword => "addlast", 
			SyntaxKind.AddBeforeKeyword => "addbefore", 
			SyntaxKind.AddAfterKeyword => "addafter", 
			SyntaxKind.MoveFirstKeyword => "movefirst", 
			SyntaxKind.MoveLastKeyword => "movelast", 
			SyntaxKind.MoveBeforeKeyword => "movebefore", 
			SyntaxKind.MoveAfterKeyword => "moveafter", 
			SyntaxKind.ModifyKeyword => "modify", 
			SyntaxKind.DataSetKeyword => "dataset", 
			SyntaxKind.DataItemKeyword => "dataitem", 
			SyntaxKind.ColumnKeyword => "column", 
			SyntaxKind.LabelsKeyword => "labels", 
			SyntaxKind.RequestPageKeyword => "requestpage", 
			SyntaxKind.LabelKeyword => "label", 
			SyntaxKind.XmlPortSchemaKeyword => "schema", 
			SyntaxKind.XmlPortTableElementKeyword => "tableelement", 
			SyntaxKind.XmlPortFieldElementKeyword => "fieldelement", 
			SyntaxKind.XmlPortTextElementKeyword => "textelement", 
			SyntaxKind.XmlPortFieldAttributeKeyword => "fieldattribute", 
			SyntaxKind.XmlPortTextAttributeKeyword => "textattribute", 
			SyntaxKind.QueryElementsKeyword => "elements", 
			SyntaxKind.FilterKeyword => "filter", 
			SyntaxKind.WhereFormulaKeyword => "where", 
			SyntaxKind.FieldFormulaKeyword => "field", 
			SyntaxKind.ConstFormulaKeyword => "const", 
			SyntaxKind.FilterFormulaKeyword => "filter", 
			SyntaxKind.UpperLimitFormulaKeyword => "upperlimit", 
			SyntaxKind.AverageCalculationFormulaKeyword => "average", 
			SyntaxKind.CountCalculationFormulaKeyword => "count", 
			SyntaxKind.ExistCalculationFormulaKeyword => "exist", 
			SyntaxKind.LookupCalculationFormulaKeyword => "lookup", 
			SyntaxKind.MinCalculationFormulaKeyword => "min", 
			SyntaxKind.MaxCalculationFormulaKeyword => "max", 
			SyntaxKind.SumCalculationFormulaKeyword => "sum", 
			SyntaxKind.AndFilterKeyword => "&", 
			SyntaxKind.OrFilterKeyword => "|", 
			SyntaxKind.SortingKeyword => "sorting", 
			SyntaxKind.OrderKeyword => "order", 
			SyntaxKind.AscendingKeyword => "ascending", 
			SyntaxKind.DescendingKeyword => "descending", 
			SyntaxKind.CustomizesKeyword => "customizes", 
			SyntaxKind.EnumKeyword => "enum", 
			SyntaxKind.EnumValueKeyword => "value", 
			SyntaxKind.EnumExtensionKeyword => "enumextension", 
			_ => string.Empty, 
		};
	}

	public static bool IsObject(this SyntaxKind kind)
	{
		if (kind.IsApplicationObject())
		{
			return true;
		}
		if (kind != SyntaxKind.ProfileObject && kind != SyntaxKind.ControlAddInObject)
		{
			return kind == SyntaxKind.RequestPage;
		}
		return true;
	}

	public static bool IsPageControl(this SyntaxKind kind)
	{
		if ((uint)(kind - 273) <= 1u || (uint)(kind - 276) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsFieldGroup(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.Key || kind == SyntaxKind.FieldGroup)
		{
			return true;
		}
		return false;
	}

	public static bool IsObjectWithMemberDeclarations(this SyntaxKind kind)
	{
		if (!kind.IsApplicationObject())
		{
			return kind == SyntaxKind.ControlAddInObject;
		}
		return true;
	}

	public static bool IsApplicationObject(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.EnumType:
		case SyntaxKind.EnumExtensionType:
		case SyntaxKind.CodeunitObject:
		case SyntaxKind.TableObject:
		case SyntaxKind.TableExtensionObject:
		case SyntaxKind.PageObject:
		case SyntaxKind.PageExtensionObject:
		case SyntaxKind.ReportObject:
		case SyntaxKind.XmlPortObject:
		case SyntaxKind.QueryObject:
		case SyntaxKind.PageCustomizationObject:
			return true;
		default:
			return false;
		}
	}

	public static bool IsApplicationObjectWithId(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.EnumType || kind == SyntaxKind.EnumExtensionType || (uint)(kind - 411) <= 7u)
		{
			return true;
		}
		return false;
	}

	public static bool IsExtensionObject(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.EnumExtensionType:
		case SyntaxKind.TableExtensionObject:
		case SyntaxKind.PageExtensionObject:
		case SyntaxKind.PageCustomizationObject:
			return true;
		default:
			return false;
		}
	}

	public static bool CanDefineTriggers(this SyntaxKind kind)
	{
		if (!kind.IsApplicationObject())
		{
			switch (kind)
			{
			case SyntaxKind.RequestPage:
				break;
			case SyntaxKind.Field:
			case SyntaxKind.FieldModification:
			case SyntaxKind.PageField:
			case SyntaxKind.PageUserControl:
			case SyntaxKind.PageAction:
			case SyntaxKind.ActionModifyChange:
			case SyntaxKind.ControlModifyChange:
			case SyntaxKind.ReportDataItem:
			case SyntaxKind.XmlPortTableElement:
			case SyntaxKind.XmlPortFieldElement:
			case SyntaxKind.XmlPortTextElement:
			case SyntaxKind.XmlPortFieldAttribute:
			case SyntaxKind.XmlPortTextAttribute:
				return true;
			default:
				return false;
			}
		}
		return true;
	}

	public static bool IsObjectKeyword(this SyntaxKind kind)
	{
		return SupportedApplicationObjectKeywordKinds.Contains(kind);
	}

	public static AreaKind GetAreaKind(IControlSymbol symbol)
	{
		return GetAreaKind(symbol.Name);
	}

	public static ActionAreaKind GetActionAreaKind(IControlSymbol symbol)
	{
		return GetActionAreaKind(symbol.Name);
	}

	internal static bool IsSignedOrSignInvariantLiteralToken(this SyntaxKind kind)
	{
		if ((uint)(kind - 2) <= 8u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPossibleSignedLiteralToken(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.MinusToken)
		{
			return true;
		}
		if (kind.IsSignedOrSignInvariantLiteralToken())
		{
			return true;
		}
		return false;
	}

	internal static bool IsKeywordAllowedIdentifier(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.ProgramKeyword:
		case SyntaxKind.FunctionKeyword:
		case SyntaxKind.ArrayKeyword:
		case SyntaxKind.TemporaryKeyword:
		case SyntaxKind.LocalKeyword:
		case SyntaxKind.SuppressDisposeKeyword:
		case SyntaxKind.SecurityFilteringKeyword:
		case SyntaxKind.CodeunitKeyword:
		case SyntaxKind.TableKeyword:
		case SyntaxKind.TableDataKeyword:
		case SyntaxKind.SystemKeyword:
		case SyntaxKind.PageKeyword:
		case SyntaxKind.ReportKeyword:
		case SyntaxKind.QueryKeyword:
		case SyntaxKind.XmlPortKeyword:
		case SyntaxKind.ControlAddInKeyword:
		case SyntaxKind.ProfileKeyword:
		case SyntaxKind.DotNetKeyword:
		case SyntaxKind.PageCustomizationKeyword:
		case SyntaxKind.CustomizesKeyword:
		case SyntaxKind.FieldsKeyword:
		case SyntaxKind.FieldKeyword:
		case SyntaxKind.AssemblyKeyword:
		case SyntaxKind.TypeKeyword:
		case SyntaxKind.BreakKeyword:
		case SyntaxKind.FieldGroupKeyword:
		case SyntaxKind.KeysKeyword:
		case SyntaxKind.PageLayoutKeyword:
		case SyntaxKind.PageGroupKeyword:
		case SyntaxKind.ActionKeyword:
		case SyntaxKind.SeparatorKeyword:
		case SyntaxKind.AddFirstKeyword:
		case SyntaxKind.AddLastKeyword:
		case SyntaxKind.AddBeforeKeyword:
		case SyntaxKind.AddAfterKeyword:
		case SyntaxKind.MoveFirstKeyword:
		case SyntaxKind.MoveLastKeyword:
		case SyntaxKind.MoveBeforeKeyword:
		case SyntaxKind.MoveAfterKeyword:
		case SyntaxKind.ModifyKeyword:
		case SyntaxKind.DataSetKeyword:
		case SyntaxKind.DataItemKeyword:
		case SyntaxKind.ColumnKeyword:
		case SyntaxKind.LabelsKeyword:
		case SyntaxKind.LabelKeyword:
		case SyntaxKind.RequestPageKeyword:
		case SyntaxKind.XmlPortSchemaKeyword:
		case SyntaxKind.FilterKeyword:
		case SyntaxKind.QueryElementsKeyword:
		case SyntaxKind.EnumKeyword:
		case SyntaxKind.EnumExtensionKeyword:
		case SyntaxKind.EnumValueKeyword:
		case SyntaxKind.ViewsKeyword:
		case SyntaxKind.ViewKeyword:
		case SyntaxKind.FieldFormulaKeyword:
		case SyntaxKind.CountCalculationFormulaKeyword:
		case SyntaxKind.SumCalculationFormulaKeyword:
		case SyntaxKind.AverageCalculationFormulaKeyword:
		case SyntaxKind.MinCalculationFormulaKeyword:
		case SyntaxKind.MaxCalculationFormulaKeyword:
		case SyntaxKind.OrderKeyword:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsRootObject(this SyntaxKind kind)
	{
		if (kind.IsApplicationObject())
		{
			return true;
		}
		if (kind != SyntaxKind.ProfileObject && kind != SyntaxKind.ControlAddInObject)
		{
			return kind == SyntaxKind.DotNetPackage;
		}
		return true;
	}

	internal static IEnumerable<TriggerTypeInfo> GetDeclaredTriggerTypeInfos(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.TableObject:
			return TriggerDefinitions.TableTriggers.Values;
		case SyntaxKind.TableExtensionObject:
			return TriggerDefinitions.TableExtensionTriggers.Values;
		case SyntaxKind.RequestPage:
		case SyntaxKind.PageObject:
			return TriggerDefinitions.PageTriggers.Values;
		case SyntaxKind.PageExtensionObject:
			return TriggerDefinitions.PageExtensionTriggers.Values;
		case SyntaxKind.PageCustomizationObject:
			return Enumerable.Empty<TriggerTypeInfo>();
		case SyntaxKind.CodeunitObject:
			return TriggerDefinitions.CodeunitTriggers.Values;
		case SyntaxKind.Field:
			return TriggerDefinitions.FieldTriggers.Values;
		case SyntaxKind.FieldModification:
			return TriggerDefinitions.FieldExtensionTriggers.Values;
		case SyntaxKind.PageAction:
			return TriggerDefinitions.ActionTriggers.Values;
		case SyntaxKind.ActionModifyChange:
			return TriggerDefinitions.ActionExtensionTriggers.Values;
		case SyntaxKind.PageField:
			return TriggerDefinitions.ControlTriggers.Values;
		case SyntaxKind.ControlModifyChange:
			return TriggerDefinitions.ControlExtensionTriggers.Values;
		case SyntaxKind.ReportObject:
			return TriggerDefinitions.ReportTriggers.Values;
		case SyntaxKind.ReportDataItem:
			return TriggerDefinitions.ReportDataItemTriggers.Values;
		case SyntaxKind.XmlPortObject:
			return TriggerDefinitions.XmlPortTriggers.Values;
		case SyntaxKind.XmlPortTableElement:
			return TriggerDefinitions.XmlPortTableElementTriggers.Values;
		case SyntaxKind.XmlPortFieldElement:
			return TriggerDefinitions.XmlPortFieldElementTriggers.Values;
		case SyntaxKind.XmlPortTextElement:
			return TriggerDefinitions.XmlPortTextElementTriggers.Values;
		case SyntaxKind.XmlPortFieldAttribute:
			return TriggerDefinitions.XmlPortFieldAttributeTriggers.Values;
		case SyntaxKind.XmlPortTextAttribute:
			return TriggerDefinitions.XmlPortTextAttributeTriggers.Values;
		default:
			return ImmutableArray<TriggerTypeInfo>.Empty;
		}
	}

	internal static bool IsAllowedVariableType(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.IdentifierToken:
		case SyntaxKind.SecurityFilteringKeyword:
		case SyntaxKind.CodeunitKeyword:
		case SyntaxKind.PageKeyword:
		case SyntaxKind.ReportKeyword:
		case SyntaxKind.QueryKeyword:
		case SyntaxKind.XmlPortKeyword:
		case SyntaxKind.ControlAddInKeyword:
		case SyntaxKind.DotNetKeyword:
		case SyntaxKind.ActionKeyword:
		case SyntaxKind.SeparatorKeyword:
		case SyntaxKind.LabelKeyword:
		case SyntaxKind.EnumKeyword:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsOptionType(InternalSyntaxToken type)
	{
		if (type.Kind == SyntaxKind.IdentifierToken)
		{
			return NavTypeExtensions.IsOptionType(type.ValueText);
		}
		return false;
	}

	internal static bool IsTextConstType(InternalSyntaxToken type)
	{
		if (type.Kind == SyntaxKind.IdentifierToken)
		{
			return NavTypeExtensions.IsTextConstType(type.ValueText);
		}
		return false;
	}

	internal static bool IsCalculationFormulaMethodKind(this SyntaxKind kind)
	{
		if ((uint)(kind - 151) <= 6u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsCalculationFormulaStatementKind(this SyntaxKind kind)
	{
		if ((uint)(kind - 355) <= 6u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPageControlOrGroupKeyword(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.FieldKeyword || (uint)(kind - 101) <= 8u || kind == SyntaxKind.LabelKeyword)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPageChangeKeyword(this SyntaxKind kind)
	{
		if (!kind.IsPageAddChangeKeyword() && !kind.IsPageMoveChangeKeyword())
		{
			return kind == SyntaxKind.ModifyKeyword;
		}
		return true;
	}

	internal static bool IsPageAddChangeKeyword(this SyntaxKind kind)
	{
		if ((uint)(kind - 116) <= 3u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPageMoveChangeKeyword(this SyntaxKind kind)
	{
		if ((uint)(kind - 120) <= 3u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsFieldGroupChangeKeyword(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.AddLastKeyword)
		{
			return true;
		}
		return false;
	}

	internal static ChangeKind ToChangeKind(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.AddFirstKeyword => ChangeKind.AddFirst, 
			SyntaxKind.AddLastKeyword => ChangeKind.AddLast, 
			SyntaxKind.AddBeforeKeyword => ChangeKind.AddBefore, 
			SyntaxKind.AddAfterKeyword => ChangeKind.AddAfter, 
			SyntaxKind.MoveFirstKeyword => ChangeKind.MoveFirst, 
			SyntaxKind.MoveLastKeyword => ChangeKind.MoveLast, 
			SyntaxKind.MoveBeforeKeyword => ChangeKind.MoveBefore, 
			SyntaxKind.MoveAfterKeyword => ChangeKind.MoveAfter, 
			SyntaxKind.ModifyKeyword => ChangeKind.Modify, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	internal static bool IsPageActionOrGroupKeyword(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.PageGroupKeyword || (uint)(kind - 111) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPageGroupKeyword(this SyntaxKind kind)
	{
		if ((uint)(kind - 101) <= 4u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsControlChange(this SyntaxKind kind)
	{
		if ((uint)(kind - 289) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsActionChange(this SyntaxKind kind)
	{
		if ((uint)(kind - 285) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsViewChange(this SyntaxKind kind)
	{
		if ((uint)(kind - 293) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsFieldModificationChange(this SyntaxKind kind)
	{
		return kind == SyntaxKind.FieldModification;
	}

	internal static bool IsMoveChange(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.ActionMoveChange || kind == SyntaxKind.ControlMoveChange || kind == SyntaxKind.ViewMoveChange)
		{
			return true;
		}
		return false;
	}

	internal static bool IsModifyChange(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.FieldModification:
		case SyntaxKind.ActionModifyChange:
		case SyntaxKind.ControlModifyChange:
		case SyntaxKind.ViewModifyChange:
			return true;
		default:
			return false;
		}
	}

	internal static ControlKind ToControlKind(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PageAreaKeyword => ControlKind.Area, 
			SyntaxKind.PageGroupKeyword => ControlKind.Group, 
			SyntaxKind.PageRepeaterKeyword => ControlKind.Repeater, 
			SyntaxKind.PageCueGroupKeyword => ControlKind.CueGroup, 
			SyntaxKind.PageFixedKeyword => ControlKind.Fixed, 
			SyntaxKind.PageGridKeyword => ControlKind.Grid, 
			SyntaxKind.PagePartKeyword => ControlKind.Part, 
			SyntaxKind.PageSystemPartKeyword => ControlKind.SystemPart, 
			SyntaxKind.PageChartPartKeyword => ControlKind.ChartPart, 
			SyntaxKind.LabelKeyword => ControlKind.Label, 
			SyntaxKind.FieldKeyword => ControlKind.Field, 
			SyntaxKind.PageUserControlKeyword => ControlKind.UserControl, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	internal static ActionKind ToActionKind(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PageAreaKeyword => ActionKind.Area, 
			SyntaxKind.PageGroupKeyword => ActionKind.Group, 
			SyntaxKind.ActionKeyword => ActionKind.Action, 
			SyntaxKind.SeparatorKeyword => ActionKind.Separator, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	internal static AreaKind GetAreaKind(string value)
	{
		return value.ToUpperInvariant() switch
		{
			"CONTENT" => AreaKind.Content, 
			"FACTBOXES" => AreaKind.FactBoxes, 
			"ROLECENTER" => AreaKind.RoleCenter, 
			_ => AreaKind.None, 
		};
	}

	internal static ActionAreaKind GetActionAreaKind(string value)
	{
		return value.ToUpperInvariant() switch
		{
			"PROCESSING" => ActionAreaKind.Processing, 
			"REPORTING" => ActionAreaKind.Reporting, 
			"NAVIGATION" => ActionAreaKind.Navigation, 
			"CREATION" => ActionAreaKind.Creation, 
			"EMBEDDING" => ActionAreaKind.Embedding, 
			"SECTIONS" => ActionAreaKind.Sections, 
			_ => ActionAreaKind.None, 
		};
	}

	internal static PageSystemPartKind GetSystemPartKind(string value)
	{
		if (Enum.TryParse<PageSystemPartKind>(value, ignoreCase: true, out var result))
		{
			return result;
		}
		return PageSystemPartKind.None;
	}

	internal static bool IsIdentifierMultilanguageComment(string valueText)
	{
		return valueText.Equals("Comment", StringComparison.OrdinalIgnoreCase);
	}

	internal static bool IsPossibleStatement(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.IdentifierToken:
		case SyntaxKind.SemicolonToken:
		case SyntaxKind.ExitKeyword:
		case SyntaxKind.BeginKeyword:
		case SyntaxKind.CaseKeyword:
		case SyntaxKind.ElseKeyword:
		case SyntaxKind.ForKeyword:
		case SyntaxKind.IfKeyword:
		case SyntaxKind.RepeatKeyword:
		case SyntaxKind.UntilKeyword:
		case SyntaxKind.WithKeyword:
		case SyntaxKind.WhileKeyword:
		case SyntaxKind.AssertErrorKeyword:
		case SyntaxKind.ForEachKeyword:
		case SyntaxKind.BreakKeyword:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsPossibleMember(this SyntaxKind kind)
	{
		if (!kind.IsPossibleProcedure())
		{
			return kind.IsPossibleTrigger();
		}
		return true;
	}

	internal static bool IsPossibleTrigger(this SyntaxKind kind)
	{
		return kind == SyntaxKind.TriggerKeyword;
	}

	internal static bool IsPossibleProcedure(this SyntaxKind kind)
	{
		if (kind != SyntaxKind.LocalKeyword)
		{
			return kind == SyntaxKind.ProcedureKeyword;
		}
		return true;
	}

	internal static bool IsPropertyContextSourceContext(this SyntaxKind kind)
	{
		if ((uint)(kind - 379) <= 2u || kind == SyntaxKind.TextExpressionPropertyValue || kind == SyntaxKind.StyleExpressionPropertyValue)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPropertyClientExpresion(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.ClientSideBooleanExpressionPropertyValue || kind == SyntaxKind.StyleExpressionPropertyValue)
		{
			return true;
		}
		return false;
	}

	private static SyntaxKind GetObjectKeyword(string text)
	{
		return text switch
		{
			"CODEUNIT" => SyntaxKind.CodeunitKeyword, 
			"PAGE" => SyntaxKind.PageKeyword, 
			"QUERY" => SyntaxKind.QueryKeyword, 
			"XMLPORT" => SyntaxKind.XmlPortKeyword, 
			"REPORT" => SyntaxKind.ReportKeyword, 
			"TABLE" => SyntaxKind.TableKeyword, 
			"TABLEEXTENSION" => SyntaxKind.TableExtensionKeyword, 
			"PAGEEXTENSION" => SyntaxKind.PageExtensionKeyword, 
			"PROFILE" => SyntaxKind.ProfileKeyword, 
			"PAGECUSTOMIZATION" => SyntaxKind.PageCustomizationKeyword, 
			"CONTROLADDIN" => SyntaxKind.ControlAddInKeyword, 
			"DOTNET" => SyntaxKind.DotNetKeyword, 
			"ENUM" => SyntaxKind.EnumKeyword, 
			"ENUMEXTENSION" => SyntaxKind.EnumExtensionKeyword, 
			_ => SyntaxKind.None, 
		};
	}
}
