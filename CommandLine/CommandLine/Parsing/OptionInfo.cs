using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using CommandLine.Infrastructure;

namespace CommandLine.Parsing;

[DebuggerDisplay("ShortName = {ShortName}, LongName = {LongName}")]
internal sealed class OptionInfo
{
	private readonly CultureInfo _parsingCulture;

	private readonly BaseOptionAttribute _attribute;

	private readonly PropertyInfo _property;

	private readonly PropertyWriter _propertyWriter;

	private readonly bool _required;

	private readonly char? _shortName;

	private readonly string _longName;

	private readonly string _mutuallyExclusiveSet;

	private readonly object _defaultValue;

	private readonly bool _hasDefaultValue;

	public char? ShortName => _shortName;

	public string LongName => _longName;

	public string MutuallyExclusiveSet => _mutuallyExclusiveSet;

	public bool Required => _required;

	public bool IsBoolean => _property.PropertyType == typeof(bool);

	public bool IsArray => _property.PropertyType.IsArray;

	public bool IsAttributeArrayCompatible => _attribute is OptionArrayAttribute;

	public bool IsDefined { get; set; }

	public bool ReceivedValue { get; private set; }

	public bool HasBothNames
	{
		get
		{
			if (((int?)_shortName).HasValue)
			{
				return _longName != null;
			}
			return false;
		}
	}

	public bool HasParameterLessCtor { get; set; }

	public OptionInfo(BaseOptionAttribute attribute, PropertyInfo property, CultureInfo parsingCulture)
	{
		if (attribute == null)
		{
			throw new ArgumentNullException("attribute", "The attribute is mandatory.");
		}
		if (property == null)
		{
			throw new ArgumentNullException("property", "The property is mandatory.");
		}
		_required = attribute.Required;
		_shortName = attribute.ShortName;
		_longName = attribute.LongName;
		_mutuallyExclusiveSet = attribute.MutuallyExclusiveSet;
		_defaultValue = attribute.DefaultValue;
		_hasDefaultValue = attribute.HasDefaultValue;
		_attribute = attribute;
		_property = property;
		_parsingCulture = parsingCulture;
		_propertyWriter = new PropertyWriter(_property, _parsingCulture);
	}

	internal OptionInfo(char? shortName, string longName)
	{
		_shortName = shortName;
		_longName = longName;
	}

	public object GetValue(object target)
	{
		return _property.GetValue(target, null);
	}

	public object CreateInstance(object target)
	{
		try
		{
			object obj = Activator.CreateInstance(_property.PropertyType);
			_property.SetValue(target, obj, null);
			return obj;
		}
		catch (Exception innerException)
		{
			throw new ParserException("Instance defined for verb command could not be created.", innerException);
		}
	}

	public bool SetValue(string value, object options)
	{
		if (_attribute is OptionListAttribute)
		{
			return SetValueList(value, options);
		}
		if (ReflectionHelper.IsNullableType(_property.PropertyType))
		{
			return ReceivedValue = _propertyWriter.WriteNullable(value, options);
		}
		return ReceivedValue = _propertyWriter.WriteScalar(value, options);
	}

	public bool SetValue(IList<string> values, object options)
	{
		Type elementType = _property.PropertyType.GetElementType();
		Array array = Array.CreateInstance(elementType, values.Count);
		for (int i = 0; i < array.Length; i++)
		{
			try
			{
				array.SetValue(Convert.ChangeType(values[i], elementType, _parsingCulture), i);
				_property.SetValue(options, array, null);
			}
			catch (FormatException)
			{
				return false;
			}
		}
		return ReceivedValue = true;
	}

	public bool SetValue(bool value, object options)
	{
		_property.SetValue(options, value, null);
		return ReceivedValue = true;
	}

	public void SetDefault(object options)
	{
		if (_hasDefaultValue)
		{
			try
			{
				_property.SetValue(options, _defaultValue, null);
			}
			catch (Exception innerException)
			{
				throw new ParserException("Bad default value.", innerException);
			}
		}
	}

	private bool SetValueList(string value, object options)
	{
		_property.SetValue(options, new List<string>(), null);
		IList<string> list = (IList<string>)_property.GetValue(options, null);
		string[] array = value.Split(((OptionListAttribute)_attribute).Separator);
		string[] array2 = array;
		foreach (string item in array2)
		{
			list.Add(item);
		}
		return ReceivedValue = true;
	}
}
