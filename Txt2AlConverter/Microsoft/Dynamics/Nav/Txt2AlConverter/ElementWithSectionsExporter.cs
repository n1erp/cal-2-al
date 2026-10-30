using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.IO.Txt;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal abstract class ElementWithSectionsExporter : IElementExporter
{
	private readonly IElementExporter nextExporter;

	protected abstract IExporterLocator ExporterLocator { get; }

	protected ElementWithSectionsExporter(IElementExporter nextExporter)
	{
		this.nextExporter = nextExporter;
	}

	public virtual void Export(IElement element, IAlWriter writer)
	{
		ExportSections(element, writer);
		nextExporter.Export(element, writer);
	}

	protected virtual string GetSectionName(ElementRelationExport section)
	{
		return section.ContainerName.ToLowerInvariant();
	}

	private static bool IsValidElement(IElement element, ElementRelationExport s)
	{
		if (s.ParentType == element.ElementTypeInfo.ElementType && !string.IsNullOrEmpty(s.ContainerName))
		{
			if (s.ParentType == ElementType.XmlPort)
			{
				return s.ChildType != ElementType.XmlPortEvent;
			}
			return true;
		}
		return false;
	}

	private void ExportSections(IElement element, IAlWriter writer)
	{
		foreach (ElementRelationExport item in from s in ElementRelationExport.Definitions
			where IsValidElement(element, s)
			orderby s.ExportOrder
			select s)
		{
			ExportSection(writer, element, item);
		}
	}

	private void ExportSection(IAlWriter writer, IElement element, ElementRelationExport section)
	{
		bool hasChildren = element.GetSubElements(section.ChildType).Any();
		if (section.SkipIfEmpty && !hasChildren)
		{
			return;
		}
		if (writer.PendingEmptyLine)
		{
			writer.WriteLine();
		}
		writer.PendingEmptyLine = false;
		writer.WriteSection(GetSectionName(section), () =>
		{
			if (hasChildren)
			{
				ElementTreeNode elementTreeNode = ElementTreeNode.Create(element, section.ChildType);
				if (!IsElementPartListPage(element) || !elementTreeNode.Children.Any((ElementTreeNode x) => IsElementNotValidAction(x.Element)))
				{
					foreach (ElementTreeNode child in elementTreeNode.Children)
					{
						string elementKey = ExporterLocator.GetElementKey(child.Element);
						ExporterLocator.GetService<INestedElementExporter>(elementKey).Export(child, writer);
					}
					return;
				}
				CorrectListPartPageActions(elementTreeNode, writer);
			}
		});
		writer.PendingEmptyLine = true;
	}

	private void CorrectListPartPageActions(ElementTreeNode tree, IAlWriter writer)
	{
		if (tree.Children.Any((ElementTreeNode x) => IsElementActionContainer(x.Element)))
		{
			List<ElementTreeNode> childrenList = tree.Children.ToList();
			ElementTreeNode elementTreeNode = childrenList.First((ElementTreeNode x) => IsElementActionContainer(x.Element));
			int index = childrenList.IndexOf(elementTreeNode);
			IEnumerable<ElementTreeNode> collection = childrenList.Where((ElementTreeNode x) => IsElementNotValidAction(x.Element) && childrenList.IndexOf(x) < index);
			IEnumerable<ElementTreeNode> collection2 = childrenList.Where((ElementTreeNode x) => IsElementNotValidAction(x.Element) && childrenList.IndexOf(x) > index);
			List<ElementTreeNode> list = new List<ElementTreeNode>();
			list.AddRange(collection);
			list.AddRange(elementTreeNode.Children);
			list.AddRange(collection2);
			elementTreeNode = new ElementTreeNode(elementTreeNode.Element, list);
			string elementKey = ExporterLocator.GetElementKey(elementTreeNode.Element);
			ExporterLocator.GetService<INestedElementExporter>(elementKey).Export(elementTreeNode, writer);
			return;
		}
		writer.WriteLine("area(processing)");
		writer.WriteSection(() =>
		{
			foreach (ElementTreeNode child in tree.Children)
			{
				string elementKey2 = ExporterLocator.GetElementKey(child.Element);
				ExporterLocator.GetService<INestedElementExporter>(elementKey2).Export(child, writer);
			}
		});
	}

	private static bool IsElementNotValidAction(IElement element)
	{
		if (element.ElementTypeInfo.ElementType == ElementType.Action && element.TryGetStringProperty(PropertyType.ActionType, out var value))
		{
			return !string.Equals(value, "ActionContainer", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static bool IsElementActionContainer(IElement element)
	{
		if (element.ElementTypeInfo.ElementType == ElementType.Action && element.TryGetStringProperty(PropertyType.ActionType, out var value))
		{
			return string.Equals(value, "ActionContainer", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static bool IsElementPartListPage(IElement element)
	{
		if (element.ElementTypeInfo.ElementType == ElementType.Page && element.TryGetStringProperty(PropertyType.PageType, out var value))
		{
			return string.Equals(value, "ListPart", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}
}
