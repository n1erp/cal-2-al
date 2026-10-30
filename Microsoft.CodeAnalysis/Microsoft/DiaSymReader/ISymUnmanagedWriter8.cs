using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.DiaSymReader;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("5ba52f3b-6bf8-40fc-b476-d39c529b331e")]
[SuppressUnmanagedCodeSecurity]
internal interface ISymUnmanagedWriter8 : ISymUnmanagedWriter5
{
	void _VtblGap1_33();

	void InitializeDeterministic([MarshalAs(UnmanagedType.IUnknown)] object emitter, [MarshalAs(UnmanagedType.IUnknown)] object stream);

	unsafe void UpdateSignatureByHashingContent([In] byte* buffer, int size);

	void UpdateSignature(Guid pdbId, uint stamp, int age);

	unsafe void SetSourceServerData([In] byte* data, int size);

	unsafe void SetSourceLinkData([In] byte* data, int size);
}
