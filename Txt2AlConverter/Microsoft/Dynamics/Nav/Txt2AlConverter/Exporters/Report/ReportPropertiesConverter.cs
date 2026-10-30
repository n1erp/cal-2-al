using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Report;

internal class ReportPropertiesConverter : PropertiesConverter
{
	protected override Dictionary<string, PropertyKind> RenamedProperties { get; } = new Dictionary<string, PropertyKind>
	{
		{
			"ReqFilterFields",
			PropertyKind.RequestFilterFields
		},
		{
			"ReqFilterHeading",
			PropertyKind.RequestFilterHeading
		},
		{
			"ReqFilterHeadingML",
			PropertyKind.RequestFilterHeadingML
		},
		{
			"AutoFormatExpr",
			PropertyKind.AutoFormatExpression
		},
		{
			"Temporary",
			PropertyKind.UseTemporary
		},
		{
			"OptionString",
			PropertyKind.OptionMembers
		}
	};

	public override string GetPropertyValue(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, IAlWriterState state)
	{
		PropertyType propertyType = propertyInfo.PropertyType;
		string stringProperty = owner.GetStringProperty(propertyType);
		if (propertyType == PropertyType.DataItemTable || propertyType == PropertyType.DataItemLinkReference || propertyType == PropertyType.WordMergeDataItem)
		{
			return AlUtilities.QuoteIdentifierIfNeeded(stringProperty);
		}
		return base.GetPropertyValue(owner, propertyInfo, state);
	}

	public override bool IsPropertyIgnored(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo)
	{
		PropertyType propertyType = propertyInfo.PropertyType;
		if (propertyType == PropertyType.SourceExpression || propertyType == PropertyType.DataItemTable || propertyType == PropertyType.ReportDataItemDataType)
		{
			return true;
		}
		return base.IsPropertyIgnored(owner, propertyInfo);
	}

	public override bool TryCommentOutProperty(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo, out string commentPart)
	{
		commentPart = string.Empty;
		if (propertyInfo.PropertyType == PropertyType.DataItemTableView && PropertiesConverter.IsEmptyValue(owner, propertyInfo))
		{
			commentPart = $"//The property '{propertyInfo.PropertyType}' shouldn't have an empty value.";
			return true;
		}
		return base.TryCommentOutProperty(owner, propertyInfo, out commentPart);
	}
}
