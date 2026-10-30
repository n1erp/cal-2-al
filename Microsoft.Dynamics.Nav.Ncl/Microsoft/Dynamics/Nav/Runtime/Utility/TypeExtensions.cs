using System;

namespace Microsoft.Dynamics.Nav.Runtime.Utility;

internal static class TypeExtensions
{
	public static bool IsSubClassOfGenericType(this Type toCheck, Type generic)
	{
		while (toCheck != null && toCheck != typeof(object))
		{
			Type type = (toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck);
			if (generic == type)
			{
				return true;
			}
			toCheck = toCheck.BaseType;
		}
		return false;
	}
}
