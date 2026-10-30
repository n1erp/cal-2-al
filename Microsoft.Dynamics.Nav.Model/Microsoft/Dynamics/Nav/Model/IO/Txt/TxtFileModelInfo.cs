using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

public class TxtFileModelInfo
{
	private readonly Dictionary<ElementType, ElementRelationExportLookup> elementRelationExportLookups;

	private readonly Dictionary<ElementType, PropertyUsageLookup> propertyUsageLookups;

	private InlineTemplate[] inlineTemplates;

	[ThreadStatic]
	private static ElementRelationExportLookup lastRelationExportLookup;

	private static PropertyUsageLookup lastPropertyUsageLookup;

	private static readonly Lazy<TxtFileModelInfo> instance = new Lazy<TxtFileModelInfo>(isThreadSafe: true);

	public static TxtFileModelInfo Instance => instance.Value;

	public TxtFileModelInfo()
	{
		elementRelationExportLookups = new Dictionary<ElementType, ElementRelationExportLookup>(1024);
		propertyUsageLookups = new Dictionary<ElementType, PropertyUsageLookup>(1024);
		InitializeInlineTemplates();
	}

	public ElementRelationExportLookup GetScopedElementRelationLookup(IElement element)
	{
		ElementRelationExportLookup value = lastRelationExportLookup;
		if (value != null && value.ScopingElement.ElementType == element.ElementTypeInfo.ElementType)
		{
			return value;
		}
		lock (elementRelationExportLookups)
		{
			if (elementRelationExportLookups.TryGetValue(element.ElementTypeInfo.ElementType, out value))
			{
				lastRelationExportLookup = value;
				return value;
			}
		}
		value = new ElementRelationExportLookup(element.ElementTypeInfo, ElementRelationExport.Definitions.Where((ElementRelationExport ere) => ere.ParentType == element.ElementTypeInfo.ElementType), AlternateRelationLookupName);
		lock (elementRelationExportLookups)
		{
			lastRelationExportLookup = value;
			elementRelationExportLookups[element.ElementTypeInfo.ElementType] = value;
			return value;
		}
	}

	private string AlternateRelationLookupName(ElementRelationExport elementRelation)
	{
		if (!string.IsNullOrEmpty(elementRelation.ContainerName))
		{
			return elementRelation.ContainerName;
		}
		return EnumNameLookup<ElementType>.EnumToString(elementRelation.Child.ElementType);
	}

	public PropertyUsageLookup GetScopedPropertyUsageLookup(IElement element)
	{
		PropertyUsageLookup value = lastPropertyUsageLookup;
		if (value != null && value.ScopingElementType == element.ElementTypeInfo.ElementType)
		{
			return value;
		}
		lock (propertyUsageLookups)
		{
			if (propertyUsageLookups.TryGetValue(element.ElementTypeInfo.ElementType, out value))
			{
				lastPropertyUsageLookup = value;
				return value;
			}
		}
		value = new PropertyUsageLookup(element.ElementTypeInfo.ElementType, PropertyUsage.Definitions.Where((PropertyUsage pu) => pu.ElementType == element.ElementTypeInfo.ElementType));
		lock (propertyUsageLookups)
		{
			lastPropertyUsageLookup = value;
			propertyUsageLookups[element.ElementTypeInfo.ElementType] = value;
			return value;
		}
	}

	private void InitializeInlineTemplates()
	{
		int num = ElementTypeInfo.Lookup.Values.Select((ElementTypeInfo et) => (int)et.ElementType).Max();
		inlineTemplates = new InlineTemplate[num + 1];
		InlineTemplate[] definitions = InlineTemplate.Definitions;
		foreach (InlineTemplate inlineTemplate in definitions)
		{
			inlineTemplates[(int)inlineTemplate.ElementType] = inlineTemplate;
		}
	}

	internal InlineTemplate GetInlineTemplate(ElementType elementType)
	{
		if (elementType >= ElementType.Unknown && (int)elementType < inlineTemplates.Length)
		{
			return inlineTemplates[(int)elementType];
		}
		return null;
	}
}
