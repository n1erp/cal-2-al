using System;
using System.Reflection;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class CoreClrShim
{
	internal static class AssemblyLoadContext
	{
		internal static readonly Type Type = ReflectionUtilities.TryGetType("System.Runtime.Loader.AssemblyLoadContext, System.Runtime.Loader, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
	}

	internal static class AppContext
	{
		internal static readonly Type Type = ReflectionUtilities.TryGetType("System.AppContext, System.AppContext, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");

		internal static readonly Func<string, object> GetData;

		static AppContext()
		{
			MethodInfo? declaredMethod = Type.GetTypeInfo().GetDeclaredMethod("GetData");
			GetData = (((object)declaredMethod != null) ? ReflectionUtilities.CreateDelegate<Func<string, object>>(declaredMethod) : null);
		}
	}

	internal static bool IsRunningOnCoreClr => (object)AssemblyLoadContext.Type != null;
}
