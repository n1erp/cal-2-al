using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dynamics.Nav.Runtime;

internal struct WINTRUST_FILE_INFO : IDisposable
{
	public uint CbStruct;

	[MarshalAs(UnmanagedType.LPTStr)]
	public string PcwszFilePath;

	public IntPtr FileHandle;

	public IntPtr KnownSubject;

	public WINTRUST_FILE_INFO(string fileName, Guid subject)
	{
		CbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_FILE_INFO));
		PcwszFilePath = fileName;
		if (subject != Guid.Empty)
		{
			KnownSubject = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Guid)));
			Marshal.StructureToPtr(subject, KnownSubject, fDeleteOld: false);
		}
		else
		{
			KnownSubject = IntPtr.Zero;
		}
		FileHandle = IntPtr.Zero;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	private void Dispose(bool disposing)
	{
		if (KnownSubject != IntPtr.Zero)
		{
			Marshal.DestroyStructure(KnownSubject, typeof(Guid));
			Marshal.FreeHGlobal(KnownSubject);
		}
	}
}
