using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Query;

internal class QueryPropertiesConverter : PropertiesConverter
{
	protected override Dictionary<string, PropertyKind> RenamedProperties { get; } = new Dictionary<string, PropertyKind>
	{
		{
			"DataItemLinkType",
			PropertyKind.SqlJoinType
		},
		{
			"SQLJoinType",
			PropertyKind.SqlJoinType
		}
	};

	public override string GetPropertyValue(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, IAlWriterState state)
	{
		PropertyType propertyType = propertyInfo.PropertyType;
		string stringProperty = owner.GetStringProperty(propertyType);
		switch (propertyType)
		{
		case PropertyType.ColumnFilter:
		case PropertyType.DataItemTableFilter:
			return stringProperty;
		case PropertyType.OrderBy:
			return ConvertOrderByProperty(stringProperty);
		case PropertyType.DataItemLink:
			return ConvertDataItemLinkProperty(stringProperty);
		case PropertyType.DataItemLinkType:
			return GetMergedDataItemLinkType(owner);
		case PropertyType.SqlJoinType:
			return ConvertSqlJoinType(stringProperty);
		case PropertyType.DataItemTable:
		case PropertyType.DataSource:
			return AlUtilities.QuoteIdentifierIfNeeded(stringProperty);
		default:
			return base.GetPropertyValue(owner, propertyInfo, state);
		}
	}

	public override bool IsPropertyIgnored(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo)
	{
		switch (propertyInfo.PropertyType)
		{
		case PropertyType.DataItemTable:
		case PropertyType.QueryElementType:
		case PropertyType.DataSource:
		case PropertyType.SqlJoinType:
		case PropertyType.MethodType:
			return true;
		default:
			return base.IsPropertyIgnored(owner, propertyInfo);
		}
	}

	public override bool TryCommentOutProperty(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, out string commentPart)
	{
		commentPart = string.Empty;
		switch (propertyInfo.PropertyType)
		{
		case PropertyType.EntitySetName:
		case PropertyType.EntityName:
		case PropertyType.APIGroup:
		case PropertyType.APIPublisher:
			return PropertiesConverter.TryCommentOutPropertyOnElementOfTypeApi(owner, propertyInfo, ElementType.Query, out commentPart);
		default:
			return base.TryCommentOutProperty(owner, propertyInfo, out commentPart);
		}
	}

	private static string ConvertOrderByProperty(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return value;
		}
		RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;
		MatchCollection matchCollection = new Regex("(?<Column>([^,\"\\n]+?))=(?<Order>((Ascending)|(Descending)))", options).Matches(value);
		string[] array = new string[matchCollection.Count];
		for (int i = 0; i < matchCollection.Count; i++)
		{
			Match match = matchCollection[i];
			array[i] = match.Groups["Order"]?.ToString() + "(" + match.Groups["Column"]?.ToString() + ")";
		}
		return array.Join(",");
	}

	private static string ConvertDataItemLinkProperty(string value)
	{
		RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;
		return new Regex("\"=\"", options).Replace(value, "=");
	}

	private static string GetMergedDataItemLinkType(IElement element)
	{
		if (element.GetPropertySetState(PropertyType.SqlJoinType))
		{
			return ConvertSqlJoinType(element.GetStringProperty(PropertyType.SqlJoinType));
		}
		return ConvertDataItemLinkType(element.GetStringProperty(PropertyType.DataItemLinkType));
	}

	private static string ConvertDataItemLinkType(string value)
	{
		return value.UnquoteIdentifier() switch
		{
			"Use Default Values if No Match" => SqlJoinTypeKind.LeftOuterJoin.ToString(), 
			"Exclude Row If No Match" => SqlJoinTypeKind.InnerJoin.ToString(), 
			_ => SqlJoinTypeKind.LeftOuterJoin.ToString(), 
		};
	}

	private static string ConvertSqlJoinType(string value)
	{
		return value.UnquoteIdentifier() switch
		{
			"Left Outer Join" => SqlJoinTypeKind.LeftOuterJoin.ToString(), 
			"Inner Join" => SqlJoinTypeKind.InnerJoin.ToString(), 
			"Right Outer Join" => SqlJoinTypeKind.RightOuterJoin.ToString(), 
			"Full Outer Join" => SqlJoinTypeKind.FullOuterJoin.ToString(), 
			"Cross Join" => SqlJoinTypeKind.CrossJoin.ToString(), 
			_ => throw ExceptionUtilities.UnexpectedValue(value), 
		};
	}
}
