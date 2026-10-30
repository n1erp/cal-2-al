using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Roslyn.Utilities;

internal static class CorLightup
{
	internal static class Desktop
	{
		private static class _CultureInfo
		{
			internal static readonly Type Type = typeof(CultureInfo);

			internal static readonly PropertyInfo CultureTypes = Type.GetTypeInfo().GetDeclaredProperty("CultureTypes");
		}

		private static class CultureTypes
		{
			internal const int UserCustomCulture = 8;
		}

		private static class _Directory
		{
			internal static readonly Type Type = typeof(Directory);

			internal static readonly Func<string[]> s_getLogicalDrivesOpt;

			static _Directory()
			{
				MethodInfo? declaredMethod = Type.GetTypeInfo().GetDeclaredMethod("GetLogicalDrives");
				s_getLogicalDrivesOpt = (((object)declaredMethod != null) ? ReflectionUtilities.CreateDelegate<Func<string[]>>(declaredMethod) : null);
			}
		}

		private static class _Assembly
		{
			internal static readonly Type Type = typeof(Assembly);

			internal static readonly Func<byte[], Assembly> Load_bytes = ReflectionUtilities.CreateDelegate<Func<byte[], Assembly>>(Type.GetTypeInfo().GetDeclaredMethod("Load", typeof(byte[])));

			internal static readonly Func<byte[], byte[], Assembly> Load_bytes_with_Pdb = ReflectionUtilities.CreateDelegate<Func<byte[], byte[], Assembly>>(Type.GetTypeInfo().GetDeclaredMethod("Load", typeof(byte[]), typeof(byte[])));

			internal static readonly Func<string, Assembly> LoadFile = ReflectionUtilities.CreateDelegate<Func<string, Assembly>>(Type.GetTypeInfo().GetDeclaredMethod("LoadFile", typeof(string)));

			internal static readonly Func<Assembly, string> get_Location = ReflectionUtilities.CreateDelegate<Func<Assembly, string>>(Type.GetTypeInfo().GetDeclaredMethod("get_Location"));

			internal static readonly Func<Assembly, bool> get_GlobalAssemblyCache = ReflectionUtilities.CreateDelegate<Func<Assembly, bool>>(Type.GetTypeInfo().GetDeclaredMethod("get_GlobalAssemblyCache"));
		}

		private static class _Module
		{
			internal static readonly Type Type = typeof(Module);

			internal static readonly Func<Module, Guid> get_ModuleVersionId = ReflectionUtilities.CreateDelegate<Func<Module, Guid>>(Type.GetTypeInfo().GetDeclaredMethod("get_ModuleVersionId"));
		}

		private static class _ResolveEventArgs
		{
			internal static readonly Type Type = ReflectionUtilities.TryGetType("System.ResolveEventArgs");

			internal static readonly MethodInfo get_Name = Type.GetTypeInfo().GetDeclaredMethod("get_Name");

			internal static readonly MethodInfo get_RequestingAssembly = Type.GetTypeInfo().GetDeclaredMethod("get_RequestingAssembly");
		}

		private static class _AppDomain
		{
			internal static readonly Type Type = ReflectionUtilities.TryGetType("System.AppDomain");

			internal static readonly Type ResolveEventHandlerType = ReflectionUtilities.TryGetType("System.ResolveEventHandler");

			internal static readonly MethodInfo get_CurrentDomain = Type.GetTypeInfo().GetDeclaredMethod("get_CurrentDomain");

			internal static readonly MethodInfo add_AssemblyResolve = Type.GetTypeInfo().GetDeclaredMethod("add_AssemblyResolve", ResolveEventHandlerType);

			internal static readonly MethodInfo remove_AssemblyResolve = Type.GetTypeInfo().GetDeclaredMethod("remove_AssemblyResolve", ResolveEventHandlerType);
		}

		private sealed class AssemblyResolveWrapper
		{
			private readonly Func<string, Assembly, Assembly> _handler;

			private static readonly MethodInfo s_stubInfo = typeof(AssemblyResolveWrapper).GetTypeInfo().GetDeclaredMethod("Stub");

			public AssemblyResolveWrapper(Func<string, Assembly, Assembly> handler)
			{
				_handler = handler;
			}

			private Assembly Stub(object sender, object resolveEventArgs)
			{
				string arg = (string)_ResolveEventArgs.get_Name.Invoke(resolveEventArgs, Array.Empty<object>());
				Assembly arg2 = (Assembly)_ResolveEventArgs.get_RequestingAssembly.Invoke(resolveEventArgs, Array.Empty<object>());
				return _handler(arg, arg2);
			}

			public object GetHandler()
			{
				return s_stubInfo.CreateDelegate(_AppDomain.ResolveEventHandlerType, this);
			}
		}

		internal static bool? IsUserCustomCulture(CultureInfo cultureInfo)
		{
			if ((object)_CultureInfo.CultureTypes == null)
			{
				return null;
			}
			try
			{
				return ((int)_CultureInfo.CultureTypes.GetValue(cultureInfo) & 8) != 0;
			}
			catch (Exception)
			{
				return null;
			}
		}

		internal static string[] GetLogicalDrives()
		{
			return _Directory.s_getLogicalDrivesOpt?.Invoke() ?? Array.Empty<string>();
		}

		internal static Assembly LoadAssembly(byte[] peImage)
		{
			if (_Assembly.Load_bytes == null)
			{
				throw new PlatformNotSupportedException();
			}
			return _Assembly.Load_bytes(peImage);
		}

		internal static Assembly LoadAssembly(byte[] peImage, byte[] pdbImage)
		{
			if (_Assembly.Load_bytes_with_Pdb == null)
			{
				throw new PlatformNotSupportedException();
			}
			return _Assembly.Load_bytes_with_Pdb(peImage, pdbImage);
		}

		internal static Assembly LoadAssembly(string path)
		{
			if (_Assembly.LoadFile == null)
			{
				throw new PlatformNotSupportedException();
			}
			return _Assembly.LoadFile(path);
		}

		internal static string GetAssemblyLocation(Assembly assembly)
		{
			if (_Assembly.get_Location == null)
			{
				throw new PlatformNotSupportedException();
			}
			return _Assembly.get_Location(assembly);
		}

		internal static Guid GetModuleVersionId(Module module)
		{
			return _Module.get_ModuleVersionId(module);
		}

		internal static bool IsAssemblyFromGlobalAssemblyCache(Assembly assembly)
		{
			if (_Assembly.get_GlobalAssemblyCache == null)
			{
				throw new PlatformNotSupportedException();
			}
			return _Assembly.get_GlobalAssemblyCache(assembly);
		}

		internal static object GetCurrentAppDomain()
		{
			if ((object)_AppDomain.get_CurrentDomain == null || (object)_AppDomain.add_AssemblyResolve == null || (object)_ResolveEventArgs.get_Name == null || (object)_ResolveEventArgs.get_RequestingAssembly == null)
			{
				throw new PlatformNotSupportedException();
			}
			return _AppDomain.get_CurrentDomain.Invoke(null, Array.Empty<object>());
		}

		internal static void GetOrRemoveAssemblyResolveHandler(Func<string, Assembly, Assembly> handler, MethodInfo handlerOperation)
		{
			if ((object)_AppDomain.add_AssemblyResolve == null)
			{
				throw new PlatformNotSupportedException();
			}
			object currentAppDomain = GetCurrentAppDomain();
			object handler2 = new AssemblyResolveWrapper(handler).GetHandler();
			handlerOperation.Invoke(currentAppDomain, new object[1] { handler2 });
		}

		internal static void AddAssemblyResolveHandler(Func<string, Assembly, Assembly> handler)
		{
			GetOrRemoveAssemblyResolveHandler(handler, _AppDomain.add_AssemblyResolve);
		}

		internal static void RemoveAssemblyResolveHandler(Func<string, Assembly, Assembly> handler)
		{
			GetOrRemoveAssemblyResolveHandler(handler, _AppDomain.remove_AssemblyResolve);
		}
	}
}
