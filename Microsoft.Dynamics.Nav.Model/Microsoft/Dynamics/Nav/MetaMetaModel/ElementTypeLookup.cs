using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaMetaModel;

public class ElementTypeLookup : EnumLookup<ElementType, ElementTypeInfo>
{
	public ElementTypeLookup(IEnumerable<ElementTypeInfo> definitions)
		: base(definitions, (Func<ElementTypeInfo, string>)((ElementTypeInfo et) => et.Name), (Func<ElementTypeInfo, ElementType>)((ElementTypeInfo et) => et.ElementType))
	{
	}
}
