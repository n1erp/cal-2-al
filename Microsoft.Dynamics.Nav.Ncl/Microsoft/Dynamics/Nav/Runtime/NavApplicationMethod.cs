using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavApplicationMethod
{
	private static readonly MethodInfo createByRefMethodInfo = typeof(NavApplicationMethod).GetMethod("CreateTypedByRefWrapper");

	private static readonly Dictionary<Type, MethodInfo> createTypedByRefMethodInfos = new Dictionary<Type, MethodInfo>();

	private static object lockObject = new object();

	public static object InvokeMethod(ITreeObject obj, string methodName, object[] args, bool resolveHandler = true, bool throwOnNotFound = true)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		object result = null;
		NavApplicationObjectBase navApplicationObjectBase = obj as NavApplicationObjectBase;
		if (resolveHandler && navApplicationObjectBase != null)
		{
			navApplicationObjectBase.EnsureGlobalVariablesInitialized();
			obj = navApplicationObjectBase.ResolveTriggerHandler(methodName, args);
		}
		MethodInfo methodInfo = FindMethod(obj, methodName, args, out var parameters);
		if (methodInfo == null)
		{
			if (!throwOnNotFound)
			{
				return null;
			}
			if (args == null)
			{
				throw new NavNCLMissingMethodException(obj.GetType(), methodName);
			}
			throw new NavNCLMissingMethodException(obj.GetType(), string.Format(CultureInfo.CurrentCulture, Lang.MethodSignatureWithArguments, methodName, args.Length));
		}
		object[] array = ConvertClrTypesToNavTypes(obj, args, parameters);
		object value;
		try
		{
			try
			{
				value = methodInfo.Invoke(obj, array);
			}
			catch (TargetInvocationException targetInvocationException)
			{
				throw ExceptionHandler.CreateExceptionFromTargetInvocationException(targetInvocationException);
			}
		}
		catch (Exception ex) when (!(ex is NavBaseException) && !(ex is NavServerSessionBaseException))
		{
			NavEnvironment.WriteWatsonLog(ex);
			throw;
		}
		UnwrapByRefArguments(array, args, parameters);
		if (methodInfo.ReturnType != typeof(void))
		{
			result = NavValue.CreateNavValueFromObject(NavValueMetadata.CalcMetadataFromObject(value), value).ClientObject;
		}
		return result;
	}

	internal static MethodInfo FindMethod(object obj, string methodName, object[] args, out ParameterInfo[] parameters)
	{
		if (string.IsNullOrWhiteSpace(methodName))
		{
			parameters = null;
			return null;
		}
		int argsCount = args?.Length ?? 0;
		Type[] types = args?.Select((object arg) => arg?.GetType() ?? typeof(object)).ToArray() ?? new Type[0];
		MethodInfo method = obj.GetType().GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, types, null);
		if (method != null)
		{
			parameters = method.GetParameters();
			return method;
		}
		MethodInfo[] array = (from methodInfo in obj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod)
			where methodInfo.Name.Equals(methodName, StringComparison.CurrentCultureIgnoreCase) && methodInfo.GetParameters().Length == argsCount
			select methodInfo).ToArray();
		if (array.Length != 0)
		{
			bool[] argsFromJson = DeserializeMethodArguments(args, argsCount, array[0]);
			for (int num = 0; num < array.Length; num++)
			{
				parameters = array[num].GetParameters();
				if (ConvertClrTypesToMethodParameterTypes(obj, args, argsFromJson, argsCount, parameters))
				{
					return array[num];
				}
			}
		}
		parameters = null;
		return null;
	}

	private static bool[] DeserializeMethodArguments(object[] args, int argsCount, MethodInfo methodInfo)
	{
		bool[] array = new bool[argsCount];
		for (int i = 0; i < argsCount; i++)
		{
			if (args[i] is SerializableObjectContract serializedObject)
			{
				args[i] = NavAutomationHelper.GetSerializedObject(serializedObject);
			}
			if (args[i] is JsonSerializableObjectContract jsonSerializedObject)
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length > i)
				{
					args[i] = DeserializeJsonSerializedObject(jsonSerializedObject, parameters[i]);
					array[i] = true;
				}
			}
		}
		return array;
	}

	private static bool ConvertClrTypesToMethodParameterTypes(object parent, object[] args, bool[] argsFromJson, int argsCount, ParameterInfo[] parameters)
	{
		for (int i = 0; i < argsCount; i++)
		{
			Type type = (IsByRef(parameters[i].ParameterType) ? GetByRefTypeArgument(parameters[i].ParameterType) : parameters[i].ParameterType);
			if (args[i] == null)
			{
				if (type.IsValueType)
				{
					throw new ArgumentNullException(parameters[i].Name);
				}
				INavValueMetadata navValueMetadata = NavValueMetadata.DefaultMetadata(NavNclTypeHelper.GetNavTypeFromNclType(NavNclTypeHelper.GetNclTypeFromType(type, forDotNet: true)));
				if (navValueMetadata.NclType == NavNclType.NavDotNet)
				{
					args[i] = new NavDotNet((ITreeObject)parent, null, runOnClient: false);
				}
				else
				{
					args[i] = NavValue.CreateNavValueFromObject(navValueMetadata, null);
				}
				continue;
			}
			Type type2 = args[i].GetType();
			if (args[i] is NavRecord || args[i] is NavRecordHandle)
			{
				if (!type2.Equals(type) && !type2.IsSubclassOf(type) && !type2.GetInterfaces().Contains(type))
				{
					return false;
				}
				continue;
			}
			Type type3 = CommonTypeInformation.ResolveClrType(NavNclTypeHelper.GetNavTypeFromType(type));
			if ((type3 == typeof(decimal) && (type2 == typeof(Decimal18) || type2 == typeof(decimal))) || (type == typeof(NavDotNet) && (type2.IsSerializable || argsFromJson[i])) || type == typeof(NavVariant) || type2.Equals(type3))
			{
				continue;
			}
			if (argsFromJson[i])
			{
				try
				{
					args[i] = Convert.ChangeType(args[i], type3, NavCurrentThread.Session.WindowsCulture);
				}
				catch (InvalidCastException)
				{
					goto IL_017b;
				}
				catch (FormatException)
				{
					goto IL_017b;
				}
				catch (OverflowException)
				{
					goto IL_017b;
				}
				continue;
			}
			goto IL_017b;
			IL_017b:
			return false;
		}
		return true;
	}

	private static object DeserializeJsonSerializedObject(JsonSerializableObjectContract jsonSerializedObject, ParameterInfo parameterInfo)
	{
		Type type = null;
		NavDotNetTypeInfoAttribute navDotNetTypeInfoAttribute = null;
		object[] customAttributes = parameterInfo.GetCustomAttributes(typeof(NavDotNetTypeInfoAttribute), inherit: false);
		if (customAttributes.Length == 1)
		{
			navDotNetTypeInfoAttribute = (NavDotNetTypeInfoAttribute)customAttributes[0];
			type = NavAutomationHelper.GetTypeFromAssembly(navDotNetTypeInfoAttribute.AssemblyName, navDotNetTypeInfoAttribute.TypeName);
		}
		try
		{
			return jsonSerializedObject.Deserialize(type);
		}
		catch (JsonSerializationException innerException)
		{
			throw NavNCLDotNetCreateException.CreateSerializedObject(CultureInfo.CurrentUICulture, (navDotNetTypeInfoAttribute != null) ? navDotNetTypeInfoAttribute.AssemblyName : string.Empty, (navDotNetTypeInfoAttribute != null) ? navDotNetTypeInfoAttribute.TypeName : string.Empty, innerException);
		}
		catch (JsonReaderException innerException2)
		{
			throw NavNCLDotNetCreateException.CreateSerializedObject(CultureInfo.CurrentUICulture, (navDotNetTypeInfoAttribute != null) ? navDotNetTypeInfoAttribute.AssemblyName : string.Empty, (navDotNetTypeInfoAttribute != null) ? navDotNetTypeInfoAttribute.TypeName : string.Empty, innerException2);
		}
	}

	private static object[] ConvertClrTypesToNavTypes(ITreeObject parent, object[] args, ParameterInfo[] parameters)
	{
		if (args == null)
		{
			return new object[0];
		}
		object[] array = new object[args.Length];
		int i = 0;
		for (int num = args.Length; i < num; i++)
		{
			if (args[i] == null || args[i] is NavRecord)
			{
				array[i] = args[i];
				continue;
			}
			bool flag = IsByRef(parameters[i].ParameterType);
			Type type = (flag ? GetByRefTypeArgument(parameters[i].ParameterType) : parameters[i].ParameterType);
			if (type == typeof(NavDotNet))
			{
				array[i] = new NavDotNet(parent, args[i], runOnClient: false, suppressDispose: true);
			}
			else if (type == typeof(NavVariant))
			{
				NavType navTypeFromType = NavNclTypeHelper.GetNavTypeFromType(args[i].GetType(), forDotNet: true);
				if (navTypeFromType == NavType.DotNet)
				{
					array[i] = new NavVariant(parent, new NavDotNet(parent, args[i], runOnClient: false, suppressDispose: true));
				}
				else
				{
					array[i] = new NavVariant(parent, NavValue.CreateNavValueFromObject(NavValueMetadata.DefaultMetadata(navTypeFromType), args[i]));
				}
			}
			else
			{
				array[i] = NavValue.PrepareNavFieldForBusinessApp(NavValue.CreateNavValueFromObject(NavValueMetadata.DefaultMetadata(NavNclTypeHelper.GetNavTypeFromType(type)), args[i]));
			}
			if (flag)
			{
				array[i] = CreateByRefWrapper(array[i], type);
			}
		}
		return array;
	}

	private static void UnwrapByRefArguments(object[] navArguments, object[] clrArguments, ParameterInfo[] parameters)
	{
		if (navArguments == null || clrArguments == null)
		{
			return;
		}
		int i = 0;
		for (int num = navArguments.Length; i < num; i++)
		{
			if (navArguments[i] != null && !(navArguments[i] is NavRecord) && IsByRef(parameters[i].ParameterType))
			{
				clrArguments[i] = NavValue.GetObjectFromNavObject((navArguments[i] as IByRef).ObjectValue);
			}
		}
	}

	private static bool IsByRef(Type t)
	{
		if (t.IsGenericType)
		{
			return t.GetGenericTypeDefinition().Equals(typeof(ByRef<>));
		}
		return false;
	}

	private static Type GetByRefTypeArgument(Type t)
	{
		if (!IsByRef(t))
		{
			return null;
		}
		Type[] genericArguments = t.GetGenericArguments();
		if (genericArguments != null && genericArguments.Length != 0)
		{
			return genericArguments[0];
		}
		return null;
	}

	private static object CreateByRefWrapper(object obj, Type typeArgument)
	{
		MethodInfo value = null;
		lock (lockObject)
		{
			if (!createTypedByRefMethodInfos.TryGetValue(typeArgument, out value))
			{
				value = createByRefMethodInfo.MakeGenericMethod(typeArgument);
				if (value != null)
				{
					createTypedByRefMethodInfos.Add(typeArgument, value);
				}
			}
		}
		return value.Invoke(null, new object[1] { obj });
	}

	public static ByRef<T> CreateTypedByRefWrapper<T>(object obj)
	{
		return new ByRef<T>(() => (T)obj, (T value) =>
		{
			obj = value;
		});
	}
}
