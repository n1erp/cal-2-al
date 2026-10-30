using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.Apps;

public class NavAppNullLanguageProvider : INavAppLanguageProvider
{
	public bool HasData(int appGroupId)
	{
		return false;
	}

	public bool HasDataFor(ObjectType objectType, int objectId, int appGroupId)
	{
		return false;
	}

	public LanguageElements GetMenuItemElements(string menuId, int appGroupId)
	{
		return null;
	}

	public LanguageElements GetLanguageElements(ObjectType objectType, int objectId, ControlType controlType, int controlId, int propertyType, bool requestPage, int appGroupId)
	{
		return null;
	}

	public LanguageElements GetLanguageElementsTextConst(ObjectType objectType, int objectId, ControlType controlType, int controlId, int codeId, int textId, bool requestPage, int appGroupId)
	{
		return null;
	}
}
