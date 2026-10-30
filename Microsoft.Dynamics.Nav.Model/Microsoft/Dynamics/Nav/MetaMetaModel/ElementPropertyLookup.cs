using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaMetaModel;

public class ElementPropertyLookup : EnumLookup<PropertyType, ElementProperty>
{
	private readonly ElementTypeInfo scopingElement;

	public ElementTypeInfo ScopingElement => scopingElement;

	public ElementPropertyLookup(ElementTypeInfo scopingElement, IEnumerable<ElementProperty> definitions)
		: this(scopingElement, definitions, (ElementProperty ep) => ep.Property.Name)
	{
	}

	public ElementPropertyLookup(ElementTypeInfo scopingElement, IEnumerable<ElementProperty> definitions, Func<ElementProperty, string> nameSelector)
		: base(definitions, (Func<ElementProperty, string>)((ElementProperty ep) => nameSelector(ep)), (Func<ElementProperty, PropertyType>)((ElementProperty ep) => ep.Property.PropertyType))
	{
		this.scopingElement = scopingElement ?? ElementTypeInfo.Lookup.GetValue(ElementType.Unknown);
	}
}
