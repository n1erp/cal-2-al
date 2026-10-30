using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

public class ElementRelationExportLookup : EnumLookup<ElementType, ElementRelationExport>
{
	private readonly ElementTypeInfo scopingElement;

	public ElementTypeInfo ScopingElement => scopingElement;

	public ElementRelationExportLookup(ElementTypeInfo scopingElement, IEnumerable<ElementRelationExport> definitions)
		: this(scopingElement, definitions, (ElementRelationExport er) => er.ContainerName)
	{
		this.scopingElement = scopingElement ?? ElementTypeInfo.Lookup.GetValue(ElementType.Unknown);
	}

	public ElementRelationExportLookup(ElementTypeInfo scopingElement, IEnumerable<ElementRelationExport> definitions, Func<ElementRelationExport, string> nameSelector)
		: base(definitions, (Func<ElementRelationExport, string>)((ElementRelationExport er) => nameSelector(er)), (Func<ElementRelationExport, ElementType>)((ElementRelationExport er) => er.ChildType))
	{
		this.scopingElement = scopingElement ?? ElementTypeInfo.Lookup.GetValue(ElementType.Unknown);
	}
}
