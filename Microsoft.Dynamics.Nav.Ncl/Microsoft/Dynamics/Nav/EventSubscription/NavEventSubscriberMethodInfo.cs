using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavEventSubscriberMethodInfo
{
	public MethodInfo RawMethodInfo { get; }

	public NavEventSubscriberAttribute Attribute { get; }

	public string ALName { get; }

	public NavEventSubscriberMethodInfo(MethodInfo rawMethodInfo)
	{
		RawMethodInfo = rawMethodInfo;
		Attribute = rawMethodInfo.GetCustomAttribute<NavEventSubscriberAttribute>();
		ALName = FindSubscriberMethodALName(rawMethodInfo);
	}

	private static string FindSubscriberMethodALName(MethodInfo subscriberMethodInfo)
	{
		Type declaringType = subscriberMethodInfo.DeclaringType;
		string name = declaringType.FullName + "+" + subscriberMethodInfo.Name + "_Scope";
		Type type = declaringType.Assembly.GetType(name, throwOnError: false);
		using (IEnumerator<NavNameAttribute> enumerator = type.GetCustomAttributes<NavNameAttribute>(inherit: false).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				NavNameAttribute current = enumerator.Current;
				return current.Name;
			}
		}
		return subscriberMethodInfo.Name;
	}
}
