using System;
using System.Reflection;

namespace Roslyn.Utilities;

internal static class DesktopShim
{
	internal static class FileNotFoundException
	{
		internal static readonly Type Type = ReflectionUtilities.TryGetType("System.IO.FileNotFoundException, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

		private static PropertyInfo s_fusionLog = Type?.GetTypeInfo().GetDeclaredProperty("FusionLog");

		internal static string TryGetFusionLog(object obj)
		{
			return s_fusionLog.GetValue(obj) as string;
		}
	}
}
