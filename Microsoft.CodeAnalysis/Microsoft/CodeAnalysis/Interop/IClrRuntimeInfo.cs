using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Microsoft.CodeAnalysis.Interop;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("BD39D1D2-BA2F-486A-89B0-B4B0CB466891")]
[SuppressUnmanagedCodeSecurity]
internal interface IClrRuntimeInfo
{
	[PreserveSig]
	int GetVersionString([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, [In][Out][MarshalAs(UnmanagedType.U4)] ref int bufferLength);

	[PreserveSig]
	int GetRuntimeDirectory([Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, [In][Out][MarshalAs(UnmanagedType.U4)] ref int bufferLength);

	[return: MarshalAs(UnmanagedType.Bool)]
	bool IsLoaded([In] IntPtr processHandle);

	[PreserveSig]
	int LoadErrorString([In][MarshalAs(UnmanagedType.U4)] int resourceId, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, [In][Out][MarshalAs(UnmanagedType.U4)] ref int bufferLength);

	IntPtr LoadLibrary([In][MarshalAs(UnmanagedType.LPWStr)] string dllName);

	IntPtr GetProcAddress([In][MarshalAs(UnmanagedType.LPStr)] string procName);

	[return: MarshalAs(UnmanagedType.Interface)]
	object GetInterface([In][MarshalAs(UnmanagedType.LPStruct)] Guid coClassId, [In][MarshalAs(UnmanagedType.LPStruct)] Guid interfaceId);

	[return: MarshalAs(UnmanagedType.Bool)]
	bool IsLoadable();

	void SetDefaultStartupFlags([In][MarshalAs(UnmanagedType.U4)] int startupFlags, [In][MarshalAs(UnmanagedType.LPStr)] string hostConfigFile);

	[PreserveSig]
	int GetDefaultStartupFlags([MarshalAs(UnmanagedType.U4)] out int startupFlags, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder hostConfigFile, [In][Out][MarshalAs(UnmanagedType.U4)] ref int hostConfigFileLength);

	void BindAsLegacyV2Runtime();

	void IsStarted([MarshalAs(UnmanagedType.Bool)] out bool started, [MarshalAs(UnmanagedType.U4)] out int startupFlags);
}
