using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class MethodInfoDictionary
{
	private Dictionary<Type, Dictionary<uint, MethodInfo>> methodInfoDictionary;

	internal MethodInfoDictionary()
	{
		methodInfoDictionary = new Dictionary<Type, Dictionary<uint, MethodInfo>>();
	}

	internal bool TryGetvalue(Type type, uint methodIndex, out MethodInfo methodInfos)
	{
		methodInfos = null;
		Dictionary<uint, MethodInfo> value = null;
		if (methodInfoDictionary.TryGetValue(type, out value))
		{
			MethodInfo value2 = null;
			if (value.TryGetValue(methodIndex, out value2))
			{
				methodInfos = value2;
				return true;
			}
		}
		return false;
	}

	internal void InsertValue(Type type, uint methodIndex, MethodInfo methodInfos)
	{
		Dictionary<uint, MethodInfo> value = null;
		if (!methodInfoDictionary.TryGetValue(type, out value))
		{
			value = new Dictionary<uint, MethodInfo>();
			methodInfoDictionary.Add(type, value);
		}
		value.Add(methodIndex, methodInfos);
	}
}
