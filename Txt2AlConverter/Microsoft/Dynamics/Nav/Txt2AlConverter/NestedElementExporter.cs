using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal sealed class NestedElementExporter : INestedElementExporter
{
	public IElementExporter PreChildrenParentExporter { get; set; }

	public IElementExporter PostChildrenParentExporter { get; set; }

	public IElementExporter HeaderExporter { get; set; }

	private IExporterLocator ExporterLocator { get; }

	public NestedElementExporter(IExporterLocator exporterLocator)
	{
		ExporterLocator = exporterLocator;
	}

	public void Export(ElementTreeNode elementTreeNode, IAlWriter writer)
	{
		HeaderExporter?.Export(elementTreeNode.Element, writer);
		writer.WriteSection(() =>
		{
			PreChildrenParentExporter?.Export(elementTreeNode.Element, writer);
			writer.PendingEmptyLine = false;
			foreach (ElementTreeNode child in elementTreeNode.Children)
			{
				IElement element = child.Element;
				if (!IsTrigger(element))
				{
					GetElementExporter(element).Export(child, writer);
				}
			}
			writer.PendingEmptyLine = PreChildrenParentExporter != null || !elementTreeNode.Children.IsEmpty();
			if (!IsControlWithoutNestedActionsOrTriggers(elementTreeNode.Element))
			{
				PostChildrenParentExporter?.Export(elementTreeNode.Element, writer);
			}
		});
	}

	private static bool IsTrigger(IElement element)
	{
		return element.ElementTypeInfo.Category == CategoryType.Trigger;
	}

	private static bool IsControlWithoutNestedActionsOrTriggers(IElement element)
	{
		if (element.IsUserControl())
		{
			return false;
		}
		if ((from x in element.GetSubElements()
			where x.ElementTypeInfo.ElementType != ElementType.Action
			select x).Any())
		{
			return false;
		}
		element.TryGetStringProperty(PropertyType.GroupType, out var value);
		IEnumerable<IElement> source = from x in element.GetSubElements()
			where x.ElementTypeInfo.ElementType == ElementType.Action
			select x;
		if (!string.Equals(value, "CueGroup") || !source.Any())
		{
			return true;
		}
		return false;
	}

	private INestedElementExporter GetElementExporter(IElement child)
	{
		string elementKey = ExporterLocator.GetElementKey(child);
		return ExporterLocator.GetService<INestedElementExporter>(elementKey);
	}
}
