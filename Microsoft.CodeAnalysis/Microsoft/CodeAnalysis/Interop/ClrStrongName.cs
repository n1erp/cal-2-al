using System;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis.Interop;

internal static class ClrStrongName
{
	[DllImport("mscoree.dll", EntryPoint = "CLRCreateInstance", PreserveSig = false)]
	[return: MarshalAs(UnmanagedType.Interface)]
	private static extern object nCreateInterface([MarshalAs(UnmanagedType.LPStruct)] Guid clsid, [MarshalAs(UnmanagedType.LPStruct)] Guid riid);

	internal static IClrStrongName GetInstance()
	{
		Guid clsid = new Guid(-1837098867, 3726, 18535, 179, 12, 127, 168, 56, 132, 232, 222);
		Guid riid = new Guid(-751641698, -17997, 16677, 130, 7, 161, 72, 132, 245, 50, 22);
		Guid coClassId = new Guid(-1214575923, -2611, 16539, 181, 165, 161, 98, 68, 97, 11, 146);
		Guid interfaceId = new Guid(-1120284206, -17873, 18538, 137, 176, 180, 176, 203, 70, 104, 145);
		Guid interfaceId2 = new Guid(-1613153073, 12928, 17297, 179, 169, 150, 225, 205, 231, 124, 141);
		return (IClrStrongName)((IClrRuntimeInfo)((IClrMetaHost)nCreateInterface(clsid, riid)).GetRuntime(GetRuntimeVersion(), interfaceId)).GetInterface(coClassId, interfaceId2);
	}

	internal static string GetRuntimeVersion()
	{
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("COMPLUS_InstallRoot")))
		{
			string environmentVariable = Environment.GetEnvironmentVariable("COMPLUS_Version");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				return environmentVariable;
			}
		}
		return "v4.0.30319";
	}
}
