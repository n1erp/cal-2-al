using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal class ExportLanguageVisitor : LanguageVisitor
{
	private readonly bool makeCapSupport;

	private readonly int developmentLcid;

	internal ExportLanguageVisitor(IElement root, SortedSet<int> languages, int developmentLcid, bool makeCapSupport = true)
		: base(root, languages)
	{
		this.makeCapSupport = makeCapSupport;
		this.developmentLcid = developmentLcid;
	}

	protected override IElement VisitElement(IElement element)
	{
		if (LanguageVisitor.NotSupportedElement(element))
		{
			return null;
		}
		MakeCap(element);
		return base.VisitElement(element);
	}

	protected override void VisitMultiLanguageProperty(IElement element, PropertyType propertyType)
	{
		if (!element.GetPropertySetState(propertyType))
		{
			return;
		}
		KeyValuePair<CultureInfo, string>[] source = ModelTextHelper.MLTextSplitter(element.GetStringProperty(propertyType)).ToArray();
		if (NoLanguageSpecified)
		{
			foreach (KeyValuePair<CultureInfo, string> item in source.Where((KeyValuePair<CultureInfo, string> kv) => kv.Key != null))
			{
				AddLanguage(item.Key.LCID, LanguageKeyHelper.CreateLanguageKey(element, propertyType, item.Key, 999).ToString(), item.Value);
			}
			return;
		}
		foreach (int lcid in LanguageSet)
		{
			KeyValuePair<CultureInfo, string> keyValuePair = source.SingleOrDefault((KeyValuePair<CultureInfo, string> kv) => ModelTextHelper.CultureToLcid(kv.Key) == lcid);
			if (keyValuePair.Value != null || lcid != 1)
			{
				AddLanguage(lcid, LanguageKeyHelper.CreateLanguageKey(element, propertyType, lcid, 999).ToString(), keyValuePair.Value ?? string.Empty);
			}
		}
	}

	private void MakeCap(IElement element)
	{
		if (!NoLanguageSpecified && makeCapSupport)
		{
			switch (element.ElementTypeInfo.ElementType)
			{
			case ElementType.Report:
			case ElementType.XmlPort:
			case ElementType.Table:
			case ElementType.Page:
			case ElementType.MenuNode:
			case ElementType.Action:
			case ElementType.Query:
				AddMultiLanguageProperty(element, PropertyType.Name, PropertyType.CaptionML);
				break;
			case ElementType.Field:
				AddMultiLanguageProperty(element, PropertyType.Name, PropertyType.CaptionML);
				AddMultiLanguageProperty(element, PropertyType.OptionString, PropertyType.OptionCaptionML);
				break;
			}
		}
	}

	private void AddMultiLanguageProperty(IElement element, PropertyType sourceProperty, PropertyType destinationProperty)
	{
		if (!element.GetPropertySetState(sourceProperty))
		{
			return;
		}
		List<KeyValuePair<CultureInfo, string>> list = ModelTextHelper.MLTextSplitter(element.GetStringProperty(destinationProperty)).ToList();
		foreach (int lcid in LanguageSet)
		{
			if (lcid != 1 && list.All((KeyValuePair<CultureInfo, string> kv) => ModelTextHelper.CultureToLcid(kv.Key) != lcid))
			{
				list.Add(new KeyValuePair<CultureInfo, string>(WindowsLanguageHelper.FindCulture(lcid), (lcid == developmentLcid) ? element.GetStringProperty(sourceProperty) : string.Empty));
			}
		}
		element.SetStringProperty(destinationProperty, string.Join(";", ModelTextHelper.MLTextFormatter(list, 0)));
	}
}
