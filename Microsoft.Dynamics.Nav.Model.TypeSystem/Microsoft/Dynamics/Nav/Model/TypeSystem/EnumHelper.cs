using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public static class EnumHelper<TEnum> where TEnum : struct
{
	public static readonly Func<TEnum, int> EnumToInt32;

	public static readonly Func<int, TEnum> Int32ToEnum;

	static EnumHelper()
	{
		if (typeof(TEnum).BaseType != typeof(Enum))
		{
			throw new InvalidOperationException($"EnumHelper only supports enums as generic type T.  {typeof(TEnum).ToString()} is not an enum type");
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
	}

	public static IEnumerable<KeyValuePair<TEnum, string>> GetKeyValuePairs()
	{
		return GetKeyValuePairs(null);
	}

	public static IEnumerable<KeyValuePair<TEnum, string>> GetKeyValuePairs(Func<TEnum, string, string> modifier)
	{
		if (modifier == null)
		{
			return InternalEnumerator((FieldInfo fi, TEnum value, string name) => name);
		}
		return InternalEnumerator((FieldInfo fi, TEnum value, string name) => modifier(value, name));
	}

	public static IEnumerable<KeyValuePair<TEnum, string>> GetKeyValuePairs<TNameAttribute>(Func<TEnum, string, string> modifier) where TNameAttribute : NameAttribute
	{
		return InternalEnumerator((FieldInfo fi, TEnum value, string name) =>
		{
			object[] customAttributes = fi.GetCustomAttributes(typeof(TNameAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				name = ((TNameAttribute)customAttributes[0]).Name;
			}
			return (modifier != null) ? modifier(value, name) : name;
		});
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
