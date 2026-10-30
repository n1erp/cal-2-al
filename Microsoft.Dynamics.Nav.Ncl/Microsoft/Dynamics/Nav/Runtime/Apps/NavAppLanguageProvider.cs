using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppLanguageProvider : INavAppLanguageProvider
{
	private readonly Func<Guid, LanguageStore> getAppLanguageStore;

	private readonly Func<int, NavAppGroup> getGroupFromId;

	private readonly ConcurrentDictionary<int, LanguageStore> appLanguageStores = new ConcurrentDictionary<int, LanguageStore>();

	private readonly ConcurrentDictionary<int, List<LanguageStore>> appGroupLanguageStores = new ConcurrentDictionary<int, List<LanguageStore>>();

	public NavAppLanguageProvider(Func<Guid, LanguageStore> getAppLanguageStore, Func<int, NavAppGroup> getGroupFromId)
	{
		this.getAppLanguageStore = getAppLanguageStore;
		this.getGroupFromId = getGroupFromId;
	}

	public bool HasData(int appGroupId)
	{
		return GetAppGroupLanguageStores(appGroupId).Any((LanguageStore store) => store.HasData);
	}

	public bool HasDataFor(ObjectType objectType, int objectId, int appGroupId)
	{
		return GetAppGroupLanguageStores(appGroupId).Any((LanguageStore store) => store.HasDataFor(objectType, objectId));
	}

	public LanguageElements GetMenuItemElements(string menuId, int appGroupId)
	{
		LanguageElements languageElements = null;
		foreach (LanguageStore appGroupLanguageStore in GetAppGroupLanguageStores(appGroupId))
		{
			LanguageElements menuItemElements = appGroupLanguageStore.GetMenuItemElements(menuId);
			languageElements = SafeMerge(languageElements, menuItemElements);
		}
		return languageElements;
	}

	public LanguageElements GetLanguageElements(ObjectType objectType, int objectId, ControlType controlType, int controlId, int propertyType, bool requestPage, int appGroupId)
	{
		LanguageElements languageElements = null;
		foreach (LanguageStore appGroupLanguageStore in GetAppGroupLanguageStores(appGroupId))
		{
			LanguageElements languageElements2 = appGroupLanguageStore.GetLanguageElements(objectType, objectId, controlType, controlId, propertyType, requestPage);
			languageElements = SafeMerge(languageElements, languageElements2);
		}
		return languageElements;
	}

	public LanguageElements GetLanguageElementsTextConst(ObjectType objectType, int objectId, ControlType controlType, int controlId, int codeId, int textId, bool requestPage, int appGroupId)
	{
		LanguageElements languageElements = null;
		foreach (LanguageStore appGroupLanguageStore in GetAppGroupLanguageStores(appGroupId))
		{
			LanguageElements languageElementsTextConst = appGroupLanguageStore.GetLanguageElementsTextConst(objectType, objectId, controlType, controlId, codeId, textId, requestPage);
			languageElements = SafeMerge(languageElements, languageElementsTextConst);
		}
		return languageElements;
	}

	private IEnumerable<LanguageStore> GetAppGroupLanguageStores(int appGroupId)
	{
		return appGroupLanguageStores.GetOrAdd(appGroupId, (int groupId) =>
		{
			List<LanguageStore> list = new List<LanguageStore>();
			NavAppGroup navAppGroup = getGroupFromId(appGroupId);
			foreach (NavAppRuntimeMetadata orderedAppMetadatum in navAppGroup.OrderedAppMetadata)
			{
				LanguageStore appLanguageStore = GetAppLanguageStore(orderedAppMetadatum);
				list.Add(appLanguageStore);
			}
			return list;
		});
	}

	private LanguageStore GetAppLanguageStore(NavAppRuntimeMetadata metadata)
	{
		return appLanguageStores.GetOrAdd(metadata.RuntimeAppId, (int runtimeAppId) => getAppLanguageStore(((WrappedValue<Guid>)(object)metadata.PackageId).Value));
	}

	private static LanguageElements SafeMerge(LanguageElements origElements, LanguageElements newElements)
	{
		LanguageElements result = origElements;
		if (newElements != null)
		{
			result = ((origElements != null) ? origElements.MergeWith(newElements) : newElements);
		}
		return result;
	}
}
