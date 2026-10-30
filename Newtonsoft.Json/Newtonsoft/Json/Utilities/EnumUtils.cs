using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Utilities;

internal static class EnumUtils
{
	private static readonly ThreadSafeStore<Type, BidirectionalDictionary<string, string>> EnumMemberNamesPerType = new ThreadSafeStore<Type, BidirectionalDictionary<string, string>>(InitializeEnumType);

	private static BidirectionalDictionary<string, string> InitializeEnumType(Type type)
	{
		BidirectionalDictionary<string, string> bidirectionalDictionary = new BidirectionalDictionary<string, string>(StringComparer.Ordinal, StringComparer.Ordinal);
		FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			string name = fieldInfo.Name;
			string text = (from EnumMemberAttribute a in fieldInfo.GetCustomAttributes(typeof(EnumMemberAttribute), inherit: true)
				select a.Value).SingleOrDefault() ?? fieldInfo.Name;
			if (bidirectionalDictionary.TryGetBySecond(text, out var _))
			{
				throw new InvalidOperationException("Enum name '{0}' already exists on enum '{1}'.".FormatWith(CultureInfo.InvariantCulture, text, type.Name));
			}
			bidirectionalDictionary.Set(name, text);
		}
		return bidirectionalDictionary;
	}

	public static IList<T> GetFlagsValues<T>(T value) where T : struct
	{
		Type typeFromHandle = typeof(T);
		if (!typeFromHandle.IsDefined(typeof(FlagsAttribute), inherit: false))
		{
			throw new ArgumentException("Enum type {0} is not a set of flags.".FormatWith(CultureInfo.InvariantCulture, typeFromHandle));
		}
		Type underlyingType = Enum.GetUnderlyingType(value.GetType());
		ulong num = Convert.ToUInt64(value, CultureInfo.InvariantCulture);
		IList<EnumValue<ulong>> namesAndValues = GetNamesAndValues<T>();
		IList<T> list = new List<T>();
		foreach (EnumValue<ulong> item in namesAndValues)
		{
			if ((num & item.Value) == item.Value && item.Value != 0L)
			{
				list.Add((T)Convert.ChangeType(item.Value, underlyingType, CultureInfo.CurrentCulture));
			}
		}
		if (list.Count == 0 && namesAndValues.SingleOrDefault((EnumValue<ulong> v) => v.Value == 0) != null)
		{
			list.Add(default);
		}
		return list;
	}

	public static IList<EnumValue<ulong>> GetNamesAndValues<T>() where T : struct
	{
		return GetNamesAndValues<ulong>(typeof(T));
	}

	public static IList<EnumValue<TUnderlyingType>> GetNamesAndValues<TUnderlyingType>(Type enumType) where TUnderlyingType : struct
	{
		if (enumType == null)
		{
			throw new ArgumentNullException("enumType");
		}
		if (!enumType.IsEnum())
		{
			throw new ArgumentException("Type {0} is not an enum.".FormatWith(CultureInfo.InvariantCulture, enumType.Name), "enumType");
		}
		IList<object> values = GetValues(enumType);
		IList<string> names = GetNames(enumType);
		IList<EnumValue<TUnderlyingType>> list = new List<EnumValue<TUnderlyingType>>();
		for (int i = 0; i < values.Count; i++)
		{
			try
			{
				list.Add(new EnumValue<TUnderlyingType>(names[i], (TUnderlyingType)Convert.ChangeType(values[i], typeof(TUnderlyingType), CultureInfo.CurrentCulture)));
			}
			catch (OverflowException innerException)
			{
				throw new InvalidOperationException("Value from enum with the underlying type of {0} cannot be added to dictionary with a value type of {1}. Value was too large: {2}".FormatWith(CultureInfo.InvariantCulture, Enum.GetUnderlyingType(enumType), typeof(TUnderlyingType), Convert.ToUInt64(values[i], CultureInfo.InvariantCulture)), innerException);
			}
		}
		return list;
	}

	public static IList<object> GetValues(Type enumType)
	{
		if (!enumType.IsEnum())
		{
			throw new ArgumentException("Type {0} is not an enum.".FormatWith(CultureInfo.InvariantCulture, enumType.Name), "enumType");
		}
		List<object> list = new List<object>();
		FieldInfo[] fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			object value = fields[i].GetValue(enumType);
			list.Add(value);
		}
		return list;
	}

	public static IList<string> GetNames(Type enumType)
	{
		if (!enumType.IsEnum())
		{
			throw new ArgumentException("Type {0} is not an enum.".FormatWith(CultureInfo.InvariantCulture, enumType.Name), "enumType");
		}
		List<string> list = new List<string>();
		FieldInfo[] fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			list.Add(fieldInfo.Name);
		}
		return list;
	}

	public static object ParseEnumName(string enumText, bool isNullable, bool disallowValue, Type t)
	{
		if ((enumText == string.Empty) & isNullable)
		{
			return null;
		}
		BidirectionalDictionary<string, string> map = EnumMemberNamesPerType.Get(t);
		string text;
		if (TryResolvedEnumName(map, enumText, out var resolvedEnumName))
		{
			text = resolvedEnumName;
		}
		else if (enumText.IndexOf(',') != -1)
		{
			string[] array = enumText.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i].Trim();
				array[i] = (TryResolvedEnumName(map, text2, out resolvedEnumName) ? resolvedEnumName : text2);
			}
			text = string.Join(", ", array);
		}
		else
		{
			text = enumText;
			if (disallowValue)
			{
				bool flag = true;
				for (int j = 0; j < text.Length; j++)
				{
					if (!char.IsNumber(text[j]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					throw new FormatException("Integer string '{0}' is not allowed.".FormatWith(CultureInfo.InvariantCulture, enumText));
				}
			}
		}
		return Enum.Parse(t, text, ignoreCase: true);
	}

	public static string ToEnumName(Type enumType, string enumText, bool camelCaseText)
	{
		BidirectionalDictionary<string, string> bidirectionalDictionary = EnumMemberNamesPerType.Get(enumType);
		string[] array = enumText.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			bidirectionalDictionary.TryGetByFirst(text, out var second);
			second = second ?? text;
			if (camelCaseText)
			{
				second = StringUtils.ToCamelCase(second);
			}
			array[i] = second;
		}
		return string.Join(", ", array);
	}

	private static bool TryResolvedEnumName(BidirectionalDictionary<string, string> map, string enumText, out string resolvedEnumName)
	{
		if (map.TryGetBySecond(enumText, out resolvedEnumName))
		{
			return true;
		}
		resolvedEnumName = null;
		return false;
	}
}
