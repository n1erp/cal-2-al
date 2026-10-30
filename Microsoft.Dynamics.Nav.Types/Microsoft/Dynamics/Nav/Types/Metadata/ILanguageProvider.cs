namespace Microsoft.Dynamics.Nav.Types.Metadata;

public interface ILanguageProvider
{
	bool HasData(int appGroupId);

	bool HasDataFor(ObjectType objectType, int objectId, int appGroupId);

	bool HasGlobalDataFor(ObjectType objectType, int objectId);

	bool HasAppDataFor(ObjectType objectType, int objectId, int appGroupId);

	LanguageElements GetGlobalMenuItemElements(string menuId);

	LanguageElements GetNavAppMenuItemElements(string menuId, int appGroupId);

	LanguageElements GetGlobalLanguageElements(ObjectType objectType, int objectId, ControlType controlType, int controlId, int propertyType, bool requestPage);

	LanguageElements GetNavAppLanguageElements(ObjectType objectType, int objectId, ControlType controlType, int controlId, int propertyType, bool requestPage, int appGroupId);

	LanguageElements GetGlobalLanguageElementsTextConst(ObjectType objectType, int objectId, ControlType controlType, int controlId, int codeId, int textId, bool requestPage);

	LanguageElements GetNavAppLanguageElementsTextConst(ObjectType objectType, int objectId, ControlType controlType, int controlId, int codeId, int textId, bool requestPage, int appGroupId);
}
