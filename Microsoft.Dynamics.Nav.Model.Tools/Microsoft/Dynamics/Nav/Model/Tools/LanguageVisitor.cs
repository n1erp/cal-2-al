using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal abstract class LanguageVisitor : ElementVisitor
{
	private readonly SortedDictionary<int, ApplicationObjectLanguageInfo> languageList = new SortedDictionary<int, ApplicationObjectLanguageInfo>();

	private readonly SortedSet<int> languages;

	private readonly List<CultureInfo> cultures;

	protected bool NoLanguageSpecified => languages == null;

	protected IEnumerable<int> LanguageSet => languages;

	protected List<CultureInfo> Cultures => cultures;

	internal LanguageVisitor(IElement root, SortedSet<int> languages)
		: base(root)
	{
		this.languages = languages;
		if (languages != null)
		{
			cultures = languages.Select(WindowsLanguageHelper.FindCulture).ToList();
		}
	}

	internal IEnumerable<ApplicationObjectLanguageInfo> GetLanguages()
	{
		return languageList.Values;
	}

	protected void AddLanguage(int lcid, string key, string value)
	{
		if (!languageList.TryGetValue(lcid, out var value2))
		{
			value2 = new ApplicationObjectLanguageInfo(RootElement.ElementTypeInfo.ElementType, RootElement.Id, lcid);
			languageList.Add(lcid, value2);
		}
		value2.AddLine(key, value);
	}

	protected bool IsLanguageIncluded(CultureInfo cultureInfo)
	{
		return IsLanguageIncluded(ModelTextHelper.CultureToLcid(cultureInfo));
	}

	protected bool IsLanguageIncluded(int lcid)
	{
		return LanguageTool.IsLanguageInSet(languages, lcid);
	}

	protected override IElement VisitElement(IElement element)
	{
		VisitMultiLanguageProperty(element, PropertyType.CaptionML);
		VisitMultiLanguageProperty(element, PropertyType.TextConstString);
		VisitMultiLanguageProperty(element, PropertyType.OptionCaptionML);
		VisitMultiLanguageProperty(element, PropertyType.InstructionalTextML);
		VisitMultiLanguageProperty(element, PropertyType.ToolTipML);
		VisitMultiLanguageProperty(element, PropertyType.RequestFilterHeadingML);
		VisitMultiLanguageProperty(element, PropertyType.PromotedActionCategoriesML);
		VisitMultiLanguageProperty(element, PropertyType.AdditionalSearchTermsML);
		return element;
	}

	protected static bool NotSupportedElement(IElement element)
	{
		return element.ElementTypeInfo.ElementType switch
		{
			ElementType.FieldGroup => true, 
			ElementType.XmlPortElement => true, 
			_ => false, 
		};
	}

	protected abstract void VisitMultiLanguageProperty(IElement element, PropertyType propertyType);
}
