using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Types;

public class ConfigObjectBinder<TObject, TPropertyProxy>
{
	private readonly Dictionary<PropertyInfo, TPropertyProxy> propertyMap;

	public IEnumerable<TPropertyProxy> PropertyProxies => propertyMap.Values;

	public ConfigObjectBinder(Func<PropertyInfo, ConfigParameterAttribute, TypeConverter, ConfigurationValidatorBase, TPropertyProxy> createPropertyProxy, bool onlyCmdletParameters)
	{
		propertyMap = new Dictionary<PropertyInfo, TPropertyProxy>();
		PropertyInfo[] properties = typeof(TObject).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			ConfigParameterAttribute customAttribute = propertyInfo.GetCustomAttribute<ConfigParameterAttribute>();
			if (customAttribute != null && (!onlyCmdletParameters || customAttribute.CmdletParameter))
			{
				TypeConverterAttribute customAttribute2 = propertyInfo.GetCustomAttribute<TypeConverterAttribute>();
				TypeConverter arg = ((customAttribute2 == null) ? null : ((TypeConverter)Activator.CreateInstance(Type.GetType(customAttribute2.ConverterTypeName, throwOnError: true), nonPublic: true)));
				ConfigurationValidatorBase arg2 = propertyInfo.GetCustomAttribute<ConfigurationValidatorAttribute>()?.ValidatorInstance;
				TPropertyProxy value = createPropertyProxy(propertyInfo, customAttribute, arg, arg2);
				propertyMap.Add(propertyInfo, value);
			}
		}
	}

	public TObject CreateObject(Func<TPropertyProxy, object> getPropertyValue, Func<TPropertyProxy, bool> isPropertyValueSet = null)
	{
		TObject val = (TObject)Activator.CreateInstance(typeof(TObject), nonPublic: true);
		foreach (KeyValuePair<PropertyInfo, TPropertyProxy> item in propertyMap)
		{
			if (isPropertyValueSet == null || isPropertyValueSet(item.Value))
			{
				item.Key.SetValue(val, getPropertyValue(item.Value));
			}
		}
		return val;
	}

	public void UpdatePropertiesFromObject(TObject instance, Action<TPropertyProxy, object> setPropertyValue)
	{
		foreach (KeyValuePair<PropertyInfo, TPropertyProxy> item in propertyMap)
		{
			setPropertyValue(item.Value, item.Key.GetValue(instance));
		}
	}
}
