using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Types;

public static class NavAutomationHelper
{
	private static class NativeMethods
	{
		[Flags]
		internal enum CLSCTX : uint
		{
			CLSCTX_INPROC_SERVER = 1u,
			CLSCTX_INPROC_HANDLER = 2u,
			CLSCTX_LOCAL_SERVER = 4u,
			CLSCTX_INPROC_SERVER16 = 8u,
			CLSCTX_REMOTE_SERVER = 0x10u,
			CLSCTX_INPROC_HANDLER16 = 0x20u,
			CLSCTX_RESERVED1 = 0x40u,
			CLSCTX_RESERVED2 = 0x80u,
			CLSCTX_RESERVED3 = 0x100u,
			CLSCTX_RESERVED4 = 0x200u,
			CLSCTX_NO_CODE_DOWNLOAD = 0x400u,
			CLSCTX_RESERVED5 = 0x800u,
			CLSCTX_NO_CUSTOM_MARSHAL = 0x1000u,
			CLSCTX_ENABLE_CODE_DOWNLOAD = 0x2000u,
			CLSCTX_NO_FAILURE_LOG = 0x4000u,
			CLSCTX_DISABLE_AAA = 0x8000u,
			CLSCTX_ENABLE_AAA = 0x10000u,
			CLSCTX_FROM_DEFAULT_CONTEXT = 0x20000u,
			CLSCTX_INPROC = CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER,
			CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER,
			CLSCTX_ALL = CLSCTX_SERVER | CLSCTX_INPROC_HANDLER
		}

		[ComImport]
		[Guid("B196B28F-BAB4-101A-B69C-00AA00341D07")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IClassFactory2
		{
			void CreateInstance([In][MarshalAs(UnmanagedType.Interface)] object unused, [In] ref Guid refiid, [Out][MarshalAs(UnmanagedType.LPArray)] object[] ppunk);

			void LockServer(int fLock);

			void GetLicInfo([Out] tagLICINFO licInfo);

			void RequestLicKey([In][MarshalAs(UnmanagedType.U4)] int dwReserved, [Out][MarshalAs(UnmanagedType.LPArray)] string[] pBstrKey);

			void CreateInstanceLic([In][MarshalAs(UnmanagedType.Interface)] object pUnkOuter, [In][MarshalAs(UnmanagedType.Interface)] object pUnkReserved, [In] ref Guid riid, [In][MarshalAs(UnmanagedType.BStr)] string bstrKey, [MarshalAs(UnmanagedType.Interface)] out object ppVal);
		}

		[StructLayout(LayoutKind.Sequential)]
		internal sealed class tagLICINFO
		{
			[MarshalAs(UnmanagedType.U4)]
			public int CbLicInfo;

			public int FRuntimeAvailable;

			public int FLicVerified;

			public tagLICINFO()
			{
				CbLicInfo = Marshal.SizeOf(typeof(tagLICINFO));
			}
		}

		internal static Guid IID_IUnknown = new Guid("{00000000-0000-0000-C000-000000000046}");

		internal const int CLASS_E_NOTLICENSED = -2147221230;

		[DllImport("ole32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
		internal static extern string OleRegGetUserType([In] ref Guid clsid, uint dwFormOfType);

		[DllImport("ole32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
		[return: MarshalAs(UnmanagedType.Interface)]
		internal static extern object CoGetClassObject([In][MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, CLSCTX dwClsContext, IntPtr pServerInfo, [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid);

		[DllImport("oleaut32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetActiveObject(ref Guid rclsid, IntPtr pvReserved, [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
	}

	public const string Value = "#Value";

	public const string SerializedValue = "#Serialized";

	public const string IsNull = "#IsNull";

	private const string ResourcesExtension = ".resources";

	public const string AddInsFolder = "Add-Ins";

	private static readonly Dictionary<string, AssemblyName> pathNameToAssemblyNameMap = new Dictionary<string, AssemblyName>();

	private static readonly object assemblyMapLock = new object();

	private static readonly Dictionary<string, Assembly> fullNameToAssemblyMap = new Dictionary<string, Assembly>();

	private static readonly object assemblyCacheLock = new object();

	private static readonly Dictionary<string, AssemblyName> fullNameToAssemblyNameMap = new Dictionary<string, AssemblyName>();

	private static readonly object assemblyNameCacheLock = new object();

	private static readonly Dictionary<string, string> fullNameToEffectiveFullNameMap = new Dictionary<string, string>();

	private static readonly object fullNameCacheLock = new object();

	private static readonly Dictionary<string, Type> typeNameToTypeMap = new Dictionary<string, Type>();

	private static readonly object typeCacheLock = new object();

	private static readonly Dictionary<Type, bool> typeIsDisposableMap = new Dictionary<Type, bool>();

	private static readonly object typeIsDisposableLock = new object();

	private static readonly Type DisposableType = typeof(IDisposable);

	private static readonly string DisposableTypeName = DisposableType.ToString();

	private static string addInDirectory = string.Empty;

	private static string tempAddInDirectory = string.Empty;

	private const uint ScriptableHandleTag = 2147483648u;

	private static Func<string, Assembly> navAppsServerAddInAssemblyResolver;

	private static Func<string, Assembly> navAddInResolver;

	public static void RegisterNavAppsServerAddInAssemblyResolver(Func<string, Assembly> resolver)
	{
		navAppsServerAddInAssemblyResolver = resolver;
	}

	public static void RegisterServerAddInResolver(Func<string, Assembly> resolver)
	{
		navAddInResolver = resolver;
	}

	public static string GetHumanReadableName(Guid classId)
	{
		return GetHumanReadableName(classId, throwOnError: false);
	}

	public static string GetHumanReadableName(Guid classId, bool throwOnError)
	{
		try
		{
			return NativeMethods.OleRegGetUserType(ref classId, 3u);
		}
		catch (COMException)
		{
		}
		string text = classId.ToString("B").ToUpperInvariant();
		try
		{
			string text2 = Registry.GetValue("HKEY_CLASSES_ROOT\\CLSID\\" + text, string.Empty, text).ToString();
			if (string.IsNullOrEmpty(text2))
			{
				text2 = text;
			}
			return text2;
		}
		catch (NullReferenceException)
		{
			if (throwOnError)
			{
				throw NavNCLAutomationCreateException.CreateUnknownServer(CultureInfo.InvariantCulture, classId);
			}
			return text;
		}
	}

	public static object CreateAutomationObject(Guid classId, string licenseKey, bool newServer)
	{
		object ppunk = null;
		try
		{
			Type typeFromCLSID = Type.GetTypeFromCLSID(classId);
			if (typeFromCLSID == null)
			{
				throw NavNCLAutomationCreateException.CreateUnknownServer(CultureInfo.InvariantCulture, classId);
			}
			if (newServer)
			{
				ppunk = Activator.CreateInstance(typeFromCLSID);
			}
			else if (NativeMethods.GetActiveObject(ref classId, IntPtr.Zero, out ppunk) && ppunk == null)
			{
				ppunk = Activator.CreateInstance(typeFromCLSID);
			}
		}
		catch (COMException ex)
		{
			if (ex.ErrorCode != -2147221230)
			{
				throw;
			}
			try
			{
				NativeMethods.IClassFactory2 classFactory = (NativeMethods.IClassFactory2)NativeMethods.CoGetClassObject(classId, NativeMethods.CLSCTX.CLSCTX_INPROC_SERVER | NativeMethods.CLSCTX.CLSCTX_LOCAL_SERVER, IntPtr.Zero, typeof(NativeMethods.IClassFactory2).GUID);
				if (string.IsNullOrEmpty(licenseKey))
				{
					NativeMethods.tagLICINFO tagLICINFO = new NativeMethods.tagLICINFO();
					classFactory.GetLicInfo(tagLICINFO);
					if (tagLICINFO.FRuntimeAvailable != 0)
					{
						string[] array = new string[1];
						classFactory.RequestLicKey(0, array);
						licenseKey = array[0];
					}
				}
				classFactory.CreateInstanceLic(null, null, ref NativeMethods.IID_IUnknown, licenseKey, out ppunk);
			}
			catch (COMException)
			{
				throw;
			}
		}
		return ppunk;
	}

	public static object CreateDotNetObject(string assemblyFullName, string typeName, params object[] arguments)
	{
		Type typeFromAssembly = GetTypeFromAssembly(assemblyFullName, typeName);
		if (typeFromAssembly == null)
		{
			throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, typeName, null);
		}
		return CreateObjectTypeInstance(typeFromAssembly, assemblyFullName, typeName, arguments);
	}

	public static object CreateObjectTypeInstance(Type objectType, string assemblyFullName, string typeName, params object[] arguments)
	{
		Type type = objectType;
		if (objectType != null && objectType.IsGenericType)
		{
			Type[] genericArguments = objectType.GetGenericArguments();
			Type[] array = new Type[genericArguments.Length];
			for (int i = 0; i < genericArguments.Length; i++)
			{
				Type type2 = genericArguments[i];
				if (type2.IsGenericParameter)
				{
					array.SetValue(typeof(object), i);
				}
				else
				{
					array.SetValue(type2, i);
				}
			}
			type = objectType.MakeGenericType(array);
		}
		try
		{
			return Create(type, assemblyFullName, typeName, arguments);
		}
		catch (Exception exception)
		{
			NavDiagnostics.GetMostSpecificInstance.SendExceptionTag("000016I", Category.Runtime, exception, Verbosity.Warning, DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "DotNet create error, objectType={0}, instanceType={1}, assemblyName={2}, typeName={3}", objectType, type, assemblyFullName, typeName)));
			throw;
		}
	}

	public static Assembly GetAssembly(string assemblyFullName)
	{
		Assembly value = null;
		if (!string.IsNullOrEmpty(assemblyFullName))
		{
			lock (assemblyCacheLock)
			{
				if (!fullNameToAssemblyMap.TryGetValue(assemblyFullName, out value))
				{
					HookAssemblyResolverEvent();
					try
					{
						value = Assembly.Load(assemblyFullName);
						fullNameToAssemblyMap[assemblyFullName] = value;
					}
					catch (FileNotFoundException innerException)
					{
						throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, innerException);
					}
					catch (FileLoadException innerException2)
					{
						throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, innerException2);
					}
					catch (BadImageFormatException innerException3)
					{
						throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, innerException3);
					}
					catch (PathTooLongException innerException4)
					{
						throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, innerException4);
					}
				}
			}
		}
		return value;
	}

	private static void HookAssemblyResolverEvent()
	{
		AppDomain.CurrentDomain.AssemblyResolve -= ResolveEventHandler;
		AppDomain.CurrentDomain.AssemblyResolve += ResolveEventHandler;
	}

	public static AssemblyName GetAssemblyName(string assemblyFullName)
	{
		AssemblyName value = null;
		if (!string.IsNullOrEmpty(assemblyFullName))
		{
			lock (assemblyNameCacheLock)
			{
				if (!fullNameToAssemblyNameMap.TryGetValue(assemblyFullName, out value))
				{
					try
					{
						string text = GetAddInAssemblyLocationFromAssemblyName(assemblyFullName) ?? GetAddInAssemblyLocationFromAssemblyName(assemblyFullName, find32BitAssembly: true);
						if (text != null)
						{
							pathNameToAssemblyNameMap.TryGetValue(text, out value);
						}
					}
					catch (FileNotFoundException)
					{
					}
					catch (FileLoadException)
					{
					}
					catch (BadImageFormatException)
					{
					}
					catch (PathTooLongException)
					{
					}
					fullNameToAssemblyNameMap[assemblyFullName] = value;
				}
			}
		}
		return value;
	}

	public static string GetAssemblyFullNameString(string assemblyFullName)
	{
		string value = null;
		if (!string.IsNullOrEmpty(assemblyFullName))
		{
			lock (fullNameCacheLock)
			{
				if (!fullNameToEffectiveFullNameMap.TryGetValue(assemblyFullName, out value))
				{
					value = GetAssemblyName(assemblyFullName)?.FullName;
					fullNameToEffectiveFullNameMap[assemblyFullName] = value;
				}
			}
		}
		return value;
	}

	public static Type GetTypeFromAssembly(string assemblyFullName, string typeName)
	{
		Type value = null;
		lock (typeCacheLock)
		{
			string key = assemblyFullName + "@" + typeName;
			if (!typeNameToTypeMap.TryGetValue(key, out value))
			{
				try
				{
					value = GetAssembly(assemblyFullName)?.GetType(typeName);
					typeNameToTypeMap[key] = value;
				}
				catch (FileNotFoundException innerException)
				{
					throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, typeName, innerException);
				}
				catch (FileLoadException innerException2)
				{
					throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, typeName, innerException2);
				}
				catch (BadImageFormatException innerException3)
				{
					throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, typeName, innerException3);
				}
				catch (PathTooLongException innerException4)
				{
					throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, typeName, innerException4);
				}
			}
		}
		return value;
	}

	public static ParameterInfo[] GetParameterTypes(MethodInfo mi)
	{
		HookAssemblyResolverEvent();
		try
		{
			return mi.GetParameters();
		}
		catch (FileNotFoundException innerException)
		{
			throw NavNCLDotNetInvokeException.Create(CultureInfo.CurrentUICulture, mi.Name, innerException);
		}
		catch (FileLoadException innerException2)
		{
			throw NavNCLDotNetInvokeException.Create(CultureInfo.CurrentUICulture, mi.Name, innerException2);
		}
		catch (BadImageFormatException innerException3)
		{
			throw NavNCLDotNetInvokeException.Create(CultureInfo.CurrentUICulture, mi.Name, innerException3);
		}
		catch (PathTooLongException innerException4)
		{
			throw NavNCLDotNetInvokeException.Create(CultureInfo.CurrentUICulture, mi.Name, innerException4);
		}
	}

	public static string GetAddInDirectory()
	{
		if (string.IsNullOrEmpty(addInDirectory))
		{
			addInDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Add-Ins");
		}
		return addInDirectory;
	}

	public static string GetGlobalAssemblyCacheDirectory()
	{
		return Path.Combine(Environment.ExpandEnvironmentVariables("%windir%"), "Microsoft.NET\\assembly\\");
	}

	public static string GetTempAddInDirectory()
	{
		if (string.IsNullOrEmpty(tempAddInDirectory))
		{
			string path = VersionCompatibility.AssemblyFileVersion.ToString();
			tempAddInDirectory = Path.Combine(Path.GetTempPath(), "Microsoft Dynamics NAV", "Add-Ins", path);
		}
		return tempAddInDirectory;
	}

	public static string GetAddInAssemblyLocationFromAssemblyName(string searchAssemblyName, bool find32BitAssembly = false)
	{
		string assemblyLocationFromName = GetAssemblyLocationFromName(searchAssemblyName, find32BitAssembly, "*.dll", GetAddInDirectory(), GetTempAddInDirectory());
		if (string.IsNullOrWhiteSpace(assemblyLocationFromName))
		{
			assemblyLocationFromName = GetAssemblyLocationFromName(searchAssemblyName, find32BitAssembly, "*.exe", GetAddInDirectory(), GetTempAddInDirectory());
		}
		return assemblyLocationFromName;
	}

	public static string GetAssemblyLocationFromName(string searchAssemblyName, bool find32BitAssembly, string extension, params string[] addInPaths)
	{
		string result = null;
		int num = 0;
		int num2 = 0;
		try
		{
			List<string> list = new List<string>();
			foreach (string text in addInPaths)
			{
				if (Directory.Exists(text))
				{
					list.Add(text);
				}
			}
			List<string> list2 = new List<string>();
			foreach (string item in list)
			{
				list2.AddRange(Directory.GetFiles(item, extension, SearchOption.AllDirectories));
			}
			AssemblyName assemblyName = new AssemblyName(searchAssemblyName);
			if (assemblyName.Version != null)
			{
				num = assemblyName.Version.Build;
				num2 = assemblyName.Version.Revision;
			}
			foreach (string item2 in list2)
			{
				try
				{
					AssemblyName value = null;
					if (!pathNameToAssemblyNameMap.TryGetValue(item2, out value))
					{
						lock (assemblyMapLock)
						{
							if (!pathNameToAssemblyNameMap.TryGetValue(item2, out value))
							{
								try
								{
									value = AssemblyName.GetAssemblyName(item2);
									pathNameToAssemblyNameMap.Add(item2, value);
								}
								catch (BadImageFormatException)
								{
									pathNameToAssemblyNameMap.Add(item2, null);
									goto end_IL_00c5;
								}
							}
						}
					}
					if (value == null || !IsAssemblyCompatible(assemblyName, value))
					{
						continue;
					}
					if (IsVersionCompatible(assemblyName.Version, value.Version, specificVersion: true) && IsProcessorArchitectureCompatible(value.ProcessorArchitecture, find32BitAssembly))
					{
						return item2;
					}
					int build = value.Version.Build;
					if (build < num)
					{
						continue;
					}
					num = build;
					int revision = value.Version.Revision;
					if (revision >= num2)
					{
						num2 = revision;
						if (IsProcessorArchitectureCompatible(value.ProcessorArchitecture, find32BitAssembly))
						{
							result = item2;
						}
					}
					end_IL_00c5:;
				}
				catch (FileNotFoundException)
				{
				}
				catch (FileLoadException)
				{
				}
				catch (BadImageFormatException)
				{
				}
				catch (PathTooLongException)
				{
				}
			}
		}
		catch (DirectoryNotFoundException)
		{
		}
		return result;
	}

	public static Assembly LoadAssemblyFile(string assemblyLocation)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(assemblyLocation))
			{
				return null;
			}
			return Assembly.LoadFile(assemblyLocation);
		}
		catch (FileNotFoundException)
		{
		}
		catch (FileLoadException)
		{
		}
		catch (BadImageFormatException)
		{
		}
		catch (PathTooLongException)
		{
		}
		return null;
	}

	public static Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
	{
		if (args == null || string.IsNullOrEmpty(args.Name) || args.Name.Contains(".resources"))
		{
			return null;
		}
		object obj = LoadAssemblyFile(GetAddInAssemblyLocationFromAssemblyName(args.Name, !Environment.Is64BitProcess)) ?? navAddInResolver?.Invoke(args.Name);
		if (obj == null)
		{
			Func<string, Assembly> func = navAppsServerAddInAssemblyResolver;
			if (func == null)
			{
				return null;
			}
			obj = func(args.Name);
		}
		return (Assembly)obj;
	}

	private static bool IsAssemblyCompatible(AssemblyName searchName, AssemblyName assemblyName)
	{
		if (assemblyName.FullName.Equals(searchName.FullName))
		{
			return true;
		}
		if (!searchName.Name.Equals(assemblyName.Name))
		{
			return false;
		}
		byte[] publicKeyToken = searchName.GetPublicKeyToken();
		if (publicKeyToken != null && publicKeyToken.Length != 0 && !NsDataType.AreEqual(publicKeyToken, assemblyName.GetPublicKeyToken()))
		{
			return false;
		}
		if (!IsVersionCompatible(searchName.Version, assemblyName.Version, specificVersion: false))
		{
			return false;
		}
		string cultureName = searchName.CultureName;
		if (!string.IsNullOrWhiteSpace(cultureName) && !cultureName.Equals(assemblyName.CultureName))
		{
			return false;
		}
		return true;
	}

	private static bool IsVersionCompatible(Version searchVersion, Version assemblyVersion, bool specificVersion)
	{
		if (searchVersion != null)
		{
			if (!searchVersion.Major.Equals(assemblyVersion.Major) || !searchVersion.Minor.Equals(assemblyVersion.Minor))
			{
				return false;
			}
			if (specificVersion && (!searchVersion.Build.Equals(assemblyVersion.Build) || !searchVersion.Revision.Equals(assemblyVersion.Revision)))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsProcessorArchitectureCompatible(ProcessorArchitecture assemblyArchitecture, bool find32BitAssembly)
	{
		if (assemblyArchitecture == ProcessorArchitecture.None || assemblyArchitecture == ProcessorArchitecture.MSIL)
		{
			return true;
		}
		if (find32BitAssembly)
		{
			if (assemblyArchitecture == ProcessorArchitecture.X86)
			{
				return true;
			}
		}
		else if (assemblyArchitecture == ProcessorArchitecture.Amd64 || assemblyArchitecture == ProcessorArchitecture.IA64)
		{
			return true;
		}
		return false;
	}

	private static object Create(Type objectType, string assemblyFullName, string typeName, params object[] arguments)
	{
		object obj = null;
		try
		{
			if (objectType != null)
			{
				if (objectType.IsValueType && (arguments == null || arguments.Length == 0))
				{
					obj = Activator.CreateInstance(objectType);
				}
				else
				{
					if (arguments == null)
					{
						ConstructorInfo constructor = objectType.GetConstructor(Type.EmptyTypes);
						if (constructor != null)
						{
							obj = constructor.Invoke(null);
						}
					}
					else
					{
						Type[] array = new Type[arguments.Length];
						for (int i = 0; i < arguments.Length; i++)
						{
							array[i] = arguments[i].GetType();
						}
						ConstructorInfo constructor = objectType.GetConstructor(array);
						if (constructor != null)
						{
							obj = constructor.Invoke(arguments);
						}
					}
					if (obj == null)
					{
						throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, typeName, null);
					}
				}
			}
			return obj;
		}
		catch (ArgumentNullException innerException)
		{
			throw new NavNCLDotNetCreateException(assemblyFullName, innerException);
		}
		catch (ArgumentException innerException2)
		{
			throw new NavNCLDotNetCreateException(assemblyFullName, innerException2);
		}
		catch (COMException innerException3)
		{
			throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, assemblyFullName, typeName, innerException3);
		}
		catch (TargetInvocationException ex)
		{
			Exception baseException = ex.GetBaseException();
			throw NavNCLDotNetInvokeException.Create(CultureInfo.CurrentCulture, typeName, baseException);
		}
	}

	public static object GetSerializedObject(SerializableObjectContract serializedObject)
	{
		Type typeFromAssembly = GetTypeFromAssembly(serializedObject.AssemblyName, serializedObject.TypeName);
		if (typeFromAssembly != null)
		{
			try
			{
				MemoryStream serializationStream = new MemoryStream(serializedObject.ObjectData);
				return new BinaryFormatter
				{
					Binder = new NavSerializationBinder(typeFromAssembly)
				}.Deserialize(serializationStream);
			}
			catch (SerializationException innerException)
			{
				throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, serializedObject, innerException);
			}
			catch (SecurityException innerException2)
			{
				throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, serializedObject, innerException2);
			}
		}
		throw NavNCLDotNetCreateException.Create(CultureInfo.CurrentUICulture, serializedObject, null);
	}

	public static Type GetInterfaceType(string assemblyFullName, string typeName, Type otherType)
	{
		if (string.IsNullOrEmpty(assemblyFullName) || string.IsNullOrEmpty(typeName))
		{
			return null;
		}
		return ConvertGenericInterfaceType(GetTypeFromAssembly(assemblyFullName, typeName), otherType);
	}

	public static Type ConvertGenericInterfaceType(Type objectType, Type otherType)
	{
		if (objectType != null && objectType.IsInterface && objectType.IsGenericType)
		{
			Type[] genericArguments = objectType.GetGenericArguments();
			Type[] genericArguments2 = otherType.GetGenericArguments();
			Type[] array = new Type[genericArguments.Length];
			for (int i = 0; i < genericArguments.Length; i++)
			{
				Type type = genericArguments[i];
				if (type.IsGenericParameter)
				{
					array.SetValue((i < genericArguments2.Length) ? genericArguments2[i] : typeof(object), i);
				}
				else
				{
					array.SetValue(type, i);
				}
			}
			objectType = objectType.MakeGenericType(array);
		}
		return objectType;
	}

	public static object InvokeOnInterface(string methodName, BindingFlags invokeAttribute, Type type, object instance, object[] arguments, ParameterModifier[] parameterModifier, CultureInfo cultureInfo)
	{
		object result = null;
		string searchName = GetSearchName(invokeAttribute, methodName);
		List<MethodInfo> list = BuildMethodInfoList(type, searchName, arguments.Length);
		if (list != null && list.Count > 0)
		{
			bool flag = false;
			int num = 0;
			while (!flag && num < list.Count)
			{
				try
				{
					result = list[num].DeclaringType.InvokeMember(methodName, invokeAttribute | BindingFlags.NonPublic, Type.DefaultBinder, instance, arguments, parameterModifier, cultureInfo, null);
					flag = true;
				}
				catch (MissingMethodException)
				{
					if (num == list.Count - 1)
					{
						throw;
					}
					num++;
				}
				catch (TargetInvocationException ex2)
				{
					if (ex2.GetBaseException() is COMException { HResult: -2146822426 } && IsItemMethod(methodName, list[num], invokeAttribute))
					{
						invokeAttribute = (invokeAttribute ^ BindingFlags.GetProperty) | BindingFlags.InvokeMethod;
						continue;
					}
					throw;
				}
			}
		}
		return result;
	}

	private static bool IsItemMethod(string methodName, MethodInfo methodInfo, BindingFlags invokeAttribute)
	{
		if ((invokeAttribute & BindingFlags.GetProperty) == BindingFlags.GetProperty && methodInfo.MemberType == MemberTypes.Method)
		{
			return methodName.Equals("Item");
		}
		return false;
	}

	private static string GetSearchName(BindingFlags flags, string methodName)
	{
		if ((flags & BindingFlags.GetProperty) == BindingFlags.GetProperty)
		{
			return "get_" + methodName;
		}
		if ((flags & BindingFlags.SetProperty) == BindingFlags.SetProperty)
		{
			return "set_" + methodName;
		}
		return methodName;
	}

	private static List<MethodInfo> BuildMethodInfoList(Type type, string name, int noArgs)
	{
		List<MethodInfo> list = new List<MethodInfo>();
		AddMethodsWithMatchingSignature(list, type, name, noArgs);
		Type[] interfaces = type.GetInterfaces();
		foreach (Type inter in interfaces)
		{
			AddMethodsWithMatchingSignature(list, inter, name, noArgs);
		}
		RemoveHiddenMethods(list);
		return list;
	}

	private static void AddMethodsWithMatchingSignature(List<MethodInfo> methodList, Type inter, string name, int noArgs)
	{
		MethodInfo[] methods = inter.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod);
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.Name.Equals(name) && methodInfo.GetParameters().Length == noArgs)
			{
				methodList.Add(methodInfo);
			}
		}
	}

	private static List<MethodInfo> RemoveHiddenMethods(List<MethodInfo> methodInfos)
	{
		for (int i = 0; i < methodInfos.Count; i++)
		{
			MethodInfo methodInfo = methodInfos[i];
			if (methodInfo == null)
			{
				continue;
			}
			for (int j = 0; j < methodInfos.Count; j++)
			{
				MethodInfo methodInfo2 = methodInfos[j];
				if (i == j || methodInfo2 == null)
				{
					continue;
				}
				ParameterInfo[] parameters = methodInfo.GetParameters();
				ParameterInfo[] parameters2 = methodInfo2.GetParameters();
				bool flag = true;
				for (int k = 0; k < parameters.Length; k++)
				{
					if (parameters[k].ParameterType != parameters2[k].ParameterType)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					if (IsBaseInterface(methodInfo.DeclaringType, methodInfo2.DeclaringType))
					{
						methodInfos[i] = null;
					}
					else
					{
						methodInfos[j] = null;
					}
				}
			}
		}
		int num = 0;
		while (num < methodInfos.Count)
		{
			if (methodInfos[num] == null)
			{
				methodInfos.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
		return methodInfos;
	}

	private static bool IsBaseInterface(Type t1, Type t2)
	{
		Type[] interfaces = t2.GetInterfaces();
		foreach (Type o in interfaces)
		{
			if (t1.Equals(o))
			{
				return true;
			}
		}
		return false;
	}

	public static object ConvertEnumToInteger(object value)
	{
		if (value is Enum)
		{
			FieldInfo[] fields = value.GetType().GetFields();
			if (fields.Length != 0)
			{
				Type fieldType = fields[0].FieldType;
				if (typeof(short).Equals(fieldType) || typeof(int).Equals(fieldType) || typeof(long).Equals(fieldType) || typeof(ushort).Equals(fieldType) || typeof(uint).Equals(fieldType) || typeof(ulong).Equals(fieldType) || typeof(sbyte).Equals(fieldType) || typeof(char).Equals(fieldType) || typeof(byte).Equals(fieldType))
				{
					try
					{
						return Convert.ChangeType(value, fieldType, CultureInfo.CurrentUICulture);
					}
					catch (InvalidCastException)
					{
						throw NavNCLConversionCastException.Create(CultureInfo.CurrentCulture, value.GetType(), value, fieldType);
					}
				}
			}
		}
		return value;
	}

	public static bool IsKnownNavType(Type type)
	{
		if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(SerializableObjectContract) || type == typeof(Guid) || type == typeof(CurrencyWrapper) || type == typeof(VariantWrapper) || type == typeof(DBNull))
		{
			return true;
		}
		return false;
	}

	public static bool IsNullableNavRuntimeType(Type type)
	{
		if (type.IsPrimitive || type == typeof(decimal) || type == typeof(Guid) || type == typeof(string) || type == typeof(DBNull) || type == typeof(Stream) || type == typeof(object))
		{
			return true;
		}
		return false;
	}

	public static int AddScriptableTag(int handle)
	{
		return handle | int.MinValue;
	}

	public static int RemoveScriptableTag(int handle)
	{
		return handle & 0x7FFFFFFF;
	}

	public static bool HasScriptableTag(int handle)
	{
		return (handle & int.MinValue) != 0;
	}

	public static bool IsJsonSerializable(object value)
	{
		return value is JsonSerializableObjectContract;
	}

	public static bool IsSerializable(object value, Type type)
	{
		if (value is ISerializable)
		{
			return true;
		}
		return type.IsSerializable;
	}

	public static void SerializeArguments(object[] args, bool eventHandlerArgument, bool useJsonSerialization)
	{
		if (args == null || args.Length == 0)
		{
			return;
		}
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == null)
			{
				continue;
			}
			Type type = args[i].GetType();
			if (!(type != typeof(NavAutomationHandle)))
			{
				continue;
			}
			if (IsKnownNavType(type))
			{
				if (eventHandlerArgument && type.IsPrimitive)
				{
					if (type == typeof(ushort) || type == typeof(short) || type == typeof(sbyte))
					{
						args[i] = Convert.ChangeType(args[i], typeof(int), CultureInfo.InvariantCulture);
					}
					else if (type == typeof(uint))
					{
						args[i] = Convert.ChangeType(args[i], typeof(long), CultureInfo.InvariantCulture);
					}
					else if (type == typeof(ulong) || type == typeof(float) || type == typeof(double))
					{
						args[i] = Convert.ChangeType(args[i], typeof(decimal), CultureInfo.InvariantCulture);
					}
				}
			}
			else
			{
				if (IsJsonSerializable(args[i]))
				{
					continue;
				}
				if (IsSerializable(args[i], type))
				{
					try
					{
						if (useJsonSerialization)
						{
							args[i] = new JsonSerializableObjectContract(args[i]);
						}
						else
						{
							args[i] = new SerializableObjectContract(args[i]);
						}
					}
					catch (SerializationException innerException)
					{
						throw NavNCLDotNetCreateException.CreateSerializedObject(CultureInfo.CurrentUICulture, type.Assembly.FullName, type.FullName, innerException);
					}
					catch (SecurityException innerException2)
					{
						throw NavNCLDotNetCreateException.CreateSerializedObject(CultureInfo.CurrentUICulture, type.Assembly.FullName, type.FullName, innerException2);
					}
					catch (JsonSerializationException innerException3)
					{
						throw NavNCLDotNetCreateException.CreateSerializedObject(CultureInfo.CurrentUICulture, type.Assembly.FullName, type.FullName, innerException3);
					}
				}
				else
				{
					if (!eventHandlerArgument)
					{
						throw NavNCLDotNetCreateException.CreateSerializedObject(CultureInfo.CurrentUICulture, type.Assembly.FullName, type.FullName, null);
					}
					args[i] = new object();
				}
			}
		}
	}

	public static Type[] GetArgumentTypes(ParameterModifier modifier, Type[] referenceTypes, params object[] arguments)
	{
		Type[] array = new Type[arguments.Length];
		for (int i = 0; i < arguments.Length; i++)
		{
			if (arguments[i] != null)
			{
				array[i] = arguments[i].GetType();
			}
			else if (referenceTypes != null && referenceTypes[i] != null)
			{
				array[i] = referenceTypes[i];
			}
			else
			{
				array[i] = typeof(object);
			}
			if (modifier[i])
			{
				array[i] = array[i].MakeByRefType();
			}
		}
		return array;
	}

	public static MethodInfo GetMethodInfo(string methodName, Type objectType, BindingFlags binding, ParameterModifier[] modifier, object[] arguments, Type[] referenceTypes)
	{
		Type[] types = GetArgumentTypes(modifier[0], referenceTypes, arguments);
		if ((binding & BindingFlags.GetProperty) == BindingFlags.GetProperty)
		{
			methodName = "get_" + methodName;
		}
		else if ((binding & BindingFlags.SetProperty) == BindingFlags.SetProperty)
		{
			methodName = "set_" + methodName;
		}
		MethodInfo methodInfo;
		try
		{
			methodInfo = objectType.GetMethod(methodName, binding | BindingFlags.NonPublic, Type.DefaultBinder, types, modifier);
		}
		catch (AmbiguousMatchException)
		{
			MethodInfo[] array = (from m in objectType.GetMethods(binding | BindingFlags.NonPublic)
				where m.Name == methodName && !m.IsGenericMethod
				select m).ToArray();
			if (array.Length != 1)
			{
				throw;
			}
			methodInfo = array[0];
		}
		if (methodInfo == null && types.Any((Type t) => t.IsCOMObject))
		{
			methodInfo = objectType.GetMethods(binding).FirstOrDefault((MethodInfo m) => m.Name == methodName && !m.IsGenericMethod && m.GetParameters().Length == types.Length);
		}
		if (methodInfo == null)
		{
			EmitGetMethodInfoFailureDetailsToTelemetry(methodName, objectType, binding, arguments, referenceTypes, types);
			throw NavNCLDotNetInvokeException.Create(CultureInfo.CurrentCulture, methodName, objectType, Lang.DotNetInvokeArgumentTypeError);
		}
		return methodInfo;
	}

	private static void EmitGetMethodInfoFailureDetailsToTelemetry(string methodName, Type objectType, BindingFlags binding, IReadOnlyList<object> arguments, IReadOnlyList<Type> referenceTypes, IReadOnlyList<Type> types)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("  ObjectType: " + objectType?.AssemblyQualifiedName);
		stringBuilder.AppendLine("  MethodName: " + methodName);
		stringBuilder.AppendLine("  BindingFlags: " + binding);
		for (int i = 0; i < arguments?.Count; i++)
		{
			string value = arguments[i]?.GetType().AssemblyQualifiedName ?? "<NULL>";
			stringBuilder.Append("  Arguments[").Append(i).Append("]: ")
				.AppendLine(value);
		}
		for (int j = 0; j < referenceTypes?.Count; j++)
		{
			string value2 = referenceTypes[j]?.AssemblyQualifiedName ?? "<NULL>";
			stringBuilder.Append("  ReferenceTypes[").Append(j).Append("]: ")
				.AppendLine(value2);
		}
		for (int k = 0; k < types?.Count; k++)
		{
			string value3 = types[k]?.AssemblyQualifiedName ?? "<NULL>";
			stringBuilder.Append("  Types[").Append(k).Append("]: ")
				.AppendLine(value3);
		}
		NavDiagnostics.GetMostSpecificInstance.SendTraceTag("000010P", Category.Runtime, Verbosity.Error, PrivacyClassification.SystemMetadata, stringBuilder.ToString());
	}

	public static void InvokeDispose(object instance)
	{
		Type type = instance.GetType();
		bool value = false;
		lock (typeIsDisposableLock)
		{
			if (!typeIsDisposableMap.TryGetValue(type, out value))
			{
				value = !type.IsCOMObject && type.GetInterface(DisposableTypeName) != null;
				typeIsDisposableMap[type] = value;
			}
		}
		if (!value)
		{
			return;
		}
		try
		{
			if (type.GetMethod("Dispose", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, Type.DefaultBinder, Type.EmptyTypes, null) != null)
			{
				type.InvokeMember("Dispose", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, Type.DefaultBinder, instance, null, CultureInfo.InvariantCulture);
				return;
			}
			object obj = instance as IDisposable;
			if (obj != null)
			{
				DisposableType.InvokeMember("Dispose", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, Type.DefaultBinder, obj, null, CultureInfo.InvariantCulture);
			}
		}
		catch (TargetInvocationException ex)
		{
			Exception baseException = ex.GetBaseException();
			throw NavNCLDotNetInvokeException.Create(CultureInfo.CurrentCulture, "Dispose", type, baseException);
		}
		catch (Exception ex2) when (!(ex2 is NavBaseException))
		{
			throw NavNCLDotNetInvokeException.Create(CultureInfo.CurrentCulture, "Dispose", type, ex2);
		}
	}

	public static string TryCreateArchiveFromAddIns(string assemblyFullName, out string clientSideName, bool find32BitAssembly = false)
	{
		clientSideName = null;
		try
		{
			string addInAssemblyLocationFromAssemblyName = GetAddInAssemblyLocationFromAssemblyName(assemblyFullName, find32BitAssembly);
			if (addInAssemblyLocationFromAssemblyName == null)
			{
				assemblyFullName = MapOldAssemblyFullName(assemblyFullName);
				if (assemblyFullName == null)
				{
					return null;
				}
				return TryCreateArchiveFromAddIns(assemblyFullName, out clientSideName, find32BitAssembly);
			}
			Version version = AssemblyName.GetAssemblyName(addInAssemblyLocationFromAssemblyName).Version;
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(addInAssemblyLocationFromAssemblyName));
			clientSideName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", directoryInfo.Name, version);
			string text = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			new ZipFileWrapper().CreateFromDirectory(directoryInfo.FullName, text, CompressionLevel.Optimal, includeBaseDirectory: false);
			return text;
		}
		catch (NavBaseException)
		{
			return null;
		}
	}

	public static string MapOldAssemblyFullName(string oldAssemblyName)
	{
		string text = "Interactive Timeline Visualization Add-in";
		if (oldAssemblyName.StartsWith(text, StringComparison.OrdinalIgnoreCase))
		{
			return "Microsoft.Dynamics.Nav.Client.TimelineVisualization" + oldAssemblyName.Substring(text.Length);
		}
		text = "Microsoft.Dynamics.NAV.CodeViewerControl";
		if (oldAssemblyName.StartsWith(text, StringComparison.OrdinalIgnoreCase))
		{
			return "Microsoft.Dynamics.Nav.Client.CodeViewer" + oldAssemblyName.Substring(text.Length);
		}
		text = "Microsoft.Dynamics.NAV.MicrosoftDynamicsOnlineConnectControl";
		if (oldAssemblyName.StartsWith(text, StringComparison.OrdinalIgnoreCase))
		{
			return "Microsoft.Dynamics.Nav.Client.DynamicsOnlineConnect" + oldAssemblyName.Substring(text.Length);
		}
		return null;
	}

	public static ReadOnlyCollection<string> GetAddInsAssemblyNames()
	{
		GetAddInAssemblyLocationFromAssemblyName(Guid.NewGuid().ToString());
		List<string> list = new List<string>();
		lock (assemblyMapLock)
		{
			foreach (KeyValuePair<string, AssemblyName> item in pathNameToAssemblyNameMap)
			{
				if (File.Exists(item.Key) && !IsNavAppsAddInDirectory(Path.GetDirectoryName(item.Key).Substring(item.Key.LastIndexOf(Path.DirectorySeparatorChar))) && item.Value != null)
				{
					string cSideCompliantAssemblyFullName = GetCSideCompliantAssemblyFullName(item.Value);
					if (!list.Contains(cSideCompliantAssemblyFullName))
					{
						list.Add(cSideCompliantAssemblyFullName);
					}
				}
			}
		}
		return list.AsReadOnly();
	}

	private static bool IsNavAppsAddInDirectory(string directoryName)
	{
		if (!directoryName.StartsWith("AppsAddIns_Client_", StringComparison.OrdinalIgnoreCase))
		{
			return directoryName.StartsWith("AppsAddIns_IDE_", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public static string GetCSideCompliantAssemblyFullName(AssemblyName assemblyName)
	{
		if (assemblyName == null)
		{
			throw new ArgumentNullException("assemblyName");
		}
		return assemblyName.FullName + ", Processor Architecture=" + assemblyName.ProcessorArchitecture;
	}

	public static bool IsNullableType(Type nullableType)
	{
		if (nullableType == null)
		{
			return false;
		}
		if (nullableType.IsGenericType && !nullableType.IsGenericTypeDefinition && (object)nullableType.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			return true;
		}
		return false;
	}
}
