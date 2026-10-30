using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class FlatCollectionInsertionExporter : IElementExporter
{
	private readonly INestedElementExporter subElementExporter;

	private readonly ElementType targetElementType;

	internal FlatCollectionInsertionExporter(INestedElementExporter subElementExporter, ElementType targetElementType)
	{
		this.subElementExporter = subElementExporter ?? throw new ArgumentNullException("subElementExporter");
		this.targetElementType = targetElementType;
	}

	public void Export(IElement element, IAlWriter writer)
	{
		ExportSection(element, writer, subElementExporter, targetElementType);
	}

	private static void ExportSection(IElement element, IAlWriter writer, INestedElementExporter subElementExporter, ElementType targetElementType)
	{
		ElementTreeNode changeTree = ElementTreeNode.Create(element, targetElementType);
		if (!changeTree.Children.Any())
		{
			return;
		}
		if (targetElementType == ElementType.Field || targetElementType == ElementType.Key)
		{
			ExportSubElements(changeTree.Children, subElementExporter, writer);
			return;
		}
		if (IsFirstChildAnotherContainer(changeTree))
		{
			ExportToAnotherArea(writer, subElementExporter, changeTree);
			return;
		}
		string changeTypeSectionHeader = GetChangeTypeSectionHeader(PropertyType.InsertAfter, element);
		writer.WriteSection(changeTypeSectionHeader, () =>
		{
			ExportSubElements(changeTree.Children, subElementExporter, writer);
		});
	}

	private static bool IsFirstChildAnotherContainer(ElementTreeNode changeTree)
	{
		IElement element = changeTree.Children.FirstOrDefault()?.Element;
		if (element.GetPropertySetState(PropertyType.ControlType))
		{
			return element.GetStringProperty(PropertyType.ControlType) == "Container";
		}
		if (element.GetPropertySetState(PropertyType.ActionType))
		{
			return element.GetStringProperty(PropertyType.ActionType) == "ActionContainer";
		}
		return false;
	}

	private static void ExportToAnotherArea(IAlWriter writer, INestedElementExporter subElementExporter, ElementTreeNode changeTree)
	{
		IElement element = changeTree.Children.First().Element;
		string text = ((!element.GetPropertySetState(PropertyType.ContainerType)) ? AlUtilities.GetActionNameFromActionContainerType(element.GetStringProperty(PropertyType.ActionContainerType)) : AlUtilities.GetControlNameFromControlContainerType(element.GetStringProperty(PropertyType.ContainerType)));
		string sectionName = "addfirst(" + text.ToLowerInvariant() + ")";
		writer.WriteSection(sectionName, () =>
		{
			ExportSubElements(changeTree.Children.Skip(1), subElementExporter, writer);
		});
	}

	private static void ExportSubElements(IEnumerable<ElementTreeNode> subElements, INestedElementExporter subElementExporter, IAlWriter writer)
	{
		subElements.ForEach((ElementTreeNode child) =>
		{
			subElementExporter.Export(child, writer);
		});
	}

	internal static string GetChangeTypeSectionHeader(PropertyType type, IElement element)
	{
		if (type == PropertyType.InsertAfter)
		{
			string aLNameFromElementIdentifier = ModificationContentExporter.GetALNameFromElementIdentifier(element.GetStringProperty(PropertyType.InsertAfter));
			SyntaxKind kind;
			if (aLNameFromElementIdentifier.StartsWith("AddAfter:", StringComparison.OrdinalIgnoreCase))
			{
				kind = SyntaxKind.AddAfterKeyword;
				aLNameFromElementIdentifier = aLNameFromElementIdentifier.Substring("AddAfter:".Length);
				aLNameFromElementIdentifier = CheckAndRenameForAreaName(aLNameFromElementIdentifier);
			}
			else
			{
				if (!aLNameFromElementIdentifier.StartsWith("AddFirst:", StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidOperationException("The delta file was not generated with ExportToNewSyntax flag.");
				}
				kind = SyntaxKind.AddFirstKeyword;
				aLNameFromElementIdentifier = aLNameFromElementIdentifier.Substring("AddFirst:".Length);
				aLNameFromElementIdentifier = CheckAndRenameForAreaName(aLNameFromElementIdentifier);
			}
			aLNameFromElementIdentifier = ModificationContentExporter.WrapInQuotesIfNeeded(aLNameFromElementIdentifier);
			return kind.GetText() + "(" + aLNameFromElementIdentifier + ")";
		}
		return string.Empty;
	}

	private static string CheckAndRenameForAreaName(string targetName)
	{
		return AlUtilities.GetActionNameFromActionContainerType(targetName) ?? AlUtilities.GetControlNameFromControlContainerType(targetName) ?? targetName;
	}
}
