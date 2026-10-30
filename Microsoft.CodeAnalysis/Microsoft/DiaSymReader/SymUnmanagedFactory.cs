using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.DiaSymReader;

internal static class SymUnmanagedFactory
{
	private delegate void NativeFactory(ref Guid id, [MarshalAs(UnmanagedType.IUnknown)] out object instance);

	private const string AlternateLoadPathEnvironmentVariableName = "MICROSOFT_DIASYMREADER_NATIVE_ALT_LOAD_PATH";

	private const string LegacyDiaSymReaderModuleName = "diasymreader.dll";

	private const string DiaSymReaderModuleName32 = "Microsoft.DiaSymReader.Native.x86.dll";

	private const string DiaSymReaderModuleName64 = "Microsoft.DiaSymReader.Native.amd64.dll";

	private const string CreateSymReaderFactoryName = "CreateSymReader";

	private const string CreateSymWriterFactoryName = "CreateSymWriter";

	private const string SymWriterClsid = "0AE2DEB0-F901-478b-BB9F-881EE8066788";

	private const string SymReaderClsid = "0A3976C5-4529-4ef8-B0B0-42EED37082CD";

	private static Type s_lazySymReaderComType;

	private static Type s_lazySymWriterComType;

	private static Lazy<Func<string, string>> s_lazyGetEnvironmentVariable = new Lazy<Func<string, string>>(() =>
	{
		try
		{
			foreach (MethodInfo declaredMethod in typeof(Environment).GetTypeInfo().GetDeclaredMethods("GetEnvironmentVariable"))
			{
				ParameterInfo[] parameters = declaredMethod.GetParameters();
				if (parameters.Length == 1 && (object)parameters[0].ParameterType == typeof(string))
				{
					return (Func<string, string>)declaredMethod.CreateDelegate(typeof(Func<string, string>));
				}
			}
		}
		catch
		{
		}
		return (Func<string, string>)null;
	});

	internal static string DiaSymReaderModuleName
	{
		get
		{
			if (IntPtr.Size != 4)
			{
				return "Microsoft.DiaSymReader.Native.amd64.dll";
			}
			return "Microsoft.DiaSymReader.Native.x86.dll";
		}
	}

	[DllImport("Microsoft.DiaSymReader.Native.x86.dll", EntryPoint = "CreateSymReader")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory)]
	private static extern void CreateSymReader32(ref Guid id, [MarshalAs(UnmanagedType.IUnknown)] out object symReader);

	[DllImport("Microsoft.DiaSymReader.Native.amd64.dll", EntryPoint = "CreateSymReader")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory)]
	private static extern void CreateSymReader64(ref Guid id, [MarshalAs(UnmanagedType.IUnknown)] out object symReader);

	[DllImport("Microsoft.DiaSymReader.Native.x86.dll", EntryPoint = "CreateSymWriter")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory)]
	private static extern void CreateSymWriter32(ref Guid id, [MarshalAs(UnmanagedType.IUnknown)] out object symWriter);

	[DllImport("Microsoft.DiaSymReader.Native.amd64.dll", EntryPoint = "CreateSymWriter")]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory)]
	private static extern void CreateSymWriter64(ref Guid id, [MarshalAs(UnmanagedType.IUnknown)] out object symWriter);

	[DllImport("kernel32")]
	private static extern IntPtr LoadLibrary(string path);

	[DllImport("kernel32")]
	private static extern bool FreeLibrary(IntPtr hModule);

	[DllImport("kernel32")]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

	internal static string GetEnvironmentVariable(string name)
	{
		try
		{
			return s_lazyGetEnvironmentVariable.Value?.Invoke(name);
		}
		catch
		{
			return null;
		}
	}

	private static object TryLoadFromAlternativePath(Guid clsid, string factoryName)
	{
		string environmentVariable = GetEnvironmentVariable("MICROSOFT_DIASYMREADER_NATIVE_ALT_LOAD_PATH");
		if (string.IsNullOrEmpty(environmentVariable))
		{
			return null;
		}
		IntPtr intPtr = LoadLibrary(Path.Combine(environmentVariable, DiaSymReaderModuleName));
		if (intPtr == IntPtr.Zero)
		{
			Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
		}
		object instance = null;
		try
		{
			IntPtr procAddress = GetProcAddress(intPtr, factoryName);
			if (procAddress == IntPtr.Zero)
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
			Marshal.GetDelegateForFunctionPointer<NativeFactory>(procAddress)(ref clsid, out instance);
		}
		finally
		{
			if (instance == null && !FreeLibrary(intPtr))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}
		return instance;
	}

	private static Type GetComTypeType(ref Type lazyType, Guid clsid)
	{
		if ((object)lazyType == null)
		{
			lazyType = Marshal.GetTypeFromCLSID(clsid);
		}
		return lazyType;
	}

	internal static object CreateObject(bool createReader, bool useAlternativeLoadPath, bool useComRegistry, out string moduleName, out Exception loadException)
	{
		object symWriter = null;
		loadException = null;
		moduleName = null;
		Guid id = new Guid(createReader ? "0A3976C5-4529-4ef8-B0B0-42EED37082CD" : "0AE2DEB0-F901-478b-BB9F-881EE8066788");
		try
		{
			try
			{
				if (IntPtr.Size == 4)
				{
					if (createReader)
					{
						CreateSymReader32(ref id, out symWriter);
					}
					else
					{
						CreateSymWriter32(ref id, out symWriter);
					}
				}
				else if (createReader)
				{
					CreateSymReader64(ref id, out symWriter);
				}
				else
				{
					CreateSymWriter64(ref id, out symWriter);
				}
			}
			catch (DllNotFoundException ex) when (useAlternativeLoadPath)
			{
				symWriter = TryLoadFromAlternativePath(id, createReader ? "CreateSymReader" : "CreateSymWriter");
				if (symWriter == null)
				{
					loadException = ex;
				}
			}
		}
		catch (Exception ex2)
		{
			loadException = ex2;
			symWriter = null;
		}
		if (symWriter != null)
		{
			moduleName = DiaSymReaderModuleName;
		}
		else if (useComRegistry)
		{
			try
			{
				symWriter = Activator.CreateInstance(createReader ? GetComTypeType(ref s_lazySymReaderComType, id) : GetComTypeType(ref s_lazySymWriterComType, id));
				moduleName = "diasymreader.dll";
			}
			catch (Exception ex3)
			{
				loadException = ex3;
				symWriter = null;
			}
		}
		return symWriter;
	}
}
