using System;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Types;

public class ServerSettingDefinition
{
	private readonly string propertyName;

	private readonly ConfigSettingAttribute attribute;

	private readonly PropertyInfo propertyInfo;

	public ConfigSettingAttribute Definition => attribute;

	public string PropertyName => propertyName;

	public Type PropertyType => propertyInfo.PropertyType;

	public ServerSettingDefinition(string propertyName, ConfigSettingAttribute attribute, PropertyInfo propertyInfo)
	{
		this.propertyName = propertyName ?? throw new ArgumentNullException("propertyName");
		this.attribute = attribute ?? throw new ArgumentNullException("attribute");
		this.propertyInfo = propertyInfo ?? throw new ArgumentNullException("propertyInfo");
	}

	public string ReadValue(object settingInstance)
	{
		return propertyInfo.GetValue(settingInstance)?.ToString();
	}

	public void SetValue(object settingInstance, string value)
	{
		try
		{
			if (string.IsNullOrEmpty(value))
			{
				value = Definition.DefaultValue.ToString();
			}
			if (SettingsHelper.TryParseAsMaxValue(value, attribute.SettingName, propertyInfo.PropertyType, out var maxValue))
			{
				value = maxValue.ToString();
			}
			object value2 = propertyInfo.PropertyType.InvokeMember("Parse", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod, null, null, new object[2] { value, Definition.SettingName }, CultureInfo.CurrentCulture);
			if (settingInstance != null)
			{
				propertyInfo.SetValue(settingInstance, value2, null);
			}
		}
		catch (TargetInvocationException ex)
		{
			throw ex.InnerException;
		}
	}

	public void ValidateValue(string value)
	{
		SetValue(null, value);
	}

	public bool EqualsToDefaultValue(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return true;
		}
		return value.Equals(Definition.DefaultValue.ToString(), StringComparison.OrdinalIgnoreCase);
	}
}
