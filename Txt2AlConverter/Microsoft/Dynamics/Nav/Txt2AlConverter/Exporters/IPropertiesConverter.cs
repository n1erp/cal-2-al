using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters;

internal interface IPropertiesConverter
{
	bool TryCommentOutProperty(IElement owner, PropertyTypeInfo propertyInfo, out string commentPart);

	bool IsPropertyIgnored(IElement owner, PropertyTypeInfo propertyInfo);

	string GetPropertyName(IElement owner, PropertyTypeInfo propertyInfo);

	string GetPropertyValue(IElement owner, PropertyTypeInfo propertyInfo, IAlWriterState state);

	void WriteMandatoryProperties(IElement owner, IAlWriter write, PropertyTypeInfo[] declaredProperties);
}
