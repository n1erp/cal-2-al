using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public static class MasterPageMergeHelper
{
	public static MasterPage MergeMasterPageAndPage(MasterPage masterPage, PageDefinition pageDefinition)
	{
		if (masterPage.CommandBar != null)
		{
			Dictionary<ActionContainerPlaceHolder, List<ActionBaseDefinition>> dictionary = new Dictionary<ActionContainerPlaceHolder, List<ActionBaseDefinition>>();
			if (MergeActions(masterPage.CommandBar.Actions, pageDefinition.ActionContainers, dictionary))
			{
				masterPage.CommandBar.ContainsPageTemplateElementsOnly = false;
			}
			foreach (KeyValuePair<ActionContainerPlaceHolder, List<ActionBaseDefinition>> item in dictionary)
			{
				PopActionContainerPlaceHolder(item.Key, item.Value);
			}
		}
		if (masterPage.ContentArea != null)
		{
			MergeControls(masterPage.ContentArea.Controls, pageDefinition.Content.Containers);
		}
		if (masterPage.InfopartsArea != null)
		{
			MergeControls(masterPage.InfopartsArea.Controls, pageDefinition.Content.Containers);
		}
		if (masterPage.NavigationArea != null)
		{
			MergeActions(masterPage.NavigationArea.Actions, pageDefinition.ActionContainers, null);
		}
		masterPage.PageProperties = pageDefinition.Properties;
		masterPage.Expressions = pageDefinition.Expressions;
		masterPage.Caption = pageDefinition.Caption;
		masterPage.CaptionML = pageDefinition.CaptionML;
		masterPage.ResourceIdentifier = pageDefinition.ResourceIdentifier;
		masterPage.ID = pageDefinition.ID;
		masterPage.Name = pageDefinition.Name;
		masterPage.Triggers = pageDefinition.Triggers;
		return masterPage;
	}

	public static MasterPage MergeMasterPageAndPage(PageDefinition pageDefinition, CultureInfo culture)
	{
		MasterPage masterPage = LoadMasterPageTemplateFromResource(pageDefinition.Properties.PageType, culture.ThreeLetterWindowsLanguageName);
		SetTemplateLanguage(culture, masterPage);
		return MergeMasterPageAndPage(masterPage, pageDefinition);
	}

	public static MasterPage GetMasterPage(PageType pageType, string language)
	{
		return LoadMasterPageTemplateFromResource(pageType, language);
	}

	public static void MergeNavigationPageAndPage(MasterNavigation masterNavigation, NavigationDefinition navigationDefinition)
	{
		if (masterNavigation.NavigationPane != null)
		{
			MergeActions(masterNavigation.NavigationPane.Actions, navigationDefinition.ActionContainers, null);
		}
	}

	public static void SetTemplateLanguage(CultureInfo culture, ConceptDefinition conceptDefinition)
	{
		foreach (NamedElementDefinition item in conceptDefinition.FindAll(FindAllNamedElementDefinition))
		{
			string resourceIdentifier = item.ResourceIdentifier;
			string text = string.Empty;
			if (!string.IsNullOrWhiteSpace(resourceIdentifier))
			{
				text = Lang.ResourceManager.GetString(resourceIdentifier, culture);
				if (string.IsNullOrEmpty(text))
				{
					string[] array = resourceIdentifier.Split('_');
					if (array.Length == 2)
					{
						string productNameWithCulture = ProductNames.GetProductNameWithCulture(array[0], culture);
						string text2 = Lang.ResourceManager.GetString(array[1], culture);
						text = ((!string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(productNameWithCulture)) ? string.Format(culture, text2, productNameWithCulture) : resourceIdentifier);
					}
					else
					{
						text = resourceIdentifier;
					}
				}
			}
			item.Caption = text;
			ActionDefinition action = item as ActionDefinition;
			AssignShortcutKeyFromResources(action);
			AssignToolTipFromResources(action);
		}
	}

	private static bool MergeActions(List<ActionBaseDefinition> actions, List<ActionContainerDefinition> actionContainers, Dictionary<ActionContainerPlaceHolder, List<ActionBaseDefinition>> actionPlaceHolders)
	{
		bool flag = false;
		foreach (ActionBaseDefinition action in actions)
		{
			flag |= ProcessActionBaseDefinition(action, actionContainers, actions, actionPlaceHolders);
		}
		return flag;
	}

	private static void PopActionContainerPlaceHolder(ActionContainerPlaceHolder actionContainerPlaceHolder, List<ActionBaseDefinition> parentListOfActions)
	{
		int num = parentListOfActions.IndexOf(actionContainerPlaceHolder);
		if (num >= 0)
		{
			parentListOfActions.RemoveAt(num);
			parentListOfActions.InsertRange(num, actionContainerPlaceHolder.Actions);
		}
	}

	private static void MergeControls(List<ControlBaseDefinition> controls, List<ControlContainerDefinition> controlContainers)
	{
		foreach (ControlBaseDefinition control in controls)
		{
			ProcessControlBaseDefinition(control, controlContainers);
		}
	}

	private static void ProcessControlBaseDefinition(ControlBaseDefinition controlBaseDefinition, List<ControlContainerDefinition> controlContainers)
	{
		if (controlBaseDefinition is ControlContainerPlaceHolder controlContainerPlaceHolder)
		{
			ProcessControlContainerPlaceHolder(controlContainerPlaceHolder, controlContainers);
		}
		else
		{
			if (!(controlBaseDefinition is ControlGroupDefinition controlGroupDefinition))
			{
				return;
			}
			foreach (ControlBaseDefinition control in controlGroupDefinition.Controls)
			{
				ProcessControlBaseDefinition(control, controlContainers);
			}
		}
	}

	private static void ProcessControlContainerPlaceHolder(ControlContainerPlaceHolder controlContainerPlaceHolder, List<ControlContainerDefinition> controlContainers)
	{
		foreach (ControlContainerDefinition controlContainer in controlContainers)
		{
			if (controlContainerPlaceHolder.ContainerType == controlContainer.ContainerType)
			{
				controlContainerPlaceHolder.Controls = controlContainer.Controls;
			}
		}
	}

	private static bool ProcessActionBaseDefinition(ActionBaseDefinition actionBaseDefinition, List<ActionContainerDefinition> actionContainers, List<ActionBaseDefinition> listOfActionOnParent, Dictionary<ActionContainerPlaceHolder, List<ActionBaseDefinition>> actionPlaceHolders)
	{
		bool flag = false;
		if (actionBaseDefinition is ActionContainerPlaceHolder actionContainerPlaceHolder)
		{
			flag = ProcessActionContainerPlaceHolder(actionContainerPlaceHolder, actionContainers);
			actionPlaceHolders?.Add(actionContainerPlaceHolder, listOfActionOnParent);
			return flag;
		}
		if (actionBaseDefinition is ActionGroupBaseDefinition actionGroupBaseDefinition)
		{
			foreach (ActionBaseDefinition action in actionGroupBaseDefinition.Actions)
			{
				flag |= ProcessActionBaseDefinition(action, actionContainers, actionGroupBaseDefinition.Actions, actionPlaceHolders);
			}
		}
		return flag;
	}

	private static bool ProcessActionContainerPlaceHolder(ActionContainerPlaceHolder actionContainerPlaceHolder, List<ActionContainerDefinition> actionContainers)
	{
		bool result = false;
		if (actionContainers != null)
		{
			foreach (ActionContainerDefinition actionContainer in actionContainers)
			{
				if (actionContainerPlaceHolder.ContainerType == actionContainer.ActionContainerType)
				{
					actionContainerPlaceHolder.Actions = actionContainer.Actions;
					if (actionContainer.ActionContainerType == ActionContainerType.HomeItems)
					{
						actionContainerPlaceHolder.ToolTip = actionContainer.ToolTip;
						actionContainerPlaceHolder.ToolTipML = actionContainer.ToolTipML;
					}
					result = actionContainer.Actions.Count > 0;
					break;
				}
			}
		}
		return result;
	}

	private static MasterPage LoadMasterPageTemplateFromResource(PageType pageType, string language)
	{
		MasterPage masterPage = MetadataFromResourceLoader.Load<MasterPage>(string.Format(CultureInfo.InvariantCulture, "MasterPage{0}", pageType));
		masterPage.SetLanguage(language, "ENU");
		return masterPage;
	}

	public static void AssignShortcutKeyFromResources(ActionDefinition action)
	{
		if (action != null)
		{
			string shortCutKey = ((!string.IsNullOrEmpty(action.ShortCutKeyResourceIdentifier)) ? Lang.ResourceManager.GetString(action.ShortCutKeyResourceIdentifier) : Lang.ResourceManager.GetString("MenuActionType" + action.MenuActionType));
			action.ShortCutKey = shortCutKey;
		}
	}

	public static void AssignToolTipFromResources(ActionDefinition action)
	{
		if (action != null && !string.IsNullOrEmpty(action.ToolTipResourceIdentifier))
		{
			action.ToolTip = Lang.ResourceManager.GetString(action.ToolTipResourceIdentifier);
		}
	}

	public static void SetCommandBarLanguage(CultureInfo cultureInfo, ElementDefinition commandBar)
	{
		if (commandBar == null || cultureInfo == null)
		{
			return;
		}
		foreach (ElementDefinition item in commandBar.FindAll(FindAllNamedElementDefinition))
		{
			if (item is ActionBaseDefinition actionBaseDefinition && !string.IsNullOrEmpty(actionBaseDefinition.ResourceIdentifier))
			{
				string text = Lang.ResourceManager.GetString(actionBaseDefinition.ResourceIdentifier, cultureInfo);
				if (!string.IsNullOrEmpty(text))
				{
					actionBaseDefinition.Caption = text;
				}
			}
		}
	}

	private static bool FindAllNamedElementDefinition(ElementDefinition definition)
	{
		return definition is NamedElementDefinition;
	}
}
