using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CommandLine.Infrastructure;

namespace CommandLine.Parsing;

internal sealed class ValueMapper
{
	private readonly CultureInfo _parsingCulture;

	private readonly object _target;

	private IList<string> _valueList;

	private ValueListAttribute _valueListAttribute;

	private IList<Pair<PropertyInfo, ValueOptionAttribute>> _valueOptionAttributeList;

	private int _valueOptionIndex;

	public bool CanReceiveValues
	{
		get
		{
			if (!IsValueListDefined)
			{
				return IsValueOptionDefined;
			}
			return true;
		}
	}

	private bool IsValueListDefined => _valueListAttribute != null;

	private bool IsValueOptionDefined => _valueOptionAttributeList.Count > 0;

	public ValueMapper(object target, CultureInfo parsingCulture)
	{
		_target = target;
		_parsingCulture = parsingCulture;
		InitializeValueList();
		InitializeValueOption();
	}

	public bool MapValueItem(string item)
	{
		if (IsValueOptionDefined && _valueOptionIndex < _valueOptionAttributeList.Count)
		{
			Pair<PropertyInfo, ValueOptionAttribute> pair = _valueOptionAttributeList[_valueOptionIndex++];
			PropertyWriter propertyWriter = new PropertyWriter(pair.Left, _parsingCulture);
			if (!ReflectionHelper.IsNullableType(propertyWriter.Property.PropertyType))
			{
				return propertyWriter.WriteScalar(item, _target);
			}
			return propertyWriter.WriteNullable(item, _target);
		}
		if (IsValueListDefined)
		{
			return AddValueItem(item);
		}
		return false;
	}

	private bool AddValueItem(string item)
	{
		if (_valueListAttribute.MaximumElements == 0 || _valueList.Count == _valueListAttribute.MaximumElements)
		{
			return false;
		}
		_valueList.Add(item);
		return true;
	}

	private void InitializeValueList()
	{
		_valueListAttribute = ValueListAttribute.GetAttribute(_target);
		if (IsValueListDefined)
		{
			_valueList = ValueListAttribute.GetReference(_target);
		}
	}

	private void InitializeValueOption()
	{
		IList<Pair<PropertyInfo, ValueOptionAttribute>> list = ReflectionHelper.RetrievePropertyList<ValueOptionAttribute>(_target);
		_valueOptionAttributeList = (list.All((Pair<PropertyInfo, ValueOptionAttribute> x) => x.Right.Index == 0) ? list : list.OrderBy((Pair<PropertyInfo, ValueOptionAttribute> x) => x.Right.Index).ToList());
	}
}
