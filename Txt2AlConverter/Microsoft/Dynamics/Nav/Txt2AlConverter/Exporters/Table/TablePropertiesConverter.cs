using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Table;

internal class TablePropertiesConverter : PropertiesConverter
{
	private readonly AlExportOptions options;

	protected override Dictionary<string, PropertyKind> RenamedProperties { get; } = new Dictionary<string, PropertyKind>
	{
		{
			"AutoFormatExpr",
			PropertyKind.AutoFormatExpression
		},
		{
			"OptionString",
			PropertyKind.OptionMembers
		}
	};

	public TablePropertiesConverter(AlExportOptions options)
	{
		this.options = options;
	}

	public override void WriteMandatoryProperties(IElement owner, IAlWriter writer, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo[] declaredProperties)
	{
		if (owner.ElementTypeInfo.ElementType == ElementType.Field && writer.State.DataClassificationDefaulting && !declaredProperties.Any((Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo x) => x.PropertyType == PropertyType.DataClassification) && IsDataClassificationAllowed(owner))
		{
			writer.WriteProperty(PropertyKind.DataClassification.ToString(), DataClassificationKind.CustomerContent.ToString());
		}
	}

	public override string GetPropertyValue(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, IAlWriterState state)
	{
		PropertyType propertyType = propertyInfo.PropertyType;
		string stringProperty = owner.GetStringProperty(propertyType);
		switch (propertyType)
		{
		case PropertyType.DrillDownPageId:
		case PropertyType.LookupPageId:
			return GetReferencedPageId(stringProperty);
		case PropertyType.CalcFormula:
		case PropertyType.TableRelation:
			return PropertiesConverter.FormatMultiLineProperty(stringProperty, state);
		case PropertyType.MaxValue:
		case PropertyType.MinValue:
		case PropertyType.InitValue:
			return GetInitValue(owner, stringProperty);
		case PropertyType.ExtendedDataType:
		case PropertyType.BlobSubtype:
			return GetDataType(stringProperty);
		case PropertyType.DataType:
			if (!IsEnumDeclaration(owner))
			{
				return AlUtilities.ConvertTypeString(stringProperty, 0);
			}
			return ExtractEnumDeclaration(owner, state, stringProperty);
		case PropertyType.SqlIndex:
			return stringProperty;
		case PropertyType.ExternalType:
			return stringProperty.QuoteLiteral();
		case PropertyType.OptionOrdinalValues:
			return stringProperty.Replace(";", ",");
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
		switch (propertyInfo.PropertyType)
		{
		case PropertyType.FieldNo:
		case PropertyType.DataType:
		case PropertyType.Key:
		case PropertyType.Volatile:
		case PropertyType.AltSearchField:
		case PropertyType.Fields:
		case PropertyType.KeyGroups:
		case PropertyType.SiftLevelsToMaintain:
		case PropertyType.TableIdExpression:
		case PropertyType.Extensible:
		case PropertyType.EnumTypeId:
		case PropertyType.EnumTypeName:
			return true;
		case PropertyType.OptionString:
		case PropertyType.OptionCaptionML:
			return IsEnumDeclaration(owner);
		case PropertyType.DrillDownPageId:
		case PropertyType.LookupPageId:
		case PropertyType.AutoFormatExpression:
		case PropertyType.CaptionClass:
			if (options.TableDataOnly)
			{
				return true;
			}
			break;
		case PropertyType.DataClassification:
			if (owner.ElementTypeInfo.ElementType == ElementType.Field)
			{
				return !IsDataClassificationAllowed(owner);
			}
			break;
		}
		return base.IsPropertyIgnored(owner, propertyInfo);
	}

	private static bool IsEnumDeclaration(IElement owner)
	{
		if (owner.ElementTypeInfo.ElementType != ElementType.Field)
		{
			return false;
		}
		if (!owner.GetStringProperty(PropertyType.DataType).Equals("Option", StringComparison.Ordinal))
		{
			return false;
		}
		return AlUtilities.ToBooleanValue(owner.GetStringProperty(PropertyType.Extensible)) == true;
	}

	private string ExtractEnumDeclaration(IElement owner, IAlWriterState state, string value)
	{
		string stringProperty = owner.GetStringProperty(PropertyType.EnumTypeName);
		int? int32Property = owner.GetInt32Property(PropertyType.EnumTypeId);
		bool valueOrDefault = AlUtilities.ToBooleanValue(owner.GetStringProperty(PropertyType.Extensible)) == true;
		string stringProperty2 = owner.GetStringProperty(PropertyType.OptionString);
		string stringProperty3 = owner.GetStringProperty(PropertyType.OptionCaptionML);
		state.EnumExtractor.AddEnumDeclaration(int32Property.Value, stringProperty, valueOrDefault, stringProperty2, stringProperty3);
		return "enum " + AlUtilities.QuoteIdentifierIfNeeded(stringProperty);
	}

	public override bool TryCommentOutProperty(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, out string commentPart)
	{
		commentPart = string.Empty;
		switch (propertyInfo.PropertyType)
		{
		case PropertyType.ValidateTableRelation:
		{
			owner.TryGetStringProperty(PropertyType.TableRelation, out var value);
			if (string.IsNullOrEmpty(value))
			{
				commentPart = $"//The property '{propertyInfo.PropertyType}' can only be set if the property 'TableRelation' is set";
				return true;
			}
			return false;
		}
		case PropertyType.InitValue:
			if (owner.IsOfType("datetime"))
			{
				commentPart = "//The conversion of DateTime values is not supported. Please convert manually.";
				WarningExporter.DisplayConsoleWarning("The conversion of DateTime values is not supported. Please convert manually.");
				return true;
			}
			return false;
		case PropertyType.StandardDayTimeUnit:
			commentPart = "// The property Standard Dayt/Time Unit is not supported.";
			return true;
		case PropertyType.DecimalPlaces:
			if (!owner.IsOfType("decimal"))
			{
				commentPart = "//The DecimalPlaces property is only supported on fields of type Decimal.";
				return true;
			}
			return false;
		case PropertyType.BlankZero:
			if (!owner.IsOfAnyType("biginteger", "boolean", "integer", "decimal", "duration", "option"))
			{
				commentPart = "//The property BlankZero is only supported on fields of type BigInteger, Boolean, Integer, Decimal, Duration, Enum, and Option.";
				return true;
			}
			return false;
		default:
			return base.TryCommentOutProperty(owner, propertyInfo, out commentPart);
		}
	}

	private static string GetDataType(string value)
	{
		return value.Replace("-", "").Replace(" ", "").Replace(".", "");
	}

	private static string GetReferencedPageId(string value)
	{
		return AlUtilities.GetSourceId(value) ?? AlUtilities.QuoteIdentifierIfNeeded(value);
	}

	private static string GetInitValue(IElement owner, string value)
	{
		if (!owner.TryGetStringProperty(PropertyType.DataType, out var value2))
		{
			return value;
		}
		value2 = value2.ToLowerInvariant();
		switch (value2)
		{
		case "boolean":
			return PropertiesConverter.ConvertBooleanValue(value);
		case "option":
			return AlUtilities.QuoteIdentifierIfNeeded(value);
		case "decimal":
			return value.Replace(",", ".");
		case "time":
			return value.Replace(":", "");
		case "dateformula":
			return AlUtilities.SingleQuote(value);
		case "datetime":
			return value;
		default:
			if (value2.StartsWith("Text", StringComparison.OrdinalIgnoreCase) || value2.StartsWith("Code", StringComparison.OrdinalIgnoreCase))
			{
				return AlUtilities.QuoteStringLiteralIfNeeded(value, needsSingleQuoting: true);
			}
			return value;
		}
	}

	private static bool IsDataClassificationAllowed(IElement owner)
	{
		return GetFieldClass(owner) == FieldClass.Normal;
	}

	private static FieldClass GetFieldClass(IElement owner)
	{
		string stringProperty = owner.GetStringProperty(PropertyType.FieldClass);
		if (string.IsNullOrEmpty(stringProperty))
		{
			return FieldClass.Normal;
		}
		return (FieldClass)Enum.Parse(typeof(FieldClass), stringProperty);
	}
}
