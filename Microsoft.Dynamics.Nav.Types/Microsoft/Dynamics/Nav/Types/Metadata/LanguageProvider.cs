using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class LanguageProvider : ILanguageProvider
{
	private readonly LanguageStore globalStore;

	private readonly INavAppLanguageProvider appLanguageProvider;

	private static ILanguageProvider instance;

	public static ILanguageProvider Provider
	{
		get
		{
			return instance ?? (instance = new LanguageProvider(new LanguageStore(new List<string>()), new NavAppNullLanguageProvider()));
		}
		set
		{
			instance = value;
		}
	}

	public LanguageStore GlobalStore => globalStore;

	private LanguageProvider(LanguageStore globalStore, INavAppLanguageProvider appLanguageProvider)
	{
		this.globalStore = globalStore;
		this.appLanguageProvider = appLanguageProvider;
	}

	public static ILanguageProvider Create(LanguageStore globalStore, INavAppLanguageProvider appLanguageProvider)
	{
		return new LanguageProvider(globalStore, appLanguageProvider);
	}

	public bool HasData(int appGroupId)
	{
		bool hasData = globalStore.HasData;
		bool flag = appLanguageProvider.HasData(appGroupId);
		return hasData | flag;
	}

	public bool HasDataFor(ObjectType objectType, int objectId, int appGroupId)
	{
		if (!HasGlobalDataFor(objectType, objectId))
		{
			return HasAppDataFor(objectType, objectId, appGroupId);
		}
		return true;
	}

	public bool HasGlobalDataFor(ObjectType objectType, int objectId)
	{
		return globalStore.HasDataFor(objectType, objectId);
	}

	public bool HasAppDataFor(ObjectType objectType, int objectId, int appGroupId)
	{
		return appLanguageProvider.HasDataFor(objectType, objectId, appGroupId);
	}

	public LanguageElements GetGlobalMenuItemElements(string menuId)
	{
		return globalStore.GetMenuItemElements(menuId);
	}

	public LanguageElements GetNavAppMenuItemElements(string menuId, int appGroupId)
	{
		return appLanguageProvider.GetMenuItemElements(menuId, appGroupId);
	}

	public LanguageElements GetGlobalLanguageElements(ObjectType objectType, int objectId, ControlType controlType, int controlId, int propertyType, bool requestPage)
	{
		return globalStore.GetLanguageElements(objectType, objectId, controlType, controlId, propertyType, requestPage);
	}

	public LanguageElements GetNavAppLanguageElements(ObjectType objectType, int objectId, ControlType controlType, int controlId, int propertyType, bool requestPage, int appGroupId)
	{
		return appLanguageProvider.GetLanguageElements(objectType, objectId, controlType, controlId, propertyType, requestPage, appGroupId);
	}

	public LanguageElements GetGlobalLanguageElementsTextConst(ObjectType objectType, int objectId, ControlType controlType, int controlId, int codeId, int textId, bool requestPage)
	{
		return globalStore.GetLanguageElementsTextConst(objectType, objectId, controlType, controlId, codeId, textId, requestPage);
	}

	public LanguageElements GetNavAppLanguageElementsTextConst(ObjectType objectType, int objectId, ControlType controlType, int controlId, int codeId, int textId, bool requestPage, int appGroupId)
	{
		return appLanguageProvider.GetLanguageElementsTextConst(objectType, objectId, controlType, controlId, codeId, textId, requestPage, appGroupId);
	}
}
