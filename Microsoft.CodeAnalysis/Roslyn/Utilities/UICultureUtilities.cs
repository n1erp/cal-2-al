using System;
using System.Globalization;
using System.Reflection;

namespace Roslyn.Utilities;

internal static class UICultureUtilities
{
	private const string currentUICultureName = "CurrentUICulture";

	private static readonly Action<CultureInfo> s_setCurrentUICulture;

	private static bool TryGetCurrentUICultureSetter(out Action<CultureInfo> setter)
	{
		try
		{
			Type type = Type.GetType("System.Globalization.CultureInfo, System.Globalization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a") ?? typeof(object).GetTypeInfo().Assembly.GetType("System.Globalization.CultureInfo");
			if ((object)type == null)
			{
				setter = null;
				return false;
			}
			MethodInfo methodInfo = type.GetTypeInfo().GetDeclaredProperty("CurrentUICulture")?.SetMethod;
			if ((object)methodInfo == null || !methodInfo.IsStatic || methodInfo.ContainsGenericParameters || (object)methodInfo.ReturnType != typeof(void))
			{
				setter = null;
				return false;
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length != 1 || (object)parameters[0].ParameterType != typeof(CultureInfo))
			{
				setter = null;
				return false;
			}
			setter = (Action<CultureInfo>)methodInfo.CreateDelegate(typeof(Action<CultureInfo>));
			return true;
		}
		catch
		{
			setter = null;
			return false;
		}
	}

	private static bool TryGetCurrentThreadUICultureSetter(out Action<CultureInfo> setter)
	{
		try
		{
			Type type = typeof(object).GetTypeInfo().Assembly.GetType("System.Threading.Thread");
			if ((object)type == null)
			{
				setter = null;
				return false;
			}
			TypeInfo typeInfo = type.GetTypeInfo();
			MethodInfo currentThreadGetter = typeInfo.GetDeclaredProperty("CurrentThread")?.GetMethod;
			if ((object)currentThreadGetter == null || !currentThreadGetter.IsStatic || currentThreadGetter.ContainsGenericParameters || (object)currentThreadGetter.ReturnType != type || currentThreadGetter.GetParameters().Length != 0)
			{
				setter = null;
				return false;
			}
			MethodInfo currentUICultureSetter = typeInfo.GetDeclaredProperty("CurrentUICulture")?.SetMethod;
			if ((object)currentUICultureSetter == null || currentUICultureSetter.IsStatic || currentUICultureSetter.ContainsGenericParameters || (object)currentUICultureSetter.ReturnType != typeof(void))
			{
				setter = null;
				return false;
			}
			ParameterInfo[] parameters = currentUICultureSetter.GetParameters();
			if (parameters.Length != 1 || (object)parameters[0].ParameterType != typeof(CultureInfo))
			{
				setter = null;
				return false;
			}
			setter = (CultureInfo culture) =>
			{
				MethodInfo methodInfo = currentUICultureSetter;
				object? obj2 = currentThreadGetter.Invoke(null, null);
				object[] parameters2 = new CultureInfo[1] { culture };
				methodInfo.Invoke(obj2, parameters2);
			};
			return true;
		}
		catch
		{
			setter = null;
			return false;
		}
	}

	static UICultureUtilities()
	{
		if (!TryGetCurrentUICultureSetter(out s_setCurrentUICulture) && !TryGetCurrentThreadUICultureSetter(out s_setCurrentUICulture))
		{
			s_setCurrentUICulture = null;
		}
	}

	public static Action WithCurrentUICulture(Action action)
	{
		if (s_setCurrentUICulture == null)
		{
			return action;
		}
		CultureInfo savedCulture = CultureInfo.CurrentUICulture;
		return () =>
		{
			CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
			if (currentUICulture != savedCulture)
			{
				s_setCurrentUICulture(savedCulture);
				try
				{
					action();
					return;
				}
				finally
				{
					s_setCurrentUICulture(currentUICulture);
				}
			}
			action();
		};
	}

	public static Action<T> WithCurrentUICulture<T>(Action<T> action)
	{
		if (s_setCurrentUICulture == null)
		{
			return action;
		}
		CultureInfo savedCulture = CultureInfo.CurrentUICulture;
		return (T param) =>
		{
			CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
			if (currentUICulture != savedCulture)
			{
				s_setCurrentUICulture(savedCulture);
				try
				{
					action(param);
					return;
				}
				finally
				{
					s_setCurrentUICulture(currentUICulture);
				}
			}
			action(param);
		};
	}

	public static Func<T> WithCurrentUICulture<T>(Func<T> func)
	{
		if (s_setCurrentUICulture == null)
		{
			return func;
		}
		CultureInfo savedCulture = CultureInfo.CurrentUICulture;
		return () =>
		{
			CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
			if (currentUICulture != savedCulture)
			{
				s_setCurrentUICulture(savedCulture);
				try
				{
					return func();
				}
				finally
				{
					s_setCurrentUICulture(currentUICulture);
				}
			}
			return func();
		};
	}
}
