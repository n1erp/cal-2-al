using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.Tools.SchemaGatingTools;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal class PropertyChangesFilter
{
	private readonly List<string> allowedElementsToChangeProperties = new List<string> { "ReturnValue", "Variable", "Parameter", "Procedure", "FieldGroup" };

	internal static readonly List<string> DataTypesWithLength = new List<string> { "Code", "Text" };

	private readonly AppObjectChangesFilter parent;

	private readonly Dictionary<string, List<string>> whitelist = new Dictionary<string, List<string>>();

	public PropertyChangesFilter(AppObjectChangesFilter parent)
	{
		WhitelistValidator.CheckNotNull(parent, "parent");
		this.parent = parent;
	}

	internal bool HandleObjectPropertyChange(ChangeTypes changeType, string property)
	{
		List<IElement> targetList = new List<IElement> { parent.BeforeRoot };
		return HandlePropertyChangeInternal(changeType, property, targetList);
	}

	internal bool HandlePropertyChange(ChangeTypes changeType, string targetStr, string property, IElement change)
	{
		List<IElement> targetList = parent.GetTargetList(parent.BeforeRoot, targetStr);
		if (changeType.Equals(ChangeTypes.PropertyModification) && property.Equals(EvaluatorConstants.DataTypePropertyName))
		{
			string stringProperty = AppObjectChangesFilter.GetStringProperty(change, PropertyType.OriginalValue);
			string stringProperty2 = AppObjectChangesFilter.GetStringProperty(change, PropertyType.OriginalValue);
			foreach (string item in DataTypesWithLength)
			{
				if (stringProperty.StartsWith(item, StringComparison.Ordinal) && stringProperty2.StartsWith(item, StringComparison.Ordinal))
				{
					return HandlePropertyChangeInternal(changeType, EvaluatorConstants.DataLengthName, targetList);
				}
			}
		}
		return HandlePropertyChangeInternal(changeType, property, targetList);
	}

	internal void AddSetting(string elementType, string propertyName, string value)
	{
		string key = string.Format(CultureInfo.InvariantCulture, "{0} {1}", elementType, propertyName);
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

	private bool HandlePropertyChangeInternal(ChangeTypes changeType, string property, List<IElement> targetList)
	{
		if (!IsAllowed(changeType, property, targetList.Last()))
		{
			string text = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", changeType, property);
			bool flag = parent.ErrorCallback(text, targetList);
			if (!flag)
			{
				parent.CollectDisallowedChange(text, targetList);
			}
			return flag;
		}
		return true;
	}

	private bool IsAllowed(ChangeTypes changeType, string propertyName, IElement element)
	{
		string elementType = AppObjectChangesFilter.GetElementType(element);
		if (allowedElementsToChangeProperties.Contains(elementType))
		{
			return true;
		}
		string key = string.Format(CultureInfo.InvariantCulture, "{0}Property {1}", elementType, propertyName);
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
