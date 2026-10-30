using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal class ElementChangesFilter
{
	private readonly List<string> allowedElementTypesToChange = new List<string> { "Trigger", "FieldGroup", "Variable", "Parameter", "Procedure", "ReturnValue" };

	private readonly AppObjectChangesFilter parent;

	private readonly Dictionary<string, List<string>> whitelist = new Dictionary<string, List<string>>();

	public ElementChangesFilter(AppObjectChangesFilter parent)
	{
		WhitelistValidator.CheckNotNull(parent, "parent");
		this.parent = parent;
	}

	internal bool HandleDeletionOfTarget(string targetStr)
	{
		List<IElement> targetList = parent.GetTargetList(parent.BeforeRoot, targetStr);
		IElement element = targetList.Last();
		if (parent.IsIDChange(ChangeTypes.Deletion, element))
		{
			return parent.PropertyFilter.HandlePropertyChange(ChangeTypes.PropertyModification, targetStr, "Name", null);
		}
		if (!IsAllowed(ChangeTypes.Deletion, element))
		{
			string text = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", ChangeTypes.Deletion, targetStr);
			bool flag = parent.ErrorCallback(text, targetList);
			if (!flag)
			{
				parent.CollectDisallowedChange(text, targetList);
			}
			return flag;
		}
		return true;
	}

	internal bool HandleInsertionForTarget(string targetStr, IElement change)
	{
		List<IElement> targetList = parent.GetTargetList(parent.AfterRoot, targetStr);
		return ProcessInsertion(change, targetList);
	}

	internal bool HandleChildrenInsertion(IElement change)
	{
		List<IElement> targetList = new List<IElement> { parent.AfterRoot };
		return ProcessInsertion(change, targetList);
	}

	internal void AddSetting(string entityType, string changeType, string value)
	{
		string key = string.Format(CultureInfo.InvariantCulture, "{0} {1}", changeType, entityType);
		List<string> list;
		if (!whitelist.ContainsKey(key))
		{
			list = new List<string>();
			whitelist.Add(key, list);
		}
		else
		{
			list = whitelist[key];
		}
		if (!list.Contains(value))
		{
			list.Add(value);
		}
	}

	private bool ProcessInsertion(IElement change, List<IElement> targetList)
	{
		bool flag = true;
		IElement element = targetList.Last();
		foreach (IElement element2 in change.GetSubElements())
		{
			if (parent.IsIDChange(ChangeTypes.Insertion, element2))
			{
				continue;
			}
			IElement element3 = element.GetSubElements(element2.ElementTypeInfo.ElementType).FirstOrDefault((IElement x) => x.ElementTypeInfo.Equals(element2.ElementTypeInfo) && ((element2.Id == 0) ? (x.Name?.Equals(element2.Name) ?? (element2.Name == null)) : x.Id.Equals(element2.Id)));
			targetList.Add(element3);
			if (!IsAllowed(ChangeTypes.Insertion, targetList.Last()))
			{
				string text = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", ChangeTypes.Insertion, element3);
				flag = parent.ErrorCallback(text, targetList);
				if (!flag)
				{
					parent.CollectDisallowedChange(text, targetList);
				}
			}
			targetList.RemoveAt(targetList.Count - 1);
		}
		return flag;
	}

	private bool IsAllowed(ChangeTypes changeType, IElement element)
	{
		string elementType = AppObjectChangesFilter.GetElementType(element);
		if (allowedElementTypesToChange.Contains(elementType))
		{
			return true;
		}
		string key = string.Format(CultureInfo.InvariantCulture, "{0} {1}", changeType, elementType);
		if (whitelist.ContainsKey(key))
		{
			foreach (string item in whitelist[key])
			{
				if (WhitelistEvaluator.Instance.Evaluate(changeType, item, element))
				{
					return true;
				}
			}
		}
		return false;
	}
}
