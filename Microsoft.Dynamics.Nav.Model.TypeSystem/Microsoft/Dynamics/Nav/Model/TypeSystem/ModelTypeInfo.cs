using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Model.TypeSystem.Helpers;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public static class ModelTypeInfo
{
	private static int[] dataTypeIsInteger;

	private static object lockObject;

	private static Dictionary<Type, EnumInfo> enumInfosByTypeofEnum;

	private static EnumInfo[] enumInfosByDataType;

	private static IEnumerable<KeyValuePair<Type, DataTypes>> typesWithDataTypeAttribute;

	private static Func<string, IModelType>[] typeCreators;

	public static int[] DataTypeIsInteger
	{
		get
		{
			return dataTypeIsInteger;
		}
		set
		{
			dataTypeIsInteger = value;
		}
	}

	static ModelTypeInfo()
	{
		dataTypeIsInteger = new int[100];
		lockObject = new object();
		lock (lockObject)
		{
			BuildStaticInfo();
		}
	}

	public static EnumInfo GetEnumInfo(Type typeOfEnum)
	{
		if (enumInfosByTypeofEnum.TryGetValue(typeOfEnum, out var value))
		{
			return value;
		}
		return null;
	}

	public static EnumInfo GetEnumInfo(DataTypes dataType)
	{
		if (dataType >= DataTypes.Unknown && (int)dataType < enumInfosByDataType.Length)
		{
			return enumInfosByDataType[(int)dataType];
		}
		return null;
	}

	public static IEnumerable<EnumInfo> GetEnumInfos()
	{
		return enumInfosByTypeofEnum.Values;
	}

	public static DataTypes EnumToDataType(Type typeOfEnum)
	{
		return GetEnumInfo(typeOfEnum)?.DataType ?? DataTypes.Unknown;
	}

	public static IEnumerable<KeyValuePair<Type, DataTypes>> GetTypesWithDataTypeAttribute()
	{
		return typesWithDataTypeAttribute;
	}

	public static Func<string, IModelType> GetTypeCreator(DataTypes dataType)
	{
		if (dataType >= DataTypes.Unknown && (int)dataType < typeCreators.Length)
		{
			return typeCreators[(int)dataType];
		}
		return null;
	}

	private static void BuildStaticInfo()
	{
		if (enumInfosByTypeofEnum != null)
		{
			return;
		}
		typesWithDataTypeAttribute = (from t in typeof(DataTypeAttribute).Assembly.GetTypes()
			where Attribute.IsDefined(t, typeof(DataTypeAttribute), inherit: false)
			select new KeyValuePair<Type, DataTypes>(t, t.GetCustomAttribute<DataTypeAttribute>().DataType)).ToArray();
		enumInfosByTypeofEnum = new Dictionary<Type, EnumInfo>(128);
		List<KeyValuePair<DataTypes, Func<string, IModelType>>> list = new List<KeyValuePair<DataTypes, Func<string, IModelType>>>(128);
		foreach (KeyValuePair<Type, DataTypes> item in typesWithDataTypeAttribute)
		{
			if (item.Key.IsEnum)
			{
				Type type = typeof(EnumType<>).MakeGenericType(item.Key);
				MethodInfo method = type.GetMethod("Create", new Type[1] { typeof(string) });
				Func<string, IEnumType> func = (Func<string, IEnumType>)Delegate.CreateDelegate(typeof(Func<string, IEnumType>), method);
				EnumInfo value = new EnumInfo(item.Key, type, item.Value, func);
				enumInfosByTypeofEnum.Add(item.Key, value);
				list.Add(new KeyValuePair<DataTypes, Func<string, IModelType>>(item.Value, func));
			}
			else if (item.Key.IsClass)
			{
				MethodInfo method = item.Key.GetMethod("Create", new Type[1] { typeof(string) });
				if (method == null)
				{
					throw new InvalidDataException($"Type {item.Key} has attribute DateType, but does not have a public static Create(String) method");
				}
				Func<string, IModelType> value2 = (Func<string, IModelType>)Delegate.CreateDelegate(typeof(Func<string, IModelType>), method);
				list.Add(new KeyValuePair<DataTypes, Func<string, IModelType>>(item.Value, value2));
			}
		}
		int num = enumInfosByTypeofEnum.Values.Max((EnumInfo ei) => (int)ei.DataType);
		enumInfosByDataType = new EnumInfo[num + 1];
		enumInfosByTypeofEnum.Values.ForEach((EnumInfo ei) =>
		{
			enumInfosByDataType[(int)ei.DataType] = ei;
		});
		typeCreators = new Func<string, IModelType>[num + 1];
		list.ForEach((KeyValuePair<DataTypes, Func<string, IModelType>> kvp) =>
		{
			typeCreators[(int)kvp.Key] = kvp.Value;
		});
	}
}
