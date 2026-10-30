using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.Dynamics.Nav.Runtime;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavEventPublisherReflectionWrapper
{
	private readonly Type clrType;

	private readonly ConcurrentDictionary<Type, NavEventPublisherMethodInfo> methodInfosByScopeType = new ConcurrentDictionary<Type, NavEventPublisherMethodInfo>();

	private readonly ConcurrentDictionary<string, Type> scopeTypesByExactMatch = new ConcurrentDictionary<string, Type>();

	private readonly Lazy<Dictionary<string, Type>> scopeTypesByNavNameAttribute;

	public NavEventPublisherReflectionWrapper(Type clrType)
	{
		this.clrType = clrType ?? throw new ArgumentNullException("clrType");
		scopeTypesByNavNameAttribute = new Lazy<Dictionary<string, Type>>(BuildScopeTypeByNavNameLookup, LazyThreadSafetyMode.PublicationOnly);
	}

	public NavEventPublisherMethodInfo GetMethodInfoByScopeType(Type scopeType)
	{
		if (scopeType == null)
		{
			return null;
		}
		return methodInfosByScopeType.GetOrAdd(scopeType, (Type key) =>
		{
			string name = scopeType.Name.Substring(0, scopeType.Name.Length - "_Scope".Length);
			MethodInfo method = clrType.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			NavEventPublisherMethodInfo navEventPublisherMethodInfo = new NavEventPublisherMethodInfo(method);
			return (navEventPublisherMethodInfo.Attribute == null) ? null : navEventPublisherMethodInfo;
		});
	}

	public Type GetScopeType(string alMethodName)
	{
		return TryGetScopeTypeByExactMatch(alMethodName) ?? TryGetScopeTypeByNavNameAttribute(alMethodName);
	}

	private Type TryGetScopeTypeByExactMatch(string alMethodName)
	{
		return scopeTypesByExactMatch.GetOrAdd(alMethodName, (string s) =>
		{
			string name = alMethodName + "_Scope";
			Type nestedType = clrType.GetNestedType(name, BindingFlags.Instance | BindingFlags.NonPublic);
			return IsEventPublisherScopeType(nestedType) ? nestedType : null;
		});
	}

	private Type TryGetScopeTypeByNavNameAttribute(string alMethodName)
	{
		if (!scopeTypesByNavNameAttribute.Value.TryGetValue(alMethodName, out var value))
		{
			return null;
		}
		return value;
	}

	private Dictionary<string, Type> BuildScopeTypeByNavNameLookup()
	{
		Dictionary<string, Type> dictionary = new Dictionary<string, Type>();
		Type[] nestedTypes = clrType.GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic);
		foreach (Type type in nestedTypes)
		{
			if (IsEventPublisherScopeType(type))
			{
				NavNameAttribute customAttribute = type.GetCustomAttribute<NavNameAttribute>();
				if (customAttribute != null)
				{
					dictionary.Add(customAttribute.Name, type);
				}
			}
		}
		return dictionary;
	}

	private static bool IsEventPublisherScopeType(Type t)
	{
		if (t != null && t.IsClass && t.Name.EndsWith("_Scope", StringComparison.Ordinal) && t.BaseType != null)
		{
			return t.BaseType.GetGenericTypeDefinition() == typeof(NavEventMethodScope<>);
		}
		return false;
	}
}
