using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

public class PropertyUsageLookup : EnumLookup<PropertyType, PropertyUsage>
{
	private readonly ElementType scopingElementType;

	public ElementType ScopingElementType => scopingElementType;

	public PropertyUsageLookup(ElementType scopingElementType, IEnumerable<PropertyUsage> definitions)
		: base(definitions, (Func<PropertyUsage, string>)((PropertyUsage pu) => EnumNameLookup<PropertyType>.EnumToString(pu.PropertyType)), (Func<PropertyUsage, PropertyType>)((PropertyUsage pu) => pu.PropertyType))
	{
		this.scopingElementType = scopingElementType;
	}
}
