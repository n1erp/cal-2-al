using System;
using System.Collections.Concurrent;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

internal class MenuItemObject
{
	public int ObjectId { get; private set; }

	public ConcurrentDictionary<int, string> Translations { get; private set; }

	public Guid MenuId { get; private set; }

	public int ReferenceCount { get; set; }

	internal MenuItemObject(int objectId, MenuItemElement menuItemElement)
	{
		ObjectId = objectId;
		MenuId = menuItemElement.MenuId;
		ObjectId = menuItemElement.ObjectId;
		Translations = new ConcurrentDictionary<int, string>();
		Translations.GetOrAdd(menuItemElement.LanguageCode, menuItemElement.ItemValue);
		ReferenceCount = 0;
	}
}
