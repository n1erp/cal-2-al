using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal sealed class RemoveLanguageVisitor : LanguageVisitor
{
	private readonly int defaultLanguage;

	private readonly bool reverseMakeCap;

	internal RemoveLanguageVisitor(IElement root, SortedSet<int> languages, int defaultLanguage = 0, bool reverseMakeCap = false)
		: base(root, languages)
	{
		this.defaultLanguage = ((defaultLanguage == 0) ? WindowsLanguageHelper.FindCulture("ENU").LCID : defaultLanguage);
		this.reverseMakeCap = reverseMakeCap;
	}

	protected override IElement VisitElement(IElement element)
	{
		element = base.VisitElement(element);
		switch (element.ElementTypeInfo.ElementType)
		{
		case ElementType.Report:
		case ElementType.XmlPort:
		case ElementType.Table:
		case ElementType.Page:
		case ElementType.MenuNode:
		case ElementType.Query:
			RemoveLanguageIfMatchOriginal(element, PropertyType.CaptionML, PropertyType.Name);
			break;
		case ElementType.Field:
			RemoveLanguageIfMatchOriginal(element, PropertyType.CaptionML, PropertyType.Name);
			RemoveLanguageIfMatchOriginal(element, PropertyType.OptionCaptionML, PropertyType.OptionString);
			break;
		}
		return element;
	}

	protected override void VisitMultiLanguageProperty(IElement element, PropertyType propertyType)
	{
		if (element.GetPropertySetState(propertyType))
		{
			string value = ModelTextHelper.RemoveLanguage(element.GetStringProperty(propertyType), ShouldRemove);
			if (string.IsNullOrEmpty(value))
			{
				element.ClearProperty(propertyType);
			}
			else
			{
				element.SetStringProperty(propertyType, value);
			}
		}
	}

	private bool ShouldRemove(CultureInfo culture)
	{
		if (IsLanguageIncluded(culture) && culture != null)
		{
			return ModelTextHelper.CultureToLcid(culture) != defaultLanguage;
		}
		return false;
	}

	private void RemoveLanguageIfMatchOriginal(IElement element, PropertyType target, PropertyType source)
	{
		if (reverseMakeCap && element.GetPropertySetState(target))
		{
			KeyValuePair<CultureInfo, string>[] array = ModelTextHelper.MLTextSplitter(element.GetStringProperty(target)).ToArray();
			if (array.Length == 1 && ModelTextHelper.CultureToLcid(array[0].Key) == defaultLanguage && array[0].Value == element.GetStringProperty(source))
			{
				element.ClearProperty(target);
			}
		}
	}
}
