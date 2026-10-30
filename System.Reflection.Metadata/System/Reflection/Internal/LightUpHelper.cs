using System.IO;

namespace System.Reflection.Internal;

internal static class LightUpHelper
{
	internal static Type GetType(string typeName, params string[] assemblyNames)
	{
		foreach (string text in assemblyNames)
		{
			Type type = null;
			try
			{
				type = Type.GetType(typeName + "," + text, throwOnError: false);
			}
			catch (IOException)
			{
			}
			if ((object)type != null)
			{
				return type;
			}
		}
		return null;
	}

	internal static MethodInfo GetMethod(Type type, string name, params Type[] parameterTypes)
	{
		try
		{
			return type.GetRuntimeMethod(name, parameterTypes);
		}
		catch (AmbiguousMatchException)
		{
			return null;
		}
	}
}
