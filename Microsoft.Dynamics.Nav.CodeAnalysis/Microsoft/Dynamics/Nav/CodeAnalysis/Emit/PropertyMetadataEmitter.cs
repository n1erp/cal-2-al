using System.Collections.Immutable;
using System.Globalization;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class PropertyMetadataEmitter
{
	public static void WritePermissionPropertyValue(this XmlWriter xmlWriter, BoundPermissionPropertyValue value)
	{
		PropertyTypeInfo propertyTypeInfo = value.GetPropertyTypeInfo();
		switch (propertyTypeInfo.Kind)
		{
		case PropertyKind.AccessByPermission:
			xmlWriter.WriteAccessByPermissionPropertyValue(value);
			break;
		case PropertyKind.Permissions:
			xmlWriter.WritePermissionElementPropertyValue(value);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(propertyTypeInfo.Kind);
		}
	}

	public static void WriteOrderByPropertyValue(this XmlWriter xmlWriter, BoundOrderByPropertyValue value)
	{
		foreach (BoundOrderByExpression expression in value.Elements)
		{
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "OrderBy", () =>
			{
				xmlWriter.WriteElementString("ColumnID", ((BoundQueryElementAccess)expression.Value).QueryElementSymbol.Id.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteElementString("SortingType", expression.IsAscending ? "Ascending" : "Descending");
			});
		}
	}

	public static void WriteQueryDataItemLinkPropertyValue(this XmlWriter xmlWriter, BoundDataItemLinkPropertyValue value)
	{
		foreach (BoundDataItemLinkExpression expression in value.Joins)
		{
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "DataItemLink", () =>
			{
				string name = ((BoundQueryDataItemAccess)expression.RelatedField.ReceiverOpt).QueryDataItemSymbol.Name;
				xmlWriter.WriteElementString("SourceDataItem", name);
				int id = expression.RelatedField.FieldSymbol.Id;
				xmlWriter.WriteElementString("SourceField", id.ToString(CultureInfo.InvariantCulture));
				int id2 = expression.SourceField.FieldSymbol.Id;
				xmlWriter.WriteElementString("DestinationField", id2.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteElementString("LinkOperator", "=");
			});
		}
	}

	public static void WriteReportDataItemLinkPropertyValue(this XmlWriter xmlWriter, BoundDataItemLinkPropertyValue value)
	{
		string[] array = new string[value.Joins.Length];
		for (int i = 0; i < value.Joins.Length; i++)
		{
			BoundDataItemLinkExpression boundDataItemLinkExpression = value.Joins[i];
			array[i] = string.Format(CultureInfo.InvariantCulture, "Field{0}=FIELD(Field{1})", boundDataItemLinkExpression.SourceField.FieldSymbol.Id, boundDataItemLinkExpression.RelatedField.FieldSymbol.Id);
		}
		string value2 = array.Join(",");
		xmlWriter.WriteElementString("DataItemLink", value2);
	}

	public static void WriteQueryFilterPropertyValue(this XmlWriter xmlWriter, BoundTableFilterStatementPropertyValue value, string filterElementName)
	{
		DebugAssertHelper.Assert(value.TableFilter.Condition != default(ImmutableArray<BoundEqualsPropertyExpression>));
		foreach (BoundEqualsPropertyExpression equalsExpression in value.TableFilter.Condition)
		{
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, filterElementName, () =>
			{
				switch (equalsExpression.Syntax.Kind)
				{
				case SyntaxKind.FilterExpression:
					xmlWriter.WriteElementString("FilterType", "FILTER");
					break;
				case SyntaxKind.ConstExpression:
					xmlWriter.WriteElementString("FilterType", "CONST");
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(equalsExpression.Syntax.Kind);
				}
				xmlWriter.WriteQueryFilterSourceId(equalsExpression.Left);
				string value2 = FilterValueWriterVisitor.CreateFilterOutput(equalsExpression.Right, emitBooleanAsNumeral: true);
				xmlWriter.WriteElementString("FilterValue", value2);
			});
		}
	}

	private static void WriteQueryFilterSourceId(this XmlWriter xmlWriter, BoundExpression leftOfEquals)
	{
		switch (leftOfEquals.Kind)
		{
		case BoundKind.QueryElementAccess:
			xmlWriter.WriteElementString("ColumnID", ((BoundQueryElementAccess)leftOfEquals).QueryElementSymbol.Id.ToString(CultureInfo.InvariantCulture));
			break;
		case BoundKind.FieldAccess:
			xmlWriter.WriteElementString("FieldNo", ((BoundFieldAccess)leftOfEquals).FieldSymbol.Id.ToString(CultureInfo.InvariantCulture));
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(leftOfEquals.Kind);
		}
	}

	private static void WriteAccessByPermissionPropertyValue(this XmlWriter xmlWriter, BoundPermissionPropertyValue permissionValue)
	{
		if (permissionValue.Value.Length == 1)
		{
			BoundPermissionExpression boundPermissionExpression = permissionValue.Value[0];
			DebugAssertHelper.Assert(boundPermissionExpression != null);
			xmlWriter.WriteAttributeString("AccessByObjectType", boundPermissionExpression.ObjectType.GetPermissionValueObjectTypeName());
			xmlWriter.WriteAttributeString("AccessByObjectID", boundPermissionExpression.GetPermissionObjectId().ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("AccessByPermissionMask", boundPermissionExpression.Permission.Int32Value.ToString(CultureInfo.InvariantCulture));
		}
	}

	private static void WritePermissionElementPropertyValue(this XmlWriter xmlWriter, BoundPermissionPropertyValue permissionValues)
	{
		_ = string.Empty;
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		try
		{
			foreach (BoundPermissionExpression item in permissionValues.Value)
			{
				instance.Add(item.GetPermissionString());
			}
			string value = instance.Join(",");
			xmlWriter.WriteElementString("Permissions", value);
		}
		finally
		{
			instance.Free();
		}
	}
}
