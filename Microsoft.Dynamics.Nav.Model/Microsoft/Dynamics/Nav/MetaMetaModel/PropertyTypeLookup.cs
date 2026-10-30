using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaMetaModel;

public class PropertyTypeLookup : EnumLookup<PropertyType, PropertyTypeInfo>
{
	public PropertyTypeLookup(IEnumerable<PropertyTypeInfo> definitions)
		: base(definitions, (Func<PropertyTypeInfo, string>)((PropertyTypeInfo pt) => pt.Name), (Func<PropertyTypeInfo, PropertyType>)((PropertyTypeInfo pt) => pt.PropertyType))
	{
	}
}
