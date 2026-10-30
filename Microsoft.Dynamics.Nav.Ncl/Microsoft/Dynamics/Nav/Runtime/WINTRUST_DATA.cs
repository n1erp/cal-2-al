using System;
using System.Runtime.InteropServices;

namespace Microsoft.Dynamics.Nav.Runtime;

internal struct WINTRUST_DATA : IDisposable
{
	public uint CbStruct = (uint)Marshal.SizeOf(typeof(WINTRUST_DATA));

	public IntPtr PolicyCallbackData;

	public IntPtr SIPCallbackData;

	public UiChoiceEnum UiChoice;

	public RevocationCheckFlags RevocationChecks;

	public UnionChoiceEnum UnionChoice;

	public IntPtr InfoStruct = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINTRUST_FILE_INFO)));

	public StateActionEnum StateAction;

	public IntPtr StateDataHandle;

	private IntPtr urlReference;

	public TrustProviderFlags ProvFlags;

	public UIContextEnum UIContext;

	public WINTRUST_DATA(WINTRUST_FILE_INFO fileInfo)
	{
		Marshal.StructureToPtr(fileInfo, InfoStruct, fDeleteOld: false);
		UnionChoice = UnionChoiceEnum.File;
		PolicyCallbackData = IntPtr.Zero;
		SIPCallbackData = IntPtr.Zero;
		UiChoice = UiChoiceEnum.Wtd_Ui_NoUI;
		RevocationChecks = RevocationCheckFlags.None;
		StateAction = StateActionEnum.Ignore;
		StateDataHandle = IntPtr.Zero;
		urlReference = IntPtr.Zero;
		ProvFlags = TrustProviderFlags.Safer;
		UIContext = UIContextEnum.Execute;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	private void Dispose(bool disposing)
	{
		if (UnionChoice == UnionChoiceEnum.File)
		{
			WINTRUST_FILE_INFO structure = default;
			Marshal.PtrToStructure(InfoStruct, structure);
			structure.Dispose();
		}
		Marshal.DestroyStructure(InfoStruct, typeof(WINTRUST_FILE_INFO));
		Marshal.FreeHGlobal(InfoStruct);
	}
}
