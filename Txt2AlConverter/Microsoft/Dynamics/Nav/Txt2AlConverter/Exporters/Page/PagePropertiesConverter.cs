using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Page;

internal class PagePropertiesConverter : PropertiesConverter
{
	protected override Dictionary<string, PropertyKind> RenamedProperties { get; } = new Dictionary<string, PropertyKind>
	{
		{
			"DataCaptionExpr",
			PropertyKind.DataCaptionExpression
		},
		{
			"IndentationColumnName",
			PropertyKind.IndentationColumn
		},
		{
			"AutoFormatExpr",
			PropertyKind.AutoFormatExpression
		},
		{
			"ProviderID",
			PropertyKind.Provider
		},
		{
			"FreezeColumnID",
			PropertyKind.FreezeColumn
		},
		{
			"Layout",
			PropertyKind.GridLayout
		}
	};

	public override string GetPropertyValue(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, IAlWriterState state)
	{
		PropertyType propertyType = propertyInfo.PropertyType;
		string stringProperty = owner.GetStringProperty(propertyType);
		switch (propertyType)
		{
		case PropertyType.Name:
		case PropertyType.SourceTable:
		case PropertyType.ChartPartId:
		case PropertyType.FreezeColumnId:
		case PropertyType.Image:
		case PropertyType.PromotedCategory:
			return AlUtilities.QuoteIdentifierIfNeeded(stringProperty);
		case PropertyType.TableRelation:
		case PropertyType.RunPageLink:
		case PropertyType.RunPageView:
		case PropertyType.SubpageLink:
			return PropertiesConverter.FormatMultiLineProperty(stringProperty, state);
		case PropertyType.PagePartId:
		case PropertyType.CardPageId:
		case PropertyType.DrillDownPageId:
		case PropertyType.LookupPageId:
			return AlUtilities.GetSourceId(stringProperty) ?? AlUtilities.QuoteIdentifierIfNeeded(stringProperty);
		case PropertyType.SystemPartId:
			return GetSystemPartValue(stringProperty);
		case PropertyType.Editable:
		case PropertyType.Enabled:
		case PropertyType.HideValue:
		case PropertyType.Visible:
		case PropertyType.QuickEntry:
		case PropertyType.ShowMandatory:
			return PropertiesConverter.ConvertValue(stringProperty);
		case PropertyType.ActionContainerType:
		{
			string text = AlUtilities.GetActionNameFromActionContainerType(stringProperty)?.ToLowerInvariant();
			if (text == null)
			{
				throw ExceptionUtilities.UnexpectedValue(text);
			}
			return text;
		}
		case PropertyType.ContainerType:
		{
			string text2 = AlUtilities.GetControlNameFromControlContainerType(owner.GetStringProperty(propertyType))?.ToLowerInvariant();
			if (text2 == null)
			{
				throw ExceptionUtilities.UnexpectedValue(text2);
			}
			return text2;
		}
		case PropertyType.ExtendedDataType:
		case PropertyType.BlobSubtype:
			return GetDataType(stringProperty);
		case PropertyType.ProviderId:
			return GetProviderId(owner, stringProperty);
		case PropertyType.ValuesAllowed:
			if (owner.IsOfAnyType("code", "text"))
			{
				return string.Join(",", from i in stringProperty.Split(',')
					select i.Trim().QuoteLiteral());
			}
			return stringProperty;
		default:
			return base.GetPropertyValue(owner, propertyInfo, state);
		}
	}

	public override bool IsPropertyIgnored(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo)
	{
		if (owner.IsUserControl())
		{
			return ObjectParser.LookupPageUserControlProperty(propertyInfo.Name) == null;
		}
		switch (propertyInfo.PropertyType)
		{
		case PropertyType.Name:
		case PropertyType.SourceExpression:
		case PropertyType.ActionContainerType:
		case PropertyType.ActionType:
		case PropertyType.SystemPartId:
		case PropertyType.PagePartId:
		case PropertyType.ContainerType:
		case PropertyType.ControlType:
		case PropertyType.TimerInterval:
		case PropertyType.TimerUpdate:
		case PropertyType.ChartPartId:
		case PropertyType.GroupType:
		case PropertyType.PartType:
			return true;
		case PropertyType.CaptionML:
			if (IsAreaElement(owner))
			{
				return true;
			}
			break;
		}
		return base.IsPropertyIgnored(owner, propertyInfo);
	}

	private static bool IsAreaElement(IElement owner)
	{
		ElementType elementType = owner.ElementTypeInfo.ElementType;
		if (elementType == ElementType.Control && owner.TryGetStringProperty(PropertyType.ControlType, out var value) && string.Equals(value, "container", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (elementType == ElementType.Action && owner.TryGetStringProperty(PropertyType.ActionType, out value) && string.Equals(value, "actioncontainer", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	public override bool TryCommentOutProperty(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, out string commentPart)
	{
		commentPart = string.Empty;
		switch (propertyInfo.PropertyType)
		{
		case PropertyType.Layout:
		{
			if (!owner.TryGetStringProperty(PropertyType.GroupType, out var value2) || (!value2.Equals("gridlayout", StringComparison.OrdinalIgnoreCase) && !value2.Equals("cuegroup", StringComparison.OrdinalIgnoreCase)))
			{
				commentPart = "//The GridLayout property is only supported on controls of type Grid";
				return true;
			}
			return false;
		}
		case PropertyType.PromotedIsBig:
		case PropertyType.PromotedCategory:
		case PropertyType.PromotedOnly:
		{
			if (!owner.TryGetStringProperty(PropertyType.Promoted, out var value) || AlUtilities.ToBooleanValue(value) != true)
			{
				commentPart = $"//The property '{propertyInfo.PropertyType}' can only be set if the property 'Promoted' is set to 'true'";
				return true;
			}
			return false;
		}
		case PropertyType.EntitySetName:
		case PropertyType.EntityName:
		case PropertyType.APIGroup:
		case PropertyType.APIPublisher:
			return PropertiesConverter.TryCommentOutPropertyOnElementOfTypeApi(owner, propertyInfo, ElementType.Page, out commentPart);
		case PropertyType.CaptionML:
		case PropertyType.ToolTipML:
			if (string.IsNullOrEmpty(owner.GetStringProperty(propertyInfo.PropertyType)))
			{
				commentPart = "//The property '" + GetPropertyName(owner, propertyInfo) + "' cannot be empty.";
				return true;
			}
			return false;
		case PropertyType.DecimalPlaces:
			if (!owner.IsOfUnknownType() && !owner.IsOfType("decimal"))
			{
				commentPart = "//The DecimalPlaces property is only supported on fields of type Decimal.";
				return true;
			}
			return false;
		case PropertyType.BlankZero:
			if (!owner.IsOfUnknownType() && !owner.IsOfAnyType("biginteger", "boolean", "integer", "decimal", "duration", "option"))
			{
				commentPart = "//The property BlankZero is only supported on fields of type BigInteger, Boolean, Integer, Decimal, Duration, Enum, and Option.";
				return true;
			}
			return false;
		default:
			return base.TryCommentOutProperty(owner, propertyInfo, out commentPart);
		}
	}

	private string GetSystemPartValue(string value)
	{
		if (value.Equals("RecordLinks", StringComparison.OrdinalIgnoreCase))
		{
			return "Links";
		}
		return value;
	}

	private static string GetProviderId(IElement owner, string value)
	{
		if (!int.TryParse(value, out var controlId))
		{
			return AlUtilities.QuoteIdentifierIfNeeded(value);
		}
		IElement element = AlUtilities.QueryFullElementTree(owner, ElementType.Control, (IElement x) => x.Id == controlId).FirstOrDefault();
		if (element == null)
		{
			return AlUtilities.QuoteIdentifierIfNeeded(value);
		}
		return AlUtilities.QuoteIdentifierIfNeeded(AlUtilities.GetElementName(element));
	}

	private static string GetDataType(string value)
	{
		return value.Replace("-", "").Replace(" ", "").Replace(".", "");
	}
}
