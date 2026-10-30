using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.Dynamics.Nav.Types;

public static class EnumHelper<TEnum> where TEnum : struct
{
	public static readonly Func<TEnum, int> EnumToInt32;

	public static readonly Func<int, TEnum> Int32ToEnum;

	private static readonly int cacheArrayBase;

	private static readonly string[] cacheArray;

	private static readonly IDictionary<int, string> cacheDictionary;

	static EnumHelper()
	{
		if (typeof(TEnum).BaseType != typeof(Enum))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "EnumHelper only supports enums as generic type T. {0} is not an enum type.", typeof(TEnum)));
		}
		DynamicMethod dynamicMethod = new DynamicMethod("__EnumToInt32", typeof(int), new Type[1] { typeof(TEnum) });
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Ret);
		EnumToInt32 = (Func<TEnum, int>)dynamicMethod.CreateDelegate(typeof(Func<TEnum, int>));
		DynamicMethod dynamicMethod2 = new DynamicMethod("__Int32ToEnum", typeof(TEnum), new Type[1] { typeof(int) });
		ILGenerator iLGenerator2 = dynamicMethod2.GetILGenerator();
		iLGenerator2.Emit(OpCodes.Ldarg_0);
		iLGenerator2.Emit(OpCodes.Ret);
		Int32ToEnum = (Func<int, TEnum>)dynamicMethod2.CreateDelegate(typeof(Func<int, TEnum>));
		List<KeyValuePair<int, string>> list = (from kvp in (from kvp in GetKeyValuePairs()
				select new KeyValuePair<int, string>(EnumToInt32(kvp.Key), kvp.Value)).DistinctBy((KeyValuePair<int, string> kvp) => kvp.Key)
			orderby kvp.Key
			select kvp).ToList();
		if (list.Count <= 0)
		{
			return;
		}
		int key = list[0].Key;
		int num = list[list.Count - 1].Key - key + 1;
		if (num <= 100 || num <= list.Count + num / 4)
		{
			cacheArrayBase = key;
			cacheArray = new string[num];
			list.ForEach((KeyValuePair<int, string> kvp) =>
			{
				cacheArray[kvp.Key - cacheArrayBase] = kvp.Value;
			});
		}
		else
		{
			cacheDictionary = list.ToDictionary((KeyValuePair<int, string> kvp) => kvp.Key, (KeyValuePair<int, string> kvp) => kvp.Value);
		}
	}

	public static string DefinedEnumToString(TEnum value)
	{
		int num = EnumToInt32(value);
		string value2;
		if (cacheArray != null)
		{
			int num2 = num - cacheArrayBase;
			if (num2 >= 0 && num2 < cacheArray.Length)
			{
				return cacheArray[num2];
			}
		}
		else if (cacheDictionary != null && cacheDictionary.TryGetValue(num, out value2))
		{
			return value2;
		}
		return null;
	}

	public static string EnumToString(TEnum value)
	{
		return DefinedEnumToString(value) ?? EnumToInt32(value).ToString(CultureInfo.InvariantCulture);
	}

	public static IEnumerable<KeyValuePair<TEnum, string>> GetKeyValuePairs(Func<TEnum, string, string> modifier = null)
	{
		if (modifier == null)
		{
			return InternalEnumerator((FieldInfo fi, TEnum value, string name) => name);
		}
		return InternalEnumerator((FieldInfo fi, TEnum value, string name) => modifier(value, name));
	}

	private static IEnumerable<KeyValuePair<TEnum, string>> InternalEnumerator(Func<FieldInfo, TEnum, string, string> modifier)
	{
		foreach (FieldInfo item in from fi in typeof(TEnum).GetFields()
			where fi.FieldType == typeof(TEnum)
			select fi)
		{
			TEnum val = (TEnum)item.GetRawConstantValue();
			string text = modifier(item, val, item.Name);
			if (text != null)
			{
				yield return new KeyValuePair<TEnum, string>(val, text);
			}
		}
	}
}
