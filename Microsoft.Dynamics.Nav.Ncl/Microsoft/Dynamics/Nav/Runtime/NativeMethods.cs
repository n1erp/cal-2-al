using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NativeMethods
{
	[Flags]
	internal enum DHCPCAPI_REQUEST : uint
	{
		DHCPCAPI_REQUEST_PERSISTENT = 1u,
		DHCPCAPI_REQUEST_SYNCHRONOUS = 2u,
		DHCPCAPI_REQUEST_ASYNCHRONOUS = 4u,
		DHCPCAPI_REQUEST_CANCEL = 8u,
		DHCPCAPI_REQUEST_MASK = DHCPCAPI_REQUEST_PERSISTENT | DHCPCAPI_REQUEST_SYNCHRONOUS | DHCPCAPI_REQUEST_ASYNCHRONOUS | DHCPCAPI_REQUEST_CANCEL
	}

	internal struct DHCPCAPI_PARAMS_ARRAY
	{
		public uint nParams;

		public IntPtr Params;
	}

	internal struct DHCPCAPI_PARAMS
	{
		public uint Flags;

		public uint OptionId;

		[MarshalAs(UnmanagedType.Bool)]
		public bool IsVendor;

		public IntPtr Data;

		public uint nBytesData;
	}

	public const uint NO_ERROR = 0u;

	public const uint ERROR_INVALID_DOMAINNAME = 1212u;

	public const uint ERROR_NO_SUCH_DOMAIN = 1355u;

	public const uint ERROR_INVALID_PARAMETER = 87u;

	public const uint ERROR_DS_NAME_ERROR_NO_SYNTACTICAL_MAPPING = 8474u;

	public const uint ERROR_DS_INSUFF_ACCESS_RIGHTS = 8344u;

	internal const uint ERROR_MORE_DATA = 234u;

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool FreeLibrary(IntPtr lib);

	[DllImport("kernel32.dll")]
	internal static extern IntPtr GetModuleHandle(string moduleName);

	[DllImport("kernel32.dll")]
	internal static extern ulong GetTickCount64();

	[DllImport("user32", EntryPoint = "OemToCharBuffA")]
	internal static extern int OemToCharBuff(byte[] source, byte[] dest, int bytesize);

	[DllImport("user32", EntryPoint = "CharToOemBuffA")]
	internal static extern int CharToOemBuff(byte[] source, byte[] dest, int bytesize);

	[DllImport("Wintrust.dll")]
	internal static extern uint WinVerifyTrust(IntPtr hWnd, IntPtr pgActionID, IntPtr pWinTrustData);

	[DllImport("nclcsrts.dll", CharSet = CharSet.Unicode, EntryPoint = "NCL_SpnRegister")]
	internal static extern uint SpnRegister(string pzServiceAccount, string pzFqcnSpn, string pzNetbSpn, [MarshalAs(UnmanagedType.Bool)] bool nregister);

	[DllImport("dhcpcsvc.dll")]
	internal static extern uint DhcpCApiInitialize(out uint Version);

	[DllImport("dhcpcsvc.dll")]
	internal static extern void DhcpCApiCleanup();

	[DllImport("dhcpcsvc.dll", CharSet = CharSet.Unicode)]
	internal static extern uint DhcpRequestParams(DHCPCAPI_REQUEST Flags, IntPtr Reserved, string AdapterName, IntPtr ClassId, DHCPCAPI_PARAMS_ARRAY SendParams, DHCPCAPI_PARAMS_ARRAY RecdParams, IntPtr Buffer, ref uint pSize, string RequestIdStr);
}
