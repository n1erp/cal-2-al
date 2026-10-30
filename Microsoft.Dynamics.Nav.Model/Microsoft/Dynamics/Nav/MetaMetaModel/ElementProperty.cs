using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.MetaMetaModel;

[DebuggerDisplay("Element={element}, Property={property}")]
public class ElementProperty
{
	private ElementTypeInfo element;

	private PropertyTypeInfo property;

	public ElementTypeInfo Element => element;

	public PropertyTypeInfo Property => property;

	internal ElementProperty(ElementTypeInfo element, PropertyTypeInfo property)
	{
		this.element = element;
		this.property = property;
	}
}
