using System;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

internal class MenuItemElement
{
	public int ObjectId { get; private set; }

	public int LanguageCode { get; private set; }

	public string ItemValue { get; private set; }

	public Guid MenuId { get; private set; }

	public MenuItemElement(int objectId, int language, Guid menuId, string itemValue)
	{
		ObjectId = objectId;
		LanguageCode = language;
		ItemValue = itemValue;
		MenuId = menuId;
	}
}
