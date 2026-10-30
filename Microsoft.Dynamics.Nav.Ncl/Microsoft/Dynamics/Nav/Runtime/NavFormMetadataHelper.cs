using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavFormMetadataHelper
{
	private const int DictionaryLimit = 25;

	private readonly MasterPage masterPage;

	private List<ElementDefinition> controlDefinitions;

	private List<ElementDefinition> actionDefinitions;

	private List<ElementDefinition> uiPartDefinitions;

	private Dictionary<int, int> controlIndexById;

	private Dictionary<int, int> actionIndexById;

	internal List<ElementDefinition> ControlDefinitions
	{
		get
		{
			if (controlDefinitions == null)
			{
				controlDefinitions = masterPage.FindAll((ElementDefinition element) => element is ControlDefinition);
			}
			return controlDefinitions;
		}
	}

	internal List<ElementDefinition> ActionDefinitions
	{
		get
		{
			if (actionDefinitions == null)
			{
				actionDefinitions = masterPage.FindAll((ElementDefinition element) => element is ActionDefinition);
			}
			return actionDefinitions;
		}
	}

	public List<ElementDefinition> UiPartDefinitions => uiPartDefinitions ?? (uiPartDefinitions = masterPage.FindAll((ElementDefinition element) => element is InfopartPageDefinition));

	public NavFormMetadataHelper(MasterPage masterPage)
	{
		this.masterPage = masterPage;
	}

	public ControlDefinition GetControlDefinitionById(int controlId)
	{
		if (TryGetDefinitionById<ControlDefinition>(ControlDefinitions, ref controlIndexById, controlId, out var result))
		{
			return result;
		}
		throw new NavNCLControlMetadataNotFoundException(CultureInfo.CurrentCulture, masterPage.ID, controlId);
	}

	public bool TryGetControlDefinitionById(int controlId, out ControlDefinition result)
	{
		return TryGetDefinitionById<ControlDefinition>(ControlDefinitions, ref controlIndexById, controlId, out result);
	}

	public ActionDefinition GetActionDefinitionById(int actionId)
	{
		if (TryGetDefinitionById<ActionDefinition>(ActionDefinitions, ref actionIndexById, actionId, out var result))
		{
			return result;
		}
		throw new NavNCLActionMetadataNotFoundException(CultureInfo.CurrentCulture, masterPage.ID, actionId);
	}

	public ElementDefinition FindParentByControlId(int controlId)
	{
		Dictionary<ControlBaseDefinition, ElementDefinition> dictionary = new Dictionary<ControlBaseDefinition, ElementDefinition>();
		FindInHierarchyCore(masterPage.ContentArea, masterPage.ContentArea.Controls, dictionary, (ControlBaseDefinition element) => element.ID == controlId, SubnodeSelector);
		ControlBaseDefinition controlBaseDefinition = dictionary.Keys.FirstOrDefault((ControlBaseDefinition key) => key.ID == controlId);
		if (controlBaseDefinition != null && dictionary.TryGetValue(controlBaseDefinition, out var value))
		{
			return value;
		}
		throw new NavNCLControlMetadataNotFoundException(CultureInfo.CurrentCulture, masterPage.ID, controlId);
	}

	private static List<ControlBaseDefinition> SubnodeSelector(ElementDefinition node)
	{
		if (node is ControlGroupBaseDefinition controlGroupBaseDefinition)
		{
			return controlGroupBaseDefinition.Controls;
		}
		if (node is ControlContainerDefinition controlContainerDefinition)
		{
			return controlContainerDefinition.Controls;
		}
		if (node is AreaBaseDefinition areaBaseDefinition)
		{
			return areaBaseDefinition.Controls;
		}
		if (node is ControlContainerPlaceHolder controlContainerPlaceHolder)
		{
			return controlContainerPlaceHolder.Controls;
		}
		return new List<ControlBaseDefinition>();
	}

	private void FindInHierarchyCore(ElementDefinition listParent, IEnumerable<ControlBaseDefinition> list, Dictionary<ControlBaseDefinition, ElementDefinition> result, Predicate<ControlBaseDefinition> predicate, Func<ElementDefinition, List<ControlBaseDefinition>> subnodeSelector)
	{
		foreach (ControlBaseDefinition item in list)
		{
			if (predicate(item))
			{
				result.Add(item, listParent);
			}
			FindInHierarchyCore(item, subnodeSelector(item), result, predicate, subnodeSelector);
		}
	}

	private bool TryGetDefinitionById<T>(List<ElementDefinition> list, ref Dictionary<int, int> indexById, int definitionId, out T result) where T : IDElementDefinition
	{
		if (TryGetIndexById<T>(list, ref indexById, definitionId, out var definitionIndex))
		{
			result = (T)list[definitionIndex];
			return true;
		}
		result = null;
		return false;
	}

	private bool TryGetIndexById<T>(List<ElementDefinition> list, ref Dictionary<int, int> indexById, int definitionId, out int definitionIndex) where T : IDElementDefinition
	{
		if (list.Count < 25)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is T val && val.ID == definitionId)
				{
					definitionIndex = i;
					return true;
				}
			}
		}
		else
		{
			if (indexById == null)
			{
				Dictionary<int, int> dictionary = new Dictionary<int, int>(list.Count);
				int num = 0;
				foreach (ElementDefinition item in list)
				{
					if (item is T val2)
					{
						if (dictionary.ContainsKey(val2.ID))
						{
							throw new NavNSInvalidPageDefinitionException(string.Format(CultureInfo.CurrentCulture, Lang.DuplicateActionId, masterPage.ID.ToString(CultureInfo.CurrentCulture), masterPage.Caption, val2.ID.ToString(CultureInfo.CurrentCulture)));
						}
						dictionary.Add(val2.ID, num);
						num++;
					}
				}
				indexById = dictionary;
			}
			if (indexById.TryGetValue(definitionId, out definitionIndex))
			{
				return true;
			}
		}
		definitionIndex = -1;
		return false;
	}
}
