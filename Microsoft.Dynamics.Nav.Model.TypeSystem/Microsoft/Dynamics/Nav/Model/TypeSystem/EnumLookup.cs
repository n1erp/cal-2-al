using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class EnumLookup<TEnum, TValue> : StringLookup<TValue> where TEnum : struct
{
	private TValue[] valueArray;

	private bool completeSet;

	private Func<TValue, TEnum> enumSelector;

	public bool CompleteSet => completeSet;

	public IEnumerable<KeyValuePair<TEnum, TValue>> EnumValuePairs => valueArray.Select((TValue v) => new KeyValuePair<TEnum, TValue>(enumSelector(v), v));

	public EnumLookup(IEnumerable<TValue> definitions, Func<TValue, string> stringSelector, Func<TValue, TEnum> enumSelector)
		: base(definitions, stringSelector)
	{
		this.enumSelector = enumSelector;
		valueArray = Values.OrderBy((TValue v) => enumSelector(v)).ToArray();
		_ = valueArray.LongLength;
		completeSet = !valueArray.Where((TValue e, int index) => EnumHelper<TEnum>.EnumToInt32(enumSelector(e)) != index).Any();
	}

	public bool TryGetValue(TEnum key, out TValue value)
	{
		int num = EnumHelper<TEnum>.EnumToInt32(key);
		if (completeSet)
		{
			if (num >= 0 && num < valueArray.Length)
			{
				value = valueArray[num];
				return true;
			}
		}
		else
		{
			int num2 = 0;
			int num3 = valueArray.Length - 1;
			while (num2 <= num3)
			{
				int num4 = num2 + (num3 - num2 >> 1);
				int num5 = EnumHelper<TEnum>.EnumToInt32(enumSelector(valueArray[num4])) - num;
				if (num5 == 0)
				{
					value = valueArray[num4];
					return true;
				}
				if (num5 < 0)
				{
					num2 = num4 + 1;
				}
				else
				{
					num3 = num4 - 1;
				}
			}
		}
		value = default;
		return false;
	}

	public TValue GetValue(TEnum key)
	{
		if (TryGetValue(key, out var value))
		{
			return value;
		}
		throw new InvalidOperationException($"Error finding a {typeof(TValue).Name} in a collection indexed by a {typeof(TEnum).Name} enumeration. The key {key.ToString()} did not match a value in the collection.");
	}
}
