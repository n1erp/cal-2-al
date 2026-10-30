using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class UICultureUtilities
{
	private const string CurrentUICultureName = "CurrentUICulture";

	private static readonly Action<CultureInfo> setCurrentUICulture;

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
			PropertyInfo declaredProperty = type.GetTypeInfo().GetDeclaredProperty("CurrentUICulture");
			if ((object)declaredProperty == null)
			{
				setter = null;
				return false;
			}
			MethodInfo setMethod = declaredProperty.SetMethod;
			if ((object)setMethod == null || !setMethod.IsStatic || setMethod.ContainsGenericParameters || setMethod.ReturnType != typeof(void))
			{
				setter = null;
				return false;
			}
			ParameterInfo[] parameters = setMethod.GetParameters();
			if (parameters.Length != 1 || parameters[0].ParameterType != typeof(CultureInfo))
			{
				setter = null;
				return false;
			}
			setter = (Action<CultureInfo>)setMethod.CreateDelegate(typeof(Action<CultureInfo>));
			return true;
		}
		catch
		{
			setter = null;
			return false;
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
			System.Reflection.TypeInfo typeInfo = type.GetTypeInfo();
			MethodInfo currentThreadGetter = typeInfo.GetDeclaredProperty("CurrentThread")?.GetMethod;
			if ((object)currentThreadGetter == null || !currentThreadGetter.IsStatic || currentThreadGetter.ContainsGenericParameters || currentThreadGetter.ReturnType != type || currentThreadGetter.GetParameters().Length != 0)
			{
				setter = null;
				return false;
			}
			MethodInfo currentUICultureSetter = typeInfo.GetDeclaredProperty("CurrentUICulture")?.SetMethod;
			if ((object)currentUICultureSetter == null || currentUICultureSetter.IsStatic || currentUICultureSetter.ContainsGenericParameters || currentUICultureSetter.ReturnType != typeof(void))
			{
				setter = null;
				return false;
			}
			ParameterInfo[] parameters = currentUICultureSetter.GetParameters();
			if (parameters.Length != 1 || parameters[0].ParameterType != typeof(CultureInfo))
			{
				setter = null;
				return false;
			}
			setter = (CultureInfo culture) =>
			{
				MethodInfo methodInfo = currentUICultureSetter;
				object obj2 = currentThreadGetter.Invoke(null, null);
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

	[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
	static UICultureUtilities()
	{
		if (!TryGetCurrentUICultureSetter(out setCurrentUICulture) && !TryGetCurrentThreadUICultureSetter(out setCurrentUICulture))
		{
			setCurrentUICulture = null;
		}
	}

	public static Action WithCurrentUICulture(Action action)
	{
		if (setCurrentUICulture == null)
		{
			return action;
		}
		CultureInfo savedCulture = CultureInfo.CurrentUICulture;
		return () =>
		{
			CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
			if (currentUICulture != savedCulture)
			{
				setCurrentUICulture(savedCulture);
				try
				{
					action();
					return;
				}
				finally
				{
					setCurrentUICulture(currentUICulture);
				}
			}
			action();
		};
	}

	public static Action<T> WithCurrentUICulture<T>(Action<T> action)
	{
		if (setCurrentUICulture == null)
		{
			return action;
		}
		CultureInfo savedCulture = CultureInfo.CurrentUICulture;
		return (T param) =>
		{
			CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
			if (currentUICulture != savedCulture)
			{
				setCurrentUICulture(savedCulture);
				try
				{
					action(param);
					return;
				}
				finally
				{
					setCurrentUICulture(currentUICulture);
				}
			}
			action(param);
		};
	}

	public static Func<T> WithCurrentUICulture<T>(Func<T> func)
	{
		if (setCurrentUICulture == null)
		{
			return func;
		}
		CultureInfo savedCulture = CultureInfo.CurrentUICulture;
		return () =>
		{
			CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
			if (currentUICulture != savedCulture)
			{
				setCurrentUICulture(savedCulture);
				try
				{
					return func();
				}
				finally
				{
					setCurrentUICulture(currentUICulture);
				}
			}
			return func();
		};
	}
}
