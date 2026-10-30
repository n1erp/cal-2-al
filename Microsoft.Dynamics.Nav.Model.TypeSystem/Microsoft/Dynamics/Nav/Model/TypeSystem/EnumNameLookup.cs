using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public static class EnumNameLookup<TEnum> where TEnum : struct
{
	private static EnumLookup<TEnum, KeyValuePair<TEnum, string>> lookup;

	public static EnumLookup<TEnum, KeyValuePair<TEnum, string>> Lookup
	{
		get
		{
			if (lookup == null)
			{
				lookup = new EnumLookup<TEnum, KeyValuePair<TEnum, string>>(EnumHelper<TEnum>.GetKeyValuePairs<NameAttribute>(null), (KeyValuePair<TEnum, string> kvp) => kvp.Value, (KeyValuePair<TEnum, string> kvp) => kvp.Key);
			}
			return lookup;
		}
	}

	public static TEnum StringToEnum(string key)
	{
		return Lookup.GetValue(key).Key;
	}

	public static bool TryStringToEnum(string key, out TEnum value)
	{
		bool flag = Lookup.TryGetValue(key, out var value2);
		value = (flag ? value2.Key : default(TEnum));
		return flag;
	}

	public static string EnumToString(TEnum key)
	{
		return Lookup.GetValue(key).Value;
	}

	public static bool TryEnumToString(TEnum key, out string value)
	{
		bool flag = Lookup.TryGetValue(key, out var value2);
		value = (flag ? value2.Value : null);
		return flag;
	}
}
