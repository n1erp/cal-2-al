using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class SessionSettingsScaffolding : Scaffolding<NavSessionSettings>
{
	private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public override string TypeString => ScaffoldingHelper.GetTypeString(NavNclType.NavSessionSettings, null);

	protected override bool ProbablyHasChildren => true;

	public SessionSettingsScaffolding(string name, Type targetType, ValueBinder valueBinder, bool promotedChildren)
		: base(name, targetType, valueBinder, false, promotedChildren)
	{
	}

	protected override string GetStringValue(NavSessionSettings instance)
	{
		return null;
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		return instance is NavSessionSettings;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavSessionSettings instance)
	{
		Type type = instance.GetType();
		PropertyInfo property = type.GetProperty("ALCompany", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		PropertyInfo profileIdPropertyInfo = type.GetProperty("ALProfileId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		PropertyInfo profileAppIdPropertyInfo = type.GetProperty("ALProfileAppId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		PropertyInfo profileSystemScopePropertyInfo = type.GetProperty("ALProfileSystemScope", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		PropertyInfo languageIdPropertyInfo = type.GetProperty("ALLanguageId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		PropertyInfo localeIdPropertyInfo = type.GetProperty("ALLocaleId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		PropertyInfo timeZonePropertyInfo = type.GetProperty("ALTimeZone", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("Company", property.PropertyType, new PropertyInfoValueBinder(property), instance, false);
		yield return ScaffoldingFactory.GetScaffolding("Profile ID", profileIdPropertyInfo.PropertyType, new PropertyInfoValueBinder(profileIdPropertyInfo), instance, false);
		yield return ScaffoldingFactory.GetScaffolding("Profile App ID", profileAppIdPropertyInfo.PropertyType, new PropertyInfoValueBinder(profileAppIdPropertyInfo), instance, false);
		yield return ScaffoldingFactory.GetScaffolding("Profile System Scope", profileSystemScopePropertyInfo.PropertyType, new PropertyInfoValueBinder(profileSystemScopePropertyInfo), instance, false);
		yield return ScaffoldingFactory.GetScaffolding("Language ID", languageIdPropertyInfo.PropertyType, new PropertyInfoValueBinder(languageIdPropertyInfo), instance, false);
		yield return ScaffoldingFactory.GetScaffolding("Locale ID", localeIdPropertyInfo.PropertyType, new PropertyInfoValueBinder(localeIdPropertyInfo), instance, false);
		yield return ScaffoldingFactory.GetScaffolding("Time Zone", timeZonePropertyInfo.PropertyType, new PropertyInfoValueBinder(timeZonePropertyInfo), instance, false);
	}
}
