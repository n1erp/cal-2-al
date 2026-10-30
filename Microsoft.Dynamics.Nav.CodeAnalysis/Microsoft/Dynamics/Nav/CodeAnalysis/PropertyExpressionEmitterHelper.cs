using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class PropertyExpressionEmitterHelper
{
	internal static void WriteTableRelationPropertyValue(XmlWriter writer, PropertySymbol propertySymbol)
	{
		BoundTableRelationExpression tableRelationExpression = ((BoundTableRelationPropertyValue)propertySymbol.Property.PropertyValue).TableRelationExpression;
		WriteTableRelation(writer, tableRelationExpression);
	}

	internal static void WriteTableFilters(XmlWriter writer, bool isCondition, string metadataElement, BoundTableRelationConditionExpression tableRelationExpression, string filterGroup)
	{
		if (tableRelationExpression == null)
		{
			return;
		}
		string filterTypeAttribute = "FilterType";
		string filterValueAttribute = "FilterValue";
		if (isCondition)
		{
			filterTypeAttribute = "ConditionType";
			filterValueAttribute = "ConditionValue";
		}
		foreach (BoundEqualsPropertyExpression equalsExpression in tableRelationExpression.Condition)
		{
			if (!string.IsNullOrEmpty(metadataElement))
			{
				ObjectMetadataEmitHelper.WriteStartEndElement(writer, metadataElement, () =>
				{
					WriteFilterAttributes(writer, equalsExpression, filterValueAttribute, filterTypeAttribute, filterGroup, isCondition);
				});
			}
			else
			{
				WriteFilterAttributes(writer, equalsExpression, filterValueAttribute, filterTypeAttribute, filterGroup, isCondition);
			}
		}
	}

	internal static void WriteCalculationFormulaPropertyValue(XmlWriter writer, PropertySymbol propertySymbol)
	{
		BoundCalculationFormulaPropertyValue value = (BoundCalculationFormulaPropertyValue)propertySymbol.Property.PropertyValue;
		ObjectMetadataEmitHelper.WriteStartEndElement(writer, "CalculationFormula", () =>
		{
			WriteFlowType(writer, value);
			writer.WriteAttributeString("ReverseSign", value.IsMinus ? "1" : "0");
			GetDefiningTableAndFieldInfo(value.FieldOrTableAccess, out var tableIdAsString, out var _, out var fieldIdAsString, out var _);
			writer.WriteAttributeString("Table", tableIdAsString);
			writer.WriteAttributeString("Field", fieldIdAsString);
			ObjectMetadataEmitHelper.WriteStartEndElement(writer, "TableFilters", () =>
			{
				WriteTableFilters(writer, isCondition: false, "Filter", value.TableFilter, "0");
			});
		});
	}

	private static void WriteFlowType(XmlWriter writer, BoundCalculationFormulaPropertyValue value)
	{
		switch (value.Operator)
		{
		default:
			throw ExceptionUtilities.UnexpectedValue(value.Operator);
		case SyntaxKind.SumCalculationFormulaStatement:
			writer.WriteAttributeString("FlowType", "Sum");
			break;
		case SyntaxKind.AverageCalculationFormulaStatement:
			writer.WriteAttributeString("FlowType", "Average");
			break;
		case SyntaxKind.MaxCalculationFormulaStatement:
			writer.WriteAttributeString("FlowType", "Max");
			break;
		case SyntaxKind.MinCalculationFormulaStatement:
			writer.WriteAttributeString("FlowType", "Min");
			break;
		case SyntaxKind.LookupCalculationFormulaStatement:
			writer.WriteAttributeString("FlowType", "Lookup");
			break;
		case SyntaxKind.ExistCalculationFormulaStatement:
			writer.WriteAttributeString("FlowType", "Exist");
			break;
		case SyntaxKind.CountCalculationFormulaStatement:
			writer.WriteAttributeString("FlowType", "Count");
			break;
		}
	}

	private static int? GetBoundOptionValueOrdinal(this BoundEqualsPropertyExpression expression)
	{
		if (expression.Syntax.Kind != SyntaxKind.ConstExpression)
		{
			return null;
		}
		BoundPropertyRhsExpression right = expression.Right;
		if (right.Value.Kind == BoundKind.NullPropertyValue)
		{
			SourceOptionSymbol obj = ((BoundFieldAccess)expression.Left).FieldSymbol.Type.GetMembers(" ").FirstOrDefault() as SourceOptionSymbol;
			DebugAssertHelper.Assert(obj != null);
			return obj.Ordinal;
		}
		BoundConstOrFieldAccessValueExpression obj2 = right.Value as BoundConstOrFieldAccessValueExpression;
		DebugAssertHelper.Assert(obj2 != null);
		BoundExpression value = obj2.Value;
		if (value is BoundOptionAccess boundOptionAccess)
		{
			DebugAssertHelper.Assert(boundOptionAccess.OptionSymbol != null);
			return boundOptionAccess.OptionSymbol.Ordinal;
		}
		if (value is BoundLiteral boundLiteral)
		{
			DebugAssertHelper.Assert(boundLiteral.ConstantValue != null);
			if (boundLiteral.Type.NavTypeKind == NavTypeKind.Integer)
			{
				return boundLiteral.ConstantValue.Int32Value;
			}
		}
		DebugAssertHelper.Fail("Const provided was not an Option or an Integer.");
		return null;
	}

	[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
	private static string GetConstantValueMetadata(ConstantValue value)
	{
		switch (value.Discriminator)
		{
		case ConstantValueTypeDiscriminator.Byte:
			return value.ByteValue.ToString(CultureInfo.InvariantCulture);
		case ConstantValueTypeDiscriminator.Char:
			return value.CharValue.ToString();
		case ConstantValueTypeDiscriminator.Int32:
			return value.Int32Value.ToString(CultureInfo.InvariantCulture);
		case ConstantValueTypeDiscriminator.Int64:
			return value.Int64Value.ToString(CultureInfo.InvariantCulture);
		case ConstantValueTypeDiscriminator.String:
			return value.StringValue;
		case ConstantValueTypeDiscriminator.Decimal:
			return value.DecimalValue.ToString(CultureInfo.InvariantCulture);
		case ConstantValueTypeDiscriminator.Duration:
			return value.DurationValue.ToString(CultureInfo.InvariantCulture);
		case ConstantValueTypeDiscriminator.Boolean:
			if (!value.BooleanValue)
			{
				return "0";
			}
			return "1";
		case ConstantValueTypeDiscriminator.DateTime:
			return value.DateTimeValue.ToString(DateTimeUtilities.DateTimeRoundTripFormatSpecifier, CultureInfo.InvariantCulture);
		case ConstantValueTypeDiscriminator.Time:
			return DateTimeUtilities.FormatCSideTime(value.TimeValue, DateTimeUtilities.DateTimeRoundTripFormatSpecifier);
		case ConstantValueTypeDiscriminator.Date:
			return DateTimeUtilities.FormatCSideDate(value.DateValue, DateTimeUtilities.DateTimeRoundTripFormatSpecifier);
		default:
			throw ExceptionUtilities.UnexpectedValue(value.Discriminator);
		}
	}

	private static void WriteTableRelation(XmlWriter writer, BoundTableRelationExpression tableRelationexpression)
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(writer, "TableRelations", () =>
		{
			GetDefiningTableAndFieldInfo(tableRelationexpression.FieldAccess, out var tableIdAsString, out var tableName, out var fieldIdAsString, out var _);
			writer.WriteAttributeString("TableID", tableIdAsString);
			writer.WriteAttributeString("TableName", tableName);
			writer.WriteAttributeString("FieldID", fieldIdAsString);
			if (tableRelationexpression.IfExpression != null)
			{
				WriteTableFilters(writer, isCondition: true, "Conditions", tableRelationexpression.IfExpression, "0");
			}
			if (tableRelationexpression.TableFilter != null)
			{
				WriteTableFilters(writer, isCondition: false, "Filters", tableRelationexpression.TableFilter, "0");
			}
		});
		if (tableRelationexpression.ElseExpression != null)
		{
			WriteTableRelation(writer, tableRelationexpression.ElseExpression);
		}
	}

	private static string GetConstConditionValue(BoundEqualsPropertyExpression expression)
	{
		if (expression.Syntax.Kind != SyntaxKind.ConstExpression)
		{
			return null;
		}
		BoundPropertyRhsExpression right = expression.Right;
		BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expression.Left;
		if (right.Value.Kind == BoundKind.NullPropertyValue && boundFieldAccess.Type.NavTypeKind != NavTypeKind.Option)
		{
			return "";
		}
		if (boundFieldAccess.Type.NavTypeKind == NavTypeKind.Option || boundFieldAccess.Type.NavTypeKind == NavTypeKind.Enum)
		{
			return expression.GetBoundOptionValueOrdinal()?.ToString(CultureInfo.InvariantCulture) ?? "";
		}
		BoundConstOrFieldAccessValueExpression obj = right.Value as BoundConstOrFieldAccessValueExpression;
		DebugAssertHelper.Assert(obj != null);
		BoundLiteral boundLiteral = obj.Value as BoundLiteral;
		DebugAssertHelper.Assert(boundLiteral != null);
		if (boundLiteral.ConstantValue.IsString)
		{
			return boundLiteral.ConstantValue.StringValue.UnquoteLiteral();
		}
		return GetConstantValueMetadata(boundLiteral.ConstantValue);
	}

	private static void WriteFilterAttributes(XmlWriter writer, BoundEqualsPropertyExpression equalsExpression, string filterValueAttribute, string filterTypeAttribute, string filterGroup, bool isCondition = false)
	{
		if (!isCondition)
		{
			writer.WriteAttributeString("FilterGroup", filterGroup);
		}
		FieldSymbol fieldSymbol = ((BoundFieldAccess)equalsExpression.Left).FieldSymbol;
		writer.WriteAttributeString("FieldID", fieldSymbol.Id.ToString(CultureInfo.InvariantCulture));
		switch (equalsExpression.Syntax.Kind)
		{
		default:
			throw ExceptionUtilities.UnexpectedValue(equalsExpression.Syntax.Kind);
		case SyntaxKind.FilterExpression:
			writer.WriteAttributeString(filterTypeAttribute, "FILTER");
			writer.WriteAttributeString(filterValueAttribute, FilterValueWriterVisitor.CreateFilterOutput(equalsExpression.Right));
			break;
		case SyntaxKind.ConstExpression:
			writer.WriteAttributeString(filterTypeAttribute, "CONST");
			writer.WriteAttributeString(filterValueAttribute, GetConstConditionValue(equalsExpression));
			break;
		case SyntaxKind.FieldFilterExpression:
		case SyntaxKind.SimpleFieldExpression:
		case SyntaxKind.FieldUpperLimitExpression:
		case SyntaxKind.FieldUpperLimitFilterExpression:
		{
			DebugAssertHelper.Assert(!isCondition, string.Concat("Calling an IF/ELSE expression for ", equalsExpression.Syntax.Kind, " is not supported. How come we have ended up here??"));
			writer.WriteAttributeString("FilterType", "FIELD");
			if (equalsExpression.Syntax.Kind == SyntaxKind.FieldUpperLimitFilterExpression || equalsExpression.Syntax.Kind == SyntaxKind.FieldUpperLimitExpression)
			{
				writer.WriteAttributeString("OnlyMaxLimit", "1");
			}
			if (equalsExpression.Syntax.Kind == SyntaxKind.FieldUpperLimitFilterExpression || equalsExpression.Syntax.Kind == SyntaxKind.FieldFilterExpression)
			{
				writer.WriteAttributeString("ValueIsFilter", "1");
			}
			int? boundFieldExpressionId = equalsExpression.GetBoundFieldExpressionId();
			if (boundFieldExpressionId.HasValue)
			{
				writer.WriteAttributeString(filterValueAttribute, boundFieldExpressionId.Value.ToString(CultureInfo.InvariantCulture));
			}
			break;
		}
		}
	}

	private static void GetDefiningTableAndFieldInfo(BoundExpression fieldOrTableAccess, out string tableIdAsString, out string tableName, out string fieldIdAsString, out string fieldName)
	{
		fieldIdAsString = "0";
		tableIdAsString = string.Empty;
		tableName = string.Empty;
		fieldName = string.Empty;
		ApplicationObjectTypeSymbol applicationObjectTypeSymbol = null;
		if (fieldOrTableAccess.Kind == BoundKind.FieldAccess)
		{
			FieldSymbol fieldSymbol = ((BoundFieldAccess)fieldOrTableAccess).FieldSymbol;
			fieldIdAsString = fieldSymbol.Id.ToString(CultureInfo.InvariantCulture);
			fieldName = fieldSymbol.Name;
			applicationObjectTypeSymbol = (ApplicationObjectTypeSymbol)fieldSymbol.ContainingSymbol;
		}
		else if (fieldOrTableAccess.Kind == BoundKind.ApplicationObjectAccess)
		{
			applicationObjectTypeSymbol = ((BoundApplicationObjectAccess)fieldOrTableAccess).ApplicationObjectTypeSymbol;
		}
		else if (fieldOrTableAccess.Kind == BoundKind.ClassType)
		{
			applicationObjectTypeSymbol = (ApplicationObjectTypeSymbol)((BoundClassType)fieldOrTableAccess).Type;
		}
		if (applicationObjectTypeSymbol.Kind == SymbolKind.TableExtension)
		{
			applicationObjectTypeSymbol = ((TableExtensionTypeSymbol)applicationObjectTypeSymbol).Target;
		}
		tableIdAsString = applicationObjectTypeSymbol.Id.ToString(CultureInfo.InvariantCulture);
		tableName = applicationObjectTypeSymbol.Name;
	}
}
