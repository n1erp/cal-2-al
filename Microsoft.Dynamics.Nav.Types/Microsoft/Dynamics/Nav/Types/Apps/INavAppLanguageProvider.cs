using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.Apps;

public interface INavAppLanguageProvider
{
	bool HasData(int appGroupId);

	bool HasDataFor(ObjectType objectType, int objectId, int appGroupId);

	LanguageElements GetMenuItemElements(string menuId, int appGroupId);

	LanguageElements GetLanguageElements(ObjectType objectType, int objectId, ControlType controlType, int controlId, int propertyType, bool requestPage, int appGroupId);

	LanguageElements GetLanguageElementsTextConst(ObjectType objectType, int objectId, ControlType controlType, int controlId, int codeId, int textId, bool requestPage, int appGroupId);
}
