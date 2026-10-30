using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class ElementTreeNode
{
	public IElement Element { get; }

	public IEnumerable<ElementTreeNode> Children { get; }

	internal ElementTreeNode(IElement element, IEnumerable<ElementTreeNode> children)
	{
		Element = element;
		Children = children;
	}

	private ElementTreeNode(IElement element)
		: this(element, new List<ElementTreeNode>())
	{
	}

	public static ElementTreeNode Create(IElement element, ElementType childTypes, bool forceNesting = false)
	{
		IElement[] array = element.GetSubElements(childTypes).ToArray();
		if (array.Length == 0)
		{
			return new ElementTreeNode(element);
		}
		if (!forceNesting && element.ElementTypeInfo.ElementType == ElementType.Change)
		{
			return GetChangeTree(element);
		}
		int currentPosition = 0;
		return new ElementTreeNode(element, GetElementsForLevel(array, 0, ref currentPosition));
	}

	public static ElementTreeNode[] GetElementsForLevel(IElement[] elements, int currentLevel, ref int currentPosition)
	{
		List<ElementTreeNode> list = new List<ElementTreeNode>();
		while (currentPosition < elements.Length)
		{
			IElement element = elements[currentPosition];
			int indentation = AlUtilities.GetIndentation(element);
			if (indentation == currentLevel)
			{
				currentPosition++;
				ElementTreeNode[] elementsForLevel = GetElementsForLevel(elements, currentLevel + 1, ref currentPosition);
				if (elementsForLevel.Length != 0)
				{
					list.Add(new ElementTreeNode(element, elementsForLevel));
				}
				else
				{
					list.Add(new ElementTreeNode(element));
				}
			}
			else if (indentation <= currentLevel)
			{
				break;
			}
		}
		return list.ToArray();
	}

	private static ElementTreeNode GetChangeTree(IElement root)
	{
		IEnumerable<IElement> subElements = root.GetSubElements();
		return GetChangeTreeChild(root, subElements).resultingElement;
	}

	private static int? GetLevel(IElement element)
	{
		int? value = null;
		if (element == null || !element.TryGetInt32Property(PropertyType.Level, out value))
		{
			value = null;
		}
		if (element != null && element.ElementTypeInfo.ElementType == ElementType.Change)
		{
			value = 0;
		}
		return value;
	}

	private static (ElementTreeNode resultingElement, IEnumerable<IElement> remainingOfRest) GetChangeTreeChild(IElement element, IEnumerable<IElement> rest)
	{
		List<ElementTreeNode> list = new List<ElementTreeNode>();
		int? level = GetLevel(element);
		while (rest.Any() && level < GetLevel(rest.First()))
		{
			var (item, enumerable) = GetChangeTreeChild(rest.First(), rest.Skip(1));
			list.Add(item);
			rest = enumerable;
		}
		IEnumerable<ElementTreeNode> subs = from e in element.GetSubElements()
			select GetChangeTreeChild(e, new List<IElement>()).resultingElement;
		IEnumerable<ElementTreeNode> allChildrenRecursive = GetAllChildrenRecursive(list);
		list = ChildrenWithSubsWhichDontAppearInAllNestedChildren(list, subs, allChildrenRecursive);
		return (resultingElement: new ElementTreeNode(element, list), remainingOfRest: rest);
	}

	private static List<ElementTreeNode> ChildrenWithSubsWhichDontAppearInAllNestedChildren(List<ElementTreeNode> children, IEnumerable<ElementTreeNode> subs, IEnumerable<ElementTreeNode> allNestedChildren)
	{
		return children.Concat(subs.Where((ElementTreeNode s) => allNestedChildren.All((ElementTreeNode c) => c.Element != s.Element))).ToList();
	}

	private static IEnumerable<ElementTreeNode> GetAllChildrenRecursive(IEnumerable<ElementTreeNode> elements)
	{
		return elements.SelectMany((ElementTreeNode e) => GetAllChildrenRecursive(e.Children).Concat(new ElementTreeNode[1] { e }));
	}
}
