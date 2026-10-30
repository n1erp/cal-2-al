using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public sealed class MetaPage
{
	internal static readonly XmlSerializer NavigationDefinitionSerializer = new XmlSerializer(typeof(NavigationDefinition));

	private static readonly XmlSerializer PageDefinitionSerializer = new XmlSerializer(typeof(PageDefinition));

	private MetaPage()
	{
	}

	public static PageDefinition CreatePageDefinition(XmlDocument pageDocument, int languageAppGroupId)
	{
		XmlReader xmlReader = XmlHelper.SafeXmlReader(new StringReader(pageDocument.OuterXml));
		PageDefinition pageDefinition = (PageDefinition)PageDefinitionSerializer.Deserialize(xmlReader);
		if (LanguageProvider.Provider.HasDataFor(ObjectType.Page, pageDefinition.ID, languageAppGroupId))
		{
			pageDefinition.MergeMultiLanguage(ObjectType.Page, pageDefinition.ID, languageAppGroupId);
		}
		return pageDefinition;
	}

	public static NavigationDefinition CreateNavigationDefinition(XmlDocument menuSuiteDocument, int id, int languageAppGroupId)
	{
		XmlReader xmlReader = XmlHelper.SafeXmlReader(new StringReader(menuSuiteDocument.OuterXml));
		NavigationDefinition navigationDefinition = (NavigationDefinition)NavigationDefinitionSerializer.Deserialize(xmlReader);
		navigationDefinition.ID = id;
		if (LanguageProvider.Provider.HasData(languageAppGroupId))
		{
			navigationDefinition.MergeMenuSuiteMultiLanguage(languageAppGroupId);
		}
		return navigationDefinition;
	}

	public static string MultiLanguageTextMerge(string attributeValue, ObjectType objType, int id, ControlType subtype, int subId, int attributeId, int languageAppGroupId)
	{
		LanguageElements globalLanguageElements = LanguageProvider.Provider.GetGlobalLanguageElements(objType, id, subtype, subId, attributeId, objType != ObjectType.Page);
		LanguageElements navAppLanguageElements = LanguageProvider.Provider.GetNavAppLanguageElements(objType, id, subtype, subId, attributeId, objType != ObjectType.Page, languageAppGroupId);
		return ElementDefinition.MergeGlobalLanguageStrings(ElementDefinition.MergeAppLanguageStrings(attributeValue, navAppLanguageElements), globalLanguageElements);
	}

	public static string MultiLanguageTextMerge(string attributeValue, string controlGuid, int languageAppGroupId)
	{
		LanguageElements globalMenuItemElements = LanguageProvider.Provider.GetGlobalMenuItemElements(controlGuid);
		LanguageElements navAppMenuItemElements = LanguageProvider.Provider.GetNavAppMenuItemElements(controlGuid, languageAppGroupId);
		return ElementDefinition.MergeGlobalLanguageStrings(ElementDefinition.MergeAppLanguageStrings(attributeValue, navAppMenuItemElements), globalMenuItemElements);
	}
}
