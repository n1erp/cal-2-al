using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Codeunit;

internal class CodeunitPropertiesConverter : PropertiesConverter
{
	protected override Dictionary<string, string> RenamedPropertyValueEnums { get; } = new Dictionary<string, string> { { "Static-Automatic", "StaticAutomatic" } };

	protected override string GetEscapedPropertyValue(IElement owner, PropertyTypeInfo propertyInfo, IAlWriterState state)
	{
		PropertyType propertyType = propertyInfo.PropertyType;
		string stringProperty = owner.GetStringProperty(propertyType);
		if (propertyType == PropertyType.EventSubscriberInstance && RenamedPropertyValueEnums.ContainsKey(stringProperty.UnquoteIdentifier()))
		{
			return RenamedPropertyValueEnums[stringProperty.UnquoteIdentifier()];
		}
		return base.GetEscapedPropertyValue(owner, propertyInfo, state);
	}
}
