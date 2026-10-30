using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal sealed class TestLanguageVisitor : LanguageVisitor
{
	internal TestLanguageVisitor(IElement root, SortedSet<int> languages)
		: base(root, languages)
	{
	}

	protected override IElement VisitElement(IElement element)
	{
		if (!LanguageVisitor.NotSupportedElement(element))
		{
			return base.VisitElement(element);
		}
		return null;
	}

	protected override void VisitMultiLanguageProperty(IElement element, PropertyType propertyType)
	{
		if (!element.GetPropertySetState(propertyType))
		{
			return;
		}
		IEnumerable<KeyValuePair<CultureInfo, string>> mls = from kv in ModelTextHelper.MLTextSplitter(element.GetStringProperty(propertyType))
			where kv.Key != null
			select kv;
		if (!mls.Any())
		{
			return;
		}
		foreach (CultureInfo item in Cultures.Where((CultureInfo ci) => mls.All((KeyValuePair<CultureInfo, string> kv) => kv.Key != ci)))
		{
			AddLanguage(ModelTextHelper.CultureToLcid(item), LanguageKeyHelper.CreateLanguageKey(element, propertyType, item, 999).ToString(), string.Empty);
		}
	}
}
