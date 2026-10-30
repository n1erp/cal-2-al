using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommandLine.Infrastructure;

internal static class ReflectionHelper
{
	internal static Assembly AssemblyFromWhichToPullInformation { get; set; }

	static ReflectionHelper()
	{
		AssemblyFromWhichToPullInformation = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
	}

	public static IList<Pair<PropertyInfo, TAttribute>> RetrievePropertyList<TAttribute>(object target) where TAttribute : Attribute
	{
		Pair<Type, object> key = new Pair<Type, object>(typeof(Pair<PropertyInfo, TAttribute>), target);
		object obj = ReflectionCache.Instance[key];
		if (obj == null)
		{
			IList<Pair<PropertyInfo, TAttribute>> list = new List<Pair<PropertyInfo, TAttribute>>();
			if (target != null)
			{
				PropertyInfo[] properties = target.GetType().GetProperties();
				PropertyInfo[] array = properties;
				foreach (PropertyInfo propertyInfo in array)
				{
					if (propertyInfo == null || !propertyInfo.CanRead || !propertyInfo.CanWrite)
					{
						continue;
					}
					MethodInfo setMethod = propertyInfo.GetSetMethod();
					if (!(setMethod == null) && !setMethod.IsStatic)
					{
						Attribute customAttribute = Attribute.GetCustomAttribute(propertyInfo, typeof(TAttribute), inherit: false);
						if (customAttribute != null)
						{
							list.Add(new Pair<PropertyInfo, TAttribute>(propertyInfo, (TAttribute)customAttribute));
						}
					}
				}
			}
			ReflectionCache.Instance[key] = list;
			return list;
		}
		return (IList<Pair<PropertyInfo, TAttribute>>)obj;
	}

	public static Pair<MethodInfo, TAttribute> RetrieveMethod<TAttribute>(object target) where TAttribute : Attribute
	{
		Pair<Type, object> key = new Pair<Type, object>(typeof(Pair<MethodInfo, TAttribute>), target);
		object obj = ReflectionCache.Instance[key];
		if (obj == null)
		{
			MethodInfo[] methods = target.GetType().GetMethods();
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo in array)
			{
				if (!methodInfo.IsStatic)
				{
					Attribute customAttribute = Attribute.GetCustomAttribute(methodInfo, typeof(TAttribute), inherit: false);
					if (customAttribute != null)
					{
						Pair<MethodInfo, TAttribute> pair = new Pair<MethodInfo, TAttribute>(methodInfo, (TAttribute)customAttribute);
						ReflectionCache.Instance[key] = pair;
						return pair;
					}
				}
			}
			return null;
		}
		return (Pair<MethodInfo, TAttribute>)obj;
	}

	public static TAttribute RetrieveMethodAttributeOnly<TAttribute>(object target) where TAttribute : Attribute
	{
		Pair<Type, object> key = new Pair<Type, object>(typeof(TAttribute), target);
		object obj = ReflectionCache.Instance[key];
		if (obj == null)
		{
			MethodInfo[] methods = target.GetType().GetMethods();
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo in array)
			{
				if (!methodInfo.IsStatic)
				{
					Attribute customAttribute = Attribute.GetCustomAttribute(methodInfo, typeof(TAttribute), inherit: false);
					if (customAttribute != null)
					{
						TAttribute val = (TAttribute)customAttribute;
						ReflectionCache.Instance[key] = val;
						return val;
					}
				}
			}
			return null;
		}
		return (TAttribute)obj;
	}

	public static IList<TAttribute> RetrievePropertyAttributeList<TAttribute>(object target) where TAttribute : Attribute
	{
		Pair<Type, object> key = new Pair<Type, object>(typeof(IList<TAttribute>), target);
		object obj = ReflectionCache.Instance[key];
		if (obj == null)
		{
			IList<TAttribute> list = new List<TAttribute>();
			PropertyInfo[] properties = target.GetType().GetProperties();
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (propertyInfo == null || !propertyInfo.CanRead || !propertyInfo.CanWrite)
				{
					continue;
				}
				MethodInfo setMethod = propertyInfo.GetSetMethod();
				if (!(setMethod == null) && !setMethod.IsStatic)
				{
					Attribute customAttribute = Attribute.GetCustomAttribute(propertyInfo, typeof(TAttribute), inherit: false);
					if (customAttribute != null)
					{
						list.Add((TAttribute)customAttribute);
					}
				}
			}
			ReflectionCache.Instance[key] = list;
			return list;
		}
		return (IList<TAttribute>)obj;
	}

	public static TAttribute GetAttribute<TAttribute>() where TAttribute : Attribute
	{
		object[] customAttributes = AssemblyFromWhichToPullInformation.GetCustomAttributes(typeof(TAttribute), inherit: false);
		if (customAttributes.Length <= 0)
		{
			return null;
		}
		return (TAttribute)customAttributes[0];
	}

	public static Pair<PropertyInfo, TAttribute> RetrieveOptionProperty<TAttribute>(object target, string uniqueName) where TAttribute : BaseOptionAttribute
	{
		Pair<Type, object> key = new Pair<Type, object>(typeof(Pair<PropertyInfo, BaseOptionAttribute>), target);
		object obj = ReflectionCache.Instance[key];
		if (obj == null)
		{
			if (target == null)
			{
				return null;
			}
			PropertyInfo[] properties = target.GetType().GetProperties();
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (propertyInfo == null || !propertyInfo.CanRead || !propertyInfo.CanWrite)
				{
					continue;
				}
				MethodInfo setMethod = propertyInfo.GetSetMethod();
				if (!(setMethod == null) && !setMethod.IsStatic)
				{
					Attribute customAttribute = Attribute.GetCustomAttribute(propertyInfo, typeof(TAttribute), inherit: false);
					TAttribute val = (TAttribute)customAttribute;
					if (val != null && string.CompareOrdinal(uniqueName, val.UniqueName) == 0)
					{
						Pair<PropertyInfo, TAttribute> pair = new Pair<PropertyInfo, TAttribute>(propertyInfo, (TAttribute)customAttribute);
						ReflectionCache.Instance[key] = pair;
						return pair;
					}
				}
			}
		}
		return (Pair<PropertyInfo, TAttribute>)obj;
	}

	public static bool IsNullableType(Type type)
	{
		if (type.IsGenericType)
		{
			return type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}
		return false;
	}
}
