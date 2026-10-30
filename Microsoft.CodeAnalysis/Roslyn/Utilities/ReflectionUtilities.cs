using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Roslyn.Utilities;

internal static class ReflectionUtilities
{
	private static readonly Type Missing = typeof(void);

	public static Type TryGetType(string assemblyQualifiedName)
	{
		try
		{
			return Type.GetType(assemblyQualifiedName, throwOnError: false);
		}
		catch
		{
			return null;
		}
	}

	public static Type TryGetType(ref Type lazyType, string assemblyQualifiedName)
	{
		if ((object)lazyType == null)
		{
			lazyType = TryGetType(assemblyQualifiedName) ?? Missing;
		}
		if ((object)lazyType != Missing)
		{
			return lazyType;
		}
		return null;
	}

	public static Type GetTypeFromEither(string contractName, string desktopName)
	{
		Type type = TryGetType(contractName);
		if ((object)type == null)
		{
			type = TryGetType(desktopName);
		}
		return type;
	}

	public static Type GetTypeFromEither(ref Type lazyType, string contractName, string desktopName)
	{
		if ((object)lazyType == null)
		{
			lazyType = GetTypeFromEither(contractName, desktopName) ?? Missing;
		}
		if ((object)lazyType != Missing)
		{
			return lazyType;
		}
		return null;
	}

	public static T FindItem<T>(IEnumerable<T> collection, params Type[] paramTypes) where T : MethodBase
	{
		foreach (T item in collection)
		{
			ParameterInfo[] parameters = item.GetParameters();
			if (parameters.Length != paramTypes.Length)
			{
				continue;
			}
			bool flag = true;
			for (int i = 0; i < paramTypes.Length; i++)
			{
				if ((object)parameters[i].ParameterType != paramTypes[i])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return item;
			}
		}
		return null;
	}

	internal static MethodInfo GetDeclaredMethod(this TypeInfo typeInfo, string name, params Type[] paramTypes)
	{
		return FindItem(typeInfo.GetDeclaredMethods(name), paramTypes);
	}

	internal static ConstructorInfo GetDeclaredConstructor(this TypeInfo typeInfo, params Type[] paramTypes)
	{
		return FindItem(typeInfo.DeclaredConstructors, paramTypes);
	}

	public static T CreateDelegate<T>(this MethodInfo methodInfo)
	{
		if ((object)methodInfo == null)
		{
			return default;
		}
		return (T)(object)methodInfo.CreateDelegate(typeof(T));
	}

	public static T InvokeConstructor<T>(this ConstructorInfo constructorInfo, params object[] args)
	{
		if ((object)constructorInfo == null)
		{
			return default;
		}
		try
		{
			return (T)constructorInfo.Invoke(args);
		}
		catch (TargetInvocationException ex)
		{
			ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			return default;
		}
	}

	public static object InvokeConstructor(this ConstructorInfo constructorInfo, params object[] args)
	{
		return constructorInfo.InvokeConstructor<object>(args);
	}

	public static T Invoke<T>(this MethodInfo methodInfo, object obj, params object[] args)
	{
		return (T)methodInfo.Invoke(obj, args);
	}
}
