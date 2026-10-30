using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Page;
using Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Table;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class ModificationPropertyConverter : PropertiesConverter
{
	private readonly PropertiesConverter pageConverter;

	private readonly PropertiesConverter tableConverter;

	public ModificationPropertyConverter(AlExportOptions options)
	{
		pageConverter = new PagePropertiesConverter();
		tableConverter = new TablePropertiesConverter(options);
	}

	public override string GetPropertyValue(IElement owner, PropertyTypeInfo propertyInfo, IAlWriterState state)
	{
		if (owner.Parent.GetPropertySetState(PropertyType.TargetPageType))
		{
			return pageConverter.GetPropertyValue(owner, propertyInfo, state);
		}
		return tableConverter.GetPropertyValue(owner, propertyInfo, state);
	}
}
