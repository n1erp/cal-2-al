using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaMetaModel;

public class ElementRelationLookup : EnumLookup<ElementType, ElementRelation>
{
	private readonly ElementTypeInfo scopingElement;

	public ElementTypeInfo ScopingElement => scopingElement;

	public ElementRelationLookup(ElementTypeInfo scopingElement, IEnumerable<ElementRelation> definitions)
		: this(scopingElement, definitions, (ElementRelation er) => er.Name)
	{
		this.scopingElement = scopingElement ?? ElementTypeInfo.Lookup.GetValue(ElementType.Unknown);
	}

	public ElementRelationLookup(ElementTypeInfo scopingElement, IEnumerable<ElementRelation> definitions, Func<ElementRelation, string> nameSelector)
		: base(definitions, (Func<ElementRelation, string>)((ElementRelation er) => nameSelector(er)), (Func<ElementRelation, ElementType>)((ElementRelation er) => er.ChildType))
	{
		this.scopingElement = scopingElement ?? ElementTypeInfo.Lookup.GetValue(ElementType.Unknown);
	}
}
