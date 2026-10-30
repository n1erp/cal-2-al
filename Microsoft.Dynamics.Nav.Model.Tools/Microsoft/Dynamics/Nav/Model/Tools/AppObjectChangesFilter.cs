using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.Tools.Language;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal class AppObjectChangesFilter
{
	private List<string> disallowedChanges = new List<string>();

	internal readonly List<ElementType> ElementTypesWithIDChangesAsElementChanges = new List<ElementType> { ElementType.Key };

	internal readonly List<PropertyType> IDProperties = new List<PropertyType> { PropertyType.Key };

	internal readonly ElementChangesFilter ElementFilter;

	internal readonly PropertyChangesFilter PropertyFilter;

	internal Dictionary<ElementType, List<(IElement, IElement)>> ChangesOfIDs = new Dictionary<ElementType, List<(IElement, IElement)>>();

	private readonly IElementKeyProvider keyProvider;

	private ApplicationObject after;

	private ApplicationObject before;

	internal Func<string, IReadOnlyCollection<IElement>, bool> ErrorCallback;

	private ApplicationObject modification;

	internal IElement BeforeRoot => before.RootElement;

	internal IElement AfterRoot => after.RootElement;

	public AppObjectChangesFilter(string[] settingsBody, Func<string, IReadOnlyCollection<IElement>, bool> errorCallback)
	{
		WhitelistValidator.CheckNotNull(settingsBody, "settingsBody");
		WhitelistValidator.CheckNotNull(errorCallback, "errorCallback");
		ErrorCallback = errorCallback;
		ElementFilter = new ElementChangesFilter(this);
		PropertyFilter = new PropertyChangesFilter(this);
		new SettingsParser().LoadSettings(this, settingsBody);
		keyProvider = new DisplayElementKeyProvider();
	}

	internal void CollectDisallowedChange(string changeDescription, IReadOnlyCollection<IElement> changeStack)
	{
		IElement element = changeStack.First().Parent ?? changeStack.First();
		StringBuilder stringBuilder = new StringBuilder(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}, {3}:\n", element.ElementTypeInfo.ElementType, element.Id, element.Name, NonLocalizable.ChangeIsNotAllowed));
		stringBuilder.AppendLine(changeDescription);
		string text = " ";
		foreach (IElement item in changeStack)
		{
			string value = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3} {4}", text, item.ElementTypeInfo.Category, item.ElementTypeInfo.ElementType, item.Id, item.Name);
			stringBuilder.AppendLine(value);
			text += " ";
		}
		disallowedChanges.Add(stringBuilder.ToString());
	}

	private void CalcIDChanges()
	{
		ChangesOfIDs.Clear();
		foreach (ElementType elementTypesWithIDChangesAsElementChange in ElementTypesWithIDChangesAsElementChanges)
		{
			List<(IElement, IElement)> list = new List<(IElement, IElement)>();
			ChangesOfIDs.Add(elementTypesWithIDChangesAsElementChange, list);
			List<IElement> list2 = ((BeforeRoot == null) ? new List<IElement>() : BeforeRoot.GetSubElements(elementTypesWithIDChangesAsElementChange).ToList());
			List<IElement> list3 = ((AfterRoot == null) ? new List<IElement>() : AfterRoot.GetSubElements(elementTypesWithIDChangesAsElementChange).ToList());
			int num = Math.Min(list2.Count, list3.Count);
			for (int i = 0; i < num; i++)
			{
				IElement element = list2[i];
				IElement element2 = list3[i];
				if (element.Name != element2.Name && ArePropertieEquals(element, element2))
				{
					list.Add((element, element2));
				}
			}
		}
	}

	private bool ArePropertieEquals(IElement beforeElement, IElement afterElement)
	{
		List<PropertyType> list = beforeElement.GetProperties().ToList();
		List<PropertyType> list2 = afterElement.GetProperties().ToList();
		if (list.Count != list2.Count)
		{
			return false;
		}
		for (int i = 0; i < list.Count(); i++)
		{
			if (list[i] != list2[i])
			{
				return false;
			}
			if (!IDProperties.Contains(list[i]) && beforeElement.GetStringProperty(list[i]) != afterElement.GetStringProperty(list2[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal bool IsIDChange(ChangeTypes change, IElement element)
	{
		if (!ElementTypesWithIDChangesAsElementChanges.Contains(element.ElementTypeInfo.ElementType))
		{
			return false;
		}
		var (element2, element3) = GetBeforeAndAfter(change, element);
		if (element2 != null && element3 != null)
		{
			return true;
		}
		return false;
	}

	internal (IElement, IElement) GetBeforeAndAfter(ChangeTypes change, IElement element)
	{
		return change switch
		{
			ChangeTypes.Insertion => ChangesOfIDs[element.ElementTypeInfo.ElementType].SingleOrDefault(((IElement, IElement) tuple) => tuple.Item2.Name == element.Name), 
			ChangeTypes.Deletion => ChangesOfIDs[element.ElementTypeInfo.ElementType].SingleOrDefault(((IElement, IElement) tuple) => tuple.Item1.Name == element.Name), 
			_ => (null, null), 
		};
	}

	internal IReadOnlyCollection<string> GetDisallowedChanges()
	{
		return disallowedChanges.AsReadOnly();
	}

	public bool ValidateModification(ApplicationObject inBefore, ApplicationObject inAfter)
	{
		WhitelistValidator.CheckNotNull(inBefore, "inBefore");
		WhitelistValidator.CheckNotNull(inAfter, "inAfter");
		before = inBefore;
		after = inAfter;
		CalcModification();
		CalcIDChanges();
		if (modification.RootElement == null)
		{
			return true;
		}
		WhitelistEvaluator.Instance.SetChangesFilter(this);
		if (modification.RootElement.GetProperties().Contains(PropertyType.ChangeType))
		{
			string stringProperty = modification.RootElement.GetStringProperty(PropertyType.ChangeType);
			string stringProperty2 = GetStringProperty(modification.RootElement, PropertyType.Target);
			switch ((ChangeTypes)Enum.Parse(typeof(ChangeTypes), stringProperty))
			{
			case ChangeTypes.Deletion:
				return ElementFilter.HandleDeletionOfTarget(stringProperty2);
			case ChangeTypes.Insertion:
				return ElementFilter.HandleInsertionForTarget(stringProperty2, modification.RootElement);
			}
		}
		bool result = true;
		foreach (IElement item in from x in modification.GetElements()
			where x.ElementTypeInfo.ElementType.Equals(ElementType.Change)
			select x)
		{
			if (!HandleChange(item))
			{
				result = false;
			}
		}
		return result;
	}

	internal List<IElement> GetTargetList(IElement fullObjectRoot, string targetStr)
	{
		IEnumerable<IElementKeySegment> elementKeySegments = keyProvider.GetElementKeySegments(targetStr);
		IElement element = fullObjectRoot;
		List<IElement> list = new List<IElement>();
		foreach (IElementKeySegment item in elementKeySegments.Reverse())
		{
			element = ResolveTarget(element, item);
			list.Add(element);
		}
		return list;
	}

	internal static string GetElementType(IElement element)
	{
		if (element.ElementTypeInfo.Category.Equals(CategoryType.Trigger))
		{
			return CategoryType.Trigger.ToString();
		}
		return element.ElementTypeInfo.ElementType.ToString();
	}

	internal static string GetStringProperty(IElement element, PropertyType type)
	{
		if (!element.TryGetStringProperty(type, out var value) || value == null)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.CannotFindPropertyForElement, type, element));
		}
		return value;
	}

	internal void AddElementSetting(string entityType, string changeType, string value)
	{
		ElementFilter.AddSetting(entityType, changeType, value);
	}

	internal void AddPropertySetting(string entityType, string propertyName, string value)
	{
		PropertyFilter.AddSetting(entityType, propertyName, value);
	}

	private void CalcModification()
	{
		ComputeChangesOptions options = new ComputeChangesOptions
		{
			DevBaseCompatibility = true,
			IgnoreDocumentationChanges = true
		};
		IElement element;
		if (before.RootElement == null && after.RootElement == null)
		{
			element = null;
		}
		else if (before.RootElement == null && after.RootElement != null)
		{
			element = GenericElement.Create(ElementType.Modification);
			element.SetStringProperty(PropertyType.Name, keyProvider.CreateElementKeySegmentString(after.RootElement));
			element.SetStringProperty(PropertyType.Target, keyProvider.CreateElementKeySegmentString(after.RootElement));
			element.SetStringProperty(PropertyType.ChangeType, ChangeTypes.Insertion.ToString());
		}
		else
		{
			element = new ComputeChanges(before.RootElement, options).Compute(after.RootElement);
		}
		modification = new ApplicationObject(element);
	}

	private bool HandleChange(IElement element)
	{
		string stringProperty = GetStringProperty(element, PropertyType.ChangeType);
		if (!Enum.TryParse<ChangeTypes>(stringProperty, out var result))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.CouldNotParseChangeType, stringProperty, element));
		}
		string value;
		switch (result)
		{
		case ChangeTypes.PropertyDeletion:
		case ChangeTypes.PropertyInsertion:
		case ChangeTypes.PropertyModification:
		{
			string stringProperty2 = GetStringProperty(element, PropertyType.Property);
			if (!element.TryGetStringProperty(PropertyType.Target, out value) || value == null)
			{
				return PropertyFilter.HandleObjectPropertyChange(result, stringProperty2);
			}
			return PropertyFilter.HandlePropertyChange(result, value, stringProperty2, element);
		}
		case ChangeTypes.Deletion:
			if (element.TryGetStringProperty(PropertyType.Target, out value) && value != null)
			{
				return ElementFilter.HandleDeletionOfTarget(value);
			}
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.CouldNotRecognizeChange, ChangeTypes.Deletion, element));
		case ChangeTypes.Insertion:
		{
			if (element.TryGetStringProperty(PropertyType.Target, out value) && value != null)
			{
				return ElementFilter.HandleInsertionForTarget(value, element);
			}
			if (element.TryGetStringProperty(PropertyType.AddedChildren, out var value2) && value2 != null)
			{
				return ElementFilter.HandleChildrenInsertion(element);
			}
			if (element.GetSubElementsCount() == 1 && GetElementType(element.GetSubElements().ElementAt(0)).Equals("Trigger"))
			{
				return ElementFilter.HandleInsertionForTarget(keyProvider.CreateElementKeySegmentString(AfterRoot), element);
			}
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.CouldNotRecognizeChange, ChangeTypes.Insertion, element));
		}
		case ChangeTypes.Move:
			return true;
		case ChangeTypes.RdlModification:
		case ChangeTypes.WordLayoutModification:
			return true;
		case ChangeTypes.CodeModification:
			return true;
		default:
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, NonLocalizable.CouldNotRecognizeChange, result, element));
		}
	}

	private static IElement ResolveTarget(IElement parent, IElementKeySegment key)
	{
		int id = ((!string.IsNullOrEmpty(key.Id)) ? int.Parse(key.Id, CultureInfo.InvariantCulture) : 0);
		string name = (string.IsNullOrEmpty(key.Name) ? null : key.Name);
		if (parent.ElementTypeInfo.ElementType.Equals(key.ElementType) && ((id != 0) ? parent.Id.Equals(id) : (parent.Name?.Equals(name) ?? (name == null))))
		{
			return parent;
		}
		return parent.GetSubElements(key.ElementType).FirstOrDefault((IElement x) => (id == 0) ? (x.Name?.Equals(name) ?? (name == null)) : x.Id.Equals(id));
	}
}
